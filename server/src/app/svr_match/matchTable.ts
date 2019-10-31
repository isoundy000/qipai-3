import { Application, Session } from "mydog";
import { MatchService } from "./matchService";
import { I_playerGameState, emptyGamestate } from "../svr_gameMain/gameMainMgr";
import { errCode } from "../../config/errCode";
import { gameLog } from "../logger";
import { cmd } from "../../config/cmd";
import { I_uidsid } from "../util/someInterface";
import * as gameUtil from "../util/gameUtil";

export interface I_matchPlayer {
    uid: number;
    sid: string;
    nickname: string;
    sex: number;
    headId: number;
    friendUid: number;  // 邀请者uid
    tableId: number;    // 同一个tableId表示，游戏结束后，将返回到同一个匹配桌子中
    isMaster: boolean;
    isReady: boolean;
    douzi: number;
    score: number;
    scoreA: number;
}

export class MatchTable {
    private app: Application;
    private matchService: MatchService;
    public tableId: number;                // 桌子id
    private isMatching: boolean = false;            // 是否在匹配中
    public players: I_matchPlayer[] = [];  // 所有的玩家
    public playerNum: number = 0;           // 玩家数量
    constructor(app: Application, matchService: MatchService, tableId: number) {
        this.app = app;
        this.matchService = matchService;
        this.tableId = tableId;
    }

    /**
     * 初始化玩家
     */
    initPlayers(players: I_matchPlayer[]) {
        this.players = players;
        this.playerNum = this.players.length;
        let obj: I_playerGameState = {
            "gameId": this.matchService.gameInfo.id,
            "gameSvr": this.app.serverId,
            "tableId": this.tableId,
            "chairId": 0
        }

        for (let one of players) {
            one.tableId = this.tableId;
            gameUtil.changeRoleGameState(one.uid, one.sid, obj);
        }
        let enterWaitTableData = {
            "gameType": this.matchService.gameInfo.gameType,
            "canInviteFriend": !!this.matchService.gameInfo.gameParam.canInviteFriend,
            "gameId": this.matchService.gameInfo.id,
            "tableId": this.tableId,
            "players": this.players
        };
        this.sendMsgToAll(cmd.onEnterMatchTable, enterWaitTableData);
    }



    /**
     * 发送消息
     */
    private sendMsgToAll(route: string, msg: any) {
        let uidsid: I_uidsid[] = [];
        for (let one of this.players) {
            if (one.sid) {
                uidsid.push({ "uid": one.uid, "sid": one.sid });
            }
        }
        this.app.sendMsgByUidSid(route, msg, uidsid);
    }

    private getPlayer(uid: number) {
        let tmp: I_matchPlayer = null as any;
        for (let one of this.players) {
            if (one.uid === uid) {
                tmp = one;
                break;
            }
        }
        return tmp;
    }

    private removePlayer(uid: number) {
        for (let i = this.players.length - 1; i >= 0; i--) {
            if (this.players[i].uid === uid) {
                this.players.splice(i, 1);
                this.playerNum--;
                break;
            }
        }
    }

    /**
     * 掉线
     */
    offline(uid: number) {
        let player = this.getPlayer(uid);
        if (!player) {
            return;
        }
        this.removePlayer(uid);
        this.onOneLeave(uid, player.isMaster);
    }


    /**
     * 离开桌子
     */
    leaveTable(uid: number) {
        let player = this.getPlayer(uid);
        if (!player) {
            return;
        }

        let obj: I_playerGameState = {
            "gameId": 0,
            "gameSvr": "",
            "tableId": 0,
            "chairId": 0
        };
        gameUtil.changeRoleGameState(uid, player.sid, obj);
        this.removePlayer(uid);
        this.onOneLeave(uid, player.isMaster);
    }

    /**
     * 某玩家离开
     * @param uid 
     * @param isMaster 
     */
    private onOneLeave(uid: number, isMaster: boolean) {
        if (this.playerNum === 0) {
            if (this.isMatching) {
                this.matchService.removeFromMatching(this.tableId);
            }
            this.matchService.closeTable(this.tableId);
            return;
        }

        if (this.isMatching) {
            this.matchService.removeFromMatching(this.tableId);
            this.isMatching = false;
        }

        let nowMasterUid = 0;
        if (isMaster) {
            for (let one of this.players) {
                one.isMaster = true;
                one.isReady = true;
                nowMasterUid = one.uid;
                break;
            }
        }
        for (let one of this.players) {
            if (!one.isMaster) {
                one.isReady = false;
            }
        }
        this.sendMsgToAll(cmd.onOneLeaveMatchTable, { "uid": uid, "nowMaster": nowMasterUid });
    }

