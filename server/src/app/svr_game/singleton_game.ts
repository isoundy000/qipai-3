import { GameMgr } from "./gameMgr";

interface I_singleton_game {
    gameMgr: GameMgr,
}

let singleton_game: I_singleton_game = {
    gameMgr: null as any,
}

export default singleton_game
