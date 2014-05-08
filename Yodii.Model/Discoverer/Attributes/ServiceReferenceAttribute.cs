using System;

namespace Yodii.Model
{
    [AttributeUsage( AttributeTargets.Constructor, AllowMultiple = true )]
    public sealed class DependencyRequirementAttribute : Attribute
    {
        readonly DependencyRequirement _req;
        readonly string _paramName;

        public DependencyRequirementAttribute( DependencyRequirement req, string paramName )
        {
            _req = req;
            _paramName = paramName;
        }

        string ParameterName { get { return _paramName; } }
        DependencyRequirement DependencyReq { get { return _req; } }
    }
}
