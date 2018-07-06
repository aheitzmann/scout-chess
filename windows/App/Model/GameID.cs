namespace ScoutChess.Model
{
    internal enum GameType
    {
        OfflineTwoPlayer,
        Online,
    }

    /// <summary>
    /// A unique idendifier for a game of a particular type. A GameID is used to identify a game
    /// with an IGameDataService. The members of this struct should only be accessed by implementations
    /// of IGameDataService.
    /// </summary>
    internal struct GameID
    {
        private readonly GameType _gameType;
        private readonly string _offlineGameID;
        private readonly ulong _onlineGameID;

        internal GameID(GameType gameType, string gameID)
        {
            _gameType = gameType;
            _offlineGameID = null;
            _onlineGameID = 0;

            switch (gameType)
            {
                case GameType.OfflineTwoPlayer:
                    _offlineGameID = gameID;
                    break;
                case GameType.Online:
                    _onlineGameID = ulong.Parse(gameID);
                    break;
            }
        }

        internal GameID(GameType gameType, ulong gameID)
        {
            _gameType = gameType;
            _offlineGameID = null;
            _onlineGameID = 0;

            switch (gameType)
            {
                case GameType.OfflineTwoPlayer:
                    System.Diagnostics.Debug.Assert(false, "ulong is not a valid gameID form for an offline twoplayer game.");
                    break;
                case GameType.Online:
                    _onlineGameID = gameID;
                    break;
            }
        }

        internal ulong GetOnlineGameID()
        {
            System.Diagnostics.Debug.Assert(_gameType == GameType.Online);
            return _onlineGameID;
        }

        internal string GetOfflineTwoPlayerGameID()
        {
            System.Diagnostics.Debug.Assert(_gameType == GameType.OfflineTwoPlayer);
            return _offlineGameID;

        }

    }
}