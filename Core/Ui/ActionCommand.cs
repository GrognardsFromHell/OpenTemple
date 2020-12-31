using System;
using System.Windows.Input;

namespace OpenTemple.Core.Ui
{
    public class ActionCommand : ICommand
    {
        private readonly Action _action;

        public ActionCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _action();

        public event EventHandler? CanExecuteChanged;
    }
}