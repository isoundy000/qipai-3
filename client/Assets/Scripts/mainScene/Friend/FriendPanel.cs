using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendPanel : MonoBehaviour
{
    public static FriendPanel instance;

    public Transform friendParent;
    public GameObject friendPrefab;
    public Text chatText;
    public Text signatureText;
    private InputField friendNameInput;
    private ToggleGroup friendToggleGroup;
    private int nowUid = 0;
    private InputField chatInput;

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        friendToggleGroup = friendParent.GetComponent<ToggleGroup>();
        friendNameInput = transform.Find("friendNameInput").GetComponent<InputField>();
        chatInput = transform.Find("chatInput").GetComponent<InputField>();
        InitFriends();
        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onAddFriend, OnAddFriend);
        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onDelFriend, OnDelFriend);
        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onFriendInfoChange, OnFriendInfoChange);
        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onFriendChat, OnFriendChat);
    }

    // 生成好友
    void InitFriends()
    {
        foreach (FriendData friend in PlayerInfo.friends.Values)
        {
            Transform tmpTran = Instantiate(friendPrefab, friendParent).transform;
            tmpTran.GetComponent<FriendPrefab>().Init(friend);
            tmpTran.GetComponent<Toggle>().group = friendToggleGroup;
        }
    }

    #region 好友相关消息

    void OnAddFriend(object _msg)
    {
        Transform tmpTran = Instantiate(friendPrefab, friendParent).transform;
        tmpTran.GetComponent<FriendPrefab>().Init(_msg as FriendData);
        tmpTran.GetComponent<Toggle>().group = friendToggleGroup;
    }
    void OnDelFriend(object msg)
    {
        int uid = (msg as Proto.Uidsid).uid;
        Transform trsm = friendParent.Find(uid.ToString());
        if (trsm != null)
        {
            Destroy(trsm.gameObject);
            if (nowUid == uid)
            {
                signatureText.text = "";
                chatText.text = "";
            }
        }
    }
    void OnFriendInfoChange(object msg)
    {
        Proto.FriendInfo info = msg as Proto.FriendInfo;
        Transform trsm = friendParent.Find(info.uid.ToString());
        if (trsm != null)
        {
            trsm.GetComponent<FriendPrefab>().OnInfoChanged(info);
        }
    }

    void OnFriendChat(object _msg)
    {
        ChatInfoTmp msg = _msg as ChatInfoTmp;
        ChatData tmp = new ChatData();
        tmp.isMe = msg.isMe;
        tmp.info = msg.info;
        NewChat(msg.friendUid, tmp);
    }
    #endregion



    public void Btn_AddFriend()
    {
        int friendUid = 0;
        bool ok = int.TryParse(friendNameInput.text.Trim(), out friendUid);
        if (!ok)
        {
            return;
        }
        if (friendUid == PlayerInfo.uid)
        {
            UIManager.instance.SetSomeInfo("不可添加自己为好友");
            return;
        }
        if (PlayerInfo.friends.ContainsKey(friendUid))
        {
            UIManager.instance.SetSomeInfo("该玩家已经是您的好友了");
            return;
        }

        Proto.Uidsid msg = new Proto.Uidsid();
        msg.uid = friendUid;
        SocketClient.SendMsg(Route.info_friend_askFriend, msg);
        friendNameInput.text = "";
        UIManager.instance.SetTileInfo("好友申请已发送");
    }



    /// <summary>
    /// 删除好友
    /// </summary>
    public void Btn_Del()
    {
        if (!PlayerInfo.friends.ContainsKey(nowUid))
        {
            return;
        }
        Proto.Uidsid msg = new Proto.Uidsid();
        msg.uid = nowUid;
        SocketClient.SendMsg(Route.info_friend_delFriend, msg);
    }


    /// <summary>
    /// 聊天
    /// </summary>
    public void Btn_Chat()
    {
        string str = chatInput.text.Trim();
        if (str == "")
        {
            return;
        }
        if (!PlayerInfo.friends.ContainsKey(nowUid))
        {
            return;
        }
        FriendData tmp = PlayerInfo.friends[nowUid];

        if (tmp.sid == "")
        {
            UIManager.instance.SetTileInfo("该好友不在线");
            return;
        }
        Proto.FriendChatReq msg = new Proto.FriendChatReq();
        msg.uid = nowUid;
        msg.sid = tmp.sid;
        msg.data = str;
        SocketClient.SendMsg(Route.info_friend_chat, msg);
        chatInput.text = "";
    }

    /// <summary>
    /// 好友赠送
    /// </summary>
    public void Btn_Mail()
    {
        if (!PlayerInfo.friends.ContainsKey(nowUid))
        {
            return;
        }
        FriendData tmp = PlayerInfo.friends[nowUid];
        Transform tmpTrsm = UIManager.instance.ShowPanel(UIPanel.MainScene.sendMailPanel);
        tmpTrsm.GetComponent<MailSendPanel>().Init(nowUid, tmp.nickname);
    }

    /// <summary>
    /// 切换好友焦点
    /// </summary>
    /// <param name="uid"></param>
    public void SetNowUid(int uid)
    {
        nowUid = uid;
        FriendData tmp = PlayerInfo.friends[uid];
        signatureText.text = tmp.signature;
        InitChatContent(tmp.chatInfo);
    }

    /// <summary>
    /// 新的聊天消息
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="tmp"></param>
    public void NewChat(int uid, ChatData tmp)
    {
        if (nowUid == uid)
        {
            if (tmp.isMe)
            {
                chatText.text += "\n<color=green>→</color>" + tmp.info;
            }
            else
            {
                chatText.text += "\n<color=red>→</color>" + tmp.info;
            }
        }
    }

    private void InitChatContent(List<ChatData> chatInfo)
    {
        chatText.text = "";
        if (chatInfo == null)
        {
            return;
        }
        System.Text.StringBuilder str = new System.Text.StringBuilder();
        foreach (ChatData tmp in chatInfo)
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
        chatText.text = str.ToString();
    }

    public void Btn_Close()
    {
        Destroy(gameObject);
    }


    private void OnDestroy()
    {
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onAddFriend, OnAddFriend);
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onDelFriend, OnDelFriend);
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onFriendInfoChange, OnFriendInfoChange);
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onFriendChat, OnFriendChat);

    }
}
