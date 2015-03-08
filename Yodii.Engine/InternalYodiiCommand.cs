using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class InternalYodiiCommand
    {
        public readonly YodiiCommand Command;
        public ILiveYodiiItem LiveItem;

        IDynamicItem _serviceOrPluginData;

        public InternalYodiiCommand( ILiveYodiiItem item, bool start, StartDependencyImpact impact, string callerKey )
            : this( new YodiiCommand( start, item.FullName, item.IsPlugin, impact, callerKey ), item )
        {
        }

        public InternalYodiiCommand( YodiiCommand c, ILiveYodiiItem item )
        {
            Command = c;
            LiveItem = item;
        }

        public bool IsNewValidLiveCommand
        {
            get 
            {
                Debug.Assert( LiveItem != null, "New commands are necessarily bound to an existing live item." );
                if( Command.Start )
                {
                    if( !LiveItem.Capability.CanStartWith( Command.Impact ) )
                    {
                        return false;
                    }
                }
                else
                {
                    if( !LiveItem.Capability.CanStop )
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Called by YodiiCommandList when the ConfigurationSolver instance to work 
        /// with is not the same as the previous one or by ConfigurationSolver.DynamicResolution on the new command.
        /// </summary>
        /// <param name="solver">The new solver object.</param>
        /// <returns>The bound plugin or service data. Can be null.</returns>
        internal IDynamicItem BindToConfigSolverData( ConfigurationSolver solver )
        {
            if( solver == null ) return _serviceOrPluginData = null;
            if( Command.ServiceFullName != null )
            {
                return _serviceOrPluginData = solver.FindService( Command.ServiceFullName );
            }
            return _serviceOrPluginData = solver.FindPlugin( Command.PluginFullName );
        }

        public bool IsMaskedBy( InternalYodiiCommand newOne )
        {
           Debug.Assert( newOne != null );
            var c = Command;
            var newCommand = newOne.Command;
            return newCommand.PluginFullName == Command.PluginFullName && newCommand.ServiceFullName == Command.ServiceFullName;
        }

        public bool ApplyCommand( int idxCommand )
        {
            if( _serviceOrPluginData == null ) return true;
            bool success = Command.Start 
                            ? _serviceOrPluginData.DynamicStartByCommand( (Command.Impact | _serviceOrPluginData.ConfigSolvedImpact).ClearUselessTryBits(), idxCommand == 0 ) 
                            : _serviceOrPluginData.DynamicStopByCommand();
            return success || idxCommand < 50;
        }

    }
}
