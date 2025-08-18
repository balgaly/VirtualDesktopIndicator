# 🤝 Contributing to VirtualDesktopIndicator

First off, thank you for considering contributing to VirtualDesktopIndicator! 🎉

It's people like you that make VirtualDesktopIndicator such a great tool for the Windows community.

## 📋 Table of Contents

- [Code of Conduct](#-code-of-conduct)
- [Quick Start](#-quick-start)
- [How Can I Contribute?](#-how-can-i-contribute)
- [Development Setup](#-development-setup)
- [Pull Request Process](#-pull-request-process)
- [Coding Standards](#-coding-standards)
- [Project Structure](#-project-structure)
- [Testing Guidelines](#-testing-guidelines)
- [Recognition](#-recognition)

## 📜 Code of Conduct

This project and everyone participating in it is governed by our commitment to maintaining a welcoming, inclusive environment. By participating, you are expected to uphold this standard.

### Our Standards
- ✅ Be respectful and inclusive
- ✅ Focus on constructive feedback
- ✅ Accept responsibility and learn from mistakes
- ✅ Show empathy towards other community members
- ❌ No trolling, insulting, or derogatory comments
- ❌ No harassment of any kind

## 🚀 Quick Start

### For First-Time Contributors
1. **🍴 Fork** the repository
2. **📥 Clone** your fork locally
3. **🔧 Set up** the development environment
4. **🌿 Create** a feature branch
5. **💻 Make** your changes
6. **✅ Test** your changes
7. **📤 Submit** a pull request

### For Experienced Contributors
Jump to [Development Setup](#-development-setup) or check [Open Issues](https://github.com/balgaly/VirtualDesktopIndicator/issues) for ways to help!

## 🛠️ How Can I Contribute?

### 🐛 Reporting Bugs
Found a bug? Help us fix it!

1. **Search** [existing issues](https://github.com/balgaly/VirtualDesktopIndicator/issues) first
2. **Use** the [bug report template](https://github.com/balgaly/VirtualDesktopIndicator/issues/new?template=bug_report.md)
3. **Include** system information and steps to reproduce
4. **Add** screenshots if applicable

### 💡 Suggesting Features
Have an idea for improvement?

1. **Check** [existing feature requests](https://github.com/balgaly/VirtualDesktopIndicator/issues?q=is%3Aissue+label%3Aenhancement)
2. **Use** the [feature request template](https://github.com/balgaly/VirtualDesktopIndicator/issues/new?template=feature_request.md)
3. **Describe** your use case and proposed solution
4. **Consider** implementation complexity and user impact

### 📝 Improving Documentation
Documentation is always appreciated!

- Fix typos or unclear instructions
- Add examples or screenshots
- Improve API documentation
- Translate documentation (future)

### 💻 Code Contributions
Ready to dive into the code?

- **🐛 Bug fixes** - Always welcome!
- **✨ New features** - Discuss first in an issue
- **⚡ Performance improvements** - Provide benchmarks
- **🧪 Test improvements** - Increase coverage
- **🔧 Refactoring** - Keep changes focused

## 🔧 Development Setup

### Prerequisites
- **Windows 10/11** (for testing virtual desktop functionality)
- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** 
- **Git** for version control
- **Visual Studio 2022** or **VS Code** (recommended)

### Setup Steps
```bash
# 1. Fork and clone the repository
git clone https://github.com/balgaly/VirtualDesktopIndicator.git
cd VirtualDesktopIndicator

# 2. Restore dependencies
dotnet restore

# 3. Build the solution
dotnet build

# 4. Run tests to ensure everything works
dotnet test

# 5. Run the application for testing
dotnet run --project src/VirtualDesktopIndicator.csproj
```

### IDE Setup
**Visual Studio 2022:**
- Open `VirtualDesktopIndicator.sln`
- Install recommended extensions
- Set startup project to `VirtualDesktopIndicator`

**VS Code:**
- Install C# extension
- Install .NET Extension Pack
- Use Ctrl+Shift+P → ".NET: Generate Assets for Build and Debug"

## 🔄 Pull Request Process

### Before You Start
1. **📋 Create/comment** on an issue describing what you plan to work on
2. **🌿 Create** a feature branch from `main`
3. **📝 Follow** our [coding standards](#-coding-standards)

### Branch Naming
- `feature/add-new-hotkey` - New features
- `fix/tray-icon-crash` - Bug fixes  
- `docs/update-readme` - Documentation
- `refactor/service-cleanup` - Refactoring
- `test/improve-coverage` - Test improvements

### Commit Messages
Write clear, descriptive commit messages:

```bash
# Good examples
git commit -m "Add Alt+Shift+` shortcut for previous desktop"
git commit -m "Fix tray icon not updating on Windows 11"
git commit -m "Improve desktop detection performance by 50%"

# Avoid
git commit -m "Fix bug"
git commit -m "Update code"
git commit -m "Changes"
```

### PR Checklist
- [ ] 🔍 **Self-review** completed
- [ ] 📝 **Comments** added for complex logic
- [ ] 🧪 **Tests** added/updated for new functionality
- [ ] ✅ **All tests pass** locally
- [ ] 📚 **Documentation** updated if needed
- [ ] 🔄 **CI passes** (builds and tests)
- [ ] 📋 **PR description** follows template

## 🎨 Coding Standards

### C# Style Guidelines
- **Follow** [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- **Use** PascalCase for public members
- **Use** camelCase for private fields with `_` prefix
- **Prefer** explicit types over `var` for clarity
- **Add** XML documentation for public APIs

### Code Quality
```csharp
// ✅ Good: Clear, documented, testable
/// <summary>
/// Switches to the specified virtual desktop number.
/// </summary>
/// <param name="desktopNumber">Desktop number (1-based)</param>
/// <returns>True if switch was successful</returns>
public bool SwitchToDesktop(int desktopNumber)
{
    if (desktopNumber < 1 || desktopNumber > GetDesktopCount())
    {
        throw new ArgumentOutOfRangeException(nameof(desktopNumber));
    }
    
    return TrySwitchDesktop(desktopNumber);
}

// ❌ Avoid: Unclear, undocumented
public bool Switch(int n) => DoIt(n);
```

### File Organization
```
src/
├── Services/           # Business logic
├── Views/             # UI components
├── Models/            # Data models
├── Helpers/           # Utility classes
└── Resources/         # Assets, icons
```

## 🧪 Testing Guidelines

### Test Strategy
- **Unit Tests** - Test individual components in isolation
- **Integration Tests** - Test component interactions
- **Manual Testing** - Verify real-world scenarios

### Writing Tests
```csharp
[Fact]
public void SwitchToDesktop_WithValidNumber_ShouldSucceed()
{
    // Arrange
    var service = new VirtualDesktopService();
    
    // Act
    var result = service.SwitchToDesktop(2);
    
    // Assert
    Assert.True(result);
}
```

### Test Requirements
- **Descriptive names** that explain what's being tested
- **Arrange-Act-Assert** pattern
- **Edge cases** and error conditions
- **Minimal external dependencies**

## 📂 Project Structure

```
VirtualDesktopIndicator/
├── .github/                    # GitHub templates and workflows
│   ├── workflows/             # CI/CD pipelines
│   └── ISSUE_TEMPLATE/        # Issue templates
├── .claude/                   # Development context
├── src/                       # Source code
│   ├── Services/             # Core business logic
│   ├── Views/                # WPF views and windows
│   ├── Themes/               # UI themes and styles
│   └── VirtualDesktopIndicator.csproj
├── tests/                     # Test projects
├── docs/                      # Documentation
├── assets/                    # Images, icons, demos
├── README.md                  # Main documentation
├── CONTRIBUTING.md            # This file
├── LICENSE                    # MIT License
└── VirtualDesktopIndicator.sln # Solution file
```

## 🏆 Recognition

### Contributors Wall
All contributors will be recognized in:
- 📋 **README.md** - Contributors section
- 🎉 **Release notes** - Feature/fix attribution  
- 💬 **Social media** - Shoutouts for significant contributions

### Types of Contributions
- 💻 **Code** - Features, fixes, improvements
- 📝 **Documentation** - Guides, examples, translations
- 🐛 **Testing** - Bug reports, test improvements
- 🎨 **Design** - UI/UX improvements, icons
- 💡 **Ideas** - Feature suggestions, feedback
- 🤝 **Community** - Helping others, issue triage

## 🎯 Current Priorities

Check our [project board](https://github.com/balgaly/VirtualDesktopIndicator/projects) for current priorities:

### High Priority
- 🐛 **Bug fixes** for Windows 11 compatibility
- ⚡ **Performance optimizations** for large desktop counts
- 🧪 **Test coverage** improvements

### Medium Priority  
- ✨ **New keyboard shortcuts** and customization
- 🎨 **UI/UX** enhancements and themes
- 📱 **Accessibility** improvements

### Future Ideas
- 🌍 **Internationalization** support
- 🔧 **Plugin system** for extensions
- 📊 **Usage analytics** and telemetry

## ❓ Questions?

- 💬 **Discussions**: Use [GitHub Discussions](https://github.com/balgaly/VirtualDesktopIndicator/discussions)
- 🐛 **Issues**: Check [existing issues](https://github.com/balgaly/VirtualDesktopIndicator/issues)
- 📧 **Direct contact**: Create a question issue using our template

---

## 🙏 Thank You!

Every contribution, no matter how small, makes VirtualDesktopIndicator better for everyone. We appreciate your time and effort in helping improve this project!

**Happy coding!** 🚀

---

<div align="center">

**[⬆️ Back to Top](#-contributing-to-virtualdesktopindicator)**

</div>