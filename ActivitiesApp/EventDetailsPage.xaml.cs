using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ActivitiesApp.Pages
{
    public partial class EventDetailsPage : Page
    {
        private readonly int _eventId;

        public EventDetailsPage(int eventId)
        {
            InitializeComponent();
            _eventId = eventId;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var ev = ActivitiesAppEntities.GetInstance().Мероприятия
                    .Include(x => x.Города)
                    .Include(x => x.Активности)
                    .First(x => x.ID == _eventId);

                TbName.Text = ev.Name;
                TbDate.Text = "Дата: " + (ev.Date ?? "—");
                TbCity.Text = "Город: " + (ev.Города?.Name ?? "—");

                // Описание в БД нет — используем название [file:1]
                TbDesc.Text = ev.Name;

                DgActivities.ItemsSource = ev.Активности
                    .OrderBy(a => a.Day)
                    .ThenBy(a => a.StartTime)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть мероприятие.\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationServiceEx.GoBack();

        private void BtnLogout_Click(object sender, RoutedEventArgs e) =>
            NavigationServiceEx.ResetTo(new LoginPage());
    }
}
