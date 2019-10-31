import { RoleInfoMgr } from "./roleInfoMgr";
import mysqlClient from "../dbService/mysql";

interface I_singleton_info {
    mysql: mysqlClient
    roleInfoMgr: RoleInfoMgr
}

let singleton_info: I_singleton_info = {
    mysql: null as any,
    roleInfoMgr: null as any,
}

export default singleton_info
