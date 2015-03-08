#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Utils\DropDownButton.cs) is part of CiviKey. 
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
