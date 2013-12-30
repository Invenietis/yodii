using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using GraphX;
using GraphX.GraphSharp.Algorithms.Layout;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        readonly MainWindowViewModel _vm;
        readonly YodiiLayout _graphLayout;

        /// <summary>
        /// Creates the main window.
        /// </summary>
        public MainWindow()
        {
            _vm = new MainWindowViewModel( false );
            this.DataContext = _vm;

            _vm.NewNotification += _vm_NewNotification;
            _vm.CloseBackstageRequest += _vm_CloseBackstageRequest;

            InitializeComponent();

            GraphArea.GenerateGraphFinished += GraphArea_GenerateGraphFinished;
            GraphArea.RelayoutFinished += GraphArea_RelayoutFinished;

            ZoomBox.IsAnimationDisabled = false;
            ZoomBox.MaxZoomDelta = 2;
            GraphArea.UseNativeObjectArrange = false;
            //GraphArea.SideExpansionSize = new Size( 100, 100 );

            _vm.Graph.GraphUpdateRequested += Graph_GraphUpdateRequested;

            _graphLayout = new YodiiLayout();
            GraphArea.LayoutAlgorithm = _graphLayout;
            GraphArea.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            GraphArea.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA;

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            _vm.Graph.VertexAdded += Graph_VertexAdded;
            _vm.Graph.VertexRemoved += Graph_VertexRemoved;
            _vm.Graph.EdgeAdded += Graph_EdgeAdded;
            _vm.Graph.EdgeRemoved += Graph_EdgeRemoved;
        }

        void Graph_EdgeRemoved( YodiiGraphEdge e )
        {
            GraphArea.RemoveEdge( e );
        }

        void Graph_EdgeAdded( YodiiGraphEdge e )
        {
            GraphArea.AddEdge(e, new EdgeControl(GraphArea.VertexList[e.Source], GraphArea.VertexList[e.Target], e));
        }

        void Graph_VertexRemoved( YodiiGraphVertex vertex )
        {
            GraphArea.RemoveVertex( vertex );
        }

        void Graph_VertexAdded( YodiiGraphVertex vertex )
        {
            GraphArea.AddVertex( vertex, new VertexControl( vertex ) );
        }

        void _vm_CloseBackstageRequest( object sender, EventArgs e )
        {
            if( RibbonBackstage != null ) RibbonBackstage.IsOpen = false;
        }

        void MainWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            _vm.StopAutosaveTimer();
            if( !_vm.SaveBeforeClosingFile() )
            {
                e.Cancel = true;
                _vm.StartAutosaveTimer();
            }
            else
            {
                _vm.ClearAutosave();
            }
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
            GraphArea.GenerateGraph( _vm.Graph );

            if( _vm.HasAutosave() )
            {
                var result = MessageBox.Show(
                    "Program did not properly close last time.\nDo you wish to load the last automatic save?\nPressing No will discard the save.",
                    "Auto-save available",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation,
                    MessageBoxResult.Yes
                    );

                if( result == MessageBoxResult.Yes )
                {
                    _vm.LoadAutosave();
                }
                else
                {
                    _vm.ClearAutosave();
                }
            }

            _vm.StartAutosaveTimer();
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

        private void ExportToPngButton_Click( object sender, RoutedEventArgs e )
        {
            GraphArea.ExportAsPNG( false );
        }

    }
}
