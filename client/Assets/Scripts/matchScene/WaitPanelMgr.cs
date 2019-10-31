using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class WaitPanelMgr : MonoBehaviour
{
    public enum WaitStatus
    {
        beReady,
        cancelReady,
        startMatch,
        matching
    }

    public static WaitPanelMgr instance = null;
    public GameObject waitUserPrefab;
    public Transform waitUserParent;
    public GameObject matchFriendPrefab;
    public Transform matchFriendParent;
    ToggleGroup friendToggleGroup;
    private int nowFriendUid = 0;
    private bool nowChatIsTable = true;
    public InputField chatInput;
    public Text chatContent;
    private System.Text.StringBuilder tableChatMsg = new System.Text.StringBuilder();

    public Text funcText;
    private WaitStatus nowWaitStatus;
    [HideInInspector]
    public int masterId = 0;
    private void Awake()
    {
        instance = this;
    }
    // Use this for initialization
    void Start()
    {
        friendToggleGroup = matchFriendParent.GetComponent<ToggleGroup>();
        SocketClient.AddHandler(Route.onNewPlayer, SVR_OnNewPlayer);
        SocketClient.AddHandler(Route.onReadyOrNot, SVR_OnReadyOrNot);
        SocketClient.AddHandler(Route.onStartMatch, SVR_OnStartMatch);
        SocketClient.AddHandler(Route.onCancelMatch, SVR_OnCancelMatch);
        SocketClient.AddHandler(Route.onOneLeaveMatchTable, SVR_OnOneLeaveWaitTable);
        SocketClient.AddHandler(Route.onEnterTable, SVR_OnEnterTable);
        SocketClient.AddHandler(Route.onChatInMatchTable, SVR_onChatInTable);
        SocketClient.AddHandler(Route.onMatchClose, SVR_OnMatchClose);
        SocketClient.AddHandler(Route.match_main_leaveTable, SVR_leaveBack);
        SocketClient.AddHandler(Route.onInvite, SVR_onInvite);
        SocketClient.AddHandler(Route.onKicked, SVR_onKicked);

        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onAddFriend, OnAddFriend);
        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onDelFriend, OnDelFriend);
        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onFriendInfoChange, OnFriendInfoChange);
        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onFriendChat, OnFriendChat);
        Init();
    }


    public void Init()
    {
        Proto.gameTypeInfo gameTypeInfo = PlayerInfo.gameTypes[PlayerInfo.nowGameType];
        transform.Find("roomName").GetComponent<Text>().text = gameTypeInfo.name;
        for (int i = 0; i < gameTypeInfo.roleNum; i++)
        {
            Transform tmpTrsm = Instantiate(waitUserPrefab, waitUserParent).transform;
            tmpTrsm.name = "empty";
        }
        foreach (var tmpUserInfo in PlayerInfo.waitPlayers.Values)
        {
            if (tmpUserInfo.isMaster)
            {
                masterId = tmpUserInfo.uid;
                break;
            }
        }
        foreach (var tmpUserInfo in PlayerInfo.waitPlayers.Values)
        {
            AddUser(tmpUserInfo);
        }

        foreach (var friend in PlayerInfo.friends.Values)
        {
            AddFriend(friend);
        }
    }

    void AddUser(Proto.WaitUserInfoProto userInfo)
    {
        Transform emptyTrsm = null;
        foreach (Transform trsm in waitUserParent)
        {
            if (trsm.name == "empty")
            {
                emptyTrsm = trsm;
                break;
            }
        }
        emptyTrsm.GetComponent<WaitUserPrefab>().Init(userInfo);
    }

    void AddFriend(FriendData friend)
    {
        Transform tmpTrsm = Instantiate(matchFriendPrefab, matchFriendParent).transform;
        tmpTrsm.GetComponent<MatchFriendPrefab>().Init(friend);
        tmpTrsm.GetComponent<Toggle>().group = friendToggleGroup;
    }

    /// <summary>
    /// 新进来一个玩家
    /// </summary>
    /// <param name="msg"></param>
    void SVR_OnNewPlayer(string msg)
    {
        Proto.WaitUserInfoProto data = JsonUtility.FromJson<Proto.WaitUserInfoProto>(msg);
        PlayerInfo.waitPlayers.Add(data.uid, data);
        AddUser(data);
    }

    /// <summary>
    /// 邀请好友进入匹配桌子
    /// </summary>
    public void InviteFriendToGame()
    {
        if (nowFriendUid == 0)
        {
            return;
        }
        if (!PlayerInfo.canInviteFriend)
        {
            UIManager.instance.SetTileInfo("该比赛不可好友同玩");
            return;
        }
        if (nowWaitStatus == WaitStatus.matching)
        {
            UIManager.instance.SetTileInfo("正在匹配中，不可邀请");
            return;
        }
        if (PlayerInfo.waitPlayers.ContainsKey(nowFriendUid))
        {
            UIManager.instance.SetTileInfo("该好友已在桌子中");
            return;
        }
        var friendData = PlayerInfo.friends[nowFriendUid];
        if (friendData.sid == "")
        {
            UIManager.instance.SetTileInfo("该好友不在线");
            return;
        }
        Proto.Uidsid msg = new Proto.Uidsid();
        msg.uid = nowFriendUid;
        msg.sid = friendData.sid;
        SocketClient.SendMsg(Route.match_main_inviteFriend, msg);
        UIManager.instance.SetTileInfo("邀请成功");
    }

    /// <summary>
    /// 准备或取消准备
    /// </summary>
    /// <param name="msg"></param>
    void SVR_OnReadyOrNot(string msg)
    {
        Proto.OnReadyOrNotInMatchTable data = JsonUtility.FromJson<Proto.OnReadyOrNotInMatchTable>(msg);
        Transform trsm = waitUserParent.Find(data.uid.ToString());
        PlayerInfo.waitPlayers[data.uid].isReady = data.isReady;
        if (trsm)
        {
            trsm.GetComponent<WaitUserPrefab>().ReadyOrNot(data.isReady);
        }
    }

    /// <summary>
    /// 开始匹配
    /// </summary>
    /// <param name="msg"></param>
    void SVR_OnStartMatch(string msg)
    {
        SetFuncBtn(WaitStatus.matching);
    }
    /// <summary>
    /// 停止匹配
    /// </summary>
    /// <param name="msg"></param>
    void SVR_OnCancelMatch(string msg)
    {
        if (PlayerInfo.uid == masterId)
        {
            SetFuncBtn(WaitStatus.startMatch);
        }
        else
        {
            SetFuncBtn(WaitStatus.cancelReady);
        }
    }

    /// <summary>
    /// 某人离开桌子了
    /// </summary>
    /// <param name="msg"></param>
    void SVR_OnOneLeaveWaitTable(string msg)
    {
        Proto.OnOneLeaveMatchTable data = JsonUtility.FromJson<Proto.OnOneLeaveMatchTable>(msg);
        if (data.uid == PlayerInfo.uid)
        {
            PlayerInfo.nowGameType = 0;
            PlayerInfo.nowGameId = 0;
            PlayerInfo.waitPlayers.Clear();
            ShowOutMatchWarn("您被踢出该房间");
            return;
        }
        PlayerInfo.waitPlayers.Remove(data.uid);
        Transform trsm = waitUserParent.Find(data.uid.ToString());
        if (trsm)
        {
            trsm.GetComponent<WaitUserPrefab>().Leave();
        }
        if (data.nowMaster != 0)
        {
            masterId = data.nowMaster;
            PlayerInfo.waitPlayers[masterId].isMaster = true;
            PlayerInfo.waitPlayers[masterId].isReady = true;
        }
        foreach (var tmpUserInfo in PlayerInfo.waitPlayers.Values)
        {
            if (!tmpUserInfo.isMaster)
            {
                tmpUserInfo.isReady = false;
            }
            waitUserParent.Find(tmpUserInfo.uid.ToString()).GetComponent<WaitUserPrefab>().Init(tmpUserInfo);
        }
    }

    public void SetFuncBtn(WaitStatus status)
    {
        nowWaitStatus = status;
        if (status == WaitStatus.beReady)
        {
            funcText.text = "准备";
        }
        else if (status == WaitStatus.cancelReady)
        {
            funcText.text = "取消准备";
        }
        else if (status == WaitStatus.startMatch)
        {
            funcText.text = "开始匹配";
        }
        else if (status == WaitStatus.matching)
        {
            funcText.text = "匹配中...";
        }
    }
    /// <summary>
    /// 添加等待桌子中的玩家为好友
    /// </summary>
    public void Btn_AddFriend(int uid)
    {
        if (uid == 0 || uid == PlayerInfo.uid || PlayerInfo.friends.ContainsKey(uid))
        {
            return;
        }
        var otherData = PlayerInfo.waitPlayers[uid];
        Proto.Uidsid msg = new Proto.Uidsid();
        msg.uid = uid;
        SocketClient.SendMsg(Route.info_friend_askFriend, msg);
        UIManager.instance.SetTileInfo("好友请求已发送");
    }

    public void NewFriend(int uid)
    {
        if (PlayerInfo.waitPlayers.ContainsKey(uid))
        {
            waitUserParent.Find(uid.ToString()).GetComponent<WaitUserPrefab>().BeFriend();
        }
    }

    public void Btn_Func()
    {
        if (nowWaitStatus == WaitStatus.matching)
        {
            if (masterId != PlayerInfo.uid)
            {
                return;
            }
            SocketClient.SendMsg(Route.match_main_cancelMatch);
            return;
        }
        if (nowWaitStatus == WaitStatus.beReady || nowWaitStatus == WaitStatus.cancelReady)
        {
            SocketClient.SendMsg(Route.match_main_readyOrNot);
        }
        else if (nowWaitStatus == WaitStatus.startMatch)
        {
            foreach (var tmpUserInfo in PlayerInfo.waitPlayers.Values)
            {
                if (!tmpUserInfo.isReady)
                {
                    UIManager.instance.SetTileInfo("有人未准备");
                    return;
                }
            }
            SocketClient.SendMsg(Route.match_main_startMatch);
        }
    }

    // 退出按钮
    public void Btn_Leave()
    {
        SocketClient.SendMsg(Route.match_main_leaveTable);
    }

    void SVR_leaveBack(string msg)
    {
        SceneManager.LoadScene(SceneNames.main);
    }

    public void KickPlayer(int uid)
    {
        if (nowWaitStatus == WaitStatus.matching)
        {
            UIManager.instance.SetTileInfo("匹配中，不可踢人");
            return;
        }
        Proto.Uidsid msg = new Proto.Uidsid();
        msg.uid = uid;
        SocketClient.SendMsg(Route.match_main_kickPlayer, msg);
    }

    public void SVR_OnEnterTable(string msg)
    {
        Debug.Log(msg);
        var msgObj = JsonUtility.FromJson<Proto.OnEnterTable>(msg);
        PlayerInfo.nowGameType = msgObj.gameType;
        PlayerInfo.nowGameId = msgObj.gameId;
        PlayerInfo.gameInfo = msgObj.data;
        SceneManager.LoadScene(PlayerInfo.gameTypes[PlayerInfo.nowGameType].scene);
        PlayerInfo.ChangeRecentPlay(msgObj.gameId);
    }

    /// <summary>
    /// 点击好友
    /// </summary>
    /// <param name="uid"></param>
    public void ClickFriend(int uid)
    {
        nowFriendUid = uid;
        if (nowChatIsTable)
        {
            return;
        }
        chatContent.text = "";
        InitFriendChat();
    }

    private void InitFriendChat()
    {
        if (nowFriendUid == 0)
        {
            return;
        }
        System.Text.StringBuilder str = new System.Text.StringBuilder();
        foreach (ChatData tmp in PlayerInfo.friends[nowFriendUid].chatInfo)
        {
            if (tmp.isMe)
            {
                str.Append("\n<color=lime>→</color>");
            }
            else
            {
                str.Append("\n<color=grey>→</color>");
            }
            str.Append(tmp.info);
        }
        chatContent.text = str.ToString();
    }

    #region friend listener
    void OnAddFriend(object _msg)
    {
        AddFriend(_msg as FriendData);

    }
    void OnDelFriend(object msg)
    {
        int delUid = (msg as Proto.Uidsid).uid;
        Destroy(matchFriendParent.Find(delUid.ToString()).gameObject);
        if (nowFriendUid == delUid)
        {
            nowFriendUid = 0;
            if (!nowChatIsTable)
            {
                chatContent.text = "";
            }
        }
    }
    void OnFriendInfoChange(object msg)
    {
        var tmp = msg as Proto.FriendInfo;
        matchFriendParent.Find(tmp.uid.ToString()).GetComponent<MatchFriendPrefab>().OnLineOrNot(tmp.sid != "");
    }
    void OnFriendChat(object _msg)
    {
        ChatInfoTmp msg = _msg as ChatInfoTmp;

        if (nowFriendUid != msg.friendUid || nowChatIsTable)
        {
            return;
        }

        if (msg.isMe)
        {
            chatContent.text += "\n<color=lime>→</color>" + msg.info;
        }
        else
        {
            chatContent.text += "\n<color=grey>→</color>" + msg.info;
        }

    }
    #endregion

    /// <summary>
    /// 聊天，好友还是桌子
    /// </summary>
    /// <param name="isOn"></param>
    public void Toggle_chatFriendOrTable(bool isOn)
    {
        nowChatIsTable = isOn;
        transform.Find("ToggleChat/Text").GetComponent<Text>().text = nowChatIsTable ? "桌子" : "好友";
        if (nowChatIsTable)
        {
            chatContent.text = tableChatMsg.ToString();
        }
        else
        {
            chatContent.text = "";
            InitFriendChat();
        }
    }
    /// <summary>
    /// 聊天按钮
    /// </summary>
    public void Btn_chat()
    {
        if (chatInput.text == "")
        {
            return;
        }
        if (nowChatIsTable)
        {
            Proto.OnChatInMatchTable data = new Proto.OnChatInMatchTable();
            data.data = chatInput.text;
            SocketClient.SendMsg(Route.match_main_chat, data);
            chatInput.text = "";
            return;
        }
        if (nowFriendUid == 0)
        {
            return;
        }
        FriendData tmp = PlayerInfo.friends[nowFriendUid];

        if (tmp.sid == "")
        {
            return;
        }
        Proto.FriendChatReq msg = new Proto.FriendChatReq();
        msg.uid = tmp.uid;
        msg.sid = tmp.sid;
        msg.data = chatInput.text;
        SocketClient.SendMsg(Route.info_friend_chat, msg);
        chatInput.text = "";
    }

    /// <summary>
    /// 桌子里聊天
    /// </summary>
    /// <param name="msg"></param>
    void SVR_onChatInTable(string msg)
    {
        Proto.OnChatInMatchTable data = JsonUtility.FromJson<Proto.OnChatInMatchTable>(msg);
        string chatStr = "\n<color=lime>" + data.nickname + ":</color>" + data.data;
        tableChatMsg.Append(chatStr);
        if (nowChatIsTable)
        {
            chatContent.text += chatStr;
        }
    }

    void SVR_OnMatchClose(string msg)
    {
        ShowOutMatchWarn("比赛已关闭");
    }

    void ShowOutMatchWarn(string info)
    {
        var trsm = transform.Find("outMatchWarn");
        trsm.gameObject.SetActive(true);
        trsm.Find("Text").GetComponent<Text>().text = info;
    }

    public void Btn_OutMatchWarn()
    {
        SceneManager.LoadScene(SceneNames.main);
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

    void SVR_onKicked(string msg)
    {
        SocketClient.DisConnect();
        var data = JsonUtility.FromJson<Proto.OnKicked>(msg);
        UIManager.instance.ShowPanel(UIPanel.onKickedPanel).GetComponent<OnKicked>().Init(data.info);
    }


    private void OnDestroy()
    {
        SocketClient.RemoveHandler(Route.onNewPlayer);
        SocketClient.RemoveHandler(Route.onReadyOrNot);
        SocketClient.RemoveHandler(Route.onStartMatch);
        SocketClient.RemoveHandler(Route.onCancelMatch);
        SocketClient.RemoveHandler(Route.onOneLeaveMatchTable);
        SocketClient.RemoveHandler(Route.onEnterTable);
        SocketClient.RemoveHandler(Route.onChatInMatchTable);
        SocketClient.RemoveHandler(Route.onMatchClose);
        SocketClient.RemoveHandler(Route.match_main_leaveTable);
        SocketClient.RemoveHandler(Route.onInvite);
        SocketClient.RemoveHandler(Route.onKicked);
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onAddFriend, OnAddFriend);
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onDelFriend, OnDelFriend);
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onFriendInfoChange, OnFriendInfoChange);
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onFriendChat, OnFriendChat);
        PlayerInfo.waitPlayers.Clear();
    }
}
