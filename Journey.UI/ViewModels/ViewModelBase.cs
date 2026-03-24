using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Journey.UI.ViewModels
{
    //Default by Avalonia, but I want to use my MVVM instead
    /* public abstract class ViewModelBase : ObservableObject
     {
     }*/

    public class RelayCommand(Action<object?> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute(parameter);
    }

    public static class MiniCommand
    {
        public static ICommand Create(Action<object?> action) => new RelayCommand(action);
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        // Событие, на которое подписывается Avalonia, чтобы следить за изменениями
        public event PropertyChangedEventHandler? PropertyChanged;

        // Вспомогательный метод, который уведомляет UI об изменениях
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Метод-помощник: проверяет, изменилось ли значение, и если да — обновляет его и уведомляет UI
        protected bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
