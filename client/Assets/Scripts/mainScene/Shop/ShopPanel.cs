using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanel : MonoBehaviour
{
    public static ShopPanel instance;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public void Btn_addDouzi()
    {
        SocketClient.SendMsg(Route.info_main_addDouzi);
    }

    public void Btn_addDiamond()
    {
        SocketClient.SendMsg(Route.info_main_addDiamond);
    }

    public void Btn_close()
    {
        Destroy(gameObject);
    }
}
