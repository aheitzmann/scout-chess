using ScoutChess.GameController;
using ScoutChess.GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.IO
{
    static class SerializationHelper
    {
        private static readonly string KingsideCastleStr = "0-0";
        private static readonly string QueensideCastleStr = "0-0-0";
        private static readonly char[] ColumnCharacters = { 'A', 'B', 'C', 'D', 'E', 'F', 'H', 'G' };


        internal static string SerializeBoardPosition(BoardPosition position)
        {
            return ColumnCharacters[position.Column] + (position.Row + 1).ToString();
        }

        internal static BoardPosition DeserializeBoardPosition(string positionStr)
        {
            System.Diagnostics.Debug.Assert(positionStr.Length == 2);
            return new BoardPosition(
                (uint)Array.IndexOf(ColumnCharacters, positionStr[0]), 
                (uint)Int32.Parse(positionStr.Substring(1)) - 1);
        }

        internal static string SerializePieceCapabilities(IPieceCapabilities capabilities)
        {
            if (capabilities == PawnCapabilities.Singleton)
            {
                return "P";
            }
            else if (capabilities == KnightCapabilities.Singleton)
            {
                return "Kn";
            }
            else if (capabilities == BishopCapabilities.Singleton)
            {
                return "B";
            }
            else if (capabilities == RookCapabilities.Singleton)
            {
                return "R";
            }
            else if (capabilities == QueenCapabilities.Singleton)
            {
                return "Q";
            }
            else if (capabilities == KingCapabilities.Singleton)
            {
                return "K";
            }
            else
            {
                throw new InvalidOperationException("Unknown capabilities type.");
            }
        }

        internal static IPieceCapabilities DeserializePieceCapabilities(string capabilitiesStr)
        {
            switch (capabilitiesStr)
            {
                case "P":
                    return PawnCapabilities.Singleton;
                case "Kn":
                    return KnightCapabilities.Singleton;
                case "B":
                    return BishopCapabilities.Singleton;
                case "R":
                    return RookCapabilities.Singleton;
                case "Q":
                    return QueenCapabilities.Singleton;
                case "K":
                    return KingCapabilities.Singleton;
                default:
                    throw new InvalidOperationException("Unknown capabilities type");
            }
        }

        internal static string SerializeTurnRecord(TurnRecord turn)
        {
            if (turn.SecondaryMove != null)
            {
                if (turn.Move.To.Column >= 5)
                {
                    return KingsideCastleStr;
                }
                else
                {
                    return QueensideCastleStr;
                }
            }

            string turnStr = SerializationHelper.SerializeBoardPosition(turn.Move.From);
            if (turn.Capture != null)
            {
                turnStr += "x";
            }
            else
            {
                turnStr += "-";
            }

            turnStr += SerializationHelper.SerializeBoardPosition(turn.Move.To);
            if (turn.Promotion != null)
            {
                turnStr += "(" + SerializationHelper.SerializePieceCapabilities(turn.Promotion.NewCapabilities) + ")";
            }

            return turnStr;
        }

        //TODO: Note that this method has no way of filling in the fields of the turn record of 
        // type Piece. This means that the consumer of TurnRecords must be able to infer the 
        // pieces involved in the turn from the positional information included in the record. 
        // Furthermore, either the consumers of TurnRecord need to robust against null Piece
        // refs, or the Piece refs should be removed altogether from the TurnRecord.
        internal static TurnRecord DeserializeTurnRecord(string turnStr, Side side)
        {
            uint startRow = 0;
            MoveRecord primaryMove = null;
            MoveRecord secondaryMove = null;
            CaptureRecord capture = null;

            startRow = 0;
            if (side == Side.Black) { startRow = 7; }
            if (KingsideCastleStr.Equals(turnStr))
            {
                primaryMove = new MoveRecord(
                    null, new BoardPosition(4, startRow), new BoardPosition(6, startRow));
                secondaryMove = new MoveRecord(
                    null, new BoardPosition(7, startRow), new BoardPosition(5, startRow));
                return new TurnRecord(primaryMove, secondaryMove);
            }
            else if (QueensideCastleStr.Equals(turnStr))
            {
                primaryMove = new MoveRecord(
                    null, new BoardPosition(4, startRow), new BoardPosition(2, startRow));
                secondaryMove = new MoveRecord(
                    null, new BoardPosition(0, startRow), new BoardPosition(3, startRow));
                return new TurnRecord(primaryMove, secondaryMove);
            }

            char delimeter = '\0';
            if (turnStr.Contains("-"))
            {
                delimeter = '-';
            }
            else if (turnStr.Contains("x"))
            {
                delimeter = 'x';
                capture = new CaptureRecord(null);
            }
            else
            {
                throw new InvalidOperationException("The given string is not a valid representation of a move.");
            }

            string[] fromAndTo = turnStr.Split(delimeter);
            
            if (fromAndTo.Length != 2)
            {
                throw new InvalidOperationException("The given string is not a valid representation of a move.");
            }

            BoardPosition from = SerializationHelper.DeserializeBoardPosition(fromAndTo[0]);
            BoardPosition to = SerializationHelper.DeserializeBoardPosition(fromAndTo[1].Substring(0,2));

            if (fromAndTo[1].Length > 2)
            {
                // promotion
                var pieceStrLen = fromAndTo[1].Length - 4;
                System.Diagnostics.Debug.Assert(pieceStrLen > 0);
                var pieceCapabilities = DeserializePieceCapabilities(fromAndTo[1].Substring(3, pieceStrLen));
                return new TurnRecord(new MoveRecord(null, from, to), new PromotionRecord(null, pieceCapabilities));
            }
            else
            {
                return new TurnRecord(new MoveRecord(null, from, to), capture); 
            }
        }

        
    }
}
