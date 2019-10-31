using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTypePrefab : MonoBehaviour
{

    private int gameType = 0;
    private bool isNowOn = false;
    public void Init(int _gameType, string _gamename)
    {
        gameType = _gameType;
        transform.Find("Text").GetComponent<Text>().text = _gamename;
    }

    public void Toggle_Click(bool isOn)
    {
        if (isOn && !isNowOn)
        {
            GamePanel.instance.OnGameTypeToggleClick(gameType);
        }
        isNowOn = isOn;
    }
}
