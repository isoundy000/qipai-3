import { Application, RpcClass, rpcErr } from "mydog";
import Game_Remote from "./game";
import { I_playerGameState, emptyGamestate } from "../../../app/svr_gameMain/gameMainMgr";
import { cmd } from "../../../config/cmd";
import singleton_connector from "../../../app/svr_connector/singleton_connector";
import { I_notice } from "../../../config/gameConfig";


declare global {
    interface Rpc {
        connector: {
            main: RpcClass<Main_Remote>,
            game: RpcClass<Game_Remote>
        }
    }
}

export default class Main_Remote {
    app: Application;
    constructor(app: Application) {
        this.app = app;
    }
    getUserNum(cb: (err: rpcErr, clientNum: number) => void) {
        cb(null as any, this.app.getBindClientNum());
    }

    kickUserNotTellInfoSvr(msg: { "uid": number, "info": string }, cb?: (err: rpcErr) => void) {
        if (this.app.hasClient(msg.uid)) {
            this.app.sendMsgByUid(cmd.onKicked, { "code": 1, "info": msg.info }, [msg.uid]);
            singleton_connector.connectorMgr.setUid(msg.uid);
            this.app.closeClient(msg.uid);
        }
        cb && cb(0);
    }

    kickUser(msg: { "uid": number, "info": string }, cb?: (err: rpcErr) => void) {
        if (this.app.hasClient(msg.uid)) {
            this.app.sendMsgByUid(cmd.onKicked, { "code": 1, "info": msg.info }, [msg.uid]);
            this.app.closeClient(msg.uid);
        }
        cb && cb(0);
    }

    /**
     * 配置玩家session
     */
    applySomeSession(uid: number, someSession: any, cb?: (err: rpcErr) => void) {
        this.app.applySession(uid, someSession);
        cb && cb(rpcErr.ok);
    }

    /**
     * 配置玩家gameState session
     */
    applyGameStateSession(uid: number, gameState: I_playerGameState, cb?: (err: rpcErr) => void) {
        this.app.applySession(uid, { "gameState": gameState });
        cb && cb(rpcErr.ok);
    }

    /**
     * 发送全服滚动公告
     * @param notice 
     */
    sendRollNotice(notice: I_notice) {
        this.app.sendAll(cmd.onNotice, notice);
    }
}