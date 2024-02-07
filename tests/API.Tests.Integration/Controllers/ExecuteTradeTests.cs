using API.Tests.Integration.Models;
using FluentAssertions;
using MetaExchange.API.Models;
using MetaExchange.Common.Models;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace API.Tests.Integration.Controllers
{
    public class ExecuteTradeTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly HttpClient _client;

        public ExecuteTradeTests(IntegrationTestWebAppFactory factory)
        {
            _client = factory.CreateClient();
        }
        // The task was to create a single endpoint that can handle both buy and sell orders,
        // so to not clutter this test file and to avoid mocking the data I will just focus on the 
        // overall functionality of the endpoint. I tried my best to not overengineer the solution

        [Fact]
        public async Task ExecuteTrade_ShouldReturnTheCorrectStatusCode_WhenInputsAreValid()
        {
            // Arrange
            var request = new TradeRequestDto
            {
                OrderType = "buy",
                Amount = 0.1m
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/trade/execute", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ExecuteTrade_ShouldReturnTradeOrderResult_WhenInputsAreValid()
        {
            // Arrange
            var request = new TradeRequestDto
            {
                OrderType = "buy",
                Amount = 0.1m
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/trade/execute", content);
            var result = await response.Content.ReadFromJsonAsync<TradeOrderResult>();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<TradeOrderResult>();
        }

        [Fact]
        public async Task ExecuteTrade_ShouldReturnCorrectOrderTypeAndAmount_WhenInputsAreValid()
        {
            // Arrange
            var request = new TradeRequestDto
            {
                OrderType = "buy",
                Amount = 0.1m
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/trade/execute", content);
            var result = await response.Content.ReadFromJsonAsync<TradeOrderResult>();

            // Assert
            result.OrderType.Should().Be("Buy");
            result.RequestedBTC.Should().Be(0.1m);
        }

        [Fact]
        public async Task ExecuteTrade_ShouldReturnCorrectErrorMessage_WhenOrderTypeIsInvalid()
        {
            // Arrange
            var request = new TradeRequestDto
            {
                OrderType = "invalid",
                Amount = 0.1m
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/trade/execute", content);
            var errorResponse = await response.Content.ReadFromJsonAsync<ValidationError>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            errorResponse.Errors["OrderType"].Should().Contain("Order type must be either 'buy' or 'sell'.");
        }

        [Fact]
        public async Task ExecuteTrade_ShouldReturnCorrectErrorMessage_WhenAmountIsInvalid()
        {
            // Arrange
            var request = new TradeRequestDto
            {
                OrderType = "buy",
                Amount = -0.1m
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/trade/execute", content);
            var errorResponse = await response.Content.ReadFromJsonAsync<ValidationError>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            errorResponse.Errors["Amount"].Should().Contain("BTC amount must be greater than 0.");
        }
    }
}
