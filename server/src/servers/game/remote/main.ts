import { Application, RpcClass, rpcErr } from "mydog";
import { I_matchPlayer } from "../../../app/svr_match/matchTable";
import { gameType, I_gameParam } from "../../../config/gameConfig";
import { GameMgr, I_tableBase } from "../../../app/svr_game/gameMgr";
import singleton_game from "../../../app/svr_game/singleton_game";


declare global {
    interface Rpc {
        game: {
            main: RpcClass<main>
        }
    }
}

export default class main {
    private app: Application;
    private gameMgr: GameMgr;
    constructor(app: Application) {
        this.app = app;
        this.gameMgr = singleton_game.gameMgr;
    }

    /**
     * 创建游戏桌子
     */
    createTable(gameId: number, gameType: gameType, matchServer: string, rankServer: string, gameParam: I_gameParam, players: I_matchPlayer[]) {
        this.gameMgr.createTable(gameId, gameType, matchServer, rankServer, gameParam, players);
    }

    /**
     * 某个比赛结束
     */
    closeGame(gameId: number) {
        this.gameMgr.closeGame(gameId);
    }

    /**
     * 掉线
     */
    offline(gameId: number, tableId: number, uid: number) {
        let table = this.gameMgr.getTable<I_tableBase>(gameId, tableId);
        if (table) {
            table.leave(uid);
        }
    }

    /**
     * 重新进入桌子
     */
    enterTable(gameId: number, tableId: number, uid: number, sid: string, cb: (err: rpcErr, ok: boolean) => void) {
        let table = this.gameMgr.getTable<I_tableBase>(gameId, tableId);
        if (table) {
            let ok = table.enterTable(uid, sid);
            cb(0, ok);
        } else {
            cb(0, false);
        }
    }

    /**
     * 同步豆子到桌子里
     * @param gameId 
     * @param tableId 
     * @param uid 
     * @param douzi 
     */
    syncDouziToTable(gameId: number, tableId: number, uid: number, douzi: number) {
        let table = this.gameMgr.getTable<I_tableBase>(gameId, tableId);
        table.syncDouziToTable(uid, douzi);
    }
}