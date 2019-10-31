import { Application, rpcErr } from "mydog";
import { gameLog } from "../logger";
import { GameControl } from "./gameControl";
import { timeFormat, isString } from "../util/util";
import { gameType, gameState, gameTypeState, I_gameTypeInfo, I_gameParam } from "../../config/gameConfig";
import singleton_gameMain from "./singleton_gameMain";
import { gameTypes } from "../../config/gameParamConfig";
import { mysqlTable } from "../../config/dbConfig";
import { isNumber } from "util";

/**
 * 游戏比赛总控制
 */
export class GameMainMgr {
    private app: Application;
    private games: { [id: number]: GameControl } = {};
    constructor(app: Application) {
        this.app = app;
        setTimeout(() => {
            this.init();
        }, 3000);
    }

    /**
     * 初始化游戏比赛
     */
    private init() {
        // 读取游戏类型开放状态
        singleton_gameMain.mysql.query(`select * from ${mysqlTable.gametypestate}`, null, (err, gameStateInfo: { "gameType": gameType, "state": gameTypeState }[]) => {
            if (err) {
                gameLog.error("start error!!!: ", err);
                return;
            }
            let normalGameType: number[] = [];
            for (let one of gameStateInfo) {
                let oneGameType = this.findGameTypeInfo(one.gameType);
                if (one.state !== oneGameType.state) {
                    oneGameType.state = one.state;
                    this.app.rpc("*").connector.game.updateGameTypeState(one.gameType, one.state);
                }
                if (one.state === gameTypeState.normal) {
                    normalGameType.push(one.gameType);
                }
            }

            // 加载正常开放的游戏比赛
            if (normalGameType.length === 0) {
                gameLog.error("gameMain:  no normal gameType!!!: ");
                return;
            }
            let str = `select * from ${mysqlTable.game} where state != ${gameState.close} and gmClosed = 0 and gameType in ( ${normalGameType.join(',')} )`;
            singleton_gameMain.mysql.query(str, [normalGameType.join(',')], (err, res: I_gameInfo[]) => {
                if (err) {
                    gameLog.error("start error!!!: ", err);
                    return;
                }
                for (let i = 0; i < res.length; i++) {
                    this.initGame(res[i] as I_gameInfoClient);
                }
            });
        });
    }

    /**
     * 初始化某一个游戏
     * @param one 
     */
    private initGame(one: I_gameInfoClient) {
        one.createTime = timeFormat(one.createTime);
        one.startTime = timeFormat(one.startTime);
        one.endTime = timeFormat(one.endTime);
        one.closeTime = timeFormat(one.closeTime);
        one.gameParam = JSON.parse(one.gameParam as any);
        one.award = JSON.parse(one.award as any);
        one.matchServer = "";
        one.rankServer = "";
        if (this.games[one.id]) {
            return;
        }
        this.games[one.id] = new GameControl(this.app, one);
    }


    /**
     * 创建游戏比赛  （暂未对游戏比赛数量以及前端显示数量作限制）
     */
    createGameMatch(gameInfo: I_gameInfo, cb: (err: rpcErr, ok: boolean) => void) {
        let oneGameTypeInfo = this.findGameTypeInfo(gameInfo.gameType);
        if (oneGameTypeInfo.state !== gameTypeState.normal) {
            return cb(0, false);
        }
        delete gameInfo["id"];
        gameInfo.gameParam = JSON.stringify(gameInfo.gameParam) as any;
        gameInfo.award = JSON.stringify(gameInfo.award) as any;
        singleton_gameMain.mysql.insert(mysqlTable.game, gameInfo, (err, res) => {
            if (err) {
                return cb(0, false);
            }
            cb(0, true);
            gameInfo.id = res.insertId;
            this.initGame(gameInfo as I_gameInfoClient);
        });
    }

    /**
     * 删除比赛   （由gameControl调用）
     * @param one 
     */
    delGameMatchByControl(one: I_gameInfoClient) {
        delete this.games[one.id];
    }

