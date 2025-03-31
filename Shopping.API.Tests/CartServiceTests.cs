using Moq;
using Xunit;
using Shopping.API.Application.Services;
using Shopping.API.Infrastructure.Repositories.Interfaces;
using Shopping.API.Domain.Models;
using Shopping.API.Domain.Models.Request;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Shopping.API.Application.Services.Interfaces;

namespace Shopping.API.Tests.Services
{
    public class CartServiceTests
    {
        private readonly Mock<ICartRepository> _cartRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<ILogger<CartService>> _loggerMock;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _cartRepositoryMock = new Mock<ICartRepository>();
            _cacheServiceMock = new Mock<ICacheService>();
            _loggerMock = new Mock<ILogger<CartService>>();
            _cartService = new CartService(_cartRepositoryMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetCartByIdAsync_ShouldReturnCart_WhenCartExists()
        {
            // Arrange
            int cartId = 1;
            string payerDocument = "22268425002";
            decimal discountPercentage = 10;
            var expectedCart = new Cart (cartId, payerDocument, discountPercentage) { Amount = 100 };
            _cartRepositoryMock.Setup(repo => repo.GetByIdAsync(cartId)).ReturnsAsync(expectedCart);

            // Act
            var result = await _cartService.GetCartByIdAsync(cartId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCart, result);
        }

        [Fact]
        public async Task CreateCartAsync_ShouldReturnCart_WhenRequestIsValid()
        {
            // Arrange
            int cartId = 1;
            string payerDocument = "22268425002";
            decimal discountPercentage = 10;
            var cartRequest = new CartRequest (payerDocument);
            var expectedCart = new Cart (cartId, payerDocument, discountPercentage);
            _cartRepositoryMock.Setup(repo => repo.CreateAsync(cartRequest)).ReturnsAsync(expectedCart);

            // Act
            var result = await _cartService.CreateCartAsync(cartRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCart, result);
        }

        [Fact]
        public async Task DeleteCartAsync_ShouldReturnTrue_WhenCartExists()
        {
            // Arrange
            int cartId = 1;
            string payerDocument = "22268425002";
            decimal discountPercentage = 10;
            _cartRepositoryMock.Setup(repo => repo.GetByIdAsync(cartId)).ReturnsAsync(new Cart(cartId, payerDocument, discountPercentage));
            _cartRepositoryMock.Setup(repo => repo.DeleteAsync(cartId)).ReturnsAsync(true);

            // Act
            var result = await _cartService.DeleteCartAsync(cartId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            int productId = 1;
            var expectedProduct = new Product { ProductId = productId };
            _cartRepositoryMock.Setup(repo => repo.GetProductByIdAsync(productId)).ReturnsAsync(expectedProduct);

            // Act
            var result = await _cartService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProduct, result);
        }

        [Fact]
        public async Task GetCartTotalAsync_ShouldReturnTotal_WhenCartExists()
        {
            // Arrange
            int cartId = 1;
            string payerDocument = "22268425002";
            decimal discountPercentage = 10;
            var cart = new Cart (cartId, payerDocument, discountPercentage) { Products = new List<Product> { new Product { ProductName = "Ball", Price = 100, Quantity = 2 } }};
            _cartRepositoryMock.Setup(repo => repo.GetByIdAsync(cartId)).ReturnsAsync(cart);

            // Act
            var result = await _cartService.GetCartTotalAsync(cartId);

            // Assert
            Assert.Equal(180, result); // 100 * 2 * 0.9
        }

        [Fact]
        public async Task AddProductToCartAsync_ShouldReturnProduct_WhenProductIsValid()
        {
            // Arrange
            int cartId = 1;
            var product = new Product { ProductId = 1, ProductName = "Ball", Price = 100, Quantity = 1 };
            _cartRepositoryMock.Setup(repo => repo.AddProductAsync(product)).Returns(Task.CompletedTask);

            // Act
            var result = await _cartService.AddProductToCartAsync(cartId, product);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product, result);
        }

        [Fact]
        public async Task UpdateProductInCartAsync_ShouldReturnTrue_WhenProductIsValid()
        {
            // Arrange
            int productId = 1;
            var product = new Product { ProductId = productId, ProductName = "Ball", CartId = 1, Price = 100, Quantity = 1 };
            _cartRepositoryMock.Setup(repo => repo.GetProductByIdAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.UpdateProductAsync(product)).ReturnsAsync(true);

            // Act
            var result = await _cartService.UpdateProductInCartAsync(productId, product);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RemoveProductFromCartAsync_ShouldReturnTrue_WhenProductExists()
        {
            // Arrange
            int productId = 1;
            var product = new Product { ProductId = productId, CartId = 1 };
            _cartRepositoryMock.Setup(repo => repo.GetProductByIdAsync(productId)).ReturnsAsync(product);
            _cartRepositoryMock.Setup(repo => repo.RemoveProductAsync(productId)).ReturnsAsync(true);

            // Act
            var result = await _cartService.RemoveProductFromCartAsync(productId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ApplyDiscountAsync_ShouldReturnTrue_WhenDiscountIsValid()
        {
            // Arrange
            int cartId = 1;
            string payerDocument = "22268425002";
            decimal discountPercentage = 10;
            var cart = new Cart (cartId, payerDocument, discountPercentage);
            _cartRepositoryMock.Setup(repo => repo.GetByIdAsync(cartId)).ReturnsAsync(cart);
            _cartRepositoryMock.Setup(repo => repo.UpdateCartDiscountPercentage(cart)).ReturnsAsync(true);

            // Act
            var result = await _cartService.ApplyDiscountAsync(cartId, discountPercentage);

            // Assert
            Assert.True(result);
        }
    }
}
