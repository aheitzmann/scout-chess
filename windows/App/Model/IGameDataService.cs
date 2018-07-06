using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace ScoutChess.Model
{

    internal interface IGameDataService
    {
        /// <summary>
        /// Fired when the stored state of a game has been modified.
        /// </summary>
        event EventHandler<StoredGameStateChangedEventArgs> GameStateChanged;

        /// <summary>
        /// Fired when a new game is added to the model
        /// </summary>
        event EventHandler<GameStartedEventArgs> GameStarted;

        /// <summary>
        /// Fired when an active game is completed
        /// </summary>
        event EventHandler<GameCompletedEventArgs> GameCompleted;

        /// <summary>
        /// Get a list of currently active games.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<GameDataStore>> GetActiveGamesAsync();

        /// <summary>
        /// Get a list of completed games.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<GameDataStore>> GetCompletedGamesAsync();

        /// <summary>
        /// Retrieves statistics about a saved game.
        /// </summary>
        /// <param name="gameID">The GameID identifying the game.</param>
        /// <returns>A GameInformation containing statistics about the saved game.</returns>
        Task<GameInformation> GetGameInformationAsync(GameID gameID);

        /// <summary>
        /// Retrieves the state of a saved game.
        /// </summary>
        /// <param name="gameID">The GameID identifying the game.</param>
        /// <returns>A string representation of the saved game with the given id,
        /// or null if there is no such game.</returns>
        Task<string> GetGameStateAsync(GameID gameID);

        /// <summary>
        /// Adds the specified turn record to the saved game state.
        /// </summary>
        /// <param name="gameID">The GameID identifying the game.</param>
        /// <param name="turnStr">A string representation of the turn record to save.</param>
        /// <returns>True if the turn record was successfully saved. False otherwise.</returns>
        Task<bool> StoreTurnRecordAsync(GameID gameID, string turnStr);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newGameInfo">Information describing the initial state of the game.</param>
        /// <returns></returns>
        /// 

        /// <summary>
        /// Adds a new game record to persistant storage.
        /// </summary>
        /// <param name="friendlyName">The friendly name of the game.</param>
        /// <returns>True if the new game was successfully saved. False otherwise</returns>
        Task<bool> StoreNewGameAsync(string friendlyName = null);

        /// <summary>
        /// Marks an existing active game as completed.
        /// </summary>
        /// <param name="gameID">The GameID identifying the active game.</param>
        /// <returns>True if the specified game was found and successfully marked as completed. False otherwise.</returns>
        Task<bool> CompleteGameAsync(GameID gameID);

        /// <summary>
        /// Gets the file for the specified game.
        /// </summary>
        /// <param name="gameID">The GameID identifying the game.</param>
        /// <returns>An IStorageFile contianing the game state if this IGameDataService
        /// supports the file format for saved games. Null otherwise.</returns>
        Task<IStorageFile> GetGameFileAsync(GameID gameID);
    }
}
