using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Engine
{
    partial class ServiceData
    {
        RunningStatus? _dynamicStatus;

        public RunningStatus? Status { get { return _dynamicStatus; } set { _dynamicStatus = value; } }

        internal void DynamicStart()
        {

        }
        internal void DynamicStop()
        {
        }
    }
}
