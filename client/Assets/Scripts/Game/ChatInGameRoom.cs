using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatInGameRoom : MonoBehaviour
{
    public static ChatInGameRoom instance;
    public GameObject chatSeqPrefab;
    public Transform chatSeqParent;
    private bool isOn = false;
    private InputField input;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        input = transform.Find("input").GetComponent<InputField>();
        if (!PlayerInfo.canChatInRoom)
        {
            input.interactable = false;
        }
        InitChatSeq();
    }

    private void InitChatSeq()
    {
        int index = 0;
        foreach (var chatStr in PlayerInfo.gameTypes[PlayerInfo.nowGameType].chatSeq)
        {
            Instantiate(chatSeqPrefab, chatSeqParent).GetComponent<ChatSeqPrefab>().Init(index, chatStr);
            index++;
        }
    }

    /// <summary>
    /// 快捷聊天
    /// </summary>
    /// <param name="index"></param>
    public void SeqClick(int index)
    {
        Proto.GameRoomChatSeq msg = new Proto.GameRoomChatSeq();
        msg.index = index;
        SocketClient.SendMsg(Route.game_main_chatSeq, msg);
        Btn_chatShowOrNot();
    }

    public void Btn_chatShowOrNot()
    {
        isOn = !isOn;
        input.gameObject.SetActive(isOn);
    }

    public void Btn_send()
    {
        string str = input.text;
        if (str.Length == 0)
        {
            return;
        }
        Proto.GameRoomChat msg = new Proto.GameRoomChat();
        msg.msg = str;
        SocketClient.SendMsg(Route.game_main_chat, msg);
        input.text = "";
        Btn_chatShowOrNot();
    }

}
