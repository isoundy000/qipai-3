using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendAsk : MonoBehaviour
{
    int friendUid = 0;
    public void Init(Proto.OnAskFriend msg)
    {
        friendUid = msg.uid;
        transform.Find("nickname").GetComponent<Text>().text = msg.nickname;
        transform.Find("sex").GetComponent<Image>().sprite = Util.GetSprite(ImgType.SexImg, msg.sex);
    }

    public void Btn_Close()
    {
        Destroy(gameObject);
    }

    public void Btn_Yes()
    {
        Proto.Uidsid msg = new Proto.Uidsid();
        msg.uid = friendUid;
        SocketClient.SendMsg(Route.info_friend_agreeFriend, msg);
        Btn_Close();
    }
}
