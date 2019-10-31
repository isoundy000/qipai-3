using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameNotice : MonoBehaviour
{
    private RectTransform noticeTrsm;
    private float maxX = 0;
    private float minX = 0;
    public float speed = 100;
    private bool needInit = true;

    private void Start()
    {
        maxX = GetComponent<RectTransform>().sizeDelta.x + 50;
        noticeTrsm = transform.Find("Text").GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        var rollNotices = PlayerInfo.rollNotices;
        if (rollNotices.Count == 0)
        {
            return;
        }
        if (needInit)
        {
            InitText();
            needInit = false;
            return;
        }
        PlayerInfo.rollNoticeX -= speed * Time.deltaTime;
        if (PlayerInfo.rollNoticeX < minX)
        {
            var now = rollNotices[PlayerInfo.rollNoticeI];
            if (now.count == 0)
            {
                PlayerInfo.rollNoticeI = (PlayerInfo.rollNoticeI + 1) % rollNotices.Count;
            }
            else
            {
                now.count--;
                if (now.count == 0)
                {
                    rollNotices.Remove(now);
                    if (rollNotices.Count == 0)
                    {
                        PlayerInfo.rollNoticeI = 0;
                        PlayerInfo.rollNoticeX = maxX;
                        needInit = true;
                        return;
                    }
                    if (rollNotices.Count == PlayerInfo.rollNoticeI)
                    {
                        PlayerInfo.rollNoticeI = 0;
                    }
                }
                else
                {
                    PlayerInfo.rollNoticeI = (PlayerInfo.rollNoticeI + 1) % rollNotices.Count;
                }
            }
            PlayerInfo.rollNoticeX = maxX;
            InitText();
        }
        else
        {
            noticeTrsm.anchoredPosition = new Vector2(PlayerInfo.rollNoticeX, 0);
        }
    }

    private void InitText()
    {
        if (PlayerInfo.rollNotices.Count <= PlayerInfo.rollNoticeI)
        {
            PlayerInfo.rollNoticeI = 0;
        }
        if (PlayerInfo.rollNoticeX == 0)
        {
            PlayerInfo.rollNoticeX = maxX;
        }
        var noticeText = noticeTrsm.GetComponent<Text>();
        noticeText.text = PlayerInfo.rollNotices[PlayerInfo.rollNoticeI].info;
        minX = -noticeTrsm.GetComponent<Text>().preferredWidth - 50;
        noticeTrsm.anchoredPosition = new Vector3(PlayerInfo.rollNoticeX, 0);
    }

    public void Btn_notice()
    {
        UIManager.instance.ShowPanel(UIPanel.MainScene.sendRollNoticePanel);
    }
}
