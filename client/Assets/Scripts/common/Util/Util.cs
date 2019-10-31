using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ImgType
{
    HeadImg,
    ItemImg,
    SexImg
}

public class Util
{
    //加载图片
    public static Sprite GetSprite(ImgType imgType, int id)
    {
        return Resources.Load<GameObject>(imgType.ToString() + "/" + id).GetComponent<SpriteRenderer>().sprite;
    }

    private static string FormatStr(string str, int len = 2)
    {
        while (str.Length < len)
        {
            str = "0" + str;
        }
        return str;
    }
    public static string FormatTime(int time, bool isThree = false)
    {
        string str = "";
        if (isThree)
        {
            str += FormatStr((time / 3600).ToString()) + ":";
            time = time % 3600;
        }
        str += FormatStr((time / 60).ToString()) + ":" + FormatStr((time % 60).ToString());
        return str;
    }
    public static string FormatTime(float time, bool isThree = false)
    {
        return FormatTime((int)time, isThree);
    }
}
