using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kurosuke_Universal.ViewModels
{
    public class MainPageViewModel
    {
        public ObservableCollection<AccountColumn> columns { get; set; }

        public ObservableCollection<UserAccessToken> tokens { get; set; }


        public MainPageViewModel(ObservableCollection<AccessToken> accessTokens)
        {
            columns = new ObservableCollection<AccountColumn>();
            tokens = new ObservableCollection<UserAccessToken>();
            foreach (var token in accessTokens)
            {
                columns.Add(new AccountColumn(token, tokens));
                tokens.Add(new UserAccessToken(token));
                TmpUserData.Accounts = tokens;
            }
            Run();
        }

        public void Run()
        {
            foreach (var column in columns)
            {
                try
                {
                    column.Run();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            var adColumn = new AccountColumn();
            adColumn.accountColumnRole = AccountColumn.AccountColumnRole.Advertising;
            columns.Add(adColumn);
        }
    }
}
