using Journey.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Journey.UI.ViewModels
{
    public class TaskViewModel : ViewModelBase
    {
        private readonly ProjectTask _task;
        private readonly Action _onChanged;

        public TaskViewModel(ProjectTask task, Action onChanged)
        {
            _task = task;
            _onChanged = onChanged;
        }

        public string Title => _task.Title;

        public bool IsCompleted
        {
            get => _task.IsCompleted;
            set
            {
                if (_task.IsCompleted != value)
                {
                    _task.IsCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                    _onChanged?.Invoke(); // Уведомляем MainWindowViewModel
                }
            }
        }
    }
}
