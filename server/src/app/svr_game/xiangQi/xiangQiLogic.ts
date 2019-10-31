
/**
 * 注意：本棋盘中 红色为小i（即0，1，2，3，4），会影响到部分棋子行走判断
 */

/**
 * 构造完整的棋盘
 */
export function getXQQiPan(): XQ_base[][] {
    let qipan: (XQ_base | null)[][] = [];
    qipan[0] = [
        N(0, 0, qiZiColor.red, qiZiType.ju, qipan),
        N(0, 1, qiZiColor.red, qiZiType.ma, qipan),
        N(0, 2, qiZiColor.red, qiZiType.xiang, qipan),
        N(0, 3, qiZiColor.red, qiZiType.si, qipan),
        N(0, 4, qiZiColor.red, qiZiType.jiang, qipan),
        N(0, 5, qiZiColor.red, qiZiType.si, qipan),
        N(0, 6, qiZiColor.red, qiZiType.xiang, qipan),
        N(0, 7, qiZiColor.red, qiZiType.ma, qipan),
        N(0, 8, qiZiColor.red, qiZiType.ju, qipan),
    ];
    qipan[1] = [];
    qipan[2] = [];
    for (let j = 0; j < 9; j++) {
        qipan[1][j] = null;
        qipan[2][j] = null;
    }
    qipan[2][1] = N(2, 1, qiZiColor.red, qiZiType.pao, qipan);
    qipan[2][7] = N(2, 7, qiZiColor.red, qiZiType.pao, qipan);
    qipan[3] = [];
    for (let j = 0; j < 9; j++) {
        if (j % 2 === 0) {
            qipan[3][j] = N(3, j, qiZiColor.red, qiZiType.bing, qipan);
        } else {
            qipan[3][j] = null;
        }
    }
    qipan[4] = [];
    qipan[5] = [];
    for (let j = 0; j < 9; j++) {
        qipan[4][j] = null;
        qipan[5][j] = null;
    }
    qipan[6] = [];
    for (let j = 0; j < 9; j++) {
        if (j % 2 === 0) {
            qipan[6][j] = N(6, j, qiZiColor.black, qiZiType.bing, qipan);
        } else {
            qipan[6][j] = null;
        }
    }
    qipan[7] = [];
    qipan[8] = [];
    for (let j = 0; j < 9; j++) {
        qipan[7][j] = null;
        qipan[8][j] = null;
    }
    qipan[7][1] = N(7, 1, qiZiColor.black, qiZiType.pao, qipan);
    qipan[7][7] = N(7, 7, qiZiColor.black, qiZiType.pao, qipan);
    qipan[9] = [
        N(9, 0, qiZiColor.black, qiZiType.ju, qipan),
        N(9, 1, qiZiColor.black, qiZiType.ma, qipan),
        N(9, 2, qiZiColor.black, qiZiType.xiang, qipan),
        N(9, 3, qiZiColor.black, qiZiType.si, qipan),
        N(9, 4, qiZiColor.black, qiZiType.jiang, qipan),
        N(9, 5, qiZiColor.black, qiZiType.si, qipan),
        N(9, 6, qiZiColor.black, qiZiType.xiang, qipan),
        N(9, 7, qiZiColor.black, qiZiType.ma, qipan),
        N(9, 8, qiZiColor.black, qiZiType.ju, qipan),
    ];

    return qipan as XQ_base[][];
}

function N(i: number, j: number, c: qiZiColor, t: qiZiType, qipan: (XQ_base | null)[][]) {
    let obj = {
        "i": i,
        "j": j,
        "c": c,
        "t": t,
        "qipan": qipan
    };
    let con: typeof XQ_base;
    switch (obj.t) {
        case qiZiType.ju:
            con = XQ_ju;
            break;
        case qiZiType.ma:
            con = XQ_ma;
            break;
        case qiZiType.xiang:
            con = XQ_xiang;
            break;
        case qiZiType.si:
            con = XQ_si;
            break;
        case qiZiType.jiang:
            con = XQ_jiang;
            break;
        case qiZiType.pao:
            con = XQ_pao;
            break;
        case qiZiType.bing:
            con = XQ_bing;
            break;
        default:
            throw Error("非法的象棋棋子类型：" + obj.t);
    }
    return new con(obj as any);
}



