using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ActivitiesApp.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            NavigationServiceEx.Navigate(new GuestEventsPage());
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            TbError.Text = "";

            if (!int.TryParse(TbId.Text?.Trim(), out var id))
            {
                MessageBox.Show("Введите числовой ID пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var pass = PbPassword.Password ?? "";

            try
            {
                var user = ActivitiesAppEntities.GetInstance().Пользователи.FirstOrDefault(u => u.ID == id && u.Password == pass);
                if (user == null)
                {
                    MessageBox.Show("Неверный ID или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                switch (user.RoleID)
                {
                    case 2: // Организаторы
                        NavigationServiceEx.ResetTo(new OrganizerHomePage(user));
                        break;
                    case 0: // Жюри
                        NavigationServiceEx.Navigate(new GuestEventsPage());
                        break;
                    case 1: // Модераторы
                        NavigationServiceEx.Navigate(new GuestEventsPage());
                        break;
                    case 3: // Участники
                        NavigationServiceEx.Navigate(new GuestEventsPage());
                        break;
                    default:
                        MessageBox.Show("Неизвестная роль пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось выполнить вход.\n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
