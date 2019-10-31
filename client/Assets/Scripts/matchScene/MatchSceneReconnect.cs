using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchSceneReconnect : MonoBehaviour
{
    string[] infos = new string[] { "断线重连中", "断线重连中。", "断线重连中。。", "断线重连中。。。" }; // 重连提醒文字
    int nowIndex = 0;   // 文字序号
    float infoTime = 0f;    // 文字显示时间计时
    public Text infoText;

    int reconnectTimes = 0;
    bool isFailed = false;
    float connectLateTime = 10f;
    const float Connect_Late_Time = 3f; // 重连间隔


    // Use this for initialization
    void Start()
    {
        infoText.text = infos[nowIndex];
        SocketClient.AddHandler(Route.connector_main_reconnectEnter, SVR_reconnectEntryBack);
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


    public void Btn_ReLogin()
    {
        SceneManager.LoadScene(SceneNames.login);
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
            ReconnectFail();
        }
    }

    void ReconnectFail()
    {
        isFailed = true;
        infoText.text = "重连失败";
        transform.Find("Button").gameObject.SetActive(true);
    }

    void SVR_reconnectEntryBack(string msg)
    {
        Debug.Log(msg);
        Proto.EnterServer data = JsonUtility.FromJson<Proto.EnterServer>(msg);
        if (data.code != 0)
        {
            ReconnectFail();
            return;
        }
        PlayerInfo.InitPlayer(data);
        SceneManager.LoadScene(SceneNames.main);
    }

    private void OnDisable()
    {
        SocketClient.RemoveHandler(Route.connector_main_reconnectEnter);
        SocketClient.OffOpen();
        SocketClient.OffClose();
    }
}
