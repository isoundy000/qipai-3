export const enum cmd {
    /**
     * 背包发生变化
     */
    onBagChanged = "onBagChanged",
    /**
     * 滚动播报
     */
    onNotice = "onNotice",
    /**
     * 游戏统计数据（总场次，胜场次）
     */
    onGameWinData = "onGameWinData",
    /**
     * 玩家桌子里豆子数改变
     */
    onTableDouziSync = "onTableDouziSync",
    /**
     * 通知，有新的邮件
     */
    onNewMail = "onNewMail",
    /**
     * 通知，好友请求
     */
    onAskFriend = "onAskFriend",
    /**
     * 通知，新增好友
     */
    onAddFriend = "onAddFriend",
    /**
     * 通知，删除好友
     */
    onDelFriend = "onDelFriend",
    /**
     * 通知，好友聊天
     */
    onFriendChat = "onFriendChat",
    /**
     * 通知，好友信息改变
     */
    onFriendInfoChange = "onFriendInfoChange",
    /**
     * 通知，被踢
     */
    onKicked = "onKicked",
    /**
     * 通知，比赛类型状态改变
     */
    onUpdateGameTypeState = "onUpdateGameTypeState",
    /**
     * 进入匹配桌子了
     */
    onEnterMatchTable = "onEnterMatchTable",
    /**
     * 通知，准备或取消准备
     */
    onReadyOrNot = "onReadyOrNot",
    /**
     * 通知，有人离开
     */
    onOneLeaveMatchTable = "onOneLeaveMatchTable",
    /**
     * 通知，开始匹配
     */
    onStartMatch = "onStartMatch",
    /**
     * 通知，取消匹配
     */
    onCancelMatch = "onCancelMatch",
    /**
     * 通知，接收到邀请
     */
    onInvite = "onInvite",
    /**
     * 通知，桌子里新来一个人
     */
    onNewPlayer = "onNewPlayer",
    /**
     * 通知，桌子里聊天
     */
    onChatInMatchTable = "onChatInMatchTable",
    /**
     * 匹配关闭
     */
    onMatchClose = "onMatchClose",
    onChatInTable = "onChatInTable",
    onChatSeqInTable = "onChatSeqInTable",
    onGameOver = "onGameOver",
    onEnterTable = "onEnterTable",
    /**
     * 出牌
     */
    onPlayCard = "onPlayCard",
    /**
     * 步时结束，进入读秒
     */
    onStepTimeOver = "onStepTimeOver",
    /**
     * 对方想要平局
     */
    onPingWant = "onPingWant",
    /**
     * 拒绝平局
     */
    onPingNo = "onPingNo",
}