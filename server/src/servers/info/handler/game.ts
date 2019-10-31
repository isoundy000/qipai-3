import { Application, Session, rpcErr } from "mydog";
import { errCode } from "../../../config/errCode";
import { I_playerGameState, I_gameInfo } from "../../../app/svr_gameMain/gameMainMgr";
import { RoleInfoMgr } from "../../../app/svr_info/roleInfoMgr";
import { I_matchPlayer } from "../../../app/svr_match/matchTable";
import { serverType, gameType, I_createGameReq } from "../../../config/gameConfig";
import singleton_info from "../../../app/svr_info/singleton_info";
import { refreshMyRankCost } from "../../../config/gameParamConfig";
import { checkCreateGameParam } from "../../../app/util/gameUtil";
import { ItemId } from "../../../config/item";

export default class Handler {
    app: Application
    roleInfoMgr: RoleInfoMgr;
    constructor(app: Application) {
        this.app = app;
        this.roleInfoMgr = singleton_info.roleInfoMgr;
    }


    /**
     * 创建比赛
     */
    createGameMatch(msg: I_createGameReq, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        if (!role) {
            return;
        }
        let res = checkCreateGameParam(msg);
        if (res.code !== 0) {
            return next({ "code": res.code });
        }
        let costOk = role.costItems(res.cost as any, false);
        if (!costOk) {
            return next({ "code": 2 });
        }
        let gameInfo = res.info as I_gameInfo;
        gameInfo.roleId = role.uidsid.uid;
        gameInfo.roleName = role.role.nickname;
        this.app.rpc(serverType.gameMain).gameMain.main.createGameMatch(gameInfo, (err, ok) => {
            if (err || !ok) {
                return next({ "code": -1 });
            }
            next({ "code": 0 });
        });
    }

    /**
     * 创建等待桌子
     */
    createMatchTable(msg: { gameId: number, matchSvr: string, rankSvr: string, pwd: string }, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        if (role.role.gameId) {
            return next({ "code": errCode.createWaitTable.inGame });
        }
        this.app.rpc(msg.rankSvr).rank.main.wantPlayGame(msg.gameId, session.uid, role.getItemNum(ItemId.douzi), msg.pwd, (err, data) => {
            console.log(err, data)
            if (err) {
                return next({ "code": errCode.createWaitTable.noServer });
            }
            if (data.code !== 0) {
                return next({ "code": data.code });
            }
            if (data.doorCost !== 0) {
                let cost = {} as any;
                cost[ItemId.douzi] = data.doorCost;
                role.costItems(cost, false);
            }
            let oneP: I_matchPlayer = {
                "uid": session.uid,
                "sid": session.sid,
                "nickname": role.role.nickname,
                "headId": role.role.headId,
                "sex": role.role.sex,
                "friendUid": session.uid,
                "tableId": 0,
                "isMaster": true,
                "isReady": true,
                "douzi": role.getItemNum(ItemId.douzi),
                "score": data.score as number,
                "scoreA": role.getGameTypeScore(data.gameType as gameType),
            }
            this.app.rpc(msg.matchSvr).match.main.createMatchTable(msg.gameId, [oneP]);
        });
    }

    /**
     * 接受好友邀请进入等待桌子
     */
    enterMatchTable(msg: { "gameId": number, "matchSvr": string, "rankSvr": string, "tableId": number, "friendUid": number, "pwd": string }, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        if (role.role.gameId) {
            return next({ "code": role.role.chairId === 0 ? errCode.enterFriendTable.inWaitTable : errCode.enterFriendTable.inGame });
        }
        this.app.rpc(msg.rankSvr).rank.main.wantPlayGame(msg.gameId, session.uid, role.getItemNum(ItemId.douzi), msg.pwd, (err, data) => {
            console.log(err, data)

            if (err) {
                return next({ "code": errCode.createWaitTable.noServer });
            }
            if (data.code !== 0) {
                return next({ "code": data.code });
            }
            if (data.doorCost !== 0) {
                let cost = {} as any;
                cost[ItemId.douzi] = data.doorCost;
                role.costItems(cost, false);
            }
            let oneP: I_matchPlayer = {
                "uid": session.uid,
                "sid": session.sid,
                "nickname": role.role.nickname,
                "headId": role.role.headId,
                "sex": role.role.sex,
                "friendUid": msg.friendUid,
                "tableId": msg.tableId,
                "isMaster": false,
                "isReady": false,
                "douzi": role.getItemNum(ItemId.douzi),
                "score": data.score as number,
                "scoreA": role.getGameTypeScore(data.gameType as gameType),
            }
            this.app.rpc(msg.matchSvr).match.main.enterFriendTable(msg.gameId, msg.tableId, oneP, function (err, code) {
                if (err) {
                    return next({ "code": errCode.serverErr });
                }
                if (code !== 0) {
                    return next({ "code": code });
                }
            });
        });

    };

    /**
     * 进入桌子
     */
    enterTable(msg: any, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        if (role.role.chairId === 0) {
            return next(null);
        }
        this.app.rpc(role.role.gameSvr).game.main.enterTable(role.role.gameId, role.role.tableId, session.uid, session.sid, function (err, ok) {
            if (err) {
                return next(null);
            }
            if (!ok) {
                next(null);
            }
        });
    };

    /**
     * 获取排行榜
     */
    getRankList(msg: { "gameId": number, "rankSvr": string }, session: Session, next: Function) {
        this.app.rpc(msg.rankSvr).rank.main.getRankList(msg.gameId, session.uid, (err, code, data) => {
            if (err || code !== 0) {
                return;
            }
            next(data);
        });
    }

    /**
     * 获取个人最新战绩
     */
    refreshMyRank(msg: { "gameId": number, "rankSvr": string }, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        let cost = {} as any;
        cost[ItemId.douzi] = refreshMyRankCost;
        if (!role.hasItems(cost)) {
            return;
        }
        this.app.rpc(msg.rankSvr).rank.main.refreshMyRank(msg.gameId, session.uid, (err, code, data) => {
            if (err || code !== 0) {
                return;
            }
            role.costItems(cost, true);
            next(data);
        });
    }
}

