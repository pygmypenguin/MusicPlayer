using System;
using System.Windows.Input;

namespace MusicPlayer.Infrastructure
{
    public class Command : ICommand
    {
        private Func<object, bool> _canExecuteMethod;
        private Action<object> _action;

        public event EventHandler CanExecuteChanged;

        public Command(Action action)
            : this(new Action<object>(delegate { action(); }), null)
        {
        }

        public Command(Action<object> action)
            : this(action, null)
        {
        }

        public Command(Action<object> action, Func<object, bool> canExecuteMethod)
        {
            if (_canExecuteMethod == null)
            {
                _canExecuteMethod = delegate { return true; };
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteMethod(parameter);
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}