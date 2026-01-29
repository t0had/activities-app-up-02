using System;
using System.Linq;
using System.Windows;

namespace ActivitiesApp.Windows
{
    public partial class EventEditWindow : Window
    {
        private static EventEditWindow _opened;

        private readonly int? _eventId;
        private readonly Action _onSaved;

        private Мероприятия _entity;

        public static void OpenSingleton(int? eventId, Action onSaved)
        {
            if (_opened != null)
            {
                _opened.Activate();
                return;
            }

            _opened = new EventEditWindow(eventId, onSaved);
            _opened.Closed += (_, __) => _opened = null;
            _opened.Show();
        }

        private EventEditWindow(int? eventId, Action onSaved)
        {
            InitializeComponent();
            _eventId = eventId;
            _onSaved = onSaved;

            CbCity.ItemsSource = ActivitiesAppEntities.GetInstance().Города.OrderBy(x => x.Name).ToList();

            LoadEntity();
        }

        private void LoadEntity()
        {
            if (_eventId == null)
            {
                TbTitle.Text = "Добавление мероприятия";
                CbDirection.SelectedIndex = 0;
                return;
            }

            TbTitle.Text = "Редактирование мероприятия";
            BtnAddActivity.Visibility = Visibility.Visible;

            _entity = ActivitiesAppEntities.GetInstance().Мероприятия.First(x => x.ID == _eventId.Value);

            TbName.Text = _entity.Name;
            TbDesc.Text = _entity.Name; // временно: описание = название [file:1]
            CbCity.SelectedItem = ((System.Collections.IEnumerable)CbCity.ItemsSource)
                .Cast<Города>().FirstOrDefault(c => c.ID == _entity.CityID);

            // Date в БД строка — если парсится, подставим, иначе оставим пустым
            if (DateTime.TryParse(_entity.Date, out var dt))
            {
                DpStart.SelectedDate = dt;
                DpEnd.SelectedDate = dt;
            }

            CbDirection.SelectedIndex = 0; // направления в БД нет
        }

        private void TbName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // чтобы соответствовать “описание=название” при отсутствии поля в БД [file:1]
            if (string.IsNullOrWhiteSpace(TbDesc.Text))
                TbDesc.Text = TbName.Text;
        }

        private void BtnAddActivity_Click(object sender, RoutedEventArgs e)
        {
            if (_eventId == null) return;

            // Открываем форму активности с предвыбранным мероприятием — сделаем улучшение позже:
            // сейчас просто открываем ActivityEditWindow, а EventID пользователь выберет вручную.
            ActivityEditWindow.OpenSingleton(null, _onSaved);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbName.Text))
            {
                MessageBox.Show("Название мероприятия обязательно.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!(CbCity.SelectedItem is Города city))
            {
                MessageBox.Show("Выберите город проведения.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var start = DpStart.SelectedDate;
            var end = DpEnd.SelectedDate;

            if (start.HasValue && end.HasValue && end.Value.Date < start.Value.Date)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (_eventId == null)
                {
                    var ev = new Мероприятия
                    {
                        Name = TbName.Text.Trim(),
                        CityID = city.ID,
                        Date = start?.ToShortDateString() ?? "",   // строка, как в модели
                        NumberOfDays = start.HasValue && end.HasValue
                            ? Math.Max(1, (end.Value.Date - start.Value.Date).Days + 1)
                            : 1
                    };

                    ActivitiesAppEntities.GetInstance().Мероприятия.Add(ev);
                }
                else
                {
                    var ev = ActivitiesAppEntities.GetInstance().Мероприятия.First(x => x.ID == _eventId.Value);
                    ev.Name = TbName.Text.Trim();
                    ev.CityID = city.ID;
                    ev.Date = start?.ToShortDateString() ?? ev.Date;
                    ev.NumberOfDays = start.HasValue && end.HasValue
                        ? Math.Max(1, (end.Value.Date - start.Value.Date).Days + 1)
                        : ev.NumberOfDays;
                }

                ActivitiesAppEntities.GetInstance().SaveChanges();
                _onSaved?.Invoke();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить мероприятие.\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
