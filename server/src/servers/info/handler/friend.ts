import { Application, Session } from "mydog";
import { RoleInfoMgr } from "../../../app/svr_info/roleInfoMgr";
import { cmd } from "../../../config/cmd";
import singleton_info from "../../../app/svr_info/singleton_info";

export default class Handler {
    private app: Application;
    private roleInfoMgr: RoleInfoMgr;
    constructor(app: Application) {
        this.app = app;
        this.roleInfoMgr = singleton_info.roleInfoMgr;
    }


    // 请添加好友
    askFriend(msg: { "uid": number }, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        role.friend.askFriend(msg.uid);
    }


    // 同意添加好友
    agreeFriend(msg: { "uid": number }, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        role.friend.agreeFriend(msg.uid);
    }

    // 删除好友
    delFriend(msg: { "uid": number }, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        role.friend.delFriend(msg.uid);
    }

    // 聊天
    chat(msg: any, session: Session, next: Function) {
        let data = {
            "from": session.uid,
            "to": msg.uid,
            "info": msg.data
        };
        this.app.sendMsgByUidSid(cmd.onFriendChat, data, [{ "uid": session.uid, "sid": session.sid }, { "uid": msg.uid, "sid": msg.sid }]);
    }
}