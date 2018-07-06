using ScoutChess.GameController;
using ScoutChess.GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.GameView
{

    internal enum PositionHighlightLevel
    {
        Primary,
        Secondary,
        Tertiary,
    }

    /// <summary>
    /// Contract for the view for a single game. The view does not know anything about
    /// the rules of chess. It simply provides the abilty to update various aspects of
    /// the game UI. 
    /// The controller is responsible for updating the view in response to user input
    /// and model updates.
    /// </summary>
    internal interface IGameView
    {

        /// <summary>
        /// Initialize the game view to reflect the specified game state.
        /// </summary>
        /// <param name="gameState">The game state that the game view will reflect.</param>
        void InitializeWithGameState(IGameState gameState);

        /// <summary>
        /// Move the specified piece to a new position on the board.
        /// </summary>
        /// <param name="pieceToMove">The piece to move.</param>
        /// <param name="fromLogicalPosition">The logical position that the piece currently occupies.</param>
        /// <param name="toLogicalPosition">The logical position to move the piece to.</param>
        /// 
        void MovePiece(Piece pieceToMove, BoardPosition fromLogicalPosition, BoardPosition toLogicalPosition);

        /// <summary>
        /// Removes a piece from the game board and adds it to the captured pieces panel.
        /// </summary>
        /// <param name="pieceToCapture">The piece to capture.</param>
        /// <param name="removeFromLogical">The logical position that the captured piece currently occupies.</param>
        void CapturePiece(Piece pieceToCapture, BoardPosition removeFromLogical);

        /// <summary>
        /// Prompt the user for confirmation of a move.
        /// </summary>
        /// <returns>True if the user confirms the move, false if the user cancels the move.</returns>
        Task<bool> ConfirmMoveAsync();

        /// <summary>
        /// Returns a piece to the board and removes it from the captured pieces panel.
        /// </summary>
        /// <param name="pieceToRestore">The piece to restore to the board.</param>
        /// <param name="placeAtLogical">The logical position in which to place the restored piece.</param>
        void RestorePiece(Piece pieceToRestore, BoardPosition placeAtLogical);

        /// <summary>
        /// Applies a visual highlight effect to the specified positions as a means of 
        /// differentiating them.
        /// </summary>
        /// <param name="positionsToHighlight">The collection of positions to highlight.</param>
        /// <param name="highlightLevel">The highlight level, which determines the
        /// intensity of the highlight used by the view.</param>
        void HighlightPositions(IEnumerable<BoardPosition> positionsToHighlight, PositionHighlightLevel highlightLevel);

        /// <summary>
        /// Removes the highlight effects from all squares to which they are applied.
        /// </summary>
        void ClearHighlights();

        /// <summary>
        /// Specifies which board positions should be visible.
        /// </summary>
        /// <param name="visiblePositions">The list of logical board positions
        /// whose contents will be revealed. All other positions will be hidden.</param>
        void SetVisiblePositions(IEnumerable<BoardPosition> visiblePositions);

        /// <summary>
        /// Makes all of the pieces on the board visible.
        /// </summary>
        void RevealAllPositions();

        /// <summary>
        /// Prompts the user to choose the type of piece to promote a pawn to.
        /// </summary>
        /// <returns>A PieceType, the user's selection.</returns>
        Task<PieceType> GetPromotionSelectionAsync();

        /// <summary>
        /// Fired to indicate that the user has clicked or tapped a square on the
        /// game board.
        /// </summary>
        event EventHandler<PositionEventArgs> BoardSquareClicked;

        /// <summary>
        /// Fired to indicate that the user has completed their turn.
        /// </summary>
        event EventHandler TurnCompleted;
    }
}
