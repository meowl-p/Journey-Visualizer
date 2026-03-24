using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;

namespace Journey.UI.ViewModels
{
    public class PathSegmentViewModel
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public bool IsCompleted { get; set; }
        public string Color => IsCompleted ? "Green" : "LightGray";

        // Новые свойства, которые поймет XAML
        public Point StartPoint => new Point(X1, Y1);
        public Point EndPoint => new Point(X2, Y2);
    }
}