    /**
     * 准备或取消准备
     */
    readyOrNot(uid: number) {
        let player = this.getPlayer(uid);
        if (!player || player.isMaster || this.isMatching) {
            return;
        }
        player.isReady = !player.isReady;
        this.sendMsgToAll(cmd.onReadyOrNot, { "uid": uid, "isReady": player.isReady });
    }

    /**
     * 开始匹配
     */
    startMatch(uid: number) {
        let player = this.getPlayer(uid);
        if (!player || !player.isMaster || this.isMatching) {
            return;
        }
        for (let one of this.players) {
            if (!one.isReady) {
                return;
            }
        }
        this.isMatching = true;
        this.matchService.pushToMatching(this);
        this.sendMsgToAll(cmd.onStartMatch, null);
    }

    /**
     * 取消匹配
     * @param uid 
     */
    cancelMatch(uid: number) {
        let player = this.getPlayer(uid);
        if (!player || !player.isMaster || !this.isMatching) {
            return;
        }
        this.matchService.removeFromMatching(this.tableId);
        this.isMatching = false;
        this.sendMsgToAll(cmd.onCancelMatch, null);
    }

    /**
     * 聊天
     */
    chat(session: Session, msg: string) {
        let player = this.getPlayer(session.uid);
        if (!player) {
            return;
        }
        this.sendMsgToAll(cmd.onChatInMatchTable, { "nickname": player.nickname, "data": msg });
    }


    /**
     * 踢人
     * @param uid 
     * @param kickUid 
     */
    kickPlayer(uid: number, kickUid: number) {
        let player = this.getPlayer(uid);
        if (this.isMatching || !player || !player.isMaster || uid === kickUid) {
            return;
        }
        let kickPlayer = this.getPlayer(kickUid);
        if (!kickPlayer) {
            return;
        }

        this.sendMsgToAll(cmd.onOneLeaveMatchTable, { "uid": kickUid, "nowMaster": 0 });

        this.removePlayer(kickUid);
        let obj: I_playerGameState = {
            "gameId": 0,
            "gameSvr": "",
            "tableId": 0,
            "chairId": 0
        };
        gameUtil.changeRoleGameState(kickPlayer.uid, kickPlayer.sid, obj);
    }

    /**
     * 邀请好友
     */
    inviteFriend(uid: number, msg: { "uid": number, "sid": string }) {
        console.log(uid);
        if (!this.matchService.gameInfo.gameParam.canInviteFriend) {
            console.log(111, "no");
            return;
        }
        let player = this.getPlayer(uid);
        if (!player) {
            return;
        }
        let data = {
            "uid": uid,
            "gameId": this.matchService.gameInfo.id,
            "gameType": this.matchService.gameInfo.gameType,
            "matchSvr": this.matchService.gameInfo.matchServer,
            "rankSvr": this.matchService.gameInfo.rankServer,
            "tableId": this.tableId,
            "nickname": player.nickname
        }
        console.log("send ok")
        this.app.sendMsgByUidSid(cmd.onInvite, data, [msg]);
    }

    /**
     * 接收好友邀请，进入桌子
     */
    enterFriendTable(one: I_matchPlayer): number {
        if (this.isMatching) {
            return errCode.enterFriendTable.isMatching;
        }
        if (this.playerNum >= this.matchService.roleNum) {
            return errCode.enterFriendTable.tableFull;
        }
        if (!this.getPlayer(one.friendUid)) {
            return errCode.enterFriendTable.friendNotInTable;
        }
        if (this.getPlayer(one.uid)) {
            return errCode.enterFriendTable.inTable;
        }
        this.playerNum++;

        this.sendMsgToAll(cmd.onNewPlayer, one);

        this.players.push(one);
        let obj: I_playerGameState = {
            "gameId": this.matchService.gameInfo.id,
            "gameSvr": this.app.serverId,
            "tableId": this.tableId,
            "chairId": 0
        };
        gameUtil.changeRoleGameState(one.uid, one.sid, obj);


        let enterWaitTableData = {
            "gameType": this.matchService.gameInfo.gameType,
            "canInviteFriend": !!this.matchService.gameInfo.gameParam.canInviteFriend,
            "gameId": this.matchService.gameInfo.id,
            "tableId": this.tableId,
            "players": this.players

        };
        this.app.sendMsgByUidSid(cmd.onEnterMatchTable, enterWaitTableData, [{ "uid": one.uid, "sid": one.sid }]);
        return 0;
    }

    /**
     * 同步豆子
     */
    syncDouziToTable(uid: number, douzi: number) {
        let player = this.getPlayer(uid);
        if (!player) {
            return;
        }
        player.douzi = douzi;
    }

    /**
     * 关闭
     */
    close() {
        for (let one of this.players) {
            gameUtil.changeRoleGameState(one.uid, one.sid, emptyGamestate);
        }
        this.sendMsgToAll(cmd.onMatchClose, null);
    }
}