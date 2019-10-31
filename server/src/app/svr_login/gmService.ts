import { Application } from "mydog";
import * as http from "http"
import * as querystring from "querystring"
import { gameTypeState, gameType } from "../../config/gameConfig";

class GmMsgHandler {
    private app: Application;
    constructor(app: Application) {
        this.app = app;
    }

    openOrCloseOneGame(msg: { "gameId": number, "isOpen": boolean }, next: (data: any) => void) {
        this.app.rpc("gameMain").gameMain.gm.openOrCloseOneGame(msg.gameId, msg.isOpen);
    }

    changeGameTypeState(msg: { "gameType": gameType, "state": gameTypeState }, next: (data: any) => void) {
        this.app.rpc("gameMain").gameMain.gm.gmChangeGameTypeState(msg.gameType, msg.state);
    }
}




export class GmService {
    private port: number = 3001;
    private msgHandler: GmMsgHandler;

    constructor(app: Application) {
        this.msgHandler = new GmMsgHandler(app);
        this.startHttpSvr();
    }

    private startHttpSvr() {
        let server = http.createServer((request: http.IncomingMessage, response: http.ServerResponse) => {
            if (request.method !== "POST") {
                return;
            }
            let msg = "";
            request.on("data", (chuck) => {
                msg += chuck;
            });
            request.on("end", () => {
                let body = JSON.parse(msg) as any;
                console.log("------------", body);
                if (body && body.method && (this.msgHandler as any)[body.method]) {
                    (this.msgHandler as any)[body.method](body, this.callback(response));
                }
            });
        });
        server.listen(this.port, () => {
            console.log("--- login server (http)  running at port: " + this.port + ".");
        });
        server.on("error", function (err) {
            console.log("--- login server error::", err.message);
        });
    }

    private callback(response: http.ServerResponse): (data: any) => void {
        return function (data: any) {
            if (data === undefined) {
                data = null;
            }
            response.end(JSON.stringify(data));
        }
    }

}