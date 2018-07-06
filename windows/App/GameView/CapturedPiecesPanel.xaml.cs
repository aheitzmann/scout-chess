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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ScoutChess.GameView
{
    public sealed partial class CapturedPiecesPanel : UserControl
    {
        private class DecreasingOrderPieceComparer : IComparer<Piece>
        {
            private IDictionary<PieceType, int> _pieceRanks;

            public DecreasingOrderPieceComparer()
            {
                _pieceRanks = new Dictionary<PieceType, int>();
                _pieceRanks.Add(PieceType.Pawn, 1);
                _pieceRanks.Add(PieceType.Knight, 2);
                _pieceRanks.Add(PieceType.Bishop, 3);
                _pieceRanks.Add(PieceType.Rook, 4);
                _pieceRanks.Add(PieceType.Queen, 5);
                _pieceRanks.Add(PieceType.King, 6);

            }

            public int Compare(Piece x, Piece y)
            {
                // Sorts in order of decreasing value
                return _pieceRanks[y.Capabilities.Type] - _pieceRanks[x.Capabilities.Type];
            }
        }

        private List<Piece> _capturedPieces;
        private Panel[] _verticalPanels = new Panel[10];
        private IComparer<Piece> _pieceComparer;

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(CapturedPiecesPanel), new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var panel = sender as CapturedPiecesPanel;
            var newOrientation = (Orientation)e.NewValue;
            panel.OnOrientationChanged(newOrientation);
        }

        public Side BoardOrientedFor
        {
            get { return (Side)GetValue(BoardOrientedForProperty); }
            set { SetValue(BoardOrientedForProperty, value); }
        }

        public static readonly DependencyProperty BoardOrientedForProperty =
            DependencyProperty.Register("BoardOrientedFor", typeof(Side), typeof(CapturedPiecesPanel), new PropertyMetadata(Side.White));

        internal CapturedPiecesPanel()
        {
            this.InitializeComponent();
            _capturedPieces = new List<Piece>();
            _pieceComparer = new DecreasingOrderPieceComparer();

            StackPanel panel;
            for (int i = 0; i < _verticalPanels.Length; i++)
            {
                panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;
                panel.SetValue(Grid.RowProperty, i);
                VerticalPanel.Children.Add(panel);
                _verticalPanels[i] = panel;

            }
        }

        internal void AddPiece(Piece capturedPiece)
        {
            if (_capturedPieces.Contains(capturedPiece))
            {
                throw new InvalidOperationException("The specified piece is already in the captured pieces panel.");
            }
            _capturedPieces.Add(capturedPiece);
            RepositionPiece(capturedPiece);
        }

        internal void RemovePiece(Piece capturedPiece)
        {
            if (!_capturedPieces.Contains(capturedPiece))
            {
                throw new InvalidOperationException("The specified piece is not in the captured pieces panel");
            }

            _capturedPieces.Remove(capturedPiece);
            RemovePieceVisualFromPanel(capturedPiece.Visual);
        }

        private void RemovePieceVisualFromPanel(FrameworkElement pieceVisual)
        {
            var parent = pieceVisual.Parent as Panel;
            if (parent != null)
            {
                parent.Children.Remove(pieceVisual);
            }
        }

        private void RepositionPiece(Piece piece)
        {
            if (piece.Capabilities.Type == PieceType.King)
            {
                throw new ArgumentException("Kings cannot be captured.");
            }

            WhiteCapuredHorizontalListView.Items.Remove(piece);
            BlackCapuredHorizontalListView.Items.Remove(piece);

            RemovePieceVisualFromPanel(piece.Visual);

            if (Orientation == Orientation.Horizontal)
            {
                var addTo = piece.Side == Side.White ? WhiteCapuredHorizontalListView : BlackCapuredHorizontalListView;
                int i;
                for (i = 0; i < addTo.Items.Count && _pieceComparer.Compare(piece, addTo.Items[i] as Piece) < 0; i++);
                addTo.Items.Insert(i, piece);
            }
            else
            {
                var panelIndex = BoardOrientedFor == piece.Side ? 0 : 5;
                panelIndex += (int)piece.Capabilities.Type;
                var panel = _verticalPanels[panelIndex];
                if (piece.Capabilities.Type == PieceType.Pawn)
                {
                    if (panel.Children.Count == 1)
                    {
                        var tb = new TextBlock();
                        tb.Text = "x2";
                        panel.Children.Add(tb);
                    }
                    else if (panel.Children.Count > 1)
                    {
                        var tb = panel.Children[panel.Children.Count - 1] as TextBlock;
                        var numStr = tb.Text.Substring(1);
                        var num = int.Parse(numStr);
                        tb.Text = "x" + (num + 1).ToString();
                    }
                }
                else
                {
                    panel.Children.Add(piece.Visual);
                }
            }
        }

        private void OnOrientationChanged(Orientation newOrientation)
        {
            if (newOrientation == Orientation.Horizontal)
            {
                HorizontalPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                VerticalPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                HorizontalPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed ;
                VerticalPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            foreach (var piece in _capturedPieces)
            {
                RepositionPiece(piece);
            }
        }
        
    }
}
