using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;

namespace VirtualDesktopIndicator.Services
{
    public class VirtualDesktopService
    {
        private int _currentDesktop = 1;
        private int _desktopCount = 1;
        private Dictionary<int, string> _desktopNames = new();
        private System.Windows.Threading.DispatcherTimer? _monitorTimer;

        public event EventHandler<int>? DesktopChanged;
        public event EventHandler<int>? DesktopCountChanged;

        public VirtualDesktopService()
        {
            UpdateDesktopInfo();
        }

        public int GetCurrentDesktopNumber()
        {
            UpdateDesktopInfo();
            return _currentDesktop;
        }

        public int GetDesktopCount()
        {
            UpdateDesktopInfo();
            return _desktopCount;
        }

        public string GetDesktopName(int desktopNumber)
        {
            return _desktopNames.TryGetValue(desktopNumber, out string? name) ? name : $"Desktop {desktopNumber}";
        }

        private void UpdateDesktopInfo()
        {
            try
            {
                var (current, count) = GetDesktopInfoFromRegistry();
                
                if (current == 0 || count == 0)
                {
                    (current, count) = (1, 1); // Safe fallback - assume single desktop
                }

                UpdateDesktopNames(count);

                bool desktopChanged = current != _currentDesktop;
                bool countChanged = count != _desktopCount;

                _currentDesktop = current;
                _desktopCount = count;

                if (desktopChanged)
                {
                    DesktopChanged?.Invoke(this, _currentDesktop);
                }

                if (countChanged)
                {
                    DesktopCountChanged?.Invoke(this, _desktopCount);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating desktop info: {ex.Message}");
            }
        }

        private (int current, int count) GetDesktopInfoFromRegistry()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops");
                if (key == null) 
                {
                    Debug.WriteLine("VirtualDesktops registry key not found");
                    return (1, 1);
                }

                var allDesktopsData = key.GetValue("VirtualDesktopIDs") as byte[];
                if (allDesktopsData == null) 
                {
                    Debug.WriteLine("VirtualDesktopIDs not found in registry");
                    return (1, 1);
                }

                var desktopCount = allDesktopsData.Length / 16;
                Debug.WriteLine($"Found {desktopCount} desktops in registry");

                var currentDesktopData = key.GetValue("CurrentVirtualDesktop") as byte[];
                if (currentDesktopData == null) 
                {
                    Debug.WriteLine("CurrentVirtualDesktop not found, assuming desktop 1");
                    return (1, desktopCount);
                }

                var currentDesktopGuid = new Guid(currentDesktopData);
                DebugLogger.WriteSecurityInfo($"Current desktop GUID detected");

                var currentDesktopIndex = 1;
                for (int i = 0; i < desktopCount; i++)
                {
                    var guidBytes = new byte[16];
                    Array.Copy(allDesktopsData, i * 16, guidBytes, 0, 16);
                    var guid = new Guid(guidBytes);
                    
                    DebugLogger.WriteSecurityInfo($"Desktop {i + 1} GUID processed");
                    
                    if (guid.Equals(currentDesktopGuid))
                    {
                        currentDesktopIndex = i + 1;
                        Debug.WriteLine($"Found current desktop at index {currentDesktopIndex}");
                        break;
                    }
                }

                return (currentDesktopIndex, desktopCount);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registry method failed: {ex.Message}");
                return (0, 0);
            }
        }


