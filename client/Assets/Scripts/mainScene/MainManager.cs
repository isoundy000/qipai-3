using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{

    public static MainManager instance;

    // Use this for initialization
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        WatchSocket();
        UIManager.instance.ShowPanel(UIPanel.MainScene.AboutGame.gamePanel);
        if (PlayerInfo.playerData.ifGetAward == 0)
        {
            UIManager.instance.ShowPanel(UIPanel.MainScene.signPanel);
        }
        SocketClient.AddHandler(Route.onInvite, SVR_onInvite);
        SocketClient.AddHandler(Route.onEnterMatchTable, SVR_onEnterMatchTable);
        SocketClient.AddHandler(Route.onKicked, SVR_onKicked);
    }



    /// <summary>
    /// 收到匹配邀请
    /// </summary>
    /// <param name="msg"></param>
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

    /// <summary>
    /// 进入匹配桌子
    /// </summary>
    /// <param name="msg"></param>
    void SVR_onEnterMatchTable(string msg)
    {
        var data = JsonUtility.FromJson<Proto.OnEnterMatchTable>(msg);
        PlayerInfo.nowGameType = data.gameType;
        PlayerInfo.nowGameId = data.gameId;
        PlayerInfo.canInviteFriend = data.canInviteFriend;
        PlayerInfo.waitPlayers.Clear();
        foreach (var one in data.players)
        {
            PlayerInfo.waitPlayers[one.uid] = one;
        }
        SceneManager.LoadScene(SceneNames.match);
    }

    void SVR_onKicked(string msg)
    {
        SocketClient.DisConnect();
        var data = JsonUtility.FromJson<Proto.OnKicked>(msg);
        UIManager.instance.ShowPanel(UIPanel.onKickedPanel).GetComponent<OnKicked>().Init(data.info);
    }

    /// <summary>
    /// 监听socket断开
    /// </summary>
    public void WatchSocket()
    {
        SocketClient.OnClose(OnConnectorClose);
    }


    void OnConnectorClose(string msg)
    {
        UIManager.instance.ShowPanel(UIPanel.MainScene.reconnectPanel);
    }

    private void OnDestroy()
    {
        SocketClient.OffClose();
        SocketClient.RemoveHandler(Route.onInvite);
        SocketClient.RemoveHandler(Route.onEnterMatchTable);
        SocketClient.RemoveHandler(Route.onKicked);
    }
}
