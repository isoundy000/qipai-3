using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WuZiQiPanel : MonoBehaviour
{
    public static WuZiQiPanel instance;
    public GameObject qiZiPrefab;
    public Transform qiZiParent = null; //棋子的父物体
    private RectTransform qipanScale = null;   // 棋盘缩放物体
    private GameObject resetBtn = null;        // 重置棋盘大小的物体
    private ScrollRect qipanScroll = null;
    private bool nowIsBig = false;  // 当前棋盘是否放大
    private float qipanBiggerSize = 2;   //棋盘放大倍数

    Dictionary<int, Player> players = new Dictionary<int, Player>();    // chairId -> player
    Dictionary<int, Transform> playerTrsm = new Dictionary<int, Transform>();       // chairId -> trsm
    private int gameTime;
    private int stepTime;
    private int countTime;

    private float nowGameTime;
    private float nowLeftTime;
    private bool nowIsStepTime;
    private Text nowGameTimeText;
    private Text nowLeftTimeText;
    int nowChairId = 0;
    int myChairId = 0;
    bool isGameOver = false;
    int geZiWidth = 1;
    int offset = 18;
    int[,] qiZi = new int[19, 19];
    private RectTransform lastQizi = null;  // 刚下的棋子标识

    private void Awake()
    {
        instance = this;
        qipanScale = qiZiParent.parent.parent.GetComponent<RectTransform>();
        lastQizi = transform.Find("lastQizi").GetComponent<RectTransform>();
        resetBtn = transform.Find("qipanParent/resetBtn").gameObject;
        qipanScroll = transform.Find("qipanParent/scroll").GetComponent<ScrollRect>();
        geZiWidth = (int)((qiZiParent.parent as RectTransform).sizeDelta.x / 18);
        qiZiParent.GetComponent<QiPanClick>().Init(offset, geZiWidth);
    }

    // Use this for initialization
    void Start()
    {
        GameCommon.instance.SetGameReconnectHandler(OnStartGame);
        SocketClient.AddHandler(Route.onPlayCard, SVR_OnPlayCard);
        SocketClient.AddHandler(Route.onGameOver, SVR_OnGameOver);
        SocketClient.AddHandler(Route.onTableDouziSync, SVR_onTableDouziSync);
        SocketClient.AddHandler(Route.onChatInTable, SVR_onChatInTable);
        SocketClient.AddHandler(Route.onChatSeqInTable, SVR_onChatSeqInTable);
        SocketClient.AddHandler(Route.onStepTimeOver, SVR_onStepTimeOver);
        OnStartGame();
    }

    public void QiPanClick(float x, float y, int i, int j)
    {
        if (i < 0 || i >= 19 || j < 0 || j >= 19)
        {
            return;
        }
        if (isGameOver || nowChairId != myChairId)
        {
            return;
        }
        if (!nowIsBig)
        {
            nowIsBig = true;
            resetBtn.SetActive(true);
            qipanScale.localScale = new Vector3(qipanBiggerSize, qipanBiggerSize, 1f);
            qipanScale.anchoredPosition = new Vector2((1 - qipanBiggerSize) * x, (1 - qipanBiggerSize) * y);
            qipanScroll.enabled = true;
            return;
        }

        Btn_resetQipanScale();
        Proto.PosIJ tmp = new Proto.PosIJ();
        tmp.i = i;
        tmp.j = j;
        SocketClient.SendMsg(Route.game_wuZiQi_play, tmp);
    }

    public void Btn_resetQipanScale()
    {
        nowIsBig = false;
        resetBtn.SetActive(false);
        qipanScale.localScale = new Vector3(1f, 1f, 1f);
        qipanScale.anchoredPosition = new Vector2(0, 0);
        qipanScroll.enabled = false;
    }

    void OnStartGame()
    {
        lastQizi.anchoredPosition = new Vector2(0, 1000);
        var data = JsonUtility.FromJson<Proto.EnterWuZiQi>(PlayerInfo.gameInfo);
        PlayerInfo.gameInfo = "";

        PlayerInfo.canChatInRoom = data.canChatInRoom;
        gameTime = data.gameTime;
        stepTime = data.stepTime;
        countTime = data.countTime;
        transform.Find("info/time").GetComponent<Text>().text = "局时：" + Util.FormatTime(gameTime, true) +
            "\n步时：" + Util.FormatTime(stepTime, true) + "\n读秒：" + Util.FormatTime(countTime, true);
        nowChairId = data.nowChairId;

        for (int i = 0; i < 19; i++)
        {
            for (int j = 0; j < 19; j++)
            {
                if (qiZi[i, j] != 0)
                {
                    continue;
                }
                int tmpT = data.qiZi[i].qizi[j];
                if (tmpT == 0)
                {
                    continue;
                }
                qiZi[i, j] = tmpT;
                Transform qiZiTmp = Instantiate(qiZiPrefab, qiZiParent).transform;
                qiZiTmp.GetComponent<RectTransform>().anchoredPosition = new Vector3(offset + j * geZiWidth, -offset - i * geZiWidth, 0);
                qiZiTmp.GetComponent<Image>().color = GetQiziColorByType(qiZi[i, j]);
                if (i == data.lastI && j == data.lastJ)
                {
                    lastQizi.SetParent(qiZiTmp);
                    lastQizi.anchoredPosition = new Vector2(0, 0);
                }
            }
        }

        foreach (var tmpP in data.players)
        {
            players[tmpP.chairId] = new Player()
            {
                uid = tmpP.uid,
                chairId = tmpP.chairId,
                gameTime = tmpP.gameTime,
                isStepTime = tmpP.isStepTime,
                leftTime = tmpP.leftTime,
                qiZiType = tmpP.qiZiType,
            };
            if (tmpP.uid == PlayerInfo.uid && tmpP.qiZiType == 1)
            {
                transform.Find("qipanParent/scroll").eulerAngles = new Vector3(0, 0, 180);
            }
            if (tmpP.uid == PlayerInfo.uid)
            {
                playerTrsm[tmpP.chairId] = transform.Find("info/me");
                myChairId = tmpP.chairId;
            }
            else
            {
                playerTrsm[tmpP.chairId] = transform.Find("info/other");
            }
        }

        foreach (var one in data.players)
        {
            Transform tmp = playerTrsm[one.chairId];
            tmp.Find("nickname").GetComponent<Text>().text = one.nickname;
            tmp.Find("head").GetComponent<Image>().sprite = Util.GetSprite(ImgType.HeadImg, one.headId);
            tmp.Find("score").GetComponent<Text>().text = "积分：" + one.score;
            tmp.Find("douzi").GetComponent<Text>().text = "豆子：" + one.douzi;
            tmp.Find("qiZiType").GetComponent<Image>().color = GetQiziColorByType(one.qiZiType);
            if (one.chairId == nowChairId)
            {
                nowGameTime = one.gameTime;
                nowLeftTime = one.leftTime;
                nowGameTimeText = tmp.Find("gameTime/time").GetComponent<Text>();
                nowLeftTimeText = tmp.Find("leftTime/time").GetComponent<Text>();
                nowGameTimeText.text = Util.FormatTime(nowGameTime, true);
                nowLeftTimeText.text = Util.FormatTime(nowLeftTime);
            }
            else
            {
                tmp.Find("gameTime/time").GetComponent<Text>().text = Util.FormatTime(one.gameTime, true);
                tmp.Find("leftTime/time").GetComponent<Text>().text = Util.FormatTime(one.leftTime);
            }
            if (!one.isStepTime)
            {
                tmp.Find("leftTime/title").GetComponent<Text>().text = "读秒";
            }
        }
    }


    private void Update()
    {
        if (isGameOver)
        {
            return;
        }
        var nowP = players[nowChairId];
        nowP.leftTime -= Time.deltaTime;
        if (nowP.leftTime >= 0)
        {
            nowLeftTimeText.text = Util.FormatTime(nowP.leftTime);
        }
        if (nowP.isStepTime)
        {
            nowP.gameTime += Time.deltaTime;
            if (nowP.gameTime <= gameTime)
            {
                nowGameTimeText.text = Util.FormatTime(nowP.gameTime, true);
            }
        }
    }

    private Color GetQiziColorByType(int qiziType)
    {
        return qiziType == 1 ? Color.black : Color.white;
    }

    void SVR_OnPlayCard(string msg)
    {
        var data = JsonUtility.FromJson<Proto.WuZiQiLuoZi>(msg);

        Transform qiZiTmp = Instantiate(qiZiPrefab, qiZiParent).transform;
        qiZiTmp.GetComponent<RectTransform>().anchoredPosition = new Vector3(offset + data.j * geZiWidth, -offset - data.i * geZiWidth, 0);
        qiZiTmp.GetComponent<Image>().color = GetQiziColorByType(players[nowChairId].qiZiType);
        lastQizi.SetParent(qiZiTmp);
        lastQizi.anchoredPosition = new Vector2(0, 0);

        // 游戏是否结束
        if (data.isOver)
        {
            isGameOver = true;
            return;
        }

        players[nowChairId].leftTime = 0;
        nowLeftTimeText.text = Util.FormatTime(0);

        nowChairId = 3 - nowChairId;
        nowLeftTimeText = playerTrsm[nowChairId].Find("leftTime/time").GetComponent<Text>();
        nowGameTimeText = playerTrsm[nowChairId].Find("gameTime/time").GetComponent<Text>();
        var nowP = players[nowChairId];
        nowGameTimeText.text = Util.FormatTime(nowP.gameTime, true);
        nowP.leftTime = nowP.isStepTime ? stepTime : countTime;
        nowLeftTimeText.text = Util.FormatTime(nowP.leftTime);

        if (nowIsBig)
        {
            Btn_resetQipanScale();
        }
    }

    void SVR_onStepTimeOver(string msg)
    {
        var data = JsonUtility.FromJson<Proto.Uidsid>(msg);
        var nowP = players[GetChairIdByUid(data.uid)];
        nowP.isStepTime = false;
        nowP.gameTime = gameTime;
        if (nowP.leftTime > countTime)
        {
            nowP.leftTime = countTime;
        }
        nowLeftTimeText.transform.parent.Find("title").GetComponent<Text>().text = "读秒";
        nowGameTimeText.text = Util.FormatTime(nowP.gameTime, true);
        nowLeftTimeText.text = Util.FormatTime(nowP.leftTime);
    }

    private int GetChairIdByUid(int uid)
    {
        foreach (var one in players.Values)
        {
            if (one.uid == uid)
            {
                return one.chairId;
            }
        }
        return 0;
    }

    void SVR_OnGameOver(string msg)
    {
        Debug.Log("gameOver:" + msg);
        isGameOver = true;
        var data = JsonUtility.FromJson<Proto.GameOverResult>(msg);
        UIManager.instance.ShowPanel(UIPanel.GamePanel.gameResultPanel).GetComponent<GameResultPanel>().InitResult(data);
    }

    void SVR_onTableDouziSync(string msg)
    {
        var data = JsonUtility.FromJson<Proto.TableDouziSync>(msg);
        players[data.chairId].douzi = data.douzi;
        playerTrsm[data.chairId].Find("douzi").GetComponent<Text>().text = data.douzi.ToString();
    }

    void SVR_onChatInTable(string msg)
    {
        var data = JsonUtility.FromJson<Proto.GameRoomChat>(msg);
        playerTrsm[GetChairIdByUid(data.uid)].Find("chat").GetComponent<ChatInitText>().Init(data.msg);
    }

    void SVR_onChatSeqInTable(string msg)
    {
        var data = JsonUtility.FromJson<Proto.GameRoomChatSeq>(msg);
        var str = PlayerInfo.gameTypes[PlayerInfo.nowGameType].chatSeq[data.index];
        playerTrsm[GetChairIdByUid(data.uid)].Find("chat").GetComponent<ChatInitText>().Init(str);
    }


    public void Btn_Leave()
    {
        UIManager.instance.ShowPanel(UIPanel.GamePanel.gameLeavePanel).GetComponent<GameSceneLeave>().leaveCb = Leave_Yes;
    }

    void Leave_Yes()
    {
        SocketClient.SendMsg(Route.game_main_leave);
        SceneManager.LoadScene(SceneNames.main);
    }


    private void OnDestroy()
    {
        SocketClient.RemoveHandler(Route.onPlayCard);
        SocketClient.RemoveHandler(Route.onGameOver);
        SocketClient.RemoveHandler(Route.onTableDouziSync);
        SocketClient.RemoveHandler(Route.onChatInTable);
        SocketClient.RemoveHandler(Route.onChatSeqInTable);
        SocketClient.RemoveHandler(Route.onStepTimeOver);
    }

    private class Player
    {
        public int uid;
        public int chairId;
        public float gameTime;
        public float leftTime;
        public bool isStepTime;
        public int douzi;
        public int qiZiType;
    }
}
