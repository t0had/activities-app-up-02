using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ActivitiesApp.Pages
{
    public partial class OrganizerActivitiesPage : Page
    {
        private readonly int _organizerId;

        public OrganizerActivitiesPage(int organizerId)
        {
            InitializeComponent();
            _organizerId = organizerId;

            LoadFilters();
            RefreshGrid();
        }

        private void LoadFilters()
        {
            var events = ActivitiesAppEntities.GetInstance().Мероприятия.OrderBy(x => x.Name).ToList();

            // Первый элемент "Все мероприятия" [file:1]
            CbEvent.ItemsSource = new[] { new Мероприятия { ID = -1, Name = "Все мероприятия" } }
                .Concat(events).ToList();
            CbEvent.SelectedIndex = 0;
        }

        private void UiChanged(object sender, EventArgs e) => RefreshGrid();

        private void RefreshGrid()
        {
            var ctx = ActivitiesAppEntities.GetInstance();
            if (ctx == null) throw new Exception("DbContext GetInstance() вернул null");
            if (DgActivities == null) return;

            var q = ctx.Активности
                .Include(a => a.Мероприятия)
                .Include(a => a.Пользователи)
                .AsQueryable();

            if (CbEvent?.SelectedItem is Мероприятия ev && ev.ID != -1)
                q = q.Where(a => a.EventID == ev.ID);

            var text = (TbSearch?.Text ?? "").Trim().ToLower();
            if (!string.IsNullOrEmpty(text))
            {
                q = q.Where(a =>
                    (a.Name ?? "").ToLower().Contains(text)
                    || (a.Мероприятия != null && (a.Мероприятия.Name ?? "").ToLower().Contains(text)));
            }

            var sort = (CbSort?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            if (sort.Contains("↑"))
                q = q.OrderBy(a => a.StartTime);
            else if (sort.Contains("↓"))
                q = q.OrderByDescending(a => a.StartTime);

            DgActivities.ItemsSource = q.ToList();
        }


        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigationServiceEx.GoBack();

        private void BtnLogout_Click(object sender, RoutedEventArgs e) =>
            NavigationServiceEx.ResetTo(new LoginPage());

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ActivitiesApp.Windows.ActivityEditWindow.OpenSingleton(null, RefreshGrid);
        }

        private void DgActivities_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DgActivities.SelectedItem is Активности act)
                ActivitiesApp.Windows.ActivityEditWindow.OpenSingleton(act.ID, RefreshGrid);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(DgActivities.SelectedItem is Активности act))
            {
                MessageBox.Show("Выберите активность для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Запрет: если назначено жюри, удалить нельзя [file:1]
            if (act.Пользователи != null && act.Пользователи.Any())
            {
                MessageBox.Show("Эту активность нельзя удалить: для неё уже назначено жюри.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Удалить выбранную активность? Операция необратима.",
                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                // важно: удалить из контекста актуальную сущность
                var dbAct = ActivitiesAppEntities.GetInstance().Активности.First(x => x.ID == act.ID);
                ActivitiesAppEntities.GetInstance().Активности.Remove(dbAct);
                ActivitiesAppEntities.GetInstance().SaveChanges();
                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить активность.\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
