using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CommonHandlerCb
{
    onAddFriend,
    onDelFriend,
    onFriendInfoChange,
    onFriendChat,
    onNewMail,
    onUpdateGameTypeState,
    onBagChanged,
}

public class CommonHandler
{
    private static CommonHandler _instance = null;
    private Dictionary<CommonHandlerCb, Action<object>> commonCb = new Dictionary<CommonHandlerCb, Action<object>>();

    public static CommonHandler instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CommonHandler();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 开始监听
    /// </summary>
    public void StartListen()
    {
        SocketClient.AddHandler(Route.onAskFriend, SVR_onAskFriend);
        SocketClient.AddHandler(Route.onAddFriend, SVR_onAddFriend);
        SocketClient.AddHandler(Route.onDelFriend, SVR_onDelFriend);
        SocketClient.AddHandler(Route.onFriendInfoChange, SVR_onFriendInfoChange);
        SocketClient.AddHandler(Route.onFriendChat, SVR_onFriendChat);
        SocketClient.AddHandler(Route.onNewMail, SVR_onNewMail);
        SocketClient.AddHandler(Route.onUpdateGameTypeState, SVR_onUpdateGameTypeState);
        SocketClient.AddHandler(Route.onBagChanged, SVR_onBagChanged);
        SocketClient.AddHandler(Route.onNotice, SVR_onNotice);
        SocketClient.AddHandler(Route.onGameWinData, SVR_onGameWinData);
    }
    /// <summary>
    /// 停止监听
    /// </summary>
    /// <param name="removeCb">移除消息回调</param>
    public void StopListen(bool removeCb = false)
    {
        SocketClient.RemoveHandler(Route.onAskFriend);
        SocketClient.RemoveHandler(Route.onAddFriend);
        SocketClient.RemoveHandler(Route.onDelFriend);
        SocketClient.RemoveHandler(Route.onFriendInfoChange);
        SocketClient.RemoveHandler(Route.onFriendChat);
        SocketClient.RemoveHandler(Route.onUpdateGameTypeState);
        SocketClient.RemoveHandler(Route.onBagChanged);
        SocketClient.RemoveHandler(Route.onNotice);
        SocketClient.RemoveHandler(Route.onGameWinData);
        if (removeCb)
        {
            commonCb.Clear();
        }
    }
    /// <summary>
    /// 添加回调
    /// </summary>
    public void AddCommonCb(CommonHandlerCb cbType, Action<object> cb)
    {
        if (!commonCb.ContainsKey(cbType))
        {
            commonCb[cbType] = cb;
        }
        else
        {
            commonCb[cbType] += cb;
        }
    }
    /// <summary>
    /// 移除回调
    /// </summary>
    public void RemoveCommonCb(CommonHandlerCb cbType, Action<object> cb)
    {
        if (commonCb.ContainsKey(cbType))
        {
            commonCb[cbType] -= cb;
        }
    }

    #region 好友相关消息
    void SVR_onAskFriend(string _msg)
    {
        Proto.OnAskFriend msg = JsonUtility.FromJson<Proto.OnAskFriend>(_msg);
        UIManager.instance.ShowPanel(UIPanel.MainScene.askFriend).GetComponent<FriendAsk>().Init(msg);
    }
    void SVR_onAddFriend(string _msg)
    {
        Proto.FriendInfo msg = JsonUtility.FromJson<Proto.FriendInfo>(_msg);
        FriendData tmp = new FriendData();
        tmp.uid = msg.uid;
        tmp.sid = msg.sid;
        tmp.sex = msg.sex;
        tmp.nickname = msg.nickname;
        tmp.signature = msg.signature;
        PlayerInfo.friends.Add(tmp.uid, tmp);
        if (commonCb.ContainsKey(CommonHandlerCb.onAddFriend))
        {
            commonCb[CommonHandlerCb.onAddFriend]?.Invoke(tmp);
        }
    }
    void SVR_onDelFriend(string _msg)
    {
        Proto.Uidsid msg = JsonUtility.FromJson<Proto.Uidsid>(_msg);
        PlayerInfo.friends.Remove(msg.uid);
        if (commonCb.ContainsKey(CommonHandlerCb.onDelFriend))
        {
            commonCb[CommonHandlerCb.onDelFriend]?.Invoke(msg);
        }
    }
    void SVR_onFriendInfoChange(string _msg)
    {
        Proto.FriendInfo msg = JsonUtility.FromJson<Proto.FriendInfo>(_msg);
        var friend = PlayerInfo.friends[msg.uid];
        friend.sid = msg.sid;
        if (msg.sex > 0) friend.sex = msg.sex;
        if (msg.nickname != "") friend.nickname = msg.nickname;
        if (msg.signature != "") friend.signature = msg.signature;
        if (commonCb.ContainsKey(CommonHandlerCb.onFriendInfoChange))
        {
            commonCb[CommonHandlerCb.onFriendInfoChange]?.Invoke(msg);
        }
    }
    void SVR_onFriendChat(string _msg)
    {
        Proto.FriendChatMsg msg = JsonUtility.FromJson<Proto.FriendChatMsg>(_msg);
        ChatData tmp = new ChatData();
        tmp.info = msg.info;
        int friendUid = 0;
        if (msg.from == PlayerInfo.uid)
        {
            friendUid = msg.to;
            tmp.isMe = true;
        }
        else
        {
            friendUid = msg.from;
            tmp.isMe = false;
        }
        PlayerInfo.friends[friendUid].chatInfo.Add(tmp);

        ChatInfoTmp tmpMsg = new ChatInfoTmp();
        tmpMsg.friendUid = friendUid;
        tmpMsg.info = tmp.info;
        tmpMsg.isMe = tmp.isMe;
        if (commonCb.ContainsKey(CommonHandlerCb.onFriendChat))
        {
            commonCb[CommonHandlerCb.onFriendChat]?.Invoke(tmpMsg);
        }
    }
    #endregion


