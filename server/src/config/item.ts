export let Items: { [id: string]: I_itemInfo } = {
    "1001": {
        "name": "豆子",
        "des": "游戏通用货币",
    },
    "1002": {
        "name": "钻石",
        "des": "游戏贵重物品",
    }
}
export let ItemId = {
    "douzi": 1001,
    "diamond": 1002
}


interface I_itemInfo {
    "name": string,
    "des": string,
}