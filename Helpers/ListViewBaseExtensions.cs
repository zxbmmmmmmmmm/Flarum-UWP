using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FlarumUWP.Helpers
{
    internal static class DependencyObjectExtesnions
    {
        public static ScrollViewer GetScrollViewer(this DependencyObject element)
        {
            if (element is ScrollViewer)
            {
                return (ScrollViewer)element;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }
    }

    internal static class ListViewBaseExtensions
    {
        /// <summary>
        /// Get all visible items in the ScrollViewer of the ListView
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <returns></returns>
        public static List<T> GetAllVisibleItems<T>(this ListViewBase listView)
        {
            var scrollViewer = listView.GetScrollViewer();
            if (scrollViewer == null)
            {
                return null;
            }

            List<T> targetItems = new List<T>();
            foreach (T item in listView.Items)
            {
                var itemContainer = listView.ContainerFromItem(item) as FrameworkElement;
                bool isVisible = IsVisibileToUser(itemContainer, scrollViewer, true);
                if (isVisible)
                {
                    targetItems.Add(item);
                }
            }

            return targetItems;
        }

        /// <summary>
        /// Get the first visible item in the ScrollViewer of the ListView
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <returns></returns>
        public static T GetFirstVisibleItem<T>(this ListViewBase listView, bool isTotallyVisble = false)
        {
            var scrollViewer = listView.GetScrollViewer();
            if (scrollViewer == null)
            {
                return default(T);
            }

            T targetItem = default(T);
            foreach (T item in listView.Items)
            {
                var itemContainer = listView.ContainerFromItem(item) as FrameworkElement;
                bool isVisible = IsVisibileToUser(itemContainer, scrollViewer, isTotallyVisble);
                if (isVisible)
                {
                    targetItem = item;
                    break;
                }
            }

            return targetItem;
        }

        /// <summary>
        /// Code from here:
        ///  https://social.msdn.microsoft.com/Forums/en-US/86ccf7a1-5481-4a59-9db2-34ebc760058a/uwphow-to-get-the-first-visible-group-key-in-the-grouped-listview?forum=wpdevelop
        /// </summary>
        /// <param name="element">ListViewItem or element in ListViewItem</param>
        /// <param name="container">ScrollViewer</param>
        /// <param name="isTotallyVisible">If the element is partially visible, then include it. The default value is false</param>
        /// <returns>Get the visibility of the target element</returns>
        private static bool IsVisibileToUser(FrameworkElement element, FrameworkElement container, bool isTotallyVisible = false)
        {
            if (element == null || container == null)
                return false;

            if (element.Visibility != Visibility.Visible)
                return false;

            Rect elementBounds = element.TransformToVisual(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect containerBounds = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);

            if (!isTotallyVisible)
            {
                return (elementBounds.Top < containerBounds.Bottom && elementBounds.Bottom > containerBounds.Top);
            }
            else
            {
                return (elementBounds.Bottom < containerBounds.Bottom && elementBounds.Top > containerBounds.Top);
            }
        }
    }
}
