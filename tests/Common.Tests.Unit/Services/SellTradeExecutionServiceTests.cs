namespace Common.Tests.Unit.Services
{
    public class SellTradeExecutionServiceTests
    {
        private readonly ISellTradeExecutionService _sellTradeExecutionService;
        private readonly IExchangeDataContextService _exchangeDataContextService;

        public SellTradeExecutionServiceTests()
        {
            _exchangeDataContextService = Substitute.For<IExchangeDataContextService>();
            _sellTradeExecutionService = new SellTradeExecutionService(_exchangeDataContextService);
        }

        public void ConfigureExchangeDataContextService(IEnumerable<Exchange> exchanges)
        {
            _exchangeDataContextService.GetExchangeDataAsync().Returns(Task.FromResult(exchanges));
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldReturnTradeOrderResult_WhenCalled()
        {
            // Arrange
            var amountBTC = 1.0m;
            var exchanges = new List<Exchange> { };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Should().BeOfType<TradeOrderResult>();
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldHaveCorrectAmountAndBuyOrderType_WhenCalled()
        {
            // Arrange
            var amountBTC = 1.0m;
            var exchanges = new List<Exchange> { };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.OrderType.Should().Be("Sell");
            result.RequestedBTC.Should().Be(amountBTC);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ExecuteSellOrderAsync_ShouldReturnErrorMessage_WhenRequestedBTCAmountIsInvalid(
            decimal amount)
        {
            // Arrange
            var amountBTC = amount;
            var exchanges = new List<Exchange> { };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.ErrorMessage.Should().Be("Invalid BTC amount requested.");
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldReturnErrorMessage_WhenNoExchangesAvailable()
        {
            // Arrange
            var amountBTC = 1.0m;
            var exchanges = new List<Exchange> { };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.ErrorMessage.Should().Be("No exchanges available.");
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldReturnTradeOrderResultWithSuccessFalse_WhenNoExchangesAvailable()
        {
            // Arrange
            var amountBTC = 1.0m;
            var exchanges = new List<Exchange> { };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldReturnErrorMessage_WhenExchangesIsNull()
        {
            // Arrange
            var amountBTC = 1.0m;
            List<Exchange>? exchanges = null;
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.ErrorMessage.Should().Be("No exchanges available.");
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldSellTheRequestedBTCAmount_WhenTheConditionsAreMet()
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Amount = 1.0m,
                                    Price = 1000.0m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldReturnExecutionTradeDetails_WhenTheConditionsAreMet()
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
                                    Amount = 1.0m,
                                    Price = 10000m
                                }
                            }
                        }
                    }
                }
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].Action.Should().Be("Sell");
            result.Execution[0].RequestedBTCAmount.Should().Be(amountBTC);
            result.Execution[0].PricePerBTC.Should().Be(10000m);
            result.Execution[0].TotalCostEUR.Should().Be(10000m);
        }

        [Theory]
        [InlineData(2956.116, 2953.24)]
        [InlineData(2956.116, 2953.245)]
        [InlineData(2956.116, 2953.246)]
        [InlineData(2956.116, 2953.249)]
        public async Task ExecuteSellOrderAsync_ShouldReturnTotalEURSalesWithinTwoDecimals_WhenTheConditionsAreMet(
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Price = priceA,
                                    Amount = 0.5m
                                }
                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(2);
            result.Summary.AverageBTCPrice.Should().Be(correctTotalCost / amountBTC); // not rounded
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Summary.TotalEUR.Should().Be(correctTotalCostRounded);
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldBuyTheBestOffers_WhenBidHasBetterPrice()
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(1);
            result.Execution[0].PricePerBTC.Should().Be(10000m);
            result.Execution[0].TotalCostEUR.Should().Be(10000m);
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldIgnoreOrdersOfBuyType_WhenThereAreNoBids()
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("No bids available.");
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldIgnoreOrdersOfBuyTypeAndSellCorrectAmount_WhenThereAreBidsAndAsks()
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
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            }
                        },
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(1);
            result.Execution[0].PricePerBTC.Should().Be(9000m);
            result.Execution[0].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldSellToBestOffers_WhenThereIsBetterPriceOnAnotherExchange()
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Price = 9000m,
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
                        BTC = 10.0m,
                        EUR = 100000m
                    },
                    OrderBook = new OrderBook
                    {
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange B");
            result.Execution[0].PricePerBTC.Should().Be(10000m);
            result.Execution[0].TotalCostEUR.Should().Be(10000m);
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldSellToDifferentBestOffers_WhenTheAmountIsHigherThanTheBestOffer()
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Price = 9000m,
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
                        BTC = 10.0m,
                        EUR = 100000m
                    },
                    OrderBook = new OrderBook
                    {
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(2);
            result.Execution[0].Exchange.Should().Be("Exchange B");
            result.Execution[0].PricePerBTC.Should().Be(10000m);
            result.Execution[0].TotalCostEUR.Should().Be(10000m);
            result.Execution[1].Exchange.Should().Be("Exchange A");
            result.Execution[1].PricePerBTC.Should().Be(9000m);
            result.Execution[1].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldSellToDifferentBestOffersInSameExchange_WhenAmountNeedsToBeSpreadOut()
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(2);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].PricePerBTC.Should().Be(10000m);
            result.Execution[0].TotalCostEUR.Should().Be(10000m);
            result.Execution[1].Exchange.Should().Be("Exchange A");
            result.Execution[1].PricePerBTC.Should().Be(9000m);
            result.Execution[1].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldSellFromDifferentBestOffersInSameExchange_WhenAmountNeedsToBeSpreadOut()
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(2);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].PricePerBTC.Should().Be(10000m);
            result.Execution[0].TotalCostEUR.Should().Be(10000m);
            result.Execution[1].Exchange.Should().Be("Exchange A");
            result.Execution[1].PricePerBTC.Should().Be(9000m);
            result.Execution[1].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldHaveTheRightExchangeOrder_WhenTheBestOrdersChangeBetweenExchanges()
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
                                    Price = 8000m,
                                    Amount = 0.5m
                                }
                            },
                            new OrderWrapper
                            {
                                Order = new Order
                                {
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(3);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[1].Exchange.Should().Be("Exchange B");
            result.Execution[2].Exchange.Should().Be("Exchange A");
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldReturnSuccessFalseAndErrorMessage_WhenOrderIsPartiallyCompleted()
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 1.0BTC sold.");
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldFulfillTheOrderPartially_WhenTheExchangesDoNotHaveEnoughBTC()
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 1.0BTC sold.");
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldFulfillTheOrderPartially_WhenExchangeRunsOutOfBalance()
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
                                    Price = 10000m,
                                    Amount = 1.0m
                                }
                            }
                        }
                    }
                },
            };
            ConfigureExchangeDataContextService(exchanges);

            // Act
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 1.0BTC sold.");
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldMoveToNextExchange_WhenAnExchangeRunsOutOfBalance()
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
                        Bids = new List<OrderWrapper>
                        {
                            new OrderWrapper
                            {
                                Order = new Order
                                {
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(2);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].RequestedBTCAmount.Should().Be(1.0m);
            result.Execution[1].Exchange.Should().Be("Exchange B");
            result.Execution[1].RequestedBTCAmount.Should().Be(1.0m);
        }

        [Fact]
        public async Task ExecuteSellOrderAsync_ShouldReturnSpecificErrorMessage_WhenTheExchangesDoNotHaveEnoughBTC()
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
                        BTC = 0m,
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
            var result = await _sellTradeExecutionService.ExecuteSellOrderAsync(amountBTC);

            // Assert
            result.ErrorMessage.Should().Be("Could not fulfill the order.");
        }
    }
}
