using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailPanel : MonoBehaviour
{

    public static MailPanel instance;

    public Text senderText;
    public Text topicText;
    public Text expireTimeText;
    public Text contentText;
    public GameObject responseBtn;
    public Button delBtn;
    public Button getAwardBtn;

    public Sprite mailReadImg;
    public Sprite boxOpenImg;
    public Transform mailPrefab;
    public Transform mailParent;
    private ToggleGroup mailToggleGroup;
    public Transform awardPrefab;
    public Transform awardParent;

    GameObject nowMailObj = null;
    Proto.MailData nowMail = null;

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        NoMailSet();
        mailToggleGroup = mailParent.GetComponent<ToggleGroup>();

        foreach (Proto.MailData mail in PlayerInfo.mails)
        {
            NewMail(mail);
        }

        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onNewMail, SVR_onNewMail);
        SocketClient.AddHandler(Route.info_mail_readMail, SVR_readMailBack);
        SocketClient.AddHandler(Route.info_mail_delMail, SVR_delMailBack);
        SocketClient.AddHandler(Route.info_mail_getMailAward, SVR_getMailAwardBack);
    }

    void SVR_onNewMail(object mail)
    {
        NewMail(mail as Proto.MailData);
    }

    // 阅读邮件的回调
    void SVR_readMailBack(string _msg)
    {
        Proto.ReadMailRsp msg = JsonUtility.FromJson<Proto.ReadMailRsp>(_msg);
        Proto.MailData mail = FindMail(msg.id);
        if (mail == null)
        {
            return;
        }
        if (msg.code == 0)
        {
            ChangeMailStatus(mail, 1);
        }
        else if (msg.code == 1)
        {
            DeleteMail(mail);
        }
    }

    // 删除邮件的回调
    void SVR_delMailBack(string _msg)
    {
        Proto.ReadMailRsp msg = JsonUtility.FromJson<Proto.ReadMailRsp>(_msg);
        Proto.MailData mail = FindMail(msg.id);
        if (mail == null)
        {
            return;
        }
        DeleteMail(mail);

    }


    // 邮件领奖的回调
    void SVR_getMailAwardBack(string _msg)
    {
        Proto.ReadMailRsp msg = JsonUtility.FromJson<Proto.ReadMailRsp>(_msg);
        Proto.MailData mail = FindMail(msg.id);
        if (mail == null)
        {
            return;
        }
        if (msg.code == 0)
        {
            getAwardBtn.interactable = false;
            ChangeMailStatus(mail, 2);
            UIManager.instance.SetTileInfo("邮件奖励领取成功");
        }
        else if (msg.code == 1)
        {
            DeleteMail(mail);
        }

    }

    // 删除邮件
    void DeleteMail(Proto.MailData mail)
    {
        PlayerInfo.mails.Remove(mail);
        if (nowMail == mail)
        {
            nowMail = null;
            Destroy(nowMailObj);
            NoMailSet();
        }
        else
        {
            Transform mailObj = mailParent.Find(mail.id.ToString());
            if (mailObj != null)
            {
                Destroy(mailObj.gameObject); ;
            }
        }
    }

    // 邮件状态改变
    void ChangeMailStatus(Proto.MailData mail, int tmpStatus)
    {
        mail.status = tmpStatus;
        if (nowMail == mail)
        {
            nowMailObj.GetComponent<MailPrefab>().Init(mail);
        }
        else
        {
            Transform mailObj = mailParent.Find(mail.id.ToString());
            if (mailObj != null)
            {
                mailObj.GetComponent<MailPrefab>().Init(mail);
            }
        }
    }


    // 当没有邮件被选中时，清除显示
    void NoMailSet()
    {
        if(nowMail != null)
        {
            return;
        }
        nowMail = null;
        nowMailObj = null;
        senderText.text = "";
        topicText.text = "";
        expireTimeText.text = "";
        contentText.text = "";
        delBtn.gameObject.SetActive(false);
        getAwardBtn.gameObject.SetActive(false);
        responseBtn.SetActive(false);
        InitMailAward(null);
    }


    // 重置邮件奖励
    void InitMailAward(List<Proto.ItemData> award)
    {
        for (int i = 0, l = awardParent.childCount; i < l; i++)
        {
            Destroy(awardParent.GetChild(i).gameObject);
        }

        if (award == null)
        {
            return;
        }

        for (int i = 0; i < award.Count; i++)
        {
            Transform tmpTran = Instantiate(awardPrefab, awardParent, false);
            tmpTran.Find("Image").GetComponent<Image>().sprite = Util.GetSprite(ImgType.ItemImg, award[i].id);
            tmpTran.Find("Text").GetComponent<Text>().text = "x" + award[i].num;
        }
    }

    // 创建一封邮件
    public void NewMail(Proto.MailData mail)
    {
        Transform tmpTran = Instantiate(mailPrefab);
        tmpTran.SetParent(mailParent, false);
        tmpTran.GetComponent<MailPrefab>().Init(mail);
        tmpTran.GetComponent<Toggle>().group = mailToggleGroup;
    }

    // 点击阅读邮件
    public void OnMailToggleClick(GameObject mailObj, Proto.MailData mail)
    {
        nowMailObj = mailObj;
        nowMail = mail;
        senderText.text = mail.sendName;
        topicText.text = mail.topic;
        expireTimeText.text = "到期时间：" + mail.expireTime;
        contentText.text = mail.content;
        delBtn.gameObject.SetActive(true);

        if (mail.sendUid == 0)
        {
            responseBtn.SetActive(false);
        }
        else
        {
            responseBtn.SetActive(true);
        }

        if (mail.items.Count == 0)
        {
            getAwardBtn.gameObject.SetActive(false);
            InitMailAward(null);
        }
        else
        {
            getAwardBtn.gameObject.SetActive(true);
            if (mail.status == 2)
            {
                getAwardBtn.interactable = false;
            }
            else
            {
                getAwardBtn.interactable = true;
            }
            InitMailAward(mail.items);

        }

        if (mail.status == 0)
        {
            Proto.MailReq tmp = new Proto.MailReq();
            tmp.id = mail.id;
            tmp.uid = mail.uid;
            SocketClient.SendMsg(Route.info_mail_readMail, tmp);
        }
    }


    // 回复邮件的按钮
    public void Btn_response()
    {
        if (nowMailObj == null)
        {
            return;
        }
        Transform tmpTrsm = UIManager.instance.ShowPanel(UIPanel.MainScene.sendMailPanel);
        tmpTrsm.GetComponent<MailSendPanel>().Init(nowMail.sendUid, nowMail.sendName);
    }

    // 删除邮件按钮
    public void Btn_del()
    {
        if (nowMailObj != null)
        {
            Proto.MailReq tmp = new Proto.MailReq();
            tmp.id = nowMail.id;
            tmp.uid = nowMail.uid;
            SocketClient.SendMsg(Route.info_mail_delMail, tmp);
        }
    }

    // 领取邮件奖励按钮
    public void Btn_getAward()
    {
        if (nowMailObj == null)
        {
            return;
        }
        if (nowMail.status == 2 || nowMail.items.Count == 0)
        {
            return;
        }
        Proto.MailReq tmp = new Proto.MailReq();
        tmp.id = nowMail.id;
        tmp.uid = nowMail.uid; SocketClient.SendMsg(Route.info_mail_getMailAward, tmp);
    }

    public void Btn_back()
    {
        Destroy(gameObject);
    }

    private Proto.MailData FindMail(int id)
    {
        foreach (var mail in PlayerInfo.mails)
        {
            if (mail.id == id)
            {
                return mail;
            }
        }
        return null;
    }

    public void OnDestroy()
    {
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onNewMail, SVR_onNewMail);
        SocketClient.RemoveHandler(Route.info_mail_readMail);
        SocketClient.RemoveHandler(Route.info_mail_delMail);
        SocketClient.RemoveHandler(Route.info_mail_getMailAward);
    }
}
