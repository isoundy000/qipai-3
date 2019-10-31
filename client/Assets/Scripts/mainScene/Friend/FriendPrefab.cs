using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendPrefab : MonoBehaviour
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

    public void OnInfoChanged(Proto.FriendInfo friend)
    {
        Image sexImg = transform.Find("sex").GetComponent<Image>();
        sexImg.color = friend.sid != "" ? Color.white : Color.black;
        if (friend.sex > 0)
        {
            sexImg.sprite = Util.GetSprite(ImgType.SexImg, friend.sex);
        }
        if (friend.nickname != "")
        {
            transform.Find("name").GetComponent<Text>().text = friend.nickname;
        }
        if (friend.signature != "" && isToggleOn)
        {
            FriendPanel.instance.SetNowUid(uid);
        }
    }

    public void Toggle_click(bool isOn)
    {
        if (isOn && !isToggleOn)
        {
            FriendPanel.instance.SetNowUid(uid);
        }
        isToggleOn = isOn;
    }
}
