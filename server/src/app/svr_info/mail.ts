import { RoleInfo } from "./roleInfo";
import { I_mailInfo, I_bag } from "../util/someInterface";
import { timeFormat } from "../util/util";
import { cmd } from "../../config/cmd";
import singleton_info from "./singleton_info";
import * as gameUtil from "../util/gameUtil";
import { Items } from "../../config/item";


export class Mail {
    private role: RoleInfo;
    private mails: I_mailInfo[];
    private publicMailIds: I_publicMailIds;
    constructor(role: RoleInfo, mails: I_mailInfo[], publicMailIds: I_publicMailIds) {
        this.role = role;
        this.mails = mails;
        this.publicMailIds = publicMailIds;
    }

    /**
     * 获取邮件
     */
    getMails() {
        let mails: I_mailInfo[] = [];
        let mailDelIds: number[] = [];
        let now = Date.now();
        for (let i = this.mails.length - 1; i >= 0; i--) {
            let one = this.mails[i];
            if (now - new Date(one.expireTime).getTime() < 0) {
                mails.push(one);
            } else {
                mailDelIds.push(one.id);
                this.mails.splice(i, 1);
            }
        }
        if (mailDelIds.length > 0) {
            singleton_info.mysql.query("delete from mail where id in (" + mailDelIds.join(',') + ")", null);
        }

        mails = mails.concat(this.getPublicMails());
        return mails;
    }

    /**
     * 获取全服邮件
     */
    private getPublicMails() {
        let publicMails = singleton_info.roleInfoMgr.getPublicMails();
        let change = false;
        let readIds = this.publicMailIds.readIds;
        let getAwardIds = this.publicMailIds.getAwardIds;
        let delIds = this.publicMailIds.delIds;

        function isMailIn(mails: I_mailInfo[], id: number) {
            for (let one of mails) {
                if (one.id === id) {
                    return true;
                }
            }
            return false;
        }

        // 删除玩家全服邮件中，已经不存在的邮件id
        for (let i = readIds.length - 1; i >= 0; i--) {
            if (!isMailIn(publicMails, readIds[i])) {
                readIds.splice(i, 1);
                change = true;
            }
        }
        for (let i = getAwardIds.length - 1; i >= 0; i--) {
            if (!isMailIn(publicMails, getAwardIds[i])) {
                getAwardIds.splice(i, 1);
                change = true;
            }
        }
        for (let i = delIds.length - 1; i >= 0; i--) {
            if (!isMailIn(publicMails, delIds[i])) {
                delIds.splice(i, 1);
                change = true;
            }
        }

        let mails: I_mailInfo[] = [];
        // 确定玩家全服邮件状态
        for (let one of publicMails) {
            if (readIds.indexOf(one.id) !== -1) {
                one.status = mailStatus.read;
                mails.push(one);
            } else if (getAwardIds.indexOf(one.id) !== -1) {
                one.status = mailStatus.get_award;
                mails.push(one);
            } else if (delIds.indexOf(one.id) === -1) {
                one.status = mailStatus.not_read;
                mails.push(one);
            }
        }
        if (change) {
            let sqlStr = "insert into mail_all(uid, readIds, getAwardIds, delIds) values(?,?,?,?) on " +
                "duplicate key update readIds=values(readIds), getAwardIds=values(getAwardIds), delIds=values(delIds)";
            singleton_info.mysql.query(sqlStr, [this.role.uidsid.uid, JSON.stringify(readIds), JSON.stringify(getAwardIds), JSON.stringify(delIds)]);
        }
        return mails;
    }

    private getMailById(id: number) {
        for (let one of this.mails) {
            if (one.id === id) {
                return one;
            }
        }
        return null;
    }

    /**
     * 阅读邮件
     */
    readMail(id: number, isPublic: boolean) {

        // 个人邮件
        if (!isPublic) {
            let mail = this.getMailById(id);
            if (!mail) {
                return { "code": 1, "id": id };
            }
            mail.status = mailStatus.read;
            singleton_info.mysql.update("mail", { "status": mailStatus.read }, { "id": id });
            return { "code": 0, "id": id };
        }

        // 全服邮件
        let mail = singleton_info.roleInfoMgr.getPublicMailById(id);
        if (!mail) {
            return { "code": 1, "id": id };
        }
        let readIds = this.publicMailIds.readIds;
        let getAwardIds = this.publicMailIds.getAwardIds;
        let delIds = this.publicMailIds.delIds;
        if (readIds.indexOf(id) !== -1 || getAwardIds.indexOf(id) !== -1 || delIds.indexOf(id) !== -1) {
            return;
        }
        readIds.push(id);
        let sqlStr = "insert into mail_all(uid, readIds, getAwardIds, delIds) values(?,?,?,?) on " +
            "duplicate key update readIds=values(readIds), getAwardIds=values(getAwardIds), delIds=values(delIds)";
        singleton_info.mysql.query(sqlStr, [this.role.uidsid.uid, JSON.stringify(readIds), JSON.stringify(getAwardIds), JSON.stringify(delIds)]);
        return { "code": 0, "id": id };
    }

