namespace ScoutChess.Model
{
    /// <summary>
    /// Identifies the data store for a game with the combination of the IGameDataService
    /// for the particular type of game, and the GameID that uniquly identifies games of
    /// that type.
    /// </summary>
    internal struct GameDataStore
    {
        internal readonly IGameDataService GameDataService;
        internal readonly GameID GameID;

        internal GameDataStore(IGameDataService gameDataService, GameID gameID)
        {
            GameDataService = gameDataService;
            GameID = gameID;
        }
    }
}
