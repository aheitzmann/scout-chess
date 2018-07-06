using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.GameModel
{
    internal class StateChangedEventArgs :  EventArgs
    {
        internal TurnRecord TurnRecord { get; private set; }
        internal IGameState GameState { get; private set; }

        internal StateChangedEventArgs(TurnRecord turnRecord, IGameState gameState)
        {
            TurnRecord = turnRecord;
            GameState = gameState;
        }
    }
}
