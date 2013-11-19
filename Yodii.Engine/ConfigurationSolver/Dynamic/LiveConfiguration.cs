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
        
        /// <summary>
        /// Adding new items at the top of the list so that the most recent command gets handled first.
        //  First command must always be satisfied. 
        //  A given caller can have at most one command on a given plugin/service
        /// </summary>
        /// <param name="command"></param>
        internal void AddYodiiCommand( YodiiCommand command )
        {
            Debug.Assert( command != null);
            YodiiCommand Com = _yodiiCommands.FirstOrDefault( c => c == command && c._caller == command._caller);
            if ( Com != null )
            {
                int i = _yodiiCommands.IndexOf( Com );
                _yodiiCommands.Move( i, 0 );
            }
            else _yodiiCommands.Insert( 0, command );
        }
      
        internal void RevokeCaller( Object caller )
        {
            if(caller.GetType() == PluginLiveInfo) { /*Need some additional verification here!*/ }
            ObservableCollection<YodiiCommand> temp = new ObservableCollection<YodiiCommand>();
            foreach ( YodiiCommand command in _yodiiCommands )
            {
                if ( command._caller != caller ) temp.Add( command );
            }
            _yodiiCommands.Clear();
            _yodiiCommands = temp;
            //Execute command list
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


        void ILiveConfiguration.RevokeCaller( object caller )
        {
            throw new NotImplementedException();
        }
    }
}
