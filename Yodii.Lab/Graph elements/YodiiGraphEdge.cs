using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using Yodii.Model.CoreModel;

namespace Yodii.Lab
{
    public class YodiiGraphEdge : Edge<YodiiGraphVertex>
    {
        readonly YodiiGraphEdgeType _type;
        readonly RunningRequirement _referenceRequirement;

        internal YodiiGraphEdge( YodiiGraphVertex source, YodiiGraphVertex target, YodiiGraphEdgeType type )
            : base( source, target )
        {
            _type = type;
        }

        internal YodiiGraphEdge( YodiiGraphVertex source, YodiiGraphVertex target, RunningRequirement requirement )
            : this( source, target, YodiiGraphEdgeType.ServiceReference )
        {
            _referenceRequirement = requirement;
        }

        public RunningRequirement ReferenceRequirement { get { return _referenceRequirement; } }
        public YodiiGraphEdgeType Type { get { return _type; } }

        public bool IsSpecialization { get { return Type == YodiiGraphEdgeType.Specialization; } }
        public bool IsImplementation { get { return Type == YodiiGraphEdgeType.Implementation; } }
        public bool IsServiceReference { get { return Type == YodiiGraphEdgeType.ServiceReference; } }
    }
}
