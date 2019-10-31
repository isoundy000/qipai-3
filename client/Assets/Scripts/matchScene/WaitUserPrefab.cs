using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitUserPrefab : MonoBehaviour
{
    private int uid = 0;

    public void Init(Proto.WaitUserInfoProto userInfo)
    {
        uid = userInfo.uid;
        transform.name = uid.ToString();
        transform.Find("headImg").GetComponent<Image>().sprite = Util.GetSprite(ImgType.HeadImg, userInfo.headId);
        Text nickname = transform.Find("nickname").GetComponent<Text>();
        nickname.text = userInfo.nickname;
        transform.Find("score").GetComponent<Text>().text = userInfo.score.ToString();
        transform.Find("invite").gameObject.SetActive(false);
        GameObject kickObj = transform.Find("kick").gameObject;
        GameObject addObj = transform.Find("add").gameObject;
        if (uid == PlayerInfo.uid)
        {
            nickname.color = Color.red;
            kickObj.SetActive(false);
            addObj.SetActive(false);
            Transform stateText = transform.Find("state");
            if (userInfo.isMaster)
            {
                WaitPanelMgr.instance.SetFuncBtn(WaitPanelMgr.WaitStatus.startMatch);
                stateText.GetComponent<Image>().color = new Color32(58, 213, 253, 255);
                stateText.Find("Text").GetComponent<Text>().text = "房主";
            }
            else if (userInfo.isReady)
            {
                WaitPanelMgr.instance.SetFuncBtn(WaitPanelMgr.WaitStatus.cancelReady);
                stateText.GetComponent<Image>().color = new Color32(79, 255, 116, 255);
                stateText.Find("Text").GetComponent<Text>().text = "已准备";
            }
            else
            {
                WaitPanelMgr.instance.SetFuncBtn(WaitPanelMgr.WaitStatus.beReady);
                stateText.GetComponent<Image>().color = new Color32(255, 141, 141, 255);
                stateText.Find("Text").GetComponent<Text>().text = "未准备";
            }
        }
        else
        {
            nickname.color = Color.black;
            if (WaitPanelMgr.instance.masterId == PlayerInfo.uid)
            {
                kickObj.SetActive(true);
            }
            else
            {
                kickObj.SetActive(false);
            }
            if (PlayerInfo.friends.ContainsKey(uid))
            {
                addObj.SetActive(false);
            }
            else
            {
                addObj.SetActive(true);
            }
            Transform stateText = transform.Find("state");
            if (userInfo.isMaster)
            {
                stateText.GetComponent<Image>().color = new Color32(58, 213, 253, 255);
                stateText.Find("Text").GetComponent<Text>().text = "房主";
            }
            else if (userInfo.isReady)
            {
                stateText.GetComponent<Image>().color = new Color32(79, 255, 116, 255);
                stateText.Find("Text").GetComponent<Text>().text = "已准备";
            }
            else
            {
                stateText.GetComponent<Image>().color = new Color32(255, 141, 141, 255);
                stateText.Find("Text").GetComponent<Text>().text = "未准备";
            }
        }


    }

    public void ReadyOrNot(bool isReady)
    {
        Transform stateText = transform.Find("state");
        if (isReady)
        {
            if (uid == PlayerInfo.uid)
            {
                WaitPanelMgr.instance.SetFuncBtn(WaitPanelMgr.WaitStatus.cancelReady);
            }
            stateText.GetComponent<Image>().color = new Color32(79, 255, 116, 255);
            stateText.Find("Text").GetComponent<Text>().text = "已准备";
        }
        else
        {
            if (uid == PlayerInfo.uid)
            {
                WaitPanelMgr.instance.SetFuncBtn(WaitPanelMgr.WaitStatus.beReady);
            }
            stateText.GetComponent<Image>().color = new Color32(255, 141, 141, 255);
            stateText.Find("Text").GetComponent<Text>().text = "未准备";
        }

    }

    public void BeFriend()
    {
        transform.Find("add").gameObject.SetActive(false);
    }

    public void Leave()
    {
        uid = 0;
        transform.Find("invite").gameObject.SetActive(true);
        transform.name = "empty";
    }

    public void Btn_Invite()
    {
        WaitPanelMgr.instance.InviteFriendToGame();
    }
    public void Btn_AddFriend()
    {
        WaitPanelMgr.instance.Btn_AddFriend(uid);
    }
    public void Btn_Kick()
    {
        WaitPanelMgr.instance.KickPlayer(uid);
    }
}
