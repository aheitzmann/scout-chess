using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using ScoutChess.GameModel;
using ScoutChess.GameView;
using ScoutChess.Model;
using ScoutChess.IO;

namespace ScoutChess.GameController
{
    class LocalPlayerController
    {
        private Board _board;
        private GameState _model;
        private IGameDataService _dataService;
        private Side _playerSide;
        private Piece _selectedPiece;
        private BoardPosition? _selectedSquare;
        private IEnumerable<BoardPosition> _validMoves;
        private IEnumerable<BoardPosition> _validAttacks;
        private bool _isCheckmate = false;

        private IEnumerable<BoardPosition> ValidMoves
        {
            get
            {
                if (_validMoves == null)
                {
                    if (_selectedPiece == null)
                    {
                        throw new InvalidOperationException("There is no piece selected.");
                    }

                    _validMoves = GetValidMoves(_selectedPiece, _selectedSquare.Value);
                }

                return _validMoves;
            }
        }

        private IEnumerable<BoardPosition> ValidAttacks
        {
            get
            {
                if (_validAttacks == null)
                {
                    if (_selectedPiece == null)
                    {
                        throw new InvalidOperationException("There is no piece selected.");
                    }

                    _validAttacks = GetValidAttacks(_selectedPiece, _selectedSquare.Value);
                }

                return _validAttacks;
            }
        }

        private IEnumerable<BoardPosition> GetValidMoves(Piece toMove, BoardPosition from)
        {
            var moves = toMove.Capabilities.GetMoves(from, toMove.Side, toMove.TurnLastMoved, _model);

            // filter out moves that don't leave the game in a legal state
            return moves.Where((to) =>
                {
                    //TODO: Extract processing from inside this loop
                    var turnTask = TurnFromPiecePositionChange(toMove, from, to, false);
                    turnTask.Wait();
                    var turn = turnTask.Result;
                    return IsMoveLegal(turn);
                });
        }

        private IEnumerable<BoardPosition> GetValidAttacks(Piece toMove, BoardPosition from)
        {
            var attacks = toMove.Capabilities.GetAttacks(from, toMove.Side, toMove.TurnLastMoved, _model, false);

            // filter out moves that don't leave the game in a legal state
            return attacks.Where((to) =>
            {
                var turnTask = TurnFromPiecePositionChange(toMove, from, to, false);
                turnTask.Wait();
                var turn = turnTask.Result;
                return IsMoveLegal(turn);
            });

        }

        internal LocalPlayerController(Side playerSide, IGameDataService dataService)
        {
            _playerSide = playerSide;
            _dataService = dataService;
        }

        internal void AttachTo(TwoPlayer board, GameState model)
        {
            _board = board;
            _model = model;

            board.SquareTapped += OnBoardSquareTapped;
            board.SwitchPlayers += (sender, e) =>
                {
                    StartTurn();
                };

            model.StateChanged += Model_StateChanged;
        }

        private async void Model_StateChanged(object sender, StateChangedEventArgs e)
        {
            if (_playerSide != e.GameState.GameInformation.NextToMove)
            {
                // if we just moved, we're responsible for persisting the move to the data store.
                await _dataService.StoreTurnRecordAsync(
                    e.GameState.GameInformation.GameDataStore.GameID,
                    SerializationHelper.SerializeTurnRecord(e.TurnRecord));

                if (IsCheckMate())
                {
                    // TODO: stalemate
                    _board.IsEnabled = false;
                    _board.RevealAllPositions();
                    await _dataService.CompleteGameAsync(_model.GameInformation.GameDataStore.GameID);
                }
            }
        }



