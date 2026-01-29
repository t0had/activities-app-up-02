using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ActivitiesApp.Windows;

namespace ActivitiesApp.Pages
{
    public partial class OrganizerEventsPage : Page
    {
        private readonly int _organizerId;

        public OrganizerEventsPage(int organizerId)
        {
            InitializeComponent();
            _organizerId = organizerId;
            RefreshList();
        }

        private void UiChanged(object sender, EventArgs e) => RefreshList();

        private void RefreshList()
        {
            var q = ActivitiesAppEntities.GetInstance().Мероприятия
                .Include(x => x.Города)
                .Include(x => x.Активности)
                .AsQueryable();

            var text = (TbSearch.Text ?? "").Trim().ToLower();
            if (!string.IsNullOrEmpty(text))
                q = q.Where(x => (x.Name ?? "").ToLower().Contains(text));

            var date = (TbDate.Text ?? "").Trim().ToLower();
            if (!string.IsNullOrEmpty(date))
                q = q.Where(x => (x.Date ?? "").ToLower().Contains(date));

            LvEvents.ItemsSource = q.OrderBy(x => x.Name).ToList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            EventEditWindow.OpenSingleton(null, RefreshList);
        }

        private void LvEvents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LvEvents.SelectedItem is Мероприятия ev)
                EventEditWindow.OpenSingleton(ev.ID, RefreshList);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(LvEvents.SelectedItem is Мероприятия ev))
            {
                MessageBox.Show("Выберите мероприятие для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Запрет: если есть активности — удалить нельзя [file:1]
            if (ev.Активности != null && ev.Активности.Any())
            {
                MessageBox.Show("Нельзя удалить мероприятие: в нём уже есть активности.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Удалить мероприятие? Операция необратима.",
                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var dbEv = ActivitiesAppEntities.GetInstance().Мероприятия.First(x => x.ID == ev.ID);
                ActivitiesAppEntities.GetInstance().Мероприятия.Remove(dbEv);
                ActivitiesAppEntities.GetInstance().SaveChanges();
                RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить мероприятие.\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationServiceEx.GoBack();

        private void BtnLogout_Click(object sender, RoutedEventArgs e) =>
            NavigationServiceEx.ResetTo(new LoginPage());
    }
}
