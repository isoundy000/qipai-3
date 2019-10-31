using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatInitText : MonoBehaviour
{
    public void Init(string info)
    {
        transform.Find("Text").GetComponent<Text>().text = info;
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(Hide());
    }

    IEnumerator Hide()
    {
        yield return new WaitForSeconds(2);
        gameObject.SetActive(false);
    }
}
