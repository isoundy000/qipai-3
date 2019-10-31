import { Application, Session } from "mydog";
import { gameLog } from "../../../app/logger";
import { I_playerGameState } from "../../../app/svr_gameMain/gameMainMgr";
import singleton_connector from "../../../app/svr_connector/singleton_connector";
import { gameTypes, gameTimeDiff, paramNames, refreshMyRankCost, createGameHourPer, createGameDiamondPer, douziGetPercent, rollNotices, sendRollNoticeCost } from "../../../config/gameParamConfig";
import { getInfoId } from "../../../app/util/gameUtil";
import { gameType, gameState } from "../../../config/gameConfig";


export default class Handler {
    app: Application;
    constructor(app: Application) {
        this.app = app;
    }

    /**
     * 获取一些配置
     */
    getSomeConfig(msg: any, session: Session, next: Function) {
        let endResult = {} as any;
        endResult["refreshMyRankCost"] = refreshMyRankCost;
        endResult["createGameHourPer"] = createGameHourPer;
        endResult["createGameDiamondPer"] = createGameDiamondPer;
        endResult["douziGetPercent"] = douziGetPercent;
        endResult["gameTypes"] = gameTypes;
        endResult["gameTimeDiff"] = gameTimeDiff;
        endResult["gameParamName"] = paramNames;
        endResult["rollNotices"] = rollNotices;
        endResult["sendRollNoticeCost"] = sendRollNoticeCost;
        endResult["notice"] = "抵制不良游戏，拒绝盗版游戏。\n" +
            "注意自我保护，谨防受骗上当。\n" +
            "适度游戏益脑，沉迷游戏伤身。\n" +
            "合理安排时间，享受健康生活。";
        next(endResult);
    }

    /**
     * 登录
     */
    enter(msg: { "uid": number, "token": number }, session: Session, next: Function) {
        if (!msg.uid || session.uid || isNaN(msg.uid)) {
            return next({ "code": 1 });
        }
        gameLog.debug("登录: ", msg.uid);
        this.app.rpc("login").login.main.isTokenOk(msg.uid, msg.token, (err, ok) => {
            if (err || !ok) {
                return next({ "code": 1 });
            }
            let infoId = getInfoId(msg.uid);
            this.app.rpc(infoId).info.main.enterServer(msg.uid, this.app.serverId, msg.token, (err, info) => {
                if (err || info.code !== 0) {
                    return next({ "code": 1 });
                }
                let tmpRole = info.info.role;
                session.set<I_playerGameState>("gameState", { "gameId": tmpRole.gameId, "gameSvr": tmpRole.gameSvr, "tableId": tmpRole.tableId, "chairId": tmpRole.chairId });
                session.set("infoId", infoId);
                session.bind(msg.uid);
                session.setCloseCb(onUserLeave);

                let endResult = {} as any;
                endResult["code"] = info.code;
                endResult["info"] = info.info;
                next(endResult);
            });
        });
    }

    /**
     * 重连
     */
    reconnectEnter(msg: { "uid": number, "token": number }, session: Session, next: Function) {
        if (session.uid) {
            return;
        }
        let self = this;
        let infoId = getInfoId(msg.uid);
        self.app.rpc(infoId).info.main.reconnectEntry(msg.uid, this.app.serverId, msg.token, function (err, info) {
            if (err || info.code !== 0) {
                return next({ "code": 1 });
            }
            let tmpRole = info.info.role;
            session.set<I_playerGameState>("gameState", { "gameId": tmpRole.gameId, "gameSvr": tmpRole.gameSvr, "tableId": tmpRole.tableId, "chairId": tmpRole.chairId });
            session.set("infoId", infoId);
            session.bind(msg.uid);
            session.setCloseCb(onUserLeave);

            let endResult = {} as any;
            endResult["code"] = info.code;
            endResult["info"] = info.info;
            next(endResult);
        });
    }

    /**
     * 获取对应的游戏列表
     */
    getGameList(msg: { "gameType": gameType, "state": gameState, "pageIndex": number }, session: Session, next: Function) {
        if (msg.pageIndex < 1) {
            return;
        }
        msg.pageIndex--;
        next(singleton_connector.connectorMgr.getGameList(msg));
    }

    /**
     * 获取最近游玩的比赛
     * @param msg 
     * @param session 
     * @param next 
     */
    getRecentPlay(msg: { "recent": number[] }, session: Session, next: Function) {
        if (msg.recent.length > 5) {
            return;
        }
        next(singleton_connector.connectorMgr.getGameByIds(msg.recent));
    }

    /**
     * 搜索游戏
     * @param msg 
     * @param session 
     * @param next 
     */
    searchGame(msg: { "search": string }, session: Session, next: Function) {
        next(singleton_connector.connectorMgr.searchGame(msg.search));
    }
}


// 玩家socket断开
function onUserLeave(app: Application, session: Session) {
    gameLog.info("--- one user leave :", session.uid);

    if (!session.uid) {
        return;
    }
    let isNeedTell = singleton_connector.connectorMgr.isNeedTell(session.uid);
    if (isNeedTell) {
        app.rpc(getInfoId(session.uid)).info.main.offline(session.uid);
    }
}
