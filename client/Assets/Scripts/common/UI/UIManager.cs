using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    public static UIManager instance = null;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 加载某 UIPanel
    /// </summary>
    /// <param name="panelPath"></param>
    /// <returns></returns>
    public Transform ShowPanel(string panelPath)
    {
        Transform tmpTran = Instantiate(Resources.Load<Transform>(panelPath));
        tmpTran.SetParent(transform, false);
        return tmpTran;
    }

    /// <summary>
    /// 弹出提示框
    /// </summary>
    /// <param name="info"></param>
    public void SetSomeInfo(string info)
    {
        ShowPanel(UIPanel.someInfo).GetComponent<SomeInfo>().SetInfo(info);
    }

    /// <summary>
    /// 弹出文字提示
    /// </summary>
    /// <param name="info"></param>
    public void SetTileInfo(string info, GameObject attachedObj = null)
    {
        ShowPanel(UIPanel.tileInfo).GetComponent<TileInfo>().Init(info, attachedObj);
    }
}
