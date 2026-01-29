using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ActivitiesApp.Pages
{
    public partial class OrganizerHomePage : Page
    {
        private readonly Пользователи _user;

        public OrganizerHomePage(Пользователи user)
        {
            InitializeComponent();
            _user = user;

            TbGreeting.Text = BuildGreeting(_user.FIO);
            LoadPhoto(_user.PhotoPath);
        }

        private static string BuildGreeting(string fio)
        {
            var parts = (fio ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var name = parts.Length > 1 ? parts[1] : (parts.Length > 0 ? parts[0] : "пользователь");
            var patronymic = parts.Length > 2 ? parts[2] : "";
            var now = DateTime.Now.Hour;

            var tod = now < 12 ? "утро" : (now < 18 ? "день" : "вечер");
            return $"Доброе {tod}, {name} {patronymic}!".Trim();
        }

        private void LoadPhoto(string relativePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativePath))
                    return;

                var full = Path.GetFullPath(relativePath);
                if (!File.Exists(full))
                    return;

                ImgPhoto.Source = new BitmapImage(new Uri(full, UriKind.Absolute));
            }
            catch
            {
                // Фото не критично.
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            NavigationServiceEx.ResetTo(new LoginPage());
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            NavigationServiceEx.Navigate(new OrganizerProfilePage(_user.ID)); // сделаем следующим шагом
        }

        private void BtnEvents_Click(object sender, RoutedEventArgs e)
        {
            NavigationServiceEx.Navigate(new OrganizerEventsPage(_user.ID)); // сделаем следующим шагом
        }

        private void BtnActivities_Click(object sender, RoutedEventArgs e)
        {
            NavigationServiceEx.Navigate(new OrganizerActivitiesPage(_user.ID));
        }
    }
}
