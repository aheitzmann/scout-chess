using ScoutChess.GameController;
using ScoutChess.GameModel;
using ScoutChess.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace ScoutChess.GameView
{
    /// <summary>
    /// The GameView class implements the IGameView contract.
    /// </summary>
    internal class GameView : IGameView
    {
        private Board _gameBoard;
        private TwoPlayerGame _gamePage;

        internal GameView(Board gameBoard, TwoPlayerGame gamePage)
        {
            _gameBoard = gameBoard;
            _gamePage = gamePage;
            
            gameBoard.SquareTapped += OnBoardSquareClicked;
        }

        #region IGameView
        
        internal void InitializeWithGameState(IGameState gameState)
        {

        }

        internal void MovePiece(GameModel.Piece pieceToMove, GameModel.BoardPosition fromLogicalPosition, GameModel.BoardPosition toLogicalPosition)
        {
            _gameBoard.MovePiece(pieceToMove, toLogicalPosition);
        }

        internal void CapturePiece(GameModel.Piece pieceToCapture, GameModel.BoardPosition removeFromLogical)
        {
            _gameBoard.CapturePiece(pieceToCapture);
        }

        public Task<bool> ConfirmMoveAsync()
        {
            return _gamePage.ConfirmMoveAsync();
        }

        internal void RestorePiece(GameModel.Piece pieceToRestore, GameModel.BoardPosition placeAtLogical)
        {
            _gameBoard.RestorePiece(pieceToRestore, placeAtLogical);
        }

        internal void HighlightPositions(IEnumerable<GameModel.BoardPosition> positionsToHighlight, PositionHighlightLevel highlightLevel)
        {
            _gameBoard.HighlightPositions(positionsToHighlight, highlightLevel);
        }

        internal void ClearHighlights()
        {
            _gameBoard.ClearHighlights();
        }

        internal void SetVisiblePositions(IEnumerable<GameModel.BoardPosition> visiblePositions)
        {
            _gameBoard.SetVisiblePositions(visiblePositions);
        }

        internal void RevealAllPositions()
        {
            _gameBoard.RevealAllPositions();
        }

        internal Task<PieceType> GetPromotionSelectionAsync()
        {
            var tcs = new TaskCompletionSource<PieceType>();

            var selector = new PromotionSelector();
            var selectorPopup = new Popup();

            selector.PromotionSelected += (sender, args) =>
            {
                selectorPopup.IsOpen = false;
                //BoardGrid.Children.Remove(selectorPopup);
                tcs.SetResult(args.NewCapabilities.Type);
            };

            selector.PromotionCancelled += (sender, args) =>
            {
                selectorPopup.IsOpen = false;
                //BoardGrid.Children.Remove(selectorPopup);
                tcs.SetCanceled();
            };

            selectorPopup.Child = selector;
            selectorPopup.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            selectorPopup.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            //selectorPopup.HorizontalOffset = -300;
            //selectorPopup.VerticalOffset = -125;
            var transitions = new TransitionCollection();
            transitions.Add(new PopupThemeTransition());
            selectorPopup.Transitions = transitions;
            //GameTableGrid.Children.Add(selectorPopup);
            selectorPopup.IsOpen = true;

            return tcs.Task;
        }
        
        internal event EventHandler<PositionEventArgs> BoardSquareClicked
        { 
            add { _boardSquareClickedHandler += value; }
            remove { _boardSquareClickedHandler -= value; }
        }

        internal event EventHandler TurnCompleted
        {
            add { _turnCompletedHandler += value; }
            remove { _turnCompletedHandler -= value; }
        }

        #endregion IGameView

        #region Event Triggers

        private EventHandler<PositionEventArgs> _boardSquareClickedHandler;
        internal void OnBoardSquareClicked(object sender, PositionEventArgs args)
        {
            var handler = _boardSquareClickedHandler;
            if (handler != null)
            {
                handler.Invoke(this, args);
            }
        }

        private EventHandler _turnCompletedHandler;
        internal void OnTurnCompleted()
        {
            var handler = _turnCompletedHandler;
            if (handler != null)
            {
                handler.Invoke(this, new EventArgs());
            }
        }

        #endregion
   
       
    }
}
