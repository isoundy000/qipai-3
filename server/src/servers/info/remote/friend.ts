import { Application, rpcErr } from "mydog";
import { RoleInfoMgr } from "../../../app/svr_info/roleInfoMgr";
import { I_friendInfo, I_friendInfoChange, I_mailInfo } from "../../../app/util/someInterface";
import singleton_info from "../../../app/svr_info/singleton_info";

export default class FriendRemote {
    private roleInfoMgr: RoleInfoMgr;
    constructor(app: Application) {
        this.roleInfoMgr = singleton_info.roleInfoMgr;
    }

    /**
     * 玩家登录时，获取好友信息
     */
    getFriendInfo(uid: number, cb: (err: rpcErr, info: I_friendInfo | null) => void) {
        let role = this.roleInfoMgr.getRole(uid);
        if (role) {
            let tmpInfo = role.role;
            let info: I_friendInfo = {
                "uid": uid,
                "sid": tmpInfo.sid,
                "nickname": tmpInfo.nickname,
                "sex": tmpInfo.sex,
                "signature": tmpInfo.signature
            }
            return cb(0, info);
        }
        this.roleInfoMgr.loginUtil.getFriendInfoFromDb(uid, cb);
    }


    /**
     * 好友申请的通知
     */
    askFriendTell(uid: number, info: { "uid": number, "nickname": string, "sex": number }) {
        let role = this.roleInfoMgr.getRole(uid);
        if (role) {
            role.friend.askFriendTell(info);
        }
    }

    /**
     * 同意好友的通知
     */
    agreeFriendTell(uid: number, info: I_friendInfo, cb: (err: rpcErr, info: I_friendInfo) => void) {
        let role = this.roleInfoMgr.getRole(uid);
        if (role) {
            role.friend.agreeFriendTell(info, cb);
        } else {
            cb(0, null as any);
        }
    }

    /**
     * 删除好友的通知
     */
    delFriendTell(uid: number, uidF: number) {
        let role = this.roleInfoMgr.getRole(uid);
        if (role) {
            role.friend.delFriendTell(uidF);
        }
    }

    /**
     * 好友信息改变
     */
    friendInfoChangeTell(uid: number, info: I_friendInfoChange) {
        let role = this.roleInfoMgr.getRole(uid);
        if (role) {
            role.friend.friendInfoChangeTell(info);
        }
    }

    /**
     * 发送邮件
     */
    sendMailTell(uid: number, info: I_mailInfo) {
        let role = this.roleInfoMgr.getRole(uid);
        if (role) {
            role.mail.sendMailTell(info);
        }
    }
}

