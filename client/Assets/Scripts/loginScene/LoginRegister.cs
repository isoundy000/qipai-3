using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginRegister : MonoBehaviour
{

    public InputField username;
    public InputField password;
    string tmpUsername = "";
    string tmpPassword = "";


    public void OnRegisterBtn()
    {
        if (!isValid())
        {
            return;
        }
        WWWForm tmp = new WWWForm();
        tmp.AddField("method", "register");
        tmp.AddField("username", tmpUsername);
        tmp.AddField("password", tmpPassword);
        WWWManager.instance.SendHttpRequest(LoginMgr.instance.GetLoginUrl(), tmp, OnRegisterBack);
    }

    void OnRegisterBack(int status, string msg)
    {
        Proto.httpRegOrLoginRsp data = JsonUtility.FromJson<Proto.httpRegOrLoginRsp>(msg);
        if (status != 0)
        {
            UIManager.instance.SetSomeInfo("连接超时！");
        }
        else if (data.code == 3)
        {
            UIManager.instance.SetSomeInfo("用户名已存在！");
        }
        else if (data.code == 0)
        {
            ResetLocalRecords();
            Debug.Log("注册成功:" + msg.ToString());
            LoginMgr.instance.ConnectToConnector(data.host, data.port, data.uid, data.token);
        }
        else
        {
            UIManager.instance.SetSomeInfo("未知错误！");
        }
    }

    public void OnLoginBtn()
    {
        if (!isValid())
        {
            return;
        }
        WWWForm tmp = new WWWForm();
        tmp.AddField("method", "login");
        tmp.AddField("username", tmpUsername);
        tmp.AddField("password", tmpPassword);
        WWWManager.instance.SendHttpRequest(LoginMgr.instance.GetLoginUrl(), tmp, OnLoginBack);
    }

    void OnLoginBack(int status, string msg)
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
            ResetLocalRecords();
            Debug.Log("登录成功:" + msg.ToString());
            LoginMgr.instance.ConnectToConnector(data.host, data.port, data.uid, data.token);
        }
        else
        {
            UIManager.instance.SetSomeInfo("未知错误！");
        }

    }

    public void OnBackBtn()
    {
        UIManager.instance.ShowPanel(UIPanel.LoginScene.fastLogin);
        Destroy(gameObject);
    }

    bool isValid()
    {
        tmpUsername = username.text.Trim();
        tmpPassword = password.text.Trim();
        if (tmpUsername.Contains(" "))
        {
            UIManager.instance.SetSomeInfo("帐号不可包含空格");
            return false;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(tmpUsername, @"^[\dA-Za-z]{6,12}$"))
        {
            UIManager.instance.SetSomeInfo("帐号为6-12位字母或数字");
            return false;
        }
        if (tmpPassword.Contains(" "))
        {
            UIManager.instance.SetSomeInfo("密码不可包含空格");
            return false;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(tmpPassword, @"^[\dA-Za-z]{6,12}$"))
        {
            UIManager.instance.SetSomeInfo("密码为6-12位字母或数字");
            return false;
        }
        return true;

    }

    void ResetLocalRecords()
    {
        string last = PlayerPrefs.GetString("loginRecords");
        LocalRecord one = new LocalRecord();
        one.username = tmpUsername;
        one.password = tmpPassword;
        if (last == "")
        {
            LocalRecordList recordListTmp = new LocalRecordList();
            recordListTmp.records.Add(one);
            PlayerPrefs.SetString("loginRecords", JsonUtility.ToJson(recordListTmp));
            return;
        }

        LocalRecordList recordList = JsonUtility.FromJson<LocalRecordList>(last);
        for (int i = 0; i < recordList.records.Count; i++)
        {
            if (recordList.records[i].username == tmpUsername)
            {
                recordList.records.RemoveAt(i);
                break;
            }
        }
        recordList.records.Insert(0, one);
        PlayerPrefs.SetString("loginRecords", JsonUtility.ToJson(recordList));
    }
}
