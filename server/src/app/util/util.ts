
export function size(obj: { [key: string]: any }): number {
    return Object.keys(obj).length;
}

export function clone(origin: any): any {
    if (origin === null || origin === undefined) {
        return origin;
    }
    let con = origin.constructor;
    if (con === Number || con === String) {
        return origin.valueOf();
    }
    if (con === Array) {
        let arr = [];
        for (let i = 0; i < origin.length; i++) {
            if (origin[i] === undefined) {
                arr[i] = null;
            } else {
                arr[i] = clone(origin[i]);
            }
        }
        return arr;
    }
    let obj: { [key: string]: any } = {};
    for (let f in origin) {
        if (origin.hasOwnProperty(f) && origin[f] !== undefined) {
            obj[f] = clone(origin[f]);
        }
    }
    return obj;
}

export function timeFormat(_date: any): string {
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
}

export function getDiffDays(lastTime: Date, nowTime: Date, freshHour?: number): number {
    freshHour = freshHour || 0;
    let date = nowTime.getDate();
    if (nowTime.getHours() < freshHour) {
        date -= 1;
    }
    nowTime.setDate(date);
    nowTime.setHours(freshHour, 0, 0, 0);
    let diff = Math.floor((nowTime.getTime() - lastTime.getTime()) / (24 * 3600 * 1000)) + 1;
    return diff;
}


export class Latchdown {
    private count: number;
    private callback: Function;
    constructor(count: number, callback: Function) {
        this.count = count;
        this.callback = callback;
        if (count <= 0) {
            this.callback();
        }
    }

    down() {
        this.count--;
        if (this.count === 0) {
            this.callback();
        }
    }
}

export function createLatchdown(count: number, callback: Function): Latchdown {
    return new Latchdown(count, callback);
};

/**
 * 判断是否是字符串类型
 * @param x 
 */
export function isString(x: any): x is string {
    return typeof x === "string";
}

/**
 * 判断对象是否为空
 * @param obj 
 */
export function isEmptyObj(obj: Object) {
    for (let x in obj) {
        return false;
    }
    return true;
}

/**
 * 天梯积分计算
 * @param winList 
 * @param loseList 
 */
export function calculateScore(winList: { uid: number, score: number, scoreA: number, douzi: number }[],
    loseList: { uid: number, score: number, scoreA: number, douzi: number }[], loseDouzi: number) {
    let tmpScore: number = 0;
    let tmpScoreA: number = 0;
    let allLoseDouzi = 0;
    for (let one of loseList) {
        tmpScore += one.score;
        tmpScoreA += one.scoreA;
        if (one.douzi <= loseDouzi) {
            allLoseDouzi += one.douzi;
            one.douzi = 0;
        } else {
            allLoseDouzi += loseDouzi;
            one.douzi -= loseDouzi;
        }
    }
    let avgLose = Math.floor(tmpScore / loseList.length);
    let avgALose = Math.floor(tmpScoreA / loseList.length);

    let avgGetDouzi = Math.floor(allLoseDouzi / winList.length);
    tmpScore = 0;
    tmpScoreA = 0;
    for (let one of winList) {
        tmpScore += one.score;
        tmpScoreA += one.scoreA;
        one.douzi += avgGetDouzi;
    }
    let avgWin = Math.floor(tmpScore / winList.length);
    let avgAWin = Math.floor(tmpScoreA / winList.length);


    let Ewin = 1 / (1 + Math.pow(10, (avgLose - avgWin) / 400));
    let EAwin = 1 / (1 + Math.pow(10, (avgALose - avgAWin) / 400));

    let changeScore = Math.floor(32 * (1 - Ewin));
    let changeScoreA = Math.floor(32 * (1 - EAwin));

    let endRes: { [uid: number]: { "score": number, "scoreA": number, "douzi": number } } = {};
    for (let one of winList) {
        endRes[one.uid] = { "score": one.score + changeScore, "scoreA": one.scoreA + changeScoreA, "douzi": one.douzi };
    }
    for (let one of loseList) {
        endRes[one.uid] = { "score": one.score - changeScore, "scoreA": one.scoreA - changeScoreA, "douzi": one.douzi };
        if (endRes[one.uid].score < 1) {
            endRes[one.uid].score = 1;
        }
        if (endRes[one.uid].scoreA < 1) {
            endRes[one.uid].scoreA = 1;
        }
    }
    return endRes;
}