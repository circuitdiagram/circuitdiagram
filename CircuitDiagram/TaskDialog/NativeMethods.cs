using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TaskDialogInterop
{
	internal class NativeMethods
	{
		internal const int GWL_STYLE = -16;
		internal const int GWL_EXSTYLE = -20;
		internal const int SWP_NOSIZE = 0x0001;
		internal const int SWP_NOMOVE = 0x0002;
		internal const int SWP_NOZORDER = 0x0004;
		internal const int SWP_FRAMECHANGED = 0x0020;
		internal const uint WM_SETICON = 0x0080;
		internal const int WS_SYSMENU = 0x00080000;
		internal const int WS_EX_DLGMODALFRAME = 0x0001;

		[DllImport("user32.dll")]
		internal extern static int SetWindowLong(IntPtr hwnd, int index, int value);
		[DllImport("user32.dll")]
		internal extern static int GetWindowLong(IntPtr hwnd, int index);
		[DllImport("user32.dll")]
		internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);
		[DllImport("user32.dll")]
		internal static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll")]
		internal static extern IntPtr DefWindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
	}
}
