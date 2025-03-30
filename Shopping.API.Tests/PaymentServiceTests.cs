using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Shopping.API.Models.Request;
using Shopping.API.Models.Response;
using Shopping.API.Services;
using Shopping.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Xunit;
using Shopping.API.Services.Interfaces;

namespace Shopping.API.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly PaymentService _service;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public PaymentServiceTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var client = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            _service = new PaymentService(
                _mockHttpClientFactory.Object.CreateClient(),
                new Mock<ICartService>().Object, // Mock de ICartService
                _mockCacheService.Object,
                _mockOrderRepository.Object,
                new Mock<ILogger<PaymentService>>().Object
            );
        }

        [Fact]
        public async Task ProcessPaymentAsync_ReturnsPaymentResponse_WhenSuccessful()
        {
            // Arrange
            var request = new PaymentRequest
            {
                CartId = 123,
                Amount = 10.00m,
                PayerDocument = "12312312389"
            };

            var expectedResponse = new PaymentResponse
            {
                PaymentId = Guid.NewGuid(),
                Status = "INLINE",
                CreatedAt = DateTime.UtcNow
            };

            // Mock HTTP response
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
                });

            // Act
            var result = await _service.ProcessPayment(request.CartId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.PaymentId, result.PaymentId);
            Assert.Equal(expectedResponse.Status, result.PaymentStatus);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ThrowsException_WhenPaymentFails()
        {
            // Arrange
            var request = new PaymentRequest
            {
                CartId = 123,
                Amount = 10.00m,
                PayerDocument = "12312312389"
            };

            // Mock HTTP response for failure
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _service.ProcessPayment(request.CartId));
        }
    }
}
