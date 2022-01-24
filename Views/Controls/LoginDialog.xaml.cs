using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace FlarumUWP.Views.Controls
{
    public sealed partial class LoginDialog : ContentDialog
    {
        public string Website = ApplicationData.Current.LocalSettings.Values["website"] as string;

        public LoginDialog()
        {
            this.InitializeComponent();
            LoginWebview.Source = new Uri($"https://{Website}/login");

        }
        public string UserName;
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if(PrimaryButtonText == "下一步")
            {
            }
        }

        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            if(LoginPivot.SelectedItem == UserNamePivotItem)
            {
                LoginPivot.SelectedItem = WebViewPivotItem;
            }
            else
            {
                LoginContentDialog.Hide();
            }
        }

        private void LoginPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoginPivot.SelectedItem == WebViewPivotItem)
            {
                NextStepButton.Content = "完成";
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["username"] = UserName;
                GetCookies();
            }
            else
            {
                NextStepButton.Content = "下一步";

            }
        }

        private void GetCookies()
        {
        }

        private void UserNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var data = sender as TextBox;
            UserName = data.Text;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri($"https://{Website}/");
            LoginWebview.CoreWebView2.Navigate(uri.ToString());
        }
    }
}
