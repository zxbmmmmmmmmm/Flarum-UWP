using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using FlarumUWP.Core.Models;
using FlarumUWP.Helpers;
using FlarumUWP.Services;
using FlarumUWP.Views.Controls;

namespace FlarumUWP.Views
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/UWP/pages/settings-codebehind.md
    // TODO WTS: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public UserInfo MyData = new UserInfo();
        public ObservableCollection<Included> UserProfileViews = new ObservableCollection<Included>();
        public ObservableCollection<Included> Users = new ObservableCollection<Included>();
        public string groupText = string.Empty;

        public string Website = ApplicationData.Current.LocalSettings.Values["website"] as string;
        //public bool IsCloudflareUnderUnderAttackModeOn = (bool)ApplicationData.Current.LocalSettings.Values["CloudflareUnderAttackMode"];

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            await Task.CompletedTask;
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - Beta {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async void ThemeChanged_CheckedAsync(object sender, RoutedEventArgs e)
        {
            var param = (sender as RadioButton)?.CommandParameter;

            if (param != null)
            {
                await ThemeSelectorService.SetThemeAsync((ElementTheme)param);
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

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LoginDialog();
            await dialog.ShowAsync();

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string username = localSettings.Values["username"] as string;
            var userData = await wvbProxy.GetMainPagePostsInfo("",false, $"https://{Website}/api/users?filter[q]={username}");
            var userID = userData.data[0].id;
            localSettings.Values["userID"] = userID;

            if (username != null)
            {
                GetUserData(userID);
            }
        }



        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            var userID = localSettings.Values["userID"] as string;
            string username = localSettings.Values["username"] as string;
            if(username != null && username != "")
            {
                GetUserData(userID);
            }
            
            if ((bool)localSettings.Values["CloudflareUnderAttackMode"])
            {
                CloudflareToggleSwitch.IsOn = true;
            }
            else
            {
                CloudflareToggleSwitch.IsOn = false;
            }

        }
        public async void GetUserData(string userID)
        {
            LoadingProgressBar.Visibility = Visibility.Visible;

            UserProfileViews.Clear();
            var me = new UserInfo();
            try
            {
                me = await wvbProxy.GetUserInfo($"https://{Website}/api/users/{userID}");
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
            catch
            {
                LoadingProgressBar.ShowError = true;
            }
            MyData = me;
            GroupsTextBlock.Text = groupText;
            DisplayNameTextBlock.Text = MyData.data.attributes.displayName;
            NameTextBlock.Text = MyData.data.attributes.username;
            if(me.data.attributes.avatarUrl == null)
            {
                me.data.attributes.avatarUrl = "/Assets/guest.png";
            }
            BitmapImage myAvatar = new BitmapImage();
            myAvatar.UriSource = new Uri(me.data.attributes.avatarUrl);
            AvatarPersonPicture.ProfilePicture = myAvatar;

            var includeds = me.included;
            List<Included> profileViews = new List<Included>();
            List<Included> groups = new List<Included>();
            if(includeds != null)
            {
                foreach (var included in includeds)
            {
                if (included.type == "userprofileview")
                {
                    profileViews.Add(included);
                }
                if (included.type == "users")
                {
                    Users.Add(included);
                }
                if (included.type == "groups")
                {
                    groups.Add(included);
                }
            }
                int i = 0;
                foreach (var user in Users)
            {
                profileViews[i].attributes.visited_at = wvbProxy.StringDateTimeFriendFormat(profileViews[i].attributes.visited_at);
                profileViews[i].attributes.user = user.attributes;
                UserProfileViews.Add(profileViews[i]);
                i++;

            }
                int j = 0;
                foreach (var group in groups)
               {
                    if (j == 0)
                    {
                        groupText = groupText.Insert(groupText.Length, group.attributes.nameSingular);
                    }
                    else
                    {
                        groupText = groupText.Insert(groupText.Length, "|" + group.attributes.nameSingular);
                    }
                    j++;
                }
            }
            else
            {
            }

        }

        private async void UserProfileViewsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var clicked = UserProfileViewsListView.SelectedItem as Included;
            var user = clicked.attributes.user;
            if (user.joinTime.Contains("00:00"))
            {
                user.joinTime = wvbProxy.StringDateTimeFriendFormat(user.joinTime);
            }

            var dialog = new UserDialog(clicked.attributes.user);
            await dialog.ShowAsync();
        }

        private async void EditWebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditWebsiteDialog();
            await dialog.ShowAsync();

        }

        private void CloudflareToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (CloudflareToggleSwitch.IsOn)
            {
                localSettings.Values["CloudflareUnderAttackMode"] = true;
            }
            else
            {
                localSettings.Values["CloudflareUnderAttackMode"] = false;
            }

        }
    }
}
