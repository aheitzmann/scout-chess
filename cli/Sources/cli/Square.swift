class Square : Hashable {
    static let minOrdinate = 0
    static let maxOrdinate = 7
    static let rankChars = ["a", "b", "c", "d", "e", "f", "g", "h"]

    let rank: Int   // ranks are rows.
    let file: Int   // files are columns.
    let color: Color
    var hashValue: Int {
        var hash = 17
        hash = 19 * hash + rank
        hash = 23 * hash + file
        return hash
    }

    init(rank: Int, file: Int) {
        self.rank = rank
        self.file = file
        self.color = (rank - file) % 2 == 0 ? Color.black : Color.white
        
        precondition(
            rank >= Square.minOrdinate && rank <= Square.maxOrdinate
            && file >= Square.minOrdinate && file <= Square.maxOrdinate, 
            "Rank and file should both be between 0 and 7. Given: rank: \(rank), file: \(file)")
    }

    func getPrintableColor() -> String {
        return self.color == Color.black ? "■" : "☐"
    }

    func getNotation() -> String {
        return "\(Square.rankChars[rank])\(file+1)" 
    }

    static func get(notation: String) -> Square {
        precondition(notation.count == 2, "Invalid notation for square: \(notation)")

        let rank = Square.rankChars.firstIndex(of: String(notation.prefix(1)).lowercased())
        let file = Int(String(notation.suffix(1)))
        precondition(
            rank != nil && file != nil, 
            "Invalid rank: \(rank as Optional), file: \(file as Optional) in notation: \(notation)")

        return Square(rank: rank!, file: file! - 1)
    }
}

func == (square1: Square, square2: Square) -> Bool {
    return square1.rank == square2.rank && square1.file == square2.file
}