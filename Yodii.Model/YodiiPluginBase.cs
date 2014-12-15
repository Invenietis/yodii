using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Small abstract base class that explicitly implements <see cref="IYodiiPlugin.PreStop"/>, 
    /// <see cref="IYodiiPlugin.PreStart"/>, <see cref="IYodiiPlugin.Stop"/> and 
    /// <see cref="IYodiiPlugin.Start"/> by redirecting them to virtual 
    /// protected empty methods.
    /// Using this class is totally optional: implementing <see cref="IYodiiPlugin"/> actually defines a plugin.
    /// </summary>
    public abstract class YodiiPluginBase : IYodiiPlugin
    {
        void IYodiiPlugin.PreStop( IPreStopContext c )
        {
            PluginPreStop( c );
        }

        void IYodiiPlugin.PreStart( IPreStartContext c )
        {
            PluginPreStart( c );
        }

        void IYodiiPlugin.Start( IStartContext c )
        {
            PluginStart( c );
        }

        void IYodiiPlugin.Stop( IStopContext c )
        {
            PluginStop( c );
        }

        /// <summary>
        /// Called before the actual <see cref="PluginStop"/> method.
        /// If this plugin can not be stopped, the transition must 
        /// be canceled by calling <see cref="IPreStopContext.Cancel"/>.
        /// </summary>
        /// <param name="c">The context to use.</param>
        protected virtual void PluginPreStop( IPreStopContext c )
        {
        }

        /// <summary>
        /// Called before the actual <see cref="PluginStart"/> method.
        /// If this plugin can not start, the transition must 
        /// be canceled by calling <see cref="IPreStartContext.Cancel"/> .
        /// </summary>
        /// <param name="c">The context to use.</param>
        protected virtual void PluginPreStart( IPreStartContext c )
        {
        }

        /// <summary>
        /// Called after successful calls to all <see cref="PluginPreStop"/> and <see cref="PluginPreStart"/>.
        /// This may also be called to cancel a previous call to <see cref="PluginPreStart"/> if another
        /// plugin rejected the transition.
        /// </summary>
        /// <param name="c">The context to use.</param>
        protected virtual void PluginStart( IStartContext c )
        {
        }

        /// <summary>
        /// Called after successful calls to all <see cref="PluginPreStop"/> and <see cref="PluginPreStart"/>.
        /// This may also be called to cancel a previous call to <see cref="PluginPreStop"/> if another
        /// plugin rejected the transition.
        /// </summary>
        /// <param name="c">The context to use.</param>
        protected virtual void PluginStop( IStopContext c )
        {
        }
    }
}
