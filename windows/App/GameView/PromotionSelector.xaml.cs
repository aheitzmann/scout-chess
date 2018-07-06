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
using ScoutChess.GameController;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ScoutChess.GameView
{
    internal sealed partial class PromotionSelector : UserControl
    {
        internal class PromotionSelectedEventArgs : EventArgs
        {
            internal IPieceCapabilities NewCapabilities { get; private set; }

            internal PromotionSelectedEventArgs(IPieceCapabilities newCapabilities)
            {
                NewCapabilities = newCapabilities;
            }
        }

        private IPieceCapabilities newCapabilities = null;
        private event EventHandler<PromotionSelectedEventArgs> promotionSelected;
        private event EventHandler promotionCancelled;

        internal event EventHandler<PromotionSelectedEventArgs> PromotionSelected
        { 
            add { promotionSelected += value; } 
            remove { promotionSelected -= value; }
        }

        internal event EventHandler PromotionCancelled
        {
            add { promotionCancelled += value; }
            remove { promotionCancelled -= value; }
        }

        internal PromotionSelector()
        {
            this.InitializeComponent();
        }

        private void OnPromotionSelected(PromotionSelectedEventArgs e)
        {
            var handler = promotionSelected;
            if (handler != null)
            {
                handler.Invoke(this, e);
            }
        }

        private void OnPromotionCancelled(EventArgs e)
        {
            var handler = promotionCancelled;
            if (handler != null)
            {
                handler.Invoke(this, e);
            }
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            PromoteBtn.IsEnabled = true;

            if (KnightRadioBtn.IsChecked.Value)
            {
                newCapabilities = KnightCapabilities.Singleton;
            }
            else if (BishopRadioBtn.IsChecked.Value)
            {
                newCapabilities = BishopCapabilities.Singleton;
            }
            else if (RookRadioBtn.IsChecked.Value)
            {
                newCapabilities = RookCapabilities.Singleton;
            }
            else if (QueenRadioBtn.IsChecked.Value)
            {
                newCapabilities = QueenCapabilities.Singleton;
            }
        }

        private void PromoteBtn_Click_1(object sender, RoutedEventArgs e)
        {
            OnPromotionSelected(new PromotionSelectedEventArgs(newCapabilities));
        }

        private void CancelBtn_Click_1(object sender, RoutedEventArgs e)
        {
            OnPromotionCancelled(new EventArgs());
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            var knightImage = new Image();
            knightImage.Source = Board.whiteKnightImageSource;
            var bishopImage = new Image();
            bishopImage.Source = Board.whiteBishopImageSource;
            var rookImage = new Image();
            rookImage.Source = Board.whiteRookImageSource;
            var queenImage = new Image();
            queenImage.Source = Board.whiteQueenImageSource;

            KnightRadioBtn.Content = knightImage;
            BishopRadioBtn.Content = bishopImage;
            RookRadioBtn.Content = rookImage;
            QueenRadioBtn.Content = queenImage;
        }
    }
}
