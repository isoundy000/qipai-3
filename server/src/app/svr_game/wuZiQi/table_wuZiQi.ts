import { Application } from "mydog";
import { errCode } from "../../../config/errCode";
import { I_tableBase } from "../gameMgr";
import { GameService } from "../gameService";
import { I_matchPlayer } from "../../svr_match/matchTable";
import { I_playerGameState, emptyGamestate } from "../../svr_gameMain/gameMainMgr";
import { cmd } from "../../../config/cmd";
import { I_uidsid, I_gameOverResult, I_gameOverUserInfo } from "../../util/someInterface";
import { changeRoleGameState, getInfoId, table_rank } from "../../util/gameUtil";
import { calculateScore } from "../../util/util";
import { I_gameOverScoreDouzi } from "../../../servers/info/remote/main";
import { I_gameParam } from "../../../config/gameConfig";


export class Table_WuZiQi implements I_tableBase {
    private app: Application;
    private gameService: GameService;
    private tableId: number = 0;
    private players: WuZiQiPlayer[] = [];
    private nowChairId = 0;
    private leftQiZi = 0;       // 剩余棋子数
    private lastI = -1;         // 上次棋子位置i
    private lastJ = -1;         // 上次棋子位置j
    private lastI2 = -1;        // 上上次棋子位置i
    private lastJ2 = -1;        // 上上次棋子位置j

    gameTime: number = 0;   // 局时
    stepTime: number = 0;   // 步时
    countTime: number = 0;  // 读秒
    private qiZi: number[][] = [];
    constructor(gameService: GameService, tableId: number) {
        this.app = gameService.app;
        this.gameService = gameService;
        this.tableId = tableId;

        this.gameTime = gameService.gameParam.gameTime * 1000;
        this.stepTime = gameService.gameParam.stepTime * 1000;
        this.countTime = gameService.gameParam.countTime * 1000;

        for (let i = 0; i < 19; i++) {
            this.qiZi[i] = [];
            for (let j = 0; j < 19; j++) {
                this.qiZi[i][j] = 0;
            }
        }
        this.leftQiZi = 19 * 19;
    }

    initPlayers(players: I_matchPlayer[]) {
        let chairId = 1;
        this.nowChairId = Math.random() > 0.5 ? 1 : 2;
        for (let one of players) {
            let tmp = new WuZiQiPlayer(one, chairId, this);
            chairId++;

            this.players.push(tmp);
            if (tmp.chairId === this.nowChairId) {
                tmp.qiZiType = QiziType.black;
                tmp.leftTime = this.stepTime;
            } else {
                tmp.qiZiType = QiziType.white;
            }
        }
        for (let one of this.players) {
            let info: I_playerGameState = {
                "gameId": this.gameService.gameId,
                "gameSvr": this.app.serverId,
                "tableId": this.tableId,
                "chairId": one.chairId
            }
            changeRoleGameState(one.info.uid, one.info.sid, info);
        }

        this.sendMsgToAll(cmd.onEnterTable, this.getEnterTableData());
    }

    /**
     * 客户端无法解析，转换数据结构
     */
    private getQiZiStruct() {
        let arr: any[] = [];
        for (let one of this.qiZi) {
            arr.push({ "qizi": one });
        }
        return arr;
    }

    leave(uid: number) {
        let player = this.getPlayer(uid);
        if (!player) {
            return;
        }
        player.info.sid = "";
    }


    /**
     * 发送消息
     */
    sendMsgToAll(route: string, msg: any) {
        let uidsid: I_uidsid[] = [];
        for (let one of this.players) {
            if (one.info.sid) {
                uidsid.push({ "uid": one.info.uid, "sid": one.info.sid });
            }
        }
        this.app.sendMsgByUidSid(route, msg, uidsid);
    }

    private getPlayer(uid: number) {
        let tmp: WuZiQiPlayer = null as any;
        for (let one of this.players) {
            if (one.info.uid === uid) {
                tmp = one;
                break;
            }
        }
        return tmp;
    }

    private getPlayerByChairId(chairId: number) {
        let tmp: WuZiQiPlayer = null as any;
        for (let one of this.players) {
            if (one.chairId === chairId) {
                tmp = one;
                break;
            }
        }
        return tmp;
    }

    enterTable(uid: number, sid: string): boolean {
        let player = this.getPlayer(uid);
        if (!player) {
            return false;
        }
        player.info.sid = sid;
        this.app.sendMsgByUidSid(cmd.onEnterTable, this.getEnterTableData(), [{ "uid": uid, "sid": sid }]);
        return true;
    }

    private getEnterTableData() {
        let tmpPlayers: any[] = [];
        for (let one of this.players) {
            tmpPlayers.push(one.toClientData());
        }
        let info = {
            "gameId": this.gameService.gameId,
            "gameSvr": this.app.serverId,
            "tableId": this.tableId,
            "canChatInRoom": this.gameService.gameParam.canRoomChat,
            "nowChairId": this.nowChairId,
            "qiZi": this.getQiZiStruct(),
            "gameTime": this.gameTime / 1000,
            "stepTime": this.stepTime / 1000,
            "countTime": this.countTime / 1000,
            "players": tmpPlayers,
            "lastI": this.lastI,
            "lastJ": this.lastJ,
        };
        var msg = {
            "gameId": this.gameService.gameId,
            "gameType": this.gameService.gameType,
            "data": JSON.stringify(info)
        }
        return msg;
    }

