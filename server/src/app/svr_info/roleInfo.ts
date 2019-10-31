import { Application, rpcErr } from "mydog";
import { I_roleInfo, I_bag, I_friendInfo, I_roleAllInfo, I_roleInfoMysql, I_roleAllInfoClient, I_gameData, DicObj } from "../util/someInterface";
import { Friend } from "./friend";
import { Mail, I_publicMailIds } from "./mail";
import * as util from "../util/util"
import singleton_info from "./singleton_info";
import { I_roleRankInfo } from "../svr_rank/rankService";
import { cmd } from "../../config/cmd";
import { gameType } from "../../config/gameConfig";
import { ItemId } from "../../config/item";
import { I_gameOverScoreDouzi } from "../../servers/info/remote/main";

// 数据库里的玩家数据（字段全部为真，以供下面使用）
let roleMysql: I_roleInfoMysql = {
    "uid": 1,
    "nickname": "1",
    "sex": 1,
    "headId": 1,
    "signature": "1",
    "loginTime": "1",
    "regTime": "1",
    "loginDays": 1,
    "ifGetAward": 1,
    "bag": {},
    "gameData": []
};

export class RoleInfo {
    public app: Application;
    public uidsid: { "uid": number, "sid": string };
    public role: I_roleInfo;
    public friend: Friend;
    public mail: Mail;
    private deleteTimer: NodeJS.Timer = null as any;
    constructor(app: Application, allInfo: I_roleAllInfo) {
        this.app = app;
        this.role = allInfo.role;
        this.uidsid = { "uid": allInfo.role.uid, "sid": allInfo.role.sid };
        this.friend = new Friend(this, allInfo.friends);
        this.mail = new Mail(this, allInfo.mails, allInfo.publicMailIds as I_publicMailIds);
    }

    entryServerLogic(sid: string, cb: (err: rpcErr, info: { "code": number, "info": I_roleAllInfoClient }) => void) {
        let diffDays = util.getDiffDays(new Date(this.role.loginTime), new Date());
        let changed: { "loginDays"?: number, "ifGetAward"?: number, "loginTime"?: string, "sid"?: string } = {};
        if (diffDays > 0) {
            changed.loginDays = diffDays > 1 ? 1 : (this.role.loginDays + 1);
            if (changed.loginDays > 7) {
                changed.loginDays = 1;
            }
            changed.ifGetAward = 0;
        }
        changed.loginTime = util.timeFormat(new Date());
        changed.sid = sid;
        this.uidsid.sid = sid;
        this.changeRoleInfo(changed);
        this.online();
        let bag = this.role.bag;
        delete this.role.bag;
        cb(0, { "code": 0, "info": { "role": this.role, "bag": this.changeBagStruct(bag), "friends": this.friend.getAllFriends(), "mails": this.mail.getMails() } });
        this.role.bag = bag;
    }

    private changeBagStruct(bag: DicObj<number>) {
        let bagArr: { "id": number, "num": number }[] = [];
        for (let id in bag) {
            bagArr.push({ "id": Number(id), "num": bag[id] });
        }
        return bagArr;
    }



    /**
     * 上线
     */
    online() {
        clearTimeout(this.deleteTimer);
        this.friend.changeInfo(this.uidsid);
    }

    /**
     * 掉线
     */
    offline() {
        clearTimeout(this.deleteTimer);
        this.deleteTimer = setTimeout(this.deleteSelf.bind(this), 12 * 3600 * 1000);

        this.uidsid.sid = "";
        this.changeRoleInfo({ "sid": "" });
        this.friend.changeInfo(this.uidsid);

        let tmpRole = this.role;
        if (tmpRole.gameId !== 0) {
            if (tmpRole.chairId === 0) {
                this.app.rpc(tmpRole.gameSvr).match.main.offline(tmpRole.gameId, tmpRole.tableId, this.uidsid.uid);
                this.changeRoleInfo({ "gameId": 0, "gameSvr": "", "tableId": 0 });
            } else {
                this.app.rpc(tmpRole.gameSvr).game.main.offline(tmpRole.gameId, tmpRole.tableId, this.uidsid.uid);
            }
        }
    }

    private deleteSelf() {
        singleton_info.roleInfoMgr.deleteRole(this.uidsid.uid);
    }

    /**
     * 玩家信息改变
     */
    changeRoleInfo(changed: { [K in keyof I_roleInfo]?: I_roleInfo[K] }) {
        let changedMysql = {} as any;
        for (let x in changed) {
            (this.role as any)[x] = (changed as any)[x];
            if ((roleMysql as any)[x]) {
                changedMysql[x] = (changed as any)[x];
            }
        }
        if (Object.keys(changedMysql).length === 0) {
            return;
        }
        singleton_info.mysql.update("player", changedMysql, { "uid": this.uidsid.uid }, function (err, res) {
            if (err) {
                console.log("changeRoleInfo", err);
            }
        });
    }

