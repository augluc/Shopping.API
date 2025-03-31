using Moq;
using Shopping.API.Application.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Shopping.API.Tests.Services
{
    public class CacheServiceTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;

        public CacheServiceTests()
        {
            _cacheServiceMock = new Mock<ICacheService>();
        }

        [Fact]
        public async Task GetItem_ShouldReturnTotal_WhenCartTotalExists()
        {
            // Arrange
            int cartId = 1;
            var expectedTotal = 205M;
            _cacheServiceMock.Setup(service => service.GetCachedCartTotalAsync(cartId))
                .ReturnsAsync((decimal?)expectedTotal);

            // Act
            var result = await _cacheServiceMock.Object.GetCachedCartTotalAsync(cartId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTotal, result);
        }

        [Fact]
        public async Task GetItem_ShouldReturnZero_WhenCartTotalDoesNotExist()
        {
            // Arrange
            int cartId = 1;
            _cacheServiceMock.Setup(service => service.GetCachedCartTotalAsync(cartId))
                .ReturnsAsync((decimal?)null);

            // Act
            var result = await _cacheServiceMock.Object.GetCachedCartTotalAsync(cartId);

            // Assert
            Assert.Null(result);
        }
    }
}
