using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ActivitiesApp.Pages
{
    public partial class GuestEventsPage : Page
    {
        private List<Мероприятия> _allEvents;

        public GuestEventsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationServiceEx.GoBack(); // вернёмся на LoginPage
        }

        private void LoadData()
        {
            _allEvents = ActivitiesAppEntities
                .GetInstance()
                .Мероприятия
                .ToList();

            LvEvents.ItemsSource = _allEvents;
        }


        private void FilterChanged(object sender, EventArgs e) => ApplyFilter();

        private void ApplyFilter()
        {
            if (_allEvents == null) return;

            var direction = (TbSearchDirection.Text ?? "").Trim().ToLower();
            var date = DpDate.SelectedDate;

            var filtered = _allEvents.AsEnumerable();

            // 🔍 Фильтр по названию
            if (!string.IsNullOrWhiteSpace(direction))
            {
                filtered = filtered.Where(x =>
                    x.Name != null &&
                    x.Name.ToLower().Contains(direction));
            }

            // 📅 Фильтр по дате
            if (date.HasValue)
            {
                filtered = filtered.Where(x =>
                    DateTime.TryParse(x.Date, out var eventDate) &&
                    eventDate.Date == date.Value.Date);
            }

            LvEvents.ItemsSource = filtered.ToList();
        }


        private void LvEvents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LvEvents.SelectedItem is Мероприятия ev)
                NavigationServiceEx.Navigate(new EventDetailsPage(ev.ID));
        }
    }
}
