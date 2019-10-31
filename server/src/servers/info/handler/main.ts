import { Application, Session } from "mydog";
import { RoleInfoMgr } from "../../../app/svr_info/roleInfoMgr";
import { RoleInfo } from "../../../app/svr_info/roleInfo";
import { I_friendInfoChange, DicObj } from "../../../app/util/someInterface";
import singleton_info from "../../../app/svr_info/singleton_info";
import { I_notice } from "../../../config/gameConfig";
import { ItemId } from "../../../config/item";
import { sendRollNoticeCost } from "../../../config/gameParamConfig";

export default class Handler {
    private roleInfoMgr: RoleInfoMgr;
    private app: Application;
    constructor(app: Application) {
        this.app = app;
        this.roleInfoMgr = singleton_info.roleInfoMgr;
    }

    /**
     * 签到领奖
     */
    sign(msg: any, session: Session, next: Function) {
        let role: RoleInfo = this.roleInfoMgr.getRole(session.uid);
        if (role.role.ifGetAward === 1) {
            return next({ "code": 2 });
        }
        role.changeRoleInfo({ "ifGetAward": 1 });
        next({ "code": 0 });
    }

    /**
     * 修改个人信息
     */
    changeMyInfo(msg: I_changeInfo, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);

        let changeObj: I_changeInfo = {} as any;
        let friendTell: I_friendInfoChange = { "uid": session.uid, "sid": role.role.sid };
        if (msg.headId) {
            changeObj.headId = msg.headId;
        }
        if (msg.sex) {
            changeObj.sex = msg.sex;
            friendTell.sex = msg.sex;
        }
        if (msg.nickname) {
            changeObj.nickname = msg.nickname;
            friendTell.nickname = msg.nickname;
        }
        if (msg.signature) {
            changeObj.signature = msg.signature;
            friendTell.signature = msg.signature;
        }
        role.changeRoleInfo(changeObj);


        role.friend.changeInfo(friendTell);

        (changeObj as any)["code"] = 0;
        next(changeObj);
    }

    /**
     * 发送全服滚动公告
     */
    sendRollNotice(msg: I_notice, session: Session, next: Function) {
        if (msg.info.length > 50) {
            return;
        }
        if (msg.count !== 1 && msg.count !== 2 && msg.count !== 3) {
            return;
        }
        let role = this.roleInfoMgr.getRole(session.uid);
        let cost: DicObj<number> = {};
        cost[ItemId.diamond] = sendRollNoticeCost[msg.count - 1];
        let ok = role.costItems(cost, true);
        if (ok) {
            msg.info = role.role.nickname + "：" + msg.info;
            this.app.rpc("*").connector.main.sendRollNotice(msg);
        }
    }

    // 临时加豆子
    addDouzi(msg: any, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        let items: DicObj<number> = {};
        items[ItemId.douzi] = 500;
        role.addItems(items, true);
    }

    // 临时加钻石
    addDiamond(msg: any, session: Session, next: Function) {
        let role = this.roleInfoMgr.getRole(session.uid);
        let items: DicObj<number> = {};
        items[ItemId.diamond] = 500;
        role.addItems(items, true);
    }
}


interface I_changeInfo {
    "headId"?: number,
    "sex"?: number,
    "nickname"?: string,
    "signature"?: string
}