
/**
 * 游戏信息数据库（如比赛）
 */
export let game_RedisConfig = [
    { "port": 6380 },
]

/**
 * mysql配置
 */
export let mysqlConfig = {
    "host": "127.0.0.1",
    "port": 3306,
    "user": "root",
    "password": "123456",
    "database": "qipai",
    "connectionLimit": 5
}

/**
 * mysql表
 */
export enum mysqlTable {
    /**
     * 账号
     */
    account = "account",
    /**
     * 玩家信息
     */
    player = "player",
    /**
     * 比赛
     */
    game = "game",
    /**
     * 比赛类型状态
     */
    gametypestate = "gametypestate",
    /**
     * 邮件
     */
    mail = "mail",
    /**
     * 全服邮件的个人状态
     */
    mail_all = "mail_all",
    /**
     * 好友
     */
    friend = "friend"
}