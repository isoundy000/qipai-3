import * as log4js from "log4js"
import { Application } from "mydog";

let config: log4js.Configuration = {
    appenders: {
        mydog: {
            type: 'file',
            filename: './log/mydog.log',
            maxLogSize: 10 * 1024 * 1024,
            backups: 5
        },
        gameLog: {
            type: 'file',
            filename: './log/gameLog.log',
            maxLogSize: 10 * 1024 * 1024,
            backups: 5
        },
        out: {
            type: 'stdout'
        }
    },
    categories: {
        default: { appenders: ['out', "mydog"], level: 'all' },
        mydog: { appenders: ['mydog', 'out'], level: 'error' },
        gameLog: { appenders: ['gameLog', 'out'], level: 'all' }
    }
};

// 替换文件名的服务器id
function replaceServerId(app: Application) {
    let one;
    for (let x in config.appenders) {
        one = config.appenders[x] as any;
        if (one.filename) {
            one.filename = (one.filename as string).replace("serverId", app.serverId);
        }
    }
    log4js.configure(config);
}

export function log4js_init(app: Application) {
    replaceServerId(app);
};

/**
 * mydog框架，内部日志
 */
export let mydogInnerLog = log4js.getLogger("mydog");

/**
 * 游戏日志
 */
export let gameLog = log4js.getLogger("gameLog");