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

        [Theory]
        [InlineData(2956.116, 2953.24)]
        [InlineData(2956.116, 2953.245)]
        [InlineData(2956.116, 2953.246)]
        [InlineData(2956.116, 2953.249)]
        public void ExecuteSellOrder_ShouldReturnTotalEURSalesWithinTwoDecimals_WhenTheConditionsAreMet(
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = priceA,
                                Amount = 0.5m
                            },
                            new Order
                            {
                                Price = priceB,
                                Amount = 0.5m
                            }
                        }
                    }
                }
            };

            decimal correctTotalCost = priceA * 0.5m + priceB * 0.5m;
            decimal correctTotalCostRounded = Math.Round(correctTotalCost, 2);

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(2);
            result.Summary.AverageBTCPrice.Should().Be(correctTotalCost / amountBTC); // not rounded
            result.Summary.BTCVolume.Should().Be(amountBTC);
            result.Summary.TotalEUR.Should().Be(correctTotalCostRounded);
        }

        [Fact]
        public void ExecuteSellOrder_ShouldBuyTheBestOffers_WhenBidHasBetterPrice()
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            },
                            new Order
                            {
                                Price = 9000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(1);
            result.Execution[0].PricePerBTC.Should().Be(10000m);
            result.Execution[0].TotalCostEUR.Should().Be(10000m);
        }

        [Fact]
        public void ExecuteSellOrder_ShouldIgnoreOrdersOfBuyType_WhenThereAreNoBids()
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
                        Asks = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("No bids available.");
        }

        [Fact]
        public void ExecuteSellOrder_ShouldIgnoreOrdersOfBuyTypeAndSellCorrectAmount_WhenThereAreBidsAndAsks()
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
                        Asks = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            }
                        },
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 9000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(1);
            result.Execution[0].PricePerBTC.Should().Be(9000m);
            result.Execution[0].TotalCostEUR.Should().Be(9000m);
        }

        [Fact]
        public void ExecuteSellOrder_ShouldSellToBestOffers_WhenThereIsBetterPriceOnAnotherExchange()
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 9000m,
                                Amount = 1.0m
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(1);
            result.Execution[0].Exchange.Should().Be("Exchange B");
            result.Execution[0].PricePerBTC.Should().Be(10000m);
            result.Execution[0].TotalCostEUR.Should().Be(10000m);
        }

        [Fact]
        public void ExecuteSellOrder_ShouldSellToDifferentBestOffers_WhenTheAmountIsHigherThanTheBestOffer()
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 9000m,
                                Amount = 1.0m
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

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
        public void ExecuteSellOrder_ShouldSellToDifferentBestOffersInSameExchange_WhenAmountNeedsToBeSpreadOut()
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 9000m,
                                Amount = 1.0m
                            },
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

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
        public void ExecuteSellOrder_ShouldSellFromDifferentBestOffersInSameExchange_WhenAmountNeedsToBeSpreadOut()
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            },
                            new Order
                            {
                                Price = 9000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

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
        public void ExecuteSellOrder_ShouldHaveTheRightExchangeOrder_WhenTheBestOrdersChangeBetweenExchanges()
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
                        Bids = new List<Order>
                        {
                              new Order
                            {
                                Price = 8000m,
                                Amount = 0.5m
                            },
                            new Order
                            {
                                Price = 10000m,
                                Amount = 0.5m
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 9000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(3);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[1].Exchange.Should().Be("Exchange B");
            result.Execution[2].Exchange.Should().Be("Exchange A");
        }

        [Fact]
        public void ExecuteSellOrder_ShouldReturnSuccessFalseAndErrorMessage_WhenOrderIsPartiallyCompleted()
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 1.0BTC sold.");
        }
        [Fact]
        public void ExecuteSellOrder_ShouldFulfillTheOrderPartially_WhenTheExchangesDoNotHaveEnoughBTC()
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 1.0BTC sold.");
        }

        [Fact]
        public void ExecuteSellOrder_ShouldFulfillTheOrderPartially_WhenExchangeRunsOutOfBalance()
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            }
                        }
                    }
                },
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Could only fulfill the order partially. only 1.0BTC sold.");
        }

        [Fact]
        public void ExecuteSellOrder_ShouldMoveToNextExchange_WhenAnExchangeRunsOutOfBalance()
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 2.0m
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 9000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.Success.Should().BeTrue();
            result.Execution.Should().HaveCount(2);
            result.Execution[0].Exchange.Should().Be("Exchange A");
            result.Execution[0].RequestedBTCAmount.Should().Be(1.0m);
            result.Execution[1].Exchange.Should().Be("Exchange B");
            result.Execution[1].RequestedBTCAmount.Should().Be(1.0m);
        }

        [Fact]
        public void ExecuteSellOrder_ShouldReturnSpecificErrorMessage_WhenTheExchangesDoNotHaveEnoughBTC()
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
                        Bids = new List<Order>
                        {
                            new Order
                            {
                                Price = 10000m,
                                Amount = 1.0m
                            }
                        }
                    }
                }
            };

            // Act
            var result = _sellTradeExecutionService.ExecuteSellOrder(amountBTC, exchanges);

            // Assert
            result.ErrorMessage.Should().Be("Could not fulfill the order.");
        }
    }
}
