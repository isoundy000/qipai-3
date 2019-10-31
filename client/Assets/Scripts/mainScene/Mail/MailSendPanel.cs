using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailSendPanel : MonoBehaviour
{
    public static MailSendPanel instance;

    public Transform itemParent;
    public GameObject itemPrefab;
    public ToggleGroup itemToggleGroup;
    public Transform sendBtnParent;
    public Text nicknameText;
    public InputField infoInput;
    private int uid;
    private Dictionary<int, int> cloneBagInfo = new Dictionary<int, int>();
    private int[] sendIds = new int[] { -1, -1, -1 };
    private int[] sendNums = new int[] { 0, 0, 0 };

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitBags();
    }

    private void InitBags()
    {
        foreach (int key in PlayerInfo.bagInfo.Keys)
        {
            cloneBagInfo.Add(key, PlayerInfo.bagInfo[key]);
            Transform tmpTran = Instantiate(itemPrefab, itemParent).transform;
            tmpTran.GetComponent<ItemSendPrefab>().Init(key, cloneBagInfo[key]);
            tmpTran.GetComponent<Toggle>().group = itemToggleGroup;
        }
    }

    public void Init(int _uid, string nickname)
    {
        uid = _uid;
        nicknameText.text = nickname;
    }

    public void Btn_Send()
    {
        Proto.SendMailReq msg = new Proto.SendMailReq();
        msg.getUid = uid;
        msg.info = infoInput.text;
        for (int i = 0; i < sendIds.Length; i++)
        {
            if (sendIds[i] != -1)
            {
                Proto.ItemData one = new Proto.ItemData();
                one.id = sendIds[i];
                one.num = sendNums[i];
                msg.items.Add(one);
            }
        }
        SocketClient.SendMsg(Route.info_mail_sendMail, msg);
        UIManager.instance.SetTileInfo("邮件发送成功");
        Btn_Close();
    }

    // 点击背包里的物品，转到发送栏
    public void OnBagItemClick(int id)
    {
        int nowIndex = -1;
        int firstEmptyIndex = -1;
        for (int i = 0; i < sendIds.Length; i++)
        {
            if (sendIds[i] == id)
            {
                nowIndex = i;
            }
            if (firstEmptyIndex == -1 && sendIds[i] == -1)
            {
                firstEmptyIndex = i;
            }
        }
        if (nowIndex != -1)
        {
            sendNums[nowIndex] += 1;
            sendBtnParent.Find(nowIndex.ToString()).Find("Text").GetComponent<Text>().text = sendNums[nowIndex].ToString();
            cloneBagInfo[id]--;
            itemParent.Find(id.ToString()).GetComponent<ItemSendPrefab>().UpOrDown(false);
            return;
        }
        if (firstEmptyIndex != -1)
        {
            sendIds[firstEmptyIndex] = id;
            sendNums[firstEmptyIndex] = 1;
            Transform tmpTrsm = sendBtnParent.Find(firstEmptyIndex.ToString());
            tmpTrsm.Find("Image").gameObject.SetActive(true);
            tmpTrsm.Find("Image").GetComponent<Image>().sprite = Util.GetSprite(ImgType.ItemImg, id);
            tmpTrsm.Find("Text").GetComponent<Text>().text = sendNums[firstEmptyIndex].ToString();
            cloneBagInfo[id]--;
            itemParent.Find(id.ToString()).GetComponent<ItemSendPrefab>().UpOrDown(false);
        }
    }


    public void Btn_SendItem(int index)
    {
        int id = sendIds[index];
        if (id == -1)
        {
            return;
        }
        cloneBagInfo[id]++;
        itemParent.Find(id.ToString()).GetComponent<ItemSendPrefab>().UpOrDown(true);
        sendNums[index] -= 1;
        Transform tmpTrsm = sendBtnParent.Find(index.ToString());
        if (sendNums[index] == 0)
        {
            sendIds[index] = -1;
            tmpTrsm.Find("Image").gameObject.SetActive(false);
            tmpTrsm.Find("Text").GetComponent<Text>().text = "";
        }
        else
        {
            tmpTrsm.Find("Text").GetComponent<Text>().text = sendNums[index].ToString();
        }
    }


    public void Btn_Close()
    {
        Destroy(gameObject);
    }
}
