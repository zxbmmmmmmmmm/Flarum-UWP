using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

using FlarumUWP.Core.Models;
using FlarumUWP.Core.Services;
using FlarumUWP.Helpers;
using FlarumUWP.Views.Controls;

namespace FlarumUWP.Views
{
    public sealed partial class HomePage : Page, INotifyPropertyChanged
    {
        private Datum _selected;
        public static HomePage Current { get; private set; }
        public ObservableCollection<Datum> MainPageData = new ObservableCollection<Datum>();

        public ObservableCollection<Datum> AddingData = new ObservableCollection<Datum>();
        public ObservableCollection<Included> AddingIncluded = new ObservableCollection<Included>();
        public string Sort;

        public ObservableCollection<Included> Included = new ObservableCollection<Included>();

        public MainPagePosts AllPostsData = new MainPagePosts();//获得的数据
        public ObservableCollection<Included> AddingPostsData = new ObservableCollection<Included>();
        public ObservableCollection<Included> AddingUsers = new ObservableCollection<Included>();
        public ObservableCollection<Included> AddingTags = new ObservableCollection<Included>();

        public ObservableCollection<Included> PostsData = new ObservableCollection<Included>();
        public ObservableCollection<Included> Users = new ObservableCollection<Included>();
        public ObservableCollection<Included> Tags = new ObservableCollection<Included>();
        public string selectedTag = ShellPage.GetTag();
        public string HtmlData;

        public string Website = ApplicationData.Current.LocalSettings.Values["website"] as string;

        public Datum Selected
        {
            get { return _selected; }
            set { Set(ref _selected, value); }
        }

        //public ObservableCollection<SampleOrder> SampleItems { get; private set; } = new ObservableCollection<SampleOrder>();

        public HomePage()
        {
            InitializeComponent();
            Current = this;
            Loaded += HomePage_Loaded;
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            //MainPageData = wvbProxy.GetMainPagePostsInfo().Result.data;
            /*SampleItems.Clear();

            var data = await SampleDataService.GetListDetailsDataAsync();

            foreach (var item in data)
            {
                SampleItems.Add(item);
            }*/

            if (ListDetailsViewControl.ViewState == ListDetailsViewState.Both)
            {
                Selected = MainPageData.FirstOrDefault();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            
            GetData($"https://{Website}/api/discussions?{Sort}");
        }

        private async void LoadMoreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AllPostsData = await wvbProxy.GetMainPagePostsInfo("", false, AllPostsData.links.next);
            }
            catch (Exception)
            {
                throw new TimeoutException();
            }
          
            
            AddingData = AllPostsData.data;
            AddingIncluded = AllPostsData.included;
            LoadMore();
        }

        private void LoadMore()
        {
            FiltTypes();
            PostToUser();
            foreach (var addingDatum in AddingData)
            {
                MainPageData.Add(addingDatum);
            }
            ListDetailsViewControl.ItemsSource = MainPageData;

        }

        private void FiltTypes()
        {
            AddingPostsData.Clear();
            AddingUsers.Clear();
            AddingTags.Clear();

            foreach (Included included in AllPostsData.included)//筛选类型
            {
                if (included.type == "posts")
                    AddingPostsData.Add(included);
                else if (included.type == "users")
                    AddingUsers.Add(included);
                else if (included.type == "tags")
                    AddingTags.Add(included);
            }
        }

        private void PostToUser()
        {
            int i = 0;

            foreach (var datum in AddingData)//将Post与User一一对应
            {
                int j = 0;
                AddingData[i].attributes.lastPostedAt = wvbProxy.StringDateTimeFriendFormat(AddingData[i].attributes.lastPostedAt);
                foreach (var datum2 in AddingUsers)
                {
                    if(AddingData[i].relationships.user == null)
                    {
                        var user = new User();
                        var data = new Data { id = "0", type = "user" };
                        user.data = data;
                        
                        AddingData[i].relationships.user = user;
                        AddingData[i].attributes.poster_display_name = "已注销";
                        AddingData[i].attributes.poster_picture = "/Assets/guest.png";
                        continue;
                    }
                    if (AddingData[i].relationships.user.data.id == AddingUsers[j].id)
                    {
                        AddingData[i].attributes.poster_display_name = AddingUsers[j].attributes.displayName;
                        AddingData[i].attributes.user = AddingUsers[j].attributes;
                        if (AddingUsers[j].attributes.avatarUrl != null)
                            AddingData[i].attributes.poster_picture = AddingUsers[j].attributes.avatarUrl;//切换头像
                        else
                            AddingData[i].attributes.poster_picture = "/Assets/guest.png";//若没有头像则使用默认
                        break;
                    }
                    j++;
                }
                i++;
            }
        }

        private async void UserButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = (HyperlinkButton)sender;       
            var clicked = (Datum)btn.DataContext;

            var user = clicked.attributes.user;
            if (user.joinTime.Contains("+00:00"))
            {
                user.joinTime = wvbProxy.StringDateTimeFriendFormat(user.joinTime);
            }
            if (user.poster_picture == null)
                user.poster_picture = "/Assets/guest.png";
            /*if (user.lastSeenAt.Contains("+00:00"))
            {
                var dateTime = wvbProxy.StringToDateTime(user.lastSeenAt);
                user.lastSeenAt = wvbProxy.FriendFormat(dateTime);
            }*/
            var dialog = new UserDialog(user);
            await dialog.ShowAsync();
        }

        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            var clicked = args.InvokedItem;
            switch (clicked)
            {
                case "最新回复":
                    Sort = "";
                    break;
                case "热门主题":
                    Sort = "sort=-commentCount";
                    break;
                case "新鲜出炉":
                    Sort = "sort=-createdAt";
                    break;
                case "陈年旧帖":
                    Sort = "sort=createdAt";
                    break;
                case "最多被赞":
                    Sort = "sort=votes";
                    break;
                case "精品创作":
                    Sort = "sort=front";
                    break;
                default:
                    break;
            };
            
            MainPageData.Clear();
            GetData($"https://{Website}/api/discussions?{Sort}");

        }
        private async void GetData(string link)
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            try
            {
                AllPostsData = await wvbProxy.GetMainPagePostsInfo("", false, link);
                AddingData = AllPostsData.data;
                AddingIncluded = AllPostsData.included;
                LoadMore();
                MainPageData = AllPostsData.data;
                Included = AllPostsData.included;
                LoadingProgressBar.Visibility = Visibility.Collapsed;

            }
            catch
            {
                LoadingProgressBar.ShowError = true;
            }
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            var data = sender as ComboBox;
            var item = data.SelectedItem as ComboBoxItem;
            var clicked = item.Content.ToString();
            switch (clicked)
            {
                case "最新回复":
                    Sort = "";
                    break;
                case "热门主题":
                    Sort = "sort=-commentCount";
                    break;
                case "新鲜出炉":
                    Sort = "sort=-createdAt";
                    break;
                case "陈年旧帖":
                    Sort = "sort=createdAt";
                    break;
                case "最多被赞":
                    Sort = "sort=votes";
                    break;
                case "精品创作":
                    Sort = "sort=front";
                    break;
                default:
                    break;
            };
            MainPageData.Clear();
            GetData($"https://{Website}/api/discussions?{Sort}");
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            MainPageData.Clear();
            GetData($"https://{Website}/api/discussions");
        }

        private void ListDetailsViewControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
