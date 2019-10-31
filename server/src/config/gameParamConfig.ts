import { gameType, paramType, I_param_dropdown, I_game_param, uiType, I_param_input, I_gameTypeInfo, gameTypeState, I_notice } from "./gameConfig";

// -----------------------------------------------------------------------------------------------
/**
 * 创建比赛时，各时间间隔
 */
export let gameTimeDiff = {
    "create_start_minH": 0,         // 创建--开始，最小时数
    "create_start_maxD": 35,         // 创建--开始，最大天数
    "start_end_minH": 0,            // 开始--结束，最小时数
    "start_end_maxD": 35,            // 开始--结束，最大天数
    "end_close_minH": 0,            // 结束--关闭，最小时数
    "end_close_maxD": 35,            // 结束--关闭，最大天数
}

/**
 * 刷新个人最新战绩消耗金币数
 */
export let refreshMyRankCost = 100;

/**
 * 创建比赛时，每多少小时，消耗多少钻石
 */
export let createGameHourPer = 5;
export let createGameDiamondPer = 5;
/**
 * 比赛结算时，豆子平台提成（百分制）
 */
export let douziGetPercent = 10;

/**
 * 发送全服滚动公告消耗钻石数（提供滚动1-3次选项）
 */
export let sendRollNoticeCost = [10, 15, 20];

// -----------------------------------------------------------------------------------------------
// 各参数名字
export let paramNames: { "type": paramType, "name": string }[] = [
    { "type": paramType.rankNum, "name": "排行人数" },
    { "type": paramType.password, "name": "密码" },
    { "type": paramType.gameTime, "name": "局时" },
    { "type": paramType.stepTime, "name": "步时" },
    { "type": paramType.countTime, "name": "读秒" },
    { "type": paramType.doorCost, "name": "门票" },
    { "type": paramType.tableCost, "name": "桌费" },
    { "type": paramType.baseCost, "name": "底分" },
    { "type": paramType.canRoomChat, "name": "房内聊天" },
    { "type": paramType.canInviteFriend, "name": "好友同玩" },
];

function createData_dropdown(val: I_param_dropdown): I_game_param {
    return { "uiType": uiType.dropdown, "data": JSON.stringify(val) };
}

function createData_input(val: I_param_input): I_game_param {
    return { "uiType": uiType.input, "data": JSON.stringify(val) };
}

// -----------------------------------------------------------------------------------------------

/**
 * 五子棋参数
 */
let param_wuZiQi: I_game_param[] = [
    createData_dropdown({ "type": paramType.rankNum, "values": ["50人", "100人", "150人"], "realValues": [50, 100, 150], "defaultIndex": 0 }),
    createData_dropdown({ "type": paramType.gameTime, "values": ["10分钟", "30分钟", "1小时"], "realValues": [10 * 60, 30 * 60, 60 * 60], "defaultIndex": 1 }),
    createData_dropdown({ "type": paramType.stepTime, "values": ["1分钟", "3分钟", "5分钟"], "realValues": [1 * 60, 3 * 60, 5 * 60], "defaultIndex": 1 }),
    createData_dropdown({ "type": paramType.countTime, "values": ["30秒", "1分钟", "2分钟"], "realValues": [30, 1 * 60, 2 * 60], "defaultIndex": 1 }),
    createData_input({ "type": paramType.password, "isNumber": false, "min": 0, "max": 0, "placeholder": "请输入密码" }),
    createData_input({ "type": paramType.doorCost, "isNumber": true, "min": 0, "max": 10000, "placeholder": "请输入门票" }),
    createData_input({ "type": paramType.tableCost, "isNumber": true, "min": 0, "max": 200, "placeholder": "请输入桌费" }),
    createData_input({ "type": paramType.baseCost, "isNumber": true, "min": 0, "max": 100000, "placeholder": "请输入底分" }),
    createData_dropdown({ "type": paramType.canRoomChat, "values": ["允许", "禁止"], "realValues": [1, 0], "defaultIndex": 0 }),
    createData_dropdown({ "type": paramType.canInviteFriend, "values": ["允许", "禁止"], "realValues": [1, 0], "defaultIndex": 0 }),
]

/**
 * 五子棋参数
 */
