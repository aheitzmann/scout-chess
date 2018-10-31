class Move: Hashable {
    let from: Square
    let to: Square
    var hashValue: Int {
        var hash = 17
        hash = 19 * from.hashValue + hash
        hash = 23 * to.hashValue + hash
        return hash
    }

    init(from: Square, to: Square) {
        self.from = from
        self.to = to
    }

    func getNotation() -> String {
        return "\(from.getNotation())\(to.getNotation())" 
    }

    static func get(notation: String) -> Move {
        precondition(notation.count == 4, "Invalid notation for move: \(notation)")

        let from = Square.get(notation: String(notation.prefix(2)))
        let to = Square.get(notation: String(notation.suffix(2)))
        return Move(from: from, to: to)
    }
}

func == (move1: Move, move2: Move) -> Bool {
    return move1.from == move2.from && move1.to == move2.to
}