using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameSceneLeave : MonoBehaviour
{

    public Action leaveCb = null;

    public void Btn_Close()
    {
        Destroy(gameObject);
    }

    public void Btn_Leave()
    {
        leaveCb?.Invoke();
    }
}
