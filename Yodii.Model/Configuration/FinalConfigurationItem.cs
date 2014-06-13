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

        /// <summary>
        /// A FinalConfigurationItem is an immutable object that displays the latest configuration of an item.
        /// </summary>
        public FinalConfigurationItem( string serviceOrPluginFullName, ConfigurationStatus status, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceOrPluginFullName ) );

            _serviceOrPluginFullName = serviceOrPluginFullName;
            _status = status;
            _impact = impact;
        }

        /// <summary>
        /// This function encapsulates all configuration constraints regarding the <see cref="ConfigurationStatus"/> of an item.
        /// </summary>
        /// <param name="s1">Current <see cref="ConfigurationStatus"/></param>
        /// <param name="s2">Wanted <see cref="ConfigurationStatus"/></param>
        /// <param name="invalidCombination">string expliciting the error. Empty if all is well</param>
        /// <returns></returns>
        public static ConfigurationStatus Combine( ConfigurationStatus s1, ConfigurationStatus s2, out string invalidCombination )
        {
            invalidCombination = "";
            if( s1 == s2 ) return s1;
           
            if( s2 != ConfigurationStatus.Optional )
            {
                if( s1 == ConfigurationStatus.Optional || (s1 >= ConfigurationStatus.Runnable && s2 >= ConfigurationStatus.Runnable) )
                {
                    return (s1 == ConfigurationStatus.Running) ? s1 : s2;
                }
                else if( s1 != s2 )
                {
                    invalidCombination = string.Format( "Conflict for statuses {0} and {1}", s1, s2 );
                    return s1;
                }
                else
                {
                    invalidCombination = string.Format( "Something went terribly, terribly wrong..." );
                    return s1;
                }
            }
            return s1;
        }

        /// <summary>
        /// This function encapsulates all configuration constraints regarding the <see cref="StartDependencyImpact"/> of an item.
        /// </summary>
        /// <param name="s1">Current <see cref="StartDependencyImpact"/></param>
        /// <param name="s2">Wanted <see cref="StartDependencyImpact"/></param>
        /// <param name="invalidCombination">string expliciting the error. Empty if all is well</param>
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
                        || i2 == StartDependencyImpact.TryStopOptionalAndRunnable 
                        || i2 == StartDependencyImpact.TryStartRecommendedAndStopOptionalAndRunnable ) return i1;
                    ///Impacts causing conflict: FullStart, StartRecommended, StartRecommendedAndStopOptionalAndRunnable
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.StopOptionalAndRunnable:
                    if( i2 == StartDependencyImpact.Minimal 
                        || i2 == StartDependencyImpact.TryFullStart
                        || i2 == StartDependencyImpact.TryFullStop
                        || i2 == StartDependencyImpact.TryStartRecommended
                        || i2 == StartDependencyImpact.TryStopOptionalAndRunnable
                        || i2 == StartDependencyImpact.TryStartRecommendedAndStopOptionalAndRunnable ) return i1;
                    if( i2 == StartDependencyImpact.FullStop || i2 == StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable ) return i2;
                    if( i2 == StartDependencyImpact.StartRecommended ) return StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable;
                    ///Impacts causing conflict: FullStart
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.Minimal:
                    return i2;

                case StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable:
                    if( i2 == StartDependencyImpact.Minimal
                        || i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.StopOptionalAndRunnable
                        || i2 == StartDependencyImpact.TryFullStart
                        || i2 == StartDependencyImpact.TryFullStop
                        || i2 == StartDependencyImpact.TryStartRecommended
                        || i2 == StartDependencyImpact.TryStopOptionalAndRunnable
                        || i2 == StartDependencyImpact.TryStartRecommendedAndStopOptionalAndRunnable ) return i1;
                        //Impacts causing conflicts: FullStart, FullStop
                    else invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.StartRecommended:
                    if( i2 == StartDependencyImpact.Minimal
                        || i2 == StartDependencyImpact.TryFullStart
                        || i2 == StartDependencyImpact.TryFullStop
                        || i2 == StartDependencyImpact.TryStartRecommended
                        || i2 == StartDependencyImpact.TryStopOptionalAndRunnable
                        || i2 == StartDependencyImpact.TryStartRecommendedAndStopOptionalAndRunnable ) return i1;
                    if( i2 == StartDependencyImpact.FullStart || i2 == StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable ) return i2;
                    if( i2 == StartDependencyImpact.StopOptionalAndRunnable ) return StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable;
                    ///Impacts causing conflicts: FullStop
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.FullStart:
                    if( i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.Minimal
                        || i2 == StartDependencyImpact.TryFullStart
                        || i2 == StartDependencyImpact.TryFullStop
                        || i2 == StartDependencyImpact.TryStartRecommended
                        || i2 == StartDependencyImpact.TryStopOptionalAndRunnable
                        || i2 == StartDependencyImpact.TryStartRecommendedAndStopOptionalAndRunnable ) return i1;
                    ///Impacts causing conflict: FullStop, StopOptionalAndRunnable, StartRecommendedAndStopOptionalAndRunnable
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.TryFullStop:
                    if( i2 == StartDependencyImpact.FullStart
                        || i2 == StartDependencyImpact.FullStop
                        || i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.StopOptionalAndRunnable
                        || i2 == StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable ) return i2;
                    else if( i2 == StartDependencyImpact.Minimal || i2 == StartDependencyImpact.TryStopOptionalAndRunnable ) return i1;
                    ///Impacts causing conflict: TryFullStart, TryStartRecommended, TryStartRecommendedAndStopOptionalAndRunnable
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.TryFullStart:
                    if( i2 == StartDependencyImpact.FullStart
                        || i2 == StartDependencyImpact.FullStop
                        || i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.StopOptionalAndRunnable
                        || i2 == StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable ) return i2;
                    else if( i2 == StartDependencyImpact.Minimal || i2 == StartDependencyImpact.TryStartRecommended ) return i1;
                    ///Impacts causing conflict: TryFullStop, TryStopOptionalAndRunnable, TryStartRecommendedAndStopOptionalAndRunnable
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.TryStartRecommended:
                    if( i2 == StartDependencyImpact.FullStart
                        || i2 == StartDependencyImpact.FullStop
                        || i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.StopOptionalAndRunnable 
                        || i2 == StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable
                        || i2 == StartDependencyImpact.TryFullStart 
                        || i2 == StartDependencyImpact.TryStartRecommendedAndStopOptionalAndRunnable ) return i2;
                    else if( i2 == StartDependencyImpact.Minimal ) return i1;
                    ///Impacts causing conflict: TryFullStop, TryStopOptionalAndRunnable
                    invalidCombination = string.Format( "{0} and {1} cannot be combined", i1, i2 );
                    break;

                case StartDependencyImpact.TryStartRecommendedAndStopOptionalAndRunnable:
                    if( i2 == StartDependencyImpact.FullStart
                        || i2 == StartDependencyImpact.FullStop
                        || i2 == StartDependencyImpact.StartRecommended
                        || i2 == StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable
                        || i2 == StartDependencyImpact.StopOptionalAndRunnable
                        || i2 == StartDependencyImpact.TryStartRecommended
                        || i2 == StartDependencyImpact.TryStopOptionalAndRunnable ) return i2;
                    //Impacts causing conflict: TryFullStart, TryFullStop
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
    }
}
