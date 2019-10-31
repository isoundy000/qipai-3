import { Application, Session } from "mydog";
import { GameMgr, I_tableBase } from "../../../app/svr_game/gameMgr";
import { I_playerGameState } from "../../../app/svr_gameMain/gameMainMgr";
import singleton_game from "../../../app/svr_game/singleton_game";
import { getGameState } from "../../../app/util/gameUtil";

export default class Handler {
    private gameMgr: GameMgr;
    constructor(app: Application) {
        this.gameMgr = singleton_game.gameMgr;
    }

    leave(msg: any, session: Session, next: Function) {
        let gameState = getGameState(session);
        let table = this.gameMgr.getTable<I_tableBase>(gameState.gameId, gameState.tableId);
        if (table) {
            table.leave(session.uid);
        }
    }

    chat(msg: { msg: string }, session: Session, next: Function) {
        let gameState = getGameState(session);
        let table = this.gameMgr.getTable<I_tableBase>(gameState.gameId, gameState.tableId);
        if (table) {
            table.chat(session.uid, msg.msg);
        }
    }

    chatSeq(msg: { index: number }, session: Session, next: Function) {
        let gameState = getGameState(session);
        let table = this.gameMgr.getTable<I_tableBase>(gameState.gameId, gameState.tableId);
        if (table) {
            table.chatSeq(session.uid, msg.index);
        }
    }
}