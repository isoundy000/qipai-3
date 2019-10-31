import { Application } from "mydog";
import * as http from "http"
import * as querystring from "querystring"
import { Handler } from "./loginMsgHandler";
import mysqlClient from "../dbService/mysql";
import { serverType } from "../../config/gameConfig";
import { gameLog } from "../logger";

export interface UserToken {
    token: number,
    time: number,
}

export class LoginMgr {
    private port: number = 3000;
    private msgHandler: Handler;
    public app: Application;
    private userTokens: { [uid: number]: UserToken } = {};

    constructor(app: Application) {
        this.app = app;
        this.msgHandler = new Handler();
        this.startHttpSvr();
    }

    private startHttpSvr() {
        let server = http.createServer((request: http.IncomingMessage, response: http.ServerResponse) => {
            if (request.method !== "POST") {
                return;
            }
            let msg = "";
            request.on("data", (chuck) => {
                msg += chuck;
            });
            request.on("end", () => {
                let body = querystring.parse(msg) as any;
                gameLog.debug("------login---->>>>---", body);
                if (body && body.method && (this.msgHandler as any)[body.method]) {
                    (this.msgHandler as any)[body.method](body, this.callback(response));
                }
            });
        });
        server.listen(this.port, () => {
            console.log("--- login server (http)  running at port: " + this.port + ".");
        });
        server.on("error", function (err) {
            console.log("--- login server error::", err.message);
        });

        setInterval(this.rpcGetUserNum.bind(this), 5000);
    }

    private callback(response: http.ServerResponse): (data: any) => void {
        return function (data: any) {
            if (data === undefined) {
                data = null;
            }
            gameLog.debug("------login----<<<<---", data);
            response.end(JSON.stringify(data));
        }
    }


    private rpcGetUserNum() {

        // 获取网关服连接数
        let svrs = this.app.getServersByType(serverType.connector);
        for (let one of svrs) {
            this.app.rpc(one.id).connector.main.getUserNum(function (err, num) {
                if (err) {
                    return;
                }
                one.userNum = num;
            });
        }

        // 清除过期token
        let now = Date.now();
        for (let uid in this.userTokens) {
            if (this.userTokens[uid].time < now) {
                delete this.userTokens[uid];
            }
        }
    }

    /**
     * 设置token
     * @param uid 
     * @param token 
     */
    setUserToken(uid: number, token: number) {
        this.userTokens[uid] = { "time": Date.now() + 60 * 1000, "token": token };
    }

    /**
     * 验证token
     * @param uid 
     * @param token 
     */
    isTokenOk(uid: number, token: number) {
        let one = this.userTokens[uid];
        if (one && one.token === token) {
            delete this.userTokens[uid];
            return true;
        }
        return false;
    }
}