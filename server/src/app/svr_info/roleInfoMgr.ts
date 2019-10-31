import { Application, rpcErr } from "mydog";
import { RoleInfo } from "./roleInfo";
import { I_roleAllInfo, I_mailInfo, I_roleAllInfoClient } from "../util/someInterface";
import * as util from "../util/util"
import { LoginUtil } from "./loginUtil";
import singleton_info from "./singleton_info";

export class RoleInfoMgr {
    private app: Application;
    public loginUtil: LoginUtil;                        // 登陆 util
    private roles: { [uid: number]: RoleInfo } = {};    // 所有玩家数据
    private publicMails: I_mailInfo[] = [];             // 全服邮件
    constructor(app: Application) {
        this.app = app;
        this.loginUtil = new LoginUtil(app);
        singleton_info.mysql.select("mail", "*", { "uid": 0 }, (err: any, res: any) => {
            if (err) {
                console.error("全服邮件加载失败:", this.app.serverId, err);
                return;
            }
            for (let one of res) {
                one.items = JSON.parse(one.items);
                one.createTime = util.timeFormat(one.createTime);
                one.expireTime = util.timeFormat(one.expireTime);
                this.publicMails.push(one);
            }
        });
    }

    /**
     * 玩家登录游戏
     * @param uid 
     */
    enterServer(uid: number, sid: string, token: number, cb: (err: rpcErr, info: { "code": number, "info": I_roleAllInfoClient }) => void) {
        let role = this.roles[uid];
        if (!role) {
            this.loginUtil.getAllRoleInfo(uid, (err, allInfo) => {
                if (err) {
                    console.log(err);
                    return cb(0, { code: -1 } as any);
                }
                role = new RoleInfo(this.app, allInfo);
                this.roles[uid] = role;
                role.role.token = token;
                role.entryServerLogic(sid, cb);
            });
            return;
        }
        console.log('信息已存在', role.role.sid);
        if (role.role.sid) {
            let data = { "uid": uid, "info": "您的账号在别处登陆" };
            this.app.rpc(role.role.sid).connector.main.kickUserNotTellInfoSvr(data, (err) => {
                if (err && err !== rpcErr.noServer) {
                    return cb(0, { code: -1 } as any);
                }
                role.offline();
                role.role.token = token;
                role.entryServerLogic(sid, cb);
            });
        } else {
            role.role.token = token;
            role.entryServerLogic(sid, cb);
        }
    }

    /**
     * 重连
     */
    reconnectEntry(uid: number, sid: string, token: number, cb: (err: rpcErr, info: { "code": number, "info": I_roleAllInfoClient }) => void) {
        let role = this.roles[uid];
        if (!role) {
            return cb(1, null as any);
        }
        if (role.role.token !== token) {
            return cb(1, null as any);
        }
        if (role.role.sid) {
            let data = { "uid": uid, "info": "您的账号在别处登陆" };
            this.app.rpc(role.role.sid).connector.main.kickUserNotTellInfoSvr(data, (err) => {
                if (err && err !== rpcErr.noServer) {
                    return cb(0, { code: -1 } as any);
                }
                role.offline();
                role.entryServerLogic(sid, cb);
            });
        } else {
            role.entryServerLogic(sid, cb);
        }
    }

    /**
     * 获取全服邮件
     */
    getPublicMails() {
        let now = Date.now();
        let delIds: number[] = [];
        let mails: I_mailInfo[] = [];
        for (let i = this.publicMails.length - 1; i >= 0; i--) {
            let one = this.publicMails[i];
            if (now - new Date(one.expireTime).getTime() < 0) {
                mails.push(one);
            } else {
                delIds.push(one.id);
                this.publicMails.splice(i, 1);
            }
        }
        if (delIds.length > 0) {
            singleton_info.mysql.query("delete from mail where id in (" + delIds.join(',') + ")", null);
        }
        return mails;
    }

    /**
     * 获取某一个全服邮件
     */
    getPublicMailById(id: number) {
        for (let one of this.publicMails) {
            if (id === one.id) {
                return one;
            }
        }
        return null;
    }

    /**
     * 获取玩家信息
     */
    getRole(uid: number) {
        return this.roles[uid];
    }

    deleteRole(uid: number) {
        delete this.roles[uid];
    }
}

