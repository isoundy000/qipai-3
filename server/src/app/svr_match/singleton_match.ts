import { MatchMgr } from "./matchMgr";

interface I_singleton_match {
    matchMgr: MatchMgr,
}

let singleton_match: I_singleton_match = {
    matchMgr: null as any,
}

export default singleton_match
