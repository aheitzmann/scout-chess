class Board : CustomStringConvertible {
    private static let range = (Square.minOrdinate...Square.maxOrdinate)
    private static let reverseRange = Board.range.reversed()
    private var board = 
        [[ColoredPiece?]](repeating: [ColoredPiece?](repeating: nil, count: 8), count: 8)
    private var history: [ConfirmedTurn]
    private var turn: Turn

    init() {
        // Rooks
        board[0][0] = ColoredPiece(color: Color.white, piece: Piece.rook)
        board[0][7] = ColoredPiece(color: Color.white, piece: Piece.rook)
        board[7][0] = ColoredPiece(color: Color.black, piece: Piece.rook)
        board[7][7] = ColoredPiece(color: Color.black, piece: Piece.rook)

        // Knights
        board[0][1] = ColoredPiece(color: Color.white, piece: Piece.knight)
        board[0][6] = ColoredPiece(color: Color.white, piece: Piece.knight)
        board[7][1] = ColoredPiece(color: Color.black, piece: Piece.knight)
        board[7][6] = ColoredPiece(color: Color.black, piece: Piece.knight)

        // Bishops
        board[0][2] = ColoredPiece(color: Color.white, piece: Piece.bishop)
        board[0][5] = ColoredPiece(color: Color.white, piece: Piece.bishop)
        board[7][2] = ColoredPiece(color: Color.black, piece: Piece.bishop)
        board[7][5] = ColoredPiece(color: Color.black, piece: Piece.bishop)

        // Queens
        board[0][3] = ColoredPiece(color: Color.white, piece: Piece.queen)
        board[7][3] = ColoredPiece(color: Color.black, piece: Piece.queen)

        // Kings
        board[0][4] = ColoredPiece(color: Color.white, piece: Piece.king)
        board[7][4] = ColoredPiece(color: Color.black, piece: Piece.king)

        // Pawns
        for file in Board.range {
            board[1][file] = ColoredPiece(color: Color.white, piece: Piece.pawn)
            board[6][file] = ColoredPiece(color: Color.black, piece: Piece.pawn)
        }

        history = []
        turn = Turn(state: TurnState.start, legalMoves: Set<Move>());
        updateTurn()
    }

    private func updateTurn() {
        if turn.state == TurnState.start {
            turn = Turn(state: TurnState.white, legalMoves: getLegalMoves(color: Color.white))
        }
    }

    private func getLegalMoves(color: Color) -> Set<Move> {
        var legalMoves = Set<Move>()
        for rank in Board.range {
            for file in Board.range {
                if board[rank][file] != nil {
                    let piece = board[rank][file]!
                    if piece.color == color {
                        let square = Square(rank: rank, file: file)
                        let pieceLegalMoves = piece.getLegalMoves(square: square)
                        legalMoves = legalMoves.union(pieceLegalMoves)
                    }
                }
            }
        }
        return legalMoves
    }

    func confirmMove(move: Move) -> Bool {
        if turn.legalMoves.contains(move) {
            board[move.to.rank][move.to.file] = board[move.from.rank][move.from.file]
            board[move.from.rank][move.from.file] = nil
            history.append(ConfirmedTurn(turn: turn, confirmed: move))
            updateTurn()
            return true
        } else {
            return false
        }
    }

    var description: String {
        var desc = "\n\t"
        desc += "    a  b  c  d  e  f  g  h\n\t"
        var index = 8
        for rank in Board.reverseRange {
            desc += " \(index) "
            for file in Board.range {
                if board[rank][file] == nil {
                    desc += " \(Square(rank: rank, file: file).getPrintableColor()) "
                } else {
                    desc += " \(board[rank][file]!) "
                }
            }
            desc += " \(index) "
            desc += "\n\t"
            index -= 1
        }
        desc += "    a  b  c  d  e  f  g  h\n\t"
        return desc
    }
}