import { Application, Session } from "mydog";
import { GameMgr } from "../../../app/svr_game/gameMgr";
import { Table_WuZiQi } from "../../../app/svr_game/wuZiQi/table_wuZiQi";
import singleton_game from "../../../app/svr_game/singleton_game";
import { getGameState } from "../../../app/util/gameUtil";

export default class Handler {
    private gameMgr: GameMgr;
    constructor(app: Application) {
        this.gameMgr = singleton_game.gameMgr;
    }

    play(msg: { "i": number, "j": number }, session: Session, next: Function) {
        let gameState = getGameState(session);
        let table = this.gameMgr.getTable<Table_WuZiQi>(gameState.gameId, gameState.tableId);
        if (table) {
            table.play(session.uid, msg);
        }
    }
}