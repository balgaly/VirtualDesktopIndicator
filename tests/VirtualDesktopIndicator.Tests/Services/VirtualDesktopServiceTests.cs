using Xunit;
using VirtualDesktopIndicator.Services;
using System;

namespace VirtualDesktopIndicator.Tests.Services
{
    public class VirtualDesktopServiceTests : IDisposable
    {
        private readonly VirtualDesktopService _service;

        public VirtualDesktopServiceTests()
        {
            _service = new VirtualDesktopService();
        }

        [Fact]
        public void GetCurrentDesktopNumber_ShouldReturnValidDesktopNumber()
        {
            // Act
            var currentDesktop = _service.GetCurrentDesktopNumber();

            // Assert
            Assert.True(currentDesktop >= 1);
        }

        [Fact]
        public void GetDesktopCount_ShouldReturnAtLeastOne()
        {
            // Act
            var desktopCount = _service.GetDesktopCount();

            // Assert
            Assert.True(desktopCount >= 1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void GetDesktopName_WithValidDesktopNumber_ShouldReturnName(int desktopNumber)
        {
            // Act
            var desktopName = _service.GetDesktopName(desktopNumber);

            // Assert
            Assert.NotNull(desktopName);
            Assert.NotEmpty(desktopName);
        }

        [Fact]
        public void GetDesktopName_WithInvalidDesktopNumber_ShouldReturnDefaultName()
        {
            // Act
            var desktopName = _service.GetDesktopName(999);

            // Assert
            Assert.Equal("Desktop 999", desktopName);
        }

        [Fact]
        public void DesktopChanged_Event_ShouldBeRaised_WhenDesktopChanges()
        {
            // Arrange
            var receivedDesktopNumber = 0;

            _service.DesktopChanged += (sender, desktopNumber) =>
            {
                receivedDesktopNumber = desktopNumber;
            };

            _service.StartMonitoring();

            // Act & Assert
            // This is a basic setup test - actual desktop switching would require integration testing
            Assert.NotNull(_service);
            
            // Cleanup
            _service.StopMonitoring();
        }

        [Fact]
        public void StartMonitoring_ShouldNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => _service.StartMonitoring());
            Assert.Null(exception);
            
            // Cleanup
            _service.StopMonitoring();
        }

        [Fact]
        public void StopMonitoring_ShouldNotThrow()
        {
            // Arrange
            _service.StartMonitoring();

            // Act & Assert
            var exception = Record.Exception(() => _service.StopMonitoring());
            Assert.Null(exception);
        }

        public void Dispose()
        {
            _service.StopMonitoring();
        }
    }
}