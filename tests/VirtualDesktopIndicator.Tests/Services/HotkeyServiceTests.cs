using Xunit;
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
            var exception = Record.Exception(() => new HotkeyService(_mockDesktopService.Object));
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_WithNullDesktopService_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HotkeyService(null!));
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => _hotkeyService.Dispose());
            Assert.Null(exception);
        }

        [Fact]
        public void HotkeyService_ShouldImplementIDisposable()
        {
            // Assert
            Assert.IsAssignableFrom<IDisposable>(_hotkeyService);
        }

        public void Dispose()
        {
            _hotkeyService?.Dispose();
        }
    }
}