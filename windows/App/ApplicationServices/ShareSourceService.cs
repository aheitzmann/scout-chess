using ScoutChess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;

namespace ScoutChess.ApplicationServices
{
    internal class ShareSourceService
    {
        private static IStorageItem _currentGameFile = null;

        private static ShareSourceService _current;
        internal static ShareSourceService Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new ShareSourceService();
                }
                return _current;
            }
        }

        internal void Initialize()
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += ShareSource_DataRequested;
        }

        internal async Task SetDataForShareSource(GameDataStore? gameDataStore)
        {
            if (gameDataStore.HasValue)
            {
                var dataStore = gameDataStore.Value;
                _currentGameFile = await dataStore.GameDataService.GetGameFileAsync(dataStore.GameID);
            }
            else
            {
                _currentGameFile = null;
            }
        }

        private void ShareSource_DataRequested(DataTransferManager dtm, DataRequestedEventArgs e)
        {
            if (_currentGameFile != null)
            {
                e.Request.Data.Properties.Title = "Share Game";
                e.Request.Data.Properties.Description = "Shares the current game file so you can continue playing on a friend's computer.";
                e.Request.Data.SetStorageItems(new IStorageItem[] { _currentGameFile });
            }
            else
            {
                e.Request.FailWithDisplayText("To share you'll need to start a new game or resume an existing one.");
            }
        }
    }
}
