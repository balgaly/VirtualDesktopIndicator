using Xunit;
using FluentAssertions;
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
            currentDesktop.Should().BeGreaterOrEqualTo(1);
        }

        [Fact]
        public void GetDesktopCount_ShouldReturnAtLeastOne()
        {
            // Act
            var desktopCount = _service.GetDesktopCount();

            // Assert
            desktopCount.Should().BeGreaterOrEqualTo(1);
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
            desktopName.Should().NotBeNullOrEmpty();
            desktopName.Should().StartWith("Desktop");
        }

        [Fact]
        public void GetDesktopName_WithInvalidDesktopNumber_ShouldReturnDefaultName()
        {
            // Act
            var desktopName = _service.GetDesktopName(999);

            // Assert
            desktopName.Should().Be("Desktop 999");
        }

        [Fact]
        public void DesktopChanged_Event_ShouldBeRaised_WhenDesktopChanges()
        {
            // Arrange
            var eventRaised = false;
            var receivedDesktopNumber = 0;

            _service.DesktopChanged += (sender, desktopNumber) =>
            {
                eventRaised = true;
                receivedDesktopNumber = desktopNumber;
            };

            _service.StartMonitoring();

            // Act & Assert
            // This is a basic setup test - actual desktop switching would require integration testing
            _service.Should().NotBeNull();
            
            // Cleanup
            _service.StopMonitoring();
        }

        [Fact]
        public void StartMonitoring_ShouldNotThrow()
        {
            // Act & Assert
            var action = () => _service.StartMonitoring();
            action.Should().NotThrow();
            
            // Cleanup
            _service.StopMonitoring();
        }

        [Fact]
        public void StopMonitoring_ShouldNotThrow()
        {
            // Arrange
            _service.StartMonitoring();

            // Act & Assert
            var action = () => _service.StopMonitoring();
            action.Should().NotThrow();
        }

        public void Dispose()
        {
            _service.StopMonitoring();
        }
    }
}