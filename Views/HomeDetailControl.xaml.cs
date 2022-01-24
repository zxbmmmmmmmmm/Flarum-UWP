using ReverseMarkdown;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using FlarumUWP.Core.Models;
using FlarumUWP.Helpers;
using FlarumUWP.Views.Controls;

namespace FlarumUWP.Views
{

    public sealed partial class HomeDetailControl : UserControl
    {
        /*public SampleOrder ListMenuItem
        {
            get { return GetValue(ListMenuItemProperty) as SampleOrder; }
            set { SetValue(ListMenuItemProperty, value); }
        }*/
        public Datum ListMenuItem
        {
            get { return GetValue(ListMenuItemProperty) as Datum; }
            set { SetValue(ListMenuItemProperty, value); }
            
        }


        public Uri PostLink;
        public MainPagePosts Posts = new MainPagePosts();
        public ObservableCollection<Included> PostsData = new ObservableCollection<Included>();

        public ObservableCollection<Included> Users = new ObservableCollection<Included>();

        public static DependencyProperty ListMenuItemProperty = DependencyProperty.Register("ListMenuItem", typeof(Datum), typeof(HomeDetailControl), new PropertyMetadata(null, OnListMenuItemPropertyChanged));

        public string Website = ApplicationData.Current.LocalSettings.Values["website"] as string;

        public HomeDetailControl()

        {
            InitializeComponent();
        }

