using System;
using System.Windows.Input;

namespace SessionsControllerServer.Helpers
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private readonly Action<object> _methodToExecute;
        private readonly Func<bool> _canExecuteEvaluator;

        public RelayCommand(Action<object> methodToExecute, Func<bool> canExecuteEvaluator)
        {
            _methodToExecute = methodToExecute;
            _canExecuteEvaluator = canExecuteEvaluator;
        }

        public RelayCommand(Action<object> methodToExecute)
            : this(methodToExecute, null)
        {
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecuteEvaluator == null)
            {
                return true;
            }
            var result = _canExecuteEvaluator.Invoke();
            return result;
        }

        public void Execute(object parameter)
        {
            _methodToExecute.Invoke(parameter);
        }
    }
}
