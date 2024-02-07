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
        public void ExecuteBuyOrder_ShouldReturnErrorMessage_WhenNoExchangesAvailable()
        {
            // Arrange
            var amountBTC = 1.2m;
            var exchanges = new List<Exchange> { };

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.ErrorMessage.Should().Be("No exchanges available.");
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldReturnErrorMessage_WhenExchangesIsNull()
        {
            // Arrange
            var amountBTC = 1.2m;
            IEnumerable<Exchange> exchanges = null;

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.ErrorMessage.Should().Be("No exchanges available.");
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldBuyTheRequestedBTCAmount_WhenTheConditionsAreMet()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldReturnExecutionTradeDetails_WhenTheConditionsAreMet()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

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
        public void ExecuteBuyOrder_ShouldReturnTotalCostWithinTwoDecimals_WhenTheConditionsAreMet(
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

            decimal correctTotalCost = priceA * 0.5m + priceB * 0.5m;
            decimal correctTotalCostRounded = Math.Round(correctTotalCost, 2);

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(2);
            result.Summary.AverageBTCPrice.Should().Be(correctTotalCost / amountBTC); // not rounded
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Summary.TotalEUR.Should().Be(correctTotalCostRounded);
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldBuyTheBestOffers_WhenAnAskHasBetterPrice()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].PricePerBTC.Should().Be(9000m);
            result.Execution[0].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldIgnoreOrdersOfSellType_WhenThereAreNoAsks()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("No asks available.");
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldIgnoreOrdersOfSellTypeAndBuyCorrectAmount_WhenThereAreBidsAndAsks()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].PricePerBTC.Should().Be(9000m);
            result.Execution[0].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldBuyTheBestOffers_WhenThereIsBetterPriceOnAnotherExchange()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange B");
            result.Execution[0].PricePerBTC.Should().Be(9000m);
            result.Execution[0].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldBuyFromDifferentBestOffers_WhenTheAmountIsHigherThanTheBestOffer()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

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
        public void ExecuteBuyOrder_ShouldBuyFromDifferentBestOffersInSameExchange_WhenAmountNeedsToBeSpreadOut()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(4);
            result.Summary.TotalEUR.Should().Be(7600m);
        }
        [Fact]
        public void ExecuteBuyOrder_ShouldHaveTheRightExchangeOrder_WhenTheBestOrdersChangeBetweenExchanges()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Execution.Should().HaveCount(3);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[1].Exchange.Should().Be("Exchange B");
            result.Execution[2].Exchange.Should().Be("Exchange A");
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldReturnSuccessFalseAndErrorMessage_WhenOrderIsPartiallyCompleted()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 0.5BTC acquired.");
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldFulfillTheOrderPartially_WhenTheExchangesDoNotHaveEnoughBTC()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 1.0BTC acquired.");
            result.Summary.BTCVolume.Should().Be(1.0m);
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldFulfillTheOrderPartially_WhenExchangeRunsOutOfBalance()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 1BTC acquired.");
        }

        [Fact]
        public void ExecuteBuyOrder_ShouldMoveToNextExchange_WhenAnExchangeRunsOutOfBalance()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

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
        public void ExecuteBuyOrder_ShouldReturnSpecificErrorMessage_WhenTheExchangesDoNotHaveEnoughEUR()
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

            // Act
            var result = _buyTradeExecutionService.ExecuteBuyOrder(amountBTC, exchanges);

            // Assert
            result.ErrorMessage.Should().Be("Could not fulfill the order.");
        }
    }
}
