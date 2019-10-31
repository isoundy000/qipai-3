import { RedisClient } from "redis";
import mysqlClient from "./mysql";
import { redisPool } from "./redisPool";

type callback = (err: Error | null, res?: any) => void;
type normalObj = { [key: string]: any };

function timeFormat(_date: any) {
    let date: Date = typeof _date === "object" ? _date : new Date(_date);
    let timeStr = "";
    let tmp: number;
    timeStr += date.getFullYear() + "-";
    tmp = date.getMonth() + 1;
    timeStr += (tmp > 9 ? tmp : "0" + tmp) + "-";
    tmp = date.getDate();
    timeStr += (tmp > 9 ? tmp : "0" + tmp) + " ";
    tmp = date.getHours();
    timeStr += (tmp > 9 ? tmp : "0" + tmp) + ":";
    tmp = date.getMinutes();
    timeStr += (tmp > 9 ? tmp : "0" + tmp) + ":";
    tmp = date.getSeconds();
    timeStr += tmp > 9 ? tmp : "0" + tmp;
    return timeStr;
};


function isEmpty(obj: object) {
    return Object.keys(obj).length === 0;
}

/*
    {
        "name" : "player",
        "index" : "uid",
        "field" : {
            "uid": "number",
            "nickname": "string",
            "birthday": "time",
            "info": "json"
        }
    }

    例： get(player:1) 结果为 {"uid": 1, "nickname": "zhangsan"}
 */

interface I_RedisObject_config {
    name: string,
    index: string,
    field: { [key: string]: string }
}

export class RedisObject {
    private db: mysqlClient;
    private redis: redisPool;
    private config: I_RedisObject_config;
    constructor(db: mysqlClient, redisPool: redisPool, config: I_RedisObject_config) {
        this.db = db;
        this.redis = redisPool;
        this.config = config;
    }

    selectFromDb(index: number, cb: callback) {
        let config = this.config;
        let objCon = {} as any;
        objCon[config.index] = index;
        this.db.select(config.name, "*", objCon, function (err, res) {
            if (err || res.length === 0) {
                return cb(err);
            }
            res = res[0];
            let type;
            let endValue = {} as any;
            for (let key in res) {
                type = config.field[key];
                if (type && res[key] !== null) {
                    if (type === "time") {
                        endValue[key] = timeFormat(res[key]);
                    } else if (type === "json") {
                        endValue[key] = JSON.parse(res[key]);
                    } else {
                        endValue[key] = res[key];
                    }
                }
            }

            if (isEmpty(endValue)) {
                cb(null);
            } else {
                cb(null, endValue);
            }
        });
    }

    insert(index: number, value: normalObj, cb?: callback) {
        let config = this.config;
        let endValue = {} as any;
        for (let key in value) {
            if (config.field[key] && value[key] != null) {
                if (config.field[key] === "json") {
                    endValue[key] = JSON.stringify(value[key]);
                } else {
                    endValue[key] = value[key];
                }
            }
        }
        if (isEmpty(endValue)) {
            cb && cb(null);
        } else {
            this.redis.get(index).hmset(config.name + ":" + index, endValue, cb);
        }
    }

    update(index: number, value: normalObj, cb?: callback) {
        this.insert(index, value, cb);
    }

    select(index: number, field: "*" | string[], cb: callback) {
        let config = this.config;
        if (field === "*") {
            field = [];
            for (let key in config.field) {
                field.push(key);
            }
        }
        this.redis.get(index).hmget(config.name + ":" + index, field, function (err, res) {
            if (err) {
                return cb(err);
            }

            let result: normalObj = {};
            let type;
            for (let i = 0; i < field.length; i++) {
                type = config.field[field[i]];
                if (type && res[i] !== null) {
                    if (type === "number") {
                        result[field[i]] = Number(res[i]);
                    } else if (type === "json") {
                        result[field[i]] = JSON.parse(res[i]);
                    } else {
                        result[field[i]] = res[i];
                    }
                }
            }
            if (isEmpty(result)) {
                cb(null);
            } else {
                cb(null, result);
            }
        });
    }

    delete(index: number, cb?: callback) {
        this.redis.get(index).del(this.config.name + ":" + index, cb);
    }

    updateToDb(index: number, value: normalObj, cb?: callback) {
        let config = this.config;
        let endValue: normalObj = {};
        for (let key in value) {
            if (config.field[key] && value[key] !== null) {
                if (config.field[key] === "json") {
                    endValue[key] = JSON.stringify(value[key]);
                } else if (config.field[key] === "number") {
                    endValue[key] = Number(value[key]);
                } else {
                    endValue[key] = value[key];
                }
            }
        }
        let objCon: normalObj = {};
        objCon[config.index] = index;
        this.db.update(config.name, endValue, objCon, cb);
    }
}

