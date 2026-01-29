using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ActivitiesApp.Pages
{
    public partial class OrganizerProfilePage : Page
    {
        private readonly int _userId;
        private Пользователи _user;
        private bool _passVisible;

        public OrganizerProfilePage(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadUser();
        }

        private void LoadUser()
        {
            _user = ActivitiesAppEntities.GetInstance().Пользователи.First(u => u.ID == _userId);

            TbId.Text = _user.ID.ToString();
            TbFio.Text = _user.FIO;
            TbEmail.Text = _user.Email;
            TbPhone.Text = _user.PhoneNumber;

            TbPhotoPath.Text = _user.PhotoPath ?? "";
            LoadPhoto(_user.PhotoPath);
        }

        private void LoadPhoto(string path)
        {
            try
            {
                ImgPhoto.Source = null;
                if (string.IsNullOrWhiteSpace(path)) return;

                var full = Path.GetFullPath(path);
                if (!File.Exists(full)) return;

                ImgPhoto.Source = new BitmapImage(new Uri(full, UriKind.Absolute));
            }
            catch
            {
                // Фото не критично.
            }
        }

        private void BtnTogglePass_Click(object sender, RoutedEventArgs e)
        {
            _passVisible = !_passVisible;

            if (_passVisible)
            {
                TbPass1.Text = PbPass1.Password;
                TbPass1.Visibility = Visibility.Visible;
                PbPass1.Visibility = Visibility.Collapsed;
                (sender as Button).Content = "Скрыть";
            }
            else
            {
                PbPass1.Password = TbPass1.Text;
                PbPass1.Visibility = Visibility.Visible;
                TbPass1.Visibility = Visibility.Collapsed;
                (sender as Button).Content = "Показать";
            }
        }

        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog() != true) return;

            _user.PhotoPath = dlg.FileName;
            TbPhotoPath.Text = _user.PhotoPath;
            LoadPhoto(_user.PhotoPath);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _user.FIO = (TbFio.Text ?? "").Trim();
                _user.Email = (TbEmail.Text ?? "").Trim();
                _user.PhoneNumber = (TbPhone.Text ?? "").Trim();

                var pass1 = _passVisible ? (TbPass1.Text ?? "") : (PbPass1.Password ?? "");
                var pass2 = PbPass2.Password ?? "";

                if (!string.IsNullOrWhiteSpace(pass1) || !string.IsNullOrWhiteSpace(pass2))
                {
                    if (pass1 != pass2)
                    {
                        MessageBox.Show("Пароли не совпадают. Повторите ввод.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    _user.Password = pass1;
                }

                ActivitiesAppEntities.GetInstance().SaveChanges();
                MessageBox.Show("Профиль сохранён.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить профиль.\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationServiceEx.GoBack();

        private void BtnLogout_Click(object sender, RoutedEventArgs e) =>
            NavigationServiceEx.ResetTo(new LoginPage());
    }
}
