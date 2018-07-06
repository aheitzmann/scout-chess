using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScoutChess.GameController;
using ScoutChess.Model;

namespace ScoutChess.GameModel
{
    internal class GameState : IGameState
    {
        private GameInformation _gameInfo;
        private Piece[,] boardState;
        private Dictionary<Piece, BoardPosition> activePiecePositions;
        private List<Piece> capturedPieces;
        private List<TurnRecord> history;
        private uint numMoves;
        private event EventHandler<StateChangedEventArgs> stateChanged;
        private SimulatedGameState lastSimulatedState;

#region IGameState Implementation

        public IDictionary<Piece, BoardPosition> ActivePiecePositions { get { return activePiecePositions; } }
        public IEnumerable<Piece> CapturedPieces { get { return capturedPieces; } }
        public GameInformation GameInformation { get { return _gameInfo; } }
        public bool IsSnapshot { get { return false; } }
        public event EventHandler<StateChangedEventArgs> StateChanged
        {
            add
            {
                stateChanged += value;
            }
            remove
            {
                stateChanged -= value;
            }
        }

        public Piece GetPieceAt(BoardPosition position)
        {
            return boardState[position.Column, position.Row];
        }

#endregion

        internal GameState(GameInformation gameInfo)
        {
            boardState = new Piece[8, 8];
            activePiecePositions = new Dictionary<Piece, BoardPosition>();
            capturedPieces = new List<Piece>();
            history = new List<TurnRecord>();
            lastSimulatedState = null;
            _gameInfo = gameInfo;

        }

        internal void AddPiece(Piece piece, BoardPosition position)
        {
            System.Diagnostics.Contracts.Contract.Assert(!activePiecePositions.ContainsKey(piece), "The specified piece is already active on the board.");
            System.Diagnostics.Contracts.Contract.Assert(!capturedPieces.Contains(piece), "The specified piece was captured.");
            System.Diagnostics.Contracts.Contract.Assert(boardState[position.Column, position.Row] == null, "The specified position is occupied by another piece.");

            boardState[position.Column, position.Row] = piece;
            activePiecePositions.Add(piece, position);
        }
        
        /// <summary>
        /// Use to see the game state following a potential move. 
        /// </summary>
        /// <param name="potentialTurn"></param>
        /// <param name="doRecycle">True to specify that the caller expects the returned
        /// IGameState to be reused upon the next call to this function. False to specify
        /// that the caller needs the returned IGameState to be unaltered by future calls
        /// to this function.</param>
        /// <returns>An IGameState representing the state that the game would
        /// be in if the given turn were applied to the current game state.</returns>
        internal IGameState SimulateMove(TurnRecord potentialTurn, bool doRecycle = true)
        {
            SimulatedGameState simulatedState = lastSimulatedState;
            if (simulatedState == null || !doRecycle)
            {
                simulatedState = new SimulatedGameState(this, potentialTurn);
            }
            else if (doRecycle)
            {
                simulatedState.Initialize(potentialTurn);
            }

            return simulatedState;
        }


        /// <summary>
        /// Modify the model state according to the specified turn.
        /// </summary>
        /// <param name="turn"></param>
        internal void ExecuteMove(TurnRecord turn, bool isUserInitiated)
        {
            Piece piece = null;
            numMoves++; // TODO: consider adding num moves to GameInfo and storing in place of Side in the saved game files.

            // If the turn record was created from a serialized game record, the piece info
            // may not be fully specified. In this case, we should fill in the missing info
            // before saving the turn record on the game history stack.
            if (turn.Move.PieceMoved == null)
            {
                turn.Move.UpdatePieceMoved(this);
            }

            if (turn.SecondaryMove != null && turn.SecondaryMove.PieceMoved == null)
            {
                turn.SecondaryMove.UpdatePieceMoved(this);
            }

            if (turn.Capture != null)
            {
                if (turn.Capture.PieceCaptured == null)
                {
                    turn.Capture.UpdatePieceCaptured(turn.Move, this);
                }
                piece = turn.Capture.PieceCaptured;
                activePiecePositions.Remove(piece);
                capturedPieces.Add(piece);
            }

            if (turn.Promotion != null)
            {
                if (turn.Promotion.PiecePromoted == null)
                {
                    turn.Promotion.UpdatePiecePromoted(turn.Move, this);
                }
                piece = turn.Promotion.PiecePromoted;
                piece.Capabilities = turn.Promotion.NewCapabilities;
            }

            piece = turn.Move.PieceMoved;
            boardState[turn.Move.From.Column, turn.Move.From.Row] = null;
            boardState[turn.Move.To.Column, turn.Move.To.Row] = piece;
            activePiecePositions[piece] = turn.Move.To;
            piece.TurnLastMoved = numMoves;
            
            if (turn.SecondaryMove != null)
            {
                piece = turn.SecondaryMove.PieceMoved ?? GetPieceAt(turn.SecondaryMove.From);
                boardState[turn.SecondaryMove.From.Column, turn.SecondaryMove.From.Row] = null;
                boardState[turn.SecondaryMove.To.Column, turn.SecondaryMove.To.Row] = piece;
                activePiecePositions[piece] = turn.SecondaryMove.To;
                piece.TurnLastMoved = numMoves;
            }

            history.Add(turn);
            if (isUserInitiated)
            {
                // rename and refactor
                UpdateGameInfo();
            }
            OnStateChanged(new StateChangedEventArgs(turn, this));
        }

        /// <summary>
        /// Fires the StateChanged event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnStateChanged(StateChangedEventArgs e)
        {
            var handler = stateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void UpdateGameInfo()
        {
            // TODO: most of this info is not needed by the game model. store only the needed info instead of the whole GameInformation.
            _gameInfo = new GameInformation(
                _gameInfo.GameDataStore,
                _gameInfo.FriendlyName,
                _gameInfo.GameType,
                DateTime.Now,
                _gameInfo.MoveCount + 1,
                true
                );
        }
    }
}
