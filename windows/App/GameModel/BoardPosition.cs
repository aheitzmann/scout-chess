namespace ScoutChess.GameModel
{
    /// <summary>
    /// Represents a logical position on the board, where row 0, column 0 corresponds to row 0, column A 
    /// in standard chess nomanclature. 
    /// </summary>
    internal struct BoardPosition : System.IEquatable<BoardPosition>
    {
        internal readonly uint Column;
        internal readonly uint Row;

        internal BoardPosition(uint col, uint row)
        {
            Column = col;
            Row = row;
        }

        public bool Equals(BoardPosition other)
        {
            return Column == other.Column && Row == other.Row;
        }

        public override string ToString()
        {
            return IO.SerializationHelper.SerializeBoardPosition(this);
        }
    }

}