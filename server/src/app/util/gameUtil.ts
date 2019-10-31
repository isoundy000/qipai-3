import mysqlClient from "../dbService/mysql";
import { I_mailInfo } from "./someInterface";
import { I_gameInfo, I_playerGameState } from "../svr_gameMain/gameMainMgr";
import { gameTypeParam, gameTimeDiff, createGameHourPer, createGameDiamondPer } from "../../config/gameParamConfig";
import { I_param_dropdown, I_param_input, paramType, gameState, serverType, I_createGameReq, I_gameParam } from "../../config/gameConfig";
import { timeFormat, isString } from "./util";
import { ItemId } from "../../config/item";
import { app, Session } from "mydog"

/**
 * 发送邮件
 * @param mysql 
 * @param mail 
 * @param cb 
 */
export function sendMail(mysql: mysqlClient, mail: I_mailInfo, cb: (err: null | Error, res: any) => void) {
    let sql = "insert into mail(uid, topic, content, items, status, createTime, expireTime, sendUid, sendName) values(?,?,?,?,?,?,?,?,?)";
    let args = [mail.uid, mail.topic, mail.content, JSON.stringify(mail.items), 0, mail.createTime, mail.expireTime, mail.sendUid, mail.sendName];
    mysql.query(sql, args, cb);
}

/**
 * 创建游戏时，检查参数正确性
 */
export function checkCreateGameParam(msg: I_createGameReq): { code: number, cost?: { [id: number]: number }, info?: I_gameInfo } {
    let params = gameTypeParam[msg.gameType];
    if (!params) {
        return { "code": 1 };
    }
    if (!isString(msg.gameName) || msg.gameName.length < 3 || msg.gameName.length > 10) {   // 名字3-10个字符
        return { "code": 1 };
    }

    if (!isString(msg.gameNotice) || msg.gameNotice.length > 50) {  // 公告不超过50个字符
        return { "code": 1 };
    }


    let now = Date.now();
    let start = new Date(msg.startTime).getTime();
    let end = new Date(msg.endTime).getTime();
    let close = new Date(msg.closeTime).getTime();
    if (isNaN(start) || isNaN(end) || isNaN(close)) {
        return { "code": 1 };
    }
    if (getHours(start - now) < gameTimeDiff.create_start_minH || getDays(start - now) > gameTimeDiff.create_start_maxD) {
        return { "code": 1 };
    }
    if (getHours(end - start) < gameTimeDiff.start_end_minH || getDays(end - start) > gameTimeDiff.start_end_maxD) {
        return { "code": 1 };
    }
    if (getHours(close - end) < gameTimeDiff.end_close_minH || getDays(close - end) > gameTimeDiff.end_close_maxD) {
        return { "code": 1 };
    }

    let award: { "rank": number, "num": number }[] = msg.award;     // 奖励
    if (msg.award.length !== 5) {
        return { "code": 1 };
    }
    if (award[0].rank !== 1 || award[1].rank !== 2 || award[2].rank !== 3 || award[3].rank <= award[2].rank || award[4].rank <= award[3].rank) {
        return { "code": 1 };
    }
    for (let one of award) {
        if (one.num < 0) {
            return { "code": 1 };
        }
    }

    let rankNum = 0;    // 排行人数
    let gameParam: I_gameParam = {} as any;
    let realDropdown: I_param_dropdown;
    let realInput: I_param_input;
    for (let one of params) {
        switch (one.type) {
            case paramType.rankNum:
                realDropdown = one;
                rankNum = realDropdown.realValues[msg.rankIndex];
                if (rankNum === undefined) {
                    return { "code": 1 };
                }
                break;
            case paramType.password:
                if (!isString(msg.password) || msg.password.length > 8) {
                    return { "code": 1 };
                }
                break;
            case paramType.gameTime:
                realDropdown = one;
                gameParam.gameTime = realDropdown.realValues[msg.otherParam.gameTimeIndex];
                if (gameParam.gameTime === undefined) {
                    return { "code": 1 };
                }
                break;
            case paramType.stepTime:
                realDropdown = one;
                gameParam.stepTime = realDropdown.realValues[msg.otherParam.stepTimeIndex];
                if (gameParam.stepTime === undefined) {
                    return { "code": 1 };
                }
                break;
            case paramType.countTime:
                realDropdown = one;
                gameParam.countTime = realDropdown.realValues[msg.otherParam.countTimeIndex];
                if (gameParam.countTime === undefined) {
                    return { "code": 1 };
                }
                break;
            case paramType.doorCost:
                realInput = one;
                gameParam.doorCost = msg.otherParam.doorCost;
                if (isNaN(gameParam.doorCost) || gameParam.doorCost > realInput.max || gameParam.doorCost < realInput.min) {
                    return { "code": 1 };
                }
                break;
            case paramType.tableCost:
                realInput = one;
                gameParam.tableCost = msg.otherParam.tableCost;
                if (isNaN(gameParam.tableCost) || gameParam.tableCost > realInput.max || gameParam.tableCost < realInput.min) {
                    return { "code": 1 };
                }
                break;
            case paramType.baseCost:
                realInput = one;
                gameParam.baseCost = msg.otherParam.baseCost;
                if (isNaN(gameParam.baseCost) || gameParam.baseCost > realInput.max || gameParam.baseCost < realInput.min) {
                    return { "code": 1 };
                }
                break;
            case paramType.canRoomChat:
                realDropdown = one;
                gameParam.canRoomChat = realDropdown.realValues[msg.otherParam.canRoomChatIndex] ? true : false;
                break;
            case paramType.canInviteFriend:
                realDropdown = one;
                gameParam.canInviteFriend = realDropdown.realValues[msg.otherParam.canInviteFriendIndex] ? true : false;
                break;
            default:
                break;
        }
    }

    let gameInfo: I_gameInfo = {
        "id": 0,
        "roleId": 0,
        "roleName": "",
        "gameType": msg.gameType,
        "gameName": msg.gameName,
        "gameNotice": msg.gameNotice,
        "gameParam": gameParam,
        "createTime": timeFormat(new Date()),
        "startTime": msg.startTime,
        "endTime": msg.endTime,
        "closeTime": msg.closeTime,
        "rankNum": rankNum,
        "password": msg.password,
        "award": award,
        "state": gameState.create,
        "gmClosed": 0,
        "getDouzi": 0,
    };
    let cost: { [id: number]: number } = {};
    cost[ItemId.diamond] = Math.ceil(getHours(end - start) / createGameHourPer) * createGameDiamondPer;       // 消耗钻石数
    cost[ItemId.douzi] = getCostDouzi(award, rankNum);      // 消耗金币数
    return { "code": 0, "cost": cost, "info": gameInfo };
}


