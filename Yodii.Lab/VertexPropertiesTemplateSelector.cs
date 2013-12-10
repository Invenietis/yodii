using System.Windows;
using System.Windows.Controls;

namespace Yodii.Lab
{
    /// <summary>
    /// Properties template selector.
    /// Determines whether the plugin or service properties should be displayed.
    /// </summary>
    public class VertexPropertiesTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Template to use for services.
        /// </summary>
        public DataTemplate ServicePropertiesTemplate { get; set; }

        /// <summary>
        /// Template to use for plugins.
        /// </summary>
        public DataTemplate PluginPropertiesTemplate { get; set; }

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

            if( vertex.IsPlugin ) {
                return PluginPropertiesTemplate;
            }
            else
            {
                return ServicePropertiesTemplate;
            }

        }
    }
}
