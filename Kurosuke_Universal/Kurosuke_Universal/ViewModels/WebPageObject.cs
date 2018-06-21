using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Kurosuke_Universal.ViewModels
{
    public class WebPageObject
    {
        public ObservableCollection<WebPageObject> Collection;
        public Uri uri { get; set; }
        public int Index { get; set; }
        public FlipView flipView { get; set; }

        public WebPageObject(ObservableCollection<WebPageObject> Collection, Uri uri, FlipView flipView)
        {
            this.Collection = Collection;
            this.uri = uri;
            this.Index = Collection.Count;
            this.flipView = flipView;
        }
    }
}
