using ScoutChess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.GameModel
{
    /// <summary>
    /// A snapshot of the what the game state would be after a potential turn.
    /// </summary>
    internal class SimulatedGameState : IGameState
    {
        private IGameState basedOn;
        private TurnRecord pendingTurn;
        private Dictionary<Piece, BoardPosition> activePieces;
        private GameInformation _gameInfo;

        internal SimulatedGameState(IGameState basedOn, TurnRecord pendingTurn)
        {
            this.basedOn = basedOn;
            Initialize(pendingTurn);
        }

        internal void Initialize(TurnRecord pendingTurn)
        {
            this.pendingTurn = pendingTurn;
            this.activePieces = new Dictionary<Piece, BoardPosition>(basedOn.ActivePiecePositions);
            if (pendingTurn.Capture != null)
            {
                this.activePieces.Remove(pendingTurn.Capture.PieceCaptured);
            }

            this.activePieces[pendingTurn.Move.PieceMoved] = pendingTurn.Move.To;

            if (pendingTurn.SecondaryMove != null)
            {
                this.activePieces[pendingTurn.SecondaryMove.PieceMoved] = pendingTurn.SecondaryMove.To;
            }

            _gameInfo = new GameInformation(
                basedOn.GameInformation.GameDataStore,
                basedOn.GameInformation.FriendlyName,
                basedOn.GameInformation.GameType,
                DateTime.Now,
                basedOn.GameInformation.MoveCount + 1,
                basedOn.GameInformation.IsActive);
        }

        public Piece GetPieceAt(BoardPosition position)
        {
            if (position.Equals(pendingTurn.Move.To))
            {
                return pendingTurn.Move.PieceMoved;
            }
            if (pendingTurn.SecondaryMove != null && position.Equals(pendingTurn.SecondaryMove.To))
            {
                return pendingTurn.SecondaryMove.PieceMoved;
            }

            if (position.Equals(pendingTurn.Move.From) || 
                (pendingTurn.SecondaryMove != null && position.Equals(pendingTurn.SecondaryMove.From)))
            {
                return null;
            }

            return basedOn.GetPieceAt(position);
        }

        public IDictionary<Piece, BoardPosition> ActivePiecePositions
        {
            get
            {
                return activePieces;
            }
        }

        public IEnumerable<Piece> CapturedPieces
        {
            get
            {
                if (pendingTurn.Capture == null)
                {
                    return basedOn.CapturedPieces;
                }

                return basedOn.CapturedPieces.Union(new Piece[] { pendingTurn.Capture.PieceCaptured });
            }
        }

        public GameInformation GameInformation
        {
            get
            {
                return _gameInfo;
            }
        }

        public bool IsSnapshot
        {
            get
            {
                return true;
            }
        }

        public event EventHandler<StateChangedEventArgs> StateChanged
        {
            add
            {
                throw new NotSupportedException("The StateChanged event is not supported on this IGameState snapshot.");
            }
            remove
            {
                throw new NotSupportedException("The StateChanged event is not supported on this IGameState snapshot.");
            }
        }
    }
}
