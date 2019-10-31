import { ConnectorMgr } from "./connectorMgr";

interface I_singleton_connector {
    connectorMgr: ConnectorMgr
}

let singleton_connector: I_singleton_connector = {
    connectorMgr: null as any,
}

export default singleton_connector;
