import { Application, rpcErr } from "mydog";
import { I_gameInfo, I_gameInfoClient } from "../svr_gameMain/gameMainMgr";
import singleton_rank from "./singleton_rank";
import { gameLog } from "../logger";
import { timeFormat } from "../util/util";
import { I_mailInfo } from "../util/someInterface";
import * as gameUtil from "../util/gameUtil";
import { mailStatus } from "../svr_info/mail";
import { ItemId } from "../../config/item";
import { mysqlTable } from "../../config/dbConfig";
import { gameState, gameType } from "../../config/gameConfig";
import { douziGetPercent } from "../../config/gameParamConfig";


/**
 * 排行服务
 */
export class RankService {
    private app: Application;
    private id: number;
    private gameInfo: I_gameInfoClient;
    private rankTimer: NodeJS.Timer = null as any;      // 排行计时器
    private douziTimer: NodeJS.Timer = null as any;     // 豆子入库计时器
    private ranklist: I_roleRankInfo[] = [];
    private oldRanklist: I_roleRankInfo[] = [];
    private isRanking: boolean = false;         // 是否正在排行（排行过程为异步的）
    private needSettlement: boolean = false;    // 排行完成后，是否需要结算发奖
    private isOver: boolean = false;            // 游戏结束，不再需要排行
    private getDouzi: number = 0;               // 举办者获得的门票和台费（结算时需要扣除抽成）
    private lastGetDouzi: number = 0;           // 上次的获得（用以比较，决定是否更新mysql）
    constructor(app: Application, gameInfo: I_gameInfoClient) {
        this.app = app;
        this.id = gameInfo.id;
        this.gameInfo = gameInfo;
        this.getDouzi = gameInfo.getDouzi;
        this.lastGetDouzi = this.getDouzi;
        if (gameInfo.state === gameState.end) {
            this.isOver = true;
        }
        this.rank();
        this.updateDouziToDb();
    }

    private getRandomTime() {    // 排行间隔1小时左右
        return 30 * 1000;
        let timeout = 55 + Math.random() * 10;
        return timeout * 60 * 1000;
    }

    /**
     * 排行
     */
    private rank() {
        let self = this;
        self.isRanking = true;
        singleton_rank.gameRedis.get(this.id).zrevrange(gameUtil.table_rank(this.id), 0, this.gameInfo.rankNum - 1, "withscores", function (err, res) {
            if (!self.isOver) {
                self.rankTimer = setTimeout(self.rank.bind(self), self.getRandomTime());
            }
            if (err) {
                gameLog.error(err as any);
                return;
            }
            self.oldRanklist = self.ranklist;
            self.ranklist = [];
            let index = 0;
            function pushToRank() {
                if (index >= res.length) {
                    self.oldRanklist = [];
                    self.isRanking = false;
                    if (self.needSettlement) {
                        self.sendAward();
                    }
                    return;
                }
                index += 2;
                self.pushPlayerToRank(Number(res[index - 2]), Number(res[index - 1]), pushToRank);
            }
            pushToRank();
        });
    }


    /**
     * 从旧的排行榜中查找玩家信息
     */
    private findPlayerFromOldRanklist(uid: number) {
        for (let one of this.oldRanklist) {
            if (one.uid === uid) {
                return one;
            }
        }
        return null;
    }

    /**
     * 玩家信息入榜
     */
    private pushPlayerToRank(uid: number, score: number, cb: () => void) {
        let one = this.findPlayerFromOldRanklist(uid);
        if (one) {
            one.score = score;
            this.ranklist.push(one);
            return cb();
        }
        this.app.rpc(gameUtil.getInfoId(uid)).info.main.getRoleRankInfo(uid, (err, res) => {
            if (err) {
                return cb();
            }
            res.score = score;
            this.ranklist.push(res);
            cb();
        });
    }


    /**
     * 结算
     */
    gameSettlement() {
        this.isOver = true;
        this.needSettlement = true;
        clearTimeout(this.rankTimer);
        clearTimeout(this.douziTimer);
        this.updateDouziToDb();
        if (!this.isRanking) {
            this.rank();
        }
    }

    /**
     * 关闭
     */
    close(isGm: boolean) {
        this.isOver = true;
        clearTimeout(this.rankTimer);
        clearTimeout(this.douziTimer);
        this.updateDouziToDb();
        if (!isGm) {    // 自然关闭，即比赛完毕，则删除数据库中玩家分数。
            singleton_rank.gameRedis.get(this.id).del(gameUtil.table_rank(this.id), (err, res) => {
                if (err) {
                    console.log(err);
                }
                console.log("删除排行数据：", gameUtil.table_rank(this.id));
            });
        }
    }

    /**
     * 发放奖励（通过邮件形式）
     */
    private sendAward() {
        let award = this.gameInfo.award;
        let rankStart = 1;
        let expireTime = new Date();
        expireTime.setDate(expireTime.getDate() + 7);
        for (let i = 0; i < award.length; i++) {
            if (award[i].num === 0) {
                rankStart = award[i].rank + 1;
                continue;
            }

            for (let rank = rankStart; rank <= award[i].rank; rank++) {
                let role = this.ranklist[rank - 1];
                if (!role) {
                    continue;
                }
                let mailData: I_mailInfo = {
                    "id": 0,
                    "uid": role.uid,
                    "topic": "比赛结算",
                    "content": `恭喜您在赛事《${this.gameInfo.gameName}》中，获得第${rank}名，奖励如下`,
                    "items": [{ "id": ItemId.douzi, "num": award[i].num }],
                    "status": mailStatus.not_read,
                    "createTime": timeFormat(new Date()),
                    "expireTime": timeFormat(expireTime),
                    "sendUid": this.gameInfo.roleId,
                    "sendName": this.gameInfo.roleName,
                };
                this.sendMail(mailData);
            }
            rankStart = award[i].rank + 1;
        }
        let mailData: I_mailInfo = {
            "id": 0,
            "uid": this.gameInfo.roleId,
            "topic": "比赛结束",
            "content": `感谢您在本平台举办的比赛，现比赛已结束，您获得的豆子如下。`,
            "items": [{ "id": ItemId.douzi, "num": Math.floor(this.getDouzi * (1 - douziGetPercent / 100)) }],
            "status": mailStatus.not_read,
            "createTime": timeFormat(new Date()),
            "expireTime": timeFormat(expireTime),
            "sendUid": 0,
            "sendName": "系统大大"
        };
        this.sendMail(mailData);
    }

