export let errCode = {
    "ok": 0,
    "dbErr": -1,
    "serverErr": -2,
    "createWaitTable": {
        "inGame": 1,
        "noServer": 2,
    },
    "enterFriendTable": {
        "inGame": 1,
        "noGame": 2,
        "noTable": 3,
        "friendNotInTable": 4,
        "inTable": 5,
        "tableFull": 6,
        "isMatching": 7,
        "inWaitTable": 8
    },
    "sendMsg": {
        "noRoom": 1,
        "noTable": 2,
        "notInTable": 3,
        "noFunc": 4
    }
}