using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStateToggle : MonoBehaviour
{
    public Proto.gameState gameState = Proto.gameState.wait;
    private bool isNowOn = false;

    public void Toggle_Click(bool isOn)
    {
        if (isOn && !isNowOn)
        {
            GamePanel.instance.OnGameStateToggleClick(gameState);
        }
        isNowOn = isOn;
    }
}
