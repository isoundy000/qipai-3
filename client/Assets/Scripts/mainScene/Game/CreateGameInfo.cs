using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateGameInfo : MonoBehaviour
{
    public Text infoText;
    // Start is called before the first frame update
    void Start()
    {
        infoText.text = string.Format("" +
            "1、比赛创建后，为保障参赛者权益，各参数不可再修改，请仔细设置参数。\n" +
            "2、开始时间为可进行比赛的时间点，此前为预热时间段，以吸引参赛者。结束时间为不可再进行比赛的时间点，此后是显示比赛最终结果时间段。" +
            "关闭时间为该比赛不再显示的时间点。\n" +
            "3、请合理设置参数，以吸引参赛者。\n" +
            "4、比赛所花费的钻石为：每{0}小时消耗{1}钻石，时间为开始时间到结束时间段，不足则取整。\n" +
            "5、参赛者所扣除的门票和桌费皆为豆子，且在比赛结算时，才发放给比赛举办者。其中需要扣除{2}%的平台提成。\n" +
            "6、禁止赌博，若发现该行为，则公司有权关闭该比赛且不会返还门票等费用。" +
            "", PlayerInfo.createGameHourPer, PlayerInfo.createGameDiamondPer, PlayerInfo.douziGetPercent);
    }

    public void Btn_close()
    {
        Destroy(gameObject);
    }
}
