using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Pretwa.Gui.Annotations;

namespace Pretwa.Gui.Common
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            #if DEBUG
            LogPropertyChanged(propertyName);
            #endif
        }

        private void LogPropertyChanged(string propertyName)
        {
            var value = this.GetType().GetProperty(propertyName).GetValue(this);
            Trace.WriteLine($"Property \"{propertyName}\" changed value to \"{value}\"");
        }
    }

    public class CommandBase : ICommand
    {
        private readonly Action _Action;

        public CommandBase(Action action)
        { 
            _Action = action;
        }

        public void Execute(object parameter)
        {
            _Action();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
