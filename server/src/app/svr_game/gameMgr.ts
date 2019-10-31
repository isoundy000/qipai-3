import { Application } from "mydog";
import { GameService } from "./gameService";
import { I_matchPlayer } from "../svr_match/matchTable";
import { gameType, I_gameParam } from "../../config/gameConfig";


/**
 * 游戏服务管理器
 */
export class GameMgr {
    private app: Application;
    private gameServices: { [id: number]: GameService } = {};
    constructor(app: Application) {
        this.app = app;
    }

    /**
     * 创建游戏桌子
     */
    createTable(gameId: number, gameType: gameType, matchServer: string, rankServer: string, gameParam: I_gameParam, players: I_matchPlayer[]) {
        let service = this.gameServices[gameId];
        if (!service) {
            service = new GameService(this.app, gameId, gameType, matchServer, rankServer, gameParam);
            this.gameServices[gameId] = service;
        }
        service.createTable(players);
    }

    /**
     * 关闭比赛
     */
    closeGame(gameId: number) {
        let service = this.gameServices[gameId];
        if (service) {
            service.closeGame();
        }
    }

    /**
     * 删除比赛（进行中的桌子全部完成游戏时，由gameService调用）
     * @param gameId 
     */
    delGame(gameId: number) {
        delete this.gameServices[gameId];
    }

    getTable<T>(gameId: number, tableId: number): T {
        let service = this.gameServices[gameId];
        if (!service) {
            return null as any;
        }
        return service.getTable(tableId) as any;
    }
}

/**
 * 桌子共有的接口
 */
export interface I_tableBase {
    /**
     * 初始化所有玩家
     */
    initPlayers(players: I_matchPlayer[]): void;
    /**
     * 离开桌子
     */
    leave(uid: number): void;
    /**
     * 重新进入桌子
     */
    enterTable(uid: number, sid: string): boolean;
    /**
     * 聊天
     */
    chat(uid: number, msg: string): void;

    /**
     * 快捷聊天
     */
    chatSeq(uid: number, index: number): void;

    /**
     * 游戏结束，创建匹配桌子
     */
    gameOverCreateMatchTable(): void;

    update(dt: number): void;

    /**
     * 同步豆子到桌子里
     */
    syncDouziToTable(uid: number, douzi: number): void;
}