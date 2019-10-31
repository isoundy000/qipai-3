using System;
using System.Collections.Generic;

namespace Proto
{
    /// <summary>
    /// 游戏服登录
    /// </summary>
    [Serializable]
    public class login_req
    {
        public int uid = 0;
        public int token = 0;
    }

    /// <summary>
    /// http服务器注册和登录返回
    /// </summary>
    [Serializable]
    public class httpRegOrLoginRsp
    {
        public int code = 0;
        public int uid = 0;
        public string host = "";
        public int port = 0;
        public int token = 0;
    }

    /// <summary>
    /// 修改个人信息
    /// </summary>
    [Serializable]
    public class changeMyInfo
    {
        public int code = 0;
        public int headId = 0;
        public int sex = 0;
        public string nickname = "";
        public string signature = "";
    }

    /// <summary>
    /// 登录返回
    /// </summary>
    [Serializable]
    public class EnterServer
    {
        public int code = 0;
        public RoleAllInfo info;
    }

    /// <summary>
    /// 一些配置
    /// </summary>
    [Serializable]
    public class SomeConfig
    {
        public int refreshMyRankCost;
        public int createGameHourPer;
        public int createGameDiamondPer;
        public int douziGetPercent;
        public string notice = "";
        public List<gameTypeInfo> gameTypes = new List<gameTypeInfo>();
        public GameTimeDiff gameTimeDiff;
        public List<GameParamName> gameParamName = new List<GameParamName>();
        public List<Notice> rollNotices = new List<Notice>();
        public List<int> sendRollNoticeCost = new List<int>();
    }

    [Serializable]
    public class GameParamName
    {
        public paramType type;
        public string name;
    }

    [Serializable]
    public class RoleAllInfo
    {
        public PlayerData role;
        public List<ItemData> bag;
        public List<FriendInfo> friends;
        public List<MailData> mails;
    }

    [Serializable]
    public class GameTimeDiff
    {
        public int create_start_minH = 0;
        public int create_start_maxD = 0;
        public int start_end_minH = 0;
        public int start_end_maxD = 0;
        public int end_close_minH = 0;
        public int end_close_maxD = 0;
    }

    // [Serializable]
    // public class

    [Serializable]
    public class PlayerData
    {
        public string nickname = "";     // 昵称
        public int sex = 0;             // 性别 1男2女
        public int headId = 0;          // 头像
        public string signature = "";    // 签名
        public int loginDays = 0;       // 登录天数
        public int ifGetAward = 0;      // 今天是否登录领奖
        public List<gameTypeScoreData> gameData = new List<gameTypeScoreData>();    // 游戏场次和分数信息
        public int gameId = 0;
        public string gameSvr = "";
        public int tableId = 0;
        public int chairId = 0;
    }

    [Serializable]
    public class gameTypeScoreData
    {
        public int type;   // 游戏类型
        public int all;      // 总场次
        public int win;      // 胜场次
        public int score;   // 分数
    }

    /// <summary>
    /// 游戏类型信息
    /// </summary>
    [Serializable]
    public class gameTypeInfo
    {
        public int type = 0;
        public string name = "";
        public int roleNum = 0;
        public gameTypeState state = gameTypeState.normal;
        public string scene = "";
        public List<GameParam> param;
        public List<string> chatSeq;
    }

    [Serializable]
    public enum gameTypeState
    {
        normal = 1,     // 正常开放
        wait = 2,       // 待开放   （未开放。客户端可以显示tab。）
        closed = 3      // 关闭     （未开放。客户端不显示tab。）
    }

    /// <summary>
    /// 游戏时间段
    /// </summary>
    [Serializable]
    public enum gameState
    {
        wait = 1,        // 即将开启
        going = 2,       // 进行中
        end = 3,       // 已结束
        close = 4,      // 关闭
    }

    /// <summary>
    /// 游戏比赛基本信息
    /// </summary>
    [Serializable]
    public class gameInfo
    {
        public int id = 0;
        public int roleId = 0;
        public string roleName = "";
        public int gameType = 0;
        public string gameName = "";
        public string gameNotice = "";
        public string createTime = "";
        public string startTime = "";
        public string endTime = "";
        public string closeTime = "";
        public gameState state = gameState.wait;
        public GameOtherParam gameParam = new GameOtherParam();
        public int rankNum = 0;
        public string password = "";
        public List<GameAwardRank> award = new List<GameAwardRank>();
        public string matchServer = "";
        public string rankServer = "";
    }

    [Serializable]
    public class GameOtherParam
    {
        public int gameTime;
        public int stepTime;
        public int countTime;
        public int doorCost;
        public int tableCost;
        public int baseCost;
        public bool canRoomChat;
        public bool canInviteFriend;
    }



