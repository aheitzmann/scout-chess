using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScoutChess.GameController;

namespace ScoutChess.GameModel
{
    internal class TurnRecord
    {
        internal readonly MoveRecord Move;
        internal readonly MoveRecord SecondaryMove;
        internal readonly CaptureRecord Capture;
        internal readonly PromotionRecord Promotion;
 
        internal TurnRecord(MoveRecord move) : this (move, null, null, null)
        {}

        internal TurnRecord(MoveRecord move, CaptureRecord capture) : this (move, null, capture, null)
        {}

        internal TurnRecord(MoveRecord move, PromotionRecord promotion) : this (move, null, null, promotion)
        {}

        internal TurnRecord(MoveRecord move, MoveRecord secondaryMove) : this (move, secondaryMove, null, null)
        {}

        internal TurnRecord(MoveRecord move, CaptureRecord capture, PromotionRecord promotion) : this (move, null, capture, promotion)
        {}

        private TurnRecord(MoveRecord move, MoveRecord secondaryMove, CaptureRecord capture, PromotionRecord promotion)
        {
            Move = move;
            SecondaryMove = secondaryMove;
            Capture = capture;
            Promotion = promotion;
        }
    }

    internal class MoveRecord
    {
        internal Piece PieceMoved { get; private set; }
        internal readonly BoardPosition From;
        internal readonly BoardPosition To;

        internal MoveRecord(Piece pieceMoved, BoardPosition from, BoardPosition to)
        {
            PieceMoved = pieceMoved;
            From = from;
            To = to;
        }

        internal void UpdatePieceMoved(IGameState gameStateBefore)
        {
            PieceMoved = gameStateBefore.GetPieceAt(From);
        }
    }

    internal class CaptureRecord
    {
        internal Piece PieceCaptured { get; private set; }

        internal CaptureRecord(Piece pieceCaptured)
        {
            PieceCaptured = pieceCaptured;
        }

        internal void UpdatePieceCaptured(MoveRecord captureMove, IGameState gameStateBefore)
        {
            PieceCaptured = gameStateBefore.GetPieceAt(captureMove.To);
        }
    }

    internal class PromotionRecord
    {
        internal Piece PiecePromoted { get; private set; }
        internal IPieceCapabilities OldCapabilities { get; private set; }
        internal readonly IPieceCapabilities NewCapabilities;

        internal PromotionRecord(Piece piecePromoted, IPieceCapabilities newCapabilities)
        {
            PiecePromoted = piecePromoted;
            if (piecePromoted != null)
            {
                OldCapabilities = piecePromoted.Capabilities;
            }
            NewCapabilities = newCapabilities;
        }

        internal void UpdatePiecePromoted(MoveRecord promoteMove, IGameState gameStateBefore)
        {
            PiecePromoted = gameStateBefore.GetPieceAt(promoteMove.From);
            OldCapabilities = PiecePromoted.Capabilities;
        }
    }
}
