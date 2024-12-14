using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows;

namespace PicaPico
{
    public class AcrylicHelper
    {
        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        [Flags]
        internal enum DWMWINDOWATTRIBUTE
        {
            DWMWA_SYSTEMBACKDROP_TYPE = 38,
            DWMWA_MICA_EFFECT = 1029
        }

        [Flags]
        internal enum DWMSBT : uint
        {
            DWMSBT_AUTO = 0,
            DWMSBT_DISABLE = 1,
            DWMSBT_MAINWINDOW = 2,
            DWMSBT_TRANSIENTWINDOW = 3,
            DWMSBT_TABBEDWINDOW = 4
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmSetWindowAttribute(
            [In] IntPtr hWnd,
            [In] DWMWINDOWATTRIBUTE dwAttribute,
            [In] ref int pvAttribute,
            [In] int cbAttribute
        );

        static AcrylicHelper()
        {
            ThemeListener.ThemeChanged += ThemeListener_ThemeChanged;
        }

        private static HashSet<Window> _enabledWindows = new HashSet<Window>();
        public static void Apply(Window window, FrameworkElement dragHost = null, bool responseDeactivated = true)
        {
            if (_enabledWindows.Contains(window))
            {
                return;
            }
            _enabledWindows.Add(window);
            try
            {
                window.WindowStyle = WindowStyle.None;
                window.AllowsTransparency = true;
            }
            catch (Exception) { }
            if (dragHost != null)
            {
                dragHost.Visibility = Visibility.Visible;
                dragHost.PreviewMouseLeftButtonDown += DragWindow;
            }
            if (responseDeactivated)
            {
                window.Activated += WindowActivated;
                window.Deactivated += WindowDeactivated;
            }
            window.SourceInitialized += Window_SourceInitialized;
            window.Closed += Window_Closed;
        }

        private static void Window_Closed(object sender, EventArgs e)
        {
            Window window = sender as Window;
            if (_enabledWindows.Contains(window))
            {
                _enabledWindows.Remove(window);
            }
        }

        private static void ThemeListener_ThemeChanged(bool isDark)
        {
            foreach (var window in _enabledWindows)
            {
                if (window.IsActive)
                {
                    WindowActivated(window, null);
                }
                else
                {
                    WindowDeactivated(window, null);
                }
            }
        }

        private static void Window_SourceInitialized(object sender, EventArgs e)
        {
            var windowHandle = new WindowInteropHelper((Window)sender).Handle;
            if (windowHandle == IntPtr.Zero)
            {
                return;
            }

            RemoveBackdrop(windowHandle);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHandle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private static void RemoveBackdrop(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            var pvAttribute = 0; // Disable
            var backdropPvAttribute = (int)DWMSBT.DWMSBT_DISABLE;

            _ = DwmSetWindowAttribute(
                hWnd,
                DWMWINDOWATTRIBUTE.DWMWA_MICA_EFFECT,
                ref pvAttribute,
                Marshal.SizeOf(typeof(int))
            );

            _ = DwmSetWindowAttribute(
                hWnd,
                DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                ref backdropPvAttribute,
                Marshal.SizeOf(typeof(int))
            );
        }

        private static void DragWindow(object sender, MouseButtonEventArgs e)
        {
            Window win = (Window)sender;
            e.Handled = true;
            win.DragMove();
        }

        public static Brush BackgroundDarkA { get; set; } = new SolidColorBrush(Color.FromArgb(0xA0, 0x1F, 0x1F, 0x1F));
        public static Brush BackgroundLightA { get; set; } = new SolidColorBrush(Color.FromArgb(0xA0, 0xFF, 0xFF, 0xFF));
        public static Brush BackgroundDark { get; set; } = new SolidColorBrush(Color.FromRgb(0x1F, 0x1F, 0x1F));
        public static Brush BackgroundLight { get; set; } = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));

        private static void WindowActivated(object sender, EventArgs e)
        {
            Window win = (Window)sender;
            win.Background = ThemeListener.IsDarkMode ? BackgroundDarkA : BackgroundLightA;
        }

        private static void WindowDeactivated(object sender, EventArgs e)
        {
            Window win = (Window)sender;
            win.Background = ThemeListener.IsDarkMode ? BackgroundDark : BackgroundLight;
        }
    }
}
