using Avalonia.Controls;
using Journey.UI.ViewModels;

namespace Journey.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Связываем наше окно с его "мозгами"
            DataContext = new MainWindowViewModel();
        }
    }
}