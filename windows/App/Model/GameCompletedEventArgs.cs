using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.Model
{
    internal class GameCompletedEventArgs
    {
        internal GameInformation CompletedGameInfo { get; private set; }

        internal GameCompletedEventArgs(GameInformation completedGameInfo)
        {
            CompletedGameInfo = completedGameInfo;
        }
    }
}
