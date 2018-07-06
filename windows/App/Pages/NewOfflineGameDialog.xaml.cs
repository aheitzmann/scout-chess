using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ScoutChess.ViewModel;

namespace ScoutChess.Pages
{
   
    internal sealed partial class NewOfflineGameDialog : UserControl
    {

        internal NewOfflineGameDialog()
        {
            this.InitializeComponent();
        }

        internal static DependencyProperty StartGameCommandProperty = 
            DependencyProperty.Register("StartGameCommand", typeof(ICommand), typeof(NewOfflineGameDialog), null);

        internal ICommand StartGameCommand
        {
            get
            {
                return (ICommand)GetValue(StartGameCommandProperty);
            }
            set
            {
                SetValue(StartGameCommandProperty, value);
            }
        }

        private void StartBtn_Click_1(object sender, RoutedEventArgs e)
        {
            var gameOptions = new GameViewModel.NewOfflineGameOptions(Player1TextBox.Text, Player2TextBox.Text);
            
            if (StartGameCommand != null && StartGameCommand.CanExecute(gameOptions))
            {
                StartGameCommand.Execute(gameOptions);
            }
        }

        private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            StartBtn.IsEnabled = !String.IsNullOrEmpty(Player1TextBox.Text) && !String.IsNullOrEmpty(Player2TextBox.Text);
        }

        private void CancelBtn_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