        private async Task<TurnRecord> TurnFromPiecePositionChange(Piece pieceMoved, BoardPosition from, BoardPosition to, bool doGetPromotionSelectionIfNeeded)
        { 
            TurnRecord turn = null;
            CaptureRecord capture = null;
            PromotionRecord promotion = null;
            var move = new MoveRecord(pieceMoved, from, to);

            Piece pieceCaptured = null;
            if ((pieceCaptured = _model.GetPieceAt(to)) != null)
            {
                capture = new CaptureRecord(pieceCaptured);
            }

            if (pieceMoved.Capabilities is PawnCapabilities)
            {
                // TODO: handle en passant if needed

                // promotion
                if (doGetPromotionSelectionIfNeeded &&
                    ((pieceMoved.Side == Side.White && to.Row == 7) ||
                     (pieceMoved.Side == Side.Black && to.Row == 0)))
                {
                    var newCapability = await _board.GetPromotionSelectionAsync();
                    promotion = new PromotionRecord(pieceMoved, newCapability);
                }
            }

            if (capture != null || promotion != null)
            {
                turn = new TurnRecord(move, capture, promotion);
            }
            else if (IsMoveCastle(move))
            {
                var kingMovedBy = move.To.Column - move.From.Column;
                var moveDirection = kingMovedBy / Math.Abs(kingMovedBy);
                var newCastlePosition = new BoardPosition((uint)(move.From.Column + moveDirection), move.From.Row);

                if (kingMovedBy < 0)
                {
                    var castlePosition = new BoardPosition(0, move.From.Row);
                    var castleToMove = _model.GetPieceAt(castlePosition);
                    var secondaryMove = new MoveRecord(castleToMove, castlePosition, newCastlePosition);
                    turn = new TurnRecord(move, secondaryMove);
                }
                else
                {
                    var castlePosition = new BoardPosition(7, move.From.Row);
                    var castleToMove = _model.GetPieceAt(castlePosition);
                    var secondaryMove = new MoveRecord(castleToMove, castlePosition, newCastlePosition);
                    turn = new TurnRecord(move, secondaryMove);
                }
            }
            else
            {
                turn = new TurnRecord(move);
            }

            return turn;
        }

        internal async void OnBoardSquareTapped(object sender, PositionEventArgs e)
        {
            if (_model.GameInformation.NextToMove != _playerSide)
            {
                return;
            }

            var pieceAtSquare = _model.GetPieceAt(e.LogicalPosition);
            if (pieceAtSquare != null && pieceAtSquare.Side == _model.GameInformation.NextToMove)
            {
                SetSelection(pieceAtSquare, e.LogicalPosition);
                return;
            }

            if (_selectedPiece != null)
            {
                if (ValidMoves.Contains(e.LogicalPosition) || ValidAttacks.Contains(e.LogicalPosition))
                {
                    try
                    {
                        _model.ExecuteMove(await TurnFromPiecePositionChange(_selectedPiece, _selectedSquare.Value, e.LogicalPosition, true), true);
                        SetVisiblePositionsForPlayer();
                        _board.IsSwitchPlayerEnabled = true;
                        _board.IsGameBoardEnabled = false;
                        SetSelection(null, null);
                    }
                    catch (OperationCanceledException)
                    {
                        // ignore touch
                        return;
                    }
                }
            }
        }

        internal void StartTurn()
        {
            if (_model.GameInformation.NextToMove == _playerSide)
            {
                SetVisiblePositionsForPlayer();
                _board.IsSwitchPlayerEnabled = false;
                _board.IsGameBoardEnabled = true;
            }
        }

        internal void SetVisiblePositionsForPlayer()
        {
            var visibleSquares = new HashSet<BoardPosition>();
            foreach (var piecePositionPair in _model.ActivePiecePositions)
            {
                var piece = piecePositionPair.Key;
                var position = piecePositionPair.Value;

                if (piece.Side == _playerSide)
                {
                    visibleSquares.UnionWith(piece.Capabilities.GetVisibleSquares(position, piece.Side, piece.TurnLastMoved, _model, GameController.Visibility.Restricted));
                    visibleSquares.Add(position);
                }
            }

            _board.SetVisiblePositions(visibleSquares);
        }

        private void SetSelection(Piece selectedPiece, BoardPosition? selectedSquare)
        {
            if (this._selectedPiece != selectedPiece)
            {
                System.Diagnostics.Contracts.Contract.Assert(!this._selectedSquare.Equals(selectedSquare));
                this._selectedPiece = selectedPiece;
                this._selectedSquare = selectedSquare;
                this._validMoves = null;
                this._validAttacks = null;

                _board.ClearHighlights();

                if (selectedPiece != null)
                {
                    _board.HighlightPositions(new BoardPosition[] { selectedSquare.Value }, Board.PositionHighlightLevel.Primary);
                    _board.HighlightPositions(ValidAttacks, Board.PositionHighlightLevel.Secondary);
                    _board.HighlightPositions(ValidMoves, Board.PositionHighlightLevel.Tertiary);
                }
            }
            else
            {
                System.Diagnostics.Contracts.Contract.Assert(this._selectedSquare.Equals(selectedSquare));
            }
        }