    [Serializable]
    public class ItemData
    {
        public int id = 0;
        public int num = 0;
    }
    /// <summary>
    /// 邮件数据
    /// </summary>
    [Serializable]
    public class MailData
    {
        public int id;
        public int uid;
        public string topic;
        public string content;
        public List<ItemData> items = new List<ItemData>();
        public int status;
        public string createTime;
        public string expireTime;
        public int sendUid;
        public string sendName;
    }

    /// <summary>
    /// 阅读邮件的回调
    /// </summary>
    [Serializable]
    public class ReadMailRsp
    {
        public int code = 0;
        public int id = 0;
    }

    /// <summary>
    /// 阅读/领奖/删除邮件
    /// </summary>
    [Serializable]
    public class MailReq
    {
        public int id = 0;
        public int uid = 0;
    }
    /// <summary>
    /// 向好友发送邮件
    /// </summary>
    [Serializable]
    public class SendMailReq
    {
        public int getUid;
        public string info;
        public List<ItemData> items = new List<ItemData>();
    }

    [Serializable]
    public class JustCode
    {
        public int code = 0;
    }


    [Serializable]
    public class FriendInfo
    {
        public int uid = 0;
        public string sid = "";
        public int sex = 0;
        public string nickname = "";
        public string signature = "";
    }

    /// <summary>
    /// 添加好友消息
    /// </summary>
    [Serializable]
    public class OnAskFriend
    {
        public int uid;
        public int sex;
        public string nickname;
    }

    [Serializable]
    public class Uidsid
    {
        public int uid = 0;
        public string sid = "";
    }

    [Serializable]
    public class FriendChatReq
    {
        public int uid;
        public string sid;
        public string data;
    }
    [Serializable]
    public class FriendChatMsg
    {
        public int from;
        public int to;
        public string info;
    }

    /// <summary>
    /// 获取某一类型的游戏列表
    /// </summary>
    [Serializable]
    public class GetGameListReq
    {
        public int gameType = 0;
        public gameState state;
        public int pageIndex;
    }

    /// <summary>
    /// 某一类型的游戏列表回调
    /// </summary>
    [Serializable]
    public class GetGamesRsp
    {
        public List<gameInfo> games = new List<gameInfo>();
    }

    /// <summary>
    /// 创建游戏
    /// </summary>
    [Serializable]
    public class CreateGameReq
    {
        public int gameType;
        public string gameName; //游戏名
        public string gameNotice;   // 公告
        public string startTime;
        public string endTime;
        public string closeTime;
        public int rankIndex;   // 排行人数序号
        public string password; // 密码
        public List<GameAwardRank> award;    // 奖励
        public CreateGameOtherParam otherParam = new CreateGameOtherParam();
    }
    [Serializable]
    public class CreateGameOtherParam
    {
        public int gameTimeIndex;   // 局时序号
        public int stepTimeIndex;   // 步时序号
        public int countTimeIndex;  // 读秒序号
        public int doorCost;        // 门票
        public int tableCost;       // 桌费
        public int baseCost;       // 输豆基数
        public int canRoomChatIndex;    // 是否允许房内聊天
        public int canInviteFriendIndex;    // 能否邀请好友一起玩
    }

    [Serializable]
    public class OnUpdateGameTypeState
    {
        public int gameType;
        public gameTypeState state;
        public gameTypeState lastState;
    }

    [Serializable]
    public class CreateWaitTableReq
    {
        public int gameId;
        public string rankSvr;
        public string matchSvr;
        public string pwd;
    }

    [Serializable]
    public class CreateWaitTableRsp
    {
        public int code;
        public int tableId;
    }


    [Serializable]
    public class OnOneLeaveMatchTable
    {
        public int uid;
        public int nowMaster;
    }

    [Serializable]
    public class OnReadyOrNotInMatchTable
    {
        public int uid;
        public bool isReady;
    }

    [Serializable]
    public class OnChatInMatchTable
    {
        public string nickname;
        public string data;
    }

    [Serializable]
    public class OnInvite
    {
        public int uid;
        public int gameId;
        public int gameType;
        public string matchSvr;
        public string rankSvr;
        public int tableId;
        public string nickname;
    }

    [Serializable]
    public class EnterFriendTable
    {
        public int gameId;
        public string matchSvr;
        public string rankSvr;
        public int tableId;
        public int friendUid;
        public string pwd;
    }

    [Serializable]
    public class WaitUserInfoProto
    {
        public int uid;
        public string sid;
        public string nickname;
        public int headId;
        public bool isMaster;
        public bool isReady;
        public int score;
    }

