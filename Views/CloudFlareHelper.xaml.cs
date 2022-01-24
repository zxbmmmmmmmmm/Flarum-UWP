using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace FlarumUWP.Views
{
    public sealed partial class CloudFlareHelper : ContentDialog
    {
        public string HTMLData;
        public Uri Link;

        public CloudFlareHelper(Uri uri)
        {
            Link = uri;
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            InitializeComponent();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            
            CloudFlareHelperWebView.Reload();
           
        }

     

        private void HTMLAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            HTMLData = sender.Text;
        }

        public async void CloudFlareHelperDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var html = await CloudFlareHelperWebView.ExecuteScriptAsync("document.body.innerHTML");
            try
            {
                html = html.Remove(0, 67);
            }
            catch (Exception)
            {
                throw new ArgumentOutOfRangeException();
            }

            HTMLData = html.Remove(html.Length - 12, 12);
            HTMLData = System.Text.RegularExpressions.Regex.Unescape(HTMLData);
            returnData();
        }
        public string returnData()
        {
            return HTMLData;
        }

        private void CloudFlareHelperDialog_Loaded(object sender, RoutedEventArgs e)
        {
            CloudFlareHelperWebView.Source = Link;
        }


        private async void CloudFlareHelperWebView_NavigationCompleted(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            var html = await CloudFlareHelperWebView.ExecuteScriptAsync("document.body.innerHTML");

            if (html != "null" )
            {
                try
                {
                    html = html.Remove(0, 67);
                }
                catch (Exception)
                {
                    LoadingProgressBar.ShowError = true;
                    throw new ArgumentOutOfRangeException();
                }

                HTMLData = html.Remove(html.Length - 12, 12);
                HTMLData = System.Text.RegularExpressions.Regex.Unescape(HTMLData);
                HTMLData = HTMLData.Replace("\\u003Chr\\u003E", string.Empty);//去分割线

                if (HTMLData.Contains("links") || HTMLData.Contains("data") && html.StartsWith("\\n") == false)
                {
                    returnData();
                    CloudFlareHelperDialog.Hide();
                }
                else
                {
                    Thread.Sleep(500);
                    CloudFlareHelperWebView.NavigationCompleted += CloudFlareHelperWebView_NavigationCompleted;
                }
            }
            else
            {
                Thread.Sleep(500);
                CloudFlareHelperWebView.NavigationCompleted += CloudFlareHelperWebView_NavigationCompleted;
            }

        }

      
    }
}
