using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Journey.UI.ViewModels
{
    public class MilestoneViewModel : ViewModelBase
    {
        public required string Title { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsCompleted { get; set; }

        // Цвет точки: зеленый если пройдена, серый если нет
        public string Color => IsCompleted ? "Green" : "Gray";

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => RaiseAndSetIfChanged(ref _isSelected, value);
        }

        // Толщина обводки теперь зависит от простого bool
        public double StrokeThickness => IsSelected ? 3 : 0;
    }
}
