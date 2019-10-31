using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XiangQiMain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UIManager.instance.ShowPanel(UIPanel.GamePanel.xiangQiPanel);
    }

}
