#region LGPL License
/*----------------------------------------------------------------------------
* This file (ObjectExplorer\Yodii.ObjectExplorer.Wpf\ObjectExplorerWindow.xaml.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphX;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Wpf
{
    internal partial class ObjectExplorerWindow : Window
    {
        readonly ObjectExplorerWindowViewModel _vm;
        readonly YodiiLayout _graphLayout;
        readonly IYodiiEngine _engine;
        ILivePluginInfo _objectExplorerLiveInfo;

        /// <summary>
        /// Creates the main window.
        /// </summary>
        public ObjectExplorerWindow( IYodiiEngine engine )
        {
            _engine = engine;

            BindingErrorListener.Listen( m => MessageBox.Show( m ) );

            _vm = new ObjectExplorerWindowViewModel( engine );
            this.DataContext = _vm;

            _vm.NewNotification += _vm_NewNotification;
            _vm.VertexPositionRequest += _vm_VertexPositionRequest;
            _vm.AutoPositionRequest += _vm_AutoPositionRequest;

            _vm.Graph.GraphUpdateRequested += Graph_GraphUpdateRequested;

            InitializeComponent();

            GraphArea.GenerateGraphFinished += GraphArea_GenerateGraphFinished;
            GraphArea.RelayoutFinished += GraphArea_RelayoutFinished;

            ZoomBox.IsAnimationDisabled = false;
            ZoomBox.MaxZoomDelta = 2;
            GraphArea.UseNativeObjectArrange = false;
            //GraphArea.SideExpansionSize = new Size( 100, 100 );


            _graphLayout = new YodiiLayout();
            GraphArea.LayoutAlgorithm = _graphLayout;
            GraphArea.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            GraphArea.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA;
            GraphArea.EdgeCurvingEnabled = true;

            this.Loaded += MainWindow_Loaded;

            _vm.Graph.VertexAdded += Graph_VertexAdded;
            _vm.Graph.VertexRemoved += Graph_VertexRemoved;
            _vm.Graph.EdgeAdded += Graph_EdgeAdded;
            _vm.Graph.EdgeRemoved += Graph_EdgeRemoved;
        }

        void _vm_AutoPositionRequest( object sender, EventArgs e )
        {
            var result = MessageBox.Show(
                "Automatically position all elements in the graph?\nThis will reset all their positions.",
                "Auto-position elements",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No
                );
            if( result == MessageBoxResult.Yes )
            {
                _graphLayout.NextRecomputeForcesPositions = true;
                GraphArea.RelayoutGraph();
            }
        }

        void _vm_VertexPositionRequest( object sender, VertexPositionEventArgs e )
        {
            e.VertexPositions = GraphArea.GetVertexPositions();
        }

        void Graph_EdgeRemoved( YodiiGraphEdge e )
        {
            GraphArea.RemoveEdge( e );
        }

        void Graph_EdgeAdded( YodiiGraphEdge e )
        {
            GraphArea.AddEdge( e, new EdgeControl( GraphArea.VertexList[e.Source], GraphArea.VertexList[e.Target], e ) );
        }

        void Graph_VertexRemoved( YodiiGraphVertex vertex )
        {
            GraphArea.RemoveVertex( vertex );
        }

        void Graph_VertexAdded( YodiiGraphVertex vertex )
        {
            var control = new VertexControl( vertex );

            GraphArea.AddVertex( vertex, control );
            DragBehaviour.SetUpdateEdgesOnMove( control, true );
        }

        void _vm_NewNotification( object sender, NotificationEventArgs e )
        {
            if( this.NotificationControl != null )
            {
                this.NotificationControl.AddNotification( e.Notification );
            }
        }

        void MainWindow_Loaded( object sender, RoutedEventArgs e )
        {
            _objectExplorerLiveInfo = _engine.LiveInfo.FindPlugin( typeof( ObjectExplorerPlugin ).FullName );
            if( _objectExplorerLiveInfo != null ) _objectExplorerLiveInfo.Capability.PropertyChanged += Capability_PropertyChanged;
            _engine.LiveInfo.Plugins.CollectionChanged += Plugins_CollectionChanged;

            UpdateStatus();
            GraphArea.GenerateGraph( _vm.Graph );

            Timer updateTimer = new Timer( 500 );
            updateTimer.Elapsed += ( source, args ) =>
            {
                updateTimer.Enabled = false;

                Action a = new Action( () =>
                {
                    _graphLayout.NextRecomputeForcesPositions = true;
                    GraphArea.RelayoutGraph();
                } );
                Application.Current.Dispatcher.Invoke( a );
            };
            updateTimer.Enabled = true;
        }

        void Plugins_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == NotifyCollectionChangedAction.Add
                && e.NewItems.Count > 0
                && e.NewItems[0] is ILivePluginInfo
                && ((ILivePluginInfo)e.NewItems[0]).FullName == typeof( ObjectExplorerPlugin ).FullName )
            {
                // We were added to the run plugins.
                _objectExplorerLiveInfo = _engine.LiveInfo.FindPlugin( typeof( ObjectExplorerPlugin ).FullName );
                _objectExplorerLiveInfo.Capability.PropertyChanged += Capability_PropertyChanged;
                UpdateStatus();
                Debug.Assert( _objectExplorerLiveInfo != null );
            }
        }

        void Capability_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "CanStop" ) UpdateStatus();
        }

        void UpdateStatus()
        {
            bool enable = _objectExplorerLiveInfo != null ? _objectExplorerLiveInfo.Capability.CanStop : false;
            SetSysMenu( enable );
        }

        void GraphArea_RelayoutFinished( object sender, EventArgs e )
        {
            ZoomBox.ZoomToFill();
            ZoomBox.Mode = GraphX.Controls.ZoomControlModes.Custom;
            foreach( var item in GraphArea.VertexList )
            {
                DragBehaviour.SetIsDragEnabled( item.Value, true );
                item.Value.EventOptions.PositionChangeNotification = true;
            }
            GraphArea.ShowAllEdgesLabels();
            GraphArea.InvalidateVisual();

        }

        void GraphArea_GenerateGraphFinished( object sender, EventArgs e )
        {
            GraphArea.GenerateAllEdges();
            ZoomBox.ZoomToFill();
            ZoomBox.Mode = GraphX.Controls.ZoomControlModes.Custom;
            foreach( var item in GraphArea.VertexList )
            {
                DragBehaviour.SetIsDragEnabled( item.Value, true );
                item.Value.EventOptions.PositionChangeNotification = true;
            }
            GraphArea.ShowAllEdgesLabels();
            GraphArea.InvalidateVisual();

        }

        void Graph_GraphUpdateRequested( object sender, EventArgs e )
        {
            GraphArea.RelayoutGraph();
        }

        private void StackPanel_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            FrameworkElement vertexPanel = sender as FrameworkElement;

            YodiiGraphVertex vertex = vertexPanel.DataContext as YodiiGraphVertex;

            _vm.SelectedVertex = null;
            _vm.SelectedVertex = vertex;
        }

        private void graphLayout_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            _vm.SelectedVertex = null;
        }

        private void SetSysMenu( bool enable )
        {
            if( enable )
            {
                this.ShowSysMenu();
            }
            else
            {
                this.HideSysMenu();
            }
        }
    }

    public class BindingErrorListener : TraceListener
    {
        private Action<string> logAction;
        public static void Listen( Action<string> logAction )
        {
            PresentationTraceSources.DataBindingSource.Listeners
                .Add( new BindingErrorListener() { logAction = logAction } );
        }
        public override void Write( string message ) { }
        public override void WriteLine( string message )
        {
            logAction( message );
        }
    }

    public static class WindowExtensions
    {
        #region Win32 imports

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport( "user32.dll", SetLastError = true )]
        private static extern int GetWindowLong( IntPtr hWnd, int nIndex );
        [DllImport( "user32.dll" )]
        private static extern int SetWindowLong( IntPtr hWnd, int nIndex, int dwNewLong );

        #endregion

        internal static void HideSysMenu( this Window w )
        {
            var hwnd = new WindowInteropHelper( w ).Handle;
            SetWindowLong( hwnd, GWL_STYLE, GetWindowLong( hwnd, GWL_STYLE ) & ~WS_SYSMENU );
        }

        internal static void ShowSysMenu( this Window w )
        {
            var hwnd = new WindowInteropHelper( w ).Handle;
            SetWindowLong( hwnd, GWL_STYLE, GetWindowLong( hwnd, GWL_STYLE ) | WS_SYSMENU );
        }

    }
}
