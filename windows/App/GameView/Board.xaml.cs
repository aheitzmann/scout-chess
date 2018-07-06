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
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using ScoutChess.GameModel;
using ScoutChess.GameController;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Popups;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ScoutChess.GameView
{
    internal sealed partial class Board : UserControl
    {
        // Board size constants
        internal static readonly uint NumColumns = 8;
        internal static readonly uint NumRows = 8;

        private int capturedPawnCount, capturedOtherPawnCount;

        private IGameState gameState = null;
        private Rectangle[,] boardSquares = new Rectangle[Board.NumColumns, Board.NumRows];


        internal static readonly BitmapImage whitePawnImageSource = new BitmapImage(new Uri("ms-appx:///Assets/white_pawn.png"));
        internal static readonly BitmapImage blackPawnImageSource = new BitmapImage(new Uri("ms-appx:///Assets/black_pawn.png"));
        internal static readonly BitmapImage whiteRookImageSource = new BitmapImage(new Uri("ms-appx:///Assets/white_rook.png"));
        internal static readonly BitmapImage blackRookImageSource = new BitmapImage(new Uri("ms-appx:///Assets/black_rook.png"));
        internal static readonly BitmapImage whiteKnightImageSource = new BitmapImage(new Uri("ms-appx:///Assets/white_knight.png"));
        internal static readonly BitmapImage blackKnightImageSource = new BitmapImage(new Uri("ms-appx:///Assets/black_knight.png"));
        internal static readonly BitmapImage whiteBishopImageSource = new BitmapImage(new Uri("ms-appx:///Assets/white_bishop.png"));
        internal static readonly BitmapImage blackBishopImageSource = new BitmapImage(new Uri("ms-appx:///Assets/black_bishop.png"));
        internal static readonly BitmapImage whiteKingImageSource = new BitmapImage(new Uri("ms-appx:///Assets/white_king.png"));
        internal static readonly BitmapImage blackKingImageSource = new BitmapImage(new Uri("ms-appx:///Assets/black_king.png"));
        internal static readonly BitmapImage whiteQueenImageSource = new BitmapImage(new Uri("ms-appx:///Assets/white_queen.png"));
        internal static readonly BitmapImage blackQueenImageSource = new BitmapImage(new Uri("ms-appx:///Assets/black_queen.png"));

        private event EventHandler<PositionEventArgs> squareTapped;

        internal event EventHandler<PositionEventArgs> SquareTapped
        {
            add
            {
                squareTapped += value;
            }
            remove
            {
                squareTapped -= value;
            }
        }

        private void OnSquareTapped(PositionEventArgs e)
        {
            var handler = squareTapped;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        //private event EventHandler switchPlayers;
        //internal event EventHandler SwitchPlayers
        //{
        //    add
        //    {
        //        switchPlayers += value;
        //    }
        //    remove
        //    {
        //        switchPlayers -= value;
        //    }
        //}

        //protected void OnSwitchPlayers(EventArgs e)
        //{
        //    var handler = switchPlayers;
        //    if (handler != null)
        //    {
        //        handler(this, e);
        //    }
        //}

        //internal bool IsSwitchPlayerEnabled
        //{
        //    get
        //    {
        //        return SwitchPlayersBtn.IsEnabled;
        //    }
        //    set
        //    {
        //        SwitchPlayersBtn.IsEnabled = value;
        //    }
        //}

        //// TODO: remove
        //internal bool IsGameBoardEnabled
        //{
        //    get;
        //    set;
        //}

        #region Properties



        public Side OrientedFor
        {
            get { return (Side)GetValue(OrientedForProperty); }
            set { SetValue(OrientedForProperty, value); }
        }

        public static readonly DependencyProperty OrientedForProperty =
            DependencyProperty.Register("OrientedFor", typeof(Side), typeof(Board), new PropertyMetadata(Side.White, OnOrientedForChanged));

        private static void OnOrientedForChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

        }

        internal Color PrimaryColor { get; set; }

        #endregion Properties

        #region Constructors

        internal Board()
        {
            this.InitializeComponent();
            PrimaryColor = Colors.Firebrick;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Display the current state of the model and listen for model state changed
        /// events so that we can update the display as needed.
        /// </summary>
        /// <param name="gameState"></param>
        internal void AssociateModel(IGameState gameState)
        {
            this.gameState = gameState;

            foreach (var piecePositionPair in gameState.ActivePiecePositions)
            {
                var piece = piecePositionPair.Key;
                var logicalPosition = piecePositionPair.Value;
                AddPiece(piece, logicalPosition);
            }

            foreach (var capturedPiece in gameState.CapturedPieces)
            {
                BoardCapturedPiecesPanel.AddPiece(capturedPiece);
            }

            gameState.StateChanged += OnGameStateChanged;
        }

        internal void MovePiece(Piece piece, BoardPosition logicalPosition)
        {
            var viewPosition = LogicalToViewPosition(logicalPosition);
            piece.Visual.SetValue(Grid.RowProperty, (int)viewPosition.Row);
            piece.Visual.SetValue(Grid.ColumnProperty, (int)viewPosition.Column);
        }

        internal void CapturePiece(Piece pieceToCapture)
        {
            BoardGrid.Children.Remove(pieceToCapture.Visual);
            BoardCapturedPiecesPanel.AddPiece(pieceToCapture);
        }

        internal void RestorePiece(Piece pieceToRestore, BoardPosition placeAt)
        {
            var viewPosition = LogicalToViewPosition(placeAt);
            BoardCapturedPiecesPanel.RemovePiece(pieceToRestore);
            pieceToRestore.Visual.SetValue(Grid.ColumnProperty, viewPosition.Column);
            pieceToRestore.Visual.SetValue(Grid.RowProperty, viewPosition.Row);
            BoardGrid.Children.Add(pieceToRestore.Visual);
        }

        /// <summary>
        /// Apply highlights to the specified board positions to empasize them.
        /// </summary>
        /// <param name="positionsToHighlight">A list of positions to highlight.</param>
        /// <param name="highlightLevel">The type of highlight to apply.</param>
        internal void HighlightPositions(IEnumerable<BoardPosition> positionsToHighlight, PositionHighlightLevel highlightLevel)
        {
            var borderThickness = 0;
            var borderColor = Colors.Black;
            switch (highlightLevel)
            {
                case PositionHighlightLevel.Primary:
                    borderColor = Colors.DarkSlateBlue;
                    borderThickness = 5;
                    break;
                case PositionHighlightLevel.Secondary:
                    borderColor = Colors.Red;
                    borderThickness = 3;
                    break;
                case PositionHighlightLevel.Tertiary:
                    borderColor = Colors.Black;
                    borderThickness = 3;
                    break;
            }

            foreach (var logicalPosition in positionsToHighlight)
            {
                var viewPosition = LogicalToViewPosition(logicalPosition);
                var rect = boardSquares[(int)viewPosition.Column, (int)viewPosition.Row];
                rect.Stroke = new SolidColorBrush(borderColor);
                rect.StrokeThickness = borderThickness;
            }
        }

        /// <summary>
        /// Remove all highlights from all board positions.
        /// </summary>
        internal void ClearHighlights()
        {
            foreach (var rect in boardSquares)
            {
                rect.Stroke = null;
            }
        }

        /// <summary>
        /// Makes all pieces in the specified positions visible and hides all others.
        /// </summary>
        /// <param name="visiblePositions"></param>
        internal void SetVisiblePositions(IEnumerable<BoardPosition> visiblePositions)
        {
            foreach (var piecePositionPair in gameState.ActivePiecePositions)
            {
                var piece = piecePositionPair.Key;
                var position = piecePositionPair.Value;
                if (visiblePositions.Contains(position))
                {
                    piece.Visual.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    piece.Visual.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Makes all pieces visible.
        /// </summary>
        internal void RevealAllPositions()
        {
            foreach (var piecePositionPair in gameState.ActivePiecePositions)
            {
                var piece = piecePositionPair.Key;
                piece.Visual.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }


        #endregion Methods

        #region Private Methods

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > e.NewSize.Height)
            {
                BoardCapturedPiecesPanel.SetValue(Grid.RowProperty, 0);
                BoardCapturedPiecesPanel.SetValue(Grid.ColumnProperty, 1);
                BoardCapturedPiecesPanel.Orientation = Orientation.Vertical;
            }
            else
            {
                BoardCapturedPiecesPanel.SetValue(Grid.RowProperty, 1);
                BoardCapturedPiecesPanel.SetValue(Grid.ColumnProperty, 0);
                BoardCapturedPiecesPanel.Orientation = Orientation.Horizontal;
            }
        }

        private void OnBoardGridLoaded(object sender, RoutedEventArgs e)
        {
            var isWhite = true;
            var whiteBrush = new SolidColorBrush(Colors.AntiqueWhite);
            var blackBrush = new SolidColorBrush(PrimaryColor);
            for (int i = 0; i < NumColumns; i++)
            {
                for (int j = 0; j < NumRows; j++)
                {
                    var rect = new Rectangle();
                    boardSquares[i, j] = rect;
                    rect.Fill = isWhite ? whiteBrush : blackBrush;
                    rect.SetValue(Grid.ColumnProperty, i);
                    rect.SetValue(Grid.RowProperty, j);
                    rect.Tapped += OnBoardTapped;
                    BoardGrid.Children.Add(rect);
                    isWhite = !isWhite;
                }

                isWhite = !isWhite;
            }
        }

        private void OnBoardTapped(object sender, TappedRoutedEventArgs e)
        {
            int col, row;
            var square = (UIElement)sender;
            col = (int)square.GetValue(Grid.ColumnProperty);
            row = (int)square.GetValue(Grid.RowProperty);
            var logicalPosition = ViewToLogicalPosition(new GridCellPosition((uint)col, (uint)row));

            OnSquareTapped(new PositionEventArgs(logicalPosition));
        }

        private void OnGameStateChanged(object sender, StateChangedEventArgs e)
        {
            if (e.TurnRecord.Promotion != null)
            {
                BoardGrid.Children.Remove(e.TurnRecord.Promotion.PiecePromoted.Visual);
                e.TurnRecord.Promotion.PiecePromoted.Visual = CapabilitiesToVisual(
                    e.TurnRecord.Promotion.NewCapabilities,
                    e.TurnRecord.Promotion.PiecePromoted.Side);
                AddPiece(e.TurnRecord.Promotion.PiecePromoted, e.TurnRecord.Move.To);
            }
            else
            {
                MovePiece(e.TurnRecord.Move.PieceMoved, e.TurnRecord.Move.To);
            }

            if (e.TurnRecord.SecondaryMove != null)
            {
                MovePiece(e.TurnRecord.SecondaryMove.PieceMoved, e.TurnRecord.SecondaryMove.To);
            }

            if (e.TurnRecord.Capture != null)
            {
                var pieceCaptured = e.TurnRecord.Capture.PieceCaptured;
                BoardGrid.Children.Remove(pieceCaptured.Visual);
                BoardCapturedPiecesPanel.AddPiece(pieceCaptured);
            }
        }

        private void AddPiece(Piece piece, BoardPosition logicalPosition)
        {
            piece.Visual.Tapped += OnBoardTapped;
            BoardGrid.Children.Add(piece.Visual);
            MovePiece(piece, logicalPosition);
        }

        

        private GridCellPosition LogicalToViewPosition(BoardPosition logicalPosition)
        {
            uint col, row;
            if (OrientedFor == Side.White)
            {
                col = logicalPosition.Column;
                row = (NumRows - 1) - logicalPosition.Row;
            }
            else
            {
                col = (NumColumns - 1) - logicalPosition.Column;
                row = logicalPosition.Row;
            }

            return new GridCellPosition(col, row);
        }

        private BoardPosition ViewToLogicalPosition(GridCellPosition viewPosition)
        {
            // Transformation is symetrical
            var logicalPosition = LogicalToViewPosition(new BoardPosition(viewPosition.Column, viewPosition.Row));
            return new BoardPosition(logicalPosition.Column, logicalPosition.Row);
        }

        #endregion Private Methods
        

        
        // TODO: This should not be part of the board
        internal Task<IPieceCapabilities> GetPromotionSelectionAsync()
        {
            var tcs = new TaskCompletionSource<IPieceCapabilities>();

            var selector = new PromotionSelector();
            var selectorPopup = new Popup();

            selector.PromotionSelected += (sender, args) =>
                {
                    selectorPopup.IsOpen = false;
                    BoardGrid.Children.Remove(selectorPopup);
                    tcs.SetResult(args.NewCapabilities);
                };

            selector.PromotionCancelled += (sender, args) =>
                {
                    selectorPopup.IsOpen = false;
                    BoardGrid.Children.Remove(selectorPopup);
                    tcs.SetCanceled();
                };

            selectorPopup.Child = selector;
            selectorPopup.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            selectorPopup.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            selectorPopup.HorizontalOffset = -300;
            selectorPopup.VerticalOffset = -125;
            var transitions = new TransitionCollection();
            transitions.Add(new PopupThemeTransition());
            selectorPopup.Transitions = transitions;
            GameTableGrid.Children.Add(selectorPopup);
            selectorPopup.IsOpen = true;

            return tcs.Task;
        }

        // TODO nor should this.
        private Image CapabilitiesToVisual(IPieceCapabilities capabilities, Side side)
        {
            Image visual = new Image();
            if (side == Side.White)
            {
                if (capabilities is PawnCapabilities)
                {
                    visual.Source = Board.whitePawnImageSource;
                }
                else if (capabilities is KnightCapabilities)
                {
                    visual.Source = Board.whiteKnightImageSource;
                }
                else if (capabilities is BishopCapabilities)
                {
                    visual.Source = Board.whiteBishopImageSource;
                }
                else if (capabilities is RookCapabilities)
                {
                    visual.Source = Board.whiteRookImageSource;
                }
                else if (capabilities is QueenCapabilities)
                {
                    visual.Source = Board.whiteQueenImageSource;
                }
                else if (capabilities is KingCapabilities)
                {
                    visual.Source = Board.whiteKingImageSource;
                }
            }
            else
            {
                if (capabilities is PawnCapabilities)
                {
                    visual.Source = Board.blackPawnImageSource;
                }
                else if (capabilities is KnightCapabilities)
                {
                    visual.Source = Board.blackKnightImageSource;
                }
                else if (capabilities is BishopCapabilities)
                {
                    visual.Source = Board.blackBishopImageSource;
                }
                else if (capabilities is RookCapabilities)
                {
                    visual.Source = Board.blackRookImageSource;
                }
                else if (capabilities is QueenCapabilities)
                {
                    visual.Source = Board.blackQueenImageSource;
                }
                else if (capabilities is KingCapabilities)
                {
                    visual.Source = Board.blackKingImageSource;
                }
            }

            return visual;
        }

        

        // TODO: Remove
        //private async void SwitchPlayersBtn_Tapped_1(object sender, TappedRoutedEventArgs e)
        //{
        //    var switchPlayersDialog = new MessageDialog("Player has completed a turn. Opposing player's view of the board will be revealed upon selecting 'Begin Turn'.", "Switch Turns");
        //    switchPlayersDialog.Commands.Add(new UICommand("Cancel"));
        //    switchPlayersDialog.Commands.Add(new UICommand("Begin Turn", new UICommandInvokedHandler((command) => OnSwitchPlayers(new EventArgs()))));
        //    switchPlayersDialog.DefaultCommandIndex = 1;
        //    switchPlayersDialog.CancelCommandIndex = 0;

        //    Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        //    await switchPlayersDialog.ShowAsync();
        //    Visibility = Windows.UI.Xaml.Visibility.Visible;
        //}

    }



    

}
