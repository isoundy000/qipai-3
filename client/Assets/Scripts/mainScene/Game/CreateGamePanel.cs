using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CreateGamePanel : MonoBehaviour
{
    public static CreateGamePanel instance;
    [HideInInspector]
    public int gameType = 0;
    private int nowSetTimeType = 1;
    private Text startTimeText;
    private Text endTimeText;
    private Text closeTimeText;
    public Transform paramParent;
    public GameObject param_dropdown_prefab;
    public GameObject param_input_prefab;
    private Text costText;
    private int rankNum = 0;    // 排行人数
    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        startTimeText = transform.Find("gameStartTime/Text").GetComponent<Text>();
        endTimeText = transform.Find("gameEndTime/Text").GetComponent<Text>();
        closeTimeText = transform.Find("gameCloseTime/Text").GetComponent<Text>();
        DateTime now = DateTime.Now;
        DateTime tmpTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        string nowTimeStr = tmpTime.ToString("yyyy-MM-dd HH:mm:ss");
        startTimeText.text = nowTimeStr;
        endTimeText.text = nowTimeStr;
        closeTimeText.text = nowTimeStr;
        SocketClient.AddHandler(Route.info_game_createGameMatch, SVR_createGameRsp);
        transform.Find("gameTypeName").GetComponent<Text>().text = PlayerInfo.gameTypes[gameType].name;
        costText = transform.Find("gameCost").GetComponent<Text>();
        InitParam();
    }

    public void SetGameType(int type)
    {
        gameType = type;
    }

    public void Btn_timeSet(int timeSetType)
    {
        nowSetTimeType = timeSetType;
        string title = "";
        string tmp = "";
        if (nowSetTimeType == 1)
        {
            tmp = startTimeText.text;
            title = "开始时间";
        }
        else if (nowSetTimeType == 2)
        {
            tmp = endTimeText.text;
            title = "结束时间";
        }
        else if (nowSetTimeType == 3)
        {
            tmp = closeTimeText.text;
            title = "关闭时间";
        }
        Transform trsm = UIManager.instance.ShowPanel(UIPanel.timeSetPanel);
        trsm.GetComponent<TimeSetPanel>().Init(TimeSetCb, title, tmp);
    }

    bool TimeSetCb(DateTime time)
    {
        string timeStr = time.ToString("yyyy-MM-dd HH:mm:ss");
        bool needCalCost = false;
        if (nowSetTimeType == 1)
        {
            startTimeText.text = timeStr;
            needCalCost = true;
        }
        else if (nowSetTimeType == 2)
        {
            endTimeText.text = timeStr;
            needCalCost = true;
        }
        else if (nowSetTimeType == 3)
        {
            closeTimeText.text = timeStr;
        }
        if (needCalCost)
        {
            CalCost();
        }
        return true;
    }

    void InitParam()
    {
        var param = PlayerInfo.gameTypes[gameType].param;
        foreach (var one in param)
        {
            if (one.uiType == Proto.GameParamUIType.dropdown)
            {
                var data = JsonUtility.FromJson<I_param_dropdown>(one.data);
                var obj = Instantiate(param_dropdown_prefab, paramParent);
                obj.GetComponent<ParamDropdown>().Init(data);
            }
            else if (one.uiType == Proto.GameParamUIType.input)
            {
                var data = JsonUtility.FromJson<I_param_input>(one.data);
                var obj = Instantiate(param_input_prefab, paramParent);
                obj.GetComponent<ParamInput>().Init(data);
            }
        }
    }

    /// <summary>
    /// 设置排行人数
    /// </summary>
    /// <param name="num"></param>
    public void SetRankNum(int num)
    {
        rankNum = num;
        CalCost();
    }

    /// <summary>
    /// 排行奖励改变
    /// </summary>
    public void Input_RankAwardNum(string str)
    {
        CalCost();
    }
    /// <summary>
    /// 排行人数序号改变
    /// </summary>
    public void Dropdown_RankAwardIndex(int index)
    {
        CalCost();
    }

    /// <summary>
    /// 计算创建游戏的消耗
    /// </summary>
    private void CalCost()
    {
        DateTime startTime = DateTime.Parse(startTimeText.text);
        DateTime endTime = DateTime.Parse(endTimeText.text);
        TimeSpan delta = endTime - startTime;
        if (delta.TotalHours < 0)
        {
            costText.text = "需要花费：（时间参数错误）";
            return;
        }
        int diamondCost = (int)Math.Ceiling(delta.TotalHours / (double)PlayerInfo.createGameHourPer) * PlayerInfo.createGameDiamondPer;
        var award = GetGameAwardList(false);
        if (award == null)
        {
            costText.text = "需要花费：（奖励参数错误）";
            return;
        }
        int rankStart = 1;
        int douziCost = 0;
        for (int i = 0; i < award.Count; i++)
        {
            if (award[i].num == 0)
            {
                rankStart = award[i].rank + 1;
                continue;
            }
            bool isOver = false;
            for (int rank = rankStart; rank <= award[i].rank; rank++)
            {
                if (rank > rankNum)
                {
                    isOver = true;
                    break;
                }
                douziCost += award[i].num;
            }
            if (isOver)
            {
                break;
            }
            rankStart = award[i].rank + 1;
        }
        costText.text = string.Format("需要花费：{0}钻石，{1}豆子", diamondCost, douziCost);
    }


    /// <summary>
    /// 创建游戏
    /// </summary>
    public void Btn_yes()
    {
        Proto.CreateGameReq msg = new Proto.CreateGameReq();
        msg.gameName = transform.Find("gameNameInput").GetComponent<InputField>().text;
        if (msg.gameName.Length < 3)
        {
            UIManager.instance.SetTileInfo("比赛名字至少3个字符");
            return;
        }
        DateTime nowTime = DateTime.Now;
        DateTime startTime = DateTime.Parse(startTimeText.text);
        TimeSpan delta = startTime - nowTime;
        if (delta.TotalHours < PlayerInfo.gameTimeDiff.create_start_minH || delta.TotalDays > PlayerInfo.gameTimeDiff.create_start_maxD)
        {
            string str = string.Format("开始时间距离当前时间须在{0}小时到{1}天之间",
                PlayerInfo.gameTimeDiff.create_start_minH, PlayerInfo.gameTimeDiff.create_start_maxD);
            UIManager.instance.SetTileInfo(str);
            return;
        }
        DateTime endTime = DateTime.Parse(endTimeText.text);
        delta = endTime - startTime;
        if (delta.TotalHours < PlayerInfo.gameTimeDiff.start_end_minH || delta.TotalDays > PlayerInfo.gameTimeDiff.start_end_maxD)
        {
            string str = string.Format("结束时间距离开始时间须在{0}小时到{1}天之间",
                PlayerInfo.gameTimeDiff.start_end_minH, PlayerInfo.gameTimeDiff.start_end_maxD);
            UIManager.instance.SetTileInfo(str);
            return;
        }
        DateTime closeTime = DateTime.Parse(closeTimeText.text);
        delta = closeTime - endTime;
        if (delta.TotalHours < PlayerInfo.gameTimeDiff.end_close_minH || delta.TotalDays > PlayerInfo.gameTimeDiff.end_close_maxD)
        {
            string str = string.Format("关闭时间距离结束时间须在{0}小时到{1}天之间",
                PlayerInfo.gameTimeDiff.end_close_minH, PlayerInfo.gameTimeDiff.end_close_maxD);
            UIManager.instance.SetTileInfo(str);
            return;
        }

        var award = GetGameAwardList(true);
        if (award == null)
        {
            return;
        }

        msg.award = award;

        foreach (Transform trsm in paramParent)
        {
            var comp = trsm.GetComponent<GameParamType>();
            if (comp.uiType == Proto.GameParamUIType.dropdown)
            {
                int index = (comp as ParamDropdown).GetIndex();
                switch (comp.paramType)
                {
                    case Proto.paramType.rankNum:
                        msg.rankIndex = index;
                        break;
                    case Proto.paramType.gameTime:
                        msg.otherParam.gameTimeIndex = index;
                        break;
                    case Proto.paramType.stepTime:
                        msg.otherParam.stepTimeIndex = index;
                        break;
                    case Proto.paramType.countTime:
                        msg.otherParam.countTimeIndex = index;
                        break;
                    case Proto.paramType.canRoomChat:
                        msg.otherParam.canRoomChatIndex = index;
                        break;
                    case Proto.paramType.canInviteFriend:
                        msg.otherParam.canInviteFriendIndex = index;
                        break;
                    default:
                        break;
                }
            }
            else if (comp.uiType == Proto.GameParamUIType.input)
            {
                var compReal = comp as ParamInput;
                if (!compReal.IfNumOk())
                {
                    return;
                }
                string val = compReal.GetValue();
                switch (comp.paramType)
                {
                    case Proto.paramType.password:
                        msg.password = val;
                        break;
                    case Proto.paramType.doorCost:
                        msg.otherParam.doorCost = int.Parse(val);
                        break;
                    case Proto.paramType.tableCost:
                        msg.otherParam.tableCost = int.Parse(val);
                        break;
                    case Proto.paramType.baseCost:
                        msg.otherParam.baseCost = int.Parse(val);
                        break;
                    default:
                        break;
                }
            }
        }
        Debug.Log("ok");
        msg.gameType = gameType;
        msg.gameNotice = transform.Find("gameNoticeInput").GetComponent<InputField>().text;
        msg.startTime = startTimeText.text;
        msg.endTime = endTimeText.text;
        msg.closeTime = closeTimeText.text;
        Debug.Log(JsonUtility.ToJson(msg));
        SocketClient.SendMsg(Route.info_game_createGameMatch, msg);
    }

    List<Proto.GameAwardRank> GetGameAwardList(bool show)
    {
        Transform awardParent = transform.Find("gameAward");
        List<Proto.GameAwardRank> award = new List<Proto.GameAwardRank>();
        // 判断第一名
        Proto.GameAwardRank first = new Proto.GameAwardRank();
        string val = awardParent.Find("first/value").GetComponent<InputField>().text;
        val = val == "" ? "0" : val;
        int num;
        bool ok = int.TryParse(val, out num);
        if (!ok || num < 0)
        {
            if (show)
            {
                UIManager.instance.SetTileInfo("奖励设置错误");
            }
            return null;
        }
        first.rank = 1;
        first.num = num;
        award.Add(first);
        // 判断第二名
        Proto.GameAwardRank second = new Proto.GameAwardRank();
        val = awardParent.Find("second/value").GetComponent<InputField>().text;
        val = val == "" ? "0" : val;
        ok = int.TryParse(val, out num);
        if (!ok || num < 0)
        {
            if (show)
            {
                UIManager.instance.SetTileInfo("奖励设置错误");
            }
            return null;
        }
        second.rank = 2;
        second.num = num;
        award.Add(second);
        // 判断第三名
        Proto.GameAwardRank third = new Proto.GameAwardRank();
        val = awardParent.Find("third/value").GetComponent<InputField>().text;
        val = val == "" ? "0" : val;
        ok = int.TryParse(val, out num);
        if (!ok || num < 0)
        {
            if (show)
            {
                UIManager.instance.SetTileInfo("奖励设置错误");
            }
            return null;
        }
        third.rank = 3;
        third.num = num;
        award.Add(third);
        // 判断第四个
        Proto.GameAwardRank four = new Proto.GameAwardRank();
        val = awardParent.Find("four/value").GetComponent<InputField>().text;
        val = val == "" ? "0" : val;
        ok = int.TryParse(val, out num);
        if (!ok || num < 0)
        {
            if (show)
            {
                UIManager.instance.SetTileInfo("奖励设置错误");
            }
            return null;
        }
        four.num = num;
        four.rank = int.Parse(awardParent.Find("four/index").GetComponent<Dropdown>().captionText.text);
        award.Add(four);
        // 判断第五个
        Proto.GameAwardRank five = new Proto.GameAwardRank();
        five.rank = int.Parse(awardParent.Find("five/index").GetComponent<Dropdown>().captionText.text);
        if (five.rank <= four.rank)
        {
            if (show)
            {
                UIManager.instance.SetTileInfo("奖励设置错误");
            }
            return null;
        }
        val = awardParent.Find("five/value").GetComponent<InputField>().text;
        val = val == "" ? "0" : val;
        ok = int.TryParse(val, out num);
        if (!ok || num < 0)
        {
            if (show)
            {
                UIManager.instance.SetTileInfo("奖励设置错误");
            }
            return null;
        }
        five.num = num;
        award.Add(five);
        return award;
    }

    void SVR_createGameRsp(string msg)
    {
        Proto.JustCode data = JsonUtility.FromJson<Proto.JustCode>(msg);
        if (data.code == 0)
        {
            Destroy(gameObject);
        }
    }

    public void Btn_createGameDes()
    {
        UIManager.instance.ShowPanel(UIPanel.MainScene.AboutGame.createGameInfo);
    }

    public void Btn_close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SocketClient.RemoveHandler(Route.info_game_createGameMatch);
    }
}