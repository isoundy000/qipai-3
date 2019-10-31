using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameInfoPanel : MonoBehaviour
{
    public static GameInfoPanel instance;
    public Transform paramParent;
    public GameObject paramShowPrefab;
    public Transform rankParent;
    public GameObject rankPrefab;
    private Proto.gameInfo oneGame;

    void Awake()
    {
        instance = this;
    }

    public void Init(Proto.gameInfo tmpGame)
    {
        oneGame = tmpGame;
        SocketClient.AddHandler(Route.info_game_createMatchTable, SVR_createWaitTableRsp);
        SocketClient.AddHandler(Route.info_game_refreshMyRank, SVR_RefreshRankBack);
        SocketClient.AddHandler(Route.info_game_getRankList, SVR_GetRankListBack);

        if (oneGame.state != Proto.gameState.going)
        {
            transform.Find("startGame").gameObject.SetActive(false);
        }
        if (oneGame.state == Proto.gameState.wait)
        {
            transform.Find("rank/rankN/refresh").gameObject.SetActive(false);
        }
        else
        {
            transform.Find("rank/rankN/refresh/cost").GetComponent<Text>().text = "-" + PlayerInfo.refreshMyRankCost + "金币";
            Proto.GetRankListReq msg = new Proto.GetRankListReq();
            msg.gameId = oneGame.id;
            msg.rankSvr = oneGame.rankServer;
            SocketClient.SendMsg(Route.info_game_getRankList, msg);
        }
        transform.Find("gameName/Text").GetComponent<Text>().text = oneGame.gameName;
        transform.Find("notice/Text").GetComponent<Text>().text = oneGame.gameNotice;

        transform.Find("rankAward/rank").GetComponent<Text>().text = string.Format("1：\n2：\n3：\n4-{0}：\n" +
            "{1}-{2}：", oneGame.award[3].rank, oneGame.award[3].rank + 1, oneGame.award[4].rank);

        transform.Find("rankAward/num").GetComponent<Text>().text = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", oneGame.award[0].num,
             oneGame.award[1].num, oneGame.award[2].num, oneGame.award[3].num, oneGame.award[4].num);
        CreateShowPrefab("开始时间:", oneGame.startTime);
        CreateShowPrefab("结束时间:", oneGame.endTime);
        CreateShowPrefab("关闭时间:", oneGame.closeTime);
        CreateShowPrefab("需要密码:", oneGame.password == "" ? "否" : "是");
        CreateShowPrefab("排行人数:", oneGame.rankNum + "人");
        var param = PlayerInfo.gameTypes[oneGame.gameType].param;
        var paramData = oneGame.gameParam;
        foreach (var one in param)
        {
            Proto.paramType paramType = Proto.paramType.gameTime;
            if (one.uiType == Proto.GameParamUIType.dropdown)
            {
                paramType = JsonUtility.FromJson<I_param_dropdown>(one.data).type;
            }
            else if (one.uiType == Proto.GameParamUIType.input)
            {
                paramType = JsonUtility.FromJson<I_param_input>(one.data).type;
            }
            string showInfo = "";
            switch (paramType)
            {
                case Proto.paramType.gameTime:
                    showInfo = paramData.gameTime / 60 + "分钟";
                    break;
                case Proto.paramType.stepTime:
                    showInfo = paramData.stepTime / 60 + "分钟";
                    break;
                case Proto.paramType.countTime:
                    showInfo = paramData.countTime + "秒";
                    break;
                case Proto.paramType.doorCost:
                    showInfo = paramData.doorCost + "豆子";
                    break;
                case Proto.paramType.tableCost:
                    showInfo = paramData.tableCost + "豆子";
                    break;
                case Proto.paramType.baseCost:
                    showInfo = paramData.baseCost + "豆子";
                    break;
                case Proto.paramType.canRoomChat:
                    showInfo = paramData.canRoomChat ? "允许" : "禁止";
                    break;
                case Proto.paramType.canInviteFriend:
                    showInfo = paramData.canInviteFriend ? "允许" : "禁止";
                    break;
                default:
                    break;
            }
            if (showInfo != "")
            {
                CreateShowPrefab(PlayerInfo.gameParamName[paramType] + ":", showInfo);
            }
        }
    }


    public int GetGameType()
    {
        return oneGame.gameType;
    }

    private void CreateShowPrefab(string title, string val)
    {
        Instantiate(paramShowPrefab, paramParent).GetComponent<ParamShowPrefab>().Init(title, val);
    }


    public void Btn_startGame()
    {
        Proto.CreateWaitTableReq msg = new Proto.CreateWaitTableReq();
        msg.gameId = oneGame.id;
        msg.matchSvr = oneGame.matchServer;
        msg.rankSvr = oneGame.rankServer;
        msg.pwd = transform.Find("pwd").GetComponent<InputField>().text;
        SocketClient.SendMsg(Route.info_game_createMatchTable, msg);
    }

    void SVR_createWaitTableRsp(string msg)
    {
        Proto.CreateWaitTableRsp data = JsonUtility.FromJson<Proto.CreateWaitTableRsp>(msg);
        if (data.code == 1)
        {
            UIManager.instance.ShowPanel(UIPanel.gameWarnPanel);
        }
        else if (data.code == 101)
        {
            UIManager.instance.SetTileInfo("游戏不存在");
        }
        else if (data.code == 103)
        {
            UIManager.instance.SetTileInfo("密码错误");
        }
        else if (data.code == 104)
        {
            UIManager.instance.SetTileInfo("豆子不足");
        }
    }

    private void SVR_GetRankListBack(string data)
    {
        Debug.Log(data);
        var msg = JsonUtility.FromJson<Proto.GetRankListRsp>(data);
        int rank = 1;
        foreach (var one in msg.ranklist)
        {
            if (one.uid == PlayerInfo.uid)
            {
                transform.Find("rank/rankL/rank").GetComponent<Text>().text = "排名 " + rank;
                transform.Find("rank/rankL/score").GetComponent<Text>().text = "分数 " + one.score;
            }
            Transform trsm = Instantiate(rankPrefab, rankParent).transform;
            trsm.GetComponent<RankPrefab>().Init(rank, one);
            rank++;
        }
    }
    /// <summary>
    /// 添加排行榜中的人为好友
    /// </summary>
    /// <param name="uid"></param>
    public void WantAddFriend(int uid)
    {
        Proto.Uidsid msg = new Proto.Uidsid();
        msg.uid = uid;
        SocketClient.SendMsg(Route.info_friend_askFriend, msg);
        UIManager.instance.SetTileInfo("好友请求已发送");
    }

    /// <summary>
    /// 刷新最新个人战绩
    /// </summary>
    public void Btn_RefreshRank()
    {
        if (PlayerInfo.bagInfo[1001] < PlayerInfo.refreshMyRankCost)
        {
            UIManager.instance.SetTileInfo("豆子不足");
            return;
        }
        Proto.GetRankListReq msg = new Proto.GetRankListReq();
        msg.gameId = oneGame.id;
        msg.rankSvr = oneGame.rankServer;
        SocketClient.SendMsg(Route.info_game_refreshMyRank, msg);
    }

    void SVR_RefreshRankBack(string msg)
    {
        var data = JsonUtility.FromJson<Proto.RefreshMyRankRsp>(msg);
        transform.Find("rank/rankN/rank").GetComponent<Text>().text = "排名 " + data.meRank;
        transform.Find("rank/rankN/score").GetComponent<Text>().text = "分数 " + data.meScore;
    }


    public void Btn_close()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SocketClient.RemoveHandler(Route.info_game_createMatchTable);
        SocketClient.RemoveHandler(Route.info_game_getRankList);
        SocketClient.RemoveHandler(Route.info_game_refreshMyRank);
    }
}
