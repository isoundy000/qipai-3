import { GameMainMgr } from "./gameMainMgr";
import mysqlClient from "../dbService/mysql";

interface I_singleton_gameMain {
    mysql: mysqlClient,
    gameMainMgr: GameMainMgr,
}

let singleton_gameMain: I_singleton_gameMain = {
    mysql: null as any,
    gameMainMgr: null as any,
}

export default singleton_gameMain
