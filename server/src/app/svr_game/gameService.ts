import { Application } from "mydog";
import { gameType, I_gameParam } from "../../config/gameConfig";
import { I_matchPlayer } from "../svr_match/matchTable";
import { I_tableBase } from "./gameMgr";
import { Table_WuZiQi } from "./wuZiQi/table_wuZiQi";
import { gameLog } from "../logger";
import * as util from "../util/util";
import singleton_game from "./singleton_game";
import { Table_XiangQi } from "./xiangQi/table_xiangQi";

/**
 * 游戏服务
 */
export class GameService {
    public app: Application;
    public gameId: number;        // 游戏id
    public gameType: gameType;    // 游戏类型
    public matchServer: string;   // 匹配服
    public rankServer: string;   // 排行服
    public gameParam: I_gameParam;     // 游戏自定义参数
    public isClosed: boolean = false;   // 比赛是否关闭
    private idNow: number = 1;
    private tables: { [id: number]: I_tableBase } = {};
    private lastUpdateTime: number = Date.now();
    constructor(app: Application, gameId: number, gameType: gameType, matchServer: string, rankServer: string, gameParam: I_gameParam) {
        this.app = app;
        this.gameId = gameId;
        this.gameType = gameType;
        this.matchServer = matchServer;
        this.rankServer = rankServer;
        this.gameParam = gameParam;
        setInterval(this.update.bind(this), 100);
    }

    /**
     * 比赛关闭
     */
    closeGame() {
        this.isClosed = true;
    }

    update() {
        let now = Date.now();
        let delta = now - this.lastUpdateTime;
        this.lastUpdateTime = now;
        for (let x in this.tables) {
            this.tables[x].update(delta);
        }
    }

    /**
     * 创建游戏桌子
     * @param players 
     */
    createTable(players: I_matchPlayer[]) {
        let table: I_tableBase = null as any;
        switch (this.gameType) {
            case gameType.wuZiQi:
                table = new Table_WuZiQi(this, this.idNow);
                break;
            case gameType.xiangQi:
                table = new Table_XiangQi(this, this.idNow);
                break;
            default:
                gameLog.error("no such kind of table constructor");
                break;
        }
        if (!table) {
            return;
        }
        this.tables[this.idNow] = table;
        this.idNow++;
        table.initPlayers(players);
    }

    /**
     * 获取桌子
     */
    getTable(tableId: number) {
        return this.tables[tableId];
    }

    /**
     * 销毁桌子
     */
    closeTable(tableId: number) {
        delete this.tables[tableId];
        if (this.isClosed && util.isEmptyObj(this.tables)) {
            singleton_game.gameMgr.delGame(this.gameId);
        }
    }
}