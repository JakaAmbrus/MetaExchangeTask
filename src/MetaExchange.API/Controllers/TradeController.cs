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

        public TradeController(IBuyTradeExecutionService buyExecutionService,
            ISellTradeExecutionService sellExecutionService)
        {
            _buyExecutionService = buyExecutionService;
            _sellExecutionService = sellExecutionService;
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteTrade([FromBody] TradeRequestDto request)
        {
            TradeOrderResult result;

            switch (request.OrderType.ToLower())
            {
                case "buy":
                    result = await _buyExecutionService.ExecuteBuyOrderAsync(request.Amount);
                    break;
                case "sell":
                    result = await _sellExecutionService.ExecuteSellOrderAsync(request.Amount);
                    break;
                default:
                    return BadRequest("Order type must be either 'buy' or 'sell'.");
            }

            return Ok(result);
        }
    }
}
