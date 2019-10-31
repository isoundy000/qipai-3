import { TimeoutPro, setTimeoutPro } from "./timeoutPro";

/**
 * 顺序时间计时器
 */
export default class JobArr {
    private timers: TimeoutPro[] = [];
    constructor(times: Date[] | string[], cb: (index: number, isStart: boolean) => void, thisObj?: any) {
        let timeNumArr: number[] = [];
        for (let one of times) {
            if (typeof one === "string") {
                timeNumArr.push(new Date(one).getTime());
            } else {
                timeNumArr.push(one.getTime());
            }
        }
        let now = Date.now();
        let index = 0;
        while (true) {
            if (index === timeNumArr.length) {
                break;
            }
            if (timeNumArr[index] > now) {
                break;
            }
            index++;
        }
        for (let i = index; i < timeNumArr.length; i++) {
            let timer = setTimeoutPro(cb.bind(thisObj, i + 1, false), timeNumArr[i] - now);
            this.timers.push(timer);
        }
        cb.call(thisObj, index, true);
    }

    close() {
        for (let one of this.timers) {
            one.stop();
        }
    }
}