    private sendMail(mailData: I_mailInfo) {
        gameUtil.sendMail(singleton_rank.mysql, mailData, (err, res) => {
            if (err) {
                gameLog.error("mail err", err);
                return;
            }
            mailData.id = res.insertId;
            this.app.rpc(gameUtil.getInfoId(mailData.uid)).info.friend.sendMailTell(mailData.uid, mailData);
        });
    }


    /**
     * 获取玩家分数
     */
    getPlayerScore(uid: number, cb: (err: rpcErr, score: number) => void) {
        singleton_rank.gameRedis.get(this.id).zscore(gameUtil.table_rank(this.id), uid.toString(), (err, score) => {
            if (err) {
                return cb(5, 0);
            }
            score = score || "0";
            cb(0, parseInt(score));
        });
    }

    /**
     * 设置玩家分数
     */
    setPlayerScore(uid: number, score: number) {
        if (!this.isOver) {
            singleton_rank.gameRedis.get(this.id).zadd(gameUtil.table_rank(this.id), score, uid);
        }
    }

    /**
     * 想要进入该游戏（如果未参加过，则需验证密码和扣除门票）
     * @param uid 
     * @param gold 
     * @param pwd 
     * @param cb 
     */
    wantPlayGame(uid: number, douzi: number, pwd: string, cb: (err: rpcErr, msg: { code: number, gameType?: gameType, score?: number, doorCost?: number }) => void) {
        singleton_rank.gameRedis.get(this.id).zscore(gameUtil.table_rank(this.id), uid.toString(), (err, score) => {
            if (err) {
                return cb(0, { "code": 102 });
            }
            let doorCost = 0;
            if (!score) {
                if (this.gameInfo.password !== "" && pwd !== this.gameInfo.password) {
                    return cb(0, { "code": 103 });
                }
                if (douzi < this.gameInfo.gameParam.doorCost + this.gameInfo.gameParam.tableCost + this.gameInfo.gameParam.baseCost) {
                    return cb(0, { "code": 104 });
                }
                singleton_rank.gameRedis.get(this.id).zadd(gameUtil.table_rank(this.id), 1200, uid);
                score = "1200";
                doorCost = this.gameInfo.gameParam.doorCost;
                this.getDouzi += doorCost;
            } else if (douzi < this.gameInfo.gameParam.tableCost + this.gameInfo.gameParam.baseCost) {
                return cb(0, { "code": 104 });
            }
            cb(0, { "code": 0, "gameType": this.gameInfo.gameType, "score": parseInt(score), "doorCost": doorCost });
        });
    }
    /**
     * 获取排行榜
     * @param uid 
     * @param cb 
     */
    getRankList(uid: number, cb: (err: rpcErr, code: number, msg: { "ranklist": I_roleRankInfo[] }) => void) {
        cb(0, 0, { "ranklist": this.ranklist });
    }

    /**
     * 刷新个人最新战绩
     * @param gameId 
     * @param uid 
     * @param cb 
     */
    refreshMyRank(uid: number, cb: (err: rpcErr, code: number, msg: { "meRank": number, "meScore": number }) => void) {
        singleton_rank.gameRedis.get(this.id).zscore(gameUtil.table_rank(this.id), uid.toString(), (err, score) => {
            if (err) {
                return cb(0, 1, null as any);
            }
            if (!score) {
                return cb(0, 0, { "meRank": 0, "meScore": 0 });
            }
            singleton_rank.gameRedis.get(this.id).zrevrank(gameUtil.table_rank(this.id), uid.toString(), (err, rank) => {
                if (err) {
                    return cb(0, 1, null as any);
                }
                cb(0, 0, { "meRank": Number(rank) + 1, "meScore": Number(score) });
            });
        });
    }

    /**
     * 获得台费
     */
    addTableCost(num: number) {
        this.getDouzi += num;
    }

    /**
     * 将最新的豆子同步到数据库
     */
    updateDouziToDb() {
        let tmpDouzi = this.getDouzi;
        // console.log("douzi update --", this.lastGetDouzi, this.getDouzi);
        if (this.lastGetDouzi !== this.getDouzi) {
            singleton_rank.mysql.query(`update ${mysqlTable.game} set getDouzi = ? where id = ?`, [this.getDouzi, this.id], (err) => {
                if (err) {
                    console.log(err);
                } else {
                    this.lastGetDouzi = tmpDouzi;
                }
            });
        }
        if (!this.isOver) {
            let time = 10 + Math.random() * 5
            this.douziTimer = setTimeout(this.updateDouziToDb.bind(this), time * 1000);
        }
    }

}

/**
 * 排行榜中的玩家信息
 */
export interface I_roleRankInfo {
    uid: number,
    name: string,
    sex: number,
    score: number,
}