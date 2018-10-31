enum Piece {
    case king
    case queen
    case bishop
    case knight
    case rook
    case pawn

    func getLegalMoves(square: Square) -> Set<Move> {
        switch self {
        case .king:
          return getLegalMovesForKing(square: square)
        case .queen:
          return getLegalMovesForQueen(square: square)
        case .bishop:
          return getLegalMovesForBishop(square: square)
        case .knight:
          return getLegalMovesForKnight(square: square)
        case .rook:
          return getLegalMovesForRook(square: square)
        case .pawn:
          return getLegalMovesForPawn(square: square)
        }
    }

    private func getLegalMovesForKing(square: Square) -> Set<Move> {
        return getAdjacent(square: square)
    }

    private func getLegalMovesForQueen(square: Square) -> Set<Move> {
        return getDiagonals(square: square).union(getOrthogonals(square: square))
    }

    private func getLegalMovesForBishop(square: Square) -> Set<Move> {
        return getDiagonals(square: square)
    }
    
    private func getLegalMovesForKnight(square: Square) -> Set<Move> {
        var squares = Set<Square>()
        var r = square.rank + 1
        if r <= Square.maxOrdinate {
            if square.file - 2 >= Square.minOrdinate {
                squares.insert(Square(rank: r, file: square.file - 2))
            }
            if square.file + 2 <= Square.maxOrdinate {
                squares.insert(Square(rank: r, file: square.file + 2))
            }

            r = r + 1
            if r <= Square.maxOrdinate {
                if square.file - 1 >= Square.minOrdinate {
                    squares.insert(Square(rank: r, file: square.file - 1))
                }
                if square.file + 1 <= Square.maxOrdinate {
                    squares.insert(Square(rank: r, file: square.file + 1))
                }
            }
        }

        r = square.rank - 1
        if r >= Square.minOrdinate {
            if square.file - 2 >= Square.minOrdinate {
                squares.insert(Square(rank: r, file: square.file - 2))
            }
            if square.file + 2 <= Square.maxOrdinate {
                squares.insert(Square(rank: r, file: square.file + 2))
            }
            
            r = r - 1
            if r >= Square.minOrdinate {
                if square.file - 1 >= Square.minOrdinate {
                    squares.insert(Square(rank: r, file: square.file - 1))
                }
                if square.file + 1 <= Square.maxOrdinate {
                    squares.insert(Square(rank: r, file: square.file + 1))
                }
            }
        }
        return getMoves(from: square, to: squares)
    }

    private func getLegalMovesForRook(square: Square) -> Set<Move> {
        return getOrthogonals(square: square)
    }

    private func getLegalMovesForPawn(square: Square) -> Set<Move> {
        precondition(false, "Piece-level moves for pawn are unsupported. Try board-level.")
        return Set<Move>()
    }

    private func getAdjacent(square: Square) -> Set<Move> {
        let minRank = max(Square.minOrdinate, square.rank - 1)
        let maxRank = min(Square.maxOrdinate, square.rank + 1)
        let minFile = max(Square.minOrdinate, square.file - 1)
        let maxFile = min(Square.maxOrdinate, square.file + 1)

        var squares = Set<Square>()
        for r in minRank...maxRank {
            for f in minFile...maxFile {
                if (r != square.rank || f != square.file) {
                    squares.insert(Square(rank: r, file: f))
                }
            }
        }

        return getMoves(from: square, to: squares)
    }

    private func getDiagonals(square: Square) -> Set<Move> {
        var squares = Set<Square>()

        // Above the square.
        if square.rank < Square.maxOrdinate {
            for slope in 1...(Square.maxOrdinate - square.rank) {
                if square.file + slope <= Square.maxOrdinate {
                    squares.insert(Square(
                        rank: square.rank + slope,
                        file: square.file + slope)) 
                }
                if square.file - slope >= Square.minOrdinate {
                    squares.insert(Square(
                        rank: square.rank + slope,
                        file: square.file - slope))
                }
            }
        }

        // Below the square.
        if square.rank > Square.minOrdinate {
            for slope in 1...(square.rank - Square.minOrdinate) {
                if square.file + slope <= Square.maxOrdinate {
                    squares.insert(Square(
                        rank: square.rank - slope,
                        file: square.file + slope)) 
                }
                if square.file - slope >= Square.minOrdinate {
                    squares.insert(Square(
                        rank: square.rank - slope,
                        file: square.file - slope)) 
                }
            }
        }

        return getMoves(from: square, to: squares)
    }

    private func getOrthogonals(square: Square) -> Set<Move> {
        var squares = Set<Square>()

        for x in Square.minOrdinate...Square.maxOrdinate {
            if square.file != x {
                squares.insert(Square(rank: square.rank, file: x))
            }
            if square.rank != x {
                squares.insert(Square(rank: x, file: square.file))
            }
        }

        return getMoves(from: square, to: squares)
    }

    private func getMoves(from: Square, to: Set<Square>) -> Set<Move> {
        var moves = Set<Move>()

        for square in to {
            moves.insert(Move(from: from, to: square))
        }

        return moves
    }
}