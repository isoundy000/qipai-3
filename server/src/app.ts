import { createApp, Application, Session, connector } from "mydog"
let app = createApp();

import { log4js_init, mydogInnerLog, gameLog } from "./app/logger";
import initServer from "./app/common/serverInit";
import { serverType } from "./config/gameConfig";
import { getGameState } from "./app/util/gameUtil";
log4js_init(app);

app.serverToken = "aha";

app.setEncodeDecodeConfig({ "msgDecode": msgDecode, "msgEncode": msgEncode });
app.setConnectorConfig({ "connector": connector.connectorTcp, heartbeat: 10 });


app.configure(serverType.connector, function () {
    app.route(serverType.info, function (app: Application, session: Session, serverType: string, cb) {
        cb(session.get("infoId"));
    });
    app.route(serverType.match, function (app: Application, session: Session, serverType: string, cb) {
        cb(getGameState(session).gameSvr);
    });
    app.route(serverType.game, function (app: Application, session: Session, serverType: string, cb) {
        cb(getGameState(session).gameSvr);
    });
});


app.configure("all", function () {
    initServer(app);
});

app.onLog(function (level: string, info: string) {
    (mydogInnerLog as any)[level](app.serverId, info);
});

app.start();


process.on("uncaughtException", function (err: any) {
    gameLog.error(' caught exception: ' + err.stack)
});

function msgDecode(cmdId: number, msgBuf: Buffer): any {
    gameLog.debug("req--", app.routeConfig[cmdId], JSON.parse(msgBuf as any));
    return JSON.parse(msgBuf as any);
}

function msgEncode(cmdId: number, data: any): Buffer {
    gameLog.debug("rsp--", app.routeConfig[cmdId], JSON.stringify(data));
    return Buffer.from(JSON.stringify(data));
}


