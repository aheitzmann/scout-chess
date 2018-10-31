enum TurnState {
    case start
    case white
    case black
    case white_in_check
    case black_in_check
    case white_checkmated
    case black_checkmated
    case stalemated
}

class Turn {
    let state: TurnState
    let legalMoves: Set<Move>

    init(state: TurnState, legalMoves: Set<Move>) {
        self.state = state
        self.legalMoves = legalMoves
    }

    func isLegal(move: Move) -> Bool {
        return legalMoves.contains(move)
    }
}

typealias ConfirmedTurn = (turn: Turn, confirmed: Move)