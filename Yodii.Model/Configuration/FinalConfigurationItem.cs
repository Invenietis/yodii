using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Read-only configuration item.
    /// </summary>
    public struct FinalConfigurationItem
    {
        readonly string _serviceOrPluginFullName;
        readonly ConfigurationStatus _status;
        readonly StartDependencyImpact _impact;

        /// <summary>
        /// Service or plugin ID.
        /// </summary>
        public string ServiceOrPluginFullName
        {
            get { return _serviceOrPluginFullName; }
        }

        /// <summary>
        /// Required configuration status.
        /// </summary>
        public ConfigurationStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Required configuration impact.
        /// </summary>
        public StartDependencyImpact Impact
        {
            get { return _impact; }
        }

        public FinalConfigurationItem( string serviceOrPluginFullName, ConfigurationStatus status, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceOrPluginFullName ) );

            _serviceOrPluginFullName = serviceOrPluginFullName;
            _status = status;
            _impact = impact;
        }

        public static StartDependencyImpact Combine( StartDependencyImpact i1, StartDependencyImpact i2, out string invalidCombination ) 
        {
            invalidCombination = "";
            if( i1 == i2 ) return i1;

            if( i1 == StartDependencyImpact.Unknown ) i1 = StartDependencyImpact.Minimal;
            if( i2 == StartDependencyImpact.Unknown ) i2 = StartDependencyImpact.Minimal;

            switch( i1 )
            {
                case StartDependencyImpact.FullStop:
                    if( i2 == StartDependencyImpact.StopOptionalAndRunnable 
                        || i2 == StartDependencyImpact.Minimal
                        || i2 == StartDependencyImpact.TryFullStop 
                        || i2 == StartDependencyImpact.TryFullStart
                        || i2 == StartDependencyImpact.TryStartRecommended
                        || i2 == StartDependencyImpact.TryStopOptionalAndRunnable ) return i1;
                    ///Impacts causing conflict: FullStart, StartRecommended
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.StopOptionalAndRunnable:
                    if( i2 == StartDependencyImpact.Minimal 
                        || i2 == StartDependencyImpact.TryFullStart
                        || i2 == StartDependencyImpact.TryFullStop
                        || i2 == StartDependencyImpact.TryStartRecommended
                        || i2 == StartDependencyImpact.TryStopOptionalAndRunnable ) return i1;
                    if( i2 == StartDependencyImpact.FullStop ) return i2;

                    ///Impacts causing conflict: FullStart, StartRecommanded
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.Minimal:
                    return i2;

                case StartDependencyImpact.StartRecommended:
                    if( i2 == StartDependencyImpact.Minimal
                        || i2 == StartDependencyImpact.TryFullStart
                        || i2 == StartDependencyImpact.TryFullStop
                        || i2 == StartDependencyImpact.TryStartRecommended
                        || i2 == StartDependencyImpact.TryStopOptionalAndRunnable ) return i1;
                    else if( i2 == StartDependencyImpact.FullStart ) return i2;
                    ///Impacts causing conflicts: FullStop, StopOptionalAndRunnable
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.FullStart:
                    if( i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.Minimal
                        || i2 == StartDependencyImpact.TryFullStart
                        || i2 == StartDependencyImpact.TryFullStop
                        || i2 == StartDependencyImpact.TryStartRecommended
                        || i2 == StartDependencyImpact.TryStopOptionalAndRunnable ) return i1;
                    ///Impacts causing conflict: FullStop, StopOptionalAndRunnable
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.TryFullStop:
                    if( i2 == StartDependencyImpact.FullStart
                        || i2 == StartDependencyImpact.FullStop
                        || i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.StopOptionalAndRunnable ) return i2;
                    else if( i2 == StartDependencyImpact.Minimal || i2 == StartDependencyImpact.TryStopOptionalAndRunnable ) return i1;
                    ///Impacts causing conflict: TryFullStart, TryStartRecommended
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.TryFullStart:
                    if( i2 == StartDependencyImpact.FullStart
                        || i2 == StartDependencyImpact.FullStop
                        || i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.StopOptionalAndRunnable ) return i2;
                    else if( i2 == StartDependencyImpact.Minimal || i2 == StartDependencyImpact.TryStartRecommended ) return i1;
                    ///Impacts causing conflict: TryFullStop, TryStopOptionalAndRunnable
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.TryStartRecommended:
                    if( i2 == StartDependencyImpact.FullStart
                        || i2 == StartDependencyImpact.FullStop
                        || i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.StopOptionalAndRunnable 
                        || i2 == StartDependencyImpact.TryFullStart ) return i2;
                    else if( i2 == StartDependencyImpact.Minimal ) return i1;
                    ///Impacts causing conflict: TryFullStop, TryStopOptionalAndRunnable
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.TryStopOptionalAndRunnable:
                    if( i2 == StartDependencyImpact.FullStart
                        || i2 == StartDependencyImpact.FullStop
                        || i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.StopOptionalAndRunnable 
                        || i2 == StartDependencyImpact.TryFullStop ) return i2;
                    else if( i2 == StartDependencyImpact.Minimal ) return i1;
                    ///Impacts causing conflict: TryFullStart, TryStartRecommanded
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break; 
            }
            return StartDependencyImpact.Unknown;
        }

        //public static ConfigurationStatus Combine( ConfigurationStatus s1, ConfigurationStatus s2, out string invalidCombination ) 
        //{
        //    invalidCombination = "";
        //    if( s1 == s2 ) return s1;
        //}
    }
}
