using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailPrefab : MonoBehaviour
{

    Proto.MailData mail;
    bool isToggleOn = false;

    public void Init(Proto.MailData _mail)
    {
        mail = _mail;
        gameObject.name = mail.id.ToString();
        transform.Find("sender").GetComponent<Text>().text = mail.sendName;
        transform.Find("time").GetComponent<Text>().text = mail.createTime;
        if (mail.status != 0)
        {
            transform.Find("open").GetComponent<Image>().sprite = MailPanel.instance.mailReadImg;
        }

        if (mail.items.Count == 0)
        {
            transform.Find("item").gameObject.SetActive(false);
        }
        else if (mail.status == 2)
        {
            transform.Find("item").GetComponent<Image>().sprite = MailPanel.instance.boxOpenImg;
        }
    }


    public void OnToggleClick(bool isOn)
    {
        if (isOn && !isToggleOn)
        {
            MailPanel.instance.OnMailToggleClick(gameObject, mail);
        }
        isToggleOn = isOn;
    }
}
