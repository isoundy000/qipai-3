import { Application } from "mydog";
import { I_gameInfo, I_gameInfoClient } from "../svr_gameMain/gameMainMgr";
import { serverType } from "../../config/gameConfig";
import { MatchTable, I_matchPlayer } from "./matchTable";
import { gameTypes } from "../../config/gameParamConfig";
import { ItemId } from "../../config/item";
import { getInfoId } from "../util/gameUtil";

/**
 * 匹配服务
 */
export class MatchService {
    private app: Application;
    public gameInfo: I_gameInfoClient;
    public roleNum: number = 0;    // 一桌多少人
    private tableIdNow: number = 1; // 当前桌子id序号
    private tables: { [id: number]: MatchTable } = {};
    private tablesMatching: MatchTable[] = [];
    private matchTimer: NodeJS.Timer;
    constructor(app: Application, gameInfo: I_gameInfoClient) {
        this.app = app;
        this.gameInfo = gameInfo;

        for (let one of gameTypes) {
            if (one.type === this.gameInfo.gameType) {
                this.roleNum = one.roleNum;
                break;
            }
        }

        this.matchTimer = setInterval(this.matchInterval.bind(this), 3000);
    }

    /**
     * 关闭匹配服务
     */
    closeMatch() {
        clearInterval(this.matchTimer);
        for (let x in this.tables) {
            this.tables[x].close();
        }
    }

    /**
     * 匹配
     */
    private matchInterval() {
        let tablesMatching = this.tablesMatching;
        for (let i = 0; i < tablesMatching.length; i++) {
            let nowNum = 0;
            let tableIndex = [];
            let tmpTables = [];
            for (let j = i; j < tablesMatching.length; j++) {
                let one = tablesMatching[j];
                if (one.playerNum + nowNum > this.roleNum) {
                    continue;
                }
                nowNum += one.playerNum;
                tableIndex.push(j);
                tmpTables.push(one);
                if (nowNum < this.roleNum) {
                    continue;
                }
                i--;
                let tmpPlayers: I_matchPlayer[] = [];
                for (let k = tableIndex.length - 1; k >= 0; k--) {
                    delete this.tables[tmpTables[k].tableId];
                    tablesMatching.splice(tableIndex[k], 1);
                    for (let m in tmpTables[k].players) {
                        tmpPlayers.push(tmpTables[k].players[m]);
                    }
                }
                this.createGameTable(tmpPlayers);
                break;
            }
        }
    }

    /**
     * 匹配成功，创建游戏桌子
     */
    createGameTable(players: I_matchPlayer[]) {
        console.error("匹配成功", players);

        // 匹配成功，扣除台费，加给举办者
        let tableCost = this.gameInfo.gameParam.tableCost;
        if (tableCost > 0) {
            let cost: { [id: string]: number } = {};
            cost[ItemId.douzi] = tableCost;
            for (let one of players) {
                one.douzi -= tableCost;
                this.app.rpc(getInfoId(one.uid)).info.main.costItems(one.uid, cost, false);
            }
            this.app.rpc(this.gameInfo.rankServer).rank.main.addTableCost(this.gameInfo.id, this.roleNum * tableCost);
        }

        let gameSvrs = this.app.getServersByType(serverType.game);
        let index = Math.floor(Math.random() * gameSvrs.length);
        this.app.rpc(gameSvrs[index].id).game.main.createTable(this.gameInfo.id, this.gameInfo.gameType, this.gameInfo.matchServer, this.gameInfo.rankServer, this.gameInfo.gameParam, players);
    }

    /**
     * 创建匹配桌子
     * @param players 
     */
    createMatchTable(players: I_matchPlayer[]) {
        this.tableIdNow++;
        let tableId = this.tableIdNow;
        let matchTable = new MatchTable(this.app, this, tableId);
        this.tables[tableId] = matchTable;
        matchTable.initPlayers(players)
    }

    /**
     * 销毁桌子 ----由matchTable调用
     */
    closeTable(tableId: number) {
        delete this.tables[tableId];
        console.error("销毁桌子", tableId);
    }

    /**
     * 将桌子从匹配队列中移出 ----由matchTable调用
     */
    removeFromMatching(tableId: number) {
        for (let i = 0; i < this.tablesMatching.length; i++) {
            if (this.tablesMatching[i].tableId === tableId) {
                this.tablesMatching.splice(i, 1);
                break;
            }
        }
        console.error("匹配队列桌子数：", this.tablesMatching.length);
    }

    /**
     * 将桌子追加到匹配队列末尾 ----由matchTable调用
     */
    pushToMatching(table: MatchTable) {
        this.tablesMatching.push(table);
    }

    /**
     * 获取桌子
     */
    getTable(tableId: number) {
        return this.tables[tableId];
    }

}