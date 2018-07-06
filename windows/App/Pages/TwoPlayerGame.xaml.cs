using ScoutChess.GameView;
using ScoutChess.GameController;
using ScoutChess.GameModel;
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
using ScoutChess.Model;
using ScoutChess.IO;
using Windows.UI.Popups;
using System.Threading.Tasks;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace ScoutChess.Pages
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    internal sealed partial class TwoPlayerGame : ScoutChess.Common.LayoutAwarePage
    {
        private GameDataStore _gameDataStore;
        private LocalPlayerController _whiteController;
        private LocalPlayerController _blackController;
        private GameState _gameModel;
        private GameView.GameView _gameView;

        internal TwoPlayerGame()
        {
            this.InitializeComponent();
        }

        internal GameDataStore GameDataStore
        {
            get { return _gameDataStore; }
        }

        internal static DependencyProperty GameNameProperty = DependencyProperty.Register(
            "GameName", typeof(string), typeof(TwoPlayerGame), null);

        internal string GameName
        {
            get
            {
                return (string)GetValue(GameNameProperty);
            }
            set
            {
                SetValue(GameNameProperty, value);
            }
        }

        internal Board GameBoard { get { return TwoPlayerBoard; } }    

        internal Task<bool> ConfirmMoveAsync()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Button acceptBtn, cancelBtn;
            GetGameControlsForOrientation(out acceptBtn, out cancelBtn);
            acceptBtn.IsEnabled = true;
            cancelBtn.IsEnabled = true;

            acceptBtn.Click += (sender, e) =>
            {
                acceptBtn.IsEnabled = false;
                cancelBtn.IsEnabled = false;
                tcs.SetResult(true);
            };

            cancelBtn.Click += (sender, e) =>
            {
                acceptBtn.IsEnabled = false;
                cancelBtn.IsEnabled = false;
                tcs.SetResult(false);
            };

            return tcs.Task;
        }

        private void GetGameControlsForOrientation(out Button acceptBtn, out Button cancelBtn)
        {
            if (LandscapeGameControlsPanel.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                acceptBtn = LandscapeAcceptMoveBtn;
                cancelBtn = LandscapeCancelMoveBtn;
            }
            else
            {
                System.Diagnostics.Debug.Assert(PortraitGameControlsPanel.Visibility == Windows.UI.Xaml.Visibility.Visible));
                acceptBtn = PortraitAcceptMoveBtn;
                cancelBtn = PortraitCancelMoveBtn;
            }
                
        }

        private async void Initialize(GameDataStore gameDataStore)
        {
            _gameDataStore = gameDataStore;

            await ApplicationServices.ShareSourceService.Current.SetDataForShareSource(gameDataStore);

            var gameInfo = await gameDataStore.GameDataService.GetGameInformationAsync(gameDataStore.GameID);
            Side side = Side.White;
            GameName = gameInfo.FriendlyName;
            var gameState = await gameDataStore.GameDataService.GetGameStateAsync(gameDataStore.GameID);
            List<TurnRecord> turnHistory = new List<TurnRecord>();

            if (gameState != null)
            {
                gameState = gameState.TrimStart(Environment.NewLine.ToCharArray());
                var reader = new StringReader(gameState);
                string turnStr = null;
                while ((turnStr = reader.ReadLine()) != null)
                {
                    var turn = SerializationHelper.DeserializeTurnRecord(turnStr, side);
                    turnHistory.Add(turn);
                    side = side == Side.White ? Side.Black : Side.White;
                }
            }

            _gameModel = new GameState(gameInfo);
            _gameView = new GameView.GameView(GameBoard, this);
            _whiteController = new LocalPlayerController(Side.White, _gameDataStore.GameDataService);
            _blackController = new LocalPlayerController(Side.Black, _gameDataStore.GameDataService);

            InitializeFromTurnHistory(turnHistory);

            _whiteController.AttachTo(GameBoard, _gameModel);
            _blackController.AttachTo(GameBoard, _gameModel);

            _gameView.InitializeWithGameState(_gameModel);

            if (gameInfo.IsActive)
            {
                var nextMoveController = gameInfo.NextToMove == Side.White ? _whiteController : _blackController;
                nextMoveController.SetVisiblePositionsForPlayer();
            }
            else
            {
                IsEnabled = false;
            }
        }

        private void InitializeFromTurnHistory(List<TurnRecord> turnHistory)
        {
            Piece piece;
            Image visual;
            BoardPosition position;

            // Add Pawns
            for (uint i = 0; i < 8; i++)
            {
                piece = new Piece();
                piece.Side = Side.White;
                piece.TurnLastMoved = 0;
                piece.Capabilities = PawnCapabilities.Singleton;
                visual = new Image();
                visual.Source = Board.whitePawnImageSource;
                piece.Visual = visual;
                position = new BoardPosition(i, 1);
                _gameModel.AddPiece(piece, position);

                piece = new Piece();
                piece.Side = Side.Black;
                piece.TurnLastMoved = 0;
                piece.Capabilities = PawnCapabilities.Singleton;
                visual = new Image();
                visual.Source = Board.blackPawnImageSource;
                piece.Visual = visual;
                position = new BoardPosition(i, 6);
                _gameModel.AddPiece(piece, position);
            }

            // Rooks
            piece = new Piece();
            piece.Side = Side.White;
            piece.TurnLastMoved = 0;
            piece.Capabilities = RookCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.whiteRookImageSource;
            piece.Visual = visual;
            position = new BoardPosition(0, 0);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.White;
            piece.TurnLastMoved = 0;
            piece.Capabilities = RookCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.whiteRookImageSource;
            piece.Visual = visual;
            position = new BoardPosition(7, 0);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.Black;
            piece.TurnLastMoved = 0;
            piece.Capabilities = RookCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.blackRookImageSource;
            piece.Visual = visual;
            position = new BoardPosition(0, 7);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.Black;
            piece.TurnLastMoved = 0;
            piece.Capabilities = RookCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.blackRookImageSource;
            piece.Visual = visual;
            position = new BoardPosition(7, 7);
            _gameModel.AddPiece(piece, position);

            // Knights
            piece = new Piece();
            piece.Side = Side.White;
            piece.TurnLastMoved = 0;
            piece.Capabilities = KnightCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.whiteKnightImageSource;
            piece.Visual = visual;
            position = new BoardPosition(1, 0);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.White;
            piece.TurnLastMoved = 0;
            piece.Capabilities = KnightCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.whiteKnightImageSource;
            piece.Visual = visual;
            position = new BoardPosition(6, 0);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.Black;
            piece.TurnLastMoved = 0;
            piece.Capabilities = KnightCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.blackKnightImageSource;
            piece.Visual = visual;
            position = new BoardPosition(1, 7);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.Black;
            piece.TurnLastMoved = 0;
            piece.Capabilities = KnightCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.blackKnightImageSource;
            piece.Visual = visual;
            position = new BoardPosition(6, 7);
            _gameModel.AddPiece(piece, position);

            // Bishops
            piece = new Piece();
            piece.Side = Side.White;
            piece.TurnLastMoved = 0;
            piece.Capabilities = BishopCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.whiteBishopImageSource;
            piece.Visual = visual;
            position = new BoardPosition(2, 0);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.White;
            piece.TurnLastMoved = 0;
            piece.Capabilities = BishopCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.whiteBishopImageSource;
            piece.Visual = visual;
            position = new BoardPosition(5, 0);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.Black;
            piece.TurnLastMoved = 0;
            piece.Capabilities = BishopCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.blackBishopImageSource;
            piece.Visual = visual;
            position = new BoardPosition(2, 7);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.Black;
            piece.TurnLastMoved = 0;
            piece.Capabilities = BishopCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.blackBishopImageSource;
            piece.Visual = visual;
            position = new BoardPosition(5, 7);
            _gameModel.AddPiece(piece, position);

            // Kings
            piece = new Piece();
            piece.Side = Side.White;
            piece.TurnLastMoved = 0;
            piece.Capabilities = KingCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.whiteKingImageSource;
            piece.Visual = visual;
            position = new BoardPosition(4, 0);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.Black;
            piece.TurnLastMoved = 0;
            piece.Capabilities = KingCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.blackKingImageSource;
            piece.Visual = visual;
            position = new BoardPosition(4, 7);
            _gameModel.AddPiece(piece, position);

            // Queens
            piece = new Piece();
            piece.Side = Side.White;
            piece.TurnLastMoved = 0;
            piece.Capabilities = QueenCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.whiteQueenImageSource;
            piece.Visual = visual;
            position = new BoardPosition(3, 0);
            _gameModel.AddPiece(piece, position);

            piece = new Piece();
            piece.Side = Side.Black;
            piece.TurnLastMoved = 0;
            piece.Capabilities = QueenCapabilities.Singleton;
            visual = new Image();
            visual.Source = Board.blackQueenImageSource;
            piece.Visual = visual;
            position = new BoardPosition(3, 7);
            _gameModel.AddPiece(piece, position);

            if (turnHistory != null)
            {
                foreach (var turn in turnHistory)
                {
                    _gameModel.ExecuteMove(turn, false);
                }
            }
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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var nullableGameDataStore = navigationParameter as GameDataStore?;
            var gameDataStore = nullableGameDataStore.Value; 
            Initialize(gameDataStore);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ApplicationServices.ShareSourceService.Current.SetDataForShareSource(null);
            base.OnNavigatingFrom(e);
        }

        private void OnPageRootSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > e.NewSize.Height)
            {
                LandscapeGameControlsPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                PortraitGameControlsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                LandscapeGameControlsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                PortraitGameControlsPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
    }
}
