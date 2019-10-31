
/// <summary>
/// 所有动态加载的 UI Panel
/// </summary>
public class UIPanel
{


    private static string allDir = "UIPanel";


    public static string someInfo = allDir + "/someInfo";
    public static string tileInfo = allDir + "/tileInfo";
    public static string wwwLoading = allDir + "/loading";
    public static string gameWarnPanel = allDir + "/gameWarnPanel";
    public static string onKickedPanel = allDir + "/onKickedPanel";
    public static string timeSetPanel = allDir + "/timeSetPanel";

    public class LoginScene
    {
        private static string myDir = "/LoginScene";
        private static string allDir = UIPanel.allDir + myDir;

        public static string fastLogin = allDir + "/fastLogin";
        public static string registerAndLogin = allDir + "/registerLogin";
    }

    public class MainScene
    {
        private static string myDir = "/MainScene";
        private static string allDir = UIPanel.allDir + myDir;

        public static string noticePanel = allDir + "/noticePanel";
        public static string signPanel = allDir + "/signPanel";
        public static string mailPanel = allDir + "/mailPanel";
        public static string sendMailPanel = allDir + "/sendMailPanel";
        public static string reconnectPanel = allDir + "/MainSceneReconnect";
        public static string friendPanel = allDir + "/friendPanel";
        public static string askFriend = allDir + "/askFriendPanel";
        public static string bagPanel = allDir + "/bagPanel";
        public static string sendRollNoticePanel = allDir + "/sendRollNoticePanel";
        public static string shopPanel = allDir + "/shopPanel";

        public class InfoPanel
        {
            private static string myDir = "/InfoPanel";
            private static string allDir = MainScene.allDir + myDir;

            public static string myInfoPanel = allDir + "/myInfoPanel";
            public static string changeInfoPanel = allDir + "/changeInfoPanel";
            public static string changeHeadImgPanel = allDir + "/changeHeadImgPanel";
        }

        public class AboutGame
        {
            private static string myDir = "/AboutGame";
            private static string allDir = MainScene.allDir + myDir;

            public static string gamePanel = allDir + "/gamePanel";
            public static string gameInfoPanel = allDir + "/gameInfoPanel";
            public static string createGamePanel = allDir + "/createGamePanel";
            public static string createGameInfo = allDir + "/createGameInfo";
            public static string onInvitePanel = allDir + "/onInvitePanel";

        }
    }

    public class MatchScene
    {
        private static string myDir = "/MatchScene";
        private static string allDir = UIPanel.allDir + myDir;

        public static string waitPanel = allDir + "/waitPanel";
        public static string reconnectPanel = allDir + "/MatchSceneReconnect";
    }

    public class GamePanel
    {
        private static string myDir = "/Game";
        private static string allDir = UIPanel.allDir + myDir;

        public static string gameReconnectPanel = allDir + "/gameSceneReconnectPanel";
        public static string chatInRoomPanel = allDir + "/chatInRoomPanel";
        public static string gameLeavePanel = allDir + "/gameLeavePanel";
        public static string gameResultPanel = allDir + "/gameResultPanel";

        public static string wuZiQiPanel = allDir + "/wuZiQiPanel";
        public static string xiangQiPanel = allDir + "/xiangQiPanel";
    }
}
