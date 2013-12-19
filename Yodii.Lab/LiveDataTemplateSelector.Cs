using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Yodii.Lab
{
    /// <summary>
    /// Live data template selector.
    /// Determines whether live plugin or live service properties should be displayed.
    /// </summary>
    public class LiveDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Template to use for services.
        /// </summary>
        public DataTemplate LiveServiceDataTemplate { get; set; }

        /// <summary>
        /// Template to use for plugins.
        /// </summary>
        public DataTemplate LivePluginDataTemplate { get; set; }

        /// <summary>
        /// Selects template.
        /// </summary>
        /// <param name="item">Service or plugin.</param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate( object item, DependencyObject container )
        {
            if( item == null ) return null;

            YodiiGraphVertex vertex = (YodiiGraphVertex)item;

            if( vertex.IsPlugin )
            {
                return LivePluginDataTemplate;
            }
            else if( vertex.IsService )
            {
                return LiveServiceDataTemplate;
            }
            return null;
        }
    }
}
