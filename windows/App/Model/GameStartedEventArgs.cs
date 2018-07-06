using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.Model
{
    internal class GameStartedEventArgs
    {
        internal GameInformation NewGameInfo { get; private set; }

        internal GameStartedEventArgs(GameInformation newGameInfo)
        {
            NewGameInfo = newGameInfo;
        }
    }
}
