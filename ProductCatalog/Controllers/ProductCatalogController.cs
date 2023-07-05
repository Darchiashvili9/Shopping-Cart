using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog
{
    [Route("/products")]
    public class ProductCatalogController : ControllerBase
    {
        private readonly IProductStore productStore;
        public ProductCatalogController(IProductStore productStore)
        {
            this.productStore = productStore;
        }

        [HttpGet("")]
        [ResponseCache(Duration = 86400)]
        public IEnumerable<ProductCatalogProduct> Get([FromQuery] string productIds)
        {
            var products = this.productStore.GetProductsByIds(ParseProductIdsFromQueryString(productIds));
            return products;
        }

        private static IEnumerable<int> ParseProductIdsFromQueryString(string productIdsString)
        {
            return productIdsString.Split(',').Select(s => s.Replace("[", "").Replace("]", "")).Select(int.Parse);
        }
    }

    public interface IProductStore
    {
        IEnumerable<ProductCatalogProduct> GetProductsByIds(IEnumerable<int> productIds);
    }

    public class ProductStore : IProductStore
    {
        public IEnumerable<ProductCatalogProduct> GetProductsByIds(IEnumerable<int> productIds)
        {
            return productIds.Select(id => new ProductCatalogProduct(id, "foo" + id, "bar", new Money()));
        }
    }

    public record ProductCatalogProduct(int ProductId, string ProductName, string ProductDescription, Money Price);

    public record Money();
}
