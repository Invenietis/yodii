#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Graph elements\YodiiGraphEdge.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using GraphX;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Edge between two vertices in the Yoddi.Lab graph.
    /// </summary>
    public class YodiiGraphEdge : GraphX.EdgeBase<YodiiGraphVertex>, INotifyPropertyChanged
    {
        readonly YodiiGraphEdgeType _type;
        DependencyRequirement? _referenceRequirement;

        internal YodiiGraphEdge( YodiiGraphVertex source, YodiiGraphVertex target, YodiiGraphEdgeType type )
            : base( source, target )
        {
            _type = type;
        }

        /// <summary>
        /// Used for GraphX serialization. Not implemented.
        /// </summary>
        public YodiiGraphEdge()
            : base(null, null)
        {
        }

        internal YodiiGraphEdge( YodiiGraphVertex source, YodiiGraphVertex target, MockServiceReferenceInfo serviceRef )
            : this( source, target, YodiiGraphEdgeType.ServiceReference )
        {
            serviceRef.PropertyChanged += serviceRef_PropertyChanged;
            _referenceRequirement = serviceRef.Requirement;
        }

        void serviceRef_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "Requirement" ) ReferenceRequirement = (sender as MockServiceReferenceInfo).Requirement;
        }

        /// <summary>
        /// Requirement type of this edge, if it's a service reference.
        /// </summary>
        public DependencyRequirement? ReferenceRequirement
        {
            get { return _referenceRequirement; }
            internal set
            {
                if( value != _referenceRequirement )
                {
                    _referenceRequirement = value;
                    RaisePropertyChanged( "ReferenceRequirement" );
                    RaisePropertyChanged( "Description" );
                }
            }
        }

        /// <summary>
        /// Type of edge.
        /// </summary>
        public YodiiGraphEdgeType Type { get { return _type; } }

        /// <summary>
        /// True if edge is a specialization (Service specializes service).
        /// </summary>
        public bool IsSpecialization { get { return Type == YodiiGraphEdgeType.Specialization; } }
        
        /// <summary>
        /// True if edge is an implementation (Plugin implements Service).
        /// </summary>
        public bool IsImplementation { get { return Type == YodiiGraphEdgeType.Implementation; } }
        
        /// <summary>
        /// True if edge is a service reference (Plugin refers to Service).
        /// </summary>
        public bool IsServiceReference { get { return Type == YodiiGraphEdgeType.ServiceReference; } }

        /// <summary>
        /// Description of this edge.
        /// </summary>
        public string Description
        {
            get
            {
                if( IsSpecialization )
                {
                    return String.Format( "Specialization:\n\nService {0} specializes service {1}.",
                        Source.LabServiceInfo.ServiceInfo.ServiceFullName,
                        Target.LabServiceInfo.ServiceInfo.ServiceFullName );
                }
                else if( IsImplementation )
                {
                    return String.Format( "Implementation:\n\nPlugin {0} implements service {1}.",
                        Source.LabPluginInfo.PluginInfo.Description,
                        Target.LabServiceInfo.ServiceInfo.ServiceFullName );
                }
                else
                {
                    return String.Format( "Service reference ({0}):\n\nPlugin {1} references service {2} with requirement: {0}.",
                        ReferenceRequirement.ToString(),
                        Source.LabPluginInfo.PluginInfo.Description,
                        Target.LabServiceInfo.ServiceInfo.ServiceFullName);
                }
            }
        }

        #region INotifyPropertyChanged utilities

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Throw new PropertyChanged.
        /// </summary>
        /// <param name="caller">Fill with Member name, when called from a property.</param>
        protected void RaisePropertyChanged( string caller )
        {
            Debug.Assert( caller != null );
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( caller ) );
            }
        }

        #endregion INotifyPropertyChanged utilities
    }
}
