using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.ApplicationInsights;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Configure OpenTelemetry with Azure Monitor
var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddAzureMonitorTraceExporter(options =>
            {
                options.ConnectionString = connectionString;
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddAzureMonitorMetricExporter(options =>
            {
                options.ConnectionString = connectionString;
            });
    });

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.AddAzureMonitorLogExporter(options =>
    {
        options.ConnectionString = connectionString;
    });
});

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