    /**
     * 邮件领奖
     */
    getMailAward(id: number, isPublic: boolean) {
        // 个人邮件
        if (!isPublic) {
            let mail = this.getMailById(id);
            if (!mail) {
                return { "code": 1, "id": id };
            }
            if (mail.status !== mailStatus.read || mail.items.length === 0) {
                return;
            }
            mail.status = mailStatus.get_award;
            singleton_info.mysql.update("mail", { "status": mailStatus.get_award }, { "id": id, });

            this.role.addItems(changeAwardStruct(mail.items), true);
            return { "code": 0, "id": id };
        }

        // 全服邮件
        let mail = singleton_info.roleInfoMgr.getPublicMailById(id);
        if (!mail) {
            return { "code": 1, "id": id };
        }
        let index = this.publicMailIds.readIds.indexOf(id);
        if (index === -1 || mail.items.length === 0) {
            return;
        }
        this.publicMailIds.readIds.splice(index, 1);
        this.publicMailIds.getAwardIds.push(id);
        let updateData = {
            "readIds": JSON.stringify(this.publicMailIds.readIds),
            "getAwardIds": JSON.stringify(this.publicMailIds.getAwardIds),
            "delIds": JSON.stringify(this.publicMailIds.delIds)
        };
        singleton_info.mysql.update("mail_all", updateData, { "uid": this.role.uidsid.uid });
        this.role.addItems(changeAwardStruct(mail.items), true);


        return { "code": 0, "id": id };

        function changeAwardStruct(items: I_bag[]) {
            let obj: { [id: string]: number } = {};
            for (let one of items) {
                obj[one.id] = (obj[one.id] || 0) + one.num;
            }
            return obj;
        }
    }

    /**
     * 删除邮件
     */
    delMail(id: number, isPublic: boolean) {
        // 个人邮件
        if (!isPublic) {
            let mail = this.getMailById(id);
            if (!mail) {
                return { "code": 0, "id": id };
            }
            for (let i = this.mails.length - 1; i >= 0; i--) {
                if (this.mails[i].id === id) {
                    this.mails.splice(i, 1);
                    break;
                }
            }
            singleton_info.mysql.delete("mail", { "id": id });
            return { "code": 0, "id": id };
        }

        // 全服邮件
        let readIds = this.publicMailIds.readIds;
        let getAwardIds = this.publicMailIds.getAwardIds;
        let delIds = this.publicMailIds.delIds;
        if (delIds.indexOf(id) !== -1) {
            return;
        }
        let index = readIds.indexOf(id);
        let index2 = getAwardIds.indexOf(id)
        if (index === -1 && index2 === -1) {
            return;
        }
        if (index !== -1) {
            readIds.splice(index, 1);
        }
        if (index2 !== -1) {
            getAwardIds.splice(index2, 1);
        }
        delIds.push(id);
        let updateData = {
            "readIds": JSON.stringify(readIds),
            "getAwardIds": JSON.stringify(getAwardIds),
            "delIds": JSON.stringify(delIds)
        };
        singleton_info.mysql.update("mail_all", updateData, { "uid": this.role.uidsid.uid });
        return { "code": 0, "id": id };
    }

    /**
     * 发送邮件
     */
    sendMail(msg: { "getUid": number, "info": string, "items": I_bag[] }) {
        if (!this.role.friend.getFriend(msg.getUid)) {
            return;
        }
        if (!msg.items || !(msg.items instanceof Array)) {
            return;
        }
        let cost: { [id: string]: number } = {};
        for (let one of msg.items) {
            one.num = parseInt(one.num as any)
            if (!Items[one.id] || !one.num || one.num < 0) {
                return;
            }
            cost[one.id] = one.num;
        }
        if (!this.role.hasItems(cost)) {
            return;
        }
        let expireTime = new Date();
        expireTime.setDate(expireTime.getDate() + 7);
        let mailData: I_mailInfo = {
            "id": 0,
            "uid": msg.getUid,
            "topic": "好友赠送",
            "content": msg.info,
            "items": msg.items,
            "status": mailStatus.not_read,
            "createTime": timeFormat(new Date()),
            "expireTime": timeFormat(expireTime),
            "sendUid": this.role.uidsid.uid,
            "sendName": this.role.role.nickname
        };
        gameUtil.sendMail(singleton_info.mysql, mailData, (err: any, res: any) => {
            if (err) {
                console.log(err);
                return;
            }
            if (msg.items.length > 0) {
                this.role.costItems(cost, true);
            }
            mailData.id = res.insertId;
            this.role.app.rpc(gameUtil.getInfoId(msg.getUid)).info.friend.sendMailTell(msg.getUid, mailData);
        });
    }

    /**
     * 发送邮件消息
     */
    sendMailTell(mail: I_mailInfo) {
        this.mails.push(mail);
        this.role.app.sendMsgByUidSid(cmd.onNewMail, mail, [{ "uid": this.role.uidsid.uid, "sid": this.role.role.sid }]);
    }


}

export const enum mailStatus {
    not_read = 0,
    read = 1,
    get_award = 2,
}

export interface I_publicMailIds {
    "readIds": number[],
    "getAwardIds": number[],
    "delIds": number[],
}