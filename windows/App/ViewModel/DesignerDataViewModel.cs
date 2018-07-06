using ScoutChess.GameModel;
using ScoutChess.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.ViewModel
{
    class DesignerDataViewModel
    {

        internal DesignerDataViewModel()
        {
            GameGroups = new ObservableCollection<GameGroup>();

            var gameInfos = new List<GameInformation>();
            var rand = new Random(DateTime.Now.Millisecond);
            GameDataStore emptyGameDataStore = new GameDataStore();
            gameInfos.Add(new GameInformation(emptyGameDataStore, "Jesus vs Kim", GameType.OfflineTwoPlayer, DateTime.Now - new TimeSpan(rand.Next(100000)), 4, true));
            gameInfos.Add(new GameInformation(emptyGameDataStore, "BenCohen", GameType.OfflineTwoPlayer, DateTime.Now - new TimeSpan(rand.Next(100000)), 7, true));
            gameInfos.Add(new GameInformation(emptyGameDataStore, "Jesse", GameType.OfflineTwoPlayer, DateTime.Now - new TimeSpan(rand.Next(100000)), 12, true));
            gameInfos.Add(new GameInformation(emptyGameDataStore, "Peter", GameType.OfflineTwoPlayer, DateTime.Now - new TimeSpan(rand.Next(100000)), 1, true));

            var activeGameGroup = new GameGroup(null, "Active Games", null, null, null);
            foreach (var gameInfo in gameInfos)
            {
                activeGameGroup.Items.Add(new Game(gameInfo));
            }

            var completedGameGroup = new GameGroup(null, "Completed Games", null, null, null);
            foreach (var gameInfo in gameInfos)
            {
                completedGameGroup.Items.Add(new Game(gameInfo));
            }

            GameGroups.Add(activeGameGroup);
            GameGroups.Add(completedGameGroup);
        }

        /// <summary>
        ///  Binding source for the main page's grid of active and completed games
        /// </summary>
        internal ObservableCollection<GameGroup> GameGroups
        {
            get;
            private set;
        }
    }
}
