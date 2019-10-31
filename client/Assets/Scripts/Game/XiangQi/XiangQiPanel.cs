using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XiangQi;

public class XiangQiPanel : MonoBehaviour
{
    public static XiangQiPanel instance;
    public GameObject qiZiPrefab;
    public Transform qiZiParent = null; //棋子的父物体
    public List<Sprite> qiziSprite = new List<Sprite>();    // 棋子图片
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
    int geZiWidth = 71;
    int offset = 30;
    bool needRotate = false;
    private XQ_base[,] qipan = new XQ_base[10, 9];
    private RectTransform nowSelectTrsm;
    private RectTransform step1Trsm;
    private RectTransform step2Trsm;
    private bool isNowSelected;
    private int nowI;
    private int nowJ;
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        nowSelectTrsm = qiZiParent.parent.Find("sign/nowSelect").GetComponent<RectTransform>();
        step1Trsm = qiZiParent.parent.Find("sign/step1").GetComponent<RectTransform>();
        step2Trsm = qiZiParent.parent.Find("sign/step2").GetComponent<RectTransform>();
        qiZiParent.GetComponent<XiangQiClick>().Init(offset, geZiWidth);
        GameCommon.instance.SetGameReconnectHandler(OnStartGame);
        SocketClient.AddHandler(Route.onPlayCard, SVR_OnPlayCard);
        SocketClient.AddHandler(Route.onGameOver, SVR_OnGameOver);
        SocketClient.AddHandler(Route.onTableDouziSync, SVR_onTableDouziSync);
        SocketClient.AddHandler(Route.onChatInTable, SVR_onChatInTable);
        SocketClient.AddHandler(Route.onChatSeqInTable, SVR_onChatSeqInTable);
        SocketClient.AddHandler(Route.onStepTimeOver, SVR_onStepTimeOver);
        SocketClient.AddHandler(Route.onPingWant, SVR_onPingWant);
        SocketClient.AddHandler(Route.onPingNo, SVR_onPingNo);
        OnStartGame();
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

    void OnStartGame()
    {
        step1Trsm.anchoredPosition = new Vector2(0, 500);
        step2Trsm.anchoredPosition = new Vector2(0, 500);
        nowSelectTrsm.gameObject.SetActive(false);

        var data = JsonUtility.FromJson<Proto.EnterXiangQi>(PlayerInfo.gameInfo);
        PlayerInfo.gameInfo = "";

        PlayerInfo.canChatInRoom = data.canChatInRoom;
        gameTime = data.gameTime;
        stepTime = data.stepTime;
        countTime = data.countTime;
        transform.Find("info/time").GetComponent<Text>().text = "局时：" + Util.FormatTime(gameTime, true) +
            "\n步时：" + Util.FormatTime(stepTime, true) + "\n读秒：" + Util.FormatTime(countTime, true);
        nowChairId = data.nowChairId;

        foreach (Transform trsm in qiZiParent)
        {
            Destroy(trsm.gameObject);
        }
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                qipan[i, j] = null;
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
                qiZiColor = (QiZiColor)tmpP.qiZiColor,
            };
            if (tmpP.uid == PlayerInfo.uid && (QiZiColor)tmpP.qiZiColor == QiZiColor.red)
            {
                needRotate = true;
                Debug.Log(needRotate);

                qiZiParent.parent.localEulerAngles = new Vector3(0, 0, 180);
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
            tmp.Find("qiZiType").GetComponent<Image>().color = GetQiziColorByType((QiZiColor)one.qiZiColor);
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

        foreach (var one in data.qipan)
        {
            var xqTmp = NewQizi(one.i, one.j, (QiZiColor)one.c, (qiZiType)one.t, qipan);
            qipan[one.i, one.j] = xqTmp;
            Transform qiZiTmp = Instantiate(qiZiPrefab, qiZiParent).transform;
            qiZiTmp.name = one.i + "_" + one.j;
            qiZiTmp.GetComponent<RectTransform>().anchoredPosition = new Vector3(offset + xqTmp.j * geZiWidth, -offset - xqTmp.i * geZiWidth, 0);
            qiZiTmp.GetComponent<Image>().sprite = GetSpriteByCT(xqTmp.c, xqTmp.t);
            if (needRotate)
            {
                qiZiTmp.localEulerAngles = new Vector3(0, 0, 180);
            }
        }

    }

    private Sprite GetSpriteByCT(QiZiColor c, qiZiType t)
    {
        int num = (int)t;
        int tmp = c == QiZiColor.red ? 0 : 1;
        return qiziSprite[num + 7 * tmp];
    }

    private Color GetQiziColorByType(QiZiColor c)
    {
        return c == QiZiColor.black ? Color.black : Color.red;
    }

    XQ_base NewQizi(int i, int j, QiZiColor c, qiZiType t, XQ_base[,] qipan)
    {
        XQ_base qizi = null;
        switch (t)
        {
            case qiZiType.ju:
                qizi = new XQ_ju();
                break;
            case qiZiType.ma:
                qizi = new XQ_ma();
                break;
            case qiZiType.xiang:
                qizi = new XQ_xiang();
                break;
            case qiZiType.si:
                qizi = new XQ_si();
                break;
            case qiZiType.jiang:
                qizi = new XQ_jiang();
                break;
            case qiZiType.pao:
                qizi = new XQ_pao();
                break;
            case qiZiType.bing:
                qizi = new XQ_bing();
                break;
            default:
                Debug.Log("error qizi type");
                return null;
        }
        qizi.i = i;
        qizi.j = j;
        qizi.c = c;
        qizi.t = t;
        qizi.qipan = qipan;
        return qizi;
    }

