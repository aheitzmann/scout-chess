class ColoredPiece : CustomStringConvertible {
    let color: Color
    let piece: Piece
    
    init(color: Color, piece: Piece) {
        self.color = color
        self.piece = piece
    }

    func getLegalMoves(square: Square) -> Set<Move> {
        return piece.getLegalMoves(square: square)
    }

    var description: String {
        switch piece {
        case Piece.king:
          return color == Color.white ? "♔" : "♚"
        case Piece.queen:
          return color == Color.white ? "♕" : "♛"
        case Piece.bishop:
          return color == Color.white ? "♗" : "♝"
        case Piece.knight:
          return color == Color.white ? "♘" : "♞"
        case Piece.rook:
          return color == Color.white ? "♖" : "♜"
        case Piece.pawn:
          return color == Color.white ? "♙" : "♟"
        }
    }
}