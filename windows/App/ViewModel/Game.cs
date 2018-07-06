using ScoutChess.GameModel;
using ScoutChess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace ScoutChess.ViewModel
{
    public class Game : ScoutChess.Common.BindableBase
    {
        private static readonly Brush _whiteBrush = new SolidColorBrush(Colors.White);
        private static readonly Brush _blackBrush = new SolidColorBrush(Colors.Black);


        internal Game(GameInformation gameInfo)
        {
            UpdateState(gameInfo); 
        }

        internal void UpdateState(GameInformation gameInfo)
        {
            GameInformation = gameInfo;
            FriendlyName = gameInfo.FriendlyName;
            TimeLastMoved = gameInfo.TimeLastMoved;
            MoveCount = gameInfo.MoveCount;

            if (gameInfo.NextToMove == Side.White)
            {
                AccentBackground = _whiteBrush;
                AccentForeground = _blackBrush;
            }
            else
            {
                AccentBackground = _blackBrush;
                AccentForeground = _whiteBrush;
            }
        }

        internal GameInformation GameInformation
        {
            get;
            private set;
        }

        private string _friendlyName = string.Empty;
        public string FriendlyName
        {
            get { return this._friendlyName; }
            set { this.SetProperty(ref this._friendlyName, value); }
        }

        private DateTime _timeLastMoved;
        public DateTime TimeLastMoved
        {
            get { return this._timeLastMoved; }
            set { this.SetProperty(ref this._timeLastMoved, value); }
        }

        private uint _moveCount;
        public uint MoveCount
        {
            get { return this._moveCount; }
            set { this.SetProperty(ref this._moveCount, value); }
        }

        private Brush _accentBackground;
        public Brush AccentBackground
        {
            get { return this._accentBackground; }
            set { this.SetProperty(ref this._accentBackground, value); }
        }

        private Brush _accentForeground;
        public Brush AccentForeground
        {
            get { return this._accentForeground; }
            set { this.SetProperty(ref this._accentForeground, value); }
        }
    }
}