/**
 * 获取奖励的豆子数
 */
function getCostDouzi(award: { "rank": number, "num": number }[], rankNum: number) {
    let rankStart = 1;
    let cost: number = 0;
    for (let i = 0; i < award.length; i++) {
        if (award[i].num === 0) {
            rankStart = award[i].rank + 1;
            continue;
        }
        let isOver = false;
        for (let rank = rankStart; rank <= award[i].rank; rank++) {
            if (rank > rankNum) {
                isOver = true;
                break;
            }
            cost += award[i].num;
        }
        if (isOver) {
            break;
        }
        rankStart = award[i].rank + 1;
    }
    return cost;
}

function getHours(time: number) {
    return Math.floor(time / (60 * 60 * 1000));
}
function getDays(time: number) {
    return Math.floor(time / (24 * 60 * 60 * 1000));
}

/**
 * 设置session和信息服里的个人游戏状态
 * @param uid 
 * @param sid 
 * @param gameState 
 */
export function changeRoleGameState(uid: number, sid: string, gameState: I_playerGameState) {
    app.rpc(sid).connector.main.applyGameStateSession(uid, gameState);
    app.rpc(getInfoId(uid)).info.main.setGameState(uid, gameState);
}

/**
 * 从session里获取游戏状态
 * @param session 
 */
export function getGameState(session: Session): I_playerGameState {
    return session.get<I_playerGameState>("gameState");
}

let infoServerLen = 1;

/**
 * 获取玩家信息server id
 * @param uid 
 */
export function getInfoId(uid: number) {
    return app.getServersByType(serverType.info)[uid % infoServerLen].id;
}

export function table_rank(gameId: number) {
    return "rank:" + gameId;
}