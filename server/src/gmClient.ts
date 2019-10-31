import * as http from "http";
import * as querystring from "querystring";
import { gameType, gameTypeState } from "./config/gameConfig";


let postData = JSON.stringify({
    'method': 'changeGameTypeState',
    "gameType": gameType.wuZiQi,
    "state": gameTypeState.normal
});

let options = {
    hostname: '127.0.0.1',
    port: 3001,
    path: '',
    method: 'POST',
};

let req = http.request(options, (res) => {
    res.on('data', (chunk) => {
        console.log(`响应主体: ${chunk}`);
    });
    res.on('end', () => {
        console.log('响应中已无数据。');
    });
});

req.on('error', (e) => {
    console.error(`请求遇到问题: ${e.message}`);
});

// 写入数据到请求主体
req.write(postData);
req.end();