using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatSeqPrefab : MonoBehaviour
{
    private int _index;
    public void Init(int index, string str)
    {
        _index = index;
        transform.Find("Text").GetComponent<Text>().text = str;
    }
    public void Btn_click()
    {
        ChatInGameRoom.instance.SeqClick(_index);
    }
}
