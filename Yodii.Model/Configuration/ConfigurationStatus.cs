using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public enum ConfigurationStatus
    {
        Disable = -1,

        Optional = 0,

        Runnable = 4,

        Running = 6
    }
}
