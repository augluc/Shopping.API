
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shopping.API.Controllers;
using Shopping.API.Models;
using Shopping.API.Services.Interfaces;

namespace Shopping.API.Tests
{
    public class ShoppingCartControllerTests
    {
        private readonly Mock<ICartService> _mockService;
        private readonly Mock<ILogger<CartController>> _mockLogger;
        private readonly CartController _controller;

        public ShoppingCartControllerTests()
        {
            _mockService = new Mock<ICartService>();
            _mockLogger = new Mock<ILogger<CartController>>();
            _controller = new CartController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetCart_ReturnsOk_WhenCartExists()
        {
            // Arrange
            var cartId = 1;
            var expectedCart = new Cart(cartId, "12345678901", null, null) { CartId = cartId };
            _mockService.Setup(s => s.GetCartByIdAsync(cartId)).ReturnsAsync(expectedCart);

            // Act
            var result = await _controller.GetCart(cartId) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(expectedCart);
        }

        [Fact]
        public async Task GetCart_ReturnsNotFound_WhenCartDoesNotExist()
        {
            // Arrange
            var cartId = 1;
            _mockService.Setup(s => s.GetCartByIdAsync(cartId)).ReturnsAsync((Cart?)null);

            // Act
            var result = await _controller.GetCart(cartId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
