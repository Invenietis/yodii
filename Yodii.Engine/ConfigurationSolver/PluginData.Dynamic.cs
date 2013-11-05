//PluginData.Dynamic makes up with the PluginData partial class to work. 
//This file implements the strategy feature which is not our first concern at the moment... But we'll get back to it !

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Yodii.Model;
using Yodii.Model.ConfigurationSolver;

namespace Yodii.Engine
{
    partial class PluginData
    {
        RunningStatus? _dynamicStatus;

        public RunningStatus? Status { get { return _dynamicStatus; } }
    }
}
