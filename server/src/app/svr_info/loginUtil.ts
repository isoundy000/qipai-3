import { I_roleInfo, I_bag, I_roleInfoMysql, I_roleAllInfo, I_friendInfo, I_mailInfo } from "../util/someInterface";
import * as util from "../util/util"
import { rpcErr, Application } from "mydog";
import { I_publicMailIds } from "./mail";
import singleton_info from "./singleton_info";
import { ItemId } from "../../config/item";
import { getInfoId } from "../util/gameUtil";

/**
 * 登录时的处理
 */
export class LoginUtil {
    private app: Application
    constructor(app: Application) {
        this.app = app;
    }

    getAllRoleInfo(uid: number, cb: (err: any, allInfo: I_roleAllInfo) => void) {
        let self = this;
        self.getRoleInfo(uid, function (err, role) {
            if (err) {
                return cb(err, null as any);
            }
            self.getFriends(uid, function (err, friends) {
                if (err) {
                    return cb(err, null as any);
                }
                self.getMails(uid, function (err, mails, publicMailIds) {
                    if (err) {
                        return cb(err, null as any);
                    }
                    cb(null, { "role": role, "friends": friends, "mails": mails, "publicMailIds": publicMailIds });
                });
            });
        })
    }


    private getRoleInfo(uid: number, cb: (err: any, info: I_roleInfo) => void) {
        let self = this;
        let sql = "select * from player where uid = ? limit 1";
        singleton_info.mysql.query(sql, [uid], function (err, res) {
            if (err) {
                return cb(err, null as any);
            }
            if (res.length === 0) {
                self.createRole(uid, cb);
            } else {
                cb(null, self.endRoleInfo(res[0]));
            }
        });
    }

    private createRole(uid: number, cb: (err: any, info: I_roleInfo) => void) {
        let nowTime = util.timeFormat(new Date());
        let defaultBag: { [id: string]: number } = {};
        defaultBag[ItemId.douzi] = 1000;
        let tmp_player_mysql: I_roleInfoMysql = {
            "uid": uid,
            "headId": 1,
            "loginTime": nowTime,
            "nickname": "豆豆" + uid,
            "regTime": nowTime,
            "sex": 1,
            "signature": "吃饭，睡觉，打豆豆！",
            "loginDays": 1,
            "ifGetAward": 0,
            "bag": defaultBag,
            "gameData": [],
        }
        let self = this;
        tmp_player_mysql.bag = JSON.stringify(tmp_player_mysql.bag) as any;
        tmp_player_mysql.gameData = JSON.stringify(tmp_player_mysql.gameData) as any;
        singleton_info.mysql.insert("player", tmp_player_mysql, function (err) {
            if (err) {
                return cb(err, null as any);
            }
            cb(null, self.endRoleInfo(tmp_player_mysql));
        });
    }

    private endRoleInfo(tmp_player_mysql: I_roleInfoMysql) {
        tmp_player_mysql.bag = JSON.parse(tmp_player_mysql.bag as any);
        tmp_player_mysql.gameData = JSON.parse(tmp_player_mysql.gameData as any);
        type infoNotMysql = Exclude<keyof I_roleInfo, keyof I_roleInfoMysql>
        let dataNotMysql: { [p in infoNotMysql]: I_roleInfo[p] } = {
            "sid": "",
            "gameId": 0,
            "gameSvr": "",
            "tableId": 0,
            "chairId": 0,
            "token": 0,
        };
        tmp_player_mysql.loginTime = util.timeFormat(tmp_player_mysql.loginTime);
        tmp_player_mysql.regTime = util.timeFormat(tmp_player_mysql.regTime);
        let roleInfo: I_roleInfo = tmp_player_mysql as any;
        for (let key in dataNotMysql) {
            (roleInfo as any)[key] = (dataNotMysql as any)[key];
        }
        return roleInfo;
    }

    private getFriends(uid: number, cb: (err: any, friends: I_friendInfo[]) => void) {
        let sql = "select * from friend where uid = ?";
        let self = this;
        singleton_info.mysql.query(sql, [uid], function (err: any, res: { "id": number, "uid": number, "uidF": number }[]) {
            if (err) {
                return cb(err, []);
            }
            let friendList: I_friendInfo[] = [];
            let latchdown = util.createLatchdown(res.length, function callback() {
                cb(null, friendList);
            });

            for (let one of res) {
                self.app.rpc(getInfoId(one.uidF)).info.friend.getFriendInfo(one.uidF, (err, friendInfo) => {
                    if (err || !friendInfo) {
                        return latchdown.down();
                    }
                    friendList.push(friendInfo);
                    latchdown.down();
                });
            }
        });
    }

    /**
     * 从数据库中获取一些好友信息
     */
    getFriendInfoFromDb(uid: number, cb: (err: rpcErr, info: I_friendInfo) => void) {
        let sql = "select sex,nickname,signature from player where uid = ? limit 1";
        singleton_info.mysql.query(sql, [uid], function (err, res) {
            if (err || res.length === 0) {
                return cb(0, null as any);
            }
            let info: I_friendInfo = {
                "uid": uid,
                "sid": "",
                "sex": res[0]["sex"],
                "nickname": res[0]["nickname"],
                "signature": res[0]["signature"],
            }
            cb(0, info);
        });
    }

    private getMails(uid: number, cb: (err: any, mails: I_mailInfo[], publicMailIds: I_publicMailIds) => void) {
        let sql = "select * from mail where uid = ?";
        singleton_info.mysql.query(sql, [uid], function (err: any, res) {
            if (err) {
                return cb(err, null as any, null as any);
            }
            let sql2 = "select * from mail_all where uid = ? limit 1";
            singleton_info.mysql.query(sql2, [uid], function (err: any, res2) {
                if (err) {
                    return cb(err, null as any, null as any);
                }
                for (let one of res) {
                    one.items = JSON.parse(one.items);
                    one.createTime = util.timeFormat(one.createTime);
                    one.expireTime = util.timeFormat(one.expireTime);
                }
                let mailIds: I_publicMailIds = {
                    "readIds": [],
                    "getAwardIds": [],
                    "delIds": [],
                };
                if (res2.length === 1) {
                    mailIds.readIds = JSON.parse(res2[0]["readIds"]);
                    mailIds.getAwardIds = JSON.parse(res2[0]["getAwardIds"]);
                    mailIds.delIds = JSON.parse(res2[0]["delIds"]);
                }
                cb(null, res, mailIds);
            });
        });
    }

}