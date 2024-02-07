# Meta Exchange Task

The MetaExchange project simulates a meta-exchange platform, offering an algorithm to provide the best execution plan for trading Bitcoin (BTC) against the Euro (EUR). It aggregates order books from multiple simulated crypto exchanges to ensure users can buy or sell BTC at the best possible prices within given balance constraints.

## Project Structure

- **MetaExchange.ConsoleApp**: A .NET Core console application that when run gives you the best execution plan for trading BTC against EUR.

- **MetaExchange.API**: A .NET Core Web API exposing functionality through HTTP endpoints, allowing users to submit trading orders (buy/sell), the amount of BTC they wish to sell/buy and receive the best execution plan as a JSON response.

- **MetaExchange.Common**: Contains common models, interfaces, services and test demonstration data used by both the console application and the Web API.

## Testing

- **Unit tests** for core algorithm logic of my execution plan services, ensuring the best execution plan is returned for a given set of orders and balance constraints. The algorithm is tested putting it against different scenarios and edge cases to insure it's robustness.
- **Integration tests** for API endpoints, validating end-to-end functionality and response integrity.

## Running the Application

### Console Application

1. Navigate to the console app directory.
2. Execute `dotnet run` to start the application.
3. Follow the on-screen prompts to enter the order type and BTC amount.

### API

#### 1. Build and run the Docker container from the root directory

```bash
docker build -f ./src/MetaExchange.API/Dockerfile . -t metaexchange-api
```

#### 2. Run the container

```bash
docker run -p 8080:80 metaexchange-api
```

#### 3. Access Swagger UI at `http://localhost:8080/swagger` to interact with the API.

## Console Application Output Demo

```bash

Enter the order type (buy/sell):
buy
Enter the amount of BTC to trade(positive decimal):
2
--------------------------------------------------------------------------------
Order Execution Summary
--------------------------------------------------------------------------------
Order Type: Buy
BTC Ammount requested: 2.00000000

Execution Details:
- Exchange: Exchange A
  Action: Buy
  BTC: 0.40500000
  Price per BTC: 2964.29
  Total Cost EUR: 1200.54

- Exchange: Exchange B
  Action: Buy
  BTC: 0.40500000
  Price per BTC: 2964.29
  Total Cost EUR: 1200.54

- Exchange: Exchange A
  Action: Buy
  BTC: 0.40500000
  Price per BTC: 2964.3
  Total Cost EUR: 1200.54

- Exchange: Exchange B
  Action: Buy
  BTC: 0.40500000
  Price per BTC: 2964.3
  Total Cost EUR: 1200.54

- Exchange: Exchange A
  Action: Buy
  BTC: 0.38000000
  Price per BTC: 2965.0
  Total Cost EUR: 1126.70

--------------------------------------------------------------------------------
Summary:
Total BTC Acquired: 2.00000000
Average BTC Price: 2,964.43
Total Cost EUR: 5,928.86
--------------------------------------------------------------------------------
```
