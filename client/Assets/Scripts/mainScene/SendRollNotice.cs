using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendRollNotice : MonoBehaviour
{
    public InputField infoInput;
    public Text costText;
    private int nowIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        Dropdown_valueChanged(0);
    }

    public void Dropdown_valueChanged(int index)
    {
        nowIndex = index;
        costText.text = string.Format("花费{0}钻石", PlayerInfo.sendRollNoticeCost[nowIndex]);
    }

    public void Btn_Close()
    {
        Destroy(gameObject);
    }
    public void Btn_send()
    {
        if (PlayerInfo.bagInfo[(int)ItemId.diamond] < PlayerInfo.sendRollNoticeCost[nowIndex])
        {
            UIManager.instance.SetTileInfo("钻石不足");
            return;
        }
        Proto.Notice msg = new Proto.Notice();
        msg.count = nowIndex + 1;
        msg.info = infoInput.text;
        SocketClient.SendMsg(Route.info_main_sendRollNotice, msg);
        UIManager.instance.SetTileInfo("发送播报成功");
        Btn_Close();
    }
}
