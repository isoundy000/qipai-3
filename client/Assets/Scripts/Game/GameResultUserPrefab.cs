using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUserPrefab : MonoBehaviour
{
    private int uid;
    public void Init(Proto.GameOverUserInfo info)
    {
        uid = info.uid;
        transform.Find("sex").GetComponent<Image>().sprite = Util.GetSprite(ImgType.SexImg, info.sex);
        transform.Find("name").GetComponent<Text>().text = info.name;
        transform.Find("score").GetComponent<Text>().text = info.score + "  " + GetColorText(info.scoreAdd);
        transform.Find("douzi").GetComponent<Text>().text = info.douzi + "  " + GetColorText(info.douziAdd);
        if (info.uid == PlayerInfo.uid || PlayerInfo.friends.ContainsKey(info.uid))
        {
            transform.Find("friend").gameObject.SetActive(false);
        }
    }

    public void Btn_addFriend()
    {
        Proto.Uidsid msg = new Proto.Uidsid();
        msg.uid = uid;
        SocketClient.SendMsg(Route.info_friend_askFriend, msg);
        UIManager.instance.SetTileInfo("好友请求已发送");
    }

    string GetColorText(int num)
    {
        if (num >= 0)
        {
            return "<color=lime>+" + num + "</color>";
        }
        else
        {
            return "<color=red>" + num + "</color>";
        }
    }
}
