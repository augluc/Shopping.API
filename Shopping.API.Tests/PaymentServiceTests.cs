using Moq;
using Xunit;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shopping.API.Application.Services;
using Shopping.API.Application.Services.Interfaces;
using Shopping.API.Domain.Models;
using Shopping.API.Domain.Models.Request;
using Shopping.API.Domain.Models.Response;
using Shopping.API.Infrastructure.Repositories.Interfaces;
using Moq.Protected;
using System.Text.Json;

namespace Shopping.API.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<ICartService> _cartServiceMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<ILogger<PaymentService>> _loggerMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _cartServiceMock = new Mock<ICartService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _loggerMock = new Mock<ILogger<PaymentService>>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _paymentService = new PaymentService(_httpClient, _cartServiceMock.Object, _cacheServiceMock.Object, _orderRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ProcessPayment_ShouldReturnOrder_WhenPaymentIsSuccessful()
        {
            // Arrange
            int cartId = 1;
            string payerDocument = "22268425002";
            decimal discountPercentage = 10m;
            var cart = new Cart(cartId, payerDocument, discountPercentage) { Amount = 100, Products = new List<Product> { new Product() } };
            var paymentResponse = new PaymentResponse { PaymentId = Guid.NewGuid(), Status = "INLINE", CreatedAt = DateTime.UtcNow };
            var expectedOrder = new Order { OrderId = 1, CartId = cartId };

            _cartServiceMock.Setup(service => service.GetCartByIdAsync(cartId)).ReturnsAsync(cart);
            _orderRepositoryMock.Setup(repo => repo.GetByCartIdAsync(cartId)).ReturnsAsync((Order)null);
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(paymentResponse))
                });
            _orderRepositoryMock.Setup(repo => repo.CreateAsync(cartId, paymentResponse.PaymentId, paymentResponse.Status, paymentResponse.CreatedAt)).ReturnsAsync(expectedOrder);

            _httpClient.BaseAddress = new Uri("http://localhost/");

            // Act
            var result = await _paymentService.ProcessPayment(cartId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOrder.OrderId, result.OrderId);
            Assert.Equal(expectedOrder.CartId, result.CartId);
        }


        [Fact]
        public async Task ProcessPayment_ShouldThrowArgumentException_WhenCartIdIsInvalid()
        {
            // Arrange
            int invalidCartId = -1;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _paymentService.ProcessPayment(invalidCartId));
        }

        [Fact]
        public async Task ProcessPayment_ShouldThrowArgumentException_WhenCartIsEmpty()
        {
            // Arrange
            int cartId = 1;
            string payerDocument = "22268425002";
            decimal discountPercentage = 10m;
            var cart = new Cart (cartId, payerDocument, discountPercentage) {Products = new List<Product>() };

            _cartServiceMock.Setup(service => service.GetCartByIdAsync(cartId)).ReturnsAsync(cart);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _paymentService.ProcessPayment(cartId));
        }

        [Fact]
        public async Task ProcessPayment_ShouldThrowArgumentException_WhenPayerDocumentIsMissing()
        {
            // Arrange
            int cartId = 1;
            string payerDocument = "";
            decimal discountPercentage = 10m;
            var cart = new Cart(cartId, payerDocument, discountPercentage) { Products = new List<Product>() };

            _cartServiceMock.Setup(service => service.GetCartByIdAsync(cartId)).ReturnsAsync(cart);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _paymentService.ProcessPayment(cartId));
        }

        [Fact]
        public async Task ProcessPayment_ShouldThrowApplicationException_WhenPaymentApiFails()
        {
            // Arrange
            int cartId = 1;
            string payerDocument = "22268425002";
            decimal discountPercentage = 10m;
            var cart = new Cart(cartId, payerDocument, discountPercentage) { Amount= 100, Products = new List<Product>() };

            _cartServiceMock.Setup(service => service.GetCartByIdAsync(cartId)).ReturnsAsync(cart);
            _orderRepositoryMock.Setup(repo => repo.GetByCartIdAsync(cartId)).ReturnsAsync((Order)null);
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _paymentService.ProcessPayment(cartId));
        }
    }
}
