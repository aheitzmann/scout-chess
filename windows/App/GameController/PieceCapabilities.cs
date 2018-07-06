using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScoutChess.GameModel;
using ScoutChess.GameView;

namespace ScoutChess.GameController
{
    internal enum Visibility
    {
        Restricted,
    }

    internal enum PieceType
    {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    /// <summary>
    /// Defines the movement, attack, and vision capabilites of a piece. Game state may be
    /// taken into account to enable situational capabilities such as castling and en passant.
    /// However, game-level invariants are enforced by the game controller, and are not 
    /// considered here. For example, piece capabilities should not account for the requirement 
    /// that a player cannot make a move that leaves him in check.
    /// </summary>
    internal interface IPieceCapabilities
    {
        PieceType Type { get; }
        IEnumerable<BoardPosition> GetMoves(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state);
        IEnumerable<BoardPosition> GetAttacks(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, bool includeEmptyDestinationPositions);
        
        // This check can be more efficient than getting the full list of attackable positions in some cases
        bool CanAttack(BoardPosition position, BoardPosition from, Side side, uint turnLastMoveds, IGameState state);
        IEnumerable<BoardPosition> GetVisibleSquares(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, Visibility visibility);
    }

    internal abstract class PieceCapabilitiesBase : IPieceCapabilities
    {
        public PieceType Type { get { throw new NotImplementedException(); } }

        public abstract IEnumerable<BoardPosition> GetMoves(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state);

        public abstract IEnumerable<BoardPosition> GetAttacks(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, bool includeEmptyDestinationPositions);

        public abstract bool CanAttack(BoardPosition position, BoardPosition from, Side side, uint turnLastMoved, IGameState state);

        public virtual IEnumerable<BoardPosition> GetVisibleSquares(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, Visibility visibility)
        {
            var visibleSquares = GetMoves(currentPosition, side, turnLastMoved, state);
            visibleSquares = visibleSquares.Union<BoardPosition>(GetAttacks(currentPosition, side, turnLastMoved, state, true));
            return visibleSquares;
        }

        /// <summary>
        /// Helper
        /// </summary>
        /// <param name="state"></param>
        /// <param name="list"></param>
        /// <param name="toCheck"></param>
        /// <param name="addIfOccupiedBySide">Indicates that if toCheck is occupied by a piece on Side side, the position should still be added to the list.</param>
        /// <param name="side"></param>
        /// <returns>True if toCheck was occupied by another piece or is out of bounds. False otherwise.</returns>
        protected internal bool CheckInBoundsSquareOccupiedAndAdd(IGameState state, List<BoardPosition> list, BoardPosition toCheck, bool addIfUnoccupied, bool addIfOccupiedByOtherSide, Side side)
        {
            if (toCheck.Column >= 0 && toCheck.Column < Board.NumColumns && toCheck.Row >= 0 && toCheck.Row < Board.NumRows)
            {
                var piece = state.GetPieceAt(toCheck);
                if (piece == null)
                {
                    if (addIfUnoccupied)
                    {
                        list.Add(toCheck);
                    }
                    return false;
                }
                else
                {
                    if (addIfOccupiedByOtherSide && piece.Side != side)
                    {
                        list.Add(toCheck);
                    }

                    return true;
                }
            }

            return true; ;
        }

        protected internal bool IsPieceBetween(BoardPosition position1, BoardPosition position2, IGameState state)
        {
            System.Diagnostics.Debug.Assert(
                position1.Row == position2.Row || 
                position1.Column == position2.Column || 
                Math.Abs(position2.Row - position1.Row) == Math.Abs(position2.Column - position1.Column));

            var rowdiff = position2.Row - position1.Row;
            var coldiff = position2.Column - position1.Column;
            var steps = Math.Max(Math.Abs(rowdiff), Math.Abs(coldiff));
            var rowStep = rowdiff / steps;
            var colStep = coldiff / steps;

            uint curRow = (uint)(position1.Row + rowStep);
            uint curCol = (uint)(position1.Column + colStep);
            while (curRow != position2.Row || curCol != position2.Column)
            {
                if (state.GetPieceAt(new BoardPosition(curCol, curRow)) != null)
                {
                    return true;
                }

                curRow = (uint)(curRow + rowStep);
                curCol = (uint)(curCol + colStep);
            }
            
            return false;
        }
    }



