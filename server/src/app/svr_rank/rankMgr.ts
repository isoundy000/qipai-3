import { Application } from "mydog";
import { RankService } from "./rankService";
import { I_gameInfo, I_gameInfoClient } from "../svr_gameMain/gameMainMgr";
import { gameLog } from "../logger";


/**
 * 排行服务管理器
 */
export class RankMgr {
    private app: Application;
    private rankServices: { [id: number]: RankService } = {};
    constructor(app: Application) {
        this.app = app;
    }

    /**
     * 创建排行服务
     */
    createRank(gameInfo: I_gameInfoClient) {
        if (this.rankServices[gameInfo.id]) {
            gameLog.error("该匹配服务已存在：", gameInfo);
            return;
        }
        this.rankServices[gameInfo.id] = new RankService(this.app, gameInfo);
    }


    /**
     * 关闭排行服务
     */
    closeRank(gameId: number, isGm: boolean) {
        let one = this.rankServices[gameId];
        if (one) {
            delete this.rankServices[gameId];
            one.close(isGm);
        }
    }

    /**
     * 获取排行服务
     */
    getRankService(gameId: number) {
        return this.rankServices[gameId];
    }



}