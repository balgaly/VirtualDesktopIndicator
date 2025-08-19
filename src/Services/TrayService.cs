using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using VirtualDesktopIndicator.Views;

namespace VirtualDesktopIndicator.Services
{
    public class TrayService : IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private VirtualDesktopService? _desktopService;
        private HotkeyService? _hotkeyService;
        private DesktopIndicatorWindow? _indicatorWindow;
        private int _currentDesktop = 1;
        private ThemeMode _themeMode = ThemeMode.FollowSystem;
        private System.Windows.Threading.DispatcherTimer? _hideIndicatorTimer;

        public void Initialize()
        {
            _desktopService = new VirtualDesktopService();
            _desktopService.DesktopChanged += OnDesktopChanged;
            _desktopService.DesktopCountChanged += OnDesktopCountChanged;
            _desktopService.StartMonitoring();

            _hotkeyService = new HotkeyService(_desktopService);
            _indicatorWindow = new DesktopIndicatorWindow();
            
            InitializeTrayIcon();
            InitializeHideTimer();
            
            _currentDesktop = _desktopService.GetCurrentDesktopNumber();
            System.Diagnostics.Debug.WriteLine($"Initial desktop detected: {_currentDesktop}");
            UpdateTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Visible = true,
                Text = "Virtual Desktop Indicator",
                ContextMenuStrip = CreateContextMenu()
            };

