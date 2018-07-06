using ScoutChess.Common;
using ScoutChess.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScoutChess.ViewModel
{
    public class GameViewModel : INotifyPropertyChanged
    {

        internal class NewGameOptionsBase
        {
            internal string player1Name;
            internal string player2Name;

            internal NewGameOptionsBase(string player1Name, string player2Name)
            {
                this.player1Name = player1Name;
                this.player2Name = player2Name;
            }
        }

        internal class NewOfflineGameOptions : NewGameOptionsBase
        {
            internal NewOfflineGameOptions(string player1Name, string player2Name)
                : base(player1Name, player2Name)
            {
            }
        }

        internal class NewOfflineGameCommand : ICommand
        {
            IGameDataService _dataService = null;
            internal NewOfflineGameCommand(IGameDataService dataService)
            {
                _dataService = dataService;
            }

            public bool CanExecute(object parameter)
            {
                return parameter as NewOfflineGameOptions != null;
            }

            public event EventHandler CanExecuteChanged;

            public async void Execute(object parameter)
            {
                var gameOptions = parameter as NewOfflineGameOptions;
                System.Diagnostics.Debug.Assert(gameOptions != null);

                // Add the new game to the model. The changes will be automatically
                // propagated back to the view model and exposed in the view.
                await _dataService.StoreNewGameAsync(gameOptions.player1Name + " vs. " + gameOptions.player2Name);
            }
        }

        internal delegate void GameNavigationHandler(GameInformation gameInfo);

        private IGameDataService _dataService = null;
        private ObservableCollection<GameGroup> _gameGroups = null;

        

        /// <summary>
        ///  Binding source for the main page's grid of active and completed games
        /// </summary>
        public ObservableCollection<GameGroup> GameGroups
        {
            get
            {
                return _gameGroups;
            }
        }

        public ICommand Command_NewOfflineGame
        {
            get;
            private set;
        }

        /// <summary>
        /// Invoked when navigation to a game is required.
        /// </summary>
        internal GameNavigationHandler StartNewGameHandler
        {
            get;
            set;
        }


        internal GameViewModel(IGameDataService dataService)
        {
            this._dataService = dataService;
            dataService.GameStarted += dataService_GameStarted;
            Command_NewOfflineGame = new DelegatingCommand((p) => ExecuteCommand_NewOfflineGame(p as NewOfflineGameOptions));

            _gameGroups = new ObservableCollection<GameGroup>();
            GetGameGroups();
        }

        private async void ExecuteCommand_NewOfflineGame(NewOfflineGameOptions gameOptions)
        {
            System.Diagnostics.Debug.Assert(gameOptions != null);

            // Add the new game to the model. The changes will be automatically
            // propagated back to the view model and exposed in the view.
            await _dataService.StoreNewGameAsync(gameOptions.player1Name + " vs. " + gameOptions.player2Name);
        }

        internal void dataService_GameStarted(object sender, GameStartedEventArgs e)
        {
            var handler = StartNewGameHandler;
            if (handler != null)
            {
                handler(e.NewGameInfo);
            }
        }

        /// <summary>
        /// Initialize the groups of active and completed games. Fetches the games from disk
        /// and adds them to the apporpiate GameGroup.
        /// </summary>
        private async void GetGameGroups()
        {
            var activeGames = await _dataService.GetActiveGamesAsync();
            var completedGames = await _dataService.GetCompletedGamesAsync();

            var activeGameGroup = new GameGroup(null, "Active Games", null, null, null);
            foreach (var game in activeGames)
            {
                var gameInfo = await _dataService.GetGameInformationAsync(game.GameID);
                activeGameGroup.Items.Add(new Game(gameInfo));
            }

            var completedGameGroup = new GameGroup(null, "Completed Games", null, null, null);
            foreach (var game in completedGames)
            {
                var gameInfo = await _dataService.GetGameInformationAsync(game.GameID);
                completedGameGroup.Items.Add(new Game(gameInfo));
            }

            _gameGroups.Add(activeGameGroup);
            _gameGroups.Add(completedGameGroup);

            OnPropertyChanged(new PropertyChangedEventArgs("GameGroups"));
        }

        internal void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
