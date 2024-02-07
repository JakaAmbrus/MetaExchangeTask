using MetaExchange.API.Models;
using MetaExchange.Common.Interfaces;
using MetaExchange.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace MetaExchange.API.Controllers
{
    /// <summary>
    /// Handles trade execution requests.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TradeController : ControllerBase
    {
        private readonly IBuyTradeExecutionService _buyExecutionService;
        private readonly ISellTradeExecutionService _sellExecutionService;
        private readonly IExchangeDataContextService _seedExecutionData;

        public TradeController(IBuyTradeExecutionService buyExecutionService,
            ISellTradeExecutionService sellExecutionService,
            IExchangeDataContextService seedExecutionData)
        {
            _buyExecutionService = buyExecutionService;
            _sellExecutionService = sellExecutionService;
            _seedExecutionData = seedExecutionData;
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteTrade([FromBody] TradeRequestDto request)
        {
            // Here I would just like to point out that I am passing in the exchange data as parameter
            // due to the assignment scope and requirements, this is in no way a representation
            // of passing in a database context to a service, which is a bad practice.
            var exchanges = await _seedExecutionData.GetExchangeDataAsync();

            TradeOrderResult result;

            switch (request.OrderType.ToLower())
            {
                case "buy":
                    result = _buyExecutionService.ExecuteBuyOrder(request.Amount, exchanges);
                    break;
                case "sell":
                    result = _sellExecutionService.ExecuteSellOrder(request.Amount, exchanges);
                    break;
                default:
                    return BadRequest("Order type must be either 'buy' or 'sell'.");
            }

            return Ok(result);
        }
    }
}