export enum qiZiColor {
    red = 1,    // 红方
    black = 2   // 黑方
}

export enum qiZiType {
    ju = 1, // 车
    ma,     // 马  
    xiang,  // 象
    si,     // 仕
    jiang,  // 将
    pao,    // 炮
    bing    // 兵
}


export class XQ_base {
    public qipan: XQ_base[][];
    public i: number;
    public j: number;
    public c: qiZiColor;
    public t: qiZiType;
    constructor(info: { 'i': number, 'j': number, 'c': qiZiColor, 't': qiZiType, "qipan": XQ_base[][] }) {
        this.i = info.i;
        this.j = info.j;
        this.c = info.c;
        this.t = info.t;
        this.qipan = info.qipan;
    }

    toJson() {
        return {
            "i": this.i,
            "j": this.j,
            "c": this.c,
            "t": this.t,
        };
    }

    /**
     * 想要去这里
     */
    go(i: number, j: number): boolean {
        return false;
    }
}

// 车
class XQ_ju extends XQ_base {
    constructor(info: { 'i': number, 'j': number, 'c': qiZiColor, 't': qiZiType, "qipan": XQ_base[][] }) {
        super(info);
    }

    go(i: number, j: number): boolean {
        let min = 0;
        let max = 0;
        if (i === this.i) {
            if (this.j < j) {
                min = this.j + 1;
                max = j - 1;
            } else {
                min = j + 1;
                max = this.j - 1;
            }
            for (let m = min; m <= max; m++) {
                if (this.qipan[i][m] !== null) {
                    return false;
                }
            }
        } else if (j === this.j) {
            if (this.i < i) {
                min = this.i + 1;
                max = i - 1;
            } else {
                min = i + 1;
                max = this.i - 1;
            }
            for (let m = min; m <= max; m++) {
                if (this.qipan[m][j] !== null) {
                    return false;
                }
            }
        } else {
            return false;
        }

        this.qipan[this.i][this.j] = null as any;
        this.i = i;
        this.j = j;
        this.qipan[this.i][this.j] = this;
        return true;
    }
}

// 马
class XQ_ma extends XQ_base {
    constructor(info: { 'i': number, 'j': number, 'c': qiZiColor, 't': qiZiType, "qipan": XQ_base[][] }) {
        super(info);
    }

    go(i: number, j: number): boolean {
        let diffI = Math.abs(this.i - i);
        let diffJ = Math.abs(this.j - j);
        if (diffI === 1 && diffJ === 2) {
            if (this.qipan[this.i][(this.j + j) / 2] !== null) {
                return false;
            }
        } else if (diffI === 2 && diffJ === 1) {
            if (this.qipan[(this.i + i) / 2][this.j] !== null) {
                return false;
            }
        } else {
            return false;
        }

        this.qipan[this.i][this.j] = null as any;
        this.i = i;
        this.j = j;
        this.qipan[this.i][this.j] = this;
        return true;
    }
}

// 象
class XQ_xiang extends XQ_base {
    constructor(info: { 'i': number, 'j': number, 'c': qiZiColor, 't': qiZiType, "qipan": XQ_base[][] }) {
        super(info);
    }

    go(i: number, j: number): boolean {
        if (Math.abs(this.i - i) !== 2 || Math.abs(this.j - j) !== 2) {
            return false;
        }
        if (this.qipan[(this.i + i) / 2][(this.j + j) / 2] !== null) {
            return false;
        }
        if (Math.floor(this.i / 5) !== Math.floor(i / 5)) {
            return false;
        }
        this.qipan[this.i][this.j] = null as any;
        this.i = i;
        this.j = j;
        this.qipan[this.i][this.j] = this;
        return true;
    }
}

