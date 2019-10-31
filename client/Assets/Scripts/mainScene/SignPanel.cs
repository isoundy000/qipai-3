using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignPanel : MonoBehaviour
{

    float dir = 1;  // 加速度方向
    float rotA = -50f;  // 摆动加速度
    float lastSpeed = 50f;    // 初始速度
    float nowDelta = 0;     // 当前与初始的差值

    Transform nowAwardTrsm = null;   // 当前可领取奖励的物体
                                     // Use this for initialization
    void Start()
    {
        Transform awardParent = transform.Find("award");
        Transform tmpTrsm;
        for (int i = 1; i < PlayerInfo.playerData.loginDays; i++)
        {
            tmpTrsm = awardParent.Find(i.ToString());
            tmpTrsm.GetComponent<Image>().color = new Color32(155, 155, 155, 255);
        }
        if (PlayerInfo.playerData.ifGetAward == 1)
        {
            tmpTrsm = awardParent.Find(PlayerInfo.playerData.loginDays.ToString());
            tmpTrsm.GetComponent<Image>().color = new Color32(155, 155, 155, 255);
        }
        else
        {
            nowAwardTrsm = awardParent.Find(PlayerInfo.playerData.loginDays.ToString());
        }

        SocketClient.AddHandler(Route.info_main_sign, SVR_signBack);
    }

    void Update()
    {
        if (nowAwardTrsm == null)
        {
            return;
        }

        float nowSpeed = lastSpeed + dir * rotA * Time.deltaTime;
        float rotDelta = nowSpeed * Time.deltaTime;
        lastSpeed = nowSpeed;
        nowDelta += rotDelta;
        nowAwardTrsm.Rotate(0, 0, rotDelta);
        if (dir == 1)
        {
            if (nowDelta < 0)
            {
                dir = -1;
            }
        }
        else if (nowDelta > 0)
        {
            dir = 1;
        }
    }

    public void Btn_back()
    {
        Destroy(gameObject);
    }
    public void Btn_getSignAward()
    {
        if (PlayerInfo.playerData.ifGetAward == 1)
        {
            UIManager.instance.SetSomeInfo("今日已领过奖了哦！");
            return;
        }
        SocketClient.SendMsg(Route.info_main_sign);
    }

    void SVR_signBack(string _msg)
    {
        Proto.JustCode msg = JsonUtility.FromJson<Proto.JustCode>(_msg);
        if (msg.code == 0)
        {
            PlayerInfo.playerData.ifGetAward = 1;
            nowAwardTrsm.GetComponent<Image>().color = new Color32(155, 155, 155, 255);
            nowAwardTrsm.localEulerAngles = new Vector3(0, 0, 0);
            nowAwardTrsm = null;
            UIManager.instance.SetSomeInfo("签到领奖成功！");
        }
    }

    private void OnDisable()
    {
        SocketClient.RemoveHandler(Route.info_main_sign);
    }
}
