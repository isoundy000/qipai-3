import mysqlClient from "../dbService/mysql";
import { LoginMgr } from "./loginMgr";
import { serverType } from "../../config/gameConfig";
import singleton_login from "./singleton_login";


let ErrCode = {
    "DBErr": { "code": -1 },
    "userNotExist": { "code": 1 },
    "passwordWrong": { "code": 2 },
    "nameExist": { "code": 3 }
};

export class Handler {
    constructor() {
    }

    /**
     * 登录
     * @param msg 
     * @param next 
     */
    login(msg: { "username": string, "password": string }, next: (data: any) => void) {
        singleton_login.mysql.query("select * from account where username = ?", [msg.username], (err: any, res: any) => {
            if (err) {
                next(ErrCode.DBErr);
                return;
            }
            if (res.length === 0) {
                next(ErrCode.userNotExist);
                return;
            }
            if (res[0].password !== msg.password) {
                next(ErrCode.passwordWrong);
                return;
            }
            let data = this.getSuccessData(res[0].uid);
            next(data);
        });
    }

    /**
     * 注册
     * @param msg 
     * @param next 
     */
    register(msg: { "username": string, "password": string }, next: (data: any) => void) {
        singleton_login.mysql.query("select uid from account where username = ?", [msg.username], (err, res) => {
            if (err) {
                next(ErrCode.DBErr);
                return;
            }
            if (res.length !== 0) {
                next(ErrCode.nameExist);
                return;
            }
            singleton_login.mysql.query("insert into account(username, password, regtime) values(?, ?, ?)", [msg.username, msg.password, new Date()], (err, res) => {
                if (err) {
                    next(ErrCode.DBErr);
                    return;
                }
                let data = this.getSuccessData(res.insertId);
                next(data);
            });
        });
    }

    private getSuccessData(uid: number) {
        let svrs = singleton_login.loginMgr.app.getServersByType(serverType.connector);
        let minSvr = svrs[0];
        minSvr.userNum = minSvr.userNum || 0;
        for (let one of svrs) {
            one.userNum = one.userNum || 0;
            if (one.userNum < minSvr.userNum) {
                minSvr = one;
            }
        }
        minSvr.userNum++;
        let token = Math.floor(Math.random() * 10000000);
        singleton_login.loginMgr.setUserToken(uid, token);
        let data = {
            "code": 0,
            "host": minSvr.clientHost,
            "port": minSvr.clientPort,
            "uid": uid,
            "token": token
        }
        return data
    }
}