using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneReconnect : MonoBehaviour
{
    string[] infos = new string[] { "断线重连中", "断线重连中。", "断线重连中。。", "断线重连中。。。" };
    int nowIndex = 0;
    float infoTime = 0f;
    public Text infoText;
    public Text resultText;

    bool isFailed = false;
    float connectLateTime = 10f;
    const float Connect_Late_Time = 3f;
    int reconnectTimes = 0;

    bool isToLoginScene = true;  // 重连结果是网络超时，还是房间不存在了
    bool reconnectOk = false;

    // Use this for initialization
    void Start()
    {
        infoText.text = infos[nowIndex];
        SocketClient.AddHandler(Route.connector_main_reconnectEnter, SVR_ConnectorLoginBack);
        SocketClient.AddHandler(Route.info_game_enterTable, SVR_EnterTableBack);
        SocketClient.AddHandler(Route.onEnterTable, SVR_OnEnterTable);

        SocketClient.OnOpen(OnConnectorOpen);
        SocketClient.OnClose(OnConnectorClose);
        ReConnect();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFailed)
        {
            return;
        }

        infoTime += Time.deltaTime;
        if (infoTime > 0.3f)
        {
            infoTime = 0;
            infoText.text = infos[nowIndex];
            nowIndex++;
            nowIndex = nowIndex % infos.Length;
        }

        if (connectLateTime < Connect_Late_Time)
        {
            connectLateTime += Time.deltaTime;
            if (connectLateTime >= Connect_Late_Time)
            {
                ReConnect();
            }
        }
    }

    void ReconnectFailToLoginScene(bool _isToLogin)
    {
        isToLoginScene = _isToLogin;
        isFailed = true;
        reconnectOk = false;
        if (_isToLogin)
        {
            resultText.text = "重连失败";
            transform.Find("Button/Text").GetComponent<Text>().text = "重新登录";
        }
        else
        {
            resultText.text = "游戏已结束";
            transform.Find("Button/Text").GetComponent<Text>().text = "返回大厅";
        }
        infoText.text = "";
        transform.Find("Button").gameObject.SetActive(true);
    }

    public void Btn_Back()
    {
        if (isToLoginScene)
        {
            SceneManager.LoadScene(SceneNames.login);
        }
        else
        {
            SceneManager.LoadScene(SceneNames.main);
        }
    }

    void ReConnect()
    {
        reconnectTimes++;
        SocketClient.Connect(PlayerInfo.connectorHost, PlayerInfo.connectorPort);
    }

    void OnConnectorOpen(string msg)
    {
        Debug.Log("重连成功");
        Proto.login_req data = new Proto.login_req();
        data.uid = PlayerInfo.uid;
        data.token = PlayerInfo.token;
        SocketClient.SendMsg(Route.connector_main_reconnectEnter, data);
    }

    void OnConnectorClose(string msg)
    {
        connectLateTime = 0;
        if (reconnectTimes == 4)
        {
            ReconnectFailToLoginScene(true);
        }
    }

    void SVR_ConnectorLoginBack(string msg)
    {
        Debug.Log(msg);
        Proto.EnterServer data = JsonUtility.FromJson<Proto.EnterServer>(msg);
        if (data.code != 0)
        {
            ReconnectFailToLoginScene(true);
            return;
        }
        PlayerInfo.InitPlayer(data);
        if (PlayerInfo.playerData.gameId == 0)  // 已经不在房间里了
        {
            ReconnectFailToLoginScene(false);
            return;
        }
        SocketClient.SendMsg(Route.info_game_enterTable);
    }

    void SVR_EnterTableBack(string msg) // 进入房间失败
    {
        ReconnectFailToLoginScene(false);
    }

    // 重连进入桌子成功
    void SVR_OnEnterTable(string msg)
    {
        Debug.Log(msg);
        reconnectOk = true;
        PlayerInfo.gameInfo = JsonUtility.FromJson<Proto.OnEnterTable>(msg).data;
        GameCommon.instance.ReconnectSuccess();
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        SocketClient.RemoveHandler(Route.connector_main_reconnectEnter);
        SocketClient.RemoveHandler(Route.info_game_enterTable);
        SocketClient.RemoveHandler(Route.onEnterTable);
        SocketClient.OffOpen();
        SocketClient.OffClose();
        if (reconnectOk)
        {
            GameCommon.instance.WatchSocket();
        }
    }
}
