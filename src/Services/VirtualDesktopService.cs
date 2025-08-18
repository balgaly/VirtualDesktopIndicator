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
                // Get current desktop using registry approach
                var (current, count) = GetDesktopInfoFromRegistry();
                
                // If registry approach fails, use alternative method
                if (current == 0 || count == 0)
                {
                    (current, count) = GetDesktopInfoFromTaskView();
                }

                // Update desktop names
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

                // Get all desktop IDs first
                var allDesktopsData = key.GetValue("VirtualDesktopIDs") as byte[];
                if (allDesktopsData == null) 
                {
                    Debug.WriteLine("VirtualDesktopIDs not found in registry");
                    return (1, 1);
                }

                var desktopCount = allDesktopsData.Length / 16; // Each GUID is 16 bytes
                Debug.WriteLine($"Found {desktopCount} desktops in registry");

                // Get current desktop ID
                var currentDesktopData = key.GetValue("CurrentVirtualDesktop") as byte[];
                if (currentDesktopData == null) 
                {
                    Debug.WriteLine("CurrentVirtualDesktop not found, assuming desktop 1");
                    return (1, desktopCount);
                }

                var currentDesktopGuid = new Guid(currentDesktopData);
                DebugLogger.WriteSecurityInfo($"Current desktop GUID detected");

                // Find the index of current desktop
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

        private (int current, int count) GetDesktopInfoFromTaskView()
        {
            try
            {
                // Use PowerShell to get virtual desktop info
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = @"-Command ""
                        $source = @'
                        using System;
                        using System.Runtime.InteropServices;
                        public class VirtualDesktopHelper {
                            [DllImport(""user32.dll"")]
                            public static extern IntPtr GetForegroundWindow();
                        }
'@
                        Add-Type -TypeDefinition $source
                        
                        # Try to get desktop count using Task View registry
                        try {
                            $vdKey = Get-ItemProperty -Path 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops' -ErrorAction SilentlyContinue
                            if ($vdKey -and $vdKey.VirtualDesktopIDs) {
                                $count = [math]::Floor($vdKey.VirtualDesktopIDs.Length / 16)
                                if ($vdKey.CurrentVirtualDesktop) {
                                    $currentGuid = [System.Guid]::new($vdKey.CurrentVirtualDesktop)
                                    $current = 1
                                    for ($i = 0; $i -lt $count; $i++) {
                                        $guidBytes = $vdKey.VirtualDesktopIDs[($i * 16)..(($i + 1) * 16 - 1)]
                                        $guid = [System.Guid]::new($guidBytes)
                                        if ($guid -eq $currentGuid) {
                                            $current = $i + 1
                                            break
                                        }
                                    }
                                    Write-Output ""$current,$count""
                                } else {
                                    Write-Output ""1,$count""
                                }
                            } else {
                                Write-Output ""1,1""
                            }
                        } catch {
                            Write-Output ""1,1""
                        }
                    """,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit(5000); // 5 second timeout

                    if (!string.IsNullOrEmpty(output) && output.Contains(","))
                    {
                        var parts = output.Split(',');
                        if (parts.Length == 2 && 
                            int.TryParse(parts[0], out int current) && 
                            int.TryParse(parts[1], out int count))
                        {
                            return (current, count);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PowerShell method failed: {ex.Message}");
            }

            return (1, 1); // Fallback
        }

        private void UpdateDesktopNames(int desktopCount)
        {
            try
            {
                // Clear existing names
                _desktopNames.Clear();

                // Get desktop GUIDs from registry
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops");
                var allDesktopsData = key?.GetValue("VirtualDesktopIDs") as byte[];
                
                if (allDesktopsData != null && allDesktopsData.Length >= desktopCount * 16)
                {
                    Debug.WriteLine("Getting desktop names from Desktops subkey");
                    
                    // For each desktop, get its GUID and look up the name
                    for (int i = 0; i < desktopCount; i++)
                    {
                        var guidBytes = new byte[16];
                        Array.Copy(allDesktopsData, i * 16, guidBytes, 0, 16);
                        var guid = new Guid(guidBytes);
                        
                        Debug.WriteLine($"Looking up name for desktop {i + 1} with GUID: {guid}");
                        
                        // Look for custom name in Desktops subkey
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
                    // Fallback: create default names
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
                
                // Fallback: create default names
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

                // Try direct COM API approach first
                if (TrySwitchViaComApi(desktopNumber))
                {
                    Debug.WriteLine("Successfully switched via COM API");
                }
                else
                {
                    Debug.WriteLine("COM API failed, trying hotkeys");
                    // Fallback to keyboard shortcuts
                    if (desktopNumber <= 9)
                    {
                        SendWinKey($"^({{WIN}}){desktopNumber}");
                    }
                    else if (desktopNumber == 10)
                    {
                        SendWinKey("^({WIN})0");
                    }
                }

                // Give Windows time to switch, then update
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
                // Try to use Windows 10/11 internal COM API for virtual desktops
                var shellType = Type.GetTypeFromProgID("Shell.Application");
                if (shellType != null)
                {
                    // SECURITY: Safe COM object creation with null checking
                    var shellObject = Activator.CreateInstance(shellType);
                    if (shellObject != null)
                    {
                        dynamic shell = shellObject;
                        
                        // Try to invoke the desktop switching through Shell
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
                
                // Alternative method using keybd_event API for more reliable key sending
                if (keys.Contains("WIN"))
                {
                    SendVirtualDesktopHotkey(keys);
                }
                else
                {
                    System.Windows.Forms.SendKeys.Send(keys);
                }
                
                System.Threading.Thread.Sleep(100); // Give more time for key processing
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
                
                // Run the key sending in a background thread to avoid UI blocking
                System.Threading.Tasks.Task.Run(() => {
                    try
                    {
                        // Make sure we have focus before sending keys
                        System.Threading.Thread.Sleep(50);
                        
                        Debug.WriteLine("Sending key sequence...");
                        
                        // Press Win key
                        keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
                        
                        // Press Ctrl key  
                        keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                        
                        System.Threading.Thread.Sleep(20);
                        
                        // Determine which additional key to press
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
                            // Extract number from keys string for direct desktop switching
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
                        
                        // Release Ctrl key
                        keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                        
                        // Release Win key
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

                // Small delay to ensure keys register
                System.Threading.Thread.Sleep(20);

                // Number key (handle desktop 10 as 0)
                byte numberKey;
                if (desktopNumber == 10)
                {
                    numberKey = VK_0; // '0' key for desktop 10
                }
                else
                {
                    numberKey = (byte)(VK_1 + desktopNumber - 1); // '1'-'9' keys for desktops 1-9
                }

                // Number down
                keybd_event(numberKey, 0, 0, UIntPtr.Zero);
                System.Threading.Thread.Sleep(50); // Ensure key registers
                // Number up
                keybd_event(numberKey, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                System.Threading.Thread.Sleep(20);

                // Ctrl up
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                // Win up
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

        // Windows API constants and imports
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
                Interval = TimeSpan.FromMilliseconds(250) // Check 4 times per second for responsiveness
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