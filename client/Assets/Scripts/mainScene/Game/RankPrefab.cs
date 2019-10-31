using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankPrefab : MonoBehaviour
{
    private int uid;
    public void Init(int rank, Proto.RoleRankInfo info)
    {
        transform.Find("rank").GetComponent<Text>().text = rank.ToString();
        transform.Find("username").GetComponent<Text>().text = info.name;
        transform.Find("score").GetComponent<Text>().text = info.score.ToString();
        transform.Find("sex").GetComponent<Image>().sprite = Util.GetSprite(ImgType.SexImg, info.sex);
        uid = info.uid;
        if (PlayerInfo.uid == uid)
        {
            transform.Find("add").gameObject.SetActive(false);
            transform.GetComponent<Image>().color = new Color32(42, 206, 255, 255);
        }
        else if (PlayerInfo.friends.ContainsKey(uid))
        {
            transform.Find("add").gameObject.SetActive(false);
        }

    }

    public void Btn_add()
    {
        GameInfoPanel.instance.WantAddFriend(uid);
    }
}
