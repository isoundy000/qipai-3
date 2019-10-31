
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameWarnPanel : MonoBehaviour
{

    private void Start()
    {
        SocketClient.AddHandler(Route.info_game_enterTable, SVR_EnterTableBack);
        SocketClient.AddHandler(Route.onEnterTable, SVR_OnEnterTable);
    }

    public void Btn_Yes()
    {
        SocketClient.SendMsg(Route.info_game_enterTable);
    }

    public void Btn_Close()
    {
        Destroy(gameObject);
    }

    void SVR_EnterTableBack(string msg)
    {
        UIManager.instance.SetTileInfo("进入游戏失败");
    }

    void SVR_OnEnterTable(string msg)
    {
        var msgObj = JsonUtility.FromJson<Proto.OnEnterTable>(msg);
        PlayerInfo.nowGameId = msgObj.gameId;
        PlayerInfo.nowGameType = msgObj.gameType;
        PlayerInfo.gameInfo = msgObj.data;
        SceneManager.LoadScene(PlayerInfo.gameTypes[PlayerInfo.nowGameType].scene);
        PlayerInfo.ChangeRecentPlay(msgObj.gameId);
    }

    private void OnDestroy()
    {
        SocketClient.RemoveHandler(Route.info_game_enterTable);
        SocketClient.RemoveHandler(Route.onEnterTable);
    }
}
