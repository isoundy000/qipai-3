using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WWWLoading : MonoBehaviour {

    Transform turnObj;
    float turnTime = 0;
    float startShowTime = 1f;
    float timeOutTime = 4f;

    void Start()
    {
        turnObj = transform.GetChild(0);
    }


    void Update()
    {
        timeOutTime -= Time.deltaTime;
        if(timeOutTime < 0)
        {
            WWWManager.instance.TimeoutStopHttpRequest();
            Destroy(gameObject);
        }

        if(startShowTime > 0)
        {
            startShowTime -= Time.deltaTime;
            if(startShowTime <= 0)
            {
                GetComponent<Image>().color = new Color32(0, 0, 0, 200);
                turnObj.gameObject.SetActive(true);
            }
            return;
        }

        if (turnTime > 0)
        {
            turnTime -= Time.deltaTime;
        }
        else
        {
            turnObj.Rotate(0, 0, -30);
            turnTime = 0.1f;
        }
    }
}
