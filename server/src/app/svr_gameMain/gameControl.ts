import { I_gameInfoClient, GameMainMgr } from "./gameMainMgr";
import JobArr from "../util/jobArr";
import { gameLog } from "../logger";
import { Application } from "mydog";
import { timeFormat, createLatchdown } from "../util/util";
import { gameState, serverType, gameType } from "../../config/gameConfig";
import singleton_gameMain from "./singleton_gameMain";

/**
 * 单个游戏的控制
 */
export class GameControl {
    private app: Application;
    private gameInfo: I_gameInfoClient;
    private jobArr: JobArr;
    constructor(app: Application, gameInfo: I_gameInfoClient) {
        // console.log(gameInfo);
        this.app = app;
        this.gameInfo = gameInfo;
        this.jobArr = new JobArr([gameInfo.startTime, gameInfo.endTime, gameInfo.closeTime], this.stateEvent, this);
    }

    private stateEvent(index: number, isStart: boolean) {
        console.log("gameControl:", index, isStart)
        if (index === 0) {    // 即将开始时间段
            this.tellConnectorNewGame();
        }
        else if (index === 1)   // 进行中时间段
        {
            if (this.gameInfo.state !== gameState.start) {
                this.gameInfo.state = gameState.start;
                singleton_gameMain.mysql.query("update game set state = ? where id = ?", [gameState.start, this.gameInfo.id]);
            }
            this.selectMatchSvr();
            this.selectRankSvr();
            this.createRankService();
            this.createMatchService();
            if (isStart) {
                this.tellConnectorNewGame();
            } else {
                this.tellConnectorUpdateGameState();
            }
        }
        else if (index === 2)  // 已结算未关闭时间段
        {
            if (isStart) {
                this.selectRankSvr();
                this.createRankService();
                if (this.gameInfo.state !== gameState.end) {
                    this.gameInfo.state = gameState.end;
                    singleton_gameMain.mysql.query("update game set state = ? where id = ?", [gameState.end, this.gameInfo.id]);
                    this.rankServiceSettlement();
                }
                this.tellConnectorNewGame();
            } else {
                this.gameInfo.state = gameState.end;
                singleton_gameMain.mysql.query("update game set state = ? where id = ?", [gameState.end, this.gameInfo.id]);
                this.closeMatchService();
                this.rankServiceSettlement();
                this.tellConnectorUpdateGameState();
            }

        } else if (index === 3) {  // 已关闭时间段
            singleton_gameMain.mysql.query("update game set state = ? where id = ?", [gameState.close, this.gameInfo.id]);
            if (isStart) {
                if (this.gameInfo.state < gameState.end) {   // 还未结算，需要结算一下
                    this.selectRankSvr();
                    this.createRankService();
                    this.rankServiceSettlement();
                    this.closeRankService(false);
                }
                setTimeout(this.delThisFromMain.bind(this), 0); //刚启动时便删除，由于上层还未赋值，所以需要下一帧才删
            } else {
                this.gameInfo.state = gameState.close;
                this.closeRankService(false);
                this.tellConnectorUpdateGameState();
                this.delThisFromMain();
            }
        }
    }

    /**
     * 删除自己
     */
    private delThisFromMain() {
        singleton_gameMain.gameMainMgr.delGameMatchByControl(this.gameInfo);
    }

    /**
     * 选择一个匹配服
     */
    private selectMatchSvr() {
        let matchSvrs = this.app.getServersByType(serverType.match);
        let choosed = matchSvrs[Math.floor(Math.random() * matchSvrs.length)];
        this.gameInfo.matchServer = choosed.id;
    }

    /**
     * 创建匹配服务
     */
    private createMatchService() {
        this.app.rpc(this.gameInfo.matchServer).match.main.createGameMatch(this.gameInfo);
    }

    /**
     * 关闭匹配服务
     */
    private closeMatchService() {
        let svrs = this.app.getServersByType(serverType.game);
        for (let one of svrs) {
            this.app.rpc(one.id).game.main.closeGame(this.gameInfo.id);
        }
        this.app.rpc(this.gameInfo.matchServer).match.main.closeGameMatch(this.gameInfo.id);
        this.gameInfo.matchServer = "";
    }

    /**
     * 选择一个排行服
     */
    private selectRankSvr() {
        let rankSvrs = this.app.getServersByType(serverType.rank);
        let choosed = rankSvrs[Math.floor(Math.random() * rankSvrs.length)];
        this.gameInfo.rankServer = choosed.id;
    }

    /**
     * 创建排行服务
     */
    private createRankService() {
        this.app.rpc(this.gameInfo.rankServer).rank.main.createGameRank(this.gameInfo);
    }

    /**
     * 通知排行服务进行结算
     */
    private rankServiceSettlement() {
        this.app.rpc(this.gameInfo.rankServer).rank.main.gameSettlement(this.gameInfo.id);
    }

    /**
     * 关闭排行服务
     */
    private closeRankService(isGm: boolean) {
        this.app.rpc(this.gameInfo.rankServer).rank.main.closeGameRank(this.gameInfo.id, isGm);
        this.gameInfo.rankServer = "";
    }

    /**
     * 通知客户端，有新的比赛
     */
    private tellConnectorNewGame() {
        this.app.rpc("*").connector.game.newGame(this.gameInfo);
    }

    /**
     * 通知客户端，更新游戏状态
     */
    private tellConnectorUpdateGameState() {
        this.app.rpc("*").connector.game.updateGameState({
            "id": this.gameInfo.id,
            "state": this.gameInfo.state,
            "gameType": this.gameInfo.gameType,
            "rankSvr": this.gameInfo.rankServer,
            "matchSvr": this.gameInfo.matchServer
        });
    }

    /**
     * 被gm关闭
     */
    gmClose() {
        this.delThisFromMain();
        if (this.gameInfo.matchServer) {
            this.closeMatchService();
        }
        if (this.gameInfo.rankServer) {
            this.closeRankService(true);
        }
        this.jobArr.close();

        this.gameInfo.state = gameState.close;
        this.tellConnectorUpdateGameState();

    }

    /**
     * gm关闭了该类型比赛
     * @param gameType 
     */
    gmCloseByType(gameType: gameType) {
        if (this.gameInfo.gameType === gameType) {
            this.delThisFromMain();
            if (this.gameInfo.matchServer) {
                this.closeMatchService();
            }
            if (this.gameInfo.rankServer) {
                this.closeRankService(true);
            }
            this.jobArr.close();
        }
    }
}

