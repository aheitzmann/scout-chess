using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using ScoutChess.GameController;
using Windows.UI.Xaml;

namespace ScoutChess.GameModel
{
    internal class Piece
    {
        internal Side Side { get; set; } // TOOD finish work to remove PieceProperties

        /// <summary>
        /// The turn number in the game in which this piece was last moved. Uses in 
        /// determining legality of castling, two-space pawn moves, and en passant.
        /// </summary>
        internal uint TurnLastMoved { get; set; } // TODO: Finish work in model to support this
        
        internal IPieceCapabilities Capabilities { get; set; }
        internal FrameworkElement Visual { get; set; }
    }

    public enum Side : uint
    {
        White,
        Black,
    }

}
