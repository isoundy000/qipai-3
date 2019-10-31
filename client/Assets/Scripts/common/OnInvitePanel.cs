using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnInvitePanel : MonoBehaviour
{
    public static OnInvitePanel instance = null;
    Proto.OnInvite inviteInfo = null;
    private void Awake()
    {
        instance = this;
        SocketClient.AddHandler(Route.info_game_enterMatchTable, SVR_enterFriendTableBack);
    }

    public void Init(Proto.OnInvite msg)
    {
        inviteInfo = msg;
        transform.Find("Image/nickname").GetComponent<Text>().text = msg.nickname;
        transform.Find("Image/gamename").GetComponent<Text>().text = PlayerInfo.gameTypes[msg.gameType].name;
    }

    public void Btn_Close()
    {
        Destroy(gameObject);
    }

    public void Btn_Agree()
    {
        if(WaitPanelMgr.instance != null) 
        {
            UIManager.instance.SetSomeInfo("当前在匹配桌子中，不可跟踪游戏");
            Btn_Close();
            return;
        }
        if (GameCommon.instance != null)
        {
            UIManager.instance.SetSomeInfo("当前在桌子中，不可跟踪游戏");
            Btn_Close();
            return;
        }
        Proto.EnterFriendTable msg = new Proto.EnterFriendTable();
        msg.gameId = inviteInfo.gameId;
        msg.matchSvr = inviteInfo.matchSvr;
        msg.rankSvr = inviteInfo.rankSvr;
        msg.tableId = inviteInfo.tableId;
        msg.friendUid = inviteInfo.uid;
        msg.pwd = transform.Find("Image/pwd").GetComponent<InputField>().text;
        SocketClient.SendMsg(Route.info_game_enterMatchTable, msg);
    }

    void SVR_enterFriendTableBack(string msg)
    {
        var data = JsonUtility.FromJson<Proto.JustCode>(msg);
        string wrongInfo = "";
        switch (data.code)
        {
            case 1:
                UIManager.instance.ShowPanel(UIPanel.gameWarnPanel);
                return;
            case 2:
                wrongInfo = "游戏不存在";
                break;
            case 3:
                wrongInfo = "游戏不存在";
                break;
            case 4:
                wrongInfo = "好友已不在该桌子中";
                break;
            case 5:
                wrongInfo = "你已在桌子中";
                break;
            case 6:
                wrongInfo = "桌子已满";
                break;
            case 7:
                wrongInfo = "该局游戏正在匹配中，不可进入";
                break;
            case 8:
                wrongInfo = "您当前正在匹配桌子中";
                break;
            case 103:
                wrongInfo = "密码错误";
                break;
            case 104:
                wrongInfo = "门票不足";
                break;
            case 101:
                wrongInfo = "游戏不存在";
                break;
            default:
                break;
        }
        UIManager.instance.SetSomeInfo(wrongInfo);
    }



    private void OnDestroy()
    {
        SocketClient.RemoveHandler(Route.info_game_enterMatchTable);
    }
}
