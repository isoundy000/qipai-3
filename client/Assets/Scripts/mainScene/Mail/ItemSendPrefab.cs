using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSendPrefab : MonoBehaviour
{

    private int id;
    private int num;
    public Text numText;

    public void Init(int _id, int _num)
    {
        name = _id.ToString();
        id = _id;
        num = _num;
        transform.Find("item").GetComponent<Image>().sprite = Util.GetSprite(ImgType.ItemImg, id);
        SetNumText();
    }

    void SetNumText()
    {
        numText.text = num.ToString();
    }

    public void UpOrDown(bool isAdd)
    {
        int change = isAdd ? 1 : -1;
        num += change;
        SetNumText();
    }

    public void OnToggleClick(bool isOn)
    {
        if (isOn && num > 0)
        {
            MailSendPanel.instance.OnBagItemClick(id);
        }
    }
}
