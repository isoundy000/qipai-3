import { RpcClass, Application, rpcErr } from "mydog";
import singleton_login from "../../../app/svr_login/singleton_login";

declare global {
    interface Rpc {
        login: {
            main: RpcClass<Handler>,
        }
    }
}

export default class Handler {
    constructor(app: Application) {
    }
    /**
     * 验证token
     * @param uid 
     * @param token 
     * @param cb 
     */
    isTokenOk(uid: number, token: number, cb: (err: rpcErr, ok: boolean) => void) {
        let ok = singleton_login.loginMgr.isTokenOk(uid, token);
        cb(0, ok);
    }
}