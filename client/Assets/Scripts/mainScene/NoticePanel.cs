using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoticePanel : MonoBehaviour {

    public Text noticeText;
	// Use this for initialization
	void Start () {
        noticeText.text = PlayerInfo.notice;
	}
	
	public void OnBackBtn () {
        Destroy(gameObject);
	}
}
