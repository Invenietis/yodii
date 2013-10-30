using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Yodii.Model.ConfigurationSolver
{
    partial class ServiceData
    {
        RunningStatus? _dynamicStatus;

        public RunningStatus? Status { get { return _dynamicStatus; } set { _dynamicStatus = value; } }
    }
}
