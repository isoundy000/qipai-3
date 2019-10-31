import { I_publicMailIds } from "../svr_info/mail";
import { gameType } from "../../config/gameConfig";

/**
 * uid, sid
 */
export interface I_uidsid {
    "uid": number,
    "sid": string,
}

/**
 * 玩家基本数据
 */
export interface I_roleAllInfo {
    "role": I_roleInfo,
    "friends": I_friendInfo[],
    "mails": I_mailInfo[],
    "publicMailIds"?: I_publicMailIds
}

/**
 * 玩家基本数据
 */
export interface I_roleAllInfoClient {
    "role": I_roleInfo,
    "bag": I_bag[],
    "friends": I_friendInfo[],
    "mails": I_mailInfo[],
    "publicMailIds"?: I_publicMailIds,
}

/**
 * 背包
 */
export interface I_bag {
    "id": number,
    "num": number,
}

/**
 * 游戏类型统计数据
 */
export interface I_gameData {
    "type": gameType,   // 游戏类型
    "all": number,      // 总场次
    "win": number,      // 胜场次
    "score": number,    // 分数
}


/**
 * 玩家数据结构 mysql
 */
export interface I_roleInfoMysql {
    "uid": number,              // uid
    "nickname": string,         // 昵称
    "sex": number,              // 性别
    "headId": number,           // 头像id
    "signature": string,        // 个性签名
    "loginTime": string,        // 上次登录时间
    "regTime": string,          // 注册时间
    "loginDays": number,        // 连续登录天数
    "ifGetAward": number,       // 今天是否已领取
    "bag": { [id: string]: number },  // 背包
    "gameData": I_gameData[], // 游戏统计数据 (总场次，胜场次，平均天梯积分)
}

/**
 * 玩家数据
 */
export interface I_roleInfo extends I_roleInfoMysql {
    "sid": string,              // sid
    "gameId": number,           // game id
    "gameSvr": string,          // game svr
    "tableId": number,          // 桌子id
    "chairId": number,          // 椅子序号
    "token": number,            // token
}

/**
 * 好友信息
 */
export interface I_friendInfo {
    "uid": number,
    "sid": string,
    "nickname": string,
    "sex": number,
    "signature": string,
}

/**
 * 好友信息改变，通知
 */
export interface I_friendInfoChange {
    "uid": number,
    "sid": string,
    "nickname"?: string,
    "sex"?: number,
    "signature"?: string,
}

/**
 * 邮件信息
 */
export interface I_mailInfo {
    "id": number,
    "uid": number,
    "topic": string,
    "content": string,
    "items": I_bag[],
    "status": number,
    "createTime": string,
    "expireTime": string,
    "sendUid": number,
    "sendName": string,
}

export interface DicObj<T = any> {
    [key: string]: T
}

/**
 * 游戏结算信息（客户端显示）
 */
export interface I_gameOverResult {
    isGameClosed: boolean;  // 游戏是否关闭
    winUids: number[];  // 胜者uid
    userList: I_gameOverUserInfo[]; // 结算列表
}

export interface I_gameOverUserInfo {
    "uid": number,
    "name": string,
    "sex": number,
    "score": number,
    "scoreAdd": number,
    "douzi": number,
    "douziAdd": number,
}