using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Yodii.Lab
{
    /// <summary>
    /// DropDown button. Spawns a menu on click.
    /// </summary>
    public class DropDownButton : ToggleButton
    {
        /// <summary>
        /// DropDownProperty
        /// </summary>
        public static readonly DependencyProperty DropDownProperty = DependencyProperty.Register( "DropDown", typeof( ContextMenu ), typeof( DropDownButton ), new UIPropertyMetadata( null ) );

        /// <summary>
        /// Creates a nex DropDownButton.
        /// </summary>
        public DropDownButton()
        {
            // Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property 

            Binding binding = new Binding( "DropDown.IsOpen" );
            binding.Source = this;
            this.SetBinding( IsCheckedProperty, binding );
        }

        /// <summary>
        /// DropDown menu, shown on click.
        /// </summary>
        public ContextMenu DropDown
        {
            get
            {
                return (ContextMenu)GetValue( DropDownProperty );
            }
            set
            {
                SetValue( DropDownProperty, value );
            }
        }

        /// <summary>
        /// Overriden OnClick().
        /// </summary>
        protected override void OnClick()
        {
            if( DropDown != null )
            {
                // If there is a drop-down assigned to this button, then position and display it 

                DropDown.PlacementTarget = this;
                DropDown.Placement = PlacementMode.Bottom;

                DropDown.IsOpen = true;
            }
        }
    }
}
