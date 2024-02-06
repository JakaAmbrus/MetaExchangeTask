namespace Common.Tests.Unit.Services
{
    public class BuyTradeExecutionServiceTests
    {
        private readonly IBuyTradeExecutionService _buyTradeExecutionService;

        public BuyTradeExecutionServiceTests()
        {
            _buyTradeExecutionService = new BuyTradeExecutionService();
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldReturnTradeOrderResult_WhenCalled()
        {
            // Arrange
            var amountBTC = 1.2m;
            var exchanges = new List<Exchange> { };

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Should().BeOfType<TradeOrderResult>();
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldHaveCorrectAmountAndBuyOrderType_WhenCalled()
        {
            // Arrange
            var amountBTC = 1.2m;
            var exchanges = new List<Exchange> { };

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.OrderType.Should().Be("Buy");
            result.RequestedBTC.Should().Be(amountBTC);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ExecuteBuyOrder_ShouldReturnErrorMessage_WhenRequestedBTCAmountIsInvalid(
            decimal amount)
        {
            // Arrange
            var amountBTC = amount;
            var exchanges = new List<Exchange> { };

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.ErrorMessage.Should().Be("Invalid BTC amount requested.");
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldBuyBTCFromExchanges_WhenCalled()
        {
            // Arrange
            var amountBTC = 1.2m;
            var exchanges = new List<Exchange>
            {
                new Exchange
                {
                    Identifier = "Exchange A",
                    Balances = new Balances
                    {
                        BTC = 1.0m,
                        EUR = 5000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<Order>
                        {
                            new Order
                            {
                                Type = "Sell",
                                Price = 10000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Execution.Should().NotBeEmpty();
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 0.5BTC acquired.");
            result.Summary.BTCAcquired.Should().Be(0.5m);
            result.Summary.TotalEURCost.Should().Be(5000m);
        }
    }
}
