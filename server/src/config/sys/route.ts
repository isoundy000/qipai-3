export default [
    "connector.main.getSomeConfig",        // 获取一些配置
    "connector.main.enter",                // 登录游戏
    "connector.main.reconnectEnter",       // 重连
    "connector.main.getGameList",       // 获取游戏列表
    "connector.main.getRecentPlay",       // 获取最近游玩的游戏
    "connector.main.searchGame",       // 搜索游戏

    "info.main.sign",               // 签到
    "info.main.changeMyInfo",       // 修改个人信息
    "info.main.sendRollNotice",     // 发送全服滚动公告
    "info.main.addDouzi",     // 临时加金币
    "info.main.addDiamond",     // 临时加钻石
    "onBagChanged",                 // 背包发生变化
    "onNotice",                     // 滚动播报
    "onGameWinData",                // 游戏统计数据（总场次，胜场次）
    "onTableDouziSync",           // 玩家桌子里豆子数改变

    "info.mail.readMail",           // 阅读邮件
    "info.mail.getMailAward",       // 邮件领奖
    "info.mail.delMail",            // 删除邮件
    "info.mail.sendMail",           // 给好友发送邮件
    "onNewMail",                    // 通知，有新的邮件

    "info.friend.askFriend",        // 请求添加好友
    "info.friend.agreeFriend",      // 同意添加好友
    "info.friend.delFriend",        // 删除好友
    "info.friend.chat",             // 好友聊天
    "onAskFriend",                  // 通知，好友请求
    "onAddFriend",                  // 通知，新增好友
    "onDelFriend",                  // 通知，删除好友
    "onFriendChat",                 // 通知，好友聊天
    "onFriendInfoChange",           // 通知，好友信息改变

    "info.game.createGameMatch",    // 创建游戏比赛
    "info.game.createMatchTable",   // 创建匹配桌子
    "info.game.enterMatchTable",    // 接受好友邀请进入匹配桌子
    "info.game.enterTable",         // 进入游戏桌子
    "info.game.getRankList",        // 获取排行榜
    "info.game.refreshMyRank",      // 刷新个人最新战绩

    "onKicked",                     // 通知，被踢


    "onUpdateGameTypeState",        // 通知，比赛类型状态改变

    "match.main.readyOrNot",        // 准备或取消准备
    "match.main.kickPlayer",        // 匹配桌子里踢人
    "match.main.startMatch",        // 开始匹配
    "match.main.cancelMatch",       // 取消匹配
    "match.main.leaveTable",        // 离开匹配桌子
    "match.main.chat",              // 桌子聊天
    "match.main.inviteFriend",      // 邀请好友
    "onEnterMatchTable",            // 进入匹配桌子了
    "onReadyOrNot",                 // 通知，准备或取消准备
    "onOneLeaveMatchTable",         // 通知，有人离开
    "onStartMatch",                 // 通知，开始匹配
    "onCancelMatch",                // 通知，取消匹配
    "onInvite",                     // 通知，接收到邀请
    "onNewPlayer",                  // 通知，桌子里新来一个人
    "onChatInMatchTable",           // 通知，桌子里聊天
    "onMatchClose",                 // 匹配关闭

    "game.main.leave",              // 离开游戏桌子
    "game.main.chat",               // 聊天
    "game.main.chatSeq",             // 快捷聊天
    "onChatInTable",
    "onChatSeqInTable",
    "onGameOver",
    "onEnterTable",

    "game.wuZiQi.play",             // 五子棋， 落子
    "onPlayCard",                   // 出牌
    "onStepTimeOver",               // 步时结束，进入读秒

    "game.xiangQi.play",             // 象棋， 落子
    "game.xiangQi.ping",             // 象棋， 请求平局
    "onPingWant",                    // 对方想要平局
    "game.xiangQi.pingYesOrNo",      // 象棋， 同意或拒绝平局
    "onPingNo",                      // 拒绝平局

]