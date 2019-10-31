using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyInfoPanel : MonoBehaviour
{
    public GameObject scorePrefab;
    public static MyInfoPanel instance = null;

    // Use this for initialization
    void Awake()
    {
        instance = this;
        InitHeadInfo();
        InitOtherInfo();
        InitScoreboard();
    }

    // 设置头像
    public void InitHeadInfo()
    {
        transform.Find("head").GetComponent<Image>().sprite = Util.GetSprite(ImgType.HeadImg, PlayerInfo.playerData.headId);
    }

    // 设置昵称等个人信息
    public void InitOtherInfo()
    {
        transform.Find("nickname/Text").GetComponent<Text>().text = PlayerInfo.playerData.nickname;
        transform.Find("sex/Image").GetComponent<Image>().sprite = Util.GetSprite(ImgType.SexImg, PlayerInfo.playerData.sex);
        transform.Find("signature/Text").GetComponent<Text>().text = PlayerInfo.playerData.signature;
        transform.Find("uid/uid").GetComponent<Text>().text = PlayerInfo.uid.ToString();
    }


    // 修改个人头像
    public void Btn_changeHeadImg()
    {
        UIManager.instance.ShowPanel(UIPanel.MainScene.InfoPanel.changeHeadImgPanel);
    }

    // 修改个人信息
    public void Btn_changeInfo()
    {
        UIManager.instance.ShowPanel(UIPanel.MainScene.InfoPanel.changeInfoPanel);
    }

    // 初始化游戏场次积分面板
    void InitScoreboard()
    {
        var gameData = PlayerInfo.playerData.gameData;
        if (gameData.Count == 0)
        {
            return;
        }
        Transform scoreParent = transform.Find("scoreboard/Viewport/Content");
        foreach(var one in gameData)
        {
            var obj = Instantiate(scorePrefab, scoreParent); ;
            obj.GetComponent<ScorePrefab>().Init(one);
        }
    }

    public void Btn_close()
    {
        Destroy(gameObject);
    }

}
