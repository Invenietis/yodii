using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Lab.DesignModel
{
    public class DesignTimeServiceInfoManager : ServiceInfoManager
    {
        public DesignTimeServiceInfoManager()
        {
            this.CreateNewService( "Service.A" );
            this.CreateNewService( "Service.B" );

            this.CreateNewPlugin( Guid.Parse( "D2E420E0-9C91-4280-AD32-F34D00EEF92C" ), "Plugin A" );
            this.CreateNewPlugin( Guid.Parse( "D2E420E0-9C91-4280-AD32-F34D00EEF92D" ), "Plugin B" );
        }
    }
}
