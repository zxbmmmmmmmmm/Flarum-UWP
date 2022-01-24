using FlarumUWP.Core.Models;

namespace FlarumUWP.Helpers
{
    internal class NavigationInfoHelper
    {
        public static string LastPosition { get; private set; }
        public static Included LastViewedItem { get; private set; }

        public static void SetInfo(Included lastItem, string position)
        {
            LastViewedItem = lastItem;
            LastPosition = position;
            IsHasInfo = true;
        }

        public static bool IsHasInfo { get; private set; }
    }
}
