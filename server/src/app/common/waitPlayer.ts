export default class WaitPlayer {
    uid: number;
    sid: string;
    nickname: string;
    headId: number;
    friendUid: number;
    isMaster: boolean;
    isReady: boolean;
    constructor(info?: any) {
        info = info || {};
        this.uid = info.uid;
        this.sid = info.sid;
        this.nickname = info.nickname;
        this.headId = info.headId;
        this.friendUid = info.friendUid;
        this.isMaster = info.isMaster;
        this.isReady = info.isReady;
    }
}