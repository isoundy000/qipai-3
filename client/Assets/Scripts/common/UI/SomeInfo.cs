using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SomeInfo : MonoBehaviour
{

    Text infoText = null;

    void Awake()
    {
        infoText = transform.Find("Text").GetComponent<Text>();
    }

    public void SetInfo(string info)
    {
        infoText.text = info;
    }

    public void OnBackBtn()
    {
        Destroy(gameObject);
    }
}
