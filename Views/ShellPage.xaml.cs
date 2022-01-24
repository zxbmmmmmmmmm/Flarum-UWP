using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

using FlarumUWP.Helpers;
using FlarumUWP.Services;
using Windows.UI.Xaml.Controls.Primitives;
using System.Collections.ObjectModel;
using FlarumUWP.Core.Models;
using Windows.Storage;

namespace FlarumUWP.Views
{
    // TODO WTS: Change the icons and titles for all NavigationViewItems in ShellPage.xaml.
    public sealed partial class ShellPage : Page, INotifyPropertyChanged
    {
        private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

        public ObservableCollection<Datum> Notifications = new ObservableCollection<Datum>();
        public MainPagePosts AllNotifications = new MainPagePosts();

        public static string selectedTag;
        private bool _isBackEnabled;
        private WinUI.NavigationViewItem _selected;
        public string Website = ApplicationData.Current.LocalSettings.Values["website"] as string;


        public bool IsBackEnabled
        {
            get { return _isBackEnabled; }
            set { Set(ref _isBackEnabled, value); }
        }

        public WinUI.NavigationViewItem Selected
        {
            get { return _selected; }
            set { Set(ref _selected, value); }
        }

        public ShellPage()
        {
            InitializeComponent();
            DataContext = this;
            Initialize();
        }

        private void Initialize()
        {
            NavigationService.Frame = shellFrame;
            NavigationService.NavigationFailed += Frame_NavigationFailed;
            NavigationService.Navigated += Frame_Navigated;
            navigationView.BackRequested += OnBackRequested;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            KeyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            KeyboardAccelerators.Add(_backKeyboardAccelerator);
            await Task.CompletedTask;
            Notifications = AllNotifications.data;
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw e.Exception;
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            IsBackEnabled = NavigationService.CanGoBack;
            if (e.SourcePageType == typeof(SettingsPage))
            {
                Selected = navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            var selectedItem = GetSelectedItem(navigationView.MenuItems, e.SourcePageType);
            if (selectedItem != null)
            {
                Selected = selectedItem;
            }
        }

        private WinUI.NavigationViewItem GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
        {
            foreach (var item in menuItems.OfType<WinUI.NavigationViewItem>())
            {
                if (IsMenuItemForPageType(item, pageType))
                {
                    return item;
                }

                var selectedChild = GetSelectedItem(item.MenuItems, pageType);
                if (selectedChild != null)
                {
                    return selectedChild;
                }
            }

            return null;
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        {
            var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;
            return pageType == sourcePageType;
        }

        private void OnItemInvoked(WinUI.NavigationView sender, WinUI.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                NavigationService.Navigate(typeof(SettingsPage), null, args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItem.ToString() == "通知")
            {
                if(Notifications == null)
                {
                    GetNotifications();
                }
                else
                {
                    var flyout = FlyoutBase.GetAttachedFlyout(NotificationItem);
                    NotificationsListView.ItemsSource = Notifications;
                    flyout.ShowAt(NotificationItem);
                }

            }
            else
            {
                var selectedItem = args.InvokedItemContainer as WinUI.NavigationViewItem;
                //selectedTag = selectedItem.Tag.ToString();
                var pageType = selectedItem?.GetValue(NavHelper.NavigateToProperty) as Type;
                
                if (pageType != null)
                {
                    if (pageType.Name == "HomePage")
                    {

                    }
                    NavigationService.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
                }
            }
        }
        public static string GetTag()
        {
            return selectedTag;
        }

        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args)
        {
            NavigationService.GoBack();
        }

        private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
            return keyboardAccelerator;
        }

        private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var result = NavigationService.GoBack();
            args.Handled = result;
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

        private void GetNotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            GetNotifications();
        }
        public async void GetNotifications()
        {
            Website = ApplicationData.Current.LocalSettings.Values["website"] as string;
            AllNotifications = await wvbProxy.GetMainPagePostsInfo("", false, $"https://{Website}/api/notifications");
            Notifications = AllNotifications.data;
            foreach (var notifiction in Notifications)
            {
                notifiction.attributes.createdAt = wvbProxy.StringDateTimeFriendFormat(notifiction.attributes.createdAt);
            }
            int unReadedNotifictions = Notifications.Where(p => p.attributes.isRead == false).Count();
            var flyout = FlyoutBase.GetAttachedFlyout(NotificationItem);
            NotificationsListView.ItemsSource = Notifications;
            flyout.ShowAt(NotificationItem);
            if(unReadedNotifictions == 0)
            {
                NotificationCount.Visibility = Visibility.Collapsed;
            }
            else
            {
                NotificationCount.Visibility = Visibility.Visible;
                NotificationCount.Value = unReadedNotifictions;
            }
        }
    }
}
