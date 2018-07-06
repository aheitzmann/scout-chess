using ScoutChess.GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.GameView
{
    /// <summary>
    /// Represents a logical position on the board, where row 0, column 0 corresponds to row 0, column A 
    /// in standard chess nomanclature. 
    /// </summary>
    internal struct GridCellPosition : System.IEquatable<GridCellPosition>
    {

        internal readonly uint Column;
        internal readonly uint Row;

        internal GridCellPosition(uint col, uint row)
        {
            Column = col;
            Row = row;
        }

        public bool Equals(GridCellPosition other)
        {
            return Column == other.Column && Row == other.Row;
        }

       
    }
}
