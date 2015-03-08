#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Utils\RelayCommand.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Yodii.Lab
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
