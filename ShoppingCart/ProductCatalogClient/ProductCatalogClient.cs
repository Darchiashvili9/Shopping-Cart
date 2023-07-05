using Microsoft.AspNetCore.Http.HttpResults;
using ShoppingCart.ShoppingCart;
using System;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ShoppingCart.ProductCatalogClient
{
    /// <summary>
    /// იდეაში ამას თავისი ბაზა უნდა ჰქონდეს სადაც მოვა პროდუქტ აიდი და თავის კატალოგის ბაზაში მონახავს ამ
    /// პროდუქტს დეტალურად; ანუ კატალოგია და თავისი დეტალური ბაზა აქვს ამ კატალოგის;
    /// </summary>
    public interface IProductCatalogClient
    {
        Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItems(int[] productIds);
    }

    public class ProductCatalogClient : IProductCatalogClient
    {
        private readonly HttpClient client;
        private readonly ICache cache;
        private static string productCatalogBaseUrl = @"https://localhost:7096/products";
        private static string getProductPathTemplate = "?productIds=[{0}]";

        public ProductCatalogClient(HttpClient client, ICache cache)
        {
            client.BaseAddress = new Uri(productCatalogBaseUrl);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this.client = client;
            this.cache = cache;
        }

        public async Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItems(int[] productCatalogIds)
        {
            using var response = await RequestProductFromProductCatalog(productCatalogIds);
            return await ConvertToShoppingCartItems(response);
        }

        private async Task<HttpResponseMessage> RequestProductFromProductCatalog(int[] productCatalogIds)
        {
            var productsResource = string.Format(getProductPathTemplate, string.Join(",", productCatalogIds));

            var response = this.cache.Get(productsResource) as HttpResponseMessage;
            if (response is null)
            {
                response = await this.client.GetAsync(productsResource);
                AddToCache(productsResource, response);
            }
            return response;
        }

        private void AddToCache(string resource, HttpResponseMessage response)
        {
            var cacheHeader = response.Headers.FirstOrDefault(h => h.Key == "Cache-Control");
            var val = cacheHeader.Value.FirstOrDefault();

            if (!string.IsNullOrEmpty(cacheHeader.Key) && CacheControlHeaderValue.TryParse(val, out var cacheControl) && cacheControl!.MaxAge.HasValue)
                this.cache.Add(resource, response, cacheControl.MaxAge.Value);
        }

        private static async Task<IEnumerable<ShoppingCartItem>> ConvertToShoppingCartItems(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var products =
                await JsonSerializer.DeserializeAsync<List<ProductCatalogProduct>>(
                    await response.Content.ReadAsStreamAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            return products.Select(p => new ShoppingCartItem(p.ProductId, p.ProductName, p.ProductDescription, p.Price));
        }
        private record ProductCatalogProduct(int ProductId, string ProductName, string ProductDescription, Money Price);
    }

}
