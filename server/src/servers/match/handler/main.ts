import { Application, Session } from "mydog";
import { MatchMgr } from "../../../app/svr_match/matchMgr";
import singleton_match from "../../../app/svr_match/singleton_match";
import { getGameState } from "../../../app/util/gameUtil";


export default class Handler {
    private app: Application;
    private matchMgr: MatchMgr;
    constructor(app: Application) {
        this.app = app;
        this.matchMgr = singleton_match.matchMgr;
    }


    /**
     * 离开桌子
     */
    leaveTable(msg: any, session: Session, next: Function) {
        let table = this.matchMgr.getMatchTable(getGameState(session));
        if (table) {
            table.leaveTable(session.uid);
        }
        next(null);
    }

    /**
     * 准备或取消准备
     */
    readyOrNot(msg: any, session: Session, next: Function) {
        let table = this.matchMgr.getMatchTable(getGameState(session));
        if (table) {
            table.readyOrNot(session.uid);
        }
    }

    /**
     * 开始匹配
     */
    startMatch(msg: any, session: Session, next: Function) {
        let table = this.matchMgr.getMatchTable(getGameState(session));
        if (table) {
            table.startMatch(session.uid);
        }
    }

    /**
     * 取消匹配
     */
    cancelMatch(msg: any, session: Session, next: Function) {
        let table = this.matchMgr.getMatchTable(getGameState(session));
        if (table) {
            table.cancelMatch(session.uid);
        }
    }

    /**
     * 聊天
     */
    chat(msg: any, session: Session, next: Function) {
        let table = this.matchMgr.getMatchTable(getGameState(session));
        if (table) {
            table.chat(session, msg.data);
        }
    }

    /**
     * 踢人
     */
    kickPlayer(msg: any, session: Session, next: Function) {
        let table = this.matchMgr.getMatchTable(getGameState(session));
        if (table) {
            table.kickPlayer(session.uid, msg.uid);
        }
    }

    /**
     * 邀请好友，进入匹配桌子
     */
    inviteFriend(msg: { "uid": number, "sid": string }, session: Session, next: Function) {
        let table = this.matchMgr.getMatchTable(getGameState(session));
        if (table) {
            table.inviteFriend(session.uid, msg);
        }
    }
}