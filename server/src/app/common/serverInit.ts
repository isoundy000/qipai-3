import { Application } from "mydog";
import mysqlClient from "../dbService/mysql";
import * as dbConfig from "../../config/dbConfig";
import { redisPool } from "../dbService/redisPool";
import { GameMainMgr } from "../svr_gameMain/gameMainMgr";
import { MatchMgr } from "../svr_match/matchMgr";
import { RoleInfoMgr } from "../svr_info/roleInfoMgr";
import { GameMgr } from "../svr_game/gameMgr";
import { serverType } from "../../config/gameConfig";
import { LoginMgr } from "../svr_login/loginMgr";
import singleton_login from "../svr_login/singleton_login";
import singleton_connector from "../svr_connector/singleton_connector";
import { ConnectorMgr } from "../svr_connector/connectorMgr";
import singleton_info from "../svr_info/singleton_info";
import singleton_gameMain from "../svr_gameMain/singleton_gameMain";
import singleton_match from "../svr_match/singleton_match";
import singleton_game from "../svr_game/singleton_game";
import singleton_rank from "../svr_rank/singleton_rank";
import { RankMgr } from "../svr_rank/rankMgr";
import { GmService } from "../svr_login/gmService";




export default function initServer(app: Application) {
    switch (app.serverType) {
        case serverType.login:
            loginInit(app);
            break;
        case serverType.connector:
            connectorInit(app);
            break;
        case serverType.info:
            infoInit(app);
            break;
        case serverType.gameMain:
            gameMainInit(app);
            break;
        case serverType.match:
            matchInit(app);
            break;
        case serverType.rank:
            rankInit(app);
            break;
        case serverType.game:
            gameInit(app);
            break;
        default:
            break;
    }
}

// 登录服
function loginInit(app: Application) {
    singleton_login.mysql = new mysqlClient(dbConfig.mysqlConfig);
    singleton_login.loginMgr = new LoginMgr(app);
    singleton_login.gmService = new GmService(app);
}

// 网关服
function connectorInit(app: Application) {
    singleton_connector.connectorMgr = new ConnectorMgr();
}

// 信息服
function infoInit(app: Application) {
    singleton_info.mysql = new mysqlClient(dbConfig.mysqlConfig);
    singleton_info.roleInfoMgr = new RoleInfoMgr(app);
}


// 比赛控制服
function gameMainInit(app: Application) {
    singleton_gameMain.mysql = new mysqlClient(dbConfig.mysqlConfig);
    singleton_gameMain.gameMainMgr = new GameMainMgr(app);
}

// 匹配服
function matchInit(app: Application) {
    singleton_match.matchMgr = new MatchMgr(app);
}

// 排行服
function rankInit(app: Application) {
    singleton_rank.rankMgr = new RankMgr(app);
    singleton_rank.gameRedis = new redisPool(dbConfig.game_RedisConfig)
    singleton_rank.mysql = new mysqlClient(dbConfig.mysqlConfig);
}

// 游戏服
function gameInit(app: Application) {
    singleton_game.gameMgr = new GameMgr(app);
}