    chat(uid: number, msg: string) {
        if (!this.gameService.gameParam.canRoomChat) {
            return;
        }
        let player = this.getPlayer(uid);
        if (!player) {
            return;
        }
        let data = {
            "uid": uid,
            "msg": msg
        };
        this.sendMsgToAll(cmd.onChatInTable, data);
    }
    chatSeq(uid: number, index: number) {
        let player = this.getPlayer(uid);
        if (!player) {
            return;
        }
        let data = {
            "uid": uid,
            "index": index
        };
        this.sendMsgToAll(cmd.onChatSeqInTable, data);
    }


    update(dt: number) {
        let player = this.getPlayerByChairId(this.nowChairId);
        player.update(dt);
    }

    play(uid: number, msg: { "i": number, "j": number }) {
        let player = this.getPlayer(uid);
        if (player.chairId !== this.nowChairId) {
            return;
        }
        let i = msg.i;
        let j = msg.j;
        if (isNaN(i) || isNaN(j) || i < 0 || i >= 19 || j < 0 || j >= 19) {
            return;
        }
        if (this.qiZi[i][j]) {
            return;
        }

        this.leftQiZi--;
        this.lastI2 = this.lastI;
        this.lastJ2 = this.lastJ;
        this.lastI = i;
        this.lastJ = j;
        this.qiZi[i][j] = player.qiZiType;
        player.leftTime = 0;

        this.nowChairId = 3 - this.nowChairId;
        let otherP = this.getPlayerByChairId(this.nowChairId);
        otherP.leftTime = otherP.isStepTime ? this.stepTime : this.countTime;


        let isOver = this.checkIfOver(i, j, player.qiZiType);
        let winUid = 0;
        if (isOver) {
            winUid = uid;
        } else if (this.leftQiZi === 0) {
            isOver = true;
        }
        let data = { "i": i, "j": j, "isOver": isOver };
        this.sendMsgToAll(cmd.onPlayCard, data);

        if (!isOver) {
            return;
        }

        this.gameOver(winUid);
    }


    private checkIfOver(m: number, n: number, type: number): boolean {
        let qiZi = this.qiZi;
        let i, j, num;
        // 横
        num = 0;
        for (i = m, j = n; j >= 0; j--) {
            if (qiZi[i][j] !== type) {
                break;
            }
        }
        for (++j; j < 19; j++) {
            if (qiZi[i][j] === type) {
                num++;
                if (num >= 5) {   // 结束
                    return true;
                }
            } else {
                break;
            }
        }

        // 竖
        num = 0;
        for (j = n, i = m; i >= 0; i--) {
            if (qiZi[i][j] !== type) {
                break;
            }
        }
        for (++i; i < 19; i++) {
            if (qiZi[i][j] === type) {
                num++;
                if (num >= 5) {   // 结束
                    return true;
                }
            } else {
                break;
            }
        }

        //撇
        num = 0;
        for (i = m, j = n; i < 19 && j >= 0; i++ , j--) {
            if (qiZi[i][j] !== type) {
                break;
            }
        }
        for (--i, ++j; i >= 0 && j < 19; i-- , j++) {
            if (qiZi[i][j] === type) {
                num++;
                if (num >= 5) {   // 结束
                    return true;
                }
            } else {
                break;
            }
        }

        //揦
        num = 0;
        for (i = m, j = n; i >= 0 && j >= 0; i-- , j--) {
            if (qiZi[i][j] !== type) {
                break;
            }
        }
        for (++i, ++j; i < 19 && j < 19; i++ , j++) {
            if (qiZi[i][j] === type) {
                num++;
                if (num >= 5) {   // 结束
                    return true;
                }
            } else {
                break;
            }
        }
        return false;
    }


    playerTimeout(chairId: number) {
        let winP = this.getPlayerByChairId(3 - chairId)
        this.gameOver(winP.info.uid);
    }

    gameOver(winUid: number) {
        let resData = this.settlement(winUid);
        this.sendMsgToAll(cmd.onGameOver, resData);
        this.gameService.closeTable(this.tableId);
        this.gameOverCreateMatchTable();
    }


