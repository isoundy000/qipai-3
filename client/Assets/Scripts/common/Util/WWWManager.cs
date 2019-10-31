using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Security.Cryptography;
using System.Text;

public class WWWManager : MonoBehaviour
{

    public static WWWManager instance = null;
    private Action<int, string> httpCallBack = null;
    Transform tmpLoadingObj = null;
    string url = "";
    WWWForm form = null;


    void Awake()
    {
        instance = this;
    }

    //是否联网
    bool hasNetwork()
    {
        return true;
        if (Application.internetReachability == NetworkReachability.NotReachable)//未联网
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    //发送http请求
    public void SendHttpRequest(string tmpUrl, WWWForm tmpForm, Action<int, string> handler)
    {
        if (!hasNetwork())
        {
            UIManager.instance.SetSomeInfo("请检查网络连接！");
            return;
        }
        url = tmpUrl;
        form = tmpForm;
        httpCallBack = handler;
        StartCoroutine(Send());
    }

    //超时，停止http请求
    public void TimeoutStopHttpRequest()
    {
        StopAllCoroutines();
        httpCallBack(-1, null);
    }

    IEnumerator Send()
    {
        WWW www;
        if (form == null)
        {
            www = new WWW(url);
        }
        else
        {
            www = new WWW(url, form);
        }

        SetLoadingPanel(true);
        yield return www;
        SetLoadingPanel(false);
        if (www.error != null)
        {
            httpCallBack(-2, null);
        }
        else
        {
            httpCallBack(0, www.text);
        }
        www.Dispose();
    }

    void SetLoadingPanel(bool isOn)
    {
        if (isOn)
        {
            if (!tmpLoadingObj)
            {
                tmpLoadingObj = UIManager.instance.ShowPanel(UIPanel.wwwLoading);
            }
        }
        else
        {
            if (tmpLoadingObj != null)
            {
                Destroy(tmpLoadingObj.gameObject);
            }
        }
    }

    //unicode转中文
    //public string Unicode_china(string str)
    //{
    //    return System.Text.RegularExpressions.Regex.Unescape(str);
    //}


    ////格式化数据
    //public string Sha1(string str, string code)
    //{

    //    TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
    //    string timeNow = Convert.ToInt64(ts.TotalSeconds).ToString();

    //    str = str + "," + timeNow;

    //    SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
    //    byte[] str1 = Encoding.Default.GetBytes(str + "," + code);
    //    byte[] str2 = sha1.ComputeHash(str1);
    //    string str3 = BitConverter.ToString(str2).Replace("-", "").ToLower();

    //    str = str + "," + str3;
    //    byte[] bytes = Encoding.Default.GetBytes(str);
    //    str = Convert.ToBase64String(bytes);
    //    return str;
    //}
}
