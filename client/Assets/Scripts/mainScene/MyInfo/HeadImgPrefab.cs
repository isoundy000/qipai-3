using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadImgPrefab : MonoBehaviour
{

    private int id;
    bool isNowOn = false;
    public void Init(int _id)
    {
        id = _id;
        GetComponent<Image>().sprite = Util.GetSprite(ImgType.HeadImg, id);
    }
    public void Toggle_click(bool isOn)
    {
        if (isOn && !isNowOn)
        {
            ChangeHeadImgPanel.instance.OnImgClick(id);
        }
        isNowOn = isOn;
    }
}