/*
    {
        "name" : "friend",
        "index" : "uid",
        "index2": "friendUid",
        "field" : {
            "id": "number",
            "uid": "number",
            "friendUid": "number",
            "status": "number",
            "info": "string",
            "changeTime": "time"
        }
    }

    例：hget(friend:1, 2) 结果为 {"id": 5, "uid": 1, "friendUid": 2, "info": "你好，我是张三"}
*/

interface I_RedisDictionary_config {
    name: string,
    index: string,
    index2: string,
    field: { [key: string]: string }
}
export class RedisDictionary {
    private db: mysqlClient;
    private redis: redisPool;
    private config: I_RedisDictionary_config;
    constructor(db: mysqlClient, redisPool: redisPool, config: I_RedisDictionary_config) {
        this.db = db;
        this.redis = redisPool;
        this.config = config;
    }

    selectFromDb(index: number, cb: callback) {
        let config = this.config;
        let objCon: normalObj = {};
        objCon[config.index] = index;
        this.db.select(config.name, "*", objCon, function (err, res) {
            if (err || res.length === 0) {
                return cb(err);
            }
            let endValue: normalObj = {};
            let one;
            let type;
            for (let i = 0; i < res.length; i++) {
                one = res[i];
                for (let key in one) {
                    type = config.field[key];
                    if (type && one[key] !== null) {
                        if (type === "time") {
                            one[key] = timeFormat(one[key]);
                        } else if (type === "json") {
                            one[key] = JSON.parse(one[key]);
                        }
                    } else {
                        delete one[key];
                    }
                }
                endValue[one[config.index2]] = one;
            }
            cb(null, endValue);
        });
    }
    insert(index: number, index2: number, value: normalObj, cb?: callback): void
    insert(index: number, value: normalObj, cb?: callback): void
    insert(index: number, index2: any, value?: any, cb?: any) {
        let tmpValue: normalObj = {};
        if (value) {
            if (typeof value === "function") {
                cb = value;
                tmpValue = index2;
            } else {
                tmpValue[index2] = value;
            }
        } else {
            tmpValue = index2;
        }
        let endValue: normalObj = {};
        let one;
        let config = this.config;
        for (let x in tmpValue) {
            one = tmpValue[x];
            endValue[x] = {};
            for (let y in one) {
                if (config.field[y] && one[y] !== null) {
                    endValue[x][y] = one[y];
                }
            }
            endValue[x] = JSON.stringify(endValue[x]);
        }
        this.redis.get(index).hmset(config.name + ":" + index, endValue, cb);
    }

    update(index: number, index2: number, value: normalObj, cb?: callback): void
    update(index: number, value: normalObj, cb?: callback): void
    update(index: number, index2: any, value: any, cb?: any) {
        this.insert(index, index2, value, cb);
    }

    select(index: number, index2: "*" | number[] | number, cb: callback) {
        let config = this.config;
        if (index2 === "*") {
            this.redis.get(index).hgetall(config.name + ":" + index, function (err, res) {
                if (err || !res) {
                    return cb(err);
                }
                for (let key in res) {
                    res[key] = JSON.parse(res[key]);
                }
                cb(null, res);
            });
        } else if (index2.constructor === Array) {
            this.redis.get(index).hmget(config.name + ":" + index, index2 as any, function (err, res) {
                if (err) {
                    return cb(err);
                }
                let result: normalObj = {};
                for (let i = 0; i < res.length; i++) {
                    if (res[i]) {
                        result[(index2 as any)[i]] = JSON.parse(res[i]);
                    }
                }
                cb(null, result);
            });
        } else {
            this.redis.get(index).hget(config.name + ":" + index, index2 as any, function (err, res) {
                if (err) {
                    return cb(err);
                }
                cb(null, JSON.parse(res));
            });
        }
    }

    delete(index: number, index2: number, cb?: callback) {
        this.redis.get(index).hdel(this.config.name + ":" + index, index2 as any, cb)
    }

    deleteAll(index: number, cb?: callback) {
        this.redis.get(index).del(this.config.name + ":" + index, cb);
    }

    updateToDb(index: number, index2: number, value: normalObj, cb?: callback) {
        let config = this.config;
        let endValue: normalObj = {};
        let type;
        for (let key in value) {
            type = config.field[key];
            if (type && value[key] !== null) {
                if (type === "json") {
                    endValue[key] = JSON.stringify(value[key]);
                } else if (type === "number") {
                    endValue[key] = Number(value[key]);
                } else {
                    endValue[key] = value[key];
                }
            }
        }
        let objCon: normalObj = {};
        objCon[config.index] = index;
        objCon[config.index2] = index2;
        this.db.update(config.name, endValue, objCon, cb);
    }
}