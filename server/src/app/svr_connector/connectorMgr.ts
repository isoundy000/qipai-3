import { gameType, gameState } from "../../config/gameConfig";
import { I_gameInfoClient } from "../svr_gameMain/gameMainMgr";


export class ConnectorMgr {
    private notTellInfoSvrUid: { [uid: number]: boolean } = {}; //  掉线后不用通知info服的uid集合
    private games: { [gameType: number]: { [gameState: number]: I_gameInfoClient[] } } = {};    // 所有的游戏（游戏类型：游戏状态：游戏列表）
    private gamesAll: { [id: number]: I_gameInfoClient } = {};  // 所有的游戏
    constructor() {

    }

    /**
     * 该玩家掉线后不必通知info服
     */
    setUid(uid: number) {
        this.notTellInfoSvrUid[uid] = true;
    }

    /**
     * 是否需要通知info服
     */
    isNeedTell(uid: number) {
        let has = this.notTellInfoSvrUid[uid] || false;
        if (has) {
            delete this.notTellInfoSvrUid[uid];
        }
        return !has;
    }

    /**
     * 新的游戏
     * @param gameInfo 
     */
    newGame(gameInfo: I_gameInfoClient) {
        if (!this.games[gameInfo.gameType]) {
            this.games[gameInfo.gameType] = {};
        }
        if (!this.games[gameInfo.gameType][gameInfo.state]) {
            this.games[gameInfo.gameType][gameInfo.state] = [];
        }
        this.games[gameInfo.gameType][gameInfo.state].push(gameInfo);
        this.gamesAll[gameInfo.id] = gameInfo;
    }

    /**
     * 更新游戏状态
     * @param id 
     * @param state 
     */
    updateGameState(gameInfo: { "id": number, "gameType": gameType, "state": gameState, "rankSvr": string, "matchSvr": string }) {
        let lastState = gameInfo.state - 1;
        let arr = this.games[gameInfo.gameType][lastState];
        let one: I_gameInfoClient = null as any;
        for (let i = arr.length - 1; i >= 0; i--) {
            if (arr[i].id === gameInfo.id) {
                one = arr[i];
                arr.splice(i, 1);
                break;
            }
        }
        if (!one) {
            console.error("严重错误，未找到对应游戏");
            return;
        }
        if (gameInfo.state === gameState.close) {
            delete this.gamesAll[gameInfo.id];
            return;
        }
        if (!this.games[gameInfo.gameType][gameInfo.state]) {
            this.games[gameInfo.gameType][gameInfo.state] = [];
        }
        one.state = gameInfo.state;
        one.matchServer = gameInfo.matchSvr;
        one.rankServer = gameInfo.rankSvr;
        this.games[gameInfo.gameType][gameInfo.state].push(one);
    }

    /**
     * gm 关闭某类型比赛
     * @param gameType 
     */
    gmCloseGameByType(gameType: gameType) {
        for (let state in this.games[gameType]) {
            this.games[gameType][state].forEach((one) => {
                delete this.gamesAll[one.id];
            });
        }
        delete this.games[gameType];
    }

    /**
     * 获取对应游戏列表
     * @param msg 
     */
    getGameList(msg: { "gameType": gameType, "state": gameState, "pageIndex": number }): { "games": I_gameInfoClient[] } {
        if (!this.games[msg.gameType]) {
            return { "games": [] };
        }
        if (!this.games[msg.gameType][msg.state]) {
            return { "games": [] };
        }
        return { "games": this.games[msg.gameType][msg.state].slice(msg.pageIndex * 10, (msg.pageIndex + 1) * 10) };
    }

    /**
     * 根据游戏id获取游戏
     */
    getGameByIds(ids: number[]): { "games": I_gameInfoClient[] } {
        let list: I_gameInfoClient[] = [];
        for (let id of ids) {
            if (this.gamesAll[id]) {
                list.push(this.gamesAll[id]);
            }
        }
        return { "games": list };
    }

    /**
     * 搜索比赛
     * @param search 
     */
    searchGame(search: string): { "games": I_gameInfoClient[] } {
        search = search.trim();
        if (search.length === 0 || search.length > 8) {
            return { "games": [] };
        }
        let id = Number(search);
        let gameList: I_gameInfoClient[] = [];
        if (!isNaN(id)) {
            if (this.gamesAll[id]) {
                gameList.push(this.gamesAll[id]);
            }
            return { "games": gameList };
        }
        let keys = search.split(/ +/).slice(0, 2);
        let patt = new RegExp(keys.join("|"));
        for (let x in this.gamesAll) {
            if (patt.test(this.gamesAll[x].gameName)) {
                gameList.push(this.gamesAll[x]);
            }
            if (gameList.length >= 10) {
                break;
            }
        }
        return { "games": gameList };
    }
}