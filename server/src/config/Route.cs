public class Route
{
    /// <summary>
    /// 获取一些配置
    /// </summary>
    public const string connector_main_getSomeConfig = "connector.main.getSomeConfig";
    /// <summary>
    /// 登录游戏
    /// </summary>
    public const string connector_main_enter = "connector.main.enter";
    /// <summary>
    /// 重连
    /// </summary>
    public const string connector_main_reconnectEnter = "connector.main.reconnectEnter";
    /// <summary>
    /// 获取游戏列表
    /// </summary>
    public const string connector_main_getGameList = "connector.main.getGameList";
    /// <summary>
    /// 获取最近游玩的游戏
    /// </summary>
    public const string connector_main_getRecentPlay = "connector.main.getRecentPlay";
    /// <summary>
    /// 搜索游戏
    /// </summary>
    public const string connector_main_searchGame = "connector.main.searchGame";
    /// <summary>
    /// 签到
    /// </summary>
    public const string info_main_sign = "info.main.sign";
    /// <summary>
    /// 修改个人信息
    /// </summary>
    public const string info_main_changeMyInfo = "info.main.changeMyInfo";
    /// <summary>
    /// 发送全服滚动公告
    /// </summary>
    public const string info_main_sendRollNotice = "info.main.sendRollNotice";
    /// <summary>
    /// 临时加金币
    /// </summary>
    public const string info_main_addDouzi = "info.main.addDouzi";
    /// <summary>
    /// 临时加钻石
    /// </summary>
    public const string info_main_addDiamond = "info.main.addDiamond";
    /// <summary>
    /// 背包发生变化
    /// </summary>
    public const string onBagChanged = "onBagChanged";
    /// <summary>
    /// 滚动播报
    /// </summary>
    public const string onNotice = "onNotice";
    /// <summary>
    /// 游戏统计数据（总场次，胜场次）
    /// </summary>
    public const string onGameWinData = "onGameWinData";
    /// <summary>
    /// 玩家桌子里豆子数改变
    /// </summary>
    public const string onTableDouziSync = "onTableDouziSync";
    /// <summary>
    /// 阅读邮件
    /// </summary>
    public const string info_mail_readMail = "info.mail.readMail";
    /// <summary>
    /// 邮件领奖
    /// </summary>
    public const string info_mail_getMailAward = "info.mail.getMailAward";
    /// <summary>
    /// 删除邮件
    /// </summary>
    public const string info_mail_delMail = "info.mail.delMail";
    /// <summary>
    /// 给好友发送邮件
    /// </summary>
    public const string info_mail_sendMail = "info.mail.sendMail";
    /// <summary>
    /// 通知，有新的邮件
    /// </summary>
    public const string onNewMail = "onNewMail";
    /// <summary>
    /// 请求添加好友
    /// </summary>
    public const string info_friend_askFriend = "info.friend.askFriend";
    /// <summary>
    /// 同意添加好友
    /// </summary>
    public const string info_friend_agreeFriend = "info.friend.agreeFriend";
    /// <summary>
    /// 删除好友
    /// </summary>
    public const string info_friend_delFriend = "info.friend.delFriend";
    /// <summary>
    /// 好友聊天
    /// </summary>
    public const string info_friend_chat = "info.friend.chat";
    /// <summary>
    /// 通知，好友请求
    /// </summary>
    public const string onAskFriend = "onAskFriend";
    /// <summary>
    /// 通知，新增好友
    /// </summary>
    public const string onAddFriend = "onAddFriend";
    /// <summary>
    /// 通知，删除好友
    /// </summary>
    public const string onDelFriend = "onDelFriend";
    /// <summary>
    /// 通知，好友聊天
    /// </summary>
    public const string onFriendChat = "onFriendChat";
    /// <summary>
    /// 通知，好友信息改变
    /// </summary>
    public const string onFriendInfoChange = "onFriendInfoChange";
    /// <summary>
    /// 创建游戏比赛
    /// </summary>
    public const string info_game_createGameMatch = "info.game.createGameMatch";
    /// <summary>
    /// 创建匹配桌子
    /// </summary>
    public const string info_game_createMatchTable = "info.game.createMatchTable";
    /// <summary>
    /// 接受好友邀请进入匹配桌子
    /// </summary>
    public const string info_game_enterMatchTable = "info.game.enterMatchTable";
    /// <summary>
    /// 进入游戏桌子
    /// </summary>
    public const string info_game_enterTable = "info.game.enterTable";
    /// <summary>
    /// 获取排行榜
    /// </summary>
    public const string info_game_getRankList = "info.game.getRankList";
    /// <summary>
    /// 刷新个人最新战绩
    /// </summary>
    public const string info_game_refreshMyRank = "info.game.refreshMyRank";
    /// <summary>
    /// 通知，被踢
    /// </summary>
    public const string onKicked = "onKicked";
    /// <summary>
    /// 通知，比赛类型状态改变
    /// </summary>
    public const string onUpdateGameTypeState = "onUpdateGameTypeState";
    /// <summary>
    /// 准备或取消准备
    /// </summary>
    public const string match_main_readyOrNot = "match.main.readyOrNot";
    /// <summary>
    /// 匹配桌子里踢人
    /// </summary>
    public const string match_main_kickPlayer = "match.main.kickPlayer";
    /// <summary>
    /// 开始匹配
    /// </summary>
    public const string match_main_startMatch = "match.main.startMatch";
    /// <summary>
    /// 取消匹配
    /// </summary>
    public const string match_main_cancelMatch = "match.main.cancelMatch";
    /// <summary>
    /// 离开匹配桌子
    /// </summary>
    public const string match_main_leaveTable = "match.main.leaveTable";
    /// <summary>
    /// 桌子聊天
    /// </summary>
    public const string match_main_chat = "match.main.chat";
    /// <summary>
    /// 邀请好友
    /// </summary>
    public const string match_main_inviteFriend = "match.main.inviteFriend";
    /// <summary>
    /// 进入匹配桌子了
    /// </summary>
    public const string onEnterMatchTable = "onEnterMatchTable";
    /// <summary>
    /// 通知，准备或取消准备
    /// </summary>
    public const string onReadyOrNot = "onReadyOrNot";
    /// <summary>
    /// 通知，有人离开
    /// </summary>
    public const string onOneLeaveMatchTable = "onOneLeaveMatchTable";
    /// <summary>
    /// 通知，开始匹配
    /// </summary>
    public const string onStartMatch = "onStartMatch";
    /// <summary>
    /// 通知，取消匹配
    /// </summary>
    public const string onCancelMatch = "onCancelMatch";
    /// <summary>
    /// 通知，接收到邀请
    /// </summary>
    public const string onInvite = "onInvite";
    /// <summary>
    /// 通知，桌子里新来一个人
    /// </summary>
    public const string onNewPlayer = "onNewPlayer";
    /// <summary>
    /// 通知，桌子里聊天
    /// </summary>
    public const string onChatInMatchTable = "onChatInMatchTable";
    /// <summary>
    /// 匹配关闭
    /// </summary>
    public const string onMatchClose = "onMatchClose";
    /// <summary>
    /// 离开游戏桌子
    /// </summary>
    public const string game_main_leave = "game.main.leave";
    /// <summary>
    /// 聊天
    /// </summary>
    public const string game_main_chat = "game.main.chat";
    /// <summary>
    /// 快捷聊天
    /// </summary>
    public const string game_main_chatSeq = "game.main.chatSeq";
    public const string onChatInTable = "onChatInTable";
    public const string onChatSeqInTable = "onChatSeqInTable";
    public const string onGameOver = "onGameOver";
    public const string onEnterTable = "onEnterTable";
    /// <summary>
    /// 五子棋， 落子
    /// </summary>
    public const string game_wuZiQi_play = "game.wuZiQi.play";
    /// <summary>
    /// 出牌
    /// </summary>
    public const string onPlayCard = "onPlayCard";
    /// <summary>
    /// 步时结束，进入读秒
    /// </summary>
    public const string onStepTimeOver = "onStepTimeOver";
    /// <summary>
    /// 象棋， 落子
    /// </summary>
    public const string game_xiangQi_play = "game.xiangQi.play";
    /// <summary>
    /// 象棋， 请求平局
    /// </summary>
    public const string game_xiangQi_ping = "game.xiangQi.ping";
    /// <summary>
    /// 对方想要平局
    /// </summary>
    public const string onPingWant = "onPingWant";
    /// <summary>
    /// 象棋， 同意或拒绝平局
    /// </summary>
    public const string game_xiangQi_pingYesOrNo = "game.xiangQi.pingYesOrNo";
    /// <summary>
    /// 拒绝平局
    /// </summary>
    public const string onPingNo = "onPingNo";
}