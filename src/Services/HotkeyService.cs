using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace VirtualDesktopIndicator.Services
{
    public class HotkeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private const int MOD_ALT = 0x0001;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int MOD_WIN = 0x0008;

        private readonly IntPtr _windowHandle;
        private readonly VirtualDesktopService _desktopService;
        private bool _disposed = false;

        private const int HOTKEY_NEXT_DESKTOP = 1;

        public HotkeyService(VirtualDesktopService desktopService)
        {
            _desktopService = desktopService ?? throw new ArgumentNullException(nameof(desktopService));
            
            var hiddenWindow = new HiddenWindow();
            _windowHandle = hiddenWindow.Handle;
            hiddenWindow.HotkeyPressed += OnHotkeyPressed;
            
            RegisterHotkeys();
        }

        private void RegisterHotkeys()
        {
            try
            {
                // SECURITY: Check Win32 API return value for proper error handling
                bool success = RegisterHotKey(_windowHandle, HOTKEY_NEXT_DESKTOP, MOD_ALT, (int)Keys.Oemtilde);
                if (!success)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    string errorMessage = errorCode switch
                    {
                        1409 => "Hotkey already registered by another application",
                        1400 => "Invalid window handle",
                        87 => "Invalid parameter",
                        _ => $"Win32 error code: {errorCode}"
                    };
                    DebugLogger.WriteSecurityInfo($"Hotkey registration failed: {errorMessage}");
                }
                else
                {
                    DebugLogger.WriteLine("Successfully registered hotkey Alt+`");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception during hotkey registration: {ex.Message}");
            }
        }

        private void OnHotkeyPressed(int hotkeyId)
        {
            try
            {
                switch (hotkeyId)
                {
                    case HOTKEY_NEXT_DESKTOP:
                        _desktopService.SwitchToNextDesktop();
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling hotkey {hotkeyId}: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                UnregisterHotkeys();
                _disposed = true;
            }
        }

        private void UnregisterHotkeys()
        {
            try
            {
                // SECURITY: Check Win32 API return value for proper cleanup
                bool success = UnregisterHotKey(_windowHandle, HOTKEY_NEXT_DESKTOP);
                if (!success)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    string errorMessage = errorCode switch
                    {
                        1419 => "Hotkey not registered",
                        1400 => "Invalid window handle",
                        _ => $"Win32 error code: {errorCode}"
                    };
                    System.Diagnostics.Debug.WriteLine($"Failed to unregister hotkey: {errorMessage}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Successfully unregistered hotkey");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception during hotkey unregistration: {ex.Message}");
            }
        }

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private class HiddenWindow : NativeWindow
        {
            public event Action<int>? HotkeyPressed;

            public HiddenWindow()
            {
                CreateHandle(new CreateParams());
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    var hotkeyId = m.WParam.ToInt32();
                    HotkeyPressed?.Invoke(hotkeyId);
                }
                else
                {
                    base.WndProc(ref m);
                }
            }
        }
    }
}