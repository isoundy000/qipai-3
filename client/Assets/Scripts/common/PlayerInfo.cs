using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerInfo
{

    public static List<int> testIds = new List<int>() { 1, 2, 3 };

    public static string connectorHost = "";
    public static int connectorPort = 0;
    public static int token = 0;

    public static int uid = 0;
    public static Proto.PlayerData playerData = null;

    public static Dictionary<int, int> bagInfo = new Dictionary<int, int>();    // 背包信息
    public static List<Proto.MailData> mails = new List<Proto.MailData>();    // 邮件信息
    public static Dictionary<int, FriendData> friends = new Dictionary<int, FriendData>();    // 好友信息

    public static int refreshMyRankCost = 0;    // 刷新个人最新战绩消耗金币数
    public static int createGameHourPer = 0;    // 创建比赛时，钻石消耗时间间隔
    public static int createGameDiamondPer = 0;    // 创建比赛时，钻石消耗量
    public static int douziGetPercent = 0;    // 比赛结算时，平台金币提成
    public static string notice = "";       // 公告
    public static Dictionary<int, Proto.gameTypeInfo> gameTypes = new Dictionary<int, Proto.gameTypeInfo>();  //游戏类型信息  （类型：信息）
    public static Proto.GameTimeDiff gameTimeDiff = null;       // 创建比赛的时间间隔配置（服务器下发）
    public static Dictionary<Proto.paramType, string> gameParamName = new Dictionary<Proto.paramType, string>();    // 游戏参数名字

    public static int nowGameType = 0;  // 当前游戏类型
    public static int nowGameId = 0;    // 当前游戏id
    public static bool canInviteFriend = false; // 当前匹配桌子能否邀请好友同玩
    public static bool canChatInRoom = false; // 当前游戏桌子能否聊天
    public static Dictionary<int, Proto.WaitUserInfoProto> waitPlayers = new Dictionary<int, Proto.WaitUserInfoProto>();    // 匹配桌子中的玩家信息
    public static string gameInfo = "";     // 游戏具体信息

    public static List<Proto.Notice> rollNotices = new List<Proto.Notice>();    // 滚动播报
    public static int rollNoticeI = 0;  // 当前播报序号
    public static float rollNoticeX = 0;    // 当前播报ui位置
    public static List<int> sendRollNoticeCost = new List<int>();   // 发送滚动播报的消耗
    private static Proto.RecentPlay recentPlay = null;

    /// <summary>
    /// 返回到登录场景，重置数据
    /// </summary>
    public static void ResetData()
    {
        playerData = null;
        bagInfo.Clear();
        mails.Clear();
        friends.Clear();
        gameTypes.Clear();
        waitPlayers.Clear();
        recentPlay = null;
    }

    public static void InitPlayer(Proto.EnterServer msg)
    {
        // 玩家信息
        playerData = msg.info.role;

        // 背包信息
        bagInfo.Clear();
        foreach (var one in msg.info.bag)
        {
            bagInfo[one.id] = one.num;
        }

        // 好友信息
        friends.Clear();
        foreach (var one in msg.info.friends)
        {
            FriendData tmp = new FriendData();
            tmp.uid = one.uid;
            tmp.sid = one.sid;
            tmp.sex = one.sex;
            tmp.nickname = one.nickname;
            tmp.signature = one.signature;
            friends.Add(one.uid, tmp);
        }

        // 邮件信息
        mails = msg.info.mails;

    }

    public static void InitConfig(Proto.SomeConfig config)
    {
        refreshMyRankCost = config.refreshMyRankCost;
        createGameHourPer = config.createGameHourPer;
        createGameDiamondPer = config.createGameDiamondPer;
        douziGetPercent = config.douziGetPercent;
        notice = config.notice;
        gameTimeDiff = config.gameTimeDiff;
        gameTypes.Clear();
        foreach (var one in config.gameTypes)
        {
            gameTypes[one.type] = one;
        }
        gameParamName.Clear();
        foreach (var one in config.gameParamName)
        {
            gameParamName[one.type] = one.name;
        }

        rollNotices = config.rollNotices;
        rollNoticeI = 0;
        rollNoticeX = 0;
        sendRollNoticeCost = config.sendRollNoticeCost;

    }

    public static int GetItemNum(int id)
    {
        if (bagInfo.ContainsKey(id))
        {
            return bagInfo[id];
        }
        return 0;
    }

    public static Proto.RecentPlay GetRecentPlay()
    {
        Debug.Log("recentplay: " + PlayerPrefs.GetString("recentPlay"));
        if (recentPlay == null)
        {
            string recentPlayStr = PlayerPrefs.GetString("recentPlay");
            if (recentPlayStr == "")
            {
                recentPlay = new Proto.RecentPlay();
            }
            else
            {
                recentPlay = JsonUtility.FromJson<Proto.RecentPlay>(recentPlayStr);
            }
        }
        return recentPlay;
    }

    public static void ChangeRecentPlay(int id)
    {
        if (recentPlay.recent.Count > 0 && recentPlay.recent[0] == id)
        {
            return;
        }
        recentPlay.recent.Remove(id);
        recentPlay.recent.Insert(0, id);
        if (recentPlay.recent.Count > 5)
        {
            recentPlay.recent.RemoveAt(5);
        }

        PlayerPrefs.SetString("recentPlay", JsonUtility.ToJson(recentPlay));
    }

}

public class SceneNames
{
    public const string login = "login";
    public const string main = "main";
    public const string match = "match";
}


public class RoomInfo
{
    public int roomId = 0;
    public string roomname = "";
    public int chairCount = 0;
}

public class FriendData
{
    public int uid;
    public string sid;
    public string nickname;
    public int sex;
    public string signature;
    public List<ChatData> chatInfo = new List<ChatData>();
}

public class ChatData
{
    public bool isMe = false;
    public string info = "";
}

public class ChatInfoTmp
{
    public int friendUid;
    public bool isMe;
    public string info;
}

[Serializable]
public class I_param_dropdown
{
    public Proto.paramType type;
    public List<string> values;
    public int defaultIndex;
}

[Serializable]
public class I_param_input
{
    public Proto.paramType type;
    public bool isNumber;
    public int min;
    public int max;
    public string placeholder;
}


enum ItemId
{
    douzi = 1001,
    diamond = 1002,
}