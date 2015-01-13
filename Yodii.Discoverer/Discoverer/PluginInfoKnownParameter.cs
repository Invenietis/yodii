using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Discoverer
{
    class PluginInfoKnownParameter : IPluginCtorKnownParameterInfo
    {
        readonly string _parameterName;
        readonly int _parameterIndex;
        readonly string _descriptiveType;

        public PluginInfoKnownParameter(string parameterName, int parameterIndex, string descriptiveType )
        {
            _parameterName = parameterName;
            _parameterIndex = parameterIndex;
            _descriptiveType = descriptiveType;
        }

        public string ParameterName
        {
            get { return _parameterName; }
        }

        public int ParameterIndex
        {
            get { return _parameterIndex; }
        }

        public string DescriptiveType
        {
            get { return _descriptiveType; }
        }
    }
}
