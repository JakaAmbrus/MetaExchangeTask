namespace Common.Tests.Unit.Services
{
    public class BuyTradeExecutionServiceTests
    {
        private readonly IBuyTradeExecutionService _buyTradeExecutionService;
        private readonly IExchangeDataContextService _exchangeDataContextService;

        public BuyTradeExecutionServiceTests()
        {
            _exchangeDataContextService = Substitute.For<IExchangeDataContextService>();
            _buyTradeExecutionService = new BuyTradeExecutionService(_exchangeDataContextService);      
        }

        public void ConfigureExchangeDataContextService(IEnumerable<Exchange> exchanges)
        {
            _exchangeDataContextService.GetExchangeDataAsync().Returns(Task.FromResult(exchanges));
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldReturnTradeOrderResult_WhenCalled()
        {
            // Arrange
            var amountBTC = 1.2m;
            var exchanges = new List<Exchange> { };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Should().BeOfType<TradeOrderResult>();
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldHaveCorrectAmountAndBuyOrderType_WhenCalled()
        {
            // Arrange
            var amountBTC = 1.2m;
            var exchanges = new List<Exchange> { };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.OrderType.Should().Be("Buy");
            result.RequestedBTC.Should().Be(amountBTC);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ExecuteBuyOrderAsync_ShouldReturnErrorMessage_WhenRequestedBTCAmountIsInvalid(
            decimal amount)
        {
            // Arrange
            var amountBTC = amount;
            var exchanges = new List<Exchange> { };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.ErrorMessage.Should().Be("Invalid BTC amount requested.");
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldReturnErrorMessage_WhenNoExchangesAvailable()
        {
            // Arrange
            var amountBTC = 1.2m;
            var exchanges = new List<Exchange> { };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.ErrorMessage.Should().Be("No exchanges available.");
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldReturnErrorMessage_WhenExchangesIsNull()
        {
            // Arrange
            var amountBTC = 1.2m;
            IEnumerable<Exchange> exchanges = null;
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.ErrorMessage.Should().Be("No exchanges available.");
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldBuyTheRequestedBTCAmount_WhenTheConditionsAreMet()
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
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldReturnExecutionTradeDetails_WhenTheConditionsAreMet()
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
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].Action.Should().Be("Buy");
            result.Execution[0].RequestedBTCAmount.Should().Be(amountBTC);
            result.Execution[0].PricePerBTC.Should().Be(10000m);
            result.Execution[0].TotalCostEUR.Should().Be(10000m);
        }

        [Theory]
        [InlineData(2956.116, 2953.24)]
        [InlineData(2956.116, 2953.245)]
        [InlineData(2956.116, 2953.246)]
        [InlineData(2956.116, 2953.249)]
        public async Task ExecuteBuyOrderAsync_ShouldReturnTotalCostWithinTwoDecimals_WhenTheConditionsAreMet(
            decimal priceA, decimal priceB)
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
                        BTC = 10.0m,
                        EUR = 100000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = priceA,
                                    Amount = 0.5m
                                },

                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = priceB,
                                    Amount = 0.5m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            decimal correctTotalCost = priceA * 0.5m + priceB * 0.5m;
            decimal correctTotalCostRounded = Math.Round(correctTotalCost, 2);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(2);
            result.Summary.AverageBTCPrice.Should().Be(correctTotalCost / amountBTC); // not rounded
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Summary.TotalEUR.Should().Be(correctTotalCostRounded);
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldBuyTheBestOffers_WhenAnAskHasBetterPrice()
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
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 9000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].PricePerBTC.Should().Be(9000m);
            result.Execution[0].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldIgnoreOrdersOfSellType_WhenThereAreNoAsks()
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Buy",
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("No asks available.");
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldIgnoreOrdersOfSellTypeAndBuyCorrectAmount_WhenThereAreBidsAndAsks()
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Buy",
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            }
                        },
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 9000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].PricePerBTC.Should().Be(9000m);
            result.Execution[0].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldBuyTheBestOffers_WhenThereIsBetterPriceOnAnotherExchange()
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
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                },
                new Exchange
                {
                    Identifier = "Exchange B",
                    Balances = new Balances
                    {
                        BTC = 1.0m,
                        EUR = 10000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 9000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange B");
            result.Execution[0].PricePerBTC.Should().Be(9000m);
            result.Execution[0].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldBuyFromDifferentBestOffers_WhenTheAmountIsHigherThanTheBestOffer()
        {
            // Arrange
            var amountBTC = 1.5m;
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
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                },
                new Exchange
                {
                    Identifier = "Exchange B",
                    Balances = new Balances
                    {
                        BTC = 1.0m,
                        EUR = 10000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 9000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(2);
            result.Execution[0].Exchange.Should().Be("Exchange B");
            result.Execution[0].PricePerBTC.Should().Be(9000m);
            result.Execution[0].TotalCostEUR.Should().Be(9000m);
            result.Execution[1].Exchange.Should().Be("Exchange A");
            result.Execution[1].PricePerBTC.Should().Be(10000m);
            result.Execution[1].TotalCostEUR.Should().Be(5000m);
            result.Summary.TotalEUR.Should().Be(14000m); // 9000 + 5000
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldBuyFromDifferentBestOffersInSameExchange_WhenAmountNeedsToBeSpreadOut()
        {
            // Arrange
            var amountBTC = 0.8m;
            var exchanges = new List<Exchange>
            {
                new Exchange
                {
                    Identifier = "Exchange A",
                    Balances = new Balances
                    {
                        BTC = 10.0m,
                        EUR = 100000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 8000m,
                                    Amount = 0.2m
                                }
                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 9000m,
                                    Amount = 0.2m
                                }
                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 0.2m
                                }
                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 11000m,
                                    Amount = 0.2m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(4);
            result.Summary.TotalEUR.Should().Be(7600m);
        }
        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldHaveTheRightExchangeOrder_WhenTheBestOrdersChangeBetweenExchanges()
        {
            // Arrange
            var amountBTC = 2.0m;
            var exchanges = new List<Exchange>
            {
                new Exchange
                {
                    Identifier = "Exchange A",
                    Balances = new Balances
                    {
                        BTC = 10.0m,
                        EUR = 100000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 8000m,
                                    Amount = 0.5m
                                }
                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 0.5m
                                }
                            }
                        }
                    }
                },
                new Exchange
                {
                    Identifier = "Exchange B",
                    Balances = new Balances
                    {
                        BTC = 10.0m,
                        EUR = 100000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 9000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(3);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[1].Exchange.Should().Be("Exchange B");
            result.Execution[2].Exchange.Should().Be("Exchange A");
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldReturnSuccessFalseAndErrorMessage_WhenOrderIsPartiallyCompleted()
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
                        BTC = 15m,
                        EUR = 10000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 0.5m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 0.5BTC acquired.");
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldFulfillTheOrderPartially_WhenTheExchangesDoNotHaveEnoughBTC()
        {
            // Arrange
            var amountBTC = 1.5m;
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
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            }   
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 1.0BTC acquired.");
            result.Summary.BTCVolume.Should().Be(1.0m);
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldFulfillTheOrderPartially_WhenExchangeRunsOutOfBalance()
        {
            // Arrange
            var amountBTC = 2m;
            var exchanges = new List<Exchange>
            {
                new Exchange
                {
                    Identifier = "Exchange A",
                    Balances = new Balances
                    {
                        BTC = 0.5m,
                        EUR = 10000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 2m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 1BTC acquired.");
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldMoveToNextExchange_WhenAnExchangeRunsOutOfBalance()
        {
            // Arrange
            var amountBTC = 2m;
            var exchanges = new List<Exchange>
            {
                new Exchange
                {
                    Identifier = "Exchange A",
                    Balances = new Balances
                    {
                        BTC = 0.5m,
                        EUR = 10000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 2m
                                }
                            }
                        }
                    }
                },
                new Exchange
                {
                    Identifier = "Exchange B",
                    Balances = new Balances
                    {
                        BTC = 1.0m,
                        EUR = 10000m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 2m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(2);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].RequestedBTCAmount.Should().Be(1m);
            result.Execution[1].Exchange.Should().Be("Exchange B");
            result.Execution[1].RequestedBTCAmount.Should().Be(1m);
        }

        [Fact]
        public async Task ExecuteBuyOrderAsync_ShouldReturnSpecificErrorMessage_WhenTheExchangesDoNotHaveEnoughEUR()
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
                        EUR = 0m
                    },
                    OrderBook = new OrderBook
                    {
                        Asks = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Type = "Sell",
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _buyTradeExecutionService.ExecuteBuyOrderAsync(amountBTC);

            // Assert
            result.ErrorMessage.Should().Be("Could not fulfill the order.");
        }
    }
}
