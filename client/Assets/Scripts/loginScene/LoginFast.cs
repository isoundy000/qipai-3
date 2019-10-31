using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginFast : MonoBehaviour
{

    private LocalRecordList localRecords = null;
    int nowRecordIndex = 0;

    void Start()
    {
        getLocalRecords();
    }


    // 获取本地登录记录
    void getLocalRecords()
    {
        string records = PlayerPrefs.GetString("loginRecords");
        if (records != "")
        {
            Dropdown recordDropdown = transform.Find("record").GetComponent<Dropdown>();
            recordDropdown.ClearOptions();
            localRecords = JsonUtility.FromJson<LocalRecordList>(records);
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            for (int i = 0; i < localRecords.records.Count; i++)
            {
                Dropdown.OptionData data = new Dropdown.OptionData();
                data.text = localRecords.records[i].username;
                options.Add(data);
            }
            recordDropdown.AddOptions(options);
            recordDropdown.value = 0;
        }
    }
    // 切换登录用户
    public void OnRecordChanged(int index)
    {
        nowRecordIndex = index;
    }

    //快速登录按钮
    public void OnFastLoginBtn()
    {
        if (localRecords == null)
        {
            UIManager.instance.SetSomeInfo("当前没有可登录用户！");
            return;
        }
        WWWForm tmp = new WWWForm();
        tmp.AddField("method", "login");
        tmp.AddField("username", localRecords.records[nowRecordIndex].username);
        tmp.AddField("password", localRecords.records[nowRecordIndex].password);
        WWWManager.instance.SendHttpRequest(LoginMgr.instance.GetLoginUrl(), tmp, fastLoginBack);
    }

    void fastLoginBack(int status, string msg)
    {
        Proto.httpRegOrLoginRsp data = JsonUtility.FromJson<Proto.httpRegOrLoginRsp>(msg);
        if (status != 0)
        {
            UIManager.instance.SetSomeInfo("请求超时！");
        }
        else if (data.code == 1)
        {
            UIManager.instance.SetSomeInfo("用户名不存在！");
        }
        else if (data.code == 2)
        {
            UIManager.instance.SetSomeInfo("密码错误！");
        }
        else if (data.code == 0)
        {
            Debug.Log("登录成功:" + msg.ToString());
            ResetLocalRecords();
            LoginMgr.instance.ConnectToConnector(data.host, data.port, data.uid, data.token);
        }
        else
        {
            UIManager.instance.SetSomeInfo("未知错误！");
        }
    }


    // 显示注册登录界面
    public void OnShowRegisterLoginPanelBtn()
    {
        UIManager.instance.ShowPanel(UIPanel.LoginScene.registerAndLogin);
        Destroy(gameObject);
    }

    void ResetLocalRecords()
    {
        if (nowRecordIndex == 0)
        {
            return;
        }
        LocalRecord one = localRecords.records[nowRecordIndex];
        localRecords.records.RemoveAt(nowRecordIndex);
        localRecords.records.Insert(0, one);
        PlayerPrefs.SetString("loginRecords", JsonUtility.ToJson(localRecords));
        getLocalRecords();
    }
}
