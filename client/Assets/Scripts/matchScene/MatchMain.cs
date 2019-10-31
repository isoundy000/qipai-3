using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchMain : MonoBehaviour
{
    public static MatchMain instance = null;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        SocketClient.OnClose(SVR_onClose);
        UIManager.instance.ShowPanel(UIPanel.MatchScene.waitPanel);
    }


    void SVR_onClose(string msg)
    {
        UIManager.instance.ShowPanel(UIPanel.MatchScene.reconnectPanel);
    }

    private void OnDestroy()
    {
        SocketClient.OffClose();
    }
}
