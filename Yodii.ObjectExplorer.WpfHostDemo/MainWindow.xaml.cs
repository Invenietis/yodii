using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Yodii.Discoverer;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.ObjectExplorer.WpfHostDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        IYodiiEngine _engine;
        ILivePluginInfo _objectExplorer;
        ObjectExplorerManager _manager;

        public MainWindow()
        {
            _manager = new ObjectExplorerManager();
            _engine = _manager.Engine;

            _manager.SetDiscoveredInfo();

            // ObjectExplorer doesn't exist while Engine is dead, so we watch the plugins until it does.
            _engine.LiveInfo.Plugins.CollectionChanged += Plugins_CollectionChanged;

            this.DataContext = this;

            InitializeComponent();
            Engine.Start();
        }

        void Plugins_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            ObjectExplorerPlugin = _engine.LiveInfo.FindPlugin( "Yodii.ObjectExplorer.Wpf.ObjectExplorerPlugin" );
        }

        public IYodiiEngine Engine { get { return _engine; } }

        public ILivePluginInfo ObjectExplorerPlugin
        {
            get { return _objectExplorer; }
            private set
            {
                _objectExplorer = value;
                RaisePropertyChanged();
            }
        }

        private void Start_Click( object sender, RoutedEventArgs e )
        {
            Engine.Start();
        }

        private void Stop_Click( object sender, RoutedEventArgs e )
        {
            Engine.Stop();
        }

        private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            Engine.Stop();
        }

        private void StartOE_Click( object sender, RoutedEventArgs e )
        {
            if( ObjectExplorerPlugin.Capability.CanStart )
            {
                ObjectExplorerPlugin.Start();
            }
            else
            {
                MessageBox.Show( "The Object Explorer is disabled by configuration and cannot be started.", "Cannot start" );
            }
        }
        
        private void StopOE_Click( object sender, RoutedEventArgs e )
        {
            if( ObjectExplorerPlugin.Capability.CanStop )
            {
                ObjectExplorerPlugin.Stop();
            }
            else
            {
                MessageBox.Show( "The Object Explorer is required by configuration and cannot be stopped.", "Cannot stop" );
            }
        }
        private void ResetConfig_Click( object sender, RoutedEventArgs e )
        {
            _manager.ResetConfiguration();
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
        protected void RaisePropertyChanged( [CallerMemberName] string caller = null )
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
