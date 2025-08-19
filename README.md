# 🖥️ VirtualDesktopIndicator

> **A modern, free alternative to paid virtual desktop tools for Windows**

[![Build Status](https://github.com/balgaly/VirtualDesktopIndicator/workflows/CI/badge.svg)](https://github.com/balgaly/VirtualDesktopIndicator/actions)
[![Release Version](https://img.shields.io/github/v/release/balgaly/VirtualDesktopIndicator)](https://github.com/balgaly/VirtualDesktopIndicator/releases)
[![Downloads](https://img.shields.io/github/downloads/balgaly/VirtualDesktopIndicator/total)](https://github.com/balgaly/VirtualDesktopIndicator/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010/11-blue)](https://www.microsoft.com/windows)

<div align="center">

[**📥 Download Latest**](https://github.com/balgaly/VirtualDesktopIndicator/releases/latest) • [**📖 Documentation**](#-quick-start) • [**🐛 Report Bug**](https://github.com/balgaly/VirtualDesktopIndicator/issues/new?template=bug_report.md) • [**💡 Request Feature**](https://github.com/balgaly/VirtualDesktopIndicator/issues/new?template=feature_request.md)

</div>

---

## ✨ Features

🎯 **Smart System Tray Integration**
- Modern indicator showing current desktop number
- Real-time updates when switching desktops
- Custom desktop name display support

🚀 **Instant Desktop Switching**  
- Left-click tray icon for next desktop
- Keyboard shortcuts (Alt + `, Ctrl + Win + ←/→)
- Smooth animated desktop transition popup

🎨 **2025 Modern Design**
- Clean, professional interface inspired by current design trends
- Automatic theme detection (Follow System, Light, Dark)
- Compact, informative context menu

⚡ **Lightweight & Responsive**
- Minimal system resource usage
- Real-time desktop detection via Windows APIs  
- No background services or constant polling

🔧 **Smart Desktop Detection**
- Automatically detects virtual desktop changes
- Supports custom desktop names from Windows settings
- Dynamic desktop count adjustment

---

## 🎬 Preview

### System Tray Integration
> Modern indicator with clean design that shows your current desktop

### Desktop Switching Popup  
> Smooth animated popup when switching between desktops

### Context Menu
> Informative menu showing all desktops and switching options

*Screenshots coming soon!*

---

## 💾 Installation

### 🎯 Choose Your Installation Method

| Method | Best For | Pros | Cons |
|--------|----------|------|------|
| 🚀 **Portable** | Most users | No installation, no dependencies | Larger download (~25MB) |
| 📦 **Winget** | Power users | Auto-updates, easy management | Requires Windows Package Manager |
| 🔧 **Framework** | Developers | Smaller download (~2MB) | Requires .NET 8 Runtime |

---

### 🚀 Option 1: Portable (Recommended)
**Best for**: Most users who want zero hassle

1. **Download** the latest release:
   ```
   https://github.com/balgaly/VirtualDesktopIndicator/releases/latest/download/VirtualDesktopIndicator-v1.0.0-win-x64-portable.zip
   ```
2. **Extract** to your preferred location (e.g., `C:\Tools\VirtualDesktopIndicator\`)
3. **Run** `VirtualDesktopIndicator.exe`
4. **Optional**: Pin to taskbar or add to startup folder

✅ **No installation required** • ✅ **No .NET Runtime needed** • ✅ **Single executable** • ✅ **Works offline**

---

### 📦 Option 2: Windows Package Manager (Winget)
**Best for**: Users who want automatic updates and easy management

```powershell
# Install VirtualDesktopIndicator
winget install VirtualDesktopIndicator

# Or specify the full package ID
winget install balgaly.VirtualDesktopIndicator
```

**Update to latest version:**
```powershell
winget upgrade VirtualDesktopIndicator
```

**Uninstall:**
```powershell
winget uninstall VirtualDesktopIndicator
```

✅ **Automatic updates** • ✅ **Easy management** • ✅ **No manual downloads** • ✅ **System integration**

---

### 🔧 Option 3: Framework-Dependent
**Best for**: Developers and users who already have .NET 8 installed

**Prerequisites:**
1. **Install** [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) if not already installed

**Download & Run:**
2. **Download** the framework-dependent version:
   ```
   https://github.com/balgaly/VirtualDesktopIndicator/releases/latest/download/VirtualDesktopIndicator-v1.0.0-win-x64-framework-dependent.zip
   ```
3. **Extract** and run `VirtualDesktopIndicator.exe`

💡 **Smaller download** • ⚠️ **Requires .NET 8 Runtime** • 🔄 **Shared dependencies**

---

### 🔐 Verify Download Integrity

For security, verify your download using SHA256 checksums:

```powershell
# PowerShell - Check file hash
Get-FileHash -Algorithm SHA256 "VirtualDesktopIndicator-v1.0.0-win-x64-portable.zip"

# Compare with published checksums in release notes
```

**Official checksums** are provided in each [GitHub release](https://github.com/balgaly/VirtualDesktopIndicator/releases/latest).

---

## 🚀 Quick Start

### Basic Usage
1. **Launch** VirtualDesktopIndicator.exe
2. **Look** for the numbered icon in your system tray  
3. **Left-click** the tray icon to switch to the next desktop
4. **Right-click** for the context menu with all options

### Keyboard Shortcuts
| Shortcut | Action |
|----------|--------|
| `Alt + `` | Switch to next desktop |
| `Ctrl + Win + ←` | Previous desktop |
| `Ctrl + Win + →` | Next desktop |
| **Left-click tray** | Next desktop |

### Customizing Desktop Names
1. **Open** Windows Settings → System → Multitasking
2. **Navigate** to Virtual desktops section
3. **Right-click** any desktop and choose "Rename"
4. **VirtualDesktopIndicator** will automatically display your custom names!

---

## 🎨 Themes

VirtualDesktopIndicator supports three theme modes:

- 🌐 **Follow System** (Default) - Automatically matches Windows theme
- ☀️ **Light Mode** - Always use light theme  
- 🌙 **Dark Mode** - Always use dark theme

*Access theme settings via right-click context menu → Theme*

---

## 🔧 System Requirements

- **OS**: Windows 10 version 1903+ or Windows 11
- **Architecture**: x64 (64-bit)
- **.NET**: 8.0 Runtime (framework-dependent version only)
- **Permissions**: User-level (no admin required)

---

## 🏗️ Building from Source

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 or VS Code (optional)
- Windows 10/11 for testing

### Steps
```bash
# Clone the repository
git clone https://github.com/balgaly/VirtualDesktopIndicator.git
cd VirtualDesktopIndicator

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test

# Run the application
dotnet run --project src/VirtualDesktopIndicator.csproj
```

### Publishing
```bash
# Create portable executable
dotnet publish src/VirtualDesktopIndicator.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  --output ./publish/portable \
  /p:PublishSingleFile=true
```

---

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Quick Start for Contributors
1. 🍴 Fork the repository
2. 🌿 Create a feature branch (`git checkout -b feature/amazing-feature`)
3. 💻 Make your changes
4. ✅ Add tests for new functionality
5. 🧪 Ensure all tests pass (`dotnet test`)
6. 📝 Commit your changes (`git commit -m 'Add amazing feature'`)
7. 📤 Push to the branch (`git push origin feature/amazing-feature`)
8. 🔄 Open a Pull Request

### Development Guidelines
- Follow existing code style and conventions
- Add tests for new features and bug fixes
- Update documentation for user-facing changes
- Ensure CI passes before requesting review

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

### Why MIT?
- ✅ **Commercial use allowed** - Use in your business freely
- ✅ **Modification allowed** - Customize as needed  
- ✅ **Distribution allowed** - Share with others
- ✅ **Private use allowed** - Personal projects welcome

---

## 💝 Acknowledgments

- **You** - For using and supporting this project! ⭐
- **Windows Virtual Desktop APIs** - For making virtual desktop integration possible
- **Claude AI (Anthropic)** - Development assistance and architectural guidance
- **Community Contributors** - Everyone who reports bugs, suggests features, and contributes code
- **Open Source Inspiration** - Projects like PowerToys and other Windows utilities

---

## 🌟 Star History

If VirtualDesktopIndicator has improved your workflow, please consider giving it a ⭐!

---

## 📊 Project Stats

![GitHub repo size](https://img.shields.io/github/repo-size/balgaly/VirtualDesktopIndicator)
![GitHub code size](https://img.shields.io/github/languages/code-size/balgaly/VirtualDesktopIndicator)
![GitHub issues](https://img.shields.io/github/issues/balgaly/VirtualDesktopIndicator)
![GitHub pull requests](https://img.shields.io/github/issues-pr/balgaly/VirtualDesktopIndicator)
![GitHub last commit](https://img.shields.io/github/last-commit/balgaly/VirtualDesktopIndicator)

---

<div align="center">

**Made with ❤️ for the Windows community**

[⬆️ Back to Top](#️-virtualdesktopindicator)

</div>