let param_xiangQi: I_game_param[] = [
    createData_dropdown({ "type": paramType.rankNum, "values": ["50人", "100人", "150人"], "realValues": [50, 100, 150], "defaultIndex": 0 }),
    createData_dropdown({ "type": paramType.gameTime, "values": ["10分钟", "30分钟", "1小时"], "realValues": [10 * 60, 30 * 60, 60 * 60], "defaultIndex": 1 }),
    createData_dropdown({ "type": paramType.stepTime, "values": ["1分钟", "3分钟", "5分钟"], "realValues": [1 * 60, 3 * 60, 5 * 60], "defaultIndex": 1 }),
    createData_dropdown({ "type": paramType.countTime, "values": ["30秒", "1分钟", "2分钟"], "realValues": [30, 1 * 60, 2 * 60], "defaultIndex": 1 }),
    createData_input({ "type": paramType.password, "isNumber": false, "min": 0, "max": 0, "placeholder": "请输入密码" }),
    createData_input({ "type": paramType.doorCost, "isNumber": true, "min": 0, "max": 10000, "placeholder": "请输入门票" }),
    createData_input({ "type": paramType.tableCost, "isNumber": true, "min": 0, "max": 200, "placeholder": "请输入桌费" }),
    createData_input({ "type": paramType.baseCost, "isNumber": true, "min": 0, "max": 100000, "placeholder": "请输入底分" }),
    createData_dropdown({ "type": paramType.canRoomChat, "values": ["允许", "禁止"], "realValues": [1, 0], "defaultIndex": 0 }),
    createData_dropdown({ "type": paramType.canInviteFriend, "values": ["允许", "禁止"], "realValues": [1, 0], "defaultIndex": 0 }),
]

// -----------------------------------------------------------------------------------------------
// 各游戏类型的具体参数
export let gameTypeParam: { [gameType: number]: any[] } = {};
gameTypeParam[gameType.wuZiQi] = param_str2json(param_wuZiQi);
gameTypeParam[gameType.xiangQi] = param_str2json(param_xiangQi);

function param_str2json(params: I_game_param[]): any[] {
    let res: any[] = [];
    for (let one of params) {
        res.push(JSON.parse(one.data));
    }
    return res;
}




/**
 * 游戏类型
 */
export let gameTypes: I_gameTypeInfo[] = [
    {
        "type": gameType.wuZiQi,
        "name": "五子棋",
        "roleNum": 2,
        "state": gameTypeState.normal,
        "scene": "wuZiQi",
        "param": param_wuZiQi,
        "chatSeq": ["你丫的也快点儿呀！", "你太厉害了！", "加个好友吧！"],
    },
    {
        "type": gameType.xiangQi,
        "name": "中国象棋",
        "roleNum": 2,
        "state": gameTypeState.normal,
        "scene": "xiangQi",
        "param": param_xiangQi,
        "chatSeq": ["你丫的也快点儿呀！", "你太厉害了！", "加个好友吧！"],
    },
    {
        "type": gameType.douDiZhu_3,
        "name": "三人斗地主",
        "roleNum": 3,
        "state": gameTypeState.normal,
        "scene": "douDiZhu",
        "param": [],
        "chatSeq": ["你丫的也快点儿呀！", "你太厉害了！", "加个好友吧！"],
    },
    {
        "type": gameType.maJiang_2,
        "name": "二人麻将",
        "roleNum": 2,
        "state": gameTypeState.normal,
        "scene": "doudizhu",
        "param": [],
        "chatSeq": ["你丫的也快点儿呀！", "你太厉害了！", "加个好友吧！"],
    },
    {
        "type": gameType.maJiang_4,
        "name": "四人麻将",
        "roleNum": 4,
        "state": gameTypeState.normal,
        "scene": "doudizhu",
        "param": [],
        "chatSeq": ["你丫的也快点儿呀！", "你太厉害了！", "加个好友吧！"],
    },
];

/**
 * 默认公告
 */
export let rollNotices: I_notice[] = [
    { "count": 0, "info": "欢迎来到棋牌赛事库，希望您玩得愉快！" },
    { "count": 0, "info": "适度休闲娱乐，拒绝沉迷，禁止赌博！" },
    { "count": 0, "info": "ahuang啊ahuang，你真笨！" },
]