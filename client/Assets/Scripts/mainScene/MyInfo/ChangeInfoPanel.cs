using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;

public class ChangeInfoPanel : MonoBehaviour
{

    public InputField nicknameInput;
    public InputField signatureInput;
    public Toggle sexOneToggle;
    public Toggle sexTwoToggle;
    private int sex = 1;

    // Use this for initialization
    void Start()
    {
        nicknameInput.placeholder.gameObject.GetComponent<Text>().text = PlayerInfo.playerData.nickname;
        signatureInput.placeholder.gameObject.GetComponent<Text>().text = PlayerInfo.playerData.signature;
        sexOneToggle.GetComponent<Image>().sprite = Util.GetSprite(ImgType.SexImg, 1);
        sexTwoToggle.GetComponent<Image>().sprite = Util.GetSprite(ImgType.SexImg, 2);
        if (PlayerInfo.playerData.sex == 1)
        {
            sexOneToggle.isOn = true;
        }
        else
        {
            sexTwoToggle.isOn = true;
        }
        SocketClient.AddHandler(Route.info_main_changeMyInfo, SVR_changeMyInfoBack);
    }

    public void Btn_nicknameReset()
    {
        nicknameInput.text = "";
    }
    public void Btn_signatureReset()
    {
        signatureInput.text = "";
    }
    public void Toggle_One(bool isOn)
    {
        if (isOn)
        {
            sex = 1;
        }
    }
    public void Toggle_Two(bool isOn)
    {
        if (isOn)
        {
            sex = 2;
        }
    }

    public void Btn_Change()
    {
        string tmpName = nicknameInput.text.Trim();
        if (tmpName != "")
        {
            int len = Encoding.UTF8.GetBytes(tmpName).Length;
            if (len < 5 || len > 12)
            {
                UIManager.instance.SetTileInfo("昵称长度为5-12个字节", gameObject);
                return;
            }
            Regex reg = new Regex(@"^[\u4e00-\u9fa5a-zA-Z0-9]+$");
            Match m = reg.Match(tmpName);
            if (!m.Success)
            {
                UIManager.instance.SetTileInfo("昵称不可包含特殊字符", gameObject);
                return;
            }

            string headStr = tmpName.Substring(0, 2);
            bool isInt = int.TryParse(tmpName.Substring(2), out len);
            if (headStr == "豆豆" && isInt)
            {
                UIManager.instance.SetTileInfo("昵称不可为'豆豆123'格式", gameObject);
                return;
            }
        }

        Proto.changeMyInfo tmpJson = new Proto.changeMyInfo();
        bool needSend = false;
        if (tmpName != "" && tmpName != PlayerInfo.playerData.nickname)
        {
            tmpJson.nickname = tmpName;
            needSend = true;
        }
        if (sex != PlayerInfo.playerData.sex)
        {
            tmpJson.sex = sex;
            needSend = true;
        }
        string tmpSignature = signatureInput.text.Trim();
        if (tmpSignature != "" && tmpSignature != PlayerInfo.playerData.signature)
        {
            tmpJson.signature = tmpSignature;
            needSend = true;
        }
        if (!needSend)
        {
            Btn_Close();
            return;
        }
        SocketClient.SendMsg(Route.info_main_changeMyInfo, tmpJson);
    }

    void SVR_changeMyInfoBack(string _msg)
    {
        Proto.changeMyInfo msg = JsonUtility.FromJson<Proto.changeMyInfo>(_msg);
        if (msg.code == 2)
        {
            UIManager.instance.SetTileInfo("昵称已存在", gameObject);
        }
        else if (msg.code == 0)
        {
            if (msg.sex > 0)
            {
                PlayerInfo.playerData.sex = msg.sex;
            }
            if (msg.nickname != "")
            {
                PlayerInfo.playerData.nickname = msg.nickname;
            }
            if (msg.signature != "")
            {
                PlayerInfo.playerData.signature = msg.signature;
            }
            GamePanel.instance.InitPlayerInfo();
            MyInfoPanel.instance.InitOtherInfo();
            Btn_Close();
        }
    }

    public void Btn_Close()
    {
        Destroy(gameObject);
    }

    public void OnDestroy()
    {
        SocketClient.RemoveHandler(Route.info_main_changeMyInfo);
    }
}
