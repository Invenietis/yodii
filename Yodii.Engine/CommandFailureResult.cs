using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class CommandFailureResult : ICommandFailureResult
    {
        public CommandFailureResult( InternalYodiiCommand cmd )
            : this( cmd.Command.PluginFullName ?? cmd.Command.ServiceFullName, true, false )
        {
        }

        public CommandFailureResult( string name, bool unapplicableCommand, bool unexistingItem )
        {
            PluginOrServiceFullName = name;
            UnapplicableCommand = unapplicableCommand;
            UnexistingItem = unexistingItem;
        }

        public string PluginOrServiceFullName { get; private set; }

        public bool UnapplicableCommand { get; private set; }

        public bool UnexistingItem { get; private set; }
    }
}
