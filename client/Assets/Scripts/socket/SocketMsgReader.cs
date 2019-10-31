using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SocketMsgReader : MonoBehaviour {

    public static SocketMsgReader instance = null;

	// Use this for initialization
	void Awake () {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
	}
	

	// Update is called once per frame
	void Update () {
        SocketClient.ReadMsg();
	}

    private void OnApplicationQuit()
    {
        SocketClient.DisConnect();
    }
}
