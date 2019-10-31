import { Application, Session } from "mydog";
import { RoleInfoMgr } from "../../../app/svr_info/roleInfoMgr";
import { I_bag } from "../../../app/util/someInterface";
import singleton_info from "../../../app/svr_info/singleton_info";

export default class Handler {
    private roleInfoMgr: RoleInfoMgr;
    constructor(app: Application) {
        this.roleInfoMgr = singleton_info.roleInfoMgr;
    }


    // 阅读邮件
    readMail(msg: { "id": number, "uid": number }, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        let res = role.mail.readMail(msg.id, msg.uid === 0);
        if (res) {
            next(res);
        }
    }

    // 邮件领奖
    getMailAward(msg: { "id": number, "uid": number }, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        let res = role.mail.getMailAward(msg.id, msg.uid === 0);
        if (res) {
            next(res);
        }
    }

    // 删除邮件
    delMail(msg: { "id": number, "uid": number }, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        let res = role.mail.delMail(msg.id, msg.uid === 0);
        if (res) {
            next(res);
        }
    }

    // 发送赠送邮件
    sendMail(msg: { "getUid": number, "info": string, "items": I_bag[] }, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        role.mail.sendMail(msg);
    }

}