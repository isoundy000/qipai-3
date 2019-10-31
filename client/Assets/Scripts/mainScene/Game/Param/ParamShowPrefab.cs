using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParamShowPrefab : MonoBehaviour
{
    public void Init(string title, string val)
    {
        transform.Find("title").GetComponent<Text>().text = title;
        transform.Find("value").GetComponent<Text>().text = val;
    }
}
