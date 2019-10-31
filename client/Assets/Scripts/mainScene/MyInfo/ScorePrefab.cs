using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScorePrefab : MonoBehaviour
{
    public void Init(Proto.gameTypeScoreData data)
    {
        transform.Find("name").GetComponent<Text>().text = PlayerInfo.gameTypes[data.type].name;
        transform.Find("numA").GetComponent<Text>().text = data.all.ToString();
        transform.Find("numW").GetComponent<Text>().text = data.win.ToString();
        transform.Find("per").GetComponent<Text>().text = Mathf.FloorToInt(data.win * 100f / data.all) + "%";
        transform.Find("score").GetComponent<Text>().text = data.score.ToString();
    }
}
