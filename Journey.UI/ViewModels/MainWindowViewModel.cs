using Journey.Core.Logic;
using Journey.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows.Input;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;



namespace Journey.UI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Project _currentProject;
        public string ProjectName => _currentProject.Name;
        public string ProjectGoal => _currentProject.Goal;
        public string Motivation => _currentProject.Motivation;
        public string FailureCost => _currentProject.FailureCost;
        public string TargetDateDisplay => _currentProject.TargetDate.ToShortDateString();

        // Свойство для текста прогресса (тот самый геттер с расчетом)
        public ICommand CompleteNextMilestoneCommand { get; }
        public ICommand SelectMilestoneCommand { get; }
        public ICommand ResetCommand { get; }

        private double _progressDelta;
        public string ProgressDeltaDisplay => _progressDelta > 0
            ? $"С прошлого визита вы продвинулись на {Math.Round(_progressDelta * 100, 1)}%!"
            : "Рады видеть вас снова! Время сделать новый шаг в вашем путешествии.";

        private bool _showWelcomePanel;
        public bool ShowWelcomePanel
        {
            get => _showWelcomePanel;
            set => RaiseAndSetIfChanged(ref _showWelcomePanel, value);
        }

        public ICommand CloseWelcomeCommand { get; }

        public MainWindowViewModel()
        {
            LoadOrCreateProject(); 

            //Команда для кнопки "выполнить все задачи в первой встречной вехе"
            CompleteNextMilestoneCommand = MiniCommand.Create((parameter) =>
            {
                // Находим первую невыполненную веху
                var nextMilestone = _currentProject.Milestones.FirstOrDefault(m => !m.IsCompleted);
                if (nextMilestone != null)
                {
                    // Выполняем все задачи в ней (для MVP)
                    foreach (var task in nextMilestone.Tasks) task.IsCompleted = true;

                    // Важно: пересчитываем визуал и уведомляем UI
                    UpdateVisuals();
                    SaveToDisk();
                }
            });

            SelectMilestoneCommand = MiniCommand.Create((parameter) =>
            {
                if (parameter is MilestoneViewModel mvm)
                {
                    SelectedMilestone = mvm;
                }
            });

            ResetCommand = MiniCommand.Create((parameter) =>
            {
                if (File.Exists(_saveFilePath)) File.Delete(_saveFilePath);
                LoadOrCreateProject();
                UpdateVisuals();
            });

            CloseWelcomeCommand = MiniCommand.Create((parameter) =>
            {
                // Как только пользователь нажал "Ок" или "В путь"
                // Мы запоминаем текущий прогресс как новую точку отсчета
                _currentProject.LastProgressValue = ProgressCalculator.CalculateTotalProgress(_currentProject);
                _currentProject.LastVisitDate = DateTime.Now;

                SaveToDisk(); // Сохраняем обновленные "точки отсчета" в файл
                ShowWelcomePanel = false; // Скрываем панель в UI
            });

            OpenEditCommand = MiniCommand.Create(parameter => OpenEdit());
            CancelEditCommand = MiniCommand.Create(parameter => CancelEdit());
            SaveProjectCommand = MiniCommand.Create(parameter => SaveProject());

            GenerateVisualPath();
        }

        //Дефолтный проект
        private Project CreateMockProject()
        {
            var project = new Project
            {
                Name = "Journey Visualizer MVP",
                TargetDate = new DateTime(2026, 03, 30),
                Goal = @"Разработать MVP визуализатора прогресса достижения цели и протестировать гипотезу, 
что прогресс и напоминание зачем я это делаю поддерживает во мне интерес продолжать идти вперед.",

                // Мощная и емкая мотивация
                Motivation = @"• АКТИВ СВОБОДЫ: Я строю портфолио, которое позволит мне диктовать условия рынку, а не подстраиваться под него.
• ПРЫЖОК В ДОХОДЕ: Этот проект — фундамент для кратного роста прибыли, недоступного в обычном найме.
• ПОЛНЫЙ КОНТРОЛЬ: Я сам решаю, когда работать и когда отдыхать. Каждая закрытая задача приближает меня к жизни по моим правилам.",

                // Честный и жесткий Failure Cost
                FailureCost = @"• УПУЩЕННОЕ ВРЕМЯ: Если я сдамся сейчас, годы продуктивности будут окончательно потеряны. Второго шанса на такой рывок может не быть.
• ГРУЗ ЛЖИ: Не сделав этот шаг, я не смогу дать семье ту безопасность и свободу, которых они достойны. Мне придется жить с осознанием того, что я не выложился на 100%. Мне придется смотреть с этим чувством своим детям в глаза и врать им, что я сделал все что мог. Я не могу так.
• СТАТУС-КВО: Жизнь останется 'неплохой', но я буду всегда знать, что выбрал комфорт вместо реализации своего потенциала. Это принесет только страдание.",
            };

            // 1. Развернуть Avalonia UI
            project.Milestones.Add(new Milestone
            {
                Title = "Развернуть Avalonia UI",
                Tasks = new()
        {
            new ProjectTask { Title = "Инициализация MVVM структуры", IsCompleted = true },
            new ProjectTask { Title = "Настройка базовых стилей окна", IsCompleted = true },
            new ProjectTask { Title = "Подключение Behaviors для обработки событий", IsCompleted = true }
        }
            });

            // 2. Экран прогресса
            project.Milestones.Add(new Milestone
            {
                Title = "Экран прогресса",
                Tasks = new()
        {
            new ProjectTask { Title = "Реализация логики ProgressCalculator", IsCompleted = true },
            new ProjectTask { Title = "Верстка Dashboard в верхней части экрана", IsCompleted = true },
            new ProjectTask { Title = "Создание системы Observable-коллекций", IsCompleted = true }
        }
            });

            // 3. Отображение пути
            project.Milestones.Add(new Milestone
            {
                Title = "Отображение пути",
                Tasks = new()
        {
            new ProjectTask { Title = "Алгоритм генерации координат на Canvas", IsCompleted = true },
            new ProjectTask { Title = "Отрисовка динамических линий (Path Segments)", IsCompleted = true },
            new ProjectTask { Title = "Реализация выделения вех (Selection Command)", IsCompleted = true }
        }
            });

            // 4. Экран Why this matters
            project.Milestones.Add(new Milestone
            {
                Title = "Экран Why this matters",
                Tasks = new()
        {
            new ProjectTask { Title = "Добавление полей смысла в модель Project", IsCompleted = true },
            new ProjectTask { Title = "Реализация TabControl для разделения контекстов", IsCompleted = true },
            new ProjectTask { Title = "Верстка смысловых блоков с переносом текста", IsCompleted = true }
        }
            });

            // 5. Экран Progress since last visit
            project.Milestones.Add(new Milestone
            {
                Title = "Экран Progress since last visit",
                Tasks = new()
        {
            new ProjectTask { Title = "Создание системы сохранения даты входа", IsCompleted = false },
            new ProjectTask { Title = "Расчет дельты прогресса между сессиями", IsCompleted = false },
            new ProjectTask { Title = "UI-панель приветствия с итогами отсутствия", IsCompleted = false }
        }
            });

            // 6. Полировка
            project.Milestones.Add(new Milestone
            {
                Title = "Полировка",
                Tasks = new()
        {
            new ProjectTask { Title = "Рефакторинг кода и удаление артефактов", IsCompleted = false },
            new ProjectTask { Title = "Настройка финальной цветовой палитры 'Journey'", IsCompleted = false },
            new ProjectTask { Title = "Добавление плавных анимаций переходов", IsCompleted = false }
        }
            });

            // 7. Публикация
            project.Milestones.Add(new Milestone
            {
                Title = "Публикация",
                Tasks = new()
        {
            new ProjectTask { Title = "Github репозиторий", IsCompleted = false },
            new ProjectTask { Title = "Информативный README", IsCompleted = false },
            new ProjectTask { Title = "Пост в LinkedIn", IsCompleted = false },
            new ProjectTask { Title = "Анонс в Telegram", IsCompleted = false },
            new ProjectTask { Title = "Публикация в ВК", IsCompleted = false },
            new ProjectTask { Title = "Личная презентация близким", IsCompleted = false }
        }
            });

            return project;
        }

        /* VISUAL */

        // Коллекция для привязки к UI (ObservableCollection сама уведомляет UI о добавлении элементов)
        public ObservableCollection<MilestoneViewModel> VisualMilestones { get; } = new();
        public ObservableCollection<PathSegmentViewModel> VisualPaths { get; } = new();
        public string ProgressDisplay => $"Прогресс: {Math.Round(ProgressCalculator.CalculateTotalProgress(_currentProject) * 100, 1)}%"; 
        public double CurrentProgressValue => ProgressCalculator.CalculateTotalProgress(_currentProject) * 100;
        private void UpdateVisuals()
        {
            GenerateVisualPath();               // Перерисовывает точки и линии на карте
            OnPropertyChanged(nameof(ProgressDisplay)); // Обновляет текст "Прогресс: X%" в шапке
            OnPropertyChanged(nameof(CurrentProgressValue));
        }

        private void GenerateVisualPath()
        {
            VisualMilestones.Clear();
            VisualPaths.Clear();

            var milestones = _currentProject.Milestones;
            int count = milestones.Count;
            double width = 700; // Допустимая ширина пути
            double margin = 50;  // Отступ от краев
            double y = 60;      // Фиксированная высота пути на Canvas

            for (int i = 0; i < count; i++)
            {
                var m = milestones[i];
                // Наша формула в коде:
                double x = margin + i * ((width - 2 * margin) / (count - 1));

                VisualMilestones.Add(new MilestoneViewModel
                {
                    Title = m.Title,
                    X = x,
                    Y = y,
                    IsCompleted = m.IsCompleted
                });
            }

            // После того как вехи созданы, соединяем их линиями
            for (int i = 0; i < VisualMilestones.Count - 1; i++)
            {
                var start = VisualMilestones[i];
                var end = VisualMilestones[i + 1];

                VisualPaths.Add(new PathSegmentViewModel
                {
                    X1 = start.X,
                    Y1 = start.Y,
                    X2 = end.X,
                    Y2 = end.Y,
                    // Линия "завершена", если точка, к которой она ведет, завершена
                    IsCompleted = end.IsCompleted
                });
            }
        }

        private MilestoneViewModel? _selectedMilestone;
        public MilestoneViewModel? SelectedMilestone
        {
            get => _selectedMilestone;
            set
            {
                if (_selectedMilestone != null) _selectedMilestone.IsSelected = false;

                if (RaiseAndSetIfChanged(ref _selectedMilestone, value))
                {
                    if (_selectedMilestone != null) _selectedMilestone.IsSelected = true;
                    UpdateTasksList();
                }
            }
        }

        /* TASKS */

        public ObservableCollection<TaskViewModel> CurrentTasks { get; } = new();

        private void UpdateTasksList()
        {
            CurrentTasks.Clear();
            if (_selectedMilestone == null) return;

            var milestone = _currentProject.Milestones.FirstOrDefault(m => m.Title == _selectedMilestone.Title);
            if (milestone != null)
            {
                foreach (var task in milestone.Tasks)
                {
                    // Создаем обертку и передаем ей действие: "Обнови визуал и сохранись"
                    var taskVM = new TaskViewModel(task, () =>
                    {
                        UpdateVisuals();
                        SaveToDisk();
                    });
                    CurrentTasks.Add(taskVM);
                }
            }
        }

        /* СОХРАНЕНИЕ И ЗАГРУЗКА ДЛЯ ОТОБРАЖЕНИЯ ПРОГРЕССА ВО ВРЕМЕНИ */

        private readonly string _saveFilePath = "project_data.json";

        private void SaveToDisk()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_currentProject, options);
                File.WriteAllText(_saveFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void LoadOrCreateProject()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_saveFilePath);
                    var loaded = JsonSerializer.Deserialize<Project>(json);
                    if (loaded != null)
                    {
                        _currentProject = loaded;

                        // Считаем текущий реальный прогресс
                        double currentActualProgress = ProgressCalculator.CalculateTotalProgress(_currentProject);

                        // Вычисляем дельту относительно того, что было сохранено в LastProgressValue
                        _progressDelta = currentActualProgress - _currentProject.LastProgressValue;

                        // Показываем панель приветствия
                        ShowWelcomePanel = true;

                        return;
                    }
                }
                catch { }
            }

            _currentProject = CreateMockProject();
            _progressDelta = 0;
            SaveToDisk();
        }

        /* EDIT FORM */
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => this.RaiseAndSetIfChanged(ref _isEditing, value);
        }

        // Буферные свойства для формы редактирования
        private string _editProjectName;
        public string EditProjectName
        {
            get => _editProjectName;
            set => this.RaiseAndSetIfChanged(ref _editProjectName, value);
        }

        private string _editMotivation;
        public string EditMotivation
        {
            get => _editMotivation;
            set => this.RaiseAndSetIfChanged(ref _editMotivation, value);
        }

        private string _editFailureCost;
        public string EditFailureCost
        {
            get => _editFailureCost;
            set => this.RaiseAndSetIfChanged(ref _editFailureCost, value);
        }

        private string _editTargetDate;
        public string EditTargetDate
        {
            get => _editTargetDate;
            set => this.RaiseAndSetIfChanged(ref _editTargetDate, value);
        }

        private string _editGoal;
        public string EditGoal
        {
            get => _editGoal;
            set => RaiseAndSetIfChanged(ref _editGoal, value);
        }

        public ICommand OpenEditCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SaveProjectCommand { get; }

        private void OpenEdit()
        {
            // Копируем текущие данные в буферы перед открытием окна
            EditProjectName = ProjectName;
            EditGoal = ProjectGoal;
            EditMotivation = Motivation;
            EditFailureCost = FailureCost;
            EditTargetDate = TargetDateDisplay; // Или отдельное поле даты, если оно 

            ShowWelcomePanel = false;
            IsEditing = true;
        }

        private void CancelEdit()
        {
            IsEditing = false;
        }

        /// <summary>
        /// Логика команды сохранения: переносим данные из буферов в модель
        /// </summary>
        private void SaveProject()
        {
            // 1. Обновляем данные в самой модели
            _currentProject.Name = EditProjectName;
            _currentProject.Goal = EditGoal;
            _currentProject.Motivation = EditMotivation;
            _currentProject.FailureCost = EditFailureCost;

            if (DateTime.TryParse(EditTargetDate, out var newDate))
            {
                _currentProject.TargetDate = newDate;
            }

            // 2. Уведомляем интерфейс, что свойства-обертки изменились
            // Это важно, так как у них нет своих setter-ов
            OnPropertyChanged(nameof(ProjectName));
            OnPropertyChanged(nameof(ProjectGoal));
            OnPropertyChanged(nameof(Motivation));
            OnPropertyChanged(nameof(FailureCost));
            OnPropertyChanged(nameof(TargetDateDisplay));

            // 3. Закрываем форму и сохраняем на диск
            IsEditing = false;
            SaveToDisk();
        }
    }
}
