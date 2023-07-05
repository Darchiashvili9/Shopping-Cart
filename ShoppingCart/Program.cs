using Polly;
using ShoppingCart;
using ShoppingCart.EventFeed;
using ShoppingCart.ProductCatalogClient;
using ShoppingCart.ShoppingCart;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>()
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt))));

builder.Services.AddScoped<IShoppingCartStore, ShoppingCartStore>();
//builder.Services.AddScoped<IProductCatalogClient, ProductCatalogClient>();
builder.Services.AddScoped<IEventStore, EsEventStore>();
builder.Services.AddScoped<ICache,Cache>();


var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.MapControllers();


app.MapGet("/", () => "Hello World!");





app.Run();
