
/**
 * 游戏类型
 */
export enum gameType {
    douDiZhu_3 = 11,    // 三人斗地主
    maJiang_2 = 21,     // 二人麻将
    maJiang_4 = 22,     // 四人麻将
    xiangQi = 31,       // 中国象棋
    wuZiQi = 41,        // 五子棋
}


/**
 * 游戏类型状态
 */
export enum gameTypeState {
    normal = 1,     // 正常开放
    wait = 2,       // 待开放
    closed = 3      // 关闭     （客户端不显示tab。）
}


/**
 * 游戏状态
 */
export enum gameState {
    /**
     * 即将开始
     */
    create = 1,
    /**
     * 进行中
     */
    start = 2,
    /**
     * 已结算
     */
    end = 3,
    /**
     * 已结束并关闭
     */
    close = 4,
}

// 客户端ui类型
export enum uiType {
    dropdown,   // 下拉列表
    input,      // 输入框
}

// 参数类型
export enum paramType {
    rankNum,    // 排行人数
    password,   // 密码
    gameTime,   // 局时
    stepTime,   // 步时
    countTime,  // 读秒
    doorCost,   // 门票
    tableCost,  // 桌费
    baseCost,   // 基础豆子消耗（输时最低1倍）
    canRoomChat,    // 是否允许房内聊天
    canInviteFriend,  // 能否邀请好友一起玩
}

/**
 * 创建游戏消息
 */
export interface I_createGameReq {
    gameType: number;
    gameName: string; //游戏名
    gameNotice: string;   // 公告
    startTime: string;
    endTime: string;
    closeTime: string;
    rankIndex: number;   // 排行人数序号
    password: string; // 密码
    award: { "rank": number, "num": number }[];    // 奖励
    otherParam: I_createGameReqOtherParam
}


/**
 * 创建游戏时请求参数
 */
export interface I_createGameReqOtherParam {
    gameTimeIndex: number;   // 局时序号
    stepTimeIndex: number;   // 步时序号
    countTimeIndex: number;  // 读秒序号
    doorCost: number;        // 门票
    tableCost: number;       // 桌费
    baseCost: number;       // 输豆基数
    canRoomChatIndex: number;    // 是否允许房内聊天
    canInviteFriendIndex: number;    // 能否邀请好友一起玩
}

/**
 * 数据库中游戏参数
 */
export interface I_gameParam {
    gameTime: number;        // 局时
    stepTime: number;        // 步时
    countTime: number;       // 读秒
    doorCost: number;        // 门票
    tableCost: number;       // 桌费
    baseCost: number;       // 输豆基数
    canRoomChat: boolean;    // 是否允许房内聊天
    canInviteFriend: boolean;    // 能否邀请好友一起玩
}

/**
 * 游戏类型信息
 */
export interface I_gameTypeInfo {
    type: gameType,
    name: string,
    roleNum: number,
    state: gameTypeState,
    scene: string,
    param: I_game_param[],
    chatSeq: string[],
}

// 输入框
export interface I_param_input {
    type: paramType,
    isNumber: boolean,
    min: number,
    max: number,
    placeholder: string,
}

// 下拉列表
export interface I_param_dropdown {
    type: paramType,
    values: string[],   // 显示的数据
    realValues: number[],       // 逻辑数据（目前只用了number，后续可能改）
    defaultIndex: number,
}


// 返回给客户端参数
export interface I_game_param {
    uiType: uiType,
    data: string
}


/**
 * 服务器类型
 */
export enum serverType {
    /**
     * 登录服
     */
    login = "login",
    /**
     * 网关服
     */
    connector = "connector",
    /**
     * 数据服
     */
    info = "info",
    /**
     * 比赛控制管理服
     */
    gameMain = "gameMain",
    /**
     * 匹配服
     */
    match = "match",
    /**
     * 排行服
     */
    rank = "rank",
    /**
     * 游戏服
     */
    game = "game"
}



export interface I_notice {
    count: number,  // 次数，0表示永久
    info: string,   // 内容
}