using Kurosuke_Universal.CustomControl;
using Kurosuke_Universal.Models;
using Kurosuke_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kurosuke_Universal.Utils
{
    public static class TmpUserData
    {
        public static ObservableCollection<UserAccessToken> Accounts;
        public static ObservableCollection<User> Friends;
        public static DateTime PreviousNavigationDateTime;//Web表示用
        public static DateTime PreviousBackRequest;//戻るボタン用
    }
}
