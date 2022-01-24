using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http.Filters;
using FlarumUWP.Core.Models;
using FlarumUWP.Helpers;
using FlarumUWP.Views.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FlarumUWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UsersPage : Page
    {
        public string APIText;
        public MainPagePosts MainPagePosts;
        public ObservableCollection<Datum> Users = new ObservableCollection<Datum>();
        public string Website = ApplicationData.Current.LocalSettings.Values["website"] as string;

        public UsersPage()
        {
            this.InitializeComponent();
        }

        private async void APIHyperlinkBurron_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"https://{Website}/api/users?&sort=-lastSeenAt&page[limit]=50"));

        }



        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var link = new Uri($"https://{Website}/api/users?&sort=-lastSeenAt&page[limit]=50");
            var dialog = new CloudFlareHelper(link);
            string result;
            if((bool)ApplicationData.Current.LocalSettings.Values["CloudflareUnderAttackMode"])
            {
                 await dialog.ShowAsync();
                 result = dialog.HTMLData;
            }
            else
            {
                var _filter = new HttpBaseProtocolFilter();
                var http = new Windows.Web.Http.HttpClient(_filter);
                var response = await http.GetAsync(link);
                result = await response.Content.ReadAsStringAsync();
            }
            MainPagePosts = await wvbProxy.GetMainPagePostsInfo(result, true, "");
            GetData();
        }
        public void GetData()
        {
            Users = MainPagePosts.data;
            foreach (var user in Users)
            {
                user.attributes.lastSeenAt = wvbProxy.StringDateTimeFriendFormat(user.attributes.lastSeenAt);
            }
            UsersListView.ItemsSource = Users;
        }

        private async void UsersListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clicked = (Datum)e.ClickedItem;
            var user = new Included_Attributes {
                slug = clicked.attributes.slug,
                displayName = clicked.attributes.displayName,
                bio = clicked.attributes.bio,
                username = clicked.attributes.username,
                lastSeenAt = clicked.attributes.lastSeenAt,
                joinTime = clicked.attributes.joinTime};

            var dialog = new UserDialog(user);
            await dialog.ShowAsync();

        }
    }
}
