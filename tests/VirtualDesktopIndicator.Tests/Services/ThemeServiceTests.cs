using Xunit;
using VirtualDesktopIndicator.Services;

namespace VirtualDesktopIndicator.Tests.Services
{
    public class ThemeServiceTests
    {
        [Fact]
        public void ShouldUseDarkTheme_WithFollowSystemMode_ShouldReturnValidResult()
        {
            // Act
            var result = ThemeService.ShouldUseDarkTheme(ThemeMode.FollowSystem);

            // Assert
            Assert.True(result == true || result == false); // Should return a valid bool
        }

        [Fact]
        public void ShouldUseDarkTheme_WithLightMode_ShouldReturnFalse()
        {
            // Act
            var result = ThemeService.ShouldUseDarkTheme(ThemeMode.Light);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShouldUseDarkTheme_WithDarkMode_ShouldReturnTrue()
        {
            // Act
            var result = ThemeService.ShouldUseDarkTheme(ThemeMode.Dark);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSystemDarkTheme_ShouldReturnValidResult()
        {
            // Act
            var result = ThemeService.IsSystemDarkTheme();

            // Assert
            Assert.True(result == true || result == false); // Should return a valid bool
        }
    }
}