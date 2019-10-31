import { MatchService } from "./matchService";
import { Application } from "mydog";
import { I_gameInfo, I_gameInfoClient } from "../svr_gameMain/gameMainMgr";
import { gameLog } from "../logger";

/**
 * 匹配服务管理器
 */
export class MatchMgr {
    private app: Application;
    private matchServices: { [id: number]: MatchService } = {};
    constructor(app: Application) {
        this.app = app;
    }

    /**
     * 创建匹配服务
     */
    createMatch(gameInfo: I_gameInfoClient) {
        if (this.matchServices[gameInfo.id]) {
            gameLog.error("该匹配服务已存在：", gameInfo);
            return;
        }
        this.matchServices[gameInfo.id] = new MatchService(this.app, gameInfo);
    }


    /**
     * 关闭匹配服务
     */
    closeMatch(gameId: number) {
        let one = this.matchServices[gameId];
        if (one) {
            delete this.matchServices[gameId];
            one.closeMatch();
        }
    }

    getMatchService(gameId: number): MatchService {
        return this.matchServices[gameId];
    }

    /**
     * 获取匹配桌子
     */
    getMatchTable(param: { "gameId": number, "tableId": number }) {
        let service = this.matchServices[param.gameId];
        if (!service) {
            return null;
        }
        return service.getTable(param.tableId);
    }
}