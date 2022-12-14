using CashFlow.Common.API.Responses;
using CashFlow.Common.Messaging;
using CashFlow.Domain.DTOs;
using CashFlow.Domain.Messaging;
using CashFlow.Domain.Repository;
using CashFlow.Domain.Services;
using CashFlow.Messaging;
using CashFlow.Operation;
using CashFlow.Persistence;
using CashFlow.Persistence.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("cashflowdb");

builder.Services.AddDbContext<CashFlowDataContext>(opt =>
{
    if (string.IsNullOrEmpty(connectionString))
        opt.UseInMemoryDatabase("cashflowdb");
    else
        opt.UseNpgsql(connectionString);
}, ServiceLifetime.Singleton);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.EnableAnnotations();
    opt.TagActionsBy(d => new List<string> { d.ActionDescriptor.DisplayName });
});

builder.Services.AddTransient<ICashInRepository, CashInRepository>();
builder.Services.AddTransient<ICashOutRepository, CashOutRepository>();
builder.Services.AddTransient<CashInService>();
builder.Services.AddTransient<CashOutService>();

builder.Services.AddTransient<IMessageQueueConfiguration, MessageQueueConfiguration>();
builder.Services.AddTransient<IMessage, MessageModel>();
builder.Services.AddTransient<IMessageSender, MessageSender>();

var app = builder.Build();

app.AppUseMigrations();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Welcome to CashFlow Operations.");

app.MapPost("CashIn", async (CashInService service, [FromBody] CashInDTO dto) =>
{
    var result = await service.Save(dto.Amount, dto.Date);
    return AppResults.Ok(result);
});

app.MapPost("CashOut", async (CashOutService service, [FromBody] CashOutDTO dto) =>
{
    var result = await service.Save(dto.Amount, dto.Date);
    return AppResults.Ok(result);
});

app.Run();