    void SVR_OnPlayCard(string msg)
    {
        var data = JsonUtility.FromJson<Proto.XiangQiLuoZi>(msg);

        step1Trsm.anchoredPosition = new Vector2(data.j * geZiWidth, -data.i * geZiWidth);
        step2Trsm.anchoredPosition = new Vector2(data.j2 * geZiWidth, -data.i2 * geZiWidth);

        if (qipan[data.i2, data.j2] != null)
        {
            qipan[data.i2, data.j2] = null;
            Destroy(qiZiParent.Find(data.i2 + "_" + data.j2).gameObject);
        }
        var src = qipan[data.i, data.j];
        qipan[data.i, data.j] = null;
        src.i = data.i2;
        src.j = data.j2;
        qipan[src.i, src.j] = src;
        var trsm = qiZiParent.Find(data.i + "_" + data.j);
        trsm.name = src.i + "_" + src.j;
        trsm.GetComponent<RectTransform>().anchoredPosition = new Vector2(offset + src.j * geZiWidth, -offset - src.i * geZiWidth);

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

        isNowSelected = false;
        nowSelectTrsm.gameObject.SetActive(false);
    }

    public void QiPanClick(int i, int j)
    {
        if (i < 0 || i >= 10 || j < 0 || j >= 9)
        {
            return;
        }
        if (isGameOver || nowChairId != myChairId)
        {
            return;
        }
        if (!isNowSelected)
        {
            if (qipan[i, j] == null || qipan[i, j].c != players[nowChairId].qiZiColor)
            {
                return;
            }
            isNowSelected = true;
            nowI = i;
            nowJ = j;
            nowSelectTrsm.anchoredPosition = new Vector2(j * geZiWidth, -i * geZiWidth);
            nowSelectTrsm.gameObject.SetActive(true);
            return;
        }
        if (i == nowI && j == nowJ)
        {
            return;
        }
        if (qipan[i, j] != null && qipan[i, j].c == players[nowChairId].qiZiColor)
        {
            nowI = i;
            nowJ = j;
            nowSelectTrsm.anchoredPosition = new Vector2(j * geZiWidth, -i * geZiWidth);
            return;
        }
        Proto.XiangQiLuoZi tmp = new Proto.XiangQiLuoZi();
        tmp.i = nowI;
        tmp.j = nowJ;
        tmp.i2 = i;
        tmp.j2 = j;
        SocketClient.SendMsg(Route.game_xiangQi_play, tmp);
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

    void SVR_onPingWant(string msg)
    {
        transform.Find("pingWant").gameObject.SetActive(true);
    }

    void SVR_onPingNo(string msg)
    {
        UIManager.instance.SetSomeInfo("对方拒绝平局");
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

    public void Btn_Leave()
    {
        UIManager.instance.ShowPanel(UIPanel.GamePanel.gameLeavePanel).GetComponent<GameSceneLeave>().leaveCb = Leave_Yes;
    }

    public void Btn_PingWant()
    {
        SocketClient.SendMsg(Route.game_xiangQi_ping);
    }

    public void Btn_PingYesOrNo(bool agree)
    {
        Proto.PingYesOrNo msg = new Proto.PingYesOrNo();
        msg.agree = agree;
        SocketClient.SendMsg(Route.game_xiangQi_pingYesOrNo, msg);
        transform.Find("pingWant").gameObject.SetActive(false);
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
        SocketClient.RemoveHandler(Route.onPingWant);
        SocketClient.RemoveHandler(Route.onPingNo);
    }

    private class Player
    {
        public int uid;
        public int chairId;
        public float gameTime;
        public float leftTime;
        public bool isStepTime;
        public int douzi;
        public QiZiColor qiZiColor;
    }

}


namespace XiangQi
{
    enum QiZiColor
    {
        red = 1,    // 红方
        black = 2,  // 黑方
    }
    enum qiZiType
    {
        ju = 1, // 车
        ma,     // 马  
        xiang,  // 象
        si,     // 仕
        jiang,  // 将
        pao,    // 炮
        bing    // 兵
    }

    class XQ_base
    {
        public XQ_base[,] qipan;
        public int i;
        public int j;
        public QiZiColor c;
        public qiZiType t;

        /**
         * 想要去这里
         */
        public virtual bool go(int i, int j)
        {
            return false;
        }
    }

    // 车
    class XQ_ju : XQ_base
    {
        public override bool go(int i, int j)
        {
            int min = 0;
            int max = 0;
            if (i == this.i)
            {
                if (this.j < j)
                {
                    min = this.j + 1;
                    max = j - 1;
                }
                else
                {
                    min = j + 1;
                    max = this.j - 1;
                }
                for (int m = min; m <= max; m++)
                {
                    if (this.qipan[i, m] != null)
                    {
                        return false;
                    }
                }
            }
            else if (j == this.j)
            {
                if (this.i < i)
                {
                    min = this.i + 1;
                    max = i - 1;
                }
                else
                {
                    min = i + 1;
                    max = this.i - 1;
                }
                for (int m = min; m <= max; m++)
                {
                    if (this.qipan[m, j] != null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            this.qipan[this.i, this.j] = null;
            this.i = i;
            this.j = j;
            this.qipan[this.i, this.j] = this;
            return true;
        }
    }

    // 马
    class XQ_ma : XQ_base
    {

    }
    // 象
    class XQ_xiang : XQ_base
    {

    }
    // 仕
    class XQ_si : XQ_base
    {

    }
    class XQ_jiang : XQ_base
    {

    }
    class XQ_pao : XQ_base
    {

    }
    class XQ_bing : XQ_base
    {

    }
}