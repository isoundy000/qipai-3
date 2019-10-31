import { Application, rpcErr, RpcClass } from "mydog";
import { RoleInfoMgr } from "../../../app/svr_info/roleInfoMgr";
import { I_friendInfo, I_roleAllInfo, I_roleAllInfoClient } from "../../../app/util/someInterface";
import { I_playerGameState } from "../../../app/svr_gameMain/gameMainMgr";
import FriendRemote from "./friend";
import singleton_info from "../../../app/svr_info/singleton_info";
import { I_roleRankInfo } from "../../../app/svr_rank/rankService";
import { gameType } from "../../../config/gameConfig";

declare global {
    interface Rpc {
        info: {
            main: RpcClass<MainRemote>,
            friend: RpcClass<FriendRemote>
        }
    }
}

export default class MainRemote {
    private roleInfoMgr: RoleInfoMgr;
    constructor(app: Application) {
        this.roleInfoMgr = singleton_info.roleInfoMgr;
    }

    /**
     * 进入游戏
     */
    enterServer(uid: number, sid: string, token: number, cb: (err: rpcErr, info: { "code": number, "info": I_roleAllInfoClient }) => void) {
        this.roleInfoMgr.enterServer(uid, sid, token, cb);
    }

    /**
     * 重连
     */
    reconnectEntry(uid: number, sid: string, token: number, cb: (err: rpcErr, info: { "code": number, "info": I_roleAllInfoClient }) => void) {
        this.roleInfoMgr.reconnectEntry(uid, sid, token, cb);
    }

    /**
     * 掉线
     */
    offline(uid: number) {
        this.roleInfoMgr.getRole(uid).offline();
    }

    /**
     * 改变玩家游戏桌子信息
     */
    setGameState(uid: number, gameState: I_playerGameState) {
        let role = this.roleInfoMgr.getRole(uid);
        role.changeRoleInfo(gameState);
    }

    /**
     * 获取玩家排行榜信息
     * @param uid 
     * @param cb 
     */
    getRoleRankInfo(uid: number, cb: (err: rpcErr, info: I_roleRankInfo) => void) {
        let role = this.roleInfoMgr.getRole(uid);
        if (role) {
            cb(0, role.getRoleRankInfo());
            return;
        }
        singleton_info.mysql.query("select sex, nickname from player where uid = ? limit 1", [uid], (err, res) => {
            if (err || res.length === 0) {
                return cb(3, null as any);
            }
            cb(0, { "uid": uid, "score": 0, "name": res[0].nickname, "sex": res[0].sex });
        });
    }

    /**
     * 给玩家发送消息
     */
    sendMsgToOne(uid: number, route: string, msg: any) {
        let role = this.roleInfoMgr.getRole(uid);
        if (role) {
            role.getMsg(route, msg);
        }
    }

    /**
     * 消耗道具
     */
    costItems(uid: number, cost: { [id: string]: number }, syncDouziToTable: boolean) {
        let role = this.roleInfoMgr.getRole(uid);
        if (role) {
            role.costItems(cost, syncDouziToTable);
        }
    }

    /**
     * 获得道具
     */
    addItems(uid: number, add: { [id: string]: number }, syncDouziToTable: boolean) {
        let role = this.roleInfoMgr.getRole(uid);
        if (role) {
            role.addItems(add, syncDouziToTable);
        }
    }

    /**
     * 游戏结束，统计数据
     */
    gameOverData(data: I_gameOverScoreDouzi) {
        let role = this.roleInfoMgr.getRole(data.uid);
        if (role) {
            role.gameOverData(data);
        }
    }
}

export interface I_gameOverScoreDouzi {
    uid: number,
    gameType: gameType,
    isWin: boolean,
    scoreAll: number,
    addDouzi: number,
}