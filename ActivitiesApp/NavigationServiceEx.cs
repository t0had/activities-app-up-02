using System.Windows.Controls;

namespace ActivitiesApp
{
    public static class NavigationServiceEx
    {
        public static Frame Frame { get; private set; }

        public static void Init(Frame frame) => Frame = frame;

        public static void Navigate(object page) => Frame?.Navigate(page);

        public static bool CanGoBack => Frame?.CanGoBack == true;

        public static void GoBack()
        {
            if (CanGoBack) Frame.GoBack();
        }

        public static void ResetTo(object page)
        {
            if (Frame == null) return;

            Frame.Navigate(page);
            while (Frame.CanGoBack) Frame.RemoveBackEntry();
        }
    }
}
