import { Application, RpcClass, rpcErr } from "mydog";
import { I_gameInfo, I_gameInfoClient } from "../../../app/svr_gameMain/gameMainMgr";
import { gameLog } from "../../../app/logger";
import { RankMgr } from "../../../app/svr_rank/rankMgr";
import singleton_rank from "../../../app/svr_rank/singleton_rank";
import { errCode } from "../../../config/errCode";
import { I_roleRankInfo } from "../../../app/svr_rank/rankService";
import { gameType } from "../../../config/gameConfig";


declare global {
    interface Rpc {
        rank: {
            main: RpcClass<mainRemote>
        }
    }
}

export default class mainRemote {
    private app: Application;
    private rankMgr: RankMgr;
    constructor(app: Application) {
        this.app = app;
        this.rankMgr = singleton_rank.rankMgr;
    }

    /**
     * 创建游戏排行服务
     */
    createGameRank(gameInfo: I_gameInfoClient) {
        gameLog.debug("创建排行服务")
        this.rankMgr.createRank(gameInfo);
    }

    /**
     * 关闭游戏排行
     */
    closeGameRank(gameId: number, isGm: boolean) {
        this.rankMgr.closeRank(gameId, isGm);
    }

    /**
     * 结算
     */
    gameSettlement(gameId: number) {
        let service = this.rankMgr.getRankService(gameId);
        if (service) {
            service.gameSettlement();
        }
    }

    /**
     * 获取玩家分数
     */
    getPlayerScore(gameId: number, uid: number, cb: (err: rpcErr, score: number) => void) {
        let service = this.rankMgr.getRankService(gameId);
        if (service) {
            service.getPlayerScore(uid, cb)
        } else {
            cb(5, 0);
        }
    }

    /**
     * 设置玩家分数
     */
    setPlayerScore(gameId: number, uid: number, score: number) {
        let service = this.rankMgr.getRankService(gameId);
        if (service) {
            service.setPlayerScore(uid, score)
        }
    }

    /**
     * 请求进入游戏
     */
    wantPlayGame(gameId: number, uid: number, douzi: number, pwd: string, cb: (err: rpcErr, msg: { code: number, gameType?: gameType, score?: number, doorCost?: number }) => void) {
        let service = this.rankMgr.getRankService(gameId);
        if (service) {
            service.wantPlayGame(uid, douzi, pwd, cb)
        } else {
            cb(0, { "code": 101 });
        }
    }

    /**
     * 获取排行榜
     */
    getRankList(gameId: number, uid: number, cb: (err: rpcErr, code: number, msg: { "ranklist": I_roleRankInfo[] }) => void) {
        let service = this.rankMgr.getRankService(gameId);
        if (service) {
            service.getRankList(uid, cb);
        } else {
            cb(0, 1, null as any);
        }
    }
    /**
     * 刷新个人最新战绩
     */
    refreshMyRank(gameId: number, uid: number, cb: (err: rpcErr, code: number, msg: { "meRank": number, "meScore": number }) => void) {
        let service = this.rankMgr.getRankService(gameId);
        if (service) {
            service.refreshMyRank(uid, cb);
        } else {
            cb(0, 1, null as any);
        }
    }

    /**
     * 匹配成功，创建桌子时，要扣除台费
     * @param gameId 
     * @param num 
     */
    addTableCost(gameId: number, num: number) {
        let service = this.rankMgr.getRankService(gameId);
        if (service) {
            service.addTableCost(num);
        }
    }
}