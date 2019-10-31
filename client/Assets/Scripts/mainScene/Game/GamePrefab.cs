using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePrefab : MonoBehaviour
{
    private Proto.gameInfo gameData = null;

    public void Init(Proto.gameInfo gameInfo)
    {
        gameData = gameInfo;
        gameObject.SetActive(true);
        transform.name = gameInfo.id.ToString();
        transform.Find("gameName").GetComponent<Text>().text = gameInfo.gameName;
        transform.Find("gameId").GetComponent<Text>().text = "ID: " + gameInfo.id;
        transform.Find("roleName").GetComponent<Text>().text = gameInfo.roleName;
        if (gameInfo.state == Proto.gameState.wait)
        {
            transform.Find("time1").GetComponent<Text>().text = "创建时间： " + gameInfo.createTime;
            transform.Find("time2").GetComponent<Text>().text = "开始时间： " + gameInfo.startTime;
        }
        else if (gameInfo.state == Proto.gameState.going)
        {
            transform.Find("time1").GetComponent<Text>().text = "开始时间： " + gameInfo.startTime;
            transform.Find("time2").GetComponent<Text>().text = "结束时间： " + gameInfo.endTime;
        }
        else
        {
            transform.Find("time1").GetComponent<Text>().text = "结束时间： " + gameInfo.endTime;
            transform.Find("time2").GetComponent<Text>().text = "关闭时间： " + gameInfo.closeTime;
        }
    }

    public void Btn_click()
    {
        GamePanel.instance.ShowGameInfo(gameData);
    }
}
