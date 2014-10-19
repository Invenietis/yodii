#region LGPL License
/*----------------------------------------------------------------------------
* This file (SharedAssemblyInfo.cs) is part of Yodii. 
*  
* Yodii is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with Yodii. If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2013, Invenietis <http://www.invenietis.com>.
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Reflection;

[assembly: AssemblyCompany( "Invenietis" )]
[assembly: AssemblyProduct( "Yodii" )]
[assembly: AssemblyCopyright( "Copyright © Invenietis 2013" )]
[assembly: AssemblyTrademark( "" )]
//[assembly: CLSCompliant( true )] //TODO : fix this
[assembly: AssemblyVersion( "0.0.2" )]


#if DEBUG
    [assembly: AssemblyConfiguration("Debug")]
#else
    [assembly: AssemblyConfiguration( "Release" )]
#endif

// Added by CKReleaser.
[assembly: AssemblyInformationalVersion( "%ck-standard%" )]
