using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FlarumUWP.Core.Models;
using FlarumUWP.Helpers;

namespace FlarumUWP.Views.Controls
{
    public sealed partial class UserDialog : ContentDialog
    {
        public Included_Attributes SelectedUser = new Included_Attributes();
        public UserDialog(Included_Attributes user)
        {
            SelectedUser = user;
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            InitializeComponent();
            
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
