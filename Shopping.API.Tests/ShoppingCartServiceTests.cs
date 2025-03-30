using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Shopping.API.Controllers;
using Shopping.API.Models;
using Shopping.API.Repositories.Interfaces;
using Shopping.API.Services;
using Shopping.API.Services.Interfaces;

namespace Shopping.API.Tests
{
    public class ShoppingCartServiceTests
    {
        private readonly Mock<ICartRepository> _mockRepository;
        private readonly Mock<ICacheService> _mockCache;
        private readonly Mock<ILogger<CartService>> _mockLogger;
        private readonly CartService _service;

        public ShoppingCartServiceTests()
        {
            _mockRepository = new Mock<ICartRepository>();
            _mockCache = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<CartService>>();
            _service = new CartService(_mockRepository.Object, _mockCache.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CalculateTotalAsync_ReturnsCachedValue_WhenAvailable()
        {
            // Arrange
            var cartId = 1;
            var cachedTotal = 150.50m;
            _mockCache.Setup(c => c.GetCachedCartTotalAsync(cartId)).ReturnsAsync(cachedTotal);

            // Act
            var result = await _service.GetCartTotalAsync(cartId);

            // Assert
            result.Should().Be(cachedTotal);
            _mockRepository.Verify(r => r.GetByIdAsync(cartId), Times.Never);
        }
    }
}
