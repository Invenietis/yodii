using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace Yodii.Engine
{
    internal class LiveConfiguration : ILiveConfiguration
    {
        List<ILivePluginInfo> _pluginsLiveInfo;
        List<ILiveServiceInfo> _servicesLiveInfo;
        ObservableCollection<YodiiCommand> _yodiiCommands = new ObservableCollection<YodiiCommand>(); //"Move" method available 

        internal LiveConfiguration( List<ILivePluginInfo> pluginLiveInfo, List<ILiveServiceInfo> serviceLiveInfo, ObservableCollection<YodiiCommand> YodiiCommands )
        {
            _pluginsLiveInfo = pluginLiveInfo;
            _servicesLiveInfo = serviceLiveInfo;
            _yodiiCommands = YodiiCommands;
        }
        //Adding new items at the top of the list so that the most recent command gets handled first.
        //First command must always be satisfied. 
        internal void AddYodiiCommand( YodiiCommand command, [CallerMemberName]string caller = null )
        {
            //TO DO: Check for object caller in existing methods
            Debug.Assert( command != null);
            YodiiCommand Com = _yodiiCommands.FirstOrDefault(c => c == command );
            if (Com == null)
            {
                _yodiiCommands.Insert( 0, command );
            }
            else
            {
                int i = _yodiiCommands.IndexOf( Com );
                _yodiiCommands.Move( i, 0 );
            }
            OnPropertyChanged(); 
        }

        private void OnPropertyChanged( [CallerMemberName]string caller = null )
        {
            var handler = PropertyChanged;
            if ( handler != null )
            {
                handler( this, new PropertyChangedEventArgs( caller ) );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //When a command cannot be satisfied:
        //- it gets removed from the YodiiCommand list,
        //- it gets added to the "UnsatisfiedCommand" list with an (observable?) reason (new enum) expliciting why it could not be satisfied.
        
        public IReadOnlyList<ILivePluginInfo> PluginLiveInfo { get { return _pluginsLiveInfo.AsReadOnlyList(); } }

        public IReadOnlyList<ILiveServiceInfo> ServiceLiveInfo { get { return _servicesLiveInfo.AsReadOnlyList(); } }

        public IReadOnlyList<YodiiCommand> YodiiCommands { get { return _yodiiCommands.AsReadOnlyList(); } }
    }
}
