import { LoginMgr } from "./loginMgr";
import mysqlClient from "../dbService/mysql";
import { GmService } from "./gmService";

interface I_singleton_login {
    mysql: mysqlClient,
    loginMgr: LoginMgr,
    gmService: GmService,
}

let singleton_login: I_singleton_login = {
    mysql: null as any,
    loginMgr: null as any,
    gmService: null as any,
}

export default singleton_login
