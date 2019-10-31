using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeHeadImgPanel : MonoBehaviour
{
    public static ChangeHeadImgPanel instance;

    public GameObject headImgPrefab;
    public Transform headImgParent;
    private ToggleGroup headImgToggleGroup;
    public Image changedImg;
    private int nowHeadId;
    // Use this for initialization
    void Start()
    {
        instance = this;
        headImgToggleGroup = headImgParent.GetComponent<ToggleGroup>();
        transform.Find("now").GetComponent<Image>().sprite = Util.GetSprite(ImgType.HeadImg, PlayerInfo.playerData.headId);
        for (int i = 1; i <= 4; i++)
        {
            Transform tmp = Instantiate(headImgPrefab, headImgParent).transform;
            tmp.GetComponent<HeadImgPrefab>().Init(i);
            tmp.GetComponent<Toggle>().group = headImgToggleGroup;
        }
        SocketClient.AddHandler(Route.info_main_changeMyInfo, SVR_changeMyInfoBack);
    }

    public void OnImgClick(int id)
    {
        nowHeadId = id;
        changedImg.sprite = Util.GetSprite(ImgType.HeadImg, id);
    }

    public void Btn_ChangeImg()
    {
        if (nowHeadId == PlayerInfo.playerData.headId)
        {
            Btn_Close();
        }
        else
        {
            Proto.changeMyInfo tmp = new Proto.changeMyInfo();
            tmp.headId = nowHeadId;
            SocketClient.SendMsg(Route.info_main_changeMyInfo, tmp);
        }
    }

    void SVR_changeMyInfoBack(string _msg)
    {
        Proto.changeMyInfo msg = JsonUtility.FromJson<Proto.changeMyInfo>(_msg);
        if (msg.code == 0)
        {
            PlayerInfo.playerData.headId = msg.headId;
            GamePanel.instance.InitPlayerInfo();
            MyInfoPanel.instance.InitHeadInfo();
            Btn_Close();
        }
    }

    public void Btn_Close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SocketClient.RemoveHandler(Route.info_main_changeMyInfo);
    }
}
