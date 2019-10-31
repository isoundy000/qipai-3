using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInfo : MonoBehaviour
{
    RectTransform myRect;
    float stayTime = 1.0f;
    float speedAdd = 500;
    float nowSpeed = 0;
    float tmpY;
    GameObject attachedObj;

    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
    }

    public void Init(string info, GameObject tmpAttachedObj)
    {
        transform.Find("Text").GetComponent<Text>().text = info;
        if (tmpAttachedObj == null)
        {
            attachedObj = gameObject;
        }
        else
        {
            attachedObj = tmpAttachedObj;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (attachedObj == null || !attachedObj.activeSelf)
        {
            Destroy(gameObject);
        }
        if (stayTime > 0)
        {
            stayTime -= Time.deltaTime;
        }
        else
        {
            nowSpeed += speedAdd * Time.deltaTime;
            tmpY = myRect.anchoredPosition.y + nowSpeed * Time.deltaTime;
            myRect.anchoredPosition = new Vector2(0, tmpY);
            if (tmpY > 420)
            {
                Destroy(gameObject);
            }
        }
    }
}
