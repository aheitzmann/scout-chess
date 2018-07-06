using ScoutChess.GameModel;
using System;

namespace ScoutChess.Model
{
    /// <summary>
    /// High-level information about the state of a game, provided by IGameDataService.
    /// </summary>
    internal struct GameInformation
    {
        internal readonly GameDataStore GameDataStore;
        internal readonly string FriendlyName;
        internal readonly GameType GameType;
        internal readonly DateTime TimeLastMoved;
        internal readonly uint MoveCount;
        internal readonly bool IsActive;
        internal Side NextToMove
        {
            get { return (MoveCount % 2) == 0 ? Side.White : Side.Black; }
        }

        internal GameInformation(GameDataStore gameDataStore, string friendlyName, GameType gameType, DateTime timeLastMoved, uint moveCount, bool isActive)
        {
            GameDataStore = gameDataStore;
            FriendlyName = friendlyName;
            GameType = gameType;
            TimeLastMoved = timeLastMoved;
            MoveCount = moveCount;
            IsActive = isActive;
        }
    }

    
}