    /**
     * 获取某道具数量
     * @param id 
     */
    getItemNum(id: number) {
        return this.role.bag[id] || 0;
    }

    /**
     * 获得道具
     * @param items 
     */
    addItems(items: DicObj<number>, syncDouziToTable: boolean) {
        let tellItems: { "id": number, "num": number }[] = [];
        for (let id in items) {
            this.role.bag[id] = (this.role.bag[id] || 0) + items[id];
            tellItems.push({ "id": Number(id), "num": this.role.bag[id] });
        }
        this.getMsg(cmd.onBagChanged, { "bag": tellItems });
        if (syncDouziToTable && items[ItemId.douzi] && this.role.gameId !== 0) {
            this.syncDouziToTableFunc();
        }
        singleton_info.mysql.update("player", { "bag": JSON.stringify(this.role.bag) }, { "uid": this.uidsid.uid }, function (err, res) {
            if (err) {
                console.log("addItems", err);
            }
        });
    }

    /**
     * 消耗道具
     * @param items 道具
     * @param syncDouziToTable 是否将金币同步到桌子里
     */
    costItems(items: DicObj<number>, syncDouziToTable: boolean): boolean {
        if (!this.hasItems(items)) {
            return false;
        }
        let tellItems: { "id": number, "num": number }[] = [];
        for (let id in items) {
            this.role.bag[id] = (this.role.bag[id] || 0) - items[id];
            tellItems.push({ "id": Number(id), "num": this.role.bag[id] });
        }

        this.getMsg(cmd.onBagChanged, { "bag": tellItems });
        if (syncDouziToTable && items[ItemId.douzi] && this.role.gameId !== 0) {
            this.syncDouziToTableFunc();
        }
        singleton_info.mysql.update("player", { "bag": JSON.stringify(this.role.bag) }, { "uid": this.uidsid.uid }, function (err, res) {
            if (err) {
                console.log("costItems", err);
            }
        });
        return true;
    }


    /**
     * 同步豆子到桌子里
     */
    private syncDouziToTableFunc() {
        let tmpRole = this.role;
        if (tmpRole.chairId === 0) {
            this.app.rpc(tmpRole.gameSvr).match.main.syncDouziToTable(tmpRole.gameId, tmpRole.tableId, tmpRole.uid, tmpRole.bag[ItemId.douzi]);
        } else {
            this.app.rpc(tmpRole.gameSvr).game.main.syncDouziToTable(tmpRole.gameId, tmpRole.tableId, tmpRole.uid, tmpRole.bag[ItemId.douzi]);
        }
    }

    /**
     * 道具是否足够
     */
    hasItems(items: DicObj<number>): boolean {
        for (let id in items) {
            let num = this.role.bag[id] || 0;
            if (num < items[id]) {
                return false;
            }
        }
        return true;
    }

    getRoleRankInfo(): I_roleRankInfo {
        return {
            "uid": this.uidsid.uid,
            "name": this.role.nickname,
            "score": 0,
            "sex": this.role.sex,
        }
    }

    /**
     * 收到消息
     */
    getMsg(route: string, msg: any) {
        if (this.uidsid.sid) {
            this.app.sendMsgByUidSid(route, msg, [this.uidsid]);
        }
    }

    /**
     * 获取某一游戏类型的累计分数
     * @param gameType 
     */
    getGameTypeScore(gameType: gameType) {
        for (let one of this.role.gameData) {
            if (one.type === gameType) {
                return one.score;
            }
        }
        return 1200;
    }

    /**
     * 游戏结束，统计数据
     */
    gameOverData(data: I_gameOverScoreDouzi) {
        let oneData: I_gameData = null as any;
        for (let one of this.role.gameData) {
            if (one.type === data.gameType) {
                oneData = one;
                break;
            }
        }
        if (!oneData) {
            oneData = { "type": data.gameType, "all": 0, "win": 0, "score": 0 };
            this.role.gameData.push(oneData);
        }
        oneData.score = data.scoreAll
        oneData.all++;
        if (data.isWin) {
            oneData.win++;
        }
        singleton_info.mysql.update("player", { "gameData": JSON.stringify(this.role.gameData) }, { "uid": this.uidsid.uid }, function (err, res) {
            if (err) {
                console.log("gameOverData", err);
            }
        });
        this.getMsg(cmd.onGameWinData, oneData);

        let douziChange: DicObj<number> = {};
        if (data.addDouzi > 0) {
            douziChange[ItemId.douzi] = data.addDouzi;
            this.addItems(douziChange, false);
        } else {
            douziChange[ItemId.douzi] = -data.addDouzi;
            this.costItems(douziChange, false);
        }
    }
}