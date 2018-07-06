using ScoutChess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.GameModel
{
    internal interface IGameState
    {
        /// <summary>
        /// Gets the piece occupying a given position on the board.
        /// </summary>
        /// <param name="position"></param>
        /// <returns>The piece at the given BoardPosition, or null, if no piece occupies that position.</returns>
        Piece GetPieceAt(BoardPosition position);
        
        /// <summary>
        /// The set of pieces that are still in play in the game.
        /// </summary>
        IDictionary<Piece, BoardPosition> ActivePiecePositions { get; }

        /// <summary>
        /// The set of pieces that have been captured and are no longer in-play in the game.
        /// </summary>
        IEnumerable<Piece> CapturedPieces { get; }

        /// <summary>
        /// The Side whose turn it is.
        /// </summary>
        GameInformation GameInformation { get; }

        /// <summary>
        /// True if the IGameState is a static snapshot of a point in a game, false if it is active.
        /// </summary>
        bool IsSnapshot { get; }

        /// <summary>
        /// Notifies the subscriber that the game state has changed. 
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown if the IGameState instance does not support state change notifications.
        /// </exception>
        event EventHandler<StateChangedEventArgs> StateChanged;
    }
}
