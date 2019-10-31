import { Application, RpcClass, rpcErr } from "mydog";
import { I_gameInfo, I_playerGameState, I_gameInfoClient } from "../../../app/svr_gameMain/gameMainMgr";
import { gameLog } from "../../../app/logger";
import { MatchMgr } from "../../../app/svr_match/matchMgr";
import { I_matchPlayer } from "../../../app/svr_match/matchTable";
import { errCode } from "../../../config/errCode";
import singleton_match from "../../../app/svr_match/singleton_match";


declare global {
    interface Rpc {
        match: {
            main: RpcClass<mainRemote>
        }
    }
}

export default class mainRemote {
    private app: Application;
    private matchMgr: MatchMgr;
    constructor(app: Application) {
        this.app = app;
        this.matchMgr = singleton_match.matchMgr;
    }

    /**
     * 创建游戏匹配服务
     */
    createGameMatch(gameInfo: I_gameInfoClient) {
        gameLog.debug("创建匹配服务");
        this.matchMgr.createMatch(gameInfo);
    }

    /**
     * 关闭游戏匹配服务
     */
    closeGameMatch(id: number) {
        this.matchMgr.closeMatch(id);
    }



    /**
     * 创建等待桌子
     */
    createMatchTable(gameId: number, players: I_matchPlayer[]) {
        let service = this.matchMgr.getMatchService(gameId);
        if (!service) {
            return;
        }
        service.createMatchTable(players);
    }


    /**
     * 接受好友邀请进入桌子
     */
    enterFriendTable(gameId: number, tableId: number, data: I_matchPlayer, cb: (err: rpcErr, code: number) => void) {
        let table = this.matchMgr.getMatchTable({ "gameId": gameId, "tableId": tableId });
        if (!table) {
            return cb(rpcErr.ok, errCode.enterFriendTable.noTable);
        }
        let res = table.enterFriendTable(data);
        cb(0, res);
    }

    /**
     * 掉线
     */
    offline(gameId: number, tableId: number, uid: number) {
        let table = this.matchMgr.getMatchTable({ "gameId": gameId, "tableId": tableId });
        if (table) {
            table.offline(uid);
        }
    }

    /**
     * 同步豆子到桌子里
     */
    syncDouziToTable(gameId: number, tableId: number, uid: number, douzi: number) {
        let table = this.matchMgr.getMatchTable({ "gameId": gameId, "tableId": tableId });
        if (table) {
            table.syncDouziToTable(uid, douzi);
        }
    }
}