    internal class RookCapabilities : PieceCapabilitiesBase
    {
        private static RookCapabilities singleton;

        internal static RookCapabilities Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new RookCapabilities();
                }
                return singleton;
            }
        }

        private RookCapabilities()
        {

        }

        public PieceType Type { get { return PieceType.Rook; } }

        public override IEnumerable<BoardPosition> GetMoves(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state)
        {
            var moves = new List<BoardPosition>();

            for (uint row = currentPosition.Row + 1; row < Board.NumRows; row++)
            {
                if (CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column, row), true, false, Side.White))
                    break;
            }

            for (uint row = currentPosition.Row - 1; row >= 0; row--)
            {
                if (CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column, row), true, false, Side.White))
                    break;
            }

            for (uint col = currentPosition.Column + 1; col < Board.NumColumns; col++)
            {
                if (CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(col, currentPosition.Row), true, false, Side.White))
                    break;
            }

            for (uint col = currentPosition.Column - 1; col >= 0; col--)
            {
                if (CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(col, currentPosition.Row), true, false, Side.White))
                    break;
            }

            return moves;
        }

        public override IEnumerable<BoardPosition> GetAttacks(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, bool includeEmptyDestinationPositions)
        {
            var moves = new List<BoardPosition>();
            for (uint row = currentPosition.Row + 1; row < Board.NumRows; row++)
            {
                if (CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column, row), includeEmptyDestinationPositions, true, side))
                    break;
            }

            for (uint row = currentPosition.Row - 1; row >= 0; row--)
            {
                if (CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column, row), includeEmptyDestinationPositions, true, side))
                    break;
            }

            for (uint col = currentPosition.Column + 1; col < Board.NumColumns; col++)
            {
                if (CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(col, currentPosition.Row), includeEmptyDestinationPositions, true, side))
                    break;
            }

            for (uint col = currentPosition.Column - 1; col >= 0; col--)
            {
                if (CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(col, currentPosition.Row), includeEmptyDestinationPositions, true, side))
                    break;
            }

            return moves;
        }



        public override bool CanAttack(BoardPosition position, BoardPosition from, Side side, uint turnLastMoved, IGameState state)
        {
            if (position.Equals(from))
                return false;

            uint start, end;
            if (position.Row == from.Row)
            {
                if (position.Column > from.Column)
                {
                    start = from.Column + 1;
                    end = position.Column - 1;
                }
                else
                {
                    start = position.Column + 1;
                    end = from.Column - 1;
                }
                for (uint col = start; col <= end; col++)
                {
                    if (state.GetPieceAt(new BoardPosition(col, position.Row)) != null)
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (position.Column == from.Column)
            {
                if (position.Row > from.Row)
                {
                    start = from.Row + 1;
                    end = position.Row - 1;
                }
                else
                {
                    start = position.Row + 1;
                    end = from.Row - 1;
                }
                for (uint row = start; row <= end; row++)
                {
                    if (state.GetPieceAt(new BoardPosition(position.Column, row)) != null)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }

    internal class KnightCapabilities : PieceCapabilitiesBase
    {
        private static KnightCapabilities singleton;

        internal static KnightCapabilities Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new KnightCapabilities();
                }
                return singleton;
            }
        }

        private KnightCapabilities()
        {

        }

        public PieceType Type { get { return PieceType.Knight; } }

        public override IEnumerable<BoardPosition> GetMoves(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state)
        {
            var moves = new List<BoardPosition>();
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column - 2, currentPosition.Row - 1), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column - 2, currentPosition.Row + 1), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column - 1, currentPosition.Row - 2), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column - 1, currentPosition.Row + 2), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column + 1, currentPosition.Row - 2), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column + 1, currentPosition.Row + 2), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column + 2, currentPosition.Row - 1), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column + 2, currentPosition.Row + 1), true, false, Side.White);

            return moves;
        }

        private void CheckSquareOccupiedInBoundsAndAdd(BoardPosition position, List<BoardPosition> list, IGameState state, bool addIfUnoccupied, bool addIfOccupiedByOtherSide, Side side)
        {
            if (position.Column >= 0 && position.Column < Board.NumColumns && position.Row >= 0 && position.Row < Board.NumRows)
            {
                var piece = state.GetPieceAt(position);
                if (piece == null && addIfUnoccupied)
                {
                    list.Add(position);
                }
                else if (piece != null && addIfOccupiedByOtherSide && piece.Side != side)
                {
                    list.Add(position);
                }
            }
        }

        public override IEnumerable<BoardPosition> GetAttacks(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, bool includeEmptyDestinationPositions)
        {
            var attacks = new List<BoardPosition>();
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column - 2, currentPosition.Row - 1), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column - 2, currentPosition.Row + 1), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column - 1, currentPosition.Row - 2), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column - 1, currentPosition.Row + 2), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column + 1, currentPosition.Row - 2), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column + 1, currentPosition.Row + 2), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column + 2, currentPosition.Row - 1), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column + 2, currentPosition.Row + 1), includeEmptyDestinationPositions, true, side);

            return attacks;
        }

        public override bool CanAttack(BoardPosition position, BoardPosition from, Side side, uint turnLastMoved, IGameState state)
        {
            var colDiff = Math.Abs(from.Column - position.Column);
            var rowDiff = Math.Abs(from.Row - position.Row);
            return from.Column != position.Column && from.Row != position.Row && colDiff + rowDiff == 3;
        }
    }

    internal class BishopCapabilities : PieceCapabilitiesBase
    {
        private static BishopCapabilities singleton;

        internal static BishopCapabilities Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new BishopCapabilities();
                }
                return singleton;
            }
        }

        private BishopCapabilities()
        {

        }

        public override IEnumerable<BoardPosition> GetMoves(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state)
        {
            var moves = new List<BoardPosition>();

            for (uint offset = 1; !CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column + offset, currentPosition.Row + offset), true, false, Side.White); offset++)
            { continue;  }

            for (uint offset = 1; !CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column + offset, currentPosition.Row - offset), true, false, Side.White); offset++)
            { continue; }

            for (uint offset = 1; !CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column - offset, currentPosition.Row + offset), true, false, Side.White); offset++)
            { continue; }

            for (uint offset = 1; !CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column - offset, currentPosition.Row - offset), true, false, Side.White); offset++)
            { continue; }

            return moves;
        }

        public override IEnumerable<BoardPosition> GetAttacks(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, bool includeEmptyDestinationPositions)
        {
            var attacks = new List<BoardPosition>();

            for (uint offset = 1; !CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column + offset, currentPosition.Row + offset), includeEmptyDestinationPositions, true, side); offset++)
            { continue; }

            for (uint offset = 1; !CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column + offset, currentPosition.Row - offset), includeEmptyDestinationPositions, true, side); offset++)
            { continue; }

            for (uint offset = 1; !CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column - offset, currentPosition.Row + offset), includeEmptyDestinationPositions, true, side); offset++)
            { continue; }

            for (uint offset = 1; !CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column - offset, currentPosition.Row - offset), includeEmptyDestinationPositions, true, side); offset++)
            { continue; }

            return attacks;
        }

        public override bool CanAttack(BoardPosition position, BoardPosition from, Side side, uint turnLastMoved, IGameState state)
        {
            var colOffset = position.Column - from.Column;
            var rowOffset = position.Row - from.Row;
            if (Math.Abs(colOffset) != Math.Abs(rowOffset))
            {
                return false;
            }

            var colOffsetDirection = colOffset / Math.Abs(colOffset);
            var rowOffsetDirection = rowOffset / Math.Abs(rowOffset);
            for (int offset = 1; offset < Math.Abs(colOffset); offset++)
            {
                if (state.GetPieceAt(new BoardPosition((uint)(from.Column + colOffsetDirection * offset), (uint)(from.Row + rowOffsetDirection * offset))) != null)
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal class KingCapabilities : PieceCapabilitiesBase
    {
        private static KingCapabilities singleton;

        internal static KingCapabilities Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new KingCapabilities();
                }
                return singleton;
            }
        }

        private KingCapabilities()
        {

        }

        public PieceType Type { get { return PieceType.King; } }

        public override IEnumerable<BoardPosition> GetMoves(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state)
        {
            var moves = new List<BoardPosition>();
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column - 1, currentPosition.Row - 1), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column - 1, currentPosition.Row), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column - 1, currentPosition.Row + 1), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column, currentPosition.Row - 1), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column, currentPosition.Row), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column, currentPosition.Row + 1), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column + 1, currentPosition.Row - 1), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column + 1, currentPosition.Row), true, false, Side.White);
            CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition(currentPosition.Column + 1, currentPosition.Row + 1), true, false, Side.White);

            if (turnLastMoved == 0)
            {
                // castling
                var castlePosition = new BoardPosition(0, currentPosition.Row);
                
                if (state.GetPieceAt(castlePosition) != null && 
                    state.GetPieceAt(castlePosition).TurnLastMoved == 0 && 
                    !IsPieceBetween(currentPosition, castlePosition, state))
                {

                    // Todo check king cannot cross check
                    CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition((uint)(currentPosition.Column - 2), currentPosition.Row), true, false, Side.White);
                }

                castlePosition = new BoardPosition(7, currentPosition.Row);
                if (state.GetPieceAt(castlePosition) != null &&
                    state.GetPieceAt(castlePosition).TurnLastMoved == 0 &&
                    !IsPieceBetween(currentPosition, castlePosition, state))
                {

                    // Todo check king cannot cross check
                    CheckInBoundsSquareOccupiedAndAdd(state, moves, new BoardPosition((uint)(currentPosition.Column + 2), currentPosition.Row), true, false, Side.White);
                }
            }
            return moves;
        }

        public override IEnumerable<BoardPosition> GetAttacks(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, bool includeEmptyDestinationPositions)
        {
            var attacks = new List<BoardPosition>();
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column - 1, currentPosition.Row - 1), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column - 1, currentPosition.Row), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column - 1, currentPosition.Row + 1), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column, currentPosition.Row - 1), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column, currentPosition.Row), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column, currentPosition.Row + 1), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column + 1, currentPosition.Row - 1), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column + 1, currentPosition.Row), includeEmptyDestinationPositions, true, side);
            CheckInBoundsSquareOccupiedAndAdd(state, attacks, new BoardPosition(currentPosition.Column + 1, currentPosition.Row + 1), includeEmptyDestinationPositions, true, side);

            return attacks;
        }

        public override bool CanAttack(BoardPosition position, BoardPosition from, Side side, uint turnLastMoved, IGameState state)
        {
            return !from.Equals(position) && Math.Abs(position.Column - from.Column) <= 1 && Math.Abs(position.Row - from.Row) <= 1;
        }
    }

    internal class QueenCapabilities : PieceCapabilitiesBase
    {
        private static QueenCapabilities singleton;

        internal static QueenCapabilities Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new QueenCapabilities();
                }
                return singleton;
            }
        }

        private QueenCapabilities()
        {

        }

        public PieceType Type { get { return PieceType.Queen; } }

        public override IEnumerable<BoardPosition> GetMoves(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state)
        {
            return RookCapabilities.Singleton.GetMoves(currentPosition, side, turnLastMoved, state).Union(BishopCapabilities.Singleton.GetMoves(currentPosition, side, turnLastMoved, state));
        }

        public override IEnumerable<BoardPosition> GetAttacks(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, bool includeEmptyDestinationPositions)
        {
            return RookCapabilities.Singleton.GetAttacks(currentPosition, side, turnLastMoved, state, includeEmptyDestinationPositions).Union(BishopCapabilities.Singleton.GetAttacks(currentPosition, side, turnLastMoved, state, includeEmptyDestinationPositions));

        }

        public override bool CanAttack(BoardPosition position, BoardPosition from, Side side, uint turnLastMoved, IGameState state)
        {
            return RookCapabilities.Singleton.CanAttack(position, from, side, turnLastMoved, state) || BishopCapabilities.Singleton.CanAttack(position, from, side, turnLastMoved, state);
        }
    }

    internal class PawnCapabilities : PieceCapabilitiesBase
    {
        private static PawnCapabilities singleton;

        internal static PawnCapabilities Singleton
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new PawnCapabilities();
                }
                return singleton;
            }
        }

        private PawnCapabilities()
        {

        }

        public PieceType Type { get { return PieceType.Pawn; } }

        public override IEnumerable<BoardPosition> GetMoves(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state)
        {
            var direction = side == Side.White? 1 : -1;
            var moves = new List<BoardPosition>();
            var dest = new BoardPosition(currentPosition.Column, (uint)(currentPosition.Row + (1 * direction)));
            if (state.GetPieceAt(dest) == null)
            {
                moves.Add(dest);
                if (turnLastMoved == 0)
                {
                    dest = new BoardPosition(currentPosition.Column, (uint)(currentPosition.Row + (2 * direction)));
                    if (state.GetPieceAt(dest) == null)
                    {
                        moves.Add(dest);
                    }
                }
            }

            return moves;
        }

        public override IEnumerable<BoardPosition> GetAttacks(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, bool includeEmptyDestinationPositions)
        {
            var direction = side == Side.White ? 1 : -1;
            var attacks = new List<BoardPosition>();
            BoardPosition dest;
            if (currentPosition.Column != 0)
            {
                dest = new BoardPosition((uint)(currentPosition.Column - 1), (uint)(currentPosition.Row + (1 * direction)));
                if (includeEmptyDestinationPositions || 
                    (state.GetPieceAt(dest) != null && state.GetPieceAt(dest).Side != side))
                {
                    attacks.Add(dest);
                }
            }
            if (currentPosition.Column != Board.NumColumns - 1)
            {
                dest = new BoardPosition((uint)(currentPosition.Column + 1), (uint)(currentPosition.Row + (1 * direction)));
                if (includeEmptyDestinationPositions ||
                    (state.GetPieceAt(dest) != null && state.GetPieceAt(dest).Side != side))
                {
                    attacks.Add(dest);
                }
            }

            return attacks;
        }

        public override IEnumerable<BoardPosition> GetVisibleSquares(BoardPosition currentPosition, Side side, uint turnLastMoved, IGameState state, Visibility visibility)
        {
            var direction = side == Side.White ? 1 : -1;
            var visibleSquares = new List<BoardPosition>();
            var dest = new BoardPosition(currentPosition.Column, (uint)(currentPosition.Row + (1 * direction)));
            visibleSquares.Add(dest);
            
            if (state.GetPieceAt(dest) == null && turnLastMoved == 0)
            {
                dest = new BoardPosition(currentPosition.Column, (uint)(currentPosition.Row + (2 * direction)));
                visibleSquares.Add(dest);
            }

            return visibleSquares.Union(GetAttacks(currentPosition, side, turnLastMoved, state, false));
        }

        public override bool CanAttack(BoardPosition position, BoardPosition from, Side side, uint turnLastMoved, IGameState state)
        {
            return GetAttacks(from, side, turnLastMoved, state, true).Contains(position);
        }
    }
}
