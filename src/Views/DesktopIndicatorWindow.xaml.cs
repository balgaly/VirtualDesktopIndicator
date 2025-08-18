using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VirtualDesktopIndicator.Services;
using Color = System.Windows.Media.Color;

namespace VirtualDesktopIndicator.Views
{
    public partial class DesktopIndicatorWindow : Window
    {
        private Storyboard? _fadeInAnimation;
        private Storyboard? _fadeOutAnimation;

        public DesktopIndicatorWindow()
        {
            InitializeComponent();
            InitializeAnimations();
            PositionWindow();
            
            // Start hidden
            Opacity = 0;
            Visibility = Visibility.Hidden;
        }

        private void InitializeAnimations()
        {
            _fadeInAnimation = (Storyboard)FindResource("FadeInAnimation");
            _fadeOutAnimation = (Storyboard)FindResource("FadeOutAnimation");

            if (_fadeOutAnimation != null)
            {
                _fadeOutAnimation.Completed += (s, e) => Visibility = Visibility.Hidden;
            }
        }

        private void PositionWindow()
        {
            try
            {
                // SECURITY: Safely get primary screen with null check
                var screen = System.Windows.Forms.Screen.PrimaryScreen;
                if (screen == null)
                {
                    // Fallback: Use system metrics if PrimaryScreen is null
                    var screenWidth = SystemParameters.PrimaryScreenWidth;
                    var screenHeight = SystemParameters.PrimaryScreenHeight;
                    
                    Left = screenWidth - Width - 20;
                    Top = 20;
                    return;
                }
                
                var workingArea = screen.WorkingArea;
                
                // Convert to WPF coordinates with null safety
                var dpiScale = VisualTreeHelper.GetDpi(this);
                var scaledWidth = Width * 96.0 / dpiScale.DpiScaleX;
                var scaledHeight = Height * 96.0 / dpiScale.DpiScaleY;
                
                Left = workingArea.Right / dpiScale.DpiScaleX - scaledWidth - 20;
                Top = workingArea.Top / dpiScale.DpiScaleY + 20;
            }
            catch (Exception ex)
            {
                // Fallback positioning if any error occurs
                System.Diagnostics.Debug.WriteLine($"Error positioning window: {ex.Message}");
                Left = SystemParameters.PrimaryScreenWidth - Width - 20;
                Top = 20;
            }
        }

        public void ShowDesktop(int desktopNumber, string desktopName, bool isDarkTheme = false)
        {
            // Update content - only show desktop name
            DesktopLabelText.Text = desktopName;

            // Apply theme
            ApplyTheme(isDarkTheme);

            // Show with animation
            Visibility = Visibility.Visible;
            _fadeInAnimation?.Begin(this);
        }

        public new void Hide()
        {
            _fadeOutAnimation?.Begin(this);
        }

        private void ApplyTheme(bool isDarkTheme)
        {
            if (isDarkTheme)
            {
                // Dark theme colors
                BackgroundBrush.Color = Color.FromRgb(30, 41, 59);  // slate-800
                DesktopNumberText.Foreground = new SolidColorBrush(Color.FromRgb(241, 245, 249)); // slate-100
                DesktopLabelText.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)); // slate-400
                AccentRing.Stroke = new SolidColorBrush(Color.FromRgb(59, 130, 246)); // blue-500
            }
            else
            {
                // Light theme colors
                BackgroundBrush.Color = Color.FromRgb(248, 250, 252); // slate-50
                DesktopNumberText.Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)); // slate-800
                DesktopLabelText.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)); // slate-500
                AccentRing.Stroke = new SolidColorBrush(Color.FromRgb(37, 99, 235)); // blue-600
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Make window click-through
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            WindowsApi.SetWindowExTransparent(hwnd);
        }
    }

    internal static class WindowsApi
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            try
            {
                // SECURITY: Validate window handle before Win32 calls
                if (hwnd == IntPtr.Zero)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid window handle for transparency");
                    return;
                }

                // Get current extended style with error checking
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                if (extendedStyle == 0)
                {
                    int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    if (errorCode != 0)
                    {
                        DebugLogger.WriteSecurityInfo($"GetWindowLong failed: Win32 error {errorCode}");
                        return;
                    }
                }

                // Set new extended style with error checking
                var newStyle = extendedStyle | WS_EX_TRANSPARENT;
                var result = SetWindowLong(hwnd, GWL_EXSTYLE, newStyle);
                if (result == 0)
                {
                    int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    if (errorCode != 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"SetWindowLong failed: Win32 error {errorCode}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Successfully set window transparency");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception setting window transparency: {ex.Message}");
            }
        }
    }
}