// 仕
class XQ_si extends XQ_base {
    constructor(info: { 'i': number, 'j': number, 'c': qiZiColor, 't': qiZiType, "qipan": XQ_base[][] }) {
        super(info);
    }

    go(i: number, j: number): boolean {
        if (Math.abs(this.i - i) !== 1 || Math.abs(this.j - j) !== 1) {
            return false;
        }
        if (j < 3 || j > 5) {
            return false;
        }
        if (this.c === qiZiColor.red) {
            if (i > 2) {
                return false;
            }
        } else if (i < 7) {
            return false;
        }
        this.qipan[this.i][this.j] = null as any;
        this.i = i;
        this.j = j;
        this.qipan[this.i][this.j] = this;
        return true;
    }
}

// 将
class XQ_jiang extends XQ_base {
    constructor(info: { 'i': number, 'j': number, 'c': qiZiColor, 't': qiZiType, "qipan": XQ_base[][] }) {
        super(info);
    }

    go(i: number, j: number): boolean {
        if (Math.abs(this.i - i) + Math.abs(this.j - j) !== 1) {
            return false;
        }
        if (j < 3 || j > 5) {
            return false;
        }
        if (this.c === qiZiColor.red) {
            if (i > 2) {
                return false;
            }
        } else if (i < 7) {
            return false;
        }
        this.qipan[this.i][this.j] = null as any;
        this.i = i;
        this.j = j;
        this.qipan[this.i][this.j] = this;
        return true;
    }
}

// 炮
class XQ_pao extends XQ_base {
    constructor(info: { 'i': number, 'j': number, 'c': qiZiColor, 't': qiZiType, "qipan": XQ_base[][] }) {
        super(info);
    }

    go(i: number, j: number): boolean {
        let min = 0;
        let max = 0;
        let num = 0;
        if (i === this.i) {
            if (this.j < j) {
                min = this.j + 1;
                max = j - 1;
            } else {
                min = j + 1;
                max = this.j - 1;
            }
            for (let m = min; m <= max; m++) {
                if (this.qipan[i][m] !== null) {
                    num++;
                }
            }
        } else if (j === this.j) {
            if (this.i < i) {
                min = this.i + 1;
                max = i - 1;
            } else {
                min = i + 1;
                max = this.i - 1;
            }
            for (let m = min; m <= max; m++) {
                if (this.qipan[m][j] !== null) {
                    num++;
                }
            }
        } else {
            return false;
        }
        if (this.qipan[i][j] === null) {
            if (num > 0) {
                return false;
            }
        } else {
            if (num !== 1) {
                return false;
            }
        }

        this.qipan[this.i][this.j] = null as any;
        this.i = i;
        this.j = j;
        this.qipan[this.i][this.j] = this;
        return true;
    }
}

// 兵
class XQ_bing extends XQ_base {
    constructor(info: { 'i': number, 'j': number, 'c': qiZiColor, 't': qiZiType, "qipan": XQ_base[][] }) {
        super(info);
    }

    go(i: number, j: number): boolean {
        if (Math.abs(this.i - i) + Math.abs(this.j - j) !== 1) {
            return false;
        }
        if (j === this.j) {   // 直走
            if (this.c === qiZiColor.red) {
                if (this.i > i) {
                    return false;
                }
            } else if (this.i < i) {
                return false;
            }
        } else if (this.c === qiZiColor.red) {    // 横走
            if (i < 5) {
                return false;
            }
        } else if (i > 4) {
            return false;
        }

        this.qipan[this.i][this.j] = null as any;
        this.i = i;
        this.j = j;
        this.qipan[this.i][this.j] = this;
        return true;
    }
}