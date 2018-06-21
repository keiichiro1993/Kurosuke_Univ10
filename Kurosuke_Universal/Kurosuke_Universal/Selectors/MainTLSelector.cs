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
    public class MainTLSelector : DataTemplateSelector
    {

        public DataTemplate TweetTemplate { get; set; }
        public DataTemplate EventTemplate { get; set; }
        public DataTemplate LoadButtonTemplate { get; set; }

        public MainTLSelector()
        {
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            AdvancedTweet itemObject = (AdvancedTweet)item;
            if (itemObject.tweetRole == AdvancedTweet.TweetRoles.Tweet)
            {
                switch (itemObject.state)
                {
                    case AdvancedTweet.States.Tweet:
                        return TweetTemplate;
                    case AdvancedTweet.States.Retweeted:
                        return EventTemplate;
                    default:
                        return new DataTemplate();
                }
            }
            else if (itemObject.tweetRole == AdvancedTweet.TweetRoles.LoadButton)
            {
                return LoadButtonTemplate;
            }
            else
            {
                return new DataTemplate();
            }
        }

    }
}