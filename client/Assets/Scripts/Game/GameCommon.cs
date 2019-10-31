using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameCommon : MonoBehaviour
{

    public static GameCommon instance = null;
    private Action gameReconnectHandler = null;


    private void Awake()
    {
        instance = this;
        WatchSocket();
    }

    public void WatchSocket()
    {
        SocketClient.OnClose(OnConnectorClose);
    }

    void OnConnectorClose(string msg)
    {
        UIManager.instance.ShowPanel(UIPanel.GamePanel.gameReconnectPanel);
    }


    // Use this for initialization
    void Start()
    {
        //CommonHandler.instance.SetCommonCb(CommonHandlerCb.onFriend, OnFriend);
        //CommonHandler.instance.SetCommonCb(CommonHandlerCb.onChat, OnChat);

        SocketClient.AddHandler(Route.onEnterMatchTable, SVR_onEnterMatchTable);
        SocketClient.AddHandler(Route.onReadyOrNot, SVR_OnReadyOrNot);
        SocketClient.AddHandler(Route.onOneLeaveMatchTable, SVR_OnOneLeaveWaitTable);
        SocketClient.AddHandler(Route.onKicked, SVR_onKicked);
        SocketClient.AddHandler(Route.onInvite, SVR_onInvite);
    }


    public void SetGameReconnectHandler(Action handler)
    {
        gameReconnectHandler = handler;
    }

    public void ReconnectSuccess()
    {
        gameReconnectHandler?.Invoke();
    }


    // 游戏中好友处理消息
    void OnFriend(string msg)
    {
        //int type = msg["type"].t;
        //if (type == 2)
        //{
        //    FriendData tmp = new FriendData();
        //    tmp.uid = msg["uid"].t;
        //    tmp.sid = msg["sid"].str;
        //    tmp.sex = msg["sex"].t;
        //    tmp.nickname = msg["nickname"].str;
        //    tmp.signature = msg["signature"].str;
        //    if (ChatInRoomMgr.instance != null)
        //    {
        //        ChatInRoomMgr.instance.NewFriend(tmp);
        //    }
        //    if (WaitPanelMgr.instance != null)
        //    {
        //        WaitPanelMgr.instance.NewFriend(tmp.uid);
        //    }
        //}
        //else if (type == 3)
        //{
        //    if (ChatInRoomMgr.instance != null)
        //    {
        //        ChatInRoomMgr.instance.DelFriend(msg["uid"].t);
        //    }
        //}
        //else if (type == 4)
        //{
        //    if (ChatInRoomMgr.instance != null)
        //    {
        //        ChatInRoomMgr.instance.OnLineOrNotFriend(msg["uid"].t, true);
        //    }
        //}
        //else if (type == 5)
        //{
        //    if (ChatInRoomMgr.instance != null)
        //    {
        //        ChatInRoomMgr.instance.OnLineOrNotFriend(msg["uid"].t, false);
        //    }
        //}
    }


    // 好友聊天
    void OnChat(string msg)
    {
        //if (ChatInRoomMgr.instance != null)
        //{
        //    ChatData tmp = new ChatData();
        //    tmp.isMe = msg["isMe"].b;
        //    tmp.info = msg["info"].str;

        //    ChatInRoomMgr.instance.NewChat(msg["friendUid"].t, tmp);
        //}
    }

    void SVR_onEnterMatchTable(string msg)
    {
        Debug.Log("enterMatchTable");
        var data = JsonUtility.FromJson<Proto.OnEnterMatchTable>(msg);
        PlayerInfo.nowGameType = data.gameType;
        PlayerInfo.nowGameId = data.gameId;
        PlayerInfo.canInviteFriend = data.canInviteFriend;
        PlayerInfo.waitPlayers.Clear();
        foreach (var one in data.players)
        {
            PlayerInfo.waitPlayers[one.uid] = one;
        }
    }

    /// <summary>
    /// 准备或取消准备
    /// </summary>
    /// <param name="msg"></param>
    void SVR_OnReadyOrNot(string msg)
    {
        Proto.OnReadyOrNotInMatchTable data = JsonUtility.FromJson<Proto.OnReadyOrNotInMatchTable>(msg);
        PlayerInfo.waitPlayers[data.uid].isReady = data.isReady;
    }

    void SVR_OnOneLeaveWaitTable(string msg)
    {
        Proto.OnOneLeaveMatchTable data = JsonUtility.FromJson<Proto.OnOneLeaveMatchTable>(msg);
        if (data.uid == PlayerInfo.uid)
        {
            PlayerInfo.nowGameType = 0;
            PlayerInfo.nowGameId = 0;
            PlayerInfo.waitPlayers.Clear();
            return;
        }
        PlayerInfo.waitPlayers.Remove(data.uid);

        if (data.nowMaster != 0)
        {
            PlayerInfo.waitPlayers[data.nowMaster].isMaster = true;
            PlayerInfo.waitPlayers[data.nowMaster].isReady = true;
        }
        foreach (var tmpUserInfo in PlayerInfo.waitPlayers.Values)
        {
            if (!tmpUserInfo.isMaster)
            {
                tmpUserInfo.isReady = false;
            }
        }
    }

    void SVR_onKicked(string msg)
    {
        SocketClient.DisConnect();
        var data = JsonUtility.FromJson<Proto.OnKicked>(msg);
        UIManager.instance.ShowPanel(UIPanel.onKickedPanel).GetComponent<OnKicked>().Init(data.info);
    }

    void SVR_onInvite(string msg)
    {
        var data = JsonUtility.FromJson<Proto.OnInvite>(msg);
        if (OnInvitePanel.instance != null)
        {
            OnInvitePanel.instance.Init(data);
        }
        else
        {
            UIManager.instance.ShowPanel(UIPanel.MainScene.AboutGame.onInvitePanel).GetComponent<OnInvitePanel>().Init(data);
        }
    }

    private void OnDestroy()
    {
        SocketClient.OffClose();
        SocketClient.RemoveHandler(Route.onEnterMatchTable);
        SocketClient.RemoveHandler(Route.onReadyOrNot);
        SocketClient.RemoveHandler(Route.onOneLeaveMatchTable);
        SocketClient.RemoveHandler(Route.onKicked);
        SocketClient.RemoveHandler(Route.onInvite);
    }
}