    void SVR_onNewMail(string _msg)
    {
        Proto.MailData mail = JsonUtility.FromJson<Proto.MailData>(_msg);
        PlayerInfo.mails.Add(mail);
        if (commonCb.ContainsKey(CommonHandlerCb.onNewMail))
        {
            commonCb[CommonHandlerCb.onNewMail]?.Invoke(mail);
        }
    }


    //void SVR_onKicked(JSONObject msg)
    //{
    //    Debug.Log(msg.ToString());
    //    SocketClient.DisConnect();
    //    PlayerInfo.SocketCloseReset();
    //    Transform tmp = UIManager.instance.ShowPanel(UIPanel.onKickedPanel);
    //    OnKicked tmp2 = tmp.GetComponent<OnKicked>();
    //    if (msg["code"].t == 1)
    //    {
    //        tmp2.Init("您的账号在异地登录！如果不是本人操作，请完善安全信息！");
    //    }
    //}

    #region 游戏比赛相关

    /// <summary>
    /// 更新某游戏类型状态
    /// </summary>
    /// <param name="msg"></param>
    private void SVR_onUpdateGameTypeState(string msg)
    {
        var data = JsonUtility.FromJson<Proto.OnUpdateGameTypeState>(msg);
        var gameTypeInfo = PlayerInfo.gameTypes[data.gameType];
        data.lastState = gameTypeInfo.state;
        gameTypeInfo.state = data.state;
        if (commonCb.ContainsKey(CommonHandlerCb.onUpdateGameTypeState))
        {
            commonCb[CommonHandlerCb.onUpdateGameTypeState]?.Invoke(data);
        }
    }

    #endregion

    /// <summary>
    /// 背包信息改变
    /// </summary>
    /// <param name="msg"></param>
    void SVR_onBagChanged(string msg)
    {
        var data = JsonUtility.FromJson<Proto.OnBagChanged>(msg);
        foreach (var one in data.bag)
        {
            PlayerInfo.bagInfo[one.id] = one.num;
        }
        if (commonCb.ContainsKey(CommonHandlerCb.onBagChanged))
        {
            commonCb[CommonHandlerCb.onBagChanged]?.Invoke(data);
        }
    }

    /// <summary>
    /// 滚动播报
    /// </summary>
    /// <param name="msg"></param>
    void SVR_onNotice(string msg)
    {
        var data = JsonUtility.FromJson<Proto.Notice>(msg);
        PlayerInfo.rollNotices.Add(data);
    }

    /// <summary>
    /// 游戏结算场次和分数
    /// </summary>
    /// <param name="msg"></param>
    void SVR_onGameWinData(string msg)
    {
        var data = JsonUtility.FromJson<Proto.gameTypeScoreData>(msg);
        Proto.gameTypeScoreData updateData = null;
        foreach (var one in PlayerInfo.playerData.gameData)
        {
            if (one.type == data.type)
            {
                updateData = one;
                break;
            }
        }
        if (updateData == null)
        {
            updateData = new Proto.gameTypeScoreData();
            updateData.type = data.type;
            PlayerInfo.playerData.gameData.Add(updateData);
        }
        updateData.all = data.all;
        updateData.win = data.win;
        updateData.score = data.score;
    }

}
