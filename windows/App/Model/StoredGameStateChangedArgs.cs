using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.Model
{
    internal class StoredGameStateChangedEventArgs
    {
        internal String TurnRecordStr
        {
            get;
            private set;
        }

        internal GameInformation UpdatedGameInfo
        {
            get;
            private set;
        }

        internal StoredGameStateChangedEventArgs(GameInformation updatedGameInfo, string turnRecordStr)
        {
            UpdatedGameInfo = updatedGameInfo;
            TurnRecordStr = turnRecordStr;
        }
    }
}
