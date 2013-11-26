﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Yodii.Lab
{
    public class DropDownButton : ToggleButton
    {
        // *** Dependency Properties ***


        public static readonly DependencyProperty DropDownProperty = DependencyProperty.Register( "DropDown", typeof( ContextMenu ), typeof( DropDownButton ), new UIPropertyMetadata( null ) );

        // *** Constructors *** 

        public DropDownButton()
        {
            // Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property 

            Binding binding = new Binding( "DropDown.IsOpen" );
            binding.Source = this;
            this.SetBinding( IsCheckedProperty, binding );
        }

        // *** Properties *** 

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

        // *** Overridden Methods *** 

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
