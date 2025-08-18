# Changelog

All notable changes to VirtualDesktopIndicator will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Comprehensive test suite with xUnit, FluentAssertions, and Moq
- GitHub Actions CI/CD pipeline with automated testing and releases
- Professional project structure with proper documentation
- Issue templates for bugs, features, and questions
- Contributing guidelines and code of conduct

### Changed
- Improved project organization with src/ and tests/ directories
- Enhanced README with badges, installation instructions, and comprehensive documentation

## [1.0.0] - 2025-01-XX

### Added
- 🎯 **System Tray Integration** - Clean, modern tray icon showing current desktop number
- 🚀 **Desktop Switching** - Left-click tray icon for next desktop, full keyboard shortcut support
- 🎨 **Modern 2025 UI Design** - Glassmorphism-inspired interface with contemporary styling
- 🌐 **Theme Support** - Follow System (default), Light, and Dark modes
- 📱 **Desktop Detection** - Real-time detection via Windows Registry with custom name support
- ⌨️ **Keyboard Shortcuts** - Alt + `, Alt + 1-5, Ctrl + Win + ←/→ support
- 💫 **Animated Popup** - Smooth desktop switching indicator with auto-hide
- 📋 **Informative Context Menu** - Right-click menu showing desktop status and switching instructions
- ⚡ **Lightweight Performance** - Minimal system resource usage
- 🔧 **Smart Desktop Names** - Automatic detection and display of custom Windows desktop names

### Features
- **System Tray Indicator**: Professional icon with centered numbers (1-9, 0 for desktop 10, * for 10+)
- **Desktop Switching**: Multiple methods for seamless desktop navigation
- **Theme Intelligence**: Automatic system theme detection and following
- **Custom Names**: Full support for custom desktop names from Windows settings
- **Real-time Updates**: Instant tray icon updates when desktops change
- **Modern Design**: 2025-inspired UI with proper spacing, typography, and color schemes
- **Compact Interface**: Streamlined context menu with essential information only
- **Performance Optimized**: Timer-based monitoring with minimal CPU impact

### Technical Details
- **Framework**: .NET 8.0 with WPF
- **Platform**: Windows 10 1903+ and Windows 11
- **Architecture**: Clean service-oriented design with separation of concerns
- **Dependencies**: Minimal - uses built-in Windows APIs only
- **Memory Usage**: ~15-20 MB typical usage
- **Startup Time**: <2 seconds cold start, <1 second to tray

### System Requirements
- Windows 10 version 1903 or later
- Windows 11 (all versions)
- x64 architecture
- .NET 8 Runtime (framework-dependent version only)
- No administrator privileges required

### Installation Options
- **Portable**: Self-contained executable, no installation required
- **Framework-Dependent**: Smaller download, requires .NET 8 Runtime

---

## Development History

### Design Evolution
The UI design evolved through several iterations based on user feedback:

1. **Initial Implementation**: Basic functionality with standard Windows styling
2. **First Refinement**: Modern rounded icons with better contrast and sizing
3. **User Feedback Integration**: Centered numbers, custom name support, theme following
4. **2025 Design Upgrade**: Contemporary glassmorphism-inspired styling with sophisticated typography
5. **Compact Optimization**: Streamlined interface removing redundancy while maintaining functionality

### Technical Decisions
- **Registry-based Detection**: Chose Windows Registry over COM APIs for reliability
- **Informative Menu Design**: Pivoted from functional to informative right-click menu after functionality challenges
- **Service Architecture**: Implemented clean separation of concerns with dedicated services
- **Theme Integration**: Built comprehensive theme system supporting Windows theme detection

### User-Driven Development
This project was developed with continuous user feedback integration:
- Professional design requirements guided visual decisions
- Functionality feedback shaped feature prioritization
- Modern design requests influenced the 2025 UI update
- Usability feedback led to informative menu approach

---

## Future Releases

See [ROADMAP.md](.claude/future-roadmap.md) for planned features and improvements.

### Planned for v1.1
- Enhanced Windows 11 compatibility testing
- Performance optimizations for large desktop counts
- Accessibility improvements
- Additional keyboard shortcut customization

### Planned for v1.2
- Desktop templates and saved configurations
- Enhanced multi-monitor support
- Plugin architecture foundation
- Advanced theme customization options