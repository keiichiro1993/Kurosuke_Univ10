using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using Kurosuke_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Kurosuke_Universal.CustomControl
{
    public sealed class TextBoxForTweet : Control
    {
        public TextBox TweetTextBox;
        public Button TweetButton { get; set; }
        public ListBox TweetUserList { get; set; }
        public AdvancedTweet ReplyTweet { get; set; }//リプライボタン押したら入れればいい
        public ObservableCollection<SendableImage> Images { get; set; }
        public Frame CurrentFrame { get; set; }

        private ObservableCollection<UserAccessToken> replyingUsers;
        private Flyout ReplyUserSuggestionFlyout;
        private ListBox ReplyUserSuggestionList;
        public TextBoxForTweet()
        {
            this.DefaultStyleKey = typeof(TextBoxForTweet);
        }

        protected override void OnApplyTemplate()
        {
            TweetTextBox = (TextBox)this.GetTemplateChild("TweetTextBox");
            ReplyUserSuggestionFlyout = (Flyout)this.GetTemplateChild("ReplyUserSuggestionFlyout");
            ReplyUserSuggestionList = (ListBox)this.GetTemplateChild("ReplyUserSuggestionList");

            replyingUsers = new ObservableCollection<UserAccessToken>();
            ReplyUserSuggestionList.DataContext = replyingUsers;

            TweetTextBox.KeyUp += textBox_KeyUp;
            TweetTextBox.KeyDown += textBox_KeyDown;
            TweetTextBox.TextChanged += TweetTextBox_TextChanged;
            ReplyUserSuggestionFlyout.Opened += ReplyUserSuggestionFlyout_Opened;
        }

        private void ReplyUserSuggestionFlyout_Opened(object sender, object e)
        {
            TweetTextBox.Focus(FocusState.Programmatic);
        }

        //キーボードショートカット（Ctrl+EnterでTweet）
        bool isCtrlKeyPressed = false;

        private void textBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Control:
                    isCtrlKeyPressed = false;
                    //TweetTextBox.AcceptsReturn = true;//コントロールキー押下中は改行をゆるさない...絶対にゆるさないっ！！
                    break;
                case VirtualKey.Enter:
                    if (isCtrlKeyPressed)
                    {
                        TweetButtonClicked(TweetButton, e);
                    }
                    else
                    {
                        if (textingReplyFlg)
                        {
                            var item = (UserAccessToken)ReplyUserSuggestionList.SelectedItem;
                            var screenName = item.user.screen_name;
                            var before = TweetTextBox.Text.Substring(0, screenNameBeginFrom - 1);
                            var after = TweetTextBox.Text.Substring(TweetTextBox.SelectionStart);
                            TweetTextBox.Text = before + "@" + screenName + " " + after;
                            ReplyUserSuggestionFlyout.Hide();
                            //TweetTextBox.AcceptsReturn = true;
                            textingReplyFlg = false;
                            TweetTextBox.SelectionStart = before.Length + screenName.Length + 2;
                        }
                    }
                    break;
                case VirtualKey.Space:
                    if (!isCtrlKeyPressed)
                    {
                        if (textingReplyFlg)
                        {
                            //TweetTextBox.AcceptsReturn = true;
                            textingReplyFlg = false;
                            ReplyUserSuggestionFlyout.Hide();
                        }
                    }
                    break;
            }
        }

        private void textBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Control:
                    isCtrlKeyPressed = true;
                    //TweetTextBox.AcceptsReturn = false;//コントロールキー押下中は改行をゆるさない...絶対にゆるさないっ！！
                    break;
                case VirtualKey.Down:
                    if (textingReplyFlg)
                    {
                        if (ReplyUserSuggestionList.Items.Count > ReplyUserSuggestionList.SelectedIndex + 1)
                        {
                            ReplyUserSuggestionList.SelectedIndex++;
                        }
                    }
                    break;
                case VirtualKey.Up:
                    if (textingReplyFlg)
                    {
                        if (ReplyUserSuggestionList.SelectedIndex > 0)
                        {
                            ReplyUserSuggestionList.SelectedIndex--;
                        }
                    }
                    break;
                case VirtualKey.Enter:
                    break;
            }
        }


        /// <summary>
        /// スクリーンネームの入力補助用
        /// </summary>
        bool textingReplyFlg = false;
        int screenNameBeginFrom;
        private void TweetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int charpos = TweetTextBox.SelectionStart;
            if (0 < charpos && charpos < TweetTextBox.Text.Length + 1)
            {
                if (TweetTextBox.Text[charpos - 1] == '@')
                {
                    textingReplyFlg = true;
                    //TweetTextBox.AcceptsReturn = false;
                    screenNameBeginFrom = charpos;
                    return;
                }

                if (textingReplyFlg)
                {
                    if (TweetTextBox.Text.Length > screenNameBeginFrom)
                    {
                        if (TweetTextBox.Text[screenNameBeginFrom - 1] != '@')
                        {
                            textingReplyFlg = false;
                            ReplyUserSuggestionFlyout.Hide();
                            //TweetTextBox.AcceptsReturn = true;
                            return;
                        }


                        string textingScreenName = "";
                        if (TweetTextBox.Text.Length > screenNameBeginFrom && TweetTextBox.Text.Length > charpos - 1)
                        {
                            if (screenNameBeginFrom < charpos)
                            {
                                textingScreenName = TweetTextBox.Text.Substring(screenNameBeginFrom, (charpos - 1) - screenNameBeginFrom);
                            }
                        }

                        if (TmpUserData.Friends != null)
                        {
                            var query = from friend in TmpUserData.Friends
                                        where friend.screen_name.StartsWith(textingScreenName)
                                        orderby friend.screen_name descending
                                        select friend;
                            var list = query.Take(5).ToList();
                            if (list.Count != 0)
                            {
                                replyingUsers.Clear();

                                foreach (var item in list)
                                {
                                    replyingUsers.Add(new UserAccessToken(item));
                                }

                                ReplyUserSuggestionFlyout.ShowAt(TweetTextBox);
                                ReplyUserSuggestionList.SelectedIndex = 0;
                                TweetTextBox.Focus(FocusState.Keyboard);
                            }
                            else
                            {
                                ReplyUserSuggestionFlyout.Hide();
                            }
                        }
                    }
                }
            }
            else
            {
                textingReplyFlg = false;
                ReplyUserSuggestionFlyout.Hide();
                //TweetTextBox.AcceptsReturn = true;
                return;
            }
        }

        public void TweetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Images == null)
            {
                TweetButtonClickedOnlyText(sender, e);
            }
            else
            {
                TweetButtonClickedWithMedia(sender, e);
            }
        }

        private async void TweetButtonClickedOnlyText(object sender, RoutedEventArgs e)
        {
            var success = 0;
            var users = TweetUserList.SelectedItems;
            ((Button)sender).IsEnabled = false;
            if (!string.IsNullOrEmpty(TweetTextBox.Text))
            {
                if (users != null)
                {
                    foreach (var user in users)
                    {
                        var client = new TwitterClient(((UserAccessToken)user).accessToken);
                        try
                        {
                            await client.UpdateStatus(TweetTextBox.Text, ReplyTweet);
                        }
                        catch (Exception ex)
                        {
                            var message = new MessageDialog(ex.Message + "\n" + ex.InnerException.Message, "おや？ なにかがおかしいようです。");
                            await message.ShowAsync();
                            success++;
                        }
                    }
                }
            }
            else
            {
                var message = new MessageDialog("空ではツイートを送信できません。", "おや？ なにかがおかしいようです。");
                await message.ShowAsync();
                success++;
            }
            if (success == 0)
            {
                TweetTextBox.Text = "";
                ReplyTweet = null;
            }
            ((Button)sender).IsEnabled = true;
        }

        private async void TweetButtonClickedWithMedia(object sender, RoutedEventArgs e)
        {
            var users = TweetUserList.SelectedItems;
            ((Button)sender).IsEnabled = false;
            var success = 0;
            if (!string.IsNullOrEmpty(TweetTextBox.Text))
            {
                if (users != null)
                {
                    if (Images.Count != 0)
                    {
                        foreach (var user in users)
                        {
                            try
                            {
                                var client = new TwitterClient(((UserAccessToken)user).accessToken);
                                var content = new HttpMultipartFormDataContent();

                                var status = TweetTextBox.Text;
                                foreach (var image in Images)
                                {
                                    content.Add(image.httpContent, "media[]", image.file.Name);
                                }
                                content.Add(new HttpStringContent(status), "status");

                                if (ReplyTweet != null)
                                {
                                    content.Add(new HttpStringContent(ReplyTweet.tweet.id_str), "in_reply_to_status_id");
                                }

                                await client.UpdateStatusWithMedia(content);
                            }
                            catch (Exception ex)
                            {
                                var message = new MessageDialog(ex.Message + "\n" + ex.InnerException.Message, "おや？ なにかがおかしいようです。");
                                await message.ShowAsync();
                                success++;
                            }
                        }
                    }
                    else
                    {

                        foreach (var user in users)
                        {
                            try
                            {
                                var client = new TwitterClient(((UserAccessToken)user).accessToken);
                                await client.UpdateStatus(TweetTextBox.Text, ReplyTweet);
                            }
                            catch (Exception ex)
                            {
                                var message = new MessageDialog(ex.Message + "\n" + ex.InnerException.Message, "おや？ なにかがおかしいようです。");
                                await message.ShowAsync();
                                success++;
                            }
                        }
                    }
                }
            }
            else
            {
                var message = new MessageDialog("空ではツイートを送信できません。", "おや？ なにかがおかしいようです。");
                await message.ShowAsync();
                success++;
            }
            if (success == 0)
            {
                CurrentFrame.GoBack();
            }
        }

    }
}
