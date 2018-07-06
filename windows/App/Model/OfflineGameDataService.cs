using ScoutChess.GameModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;

namespace ScoutChess.Model
{
    internal class OfflineGameDataService : IGameDataService
    {
        private const string ActiveGamesFolderName = "ActiveGames";
        private const string CompletedGamesFolderName = "CompletedGames";

        private EventHandler<StoredGameStateChangedEventArgs> _gameStateChangedHandler;
        public event EventHandler<StoredGameStateChangedEventArgs> GameStateChanged
        {
            add
            {
                _gameStateChangedHandler += value;
            }
            remove
            {
                _gameStateChangedHandler -= value;
            }
        }

        protected virtual void OnGameStateChanged(StoredGameStateChangedEventArgs args)
        {
            var handler = _gameStateChangedHandler;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        private EventHandler<GameStartedEventArgs> _gameStartedHandler;
        public event EventHandler<GameStartedEventArgs> GameStarted
        {
            add
            {
                _gameStartedHandler += value;
            }
            remove
            {
                _gameStartedHandler -= value;
            }
        }

        protected virtual void OnGameStarted(GameStartedEventArgs args)
        {
            var handler = _gameStartedHandler;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        private EventHandler<GameCompletedEventArgs> _gameCompletedHandler;
        public event EventHandler<GameCompletedEventArgs> GameCompleted
        {
            add
            {
                _gameCompletedHandler += value;
            }
            remove
            {
                _gameCompletedHandler -= value;
            }
        }

        protected virtual void OnGameCompleted(GameCompletedEventArgs args)
        {
            var handler = _gameCompletedHandler;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        internal OfflineGameDataService()
        {

        }

        public async Task<IEnumerable<GameDataStore>> GetActiveGamesAsync()
        {
            var localStorageRoot = Windows.Storage.ApplicationData.Current.LocalFolder;
            var activeGamesFolder = await localStorageRoot.CreateFolderAsync(ActiveGamesFolderName, CreationCollisionOption.OpenIfExists);
            return await GetGamesFromFolder(activeGamesFolder);
        }

        public async Task<IEnumerable<GameDataStore>> GetCompletedGamesAsync()
        {
            var localStorageRoot = ApplicationData.Current.LocalFolder;
            var completedGamesFolder = await localStorageRoot.CreateFolderAsync(CompletedGamesFolderName, CreationCollisionOption.OpenIfExists);
            return await GetGamesFromFolder(completedGamesFolder);
        }

        private async Task<IEnumerable<GameDataStore>> GetGamesFromFolder(StorageFolder folder)
        {
            var games = new List<GameDataStore>();
            var gameFiles = await folder.GetFilesAsync();

            foreach (var file in gameFiles)
            {
                games.Add(new GameDataStore(this, GetGameIDFromFileName(file.Name)));
            }

            return games;
        }

        public async Task<GameInformation> GetGameInformationAsync(GameID gameID)
        {
            bool isActive = true;
            
            var localStorageRoot = ApplicationData.Current.LocalFolder;
            var activeGamesFolder = await localStorageRoot.GetFolderAsync(ActiveGamesFolderName);
            var completedGamesFolder = await localStorageRoot.GetFolderAsync(CompletedGamesFolderName);
            var filename = GetFileNameFromGameID(gameID);
            StorageFile gameFile = null;

            try
            {
                gameFile = await activeGamesFolder.GetFileAsync(filename);
            }
            catch (FileNotFoundException)
            {
                isActive = false;
                // continue to check completed games folder
            }

            if (gameFile == null)
            {
                try
                {
                    gameFile = await completedGamesFolder.GetFileAsync(filename);
                }
                catch (FileNotFoundException)
                {
                    throw new ArgumentException("The specified GameID does not correspond to a saved game.");
                }
            }

            var fileProperties = await gameFile.GetBasicPropertiesAsync();

            using (var fileStream = await gameFile.OpenReadAsync())
            {
                using (var reader = new DataReader(fileStream))
                {
                    await reader.LoadAsync((uint)fileStream.Size);
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;
                    var moveCount = reader.ReadUInt32();

                    return new GameInformation(
                        new GameDataStore(this, gameID),
                        GetFriendlyNameFromGameID(gameID),
                        GameType.OfflineTwoPlayer,
                        fileProperties.DateModified.LocalDateTime,
                        moveCount,
                        isActive);
                }
            }
        }

        public async Task<string> GetGameStateAsync(GameID gameID)
        {
            var localStorageRoot = ApplicationData.Current.LocalFolder;
            var activeGamesFolder = await localStorageRoot.GetFolderAsync(ActiveGamesFolderName);
            var completedGamesFolder = await localStorageRoot.GetFolderAsync(CompletedGamesFolderName);
            var filename = GetFileNameFromGameID(gameID);
            StorageFile gameFile = null;

            try
            {
                gameFile = await activeGamesFolder.GetFileAsync(filename);
            }
            catch (FileNotFoundException)
            {
                // continue to check completed games folder
            }

            if (gameFile == null)
            {
                try
                {
                    gameFile = await completedGamesFolder.GetFileAsync(filename);
                }
                catch (FileNotFoundException)
                {
                    throw new ArgumentException("The specified GameID does not correspond to a saved game.");
                }
            }
            
            System.Diagnostics.Debug.Assert(gameFile != null);
                
            string stateString = null;
            using (var fileStream = await gameFile.OpenReadAsync())
            {
                using (var reader = new DataReader(fileStream))
                {
                    await reader.LoadAsync((uint)fileStream.Size);
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;
                    var numMoves = reader.ReadUInt32();
                    if (reader.UnconsumedBufferLength > 0)
                    {
                        stateString = reader.ReadString(reader.UnconsumedBufferLength);
                    }
                    return stateString;
                }
            }
        }

        public async Task<bool> StoreTurnRecordAsync(GameID gameID, string turnRecordStr)
        {
            var localStorageRoot = ApplicationData.Current.LocalFolder;
            var activeGamesFolder = await localStorageRoot.GetFolderAsync(ActiveGamesFolderName);
            StorageFile gameFile = null;

            try
            {
                gameFile = await activeGamesFolder.GetFileAsync(GetFileNameFromGameID(gameID));
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            var previousGameInfo = await GetGameInformationAsync(gameID);
            var previousState = await GetGameStateAsync(gameID);
            previousState = previousState ?? String.Empty;

            using (var transaction = await gameFile.OpenTransactedWriteAsync())
            {
                using (var writer = new DataWriter(transaction.Stream.GetOutputStreamAt(0)))
                {
                    writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    writer.ByteOrder = ByteOrder.LittleEndian;
                    writer.WriteUInt32((uint)(previousGameInfo.MoveCount + 1));
                    writer.WriteString(previousState + Environment.NewLine + turnRecordStr);
                    transaction.Stream.Size = await writer.StoreAsync();
                    await transaction.CommitAsync();
                    writer.DetachStream();
                }
            }

            return true;
        }


        public async Task<bool> StoreNewGameAsync(string friendlyName)
        {
            var localStorageRoot = ApplicationData.Current.LocalFolder;
            var activeGamesFolder = await localStorageRoot.GetFolderAsync(ActiveGamesFolderName);

            var id = CreateGameID(friendlyName);
            
            try
            {
                var gameFile = await activeGamesFolder.CreateFileAsync(GetFileNameFromGameID(id), CreationCollisionOption.FailIfExists);

                using (var transaction = await gameFile.OpenTransactedWriteAsync())
                {
                    using (var writer = new DataWriter(transaction.Stream))
                    {
                        writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        writer.ByteOrder = ByteOrder.LittleEndian;
                        writer.WriteUInt32(0);

                        transaction.Stream.Size = await writer.StoreAsync();
                        await transaction.CommitAsync();
                    }
                }
            }
            catch (Exception)
            {
                // TODO make this exception type more specific
                return false;
            }

            OnGameStarted(new GameStartedEventArgs(new GameInformation(
                new GameDataStore(this, id),
                friendlyName,
                GameType.OfflineTwoPlayer,
                DateTime.Now,
                0,
                true)));

            return true;
        }


        public async Task<bool> CompleteGameAsync(GameID gameID)
        {

            var localStorageRoot = ApplicationData.Current.LocalFolder;
            var activeGamesFolder = await localStorageRoot.GetFolderAsync(ActiveGamesFolderName);
            var completedGamesFolder = await localStorageRoot.GetFolderAsync(CompletedGamesFolderName);

            var previousGameInfo = await GetGameInformationAsync(gameID);

            try
            {
                var gameFile = await activeGamesFolder.GetFileAsync(GetFileNameFromGameID(gameID));
                await gameFile.MoveAsync(completedGamesFolder, GetFileNameFromGameID(gameID));
            }
            catch (Exception)
            {
                return false;
            }

            OnGameCompleted(new GameCompletedEventArgs(new GameInformation(
                previousGameInfo.GameDataStore,
                previousGameInfo.FriendlyName,
                previousGameInfo.GameType,
                previousGameInfo.TimeLastMoved,
                previousGameInfo.MoveCount,
                false)));

            return true;
        }

        public async Task<IStorageFile> GetGameFileAsync(GameID gameID)
        {
            var localStorageRoot = ApplicationData.Current.LocalFolder;
            var activeGamesFolder = await localStorageRoot.GetFolderAsync(ActiveGamesFolderName);
            var completedGamesFolder = await localStorageRoot.GetFolderAsync(CompletedGamesFolderName);
            var filename = GetFileNameFromGameID(gameID);
            StorageFile gameFile = null;

            try
            {
                gameFile = await activeGamesFolder.GetFileAsync(filename);
            }
            catch (FileNotFoundException)
            {
                // continue to check completed games folder
            }

            if (gameFile == null)
            {
                try
                {
                    gameFile = await completedGamesFolder.GetFileAsync(filename);
                }
                catch (FileNotFoundException)
                {
                    throw new ArgumentException("The specified GameID does not correspond to a saved game.");
                }
            }

            return gameFile;
        }

        private static readonly string _extension = ".scoutchess";

        private static GameID CreateGameID(string friendlyName)
        {
            var id = friendlyName + "_" + Guid.NewGuid().ToString();
            return new GameID(GameType.OfflineTwoPlayer, id);
        }

        private static string GetFriendlyNameFromGameID(GameID gameID)
        {
            var gameIDStr = gameID.GetOfflineTwoPlayerGameID();
            var parts = gameIDStr.Split(new char[] { '_' });
            System.Diagnostics.Debug.Assert(parts.Length == 2);
            System.Diagnostics.Debug.Assert(parts[1].EndsWith(_extension));
            return parts[0];
        }

        private static GameID GetGameIDFromFileName(string fileName)
        {
            System.Diagnostics.Debug.Assert(fileName.EndsWith(_extension));
            var id = fileName.Substring(0, fileName.Length - _extension.Length);
            return new GameID(GameType.OfflineTwoPlayer, id);
        }

        private static string GetFileNameFromGameID(GameID gameID)
        {
            var id = gameID.GetOfflineTwoPlayerGameID();
            return id + _extension;
        }
    }
}
