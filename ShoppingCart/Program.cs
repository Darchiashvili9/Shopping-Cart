﻿using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using ProtoBuf.Meta;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Json;
using ShoppingCart;
using ShoppingCart.EventFeed;
using ShoppingCart.ProductCatalogClient;
using ShoppingCart.ShoppingCart;

var builder = WebApplication.CreateBuilder(args);


//სერილოგის იმპლემენტაცია!!!!!!!!
var logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .WriteTo.Console(outputTemplate: @"{Timestamp:yyyy-MM-dd HH:mm:ss}{TraceId} {Level:u3} {Message}{NewLine}{Exception}")
    .CreateLogger();
builder.Logging.AddSerilog(logger);


builder.Services
    .AddHealthChecks()
    .AddCheck<DbHealthCheck>(nameof(DbHealthCheck), tags: new[] { "startup" })
    .AddCheck("LivenessHealthCheck", () => HealthCheckResult.Healthy(), tags: new[] { "liveness" });


builder.Services.AddControllers();

builder.Services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>()
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt))));

builder.Services.AddScoped<IShoppingCartStore, ShoppingCartStore>();
//builder.Services.AddScoped<IProductCatalogClient, ProductCatalogClient>();
builder.Services.AddScoped<IEventStore, EsEventStore>();
builder.Services.AddScoped<ICache, Cache>();



var app = builder.Build();



app.UseHttpsRedirection();


app.UseHealthChecks("/health/startup",
    new HealthCheckOptions
    {
        Predicate = x => x.Tags.Contains("startup")
    });

app.UseHealthChecks("/health/live",
    new HealthCheckOptions
    {
        Predicate = x => x.Tags.Contains("liveness")
    });



app.UseRouting();
app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();
