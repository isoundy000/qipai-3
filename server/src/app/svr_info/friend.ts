import { RoleInfo } from "./roleInfo";
import { I_friendInfo, I_friendInfoChange } from "../util/someInterface";
import { Session, rpcErr } from "mydog";
import { cmd } from "../../config/cmd";
import singleton_info from "./singleton_info";
import { getInfoId } from "../util/gameUtil";

export class Friend {
    private role: RoleInfo;
    private friends: I_friendInfo[];
    constructor(role: RoleInfo, friends: I_friendInfo[]) {
        this.role = role;
        this.friends = friends;
    }

    /**
     * 得到所有好友
     */
    getAllFriends() {
        return this.friends;
    }

    /**
     * 查找好友
     */
    getFriend(uid: number) {
        let tmp: I_friendInfo = null as any
        for (let one of this.friends) {
            if (one.uid === uid) {
                tmp = one;
                break;
            }
        }
        return tmp;
    }

    /**
     * 请求添加好友
     */
    askFriend(friendUid: number) {
        if (friendUid === this.role.uidsid.uid) {
            return;
        }
        if (this.getFriend(friendUid)) {
            return;
        }
        let askInfo = { "uid": this.role.uidsid.uid, "nickname": this.role.role.nickname, "sex": this.role.role.sex };
        this.role.app.rpc(getInfoId(friendUid)).info.friend.askFriendTell(friendUid, askInfo);
    }

    /**
     * 请求添加好友消息
     */
    askFriendTell(info: { "uid": number, "nickname": string, "sex": number }) {
        this.role.app.sendMsgByUidSid(cmd.onAskFriend, info, [this.role.uidsid]);
    }



    /**
     * 同意添加好友
     */
    agreeFriend(friendUid: number) {
        if (friendUid === this.role.uidsid.uid) {
            return;
        }
        if (this.getFriend(friendUid)) {
            return;
        }
        let tmpRole = this.role.role;
        let msg: I_friendInfo = {
            "uid": tmpRole.uid,
            "sid": tmpRole.sid,
            "nickname": tmpRole.nickname,
            "sex": tmpRole.sex,
            "signature": tmpRole.signature,
        }
        this.role.app.rpc(getInfoId(friendUid)).info.friend.agreeFriendTell(friendUid, msg, (err, friendInfo) => {
            if (err || !friendInfo) {
                return;
            }
            if (this.getFriend(friendInfo.uid)) {
                return;
            }
            this.friends.push(friendInfo);
            this.role.app.sendMsgByUidSid(cmd.onAddFriend, friendInfo, [this.role.uidsid]);

            let sql = "insert into friend (uid, uidF) values (?,?),(?,?)";
            singleton_info.mysql.query(sql, [tmpRole.uid, friendInfo.uid, friendInfo.uid, tmpRole.uid]);
        });
    }

    /**
     * 同意添加好友消息
     */
    agreeFriendTell(info: I_friendInfo, cb: (err: rpcErr, info: I_friendInfo) => void) {
        if (this.getFriend(info.uid)) {
            return cb(0, null as any);
        }
        this.friends.push(info);
        let tmpRole = this.role.role;
        this.role.app.sendMsgByUidSid(cmd.onAddFriend, info, [this.role.uidsid]);
        let msg: I_friendInfo = {
            "uid": tmpRole.uid,
            "sid": tmpRole.sid,
            "nickname": tmpRole.nickname,
            "sex": tmpRole.sex,
            "signature": tmpRole.signature,
        }
        cb(0, msg);
    }

    /**
     * 删除好友
     */
    delFriend(friendUid: number) {
        let friend = this.getFriend(friendUid);
        if (!friend) {
            return;
        }
        this.friends.splice(this.friends.indexOf(friend), 1);
        this.role.app.sendMsgByUidSid(cmd.onDelFriend, { "uid": friendUid }, [this.role.uidsid]);
        this.role.app.rpc(getInfoId(friendUid)).info.friend.delFriendTell(friendUid, this.role.uidsid.uid);

        let sql = "delete from friend where (uid = ? and uidF = ?) or (uid = ? and uidF = ?)";
        singleton_info.mysql.query(sql, [this.role.uidsid.uid, friendUid, friendUid, this.role.uidsid.uid]);
    }

    /**
     * 删除好友消息
     */
    delFriendTell(uidF: number) {
        let friend = this.getFriend(uidF);
        if (!friend) {
            return;
        }
        this.friends.splice(this.friends.indexOf(friend), 1);
        this.role.app.sendMsgByUidSid(cmd.onDelFriend, { "uid": uidF }, [this.role.uidsid]);
    }

    /**
     * 信息改变，通知好友
     */
    changeInfo(info: I_friendInfoChange) {
        for (let one of this.friends) {
            this.role.app.rpc(getInfoId(info.uid)).info.friend.friendInfoChangeTell(one.uid, info);
        }
    }

    /**
     * 好友信息改变消息
     */
    friendInfoChangeTell(info: I_friendInfoChange) {
        let friend = this.getFriend(info.uid);
        if (!friend) {
            return;
        }
        friend.sid = info.sid;
        if (info.sex) friend.sex = info.sex;
        if (info.nickname) friend.nickname = info.nickname;
        if (info.signature) friend.signature = info.signature;
        this.role.app.sendMsgByUidSid(cmd.onFriendInfoChange, info, [this.role.uidsid]);
    }
}

