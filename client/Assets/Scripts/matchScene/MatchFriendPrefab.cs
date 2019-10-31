using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchFriendPrefab : MonoBehaviour
{
    int uid = 0;
    bool isToggleOn = false;

    public void Init(FriendData friend)
    {
        uid = friend.uid;
        name = friend.uid.ToString();
        Image sexImg = transform.Find("sex").GetComponent<Image>();
        sexImg.sprite = Util.GetSprite(ImgType.SexImg, friend.sex);
        sexImg.color = friend.sid == "" ? Color.black : Color.white;
        transform.Find("name").GetComponent<Text>().text = friend.nickname;
    }

    public void OnLineOrNot(bool isOnLine)
    {
        transform.Find("sex").GetComponent<Image>().color = isOnLine ? Color.white : Color.black;
    }

    public void Toggle_click(bool isOn)
    {
        if (isOn && !isToggleOn)
        {
            WaitPanelMgr.instance.ClickFriend(uid);
        }
        isToggleOn = isOn;
    }
}
