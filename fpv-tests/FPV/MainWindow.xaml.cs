using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FPV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int SWP_SHOWWINDOW = 0x0040;       // Flag to show the window
        private const int SWP_FRAMECHANGED = 0x0020;     // Flag to indicate that the window frame has changed
        private const int SWP_NOSIZE = 0x0001;           // Flag to indicate that window size should not be changed
        private const int SWP_NOMOVE = 0x0002;           // Flag to indicate that window position should not be changed

        private const int GWL_STYLE = -16;               // Constant for getting/setting window styles
        private const uint WS_POPUP = 0x80000000;        // Constant for removing window frame
        private const int WS_VISIBLE = 0x10000000;       // Constant for showing window

        private bool isMaximized;                        // Indicates if the window is maximized
        private double previousHeight;                   // Previous window height

        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Remove window styles for frame display
            IntPtr handle = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(handle, GWL_STYLE);
            style &= unchecked((int)(WS_POPUP | WS_VISIBLE));
            SetWindowLong(handle, GWL_STYLE, style);

            // Set the window to normal state
            WindowState = WindowState.Normal;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();

                }
                catch
                {
                    //Leave this empty
                }
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();

                }
                catch
                {
                    //Leave this empty
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized && !isMaximized)
            {
                // If the window has transitioned to maximized state
                isMaximized = true;
                MaximizeWindowWithoutCoveringTaskbar();
            }
            else if (WindowState == WindowState.Normal)
            {
                // If the window has transitioned to normal state
                isMaximized = false;
            }
        }

        private void MaximizeWindowWithoutCoveringTaskbar()
        {
            // Get the taskbar height
            double taskbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height;

            // Set the maximum window height to avoid covering the taskbar
            previousHeight = Height;
            MaxHeight = SystemParameters.PrimaryScreenHeight - taskbarHeight;
        }

    }
}
