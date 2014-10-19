using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// Basic relay command.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new relay command, which will execute given action.
        /// </summary>
        /// <param name="execute">Action to execute.</param>
        public RelayCommand( Action<object> execute )
            : this( execute, null )
        {
        }

        /// <summary>
        /// Creates a new relay command, with a canExecute predicate and an action.
        /// </summary>
        /// <param name="execute">Action to execute.</param>
        /// <param name="canExecute">Predicate determining whether the command can be executed.</param>
        public RelayCommand( Action<object> execute, Predicate<object> canExecute )
        {
            if( execute == null )
                throw new ArgumentNullException( "execute" );
            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion Constructors

        #region ICommand Members

        /// <summary>
        /// Whether the command can execute.
        /// </summary>
        /// <param name="parameter">Command parameter</param>
        /// <returns>True if the command can execute.</returns>
        [DebuggerStepThrough]
        public bool CanExecute( object parameter )
        {
            return _canExecute == null ? true : _canExecute( parameter );
        }

        /// <summary>
        /// Raised when CanExecute changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">Command parameter.</param>
        public void Execute( object parameter )
        {
            _execute( parameter );
        }

        #endregion ICommand Members
    }
}
