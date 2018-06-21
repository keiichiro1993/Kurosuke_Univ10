using Kurosuke_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Kurosuke_Universal.Selectors
{
    public class ColumnSelector : DataTemplateSelector
    {

        public DataTemplate AccountColumnTemplate { get; set; }
        public DataTemplate AdvertisingColumnTemplate { get; set; }
        public DataTemplate MovieAdvertisingColumnTemplate { get; set; }
        public DataTemplate MobileAdvertisingColumnTemplate { get; set; }
        public ColumnSelector()
        {
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            AccountColumn itemObject = (AccountColumn)item;
            switch (itemObject.accountColumnRole)
            {
                case AccountColumn.AccountColumnRole.AccountColumn:
                    return AccountColumnTemplate;
                case AccountColumn.AccountColumnRole.Advertising:
                    //モバイルなら普通の広告、PCなら動画広告
                    /*switch (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily)
                    {
                        case "Windows.Universal":
                            return AdvertisingColumnTemplate;
                        case "Windows.Desktop":
                            return MovieAdvertisingColumnTemplate;
                        case "Windows.Mobile":
                            return MobileAdvertisingColumnTemplate;
                    }*/
                    if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                    {
                        return MobileAdvertisingColumnTemplate;
                    }
                    return AdvertisingColumnTemplate; ;
                default:
                    return null;
            }
        }

    }
}
