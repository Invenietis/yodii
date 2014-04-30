using System;

namespace Yodii.Model
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public sealed class DependencyRequirementAttribute : Attribute
    {
        DependencyRequirement _req;

        public DependencyRequirementAttribute( DependencyRequirement req )
        {
            DependencyRequirement _req = req;
        }

        public DependencyRequirement Requires { get { return _req; } set { _req = value; } }
    }
}
