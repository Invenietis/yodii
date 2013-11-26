using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Lab.DesignModel
{
    public class DesignTimeConfigurationManager : ConfigurationManager
    {
        public DesignTimeConfigurationManager()
        {
            var layer1 = new ConfigurationLayer( "Layer 1" );
            layer1.Items.Add( "Service.A", ConfigurationStatus.Running, "Design-time reason" );
            layer1.Items.Add( "D2E420E0-9C91-4280-AD32-F34D00EEF92C", ConfigurationStatus.Runnable, "Design-time reason" );

            var layer2 = new ConfigurationLayer();
            layer2.Items.Add( "Service.B", ConfigurationStatus.Disable, "Design-time reason" );
            layer2.Items.Add( "D2E420E0-9C91-4280-AD32-F34D00EEF92D", ConfigurationStatus.Optional, "Design-time reason" );

            this.Layers.Add( layer1 );
            this.Layers.Add( layer2 );
        }
    }
}
