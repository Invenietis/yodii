using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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

        /// <summary>
        /// Creates the main window.
        /// </summary>
        public ObjectExplorerWindow( IYodiiEngine engine )
        {
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
            GraphArea.GenerateGraph( _vm.Graph );

            Timer updateTimer = new Timer( 500 );
            updateTimer.Elapsed += ( source, args ) =>
            {
                updateTimer.Enabled = false;

                Action a = new Action( () => {
                    _graphLayout.NextRecomputeForcesPositions = true;
                    GraphArea.RelayoutGraph();
                } );
                Application.Current.Dispatcher.Invoke(a);
            };
            updateTimer.Enabled = true;
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
            GraphArea.ExportAsPNG( true );
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
}
