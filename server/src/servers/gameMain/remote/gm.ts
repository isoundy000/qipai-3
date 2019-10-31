import { Application } from "mydog";
import { GameMainMgr } from "../../../app/svr_gameMain/gameMainMgr";
import singleton_gameMain from "../../../app/svr_gameMain/singleton_gameMain";
import { gameType, gameTypeState } from "../../../config/gameConfig";


export default class GmRemote {
    private app: Application;
    private gameMainMgr: GameMainMgr;
    constructor(app: Application) {
        this.app = app;
        this.gameMainMgr = singleton_gameMain.gameMainMgr;
    }

    openOrCloseOneGame(gameId: number, isOpen: boolean) {
        this.gameMainMgr.gmOpenOrCloseOneGame(gameId, isOpen);
    }

    gmChangeGameTypeState(gameType: gameType, state: gameTypeState) {
        this.gameMainMgr.gmChangeGameTypeState(gameType, state);
    }

}