    /**
     * 结算
     * @param winUid 0则平局
     */
    settlement(winUid: number): I_gameOverResult {
        let winP = this.getPlayerByChairId(3 - this.nowChairId);
        let loseP = this.getPlayerByChairId(this.nowChairId);
        let winUids: number[] = [];
        if (winUid !== 0) {
            winUids.push(winP.info.uid);
        }
        if (this.gameService.isClosed || winUid === 0) {
            let userList: I_gameOverUserInfo[] = [];
            for (let one of this.players) {
                let tmpInfo = one.info;
                userList.push({
                    "uid": tmpInfo.uid,
                    "name": tmpInfo.nickname,
                    "sex": tmpInfo.sex,
                    "score": tmpInfo.score,
                    "scoreAdd": 0,
                    "douzi": tmpInfo.douzi,
                    "douziAdd": 0,
                });
            }
            return { "isGameClosed": true, "winUids": winUids, "userList": userList };
        }

        let scoreEnd = calculateScore([{ "uid": winP.info.uid, "score": winP.info.score, "scoreA": winP.info.scoreA, "douzi": winP.info.douzi }],
            [{ "uid": loseP.info.uid, "score": loseP.info.score, "scoreA": loseP.info.scoreA, "douzi": loseP.info.douzi }],
            this.gameService.gameParam.baseCost || 0);

        let userList: I_gameOverUserInfo[] = [];
        for (let player of this.players) {
            let one = player.info;
            let scoreAdd = scoreEnd[one.uid].score - one.score;
            one.score = scoreEnd[one.uid].score;
            one.scoreA = scoreEnd[one.uid].scoreA;
            let addDouzi = scoreEnd[one.uid].douzi - one.douzi;
            one.douzi = scoreEnd[one.uid].douzi;
            let winOverData: I_gameOverScoreDouzi = {
                "uid": one.uid,
                "gameType": this.gameService.gameType,
                "isWin": player === winP,
                "scoreAll": one.scoreA,
                "addDouzi": addDouzi,
            };
            this.app.rpc(this.gameService.rankServer).rank.main.setPlayerScore(this.gameService.gameId, one.uid, one.score);
            this.app.rpc(getInfoId(one.uid)).info.main.gameOverData(winOverData);
            userList.push({
                "uid": one.uid,
                "name": one.nickname,
                "sex": one.sex,
                "score": one.score,
                "scoreAdd": scoreAdd,
                "douzi": one.douzi,
                "douziAdd": addDouzi,
            })
        }
        return { "isGameClosed": false, "winUids": winUids, "userList": userList };

    }

    gameOverCreateMatchTable() {
        let needDouzi = this.gameService.gameParam.tableCost + this.gameService.gameParam.baseCost;
        let tmpPlayers: { [tableId: number]: I_matchPlayer[] } = {};
        for (let one of this.players) {
            if (!this.gameService.isClosed && one.info.sid !== "" && one.info.douzi >= needDouzi) {
                if (!tmpPlayers[one.info.tableId]) {
                    tmpPlayers[one.info.tableId] = [];
                }
                one.info.isReady = false;
                tmpPlayers[one.info.tableId].push(one.info);
            } else {
                changeRoleGameState(one.info.uid, one.info.sid, emptyGamestate);
            }
        }
        console.log(tmpPlayers);
        for (let x in tmpPlayers) {
            let arr = tmpPlayers[x];
            let findMaster = false;
            for (let one of arr) {
                if (one.isMaster) {
                    one.isReady = true;
                    findMaster = true;
                    break;
                }
            }
            if (!findMaster) {
                arr[0].isMaster = true;
                arr[0].isReady = true;
            }
            this.app.rpc(this.gameService.matchServer).match.main.createMatchTable(this.gameService.gameId, arr);
        }
    }

    syncDouziToTable(uid: number, douzi: number) {
        let player = this.getPlayer(uid);
        player.info.douzi = douzi;
        this.sendMsgToAll(cmd.onTableDouziSync, { "chairId": player.chairId, "douzi": douzi });
    }

}

const enum QiziType {
    black = 1,  // 黑子
    white = 2,  // 白子
}

class WuZiQiPlayer {
    info: I_matchPlayer;

    private table: Table_WuZiQi;
    isStepTime: boolean = true;
    private gameTime: number = 0;
    leftTime: number = 0;

    chairId: number;
    qiZiType: number = QiziType.black;  // 1为黑子，2为白子
    constructor(info: I_matchPlayer, chairId: number, table: Table_WuZiQi) {
        this.info = info;
        this.chairId = chairId;
        this.table = table;
    }

    toClientData() {
        return {
            "uid": this.info.uid,
            "sid": this.info.sid,
            "nickname": this.info.nickname,
            "headId": this.info.headId,
            "chairId": this.chairId,
            "qiZiType": this.qiZiType,
            "douzi": this.info.douzi,
            "score": this.info.score,
            "gameTime": Math.floor(this.gameTime / 1000),
            "isStepTime": this.isStepTime,
            "leftTime": Math.floor(this.leftTime / 1000),
        };
    }

    update(dt: number) {
        if (this.isStepTime) {  // 局时倒计时
            this.gameTime += dt;
            this.leftTime -= dt;
            if (this.gameTime >= this.table.gameTime) {
                this.isStepTime = false;
                this.gameTime = this.table.gameTime;
                if (this.leftTime > this.table.countTime) {
                    this.leftTime = this.table.countTime;
                }
                this.table.sendMsgToAll(cmd.onStepTimeOver, { "uid": this.info.uid })
            }
        } else {    // 读秒倒计时
            this.leftTime -= dt;
        }

        if (this.leftTime <= 0) {
            this.table.playerTimeout(this.chairId);
        }
    }

}