            _notifyIcon.MouseClick += OnTrayIconClick;
            _notifyIcon.MouseDoubleClick += OnTrayIconDoubleClick;
        }

        private void InitializeHideTimer()
        {
            _hideIndicatorTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(2000)
            };
            _hideIndicatorTimer.Tick += (s, e) =>
            {
                _hideIndicatorTimer.Stop();
                _indicatorWindow?.Hide();
            };
        }

        private void OnDesktopChanged(object? sender, int desktopNumber)
        {
            _currentDesktop = desktopNumber;
            UpdateTrayIcon();
            ShowDesktopIndicator(desktopNumber);
            
            if (_notifyIcon?.ContextMenuStrip != null)
            {
                _notifyIcon.ContextMenuStrip.Dispose();
                _notifyIcon.ContextMenuStrip = CreateContextMenu();
            }
        }

        private void OnDesktopCountChanged(object? sender, int desktopCount)
        {
            if (_notifyIcon?.ContextMenuStrip != null)
            {
                _notifyIcon.ContextMenuStrip.Dispose();
                _notifyIcon.ContextMenuStrip = CreateContextMenu();
            }
        }

        private void ShowDesktopIndicator(int desktopNumber)
        {
            var isDarkTheme = ThemeService.ShouldUseDarkTheme(_themeMode);
            var desktopName = _desktopService?.GetDesktopName(desktopNumber) ?? $"Desktop {desktopNumber}";
            _indicatorWindow?.ShowDesktop(desktopNumber, desktopName, isDarkTheme);
            
            _hideIndicatorTimer?.Stop();
            _hideIndicatorTimer?.Start();
        }

        private void UpdateTrayIcon()
        {
            if (_notifyIcon == null) return;

            var isDarkTheme = ThemeService.ShouldUseDarkTheme(_themeMode);
            var icon = CreateTrayIcon(_currentDesktop, isDarkTheme);
            _notifyIcon.Icon?.Dispose();
            _notifyIcon.Icon = icon;
            
            var desktopName = _desktopService?.GetDesktopName(_currentDesktop) ?? $"Desktop {_currentDesktop}";
            _notifyIcon.Text = desktopName;
        }

        private Icon CreateTrayIcon(int desktopNumber, bool darkTheme = false)
        {
            const int iconSize = 16;
            using var bitmap = new Bitmap(iconSize, iconSize, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            graphics.Clear(Color.Transparent);
            var backgroundColor = darkTheme ? Color.FromArgb(30, 41, 59) : Color.FromArgb(37, 99, 235);
            var textColor = Color.White;
            var borderColor = darkTheme ? Color.FromArgb(51, 65, 85) : Color.FromArgb(29, 78, 216);

            using var backgroundBrush = new SolidBrush(backgroundColor);
            using var borderPen = new Pen(borderColor, 1);
            
            var rect = new Rectangle(1, 1, iconSize - 2, iconSize - 2);
            var cornerRadius = 3;
            
            using var path = CreateRoundedRectanglePath(rect, cornerRadius);
            graphics.FillPath(backgroundBrush, path);
            graphics.DrawPath(borderPen, path);

            var displayText = desktopNumber <= 9 ? desktopNumber.ToString() : (desktopNumber == 10 ? "0" : "*");
            using var font = new Font("Segoe UI", 11, FontStyle.Bold, GraphicsUnit.Pixel);
            using var textBrush = new SolidBrush(textColor);
            
            using var stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.FormatFlags = StringFormatFlags.NoWrap;
            
            var textRect = new RectangleF(0, 0, iconSize, iconSize);
            graphics.DrawString(displayText, font, textBrush, textRect, stringFormat);

            // SECURITY: Properly manage GDI resources to prevent resource leaks
            return CreateIconWithProperCleanup(bitmap);
        }

        private Icon CreateIconWithProperCleanup(Bitmap bitmap)
        {
            IntPtr hIcon = IntPtr.Zero;
            try
            {
                hIcon = bitmap.GetHicon();
                var icon = Icon.FromHandle(hIcon);
                var clonedIcon = (Icon)icon.Clone();
                
                return clonedIcon;
            }
            finally
            {
                // CRITICAL: Always destroy the native handle to prevent GDI leaks
                if (hIcon != IntPtr.Zero)
                {
                    // SECURITY: Check DestroyIcon return value for proper error handling
                    bool success = DestroyIcon(hIcon);
                    if (!success)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        DebugLogger.WriteSecurityInfo($"DestroyIcon failed: Win32 error {errorCode}");
                    }
                }
            }
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            var path = new GraphicsPath();
            var diameter = cornerRadius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            
            contextMenu.Font = new Font("Segoe UI Variable", 9F);
            contextMenu.BackColor = Color.FromArgb(252, 252, 253);
            contextMenu.ForeColor = Color.FromArgb(15, 15, 15);
            contextMenu.Padding = new Padding(0, 6, 0, 6); // More compact padding
            contextMenu.Margin = new Padding(0);
            contextMenu.DropShadowEnabled = true;
            contextMenu.RenderMode = ToolStripRenderMode.Professional;

            var currentDesktop = _desktopService?.GetCurrentDesktopNumber() ?? 1;
            var desktopCount = _desktopService?.GetDesktopCount() ?? 1;
            for (int i = 1; i <= desktopCount; i++)
            {
                var isCurrentDesktop = i == currentDesktop;
                var desktopName = _desktopService?.GetDesktopName(i);
                
                var displayName = "";
                if (!string.IsNullOrEmpty(desktopName) && desktopName != $"Desktop {i}")
                {
                    displayName = $": {desktopName}";
                }
                
                var indicator = isCurrentDesktop ? "●" : "○";
                var displayText = $"  {indicator}  {i}{displayName}";
                
                var desktopLabel = new ToolStripLabel(displayText)
                {
                    Font = new Font("Segoe UI Variable", 9F, isCurrentDesktop ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isCurrentDesktop ? Color.FromArgb(16, 185, 129) : Color.FromArgb(75, 85, 99),
                    Padding = new Padding(12, 3, 12, 3), // Compact padding
                    BackColor = isCurrentDesktop ? Color.FromArgb(240, 253, 244) : Color.Transparent,
                    Margin = new Padding(0, 0, 0, 0)
                };
                
                contextMenu.Items.Add(desktopLabel);
            }

            var divider = new ToolStripSeparator() { BackColor = Color.FromArgb(226, 232, 240) };
            contextMenu.Items.Add(divider);
            var themeMenu = new ToolStripMenuItem("🎨  Theme")
            {
                Font = new Font("Segoe UI Variable", 9F),
                Padding = new Padding(12, 4, 12, 4),
                ForeColor = Color.FromArgb(15, 15, 15)
            };

            var followSystemItem = new ToolStripMenuItem("Follow System")
            {
                Font = new Font("Segoe UI Variable", 9F),
                Padding = new Padding(8, 3, 8, 3),
                Checked = _themeMode == ThemeMode.FollowSystem
            };
            followSystemItem.Click += (s, e) => SetThemeMode(ThemeMode.FollowSystem);
            themeMenu.DropDownItems.Add(followSystemItem);

            var lightThemeItem = new ToolStripMenuItem("Light")
            {
                Font = new Font("Segoe UI Variable", 9F),
                Padding = new Padding(8, 3, 8, 3),
                Checked = _themeMode == ThemeMode.Light
            };
            lightThemeItem.Click += (s, e) => SetThemeMode(ThemeMode.Light);
            themeMenu.DropDownItems.Add(lightThemeItem);

            var darkThemeItem = new ToolStripMenuItem("Dark")
            {
                Font = new Font("Segoe UI Variable", 9F),
                Padding = new Padding(8, 3, 8, 3),
                Checked = _themeMode == ThemeMode.Dark
            };
            darkThemeItem.Click += (s, e) => SetThemeMode(ThemeMode.Dark);
            themeMenu.DropDownItems.Add(darkThemeItem);

            contextMenu.Items.Add(themeMenu);

            var aboutItem = new ToolStripMenuItem("About")
            {
                Font = new Font("Segoe UI Variable", 9F),
                Padding = new Padding(12, 4, 12, 4),
                ForeColor = Color.FromArgb(15, 15, 15)
            };
            aboutItem.Click += (s, e) => ShowAbout();
            contextMenu.Items.Add(aboutItem);

            var exitItem = new ToolStripMenuItem("Exit")
            {
                Font = new Font("Segoe UI Variable", 9F),
                Padding = new Padding(12, 4, 12, 4),
                ForeColor = Color.FromArgb(185, 28, 28)
            };
            exitItem.Click += (s, e) => ExitApplication();
            contextMenu.Items.Add(exitItem);

            return contextMenu;
        }

        private void OnTrayIconClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                System.Diagnostics.Debug.WriteLine("Tray icon left-click: switching to next desktop");
                _desktopService?.SwitchToNextDesktop();
            }
        }

        private void OnTrayIconDoubleClick(object? sender, MouseEventArgs e)
        {
            ShowDesktopIndicator(_currentDesktop);
        }

        private void SetThemeMode(ThemeMode themeMode)
        {
            _themeMode = themeMode;
            UpdateTrayIcon();
            
            if (_notifyIcon?.ContextMenuStrip != null)
            {
                _notifyIcon.ContextMenuStrip.Dispose();
                _notifyIcon.ContextMenuStrip = CreateContextMenu();
            }
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "VirtualDesktopIndicator v1.0.0\n\n" +
                "A modern, free alternative to paid virtual desktop tools for Windows.\n\n" +
                "Created by: Snir Balgaly\n" +
                "Developed with assistance from Claude AI (Anthropic)\n\n" +
                "Features:\n" +
                "• System tray desktop indicator with custom names\n" +
                "• Desktop switching popup with animations\n" +
                "• Follow system theme / Light / Dark modes\n" +
                "• Real-time desktop detection\n" +
                "• Dynamic desktop count support\n\n" +
                "Open source under MIT License\n" +
                "GitHub: github.com/balgaly/VirtualDesktopIndicator",
                "About VirtualDesktopIndicator",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void ExitApplication()
        {
            System.Windows.Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _hideIndicatorTimer?.Stop();
            _hideIndicatorTimer = null;
            
            _hotkeyService?.Dispose();
            _hotkeyService = null;
            
            _notifyIcon?.Dispose();
            _notifyIcon = null;
            
            _indicatorWindow?.Close();
            _indicatorWindow = null;
            
            if (_desktopService != null)
            {
                _desktopService.DesktopChanged -= OnDesktopChanged;
                _desktopService.DesktopCountChanged -= OnDesktopCountChanged;
                _desktopService.StopMonitoring();
                _desktopService = null;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}