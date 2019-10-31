import { Application, RpcClass, rpcErr } from "mydog";
import { I_gameInfoClient } from "../../../app/svr_gameMain/gameMainMgr";
import { gameState, gameType, gameTypeState } from "../../../config/gameConfig";
import { cmd } from "../../../config/cmd";
import { gameTypes } from "../../../config/gameParamConfig";
import singleton_connector from "../../../app/svr_connector/singleton_connector";


export default class Remote_Game {
    private app: Application;
    constructor(app: Application) {
        this.app = app;
    }

    /**
     * 新的游戏
     * @param gameInfo 
     */
    newGame(gameInfo: I_gameInfoClient) {
        singleton_connector.connectorMgr.newGame(gameInfo);
    }

    /**
     * 更新游戏状态
     * @param id 
     * @param state 
     */
    updateGameState(gameInfo: { "id": number, "gameType": gameType, "state": gameState, "rankSvr": string, "matchSvr": string }) {
        singleton_connector.connectorMgr.updateGameState(gameInfo);
    }

    /**
     * 更新游戏类型开放状态
     * @param gameType 
     * @param state 
     */
    updateGameTypeState(gameType: gameType, state: gameTypeState) {
        for (let one of gameTypes) {
            if (one.type === gameType) {
                one.state = state;
                break;
            }
        }
        if (state === gameTypeState.closed || state === gameTypeState.wait) {
            singleton_connector.connectorMgr.gmCloseGameByType(gameType);
        }
        this.app.sendAll(cmd.onUpdateGameTypeState, { "gameType": gameType, "state": state });
    }
}