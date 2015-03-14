using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Yodii.Wpf.Win32
{

    public static class WindowExtensions
    {
        #region Win32 imports

        // See also: GetWindowLong (GWL)
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms633584(v=vs.85).aspx
        private const int GWL_STYLE = -16;

        // See also: Window Styles (WS)
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms632600(v=vs.85).aspx
        private const int WS_SYSMENU = 0x80000;

        // See also: EnableMenuItem function
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647636(v=vs.85).aspx
        private const int MF_BYCOMMAND = 0x00000000;
        private const int MF_BYPOSITION = 0x00000400;
        private const int MF_DISABLED = 0x00000002;
        private const int MF_ENABLED = 0x00000000;
        private const int MF_GRAYED = 0x00000001;

        // See also: WM_SYSCOMMAND message (SC)
        private const int SC_CLOSE = 0xF060;

        // See also: MENUITEMINFO structure
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647578(v=vs.85).aspx
        private const uint MIIM_STATE = 0x00000001;
        private const uint MFS_ENABLED = 0x00000000;
        private const uint MFS_DISABLED = 0x00000003;

        /// <summary>
        /// Retrieves information about the specified window.
        /// The function also retrieves the 32-bit (DWORD) value at the specified offset
        /// into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved. Valid values are in the range zero through the number of bytes of extra window memory, minus four; for example, if you specified 12 or more bytes of extra memory, a value of 8 would be an index to the third 32-bit integer.</param>
        /// <returns>If the function succeeds, the return value is the requested value.
        /// If the function fails, the return value is zero.
        /// To get extended error information, call GetLastError.
        /// If SetWindowLong has not been called previously, GetWindowLong returns zero for values
        /// in the extra window or class memory.</returns>
        /// <remarks>See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms633584(v=vs.85).aspx</remarks>
        [DllImport( "user32.dll", SetLastError = true )]
        private static extern int GetWindowLong( IntPtr hWnd, int nIndex );

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets the 32-bit (long) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of an integer. Has special values.</param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified 32-bit integer.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// If the previous value of the specified 32-bit integer is zero, and the function succeeds,
        /// the return value is zero, but the function does not clear the last error information.
        /// This makes it difficult to determine success or failure. To deal with this,
        /// you should clear the last error information by calling SetLastError with 0
        /// before calling SetWindowLong.
        /// Then, function failure will be indicated by a return value of zero and a GetLastError result
        /// that is nonzero.</returns>
        /// <remarks>
        /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms633591(v=vs.85).aspx
        /// </remarks>
        [DllImport( "user32.dll", SetLastError = true )]
        private static extern int SetWindowLong( IntPtr hWnd, int nIndex, int dwNewLong );

        /// <summary>
        /// </summary>
        /// <param name="hMenu"></param>
        /// <param name="uIDEnableItem"></param>
        /// <param name="uEnable"></param>
        /// <returns></returns>
        /// <remarks>
        /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms647636(v=vs.85).aspx
        /// </remarks>
        [DllImport( "user32.dll", SetLastError = true )]
        private static extern bool EnableMenuItem( IntPtr hMenu, uint uIDEnableItem, uint uEnable );

        /// <summary>
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="bRevert"></param>
        /// <returns></returns>
        /// <remarks>
        /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms647985(v=vs.85).aspx
        /// </remarks>
        [DllImport( "user32.dll", SetLastError = true )]
        private static extern IntPtr GetSystemMenu( IntPtr hWnd, bool bRevert );

        /// <summary>
        /// </summary>
        /// <param name="hMenu"></param>
        /// <param name="uItem"></param>
        /// <param name="fByPosition"></param>
        /// <param name="lpmii"></param>
        /// <returns></returns>
        /// <remarks>
        /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms647980(v=vs.85).aspx
        /// </remarks>
        [DllImport( "user32.dll", SetLastError = true )]
        private static extern bool GetMenuItemInfo( IntPtr hMenu, uint uItem, bool fByPosition, ref MENUITEMINFO lpmii );

        #endregion

        /// <summary>
        /// Hides the system menu from the Window by removing the WS_SYSMENU flag using the Win32 API.
        /// </summary>
        /// <param name="w">The window to change.</param>
        public static void HideSysMenu( this Window w )
        {
            IntPtr hwnd = new WindowInteropHelper( w ).Handle;
            int v = SetWindowLong( hwnd, GWL_STYLE, GetWindowLong( hwnd, GWL_STYLE ) & ~WS_SYSMENU );
            if( v == 0 ) { throw new Win32Exception( Marshal.GetLastWin32Error() ); }
        }

        /// <summary>
        /// Shows the system menu in the Window by setting the WS_SYSMENU flag using the Win32 API.
        /// </summary>
        /// <param name="w">The window to change.</param>
        public static void ShowSysMenu( this Window w )
        {
            IntPtr hwnd = new WindowInteropHelper( w ).Handle;
            int v = SetWindowLong( hwnd, GWL_STYLE, GetWindowLong( hwnd, GWL_STYLE ) | WS_SYSMENU );
            if( v == 0 ) { throw new Win32Exception( Marshal.GetLastWin32Error() ); }
        }

        public static bool IsSysMenuEnabled( this Window w )
        {
            IntPtr hwnd = new WindowInteropHelper( w ).Handle;
            int lng = GetWindowLong( hwnd, GWL_STYLE );
            if( lng == 0 ) { throw new Win32Exception( Marshal.GetLastWin32Error() ); }

            return (lng & WS_SYSMENU) == WS_SYSMENU;
        }

        public static void DisableCloseButton( this Window w )
        {
            IntPtr hwnd = new WindowInteropHelper( w ).Handle;
            IntPtr hmnu = GetSystemMenu( hwnd, false );

            EnableMenuItem( hmnu, SC_CLOSE, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED );
        }

        public static void EnableCloseButton( this Window w )
        {
            IntPtr hwnd = new WindowInteropHelper( w ).Handle;
            IntPtr hmnu = GetSystemMenu( hwnd, false );

            EnableMenuItem( hmnu, SC_CLOSE, MF_BYCOMMAND | MF_ENABLED );
        }

        public static bool IsCloseButtonDisabled( this Window w )
        {
            IntPtr hwnd = new WindowInteropHelper( w ).Handle;

            IntPtr hMenu = GetSystemMenu( hwnd, false );

            MENUITEMINFO mif = new MENUITEMINFO();
            mif.cbSize = MENUITEMINFO.sizeOf;
            mif.fMask = MIIM_STATE;
            mif.fType = 0;
            mif.dwTypeData = null;

            bool a = GetMenuItemInfo( hMenu, SC_CLOSE, false, ref mif );
            if( a == false )
            {
                int e = Marshal.GetLastWin32Error();
                throw new Win32Exception( e );
            }

            return (mif.fState & MFS_DISABLED) == MFS_DISABLED;
        }

    }


    // See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms647578(v=vs.85).aspx
    [StructLayout( LayoutKind.Sequential )]
    struct MENUITEMINFO
    {
        public uint cbSize;
        public uint fMask;
        public uint fType;
        public uint fState;
        public uint wID;
        public IntPtr hSubMenu;
        public IntPtr hbmpChecked;
        public IntPtr hbmpUnchecked;
        public IntPtr dwItemData;
        public string dwTypeData;
        public uint cch;
        public IntPtr hbmpItem;

        public static uint sizeOf
        {
            get { return (uint)Marshal.SizeOf( typeof( MENUITEMINFO ) ); }
        }
    }
}
