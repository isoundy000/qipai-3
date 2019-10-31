using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class LoginMgr : MonoBehaviour
{
    public static LoginMgr instance = null;
    void Start()
    {
        instance = this;
        CommonHandler.instance.StopListen(true);
        PlayerInfo.ResetData();
        string records = PlayerPrefs.GetString("loginRecords");
        if (records != "")
        {
            UIManager.instance.ShowPanel(UIPanel.LoginScene.fastLogin);
        }
        else
        {
            UIManager.instance.ShowPanel(UIPanel.LoginScene.registerAndLogin);
        }
    }

    public string GetLoginUrl()
    {
        return ServerIP.instance.GetLoginUrl();
    }

    /// <summary>
    /// 连接网关服
    /// </summary>
    public void ConnectToConnector(string host, int port, int _uid, int token)
    {
        PlayerInfo.uid = _uid;
        PlayerInfo.connectorHost = host;
        PlayerInfo.connectorPort = port;
        PlayerInfo.token = token;

        SocketClient.OnOpen(OnConnectorOpen);
        SocketClient.OnClose(OnConnectorClose);
        SocketClient.Connect(PlayerInfo.connectorHost, PlayerInfo.connectorPort);
    }

    void SVR_ConnectorLoginBack(string _msg)
    {
        Debug.Log(_msg);
        Proto.EnterServer msg = JsonUtility.FromJson<Proto.EnterServer>(_msg);
        if (msg.code != 0)
        {
            UIManager.instance.SetSomeInfo("登录失败");
            return;
        }
        PlayerInfo.InitPlayer(msg);
        SceneManager.LoadScene(SceneNames.main);
    }

    void SVR_GetConfigBack(string msg)
    {
        Debug.Log(msg);
        Proto.SomeConfig data = JsonUtility.FromJson<Proto.SomeConfig>(msg);
        PlayerInfo.InitConfig(data);
    }

    void OnConnectorOpen(string msg)
    {
        CommonHandler.instance.StartListen();
        //  请求配置
        SocketClient.AddHandler(Route.connector_main_getSomeConfig, SVR_GetConfigBack);
        SocketClient.SendMsg(Route.connector_main_getSomeConfig);
        //  登录
        SocketClient.AddHandler(Route.connector_main_enter, SVR_ConnectorLoginBack);
        Proto.login_req tmp = new Proto.login_req();
        tmp.uid = PlayerInfo.uid;
        tmp.token = PlayerInfo.token;
        SocketClient.SendMsg(Route.connector_main_enter, tmp);
    }
    void OnConnectorClose(string msg)
    {
        CommonHandler.instance.StopListen();
        UIManager.instance.SetSomeInfo("连不上connector服务器");
    }


    private void OnDestroy()
    {
        SocketClient.OffOpen();
        SocketClient.OffClose();
        SocketClient.RemoveHandler(Route.connector_main_enter);
        SocketClient.RemoveHandler(Route.connector_main_getSomeConfig);
    }


}


/// <summary>
/// 本地历史登录账号列表
/// </summary>
[Serializable]
public class LocalRecordList
{
    public List<LocalRecord> records = new List<LocalRecord>();
}

[Serializable]
public class LocalRecord
{
    public string username;
    public string password;
}