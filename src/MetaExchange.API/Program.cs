using MetaExchange.Common.Interfaces;
using MetaExchange.Common.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IBuyTradeExecutionService, BuyTradeExecutionService>();
builder.Services.AddScoped<ISellTradeExecutionService, SellTradeExecutionService>();
builder.Services.AddSingleton<IExchangeDataContextService, ExchangeDataContextService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
