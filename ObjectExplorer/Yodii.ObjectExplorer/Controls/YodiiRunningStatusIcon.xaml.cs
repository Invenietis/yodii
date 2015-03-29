using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Yodii.Model;
using Yodii.ObjectExplorer.ViewModels;

namespace Yodii.ObjectExplorer.Controls
{
    /// <summary>
    /// Control that displays an icon corresponding to the running status of a Yodii live item.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class YodiiRunningStatusIcon : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YodiiRunningStatusIcon"/> class.
        /// </summary>
        public YodiiRunningStatusIcon()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the running status.
        /// </summary>
        /// <value>
        /// The running status.
        /// </value>
        public RunningStatus RunningStatus
        {
            get { return (RunningStatus)GetValue( RunningStatusProperty ); }
            set { SetValue( RunningStatusProperty, value ); }
        }

        /// <summary>
        /// The running status dependency property.
        /// </summary>
        public static readonly DependencyProperty RunningStatusProperty = 
            DependencyProperty.Register( "RunningStatus", typeof( RunningStatus ), typeof( YodiiRunningStatusIcon ), new PropertyMetadata( default( RunningStatus ) ) );


    }
}
