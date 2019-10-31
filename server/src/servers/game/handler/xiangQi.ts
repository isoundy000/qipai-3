import { Application, Session } from "mydog";
import { GameMgr } from "../../../app/svr_game/gameMgr";
import singleton_game from "../../../app/svr_game/singleton_game";
import { getGameState } from "../../../app/util/gameUtil";
import { Table_XiangQi } from "../../../app/svr_game/xiangQi/table_xiangQi";

export default class Handler {
    private gameMgr: GameMgr;
    constructor(app: Application) {
        this.gameMgr = singleton_game.gameMgr;
    }

    play(msg: { "i": number, "j": number, "i2": number, "j2": number }, session: Session, next: Function) {
        let gameState = getGameState(session);
        let table = this.gameMgr.getTable<Table_XiangQi>(gameState.gameId, gameState.tableId);
        if (table) {
            table.play(session.uid, msg);
        }
    }

    ping(msg: any, session: Session, next: Function) {
        let gameState = getGameState(session);
        let table = this.gameMgr.getTable<Table_XiangQi>(gameState.gameId, gameState.tableId);
        if (table) {
            table.ping(session.uid);
        }
    }

    pingYesOrNo(msg: {"agree": boolean}, session: Session, next: Function){
        let gameState = getGameState(session);
        let table = this.gameMgr.getTable<Table_XiangQi>(gameState.gameId, gameState.tableId);
        if (table) {
            table.pingYesOrNo(session.uid, msg.agree);
        }
    }
}