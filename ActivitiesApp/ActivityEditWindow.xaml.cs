using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace ActivitiesApp.Windows
{
    public partial class ActivityEditWindow : Window
    {
        private static ActivityEditWindow _opened;

        private readonly int? _activityId;
        private readonly Action _onSaved;

        private Активности _entity;

        private readonly int? _preselectedEventId;

        public static void OpenSingleton(int? activityId, int? preselectedEventId, Action onSaved)
        {
            if (_opened != null)
            {
                _opened.Activate();
                return;
            }

            _opened = new ActivityEditWindow(activityId, preselectedEventId, onSaved);
            _opened.Closed += (_, __) => _opened = null;
            _opened.Show();
        }

        public static void OpenSingleton(int? activityId, Action onSaved)
        {
            OpenSingleton(activityId, null, onSaved);
        }

        public static void OpenSingleton(int? activityId, int? preselectedEventId)
        {
            OpenSingleton(activityId, preselectedEventId, null);
        }


        private ActivityEditWindow(int? activityId, int? preselectedEventId, Action onSaved)
        {
            InitializeComponent();
            _activityId = activityId;
            _preselectedEventId = preselectedEventId;
            _onSaved = onSaved;

            LoadEvents();
            LoadEntity();
        }

        private void LoadEvents()
        {
            CbEvent.ItemsSource = ActivitiesAppEntities.GetInstance().Мероприятия.OrderBy(x => x.Name).ToList();
        }

        private void LoadEntity()
        {
            if (_activityId == null)
            {
                TbTitle.Text = "Добавление активности";
                TbId.Visibility = Visibility.Collapsed;
                _entity = new Активности();

                if (_preselectedEventId.HasValue)
                {
                    var eve = ((System.Collections.Generic.List<Мероприятия>)CbEvent.ItemsSource)
                        .FirstOrDefault(x => x.ID == _preselectedEventId.Value);
                    if (eve != null) CbEvent.SelectedItem = eve;
                }

                RebuildStartTimes();
                return;
            }


            TbTitle.Text = "Редактирование активности";
            _entity = ActivitiesAppEntities.GetInstance().Активности
                .Include(a => a.Мероприятия)
                .First(a => a.ID == _activityId.Value);

            TbId.Text = _entity.ID.ToString();
            TbName.Text = _entity.Name;

            // выбрать мероприятие
            var ev = ((List<Мероприятия>)CbEvent.ItemsSource).FirstOrDefault(x => x.ID == _entity.EventID);
            CbEvent.SelectedItem = ev;

            // выбрать старт
            CbStartTime.SelectedItem = _entity.StartTime;
        }

        private void CbEvent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RebuildStartTimes();
        }

        private void RebuildStartTimes()
        {
            if (!(CbEvent.SelectedItem is Мероприятия ev))
            {
                CbStartTime.ItemsSource = null;
                return;
            }

            // Упрощённо: формируем слоты в пределах дня 09:00-18:00 шагом 105 минут (90 + 15) [file:1]
            // Если у тебя есть реальные границы дня/мероприятия — подставим позже.
            var dayStart = new TimeSpan(9, 0, 0);
            var dayEnd = new TimeSpan(18, 0, 0);

            var step = TimeSpan.FromMinutes(105); // 90 + 15 [file:1]
            var duration = TimeSpan.FromMinutes(90);

            var existing = ActivitiesAppEntities.GetInstance().Активности
                .Where(a => a.EventID == ev.ID && (_activityId == null || a.ID != _activityId.Value))
                .Select(a => a.StartTime)
                .ToList();

            // Занятые интервалы: [start, start+90] + обязательная “буферная пауза” 15 минут после.
            bool IsSlotFree(TimeSpan candidate)
            {
                foreach (var st in existing)
                {
                    var busyStart = st;
                    var busyEnd = st + duration + TimeSpan.FromMinutes(15); // перерыв после активности [file:1]
                    if (candidate >= busyStart && candidate < busyEnd) return false;

                    // Также не даём стартовать так, чтобы активность “врезалась” в уже стоящую:
                    var candidateEnd = candidate + duration;
                    if (candidateEnd > busyStart && candidate < busyStart) return false;
                }
                return true;
            }

            var slots = new List<TimeSpan>();
            for (var t = dayStart; t + duration <= dayEnd; t += step)
                if (IsSlotFree(t)) slots.Add(t);

            CbStartTime.ItemsSource = slots;

            // если редактирование и текущее время не попало в список (из-за логики), добавим его
            if (_activityId != null && !slots.Contains(_entity.StartTime))
            {
                slots.Insert(0, _entity.StartTime);
                CbStartTime.ItemsSource = null;
                CbStartTime.ItemsSource = slots;
            }

            if (CbStartTime.SelectedItem == null && slots.Count > 0)
                CbStartTime.SelectedIndex = 0;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbName.Text))
            {
                MessageBox.Show("Название активности обязательно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!(CbEvent.SelectedItem is Мероприятия ev))
            {
                MessageBox.Show("Выберите мероприятие.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!(CbStartTime.SelectedItem is TimeSpan st))
            {
                MessageBox.Show("Выберите время начала из списка доступных интервалов.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (_activityId == null)
                {
                    var newEntity = new Активности
                    {
                        Name = TbName.Text.Trim(),
                        EventID = ev.ID,
                        StartTime = st,
                        Day = 1,        // если у тебя реально несколько дней — подстроим
                        Moderator = 0   // модератор один, но поле int — подстроим под твою схему
                    };

                    ActivitiesAppEntities.GetInstance().Активности.Add(newEntity);
                }
                else
                {
                    var dbEntity = ActivitiesAppEntities.GetInstance().Активности.First(a => a.ID == _activityId.Value);
                    dbEntity.Name = TbName.Text.Trim();
                    dbEntity.EventID = ev.ID;
                    dbEntity.StartTime = st;
                }

                ActivitiesAppEntities.GetInstance().SaveChanges();
                _onSaved?.Invoke();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить активность.\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
