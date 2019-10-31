using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameResultPanel : MonoBehaviour
{
    public Text resText;
    public GameObject gameOverUserPrefab;
    public Transform userParent;
    public void InitResult(Proto.GameOverResult res)
    {
        if (res.isGameClosed)
        {
            transform.Find("gameCloseWarn").gameObject.SetActive(true);
        }
        if (res.winUids.Count == 0)
        {
            transform.Find("res").GetComponent<Text>().text = "平局";
        }
        else if (res.winUids.Contains(PlayerInfo.uid))
        {
            transform.Find("res").GetComponent<Text>().text = "胜利";
        }
        else
        {
            transform.Find("res").GetComponent<Text>().text = "失败";
        }

        foreach (var one in res.userList)
        {
            Instantiate(gameOverUserPrefab, userParent).GetComponent<GameResultUserPrefab>().Init(one);
        }
    }

    public void Btn_Yes()
    {
        if (PlayerInfo.waitPlayers.Count > 0)
        {
            SceneManager.LoadScene(SceneNames.match);
        }
        else
        {
            SceneManager.LoadScene(SceneNames.main);
        }
    }
}