        private bool IsMoveCastle(MoveRecord move)
        {
            return move.PieceMoved.Capabilities is KingCapabilities && Math.Abs(move.To.Column - move.From.Column) > 1;
        }

        private bool IsMoveLegal(TurnRecord potentialTurn)
        {
            // Special cases for castling 
            if (IsMoveCastle(potentialTurn.Move))
            {
                Side sideInCheck = Side.White;
                if (IsCheck(_model, out sideInCheck) && sideInCheck == _model.GameInformation.NextToMove)
                {
                    // Castling is not allowed when player is in check
                    return false;
                }

                var kingMovedBy = potentialTurn.Move.To.Column - potentialTurn.Move.From.Column;
                var moveDirection = kingMovedBy / Math.Abs(kingMovedBy);
                var from = potentialTurn.Move.From;
                var movedThrough = new BoardPosition((uint)(from.Column + moveDirection), from.Row);
                var turnTask = TurnFromPiecePositionChange(potentialTurn.Move.PieceMoved, potentialTurn.Move.From, movedThrough, false);
                var intermediateState = _model.SimulateMove(turnTask.Result);
                if (!IsGameStateLegal(intermediateState))
                {
                    // if it is illegal for the king to move to the intermediate position, castling is not allowed.
                    return false;
                }
            }
           
            var stateAfterTurn = _model.SimulateMove(potentialTurn);
            if (!IsGameStateLegal(stateAfterTurn))
            {
                return false;
            }

            return true;
        }

        private bool IsGameStateLegal(IGameState state)
        {
            Side sideInCheck = Side.White;
            if (IsCheck(state, out sideInCheck))
            {
                if (sideInCheck != state.GameInformation.NextToMove)
                {
                    // Player is in check in it isn't their turn
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if either side is in check in the specified game state.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="sideInCheck">The side that is in check. Should be ignored if the return value is false.</param>
        /// <returns>True if either side is in check, false otherwise.</returns>
        private bool IsCheck(IGameState state, out Side sideInCheck)
        {
            sideInCheck = Side.White;
            BoardPosition whiteKingPosition = new BoardPosition();
            BoardPosition blackKingPosition = new BoardPosition();

            foreach (var piecePositionPair in state.ActivePiecePositions)
            {
                var piece = piecePositionPair.Key;
                if (piece.Capabilities is KingCapabilities)
                {
                    if (piece.Side == Side.White)
                    {
                        whiteKingPosition = piecePositionPair.Value;
                    }
                    else
                    {
                        blackKingPosition = piecePositionPair.Value;
                    }
                }
            }

            foreach (var piecePositionPair in state.ActivePiecePositions)
            {
                var piece = piecePositionPair.Key;
                var position = piecePositionPair.Value;
                if (piece.Side == Side.Black && piece.Capabilities.CanAttack(whiteKingPosition, position, piece.Side, piece.TurnLastMoved, state))
                {
                    sideInCheck = Side.White;
                    return true;
                }

                if (piece.Side == Side.White && piece.Capabilities.CanAttack(blackKingPosition, position, piece.Side, piece.TurnLastMoved, state))
                {
                    sideInCheck = Side.Black;
                    return true;
                }
            }

            return false;
        }

        private bool IsCheckMate()
        {
            Side sideInCheck = Side.White;

            if (IsCheck(_model, out sideInCheck) && sideInCheck == _model.GameInformation.NextToMove)
            {
                foreach (var piecePositionPair in _model.ActivePiecePositions)
                {
                    var piece = piecePositionPair.Key;
                    var position = piecePositionPair.Value;
                    if (piece.Side == _model.GameInformation.NextToMove)
                    {
                        if (GetValidMoves(piece, position).Count() > 0 || GetValidAttacks(piece, position).Count() > 0)
                        {
                            // At least one of the current player's pieces has a valid move
                            return false;
                        }
                    }
                }

                // The current player is in check and has no valid moves
                return true;
            }

            return false;
        }

    }
}
