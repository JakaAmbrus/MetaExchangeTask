namespace Common.Tests.Unit.Services
{
    public class SellTradeExecutionServiceTests
    {
        private readonly ISellTradeExecutionService _sellTradeExecutionService;

        public SellTradeExecutionServiceTests()
        {
            _sellTradeExecutionService = new SellTradeExecutionService();
        }

        [Fact]
        public void ExecuteSellOrder_ShouldReturnTradeOrderResult_WhenCalled()
        {
            // Arrange
            var amountBTC = 1.0m;
            var exchanges = new List<Exchange> { };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Should().BeOfType<TradeOrderResult>();
        }

        [Fact]
        public void ExecuteSellOrder_ShouldHaveCorrectAmountAndBuyOrderType_WhenCalled()
        {
            // Arrange
            var amountBTC = 1.0m;
            var exchanges = new List<Exchange> { };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.OrderType.Should().Be("Sell");
            result.RequestedBTC.Should().Be(amountBTC);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ExecuteSellOrder_ShouldReturnErrorMessage_WhenRequestedBTCAmountIsInvalid(
            decimal amount)
        {
            // Arrange
            var amountBTC = amount;
            var exchanges = new List<Exchange> { };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.ErrorMessage.Should().Be("Invalid BTC amount requested.");
        }

        [Fact]
        public void ExecuteSellOrder_ShouldReturnErrorMessage_WhenNoExchangesAvailable()
        {
            // Arrange
            var amountBTC = 1.0m;
            var exchanges = new List<Exchange> { };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.ErrorMessage.Should().Be("No exchanges available.");
        }

        [Fact]
        public void ExecuteSellOrder_ShouldReturnTradeOrderResultWithSuccessFalse_WhenNoExchangesAvailable()
        {
            // Arrange
            var amountBTC = 1.0m;
            var exchanges = new List<Exchange> { };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        public void ExecuteSellOrder_ShouldReturnErrorMessage_WhenExchangesIsNull()
        {
            // Arrange
            var amountBTC = 1.0m;
            List<Exchange>? exchanges = null;

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.ErrorMessage.Should().Be("No exchanges available.");
        }

        [Fact]
        public void ExecuteSellOrder_ShouldSellTheRequestedBTCAmount_WhenTheConditionsAreMet()
        {
            // Arrange
            var amountBTC = 1.0m;
            var exchanges = new List<Exchange>
            {
                new Exchange
                {
                    Identifier = "Exchange A",
                    Balances = new Balances
                    {
                        BTC = 1.0m,
                        EUR = 1000.0m
                    },
                    OrderBook = new OrderBook
                    {
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Type = "Buy",
                                Amount = 1.0m,
                                Price = 1000.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
        }

        [Fact]
        public void ExecuteSellOrder_ShouldReturnExecutionTradeDetails_WhenTheConditionsAreMet()
        {
            // Arrange
            var amountBTC = 1.0m;
            var exchanges = new List<Exchange>
            {
                new Exchange
                {
                    Identifier = "Exchange A",
                    Balances = new Balances
                    {
                        BTC = 1.0m,
                        EUR = 10000m
                    },
                    OrderBook = new OrderBook
                    {
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Type = "Buy",
                                Amount = 1.0m,
                                Price = 10000m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].Action.Should().Be("Sell");
            result.Execution[0].RequestedBTCAmount.Should().Be(amountBTC);
            result.Execution[0].PricePerBTC.Should().Be(10000m);
            result.Execution[0].TotalCostEUR.Should().Be(10000m);
        }
    }
}