    /**
     * 查找某一游戏类型信息
     */
    private findGameTypeInfo(gameType: gameType) {
        let res: I_gameTypeInfo = null as any;
        for (let one of gameTypes) {
            if (one.type === gameType) {
                res = one;
                break;
            }
        }
        return res;
    }

    /**
     * gm 开启或关闭某比赛
     * @param gameId 
     */
    gmOpenOrCloseOneGame(gameId: number, isOpen: boolean) {
        let game = this.games[gameId];
        if (!isOpen && game) {
            let str = `update ${mysqlTable.game} set gmClosed = 1 where id = ?`;
            singleton_gameMain.mysql.query(str, [gameId], (err, res) => {
                if (err) {
                    console.log(err);
                    return;
                }
                game.gmClose();
            });
        } else if (isOpen && !game) {
            let str = `select * from ${mysqlTable.game} where id = ? limit 1`;
            singleton_gameMain.mysql.query(str, [gameId], (err, res: I_gameInfo[]) => {
                if (err) {
                    console.log(err);
                    return;
                }
                if (res.length === 0) {
                    return;
                }
                let one = res[0];
                if (one.state === gameState.close) {
                    console.log("该比赛已结束，不可开启")
                    return;
                }
                let gameTypeInfo = this.findGameTypeInfo(one.gameType);
                if (gameTypeInfo.state !== gameTypeState.normal) {
                    console.log(gameTypeInfo.name, "该类型比赛未正常开放，不可开启该比赛");
                    return;
                }
                this.initGame(one as I_gameInfoClient);
            });
        }
    }

    /**
     * gm改变某类型比赛状态
     */
    gmChangeGameTypeState(tmpGameType: gameType, state: gameTypeState) {
        if (!isNumber(tmpGameType) || !isNumber(state) || !gameType[tmpGameType] || !gameTypeState[state]) {
            return;
        }
        let oneGameType = this.findGameTypeInfo(tmpGameType);
        if (state === oneGameType.state) {
            return;
        }
        let str = `update ${mysqlTable.gametypestate} set state = ? where gameType = ?`;
        singleton_gameMain.mysql.query(str, [state, tmpGameType], (err, res: I_gameInfo[]) => {
            if (err) {
                console.log(err)
                return;
            }

            let lastState = oneGameType.state;
            oneGameType.state = state;
            this.app.rpc("*").connector.game.updateGameTypeState(tmpGameType, state);

            if (lastState === gameTypeState.normal) {   // 需要关闭该类型比赛
                for (let id in this.games) {
                    this.games[id].gmCloseByType(tmpGameType);
                }
            } else if (state === gameTypeState.normal) {   // 需要开启该类型比赛
                let str = `select * from ${mysqlTable.game} where state != ${gameState.close} and gmClosed = 0 and gameType = ?`;
                singleton_gameMain.mysql.query(str, [tmpGameType], (err, res: I_gameInfo[]) => {
                    if (err) {
                        gameLog.error("gmChangeGameTypeState error!!!: ", err);
                        return;
                    }
                    for (let i = 0; i < res.length; i++) {
                        this.initGame(res[i] as I_gameInfoClient);
                    }
                });
            }
        });
    }
}





/**
 * 游戏基本信息（数据库中）
 */
export interface I_gameInfo {
    id: number,
    roleId: number,
    roleName: string,
    gameType: gameType,
    gameName: string,
    gameNotice: string,
    createTime: string,
    startTime: string,
    endTime: string,
    closeTime: string,
    gameParam: I_gameParam,
    rankNum: number,
    password: string,
    award: { "rank": number, "num": number }[],
    state: gameState,
    gmClosed: number,
    getDouzi: number,
}


/**
 * 游戏基本信息（给客户端的）
 */
export interface I_gameInfoClient extends I_gameInfo {
    matchServer: string,
    rankServer: string,
}

export interface I_playerGameState {
    gameId: number,
    gameSvr: string,
    tableId: number,
    chairId: number,
}

/**
 * 空的游戏状态
 */
export let emptyGamestate: I_playerGameState = {
    "gameId": 0,
    "gameSvr": "",
    "tableId": 0,
    "chairId": 0
}