        private static void OnListMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as HomeDetailControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
            control.ViewPosterButton.IsChecked = false;
            control.GetData();
        }


        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListMenuItem != null && Posts!= null)
            {
                GetData();                
            }
        }



        public async void GetData()
        {
            PostsListView.IsEnabled = false;
            PostsListView.Visibility = Visibility.Collapsed;
            CopyrightInfo.Visibility = Visibility.Collapsed;
            LoadingStackPanel.Visibility = Visibility.Visible;

            Posts = new MainPagePosts();
            PostsData.Clear();
            Users.Clear();
            PostLink = new Uri($"https://{Website}/api/discussions/{ListMenuItem.id}?&page[limit]=50");
            Posts = await wvbProxy.GetPostsView(PostLink);

            foreach (Included included in Posts.included)
            {
                if (included.type == "posts")
                {
                    if (included.attributes.contentHtml != null)//HTML转MarkDown
                    {
                        var converter = new ReverseMarkdown.Converter();
                        included.attributes.contentMD = converter.Convert(included.attributes.contentHtml);
                    }

                    if(included.relationships != null)
                    {
                        if (included.relationships.user == null)//已注销
                        {
                            included.relationships.user = new Core.Models.User();
                            included.relationships.user.data = new Core.Models.Data();
                            included.relationships.user.data.id = "已注销";
                        }
                        else
                            PostsData.Add(included);
                    }
                      
                }
                else if (included.type == "users")
                {
                    var expCommments = included.attributes.commentCount * 21;
                    var expDiscussions = included.attributes.commentCount * 33;
                    var expTotal = expCommments + expDiscussions;
                    included.attributes.exp = expTotal;

                    if(expTotal <= 100)
                    {
                        included.attributes.level = 1;
                        included.attributes.levelName = "收获时节";
                    }
                    else if (expTotal <= 500)
                    {
                        included.attributes.level = 2;
                        included.attributes.levelName = "星夜飞流";
                    }
                    else if(expTotal <= 800)
                    {
                        included.attributes.level = 3;
                        included.attributes.levelName = "北景炫光";
                    }
                    else if (expTotal <= 1000)
                    {
                        included.attributes.level = 4;
                        included.attributes.levelName = "南极异芒";
                    }
                    else if (expTotal <= 2000)
                    {
                        included.attributes.level = 5;
                        included.attributes.levelName = "赤色琉璃";
                    }
                    else if (expTotal <= 3000)
                    {
                        included.attributes.level = 6;
                        included.attributes.levelName = "蓝天麦田";
                    }
                    else if (expTotal <= 5000)
                    {
                        included.attributes.level = 7;
                        included.attributes.levelName = "钢铁房梁";
                    }
                    else if (expTotal <= 8000)
                    {
                        included.attributes.level = 8;
                        included.attributes.levelName = "绿叶葱葱";
                    }
                    else if (expTotal <= 12000)
                    {
                        included.attributes.level = 9;
                        included.attributes.levelName = "草原大树";
                    }
                    else if (expTotal <= 30000)
                    {
                        included.attributes.level = 10;
                        included.attributes.levelName = "蓝天绿地";
                    }
                    else if (expTotal <= 50000)
                    {
                        included.attributes.level = 11;
                        included.attributes.levelName = "牧园角牛";
                    }
                    else if (expTotal <= 80000)
                    {
                        included.attributes.level = 12;
                        included.attributes.levelName = "镜面反射";
                    }
                    else if (expTotal <= 120000)
                    {
                        included.attributes.level = 13;
                        included.attributes.levelName = "尖锐玻璃";
                    }
                    else if (expTotal <= 150000)
                    {
                        included.attributes.level = 14;
                        included.attributes.levelName = "绿草茵茵";
                    }
                    else if (expTotal <= 200000)
                    {
                        included.attributes.level = 15;
                        included.attributes.levelName = "微观世界";
                    }
                    else if (expTotal <= 300000)
                    {
                        included.attributes.level = 16;
                        included.attributes.levelName = "平湖秋色";
                    }
                    else if (expTotal <= 500000)
                    {
                        included.attributes.level = 17;
                        included.attributes.levelName = "溪流源头";
                    }
                    else 
                    {
                        included.attributes.level = 18;
                        included.attributes.levelName = "炫彩极光";
                    }

                    Users.Add(included);
                }
            }

            int i = 0;
            foreach (Included included in PostsData)//将Post与User一一对应
            {
                //int j = 0;
                for(int j = 0; j < PostsData.Count; j++)
                {
                    if (included.relationships.user.data.id != null)
                    {
                        if (included.relationships.user.data.id == Users[j].id)
                        {
                            PostManager(i, j);
                            break;
                        }
                    }
                }
                i++;
            }

            if (ListMenuItem.attributes.commentCount > 50)
                WarningInfoBar.IsOpen = true;
            else
                WarningInfoBar.IsOpen = false;

            var list = PostsData.OrderBy(x => x.attributes.number).ToList();
            PostsData = new ObservableCollection<Included>(list);
            PostsNumberTextBlock.Text = PostsData.Count.ToString();

            PostsListView.ItemsSource = PostsData;
            PostsListView.Visibility = Visibility.Visible;
            LoadingStackPanel.Visibility = Visibility.Collapsed;
            CopyrightInfo.Visibility = Visibility.Visible;
            PostsListView.IsEnabled = true;
        }
        private void PostManager(int i,int j)
        {
            PostsData[i].attributes.createdAt = wvbProxy.StringDateTimeFriendFormat(PostsData[i].attributes.createdAt);

            PostsData[i].attributes.user = Users[j].attributes;
            PostsData[i].attributes.displayName = Users[j].attributes.displayName;
                        
            if (PostsData[i].attributes.editedAt != null)//是否编辑
            {
                PostsData[i].attributes.editedAt = wvbProxy.StringDateTimeFriendFormat(PostsData[i].attributes.editedAt);

                PostsData[i].attributes.edited_visibility = Visibility.Visible;
            }
            else
            {
                PostsData[i].attributes.edited_visibility = Visibility.Collapsed;

            }

            if (Users[j].attributes.avatarUrl != null)//头像处理
                PostsData[i].attributes.poster_picture = Users[j].attributes.avatarUrl;
            else
                PostsData[i].attributes.poster_picture = "/Assets/guest.png";

            if (PostsData[i].attributes.contentType == "comment")//帖子内容处理
            {
                PostsData[i].attributes.iconVisiblity = Visibility.Collapsed;
                PostsData[i].attributes.htmlVisiblity = Visibility.Visible;

            }
            else
            {
                PostsData[i].attributes.iconVisiblity = Visibility.Visible;
                PostsData[i].attributes.htmlVisiblity = Visibility.Collapsed;
                PostsData[i].attributes.contentHtml = "";

                if (PostsData[i].attributes.contentType == "discussionStickied")
                {
                    PostsData[i].attributes.icon = "\uE840";
                    PostsData[i].attributes.iconInfo = "置顶了此帖";
                }
                else if (PostsData[i].attributes.contentType == "discussionTagged")
                {
                    PostsData[i].attributes.icon = "\uE8EC";
                    PostsData[i].attributes.iconInfo = "更改了标签";

                }
                else if (PostsData[i].attributes.contentType == "discussionLocked")
                {
                    PostsData[i].attributes.icon = "\uE72E";
                    PostsData[i].attributes.iconInfo = "锁定了此帖";
                }
                else if (PostsData[i].attributes.contentType == "discussionRenamed")
                {
                    PostsData[i].attributes.icon = "\uE8AC";
                    PostsData[i].attributes.iconInfo = "更改了标题";
                }
            }


        }

        private async void WebButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"https://community.wvbtech.com/d/{ListMenuItem.id}"));

        }

        private async void APIButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($"https://community.wvbtech.com/api/discussions/{ListMenuItem.id}?&page[limit]=50"));

        }

        private async void MarkDownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            var link = e.Link;
            if (link.Contains($"{Website}/d/{ListMenuItem.id}"))//本贴内容
            {
                string[] split = link.Split(new char[] { '/' }, 6);//拆分
                int post = int.Parse(split[split.Count() - 1]);
                PostsListView.ScrollIntoView(PostsListView.Items[post]);
            }
            else
            {
                 await Launcher.LaunchUriAsync(new Uri(e.Link));
            }
        }
        private string ItemToKeyHandler(object item)
        {
            Included dataItem = item as Included;
            if (dataItem == null) return null;

            return dataItem.Id.ToString();
        }
        private void MarkDownTextBlock_ImageClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            var clickedImage = e.Link;
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = (HyperlinkButton)sender;
            var clicked = (Included)btn.DataContext;

            var user = clicked.attributes.user;
            if (user.joinTime.Contains("+00:00"))
            {
                user.joinTime = wvbProxy.StringDateTimeFriendFormat(user.joinTime);
            }
            if(user.poster_picture == null)            
               user.poster_picture = "/Assets/guest.png";           
            /*if (user.lastSeenAt.Contains("+00:00"))
            {
                var dateTime = wvbProxy.StringToDateTime(user.lastSeenAt);
                user.lastSeenAt = wvbProxy.FriendFormat(dateTime);
            }*/

            var dialog = new UserDialog(user);
            await dialog.ShowAsync();
        }

        private async void ViewSourceButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (MenuFlyoutItem)sender;
            var data = (Included)item.DataContext;
            var dialog = new PostSourceDialog(data.attributes.contentMD,data.attributes.contentHtml);
            await dialog.ShowAsync();
        }

        private void ViewPosterButton_Click(object sender, RoutedEventArgs e)
        {
            var appBar = sender as AppBarToggleButton;
            if ((bool)appBar.IsChecked)
            {
                var PosterID = ListMenuItem.relationships.user.data.id;
                var filted = PostsData.Where(p => p.relationships.user.data.id == PosterID);
                PostsListView.ItemsSource = filted;
                PostsNumberTextBlock.Text = filted.ToList().Count.ToString();
            }
            else
            {
                PostsListView.ItemsSource = PostsData;
                PostsNumberTextBlock.Text = PostsData.Count.ToString();

            }


        }

        private void ToTopButton_Click(object sender, RoutedEventArgs e)
        {
            ForegroundElement.ChangeView(0,0,1);
        }

        private IAsyncOperation<object> KeyToItemHandler(string key)
        {
            Func<System.Threading.CancellationToken, Task<object>> taskProvider = token =>
            {
                var items = PostsListView.ItemsSource as List<Included>;
                if (items != null)
                {
                    var targetItem = items.FirstOrDefault(m => m.attributes.number == int.Parse(key));
                    return Task.FromResult((object)targetItem);
                }
                else
                {
                    return Task.FromResult((object)null);
                }
            };

            return AsyncInfo.Run(taskProvider);
        }
    }
}