        private void UpdateDesktopNames(int desktopCount)
        {
            try
            {
                _desktopNames.Clear();
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops");
                var allDesktopsData = key?.GetValue("VirtualDesktopIDs") as byte[];
                
                if (allDesktopsData != null && allDesktopsData.Length >= desktopCount * 16)
                {
                    Debug.WriteLine("Getting desktop names from Desktops subkey");
                    
                    for (int i = 0; i < desktopCount; i++)
                    {
                        var guidBytes = new byte[16];
                        Array.Copy(allDesktopsData, i * 16, guidBytes, 0, 16);
                        var guid = new Guid(guidBytes);
                        
                        Debug.WriteLine($"Looking up name for desktop {i + 1} with GUID: {guid}");
                        
                        using var desktopKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops\Desktops\{{{guid}}}");
                        if (desktopKey?.GetValue("Name") is string customName && !string.IsNullOrWhiteSpace(customName))
                        {
                            _desktopNames[i + 1] = customName.Trim();
                            Debug.WriteLine($"Desktop {i + 1} has custom name: '{customName.Trim()}'");
                        }
                        else
                        {
                            _desktopNames[i + 1] = $"Desktop {i + 1}";
                            Debug.WriteLine($"Desktop {i + 1} using default name");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Could not get desktop GUIDs, using default names");
                    for (int i = 1; i <= desktopCount; i++)
                    {
                        _desktopNames[i] = $"Desktop {i}";
                    }
                }

                Debug.WriteLine($"Final desktop names: {string.Join(", ", _desktopNames.Select(kvp => $"{kvp.Key}='{kvp.Value}'"))}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating desktop names: {ex.Message}");
                
                _desktopNames.Clear();
                for (int i = 1; i <= desktopCount; i++)
                {
                    _desktopNames[i] = $"Desktop {i}";
                }
            }
        }


        public void SwitchToDesktop(int desktopNumber)
        {
            try
            {
                if (desktopNumber < 1 || desktopNumber > _desktopCount) 
                {
                    Debug.WriteLine($"Invalid desktop number: {desktopNumber} (count: {_desktopCount})");
                    return;
                }

                Debug.WriteLine($"Switching to desktop {desktopNumber}");

                if (TrySwitchViaComApi(desktopNumber))
                {
                    Debug.WriteLine("Successfully switched via COM API");
                }
                else
                {
                    Debug.WriteLine("COM API failed, trying hotkeys");
                    if (desktopNumber <= 9)
                    {
                        SendWinKey($"^({{WIN}}){desktopNumber}");
                    }
                    else if (desktopNumber == 10)
                    {
                        SendWinKey("^({WIN})0");
                    }
                }

                System.Threading.Tasks.Task.Delay(200).ContinueWith(t => 
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => UpdateDesktopInfo());
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to switch to desktop {desktopNumber}: {ex.Message}");
            }
        }

        private bool TrySwitchViaComApi(int desktopNumber)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("Shell.Application");
                if (shellType != null)
                {
                    // SECURITY: Safe COM object creation with null checking
                    var shellObject = Activator.CreateInstance(shellType);
                    if (shellObject != null)
                    {
                        dynamic shell = shellObject;
                        shell?.WindowsSwitchToDesktop?.Invoke(desktopNumber - 1);
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("COM object creation returned null");
                    }
                }
                else
                {
                    Debug.WriteLine("Shell.Application COM type not found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"COM API approach failed: {ex.Message}");
            }

            // SECURITY: Use direct Win32 API instead of PowerShell to eliminate injection risk
            return TrySwitchViaWin32KeyboardAPI(desktopNumber);
        }

        public void SwitchToNextDesktop()
        {
            try
            {
                Debug.WriteLine("Switching to next desktop");
                SendWinKey("^({WIN}){RIGHT}");
                System.Threading.Tasks.Task.Delay(200).ContinueWith(t => 
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => UpdateDesktopInfo());
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to switch to next desktop: {ex.Message}");
            }
        }

        public void SwitchToPreviousDesktop()
        {
            try
            {
                Debug.WriteLine("Switching to previous desktop");
                SendWinKey("^({WIN}){LEFT}");
                System.Threading.Tasks.Task.Delay(200).ContinueWith(t => 
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => UpdateDesktopInfo());
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to switch to previous desktop: {ex.Message}");
            }
        }

        private void SendWinKey(string keys)
        {
            try
            {
                Debug.WriteLine($"Sending keys: {keys}");
                
                if (keys.Contains("WIN"))
                {
                    SendVirtualDesktopHotkey(keys);
                }
                else
                {
                    System.Windows.Forms.SendKeys.Send(keys);
                }
                
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send keys {keys}: {ex.Message}");
            }
        }

        private void SendVirtualDesktopHotkey(string keys)
        {
            try
            {
                Debug.WriteLine($"SendVirtualDesktopHotkey called with: {keys}");
                
                System.Threading.Tasks.Task.Run(() => {
                    try
                    {
                        System.Threading.Thread.Sleep(50);
                        
                        Debug.WriteLine("Sending key sequence...");
                        
                        keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
                        
                        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                        
                        System.Threading.Thread.Sleep(20);
                        
                        if (keys.Contains("RIGHT"))
                        {
                            Debug.WriteLine("Sending RIGHT key");
                            keybd_event(VK_RIGHT, 0, 0, UIntPtr.Zero);
                            System.Threading.Thread.Sleep(20);
                            keybd_event(VK_RIGHT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                        }
                        else if (keys.Contains("LEFT"))
                        {
                            Debug.WriteLine("Sending LEFT key");
                            keybd_event(VK_LEFT, 0, 0, UIntPtr.Zero);
                            System.Threading.Thread.Sleep(20);
                            keybd_event(VK_LEFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                        }
                        else
                        {
                            var numberChar = keys.Where(char.IsDigit).FirstOrDefault();
                            if (numberChar != '\0')
                            {
                                Debug.WriteLine($"Sending number key: {numberChar}");
                                var vkCode = numberChar == '0' ? VK_0 : (VK_1 + (numberChar - '1'));
                                keybd_event((byte)vkCode, 0, 0, UIntPtr.Zero);
                                System.Threading.Thread.Sleep(20);
                                keybd_event((byte)vkCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                            }
                        }
                        
                        System.Threading.Thread.Sleep(20);
                        
                        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                        
                        keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                        
                        Debug.WriteLine("Successfully sent virtual desktop hotkey");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed in background thread: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send virtual desktop hotkey: {ex.Message}");
            }
        }

        private bool TrySwitchViaWin32KeyboardAPI(int desktopNumber)
        {
            try
            {
                // SECURITY: Strict validation to prevent any injection or invalid values
                if (desktopNumber < 1 || desktopNumber > 10)
                {
                    Debug.WriteLine($"SECURITY: Invalid desktop number rejected: {desktopNumber}");
                    return false;
                }

                Debug.WriteLine($"Using direct Win32 API to switch to desktop {desktopNumber}");

                // Send Win+Ctrl+<number> key combination directly using existing keybd_event function
                // Win down
                keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
                // Ctrl down  
                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);

                System.Threading.Thread.Sleep(20);
                byte numberKey;
                if (desktopNumber == 10)
                {
                    numberKey = VK_0;
                }
                else
                {
                    numberKey = (byte)(VK_1 + desktopNumber - 1);
                }

                keybd_event(numberKey, 0, 0, UIntPtr.Zero);
                System.Threading.Thread.Sleep(50);
                keybd_event(numberKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                System.Threading.Thread.Sleep(20);

                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                Debug.WriteLine("Direct Win32 API switching completed");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Win32 API approach failed: {ex.Message}");
                return false;
            }
        }

        private const byte VK_LWIN = 0x5B;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_LEFT = 0x25;
        private const byte VK_RIGHT = 0x27;
        private const byte VK_1 = 0x31;
        private const byte VK_0 = 0x30;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        public void StartMonitoring()
        {
            _monitorTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            
            _monitorTimer.Tick += (s, e) => UpdateDesktopInfo();
            _monitorTimer.Start();
        }

        public void StopMonitoring()
        {
            _monitorTimer?.Stop();
            _monitorTimer = null;
        }
    }
}