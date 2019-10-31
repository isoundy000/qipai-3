import { RankMgr } from "./rankMgr";
import { redisPool } from "../dbService/redisPool";
import mysqlClient from "../dbService/mysql";

interface I_singleton_rank {
    rankMgr: RankMgr,
    gameRedis: redisPool,
    mysql: mysqlClient

}

let singleton_rank: I_singleton_rank = {
    rankMgr: null as any,
    gameRedis: null as any,
    mysql: null as any,

}

export default singleton_rank
