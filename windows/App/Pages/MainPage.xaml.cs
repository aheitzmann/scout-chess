using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ScoutChess.GameView;
using ScoutChess.GameController;
using ScoutChess.GameModel;
using ScoutChess.ViewModel;
using ScoutChess.Model;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace ScoutChess.Pages
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    internal sealed partial class MainPage : ScoutChess.Common.LayoutAwarePage
    {
        //private GameViewModel _viewModel;
        private static IGameDataService _dataService;

        internal string MostRecentGameID
        {
            get;
            private set;
        }

        internal static DependencyProperty GameViewModelProperty = 
            DependencyProperty.Register("GameViewModel", typeof(GameViewModel), typeof(MainPage), null);
        internal GameViewModel GameViewModel
        {
            get
            {
                return (GameViewModel)GetValue(GameViewModelProperty);
            }
            set
            {
                SetValue(GameViewModelProperty, value);
            }
        }

        internal MainPage()
        {
            this.InitializeComponent();

            _dataService = _dataService ?? new OfflineGameDataService();
            GameViewModel = new GameViewModel(_dataService);
            GameViewModel.StartNewGameHandler = new GameViewModel.GameNavigationHandler((gameInfo) =>
            {
                GameDataStore? nullableGameDataStore = gameInfo.GameDataStore;
                Frame.Navigate(typeof(TwoPlayerGame), nullableGameDataStore);
            });

        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // The main page doesn't have any session state

        }

        private void NewTwoPlayerGameBtn_Click_1(object sender, RoutedEventArgs e)
        {
            NewGameOptionsPopup.IsOpen = true;
        }

        private void itemGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            var game = e.ClickedItem as Game;
            GameDataStore? nullableGameDataStore = game.GameInformation.GameDataStore;
            Frame.Navigate(typeof(TwoPlayerGame), nullableGameDataStore);
        }

        //private void TwoPlayerGameOptions_OptionsSelected_1(object sender, TwoPlayerGameOptionsDialog.TwoPlayerGameOptionSelectedEventArgs e)
        //{
        //    NewGameOptionsPopup.IsOpen = false;
        //    MostRecentGameID = e.GameName;
        //    Frame.Navigate(typeof(TwoPlayerGame), this);
        //}

        //private void TwoPlayerGameOptions_OptionsSelectionCancelled_1(object sender, EventArgs e)
        //{
        //    NewGameOptionsPopup.IsOpen = false;
        //}

     
    }
}
