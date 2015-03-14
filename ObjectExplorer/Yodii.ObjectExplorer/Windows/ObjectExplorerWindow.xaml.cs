using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Yodii.ObjectExplorer.Windows
{
    /// <summary>
    /// Interaction logic for ObjectExplorerWindow.xaml
    /// </summary>
    public partial class ObjectExplorerWindow
    {
        public ObjectExplorerWindow()
        {
            BindingErrorListener.Listen( m => MessageBox.Show( m ) );
            InitializeComponent();
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
        public override void Write( string message )
        {
            logAction( message );
        }
        public override void WriteLine( string message )
        {
            logAction( message );
        }
    }
}
