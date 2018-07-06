using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScoutChess.Common
{
    internal class DelegatingCommand : ICommand
    {
        private Action<object> _commandDelegate = null;

        internal DelegatingCommand(Action<object> commandDelegate)
        {
            _commandDelegate = commandDelegate;
        }
        internal bool CanExecute(object parameter)
        {
            return _commandDelegate != null;
        }

        private EventHandler _canExecuteChangedHandler;
        internal event EventHandler CanExecuteChanged
        {
            add { _canExecuteChangedHandler += value; }
            remove { _canExecuteChangedHandler -= value; }
        }

        private void OnCanExcecuteChanged()
        {
            var handler = _canExecuteChangedHandler;
            if (handler != null)
            {
                handler.Invoke(this, new EventArgs());
            }
        }

        internal void Execute(object parameter)
        {
           if (_commandDelegate != null)
           {
               _commandDelegate.Invoke(parameter);
           }
        }
    }
}
