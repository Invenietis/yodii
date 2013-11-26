using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Yodii.Lab
{
    public class VertexPropertiesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ServicePropertiesTemplate { get; set; }
        public DataTemplate PluginPropertiesTemplate { get; set; }

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
