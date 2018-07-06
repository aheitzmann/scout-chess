using ScoutChess.GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.GameView
{
    internal class PositionEventArgs : EventArgs
    {
        internal BoardPosition LogicalPosition { get; private set; }

        internal PositionEventArgs(BoardPosition logicalPosition)
        {
            LogicalPosition = logicalPosition;
        }
    }
}
