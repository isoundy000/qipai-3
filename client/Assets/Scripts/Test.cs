using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{

    private void Start()
    {
        AA[,] tmp = new AA[2, 2];
        Debug.Log(tmp[0, 0]);
        Debug.Log(tmp[0, 0] == null);
    }

    private void change(int[,] arr)
    {
        arr[0, 0] = 5;
    }
}

class AA
{

}


