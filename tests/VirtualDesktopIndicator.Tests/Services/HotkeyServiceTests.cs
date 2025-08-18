using Xunit;
using FluentAssertions;
using VirtualDesktopIndicator.Services;
using Moq;
using System;

namespace VirtualDesktopIndicator.Tests.Services
{
    public class HotkeyServiceTests : IDisposable
    {
        private readonly Mock<VirtualDesktopService> _mockDesktopService;
        private readonly HotkeyService _hotkeyService;

        public HotkeyServiceTests()
        {
            _mockDesktopService = new Mock<VirtualDesktopService>();
            _hotkeyService = new HotkeyService(_mockDesktopService.Object);
        }

        [Fact]
        public void Constructor_WithValidDesktopService_ShouldNotThrow()
        {
            // Act & Assert
            var action = () => new HotkeyService(_mockDesktopService.Object);
            action.Should().NotThrow();
        }

        [Fact]
        public void Constructor_WithNullDesktopService_ShouldThrow()
        {
            // Act & Assert
            var action = () => new HotkeyService(null!);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Act & Assert
            var action = () => _hotkeyService.Dispose();
            action.Should().NotThrow();
        }

        [Fact]
        public void HotkeyService_ShouldImplementIDisposable()
        {
            // Assert
            _hotkeyService.Should().BeAssignableTo<IDisposable>();
        }

        public void Dispose()
        {
            _hotkeyService?.Dispose();
        }
    }
}