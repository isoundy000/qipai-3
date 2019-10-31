using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WuZiQiMain : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        UIManager.instance.ShowPanel(UIPanel.GamePanel.wuZiQiPanel);
        //UIManager.instance.ShowPanel(UIPanel.GamePanel.chatInRoomPanel);
    }


}