    [Serializable]
    public class OnEnterMatchTable
    {
        public int gameType;
        public int gameId;
        public int tableId;
        public bool canInviteFriend;
        public List<WaitUserInfoProto> players = new List<WaitUserInfoProto>();
    }

    [Serializable]
    public class EnterWuZiQi
    {
        public int gameId;
        public int tableId;
        public bool canChatInRoom;
        public int gameTime;
        public int stepTime;
        public int countTime;
        public int nowChairId;
        public List<QiZi> qiZi;
        public List<WuZiQiPlayer> players;
        public int lastI;
        public int lastJ;
    }
    [Serializable]
    public class QiZi
    {
        public List<int> qizi;
    }

    [Serializable]
    public class WuZiQiPlayer
    {
        public int uid;
        public string sid;
        public string nickname;
        public int headId;
        public int chairId;
        public int qiZiType;
        public int douzi;
        public int score;
        public int gameTime;
        public bool isStepTime;
        public int leftTime;
    }

    [Serializable]
    public class PosIJ
    {
        public int i;
        public int j;
    }
    [Serializable]
    public class WuZiQiLuoZi
    {
        public int i;
        public int j;
        public bool isOver;
    }

    [Serializable]
    public class OnEnterTable
    {
        public int gameId;
        public int gameType;
        public string data;
    }

    [Serializable]
    public class GameParam
    {
        public GameParamUIType uiType;
        public string data;
    }

    [Serializable]
    public enum GameParamUIType
    {
        dropdown,   // 下拉列表
        input,      // 输入框
    }

    // 参数类型
    public enum paramType
    {
        rankNum,    // 排行人数
        password,   // 密码
        gameTime,   // 局时
        stepTime,   // 步时
        countTime,  // 读秒
        doorCost,   // 门票
        tableCost,  // 桌费
        baseCost,  // 输豆基数
        canRoomChat,    // 是否允许房内聊天
        canInviteFriend,  // 能否邀请好友一起玩
    }

    [Serializable]
    public class GetRankListReq
    {
        public int gameId;
        public string rankSvr;
    }
    [Serializable]
    public class GetRankListRsp
    {
        public List<RoleRankInfo> ranklist;
    }
    [Serializable]
    public class RefreshMyRankRsp
    {
        public int meRank = 0;
        public int meScore = 0;
    }
    [Serializable]
    public class RoleRankInfo
    {
        public int uid;
        public string name;
        public int sex;
        public int score;
    }

    [Serializable]
    public class GameAwardRank
    {
        public int rank;
        public int num;
    }
    [Serializable]
    public class OnBagChanged
    {
        public List<ItemData> bag = new List<ItemData>();
    }

    [Serializable]
    public class Notice
    {
        public int count = 0;
        public string info = "";
    }

    [Serializable]
    public class RecentPlay
    {
        public List<int> recent = new List<int>();
    }

    [Serializable]
    public class SearchGameReq
    {
        public string search = "";
    }

    [Serializable]
    public class TableDouziSync
    {
        public int chairId;
        public int douzi;
    }

    /// <summary>
    /// 游戏结算面板数据
    /// </summary>
    [Serializable]
    public class GameOverResult
    {
        public bool isGameClosed;
        public List<int> winUids = new List<int>();
        public List<GameOverUserInfo> userList = new List<GameOverUserInfo>();
    }
    [Serializable]
    public class GameOverUserInfo
    {
        public int uid;
        public string name;
        public int sex;
        public int score;
        public int scoreAdd;
        public int douzi;
        public int douziAdd;
    }

    [Serializable]
    public class OnKicked
    {
        public int code;
        public string info;
    }

    [Serializable]
    public class GameRoomChat
    {
        public int uid;
        public string msg;
    }

    [Serializable]
    public class GameRoomChatSeq
    {
        public int uid;
        public int index;
    }

    [Serializable]
    public class EnterXiangQi
    {
        public int gameId;
        public int tableId;
        public bool canChatInRoom;
        public int gameTime;
        public int stepTime;
        public int countTime;
        public int nowChairId;
        public List<XiangQiZi> qipan;
        public List<XiangQiPlayer> players;
    }
    [Serializable]
    public class XiangQiPlayer
    {
        public int uid;
        public string sid;
        public string nickname;
        public int headId;
        public int chairId;
        public int qiZiColor;
        public int douzi;
        public int score;
        public int gameTime;
        public bool isStepTime;
        public int leftTime;
    }

    [Serializable]
    public class XiangQiZi
    {
        public int i;
        public int j;
        public int c;
        public int t;
    }

    [Serializable]
    public class XiangQiLuoZi
    {
        public int i;
        public int j;
        public int i2;
        public int j2;
        public bool isOver;
    }

    [Serializable]
    public class PingYesOrNo
    {
        public bool agree;
    }
}
