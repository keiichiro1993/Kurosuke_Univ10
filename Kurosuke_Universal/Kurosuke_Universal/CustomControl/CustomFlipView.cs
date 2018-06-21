using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Kurosuke_Universal.CustomControl
{
    /// <summary>
    /// ナビゲーションボタンを常に表示する FlipView
    /// </summary>
    [TemplatePart(Name = "AlwaysShowPreviousButtonHorizontal", Type = typeof(Button))]
    [TemplatePart(Name = "AlwaysShowNextButtonHorizontal", Type = typeof(Button))]
    public class CustomFlipView : FlipView
    {
        #region Privates

        /// <summary>
        /// 次へボタン
        /// </summary>
        private Button nextButton;

        /// <summary>
        /// 前へボタン
        /// </summary>
        private Button prevButton;

        #endregion //Privates

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CustomFlipView()
            : base()
        {
            this.DefaultStyleKey = typeof(CustomFlipView);
            this.Loaded += this.OnLoaded;
        }

        /// <summary>
        /// 読み込み完了イベントハンドラ
        /// </summary>
        /// <param name="sender">イベント発行者</param>
        /// <param name="e">イベント引数</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.nextButton = this.GetTemplateChild("AlwaysShowNextButtonHorizontal") as Button;
            if (this.nextButton != null)
            {
                this.nextButton.Click += this.OnNextClick;
            }

            this.prevButton = this.GetTemplateChild("AlwaysShowPreviousButtonHorizontal") as Button;
            if (this.prevButton != null)
            {
                this.prevButton.Click += this.OnPrevClick;
            }
            this.UpdateNavigationButtonState();
            this.SelectionChanged += this.OnSelectionChanged;
        }

        /// <summary>
        /// 選択アイテム更新イベントハンドラ
        /// </summary>
        /// <param name="sender">イベント発行者</param>
        /// <param name="e">イベント引数</param>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateNavigationButtonState();
        }

        /// <summary>
        /// アイテム情報更新イベントハンドラ
        /// </summary>
        /// <param name="e">イベント引数</param>
        protected override void OnItemsChanged(object e)
        {
            base.OnItemsChanged(e);
            this.UpdateNavigationButtonState();
        }

        /// <summary>
        /// 前へボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender">イベント発行者</param>
        /// <param name="e">イベント引数</param>
        private void OnPrevClick(object sender, RoutedEventArgs e)
        {
            this.SelectedIndex--;
            this.UpdateNavigationButtonState();
        }

        /// <summary>
        /// 次へボタンクリックイベントハンドラ
        /// </summary>
        /// <param name="sender">イベント発行者</param>
        /// <param name="e">イベント引数</param>
        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            this.SelectedIndex++;
            this.UpdateNavigationButtonState();
        }

        /// <summary>
        /// ナビゲーションボタンの表示状態更新
        /// </summary>
        private void UpdateNavigationButtonState()
        {
            if (this.nextButton != null)
            {
                if (this.Items == null || this.Items.Count < 1 || this.Items.Count <= this.SelectedIndex + 1)
                {
                    this.nextButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.nextButton.Visibility = Visibility.Visible;
                }
            }
            if (this.prevButton != null)
            {
                if (this.Items == null || this.Items.Count < 1 || this.SelectedIndex < 1)
                {
                    this.prevButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.prevButton.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 子要素を破棄する
        /// </summary>
        protected override void OnDisconnectVisualChildren()
        {
            this.Loaded -= this.OnLoaded;
            this.SelectionChanged -= this.OnSelectionChanged;

            if (this.nextButton != null)
            {
                this.nextButton.Click -= this.OnNextClick;
            }
            if (this.prevButton != null)
            {
                this.prevButton.Click -= this.OnPrevClick;
            }

            base.OnDisconnectVisualChildren();
        }
    }
}
