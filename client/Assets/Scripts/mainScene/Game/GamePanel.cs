using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamePanel : MonoBehaviour
{
    public static GamePanel instance = null;
    public GameObject gameTypePrefab;
    public Transform gameTypeParent;
    public GameObject gamePrefab;
    public Transform gameParent;
    public Transform gamePrefabPoolParent;
    public Toggle gameGoingToggle;
    private int nowGameType = 0;
    private Proto.gameState nowGameState = Proto.gameState.wait;
    private RoomInfo nowRoomInfo = null;
    private Text gameTypeText;
    private Text pageIndexText;
    private int nowPageIndex = 1;
    private GameObject recentPlayObj;
    private bool nowIsRecentNotSearch = true;
    private List<Proto.gameInfo> recentGames = null;
    private List<Proto.gameInfo> searchGames = null;
    private string lastSearch = "";         // 上次搜索比赛的关键字（如本次相同，则不用搜索）
    private Button createGameBtn;

    private void Awake()
    {
        instance = this;
        pageIndexText = transform.Find("games/page/index").GetComponent<Text>();
        recentPlayObj = transform.Find("games/recentPlay").gameObject;
        createGameBtn = transform.Find("topBg/create").GetComponent<Button>();
    }

    // Use this for initialization
    void Start()
    {
        InitGameTypes();
        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onUpdateGameTypeState, SVR_onUpdateGameTypeState);
        CommonHandler.instance.AddCommonCb(CommonHandlerCb.onBagChanged, SVR_onBagChanged);

        SocketClient.AddHandler(Route.connector_main_getGameList, SVR_GetGamesBack);
        SocketClient.AddHandler(Route.connector_main_getRecentPlay, SVR_GetRecentBack);
        SocketClient.AddHandler(Route.connector_main_searchGame, SVR_SearchBack);

        InitPlayerInfo();
        ResetDiamondCoin();
        Transform funBtnParent = transform.Find("funcBtns/Viewport/btns");
        foreach (Transform tmpTrsm in funBtnParent)
        {
            Button tmpBtn = tmpTrsm.GetComponent<Button>();
            if (tmpBtn)
            {
                tmpBtn.onClick.AddListener(delegate ()
                {
                    FuncBtn_Click(tmpBtn);
                });
            }
        }
        gameTypeText = transform.Find("games/GameTypeText").GetComponent<Text>();
    }

    void InitGameTypes()
    {
        ToggleGroup gameTypeToggleGroup = gameTypeParent.GetComponent<ToggleGroup>();
        Transform tmpTrsm = Instantiate(gameTypePrefab, gameTypeParent).transform;
        tmpTrsm.name = "0";
        tmpTrsm.GetComponent<GameTypePrefab>().Init(0, "最近游玩");
        tmpTrsm.GetComponent<Toggle>().group = gameTypeToggleGroup;

        foreach (var oneGameType in PlayerInfo.gameTypes.Values)
        {
            if (oneGameType.state == Proto.gameTypeState.closed)
            {
                continue;
            }
            tmpTrsm = Instantiate(gameTypePrefab, gameTypeParent).transform;
            tmpTrsm.name = oneGameType.type.ToString();
            tmpTrsm.GetComponent<GameTypePrefab>().Init(oneGameType.type, oneGameType.name);
            tmpTrsm.GetComponent<Toggle>().group = gameTypeToggleGroup;
        }
    }

    public void InitPlayerInfo()
    {
        Transform tmpTrsm = transform.Find("topBg/btnUserInfo");
        tmpTrsm.Find("headImg").GetComponent<Image>().sprite = Util.GetSprite(ImgType.HeadImg, PlayerInfo.playerData.headId);
        tmpTrsm.Find("nickname").GetComponent<Text>().text = PlayerInfo.playerData.nickname;
        tmpTrsm.Find("signature").GetComponent<Text>().text = PlayerInfo.playerData.signature;
    }

    /// <summary>
    /// 设置钻石和金币数
    /// </summary>
    void ResetDiamondCoin()
    {
        var top = transform.Find("topBg");
        top.Find("diamond/num").GetComponent<Text>().text = PlayerInfo.GetItemNum((int)ItemId.diamond).ToString();
        top.Find("gold/num").GetComponent<Text>().text = PlayerInfo.GetItemNum((int)ItemId.douzi).ToString();
    }

    /// <summary>
    /// 各个按钮的点击
    /// </summary>
    /// <param name="tmpname"></param>
    void FuncBtn_Click(Button tmpname)
    {
        switch (tmpname.name)
        {
            case "btnBack":
                SocketClient.DisConnect();
                SceneManager.LoadScene(SceneNames.login);
                break;
            case "btnNotice":
                UIManager.instance.ShowPanel(UIPanel.MainScene.noticePanel);
                break;
            case "btnSign":
                UIManager.instance.ShowPanel(UIPanel.MainScene.signPanel);
                break;
            case "btnMail":
                UIManager.instance.ShowPanel(UIPanel.MainScene.mailPanel);
                break;
            case "btnBag":
                UIManager.instance.ShowPanel(UIPanel.MainScene.bagPanel);
                break;
            default:
                break;
        }
    }

    public void Btn_friend()
    {
        UIManager.instance.ShowPanel(UIPanel.MainScene.friendPanel);
    }

    public void Btn_userInfo()
    {
        UIManager.instance.ShowPanel(UIPanel.MainScene.InfoPanel.myInfoPanel);
    }

    /// <summary>
    /// 切换游戏类型
    /// </summary>
    /// <param name="gameType"></param>
    public void OnGameTypeToggleClick(int gameType)
    {
        nowGameType = gameType;
        if (nowGameType == 0)    // 0表示最近游玩
        {
            createGameBtn.interactable = false;
            recentPlayObj.SetActive(true);
            ChangeToRecentOrSearch();
            return;
        }
        if (recentPlayObj.activeSelf)
        {
            recentPlayObj.SetActive(false);
        }

        if (gameGoingToggle.isOn == true)
        {
            OnGameStateToggleClick(Proto.gameState.going);
        }
        else
        {
            gameGoingToggle.isOn = true;
        }
    }

    /// <summary>
    /// 选择游戏状态
    /// </summary>
    public void OnGameStateToggleClick(Proto.gameState state)
    {
        nowPageIndex = 1;
        pageIndexText.text = nowPageIndex.ToString();
        nowGameState = state;
        DestroyGamesObj();
        if (PlayerInfo.gameTypes[nowGameType].state == Proto.gameTypeState.wait)
        {
            createGameBtn.interactable = false;
            ShowGameTypeText();
            return;
        }
        else
        {
            createGameBtn.interactable = true;
        }
        Proto.GetGameListReq msg = new Proto.GetGameListReq();
        msg.gameType = nowGameType;
        msg.state = nowGameState;
        msg.pageIndex = nowPageIndex;
        SocketClient.SendMsg(Route.connector_main_getGameList, msg);
    }

    public void Btn_ChangePageIndex(bool add)
    {
        if (PlayerInfo.gameTypes[nowGameType].state == Proto.gameTypeState.wait)
        {
            return;
        }
        nowPageIndex += add ? 1 : -1;
        if (nowPageIndex < 1)
        {
            nowPageIndex = 1;
            return;
        }
        DestroyGamesObj();
        pageIndexText.text = nowPageIndex.ToString();
        Proto.GetGameListReq msg = new Proto.GetGameListReq();
        msg.gameType = nowGameType;
        msg.state = nowGameState;
        msg.pageIndex = nowPageIndex;
        SocketClient.SendMsg(Route.connector_main_getGameList, msg);
    }


    void SVR_GetGamesBack(string msg)
    {
        Debug.Log(msg);
        var data = JsonUtility.FromJson<Proto.GetGamesRsp>(msg);
        if (nowGameType > 0)
        {
            if (data.games.Count > 0 && data.games[0].gameType == nowGameType)
            {
                InitGames(data.games, false);
            }
            else
            {
                ShowGameTypeText();
            }
        }

    }

    void SVR_GetRecentBack(string msg)
    {
        Debug.Log(msg);
        var data = JsonUtility.FromJson<Proto.GetGamesRsp>(msg);
        recentGames = data.games;
        if (nowGameType == 0 && nowIsRecentNotSearch)
        {
            InitGames(data.games, true);
        }
    }

    void SVR_SearchBack(string msg)
    {
        Debug.Log(msg);
        var data = JsonUtility.FromJson<Proto.GetGamesRsp>(msg);
        searchGames = data.games;
        if (nowGameType == 0 && !nowIsRecentNotSearch)
        {
            InitGames(data.games, true);
        }
    }

    void InitGames(List<Proto.gameInfo> games, bool ignoreState)
    {
        foreach (var one in games)
        {
            if (!ignoreState && one.state != nowGameState)
            {
                continue;
            }
            Transform tmpTrsm;
            if (gamePrefabPoolParent.childCount > 0)
            {
                tmpTrsm = gamePrefabPoolParent.GetChild(0);
                tmpTrsm.SetParent(gameParent);
            }
            else
            {
                tmpTrsm = Instantiate(gamePrefab, gameParent).transform;
            }
            tmpTrsm.GetComponent<GamePrefab>().Init(one);

        }
        ShowGameTypeText();
    }

    /// <summary>
    /// 是否显示暂无比赛
    /// </summary>
    private void ShowGameTypeText()
    {
        StopCoroutine("RealGameTypeShow");
        StartCoroutine("RealGameTypeShow");
    }

    IEnumerator RealGameTypeShow()
    {
        yield return new WaitForEndOfFrame();
        if (nowGameType == 0)
        {
            if (gameParent.childCount > 0)
            {
                gameTypeText.text = "";
            }
            else if (nowIsRecentNotSearch)
            {
                gameTypeText.text = "暂无游玩信息哦";
            }
            else
            {
                gameTypeText.text = "未搜索到相关比赛";
            }
        }
        else if (PlayerInfo.gameTypes[nowGameType].state == Proto.gameTypeState.wait)
        {
            gameTypeText.text = "敬请期待";
        }
        else if (gameParent.childCount > 0)
        {
            gameTypeText.text = "";
        }
        else
        {
            gameTypeText.text = "当前类目，暂无比赛";
        }
    }

    /// <summary>
    /// 显示比赛详细信息面板
    /// </summary>
    public void ShowGameInfo(Proto.gameInfo gameInfo)
    {
        UIManager.instance.ShowPanel(UIPanel.MainScene.AboutGame.gameInfoPanel);
        GameInfoPanel.instance.Init(gameInfo);
    }


    public void Btn_createGame()
    {
        if (nowGameType == 0)
        {
            return;
        }
        if (PlayerInfo.gameTypes[nowGameType].state != Proto.gameTypeState.normal)
        {
            UIManager.instance.SetSomeInfo("该游戏类型暂未开放");
            return;
        }
        var trsm = UIManager.instance.ShowPanel(UIPanel.MainScene.AboutGame.createGamePanel);
        trsm.GetComponent<CreateGamePanel>().SetGameType(nowGameType);
    }


    void SVR_onUpdateGameTypeState(object info)
    {
        var data = (Proto.OnUpdateGameTypeState)info;
        if (data.lastState == Proto.gameTypeState.normal)
        {
            if (GameInfoPanel.instance != null && GameInfoPanel.instance.GetGameType() == data.gameType)
            {
                Destroy(GameInfoPanel.instance.gameObject);
            }
            if (CreateGamePanel.instance != null && CreateGamePanel.instance.gameType == data.gameType)
            {
                Destroy(CreateGamePanel.instance.gameObject);
            }
            if (nowGameType == data.gameType)
            {
                DestroyGamesObj();
            }
            if (data.state == Proto.gameTypeState.closed)
            {
                Destroy(gameTypeParent.Find(data.gameType.ToString()).gameObject);
            }
            ShowGameTypeText();
        }
        else if (data.lastState == Proto.gameTypeState.wait)
        {
            if (data.state == Proto.gameTypeState.closed)
            {
                Destroy(gameTypeParent.Find(data.gameType.ToString()).gameObject);
            }
            else if (nowGameType == data.gameType)
            {
                OnGameTypeToggleClick(data.gameType);
            }
        }
        else
        {
            Transform tmpTrsm = Instantiate(gameTypePrefab, gameTypeParent).transform;
            tmpTrsm.name = data.gameType.ToString();
            tmpTrsm.GetComponent<GameTypePrefab>().Init(data.gameType, PlayerInfo.gameTypes[data.gameType].name);
            tmpTrsm.GetComponent<Toggle>().group = gameTypeParent.GetComponent<ToggleGroup>();
        }
    }

    void SVR_onBagChanged(object data)
    {
        var msg = (Proto.OnBagChanged)data;
        bool changed = false;
        foreach (var one in msg.bag)
        {
            if (one.id == (int)ItemId.douzi || one.id == (int)ItemId.diamond)
            {
                changed = true;
                break;
            }
        }
        if (changed)
        {
            ResetDiamondCoin();
        }
    }

    /// <summary>
    /// 游戏类型和功能按钮的切换
    /// </summary>
    public void Toggle_switch(bool ShowBtn)
    {
        transform.Find("funcBtns").gameObject.SetActive(ShowBtn);
    }
    /// <summary>
    /// 搜索比赛
    /// </summary>
    /// <param name="id"></param>
    public void Input_SearchGame(string id)
    {
        id = id.Trim();
        if (id == lastSearch)
        {
            return;
        }
        if (searchGames != null)
        {
            searchGames.Clear();
        }
        DestroyGamesObj();
        if (id == "")
        {
            ShowGameTypeText();
            return;
        }
        Proto.SearchGameReq msg = new Proto.SearchGameReq();
        msg.search = id;
        lastSearch = id;
        SocketClient.SendMsg(Route.connector_main_searchGame, msg);
    }


    private void DestroyGamesObj()
    {
        for (int i = gameParent.childCount - 1; i >=0;i-- )
        {
            var one = gameParent.GetChild(i);
            if (gamePrefabPoolParent.childCount < 10)
            {
                one.SetParent(gamePrefabPoolParent);
                one.gameObject.SetActive(false);
            }
            else
            {
                Destroy(one.gameObject);
            }
        }
    }

    /// <summary>
    /// 最近游玩和搜索比赛开关
    /// </summary>
    public void Toggle_RecentOrSearch(bool isOn)
    {
        nowIsRecentNotSearch = !nowIsRecentNotSearch;
        recentPlayObj.transform.Find("idInput").gameObject.SetActive(!nowIsRecentNotSearch);
        ChangeToRecentOrSearch();
    }

    private void ChangeToRecentOrSearch()
    {
        DestroyGamesObj();
        if (nowIsRecentNotSearch)
        {
            if (recentGames == null)
            {
                var recentPlay = PlayerInfo.GetRecentPlay();
                if (recentPlay.recent.Count != 0)
                {
                    SocketClient.SendMsg(Route.connector_main_getRecentPlay, recentPlay);
                }
                else
                {
                    ShowGameTypeText();
                }
            }
            else
            {
                InitGames(recentGames, true);
            }
        }
        else if (searchGames != null)
        {
            InitGames(searchGames, true);
        }
        else
        {
            ShowGameTypeText();
        }
    }

    public void Btn_ShowShopPanel()
    {
        UIManager.instance.ShowPanel(UIPanel.MainScene.shopPanel);
    }

    private void OnDestroy()
    {
        SocketClient.RemoveHandler(Route.connector_main_getGameList);
        SocketClient.RemoveHandler(Route.connector_main_getRecentPlay);
        SocketClient.RemoveHandler(Route.connector_main_searchGame);
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onUpdateGameTypeState, SVR_onUpdateGameTypeState);
        CommonHandler.instance.RemoveCommonCb(CommonHandlerCb.onBagChanged, SVR_onBagChanged);
    }
}
