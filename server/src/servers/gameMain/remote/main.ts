import { Application, rpcErr, RpcClass } from "mydog";
import { GameMainMgr, I_gameInfoClient, I_gameInfo } from "../../../app/svr_gameMain/gameMainMgr";
import singleton_gameMain from "../../../app/svr_gameMain/singleton_gameMain";
import GmRemote from "./gm";

declare global {
    interface Rpc {
        gameMain: {
            main: RpcClass<MainRemote>,
            gm: RpcClass<GmRemote>,
        }
    }
}

export default class MainRemote {
    private app: Application;
    private gameMainMgr: GameMainMgr;
    constructor(app: Application) {
        this.app = app;
        this.gameMainMgr = singleton_gameMain.gameMainMgr;
    }

    /**
     * 创建比赛
     */
    createGameMatch(gameInfo: I_gameInfo, cb: (err: rpcErr, ok: boolean) => void) {
        this.gameMainMgr.createGameMatch(gameInfo, cb);
    }
}