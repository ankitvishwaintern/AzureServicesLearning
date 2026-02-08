using Microsoft.ApplicationInsights;
using OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Insights - this automatically configures OpenTelemetry with Azure Monitor
builder.Services.AddApplicationInsightsTelemetry();

// OpenTelemetry is automatically configured with Azure Monitor when using AddApplicationInsightsTelemetry()
// This includes tracing, metrics, and logging exporters for Azure Monitor

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

