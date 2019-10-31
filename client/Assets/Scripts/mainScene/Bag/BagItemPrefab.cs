using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagItemPrefab : MonoBehaviour
{
    private bool isOn = false;
    private int itemId;
    public void Init(int _itemId, int count)
    {
        itemId = _itemId;
        transform.Find("item").GetComponent<Image>().sprite = Util.GetSprite(ImgType.ItemImg, itemId);
        transform.Find("num").GetComponent<Text>().text = count.ToString();
    }

    public void Toggle_click(bool _isOn)
    {
        if (_isOn && !isOn)
        {
            BagPanel.instance.OnItemClick(itemId);
        }
        isOn = _isOn;
    }
}
