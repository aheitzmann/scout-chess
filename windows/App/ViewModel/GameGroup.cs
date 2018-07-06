using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoutChess.ViewModel
{
    public class GameGroup : DataCommon
    {
        internal GameGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            // This shouldn't happen very often, so instead of being clever about inserting and
            // remvoing items in the right place, we'll just resort when needed.
            var items = new ObservableCollection<Game>(Items.OrderByDescending((game) => game.TimeLastMoved));
            _topItems = new ObservableCollection<Game>(items.Take(12));
        }

        private ObservableCollection<Game> _items = new ObservableCollection<Game>();
        public ObservableCollection<Game> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<Game> _topItems = new ObservableCollection<Game>();

        /// <summary>
        /// Binding source for the grid of games in this group
        /// </summary>
        public ObservableCollection<Game> TopItems
        {
            get {return this._topItems; }
        }
    }
}
