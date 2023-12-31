﻿using Microsoft.AspNetCore.Mvc;
using ShoppingCart.EventFeed;
using ShoppingCart.ProductCatalogClient;

namespace ShoppingCart.ShoppingCart
{
    [Route("/shoppingcart")]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartStore shoppingCartStore;
        private readonly IProductCatalogClient productCatalogClient;
        private readonly IEventStore eventStore;
        private readonly ILogger logger;
        public ShoppingCartController(IShoppingCartStore shoppingCartStore, IProductCatalogClient productCatalogClient, IEventStore eventStore, ILogger logger)
        {
            this.shoppingCartStore = shoppingCartStore;
            this.productCatalogClient = productCatalogClient;
            this.eventStore = eventStore;
            this.logger = logger;
        }

        [HttpGet("{userId:int}")]
        public async Task<ShoppingCart> Get(int userId)
        {
            var data = await this.shoppingCartStore.Get(userId);

            return data;
        }

        [HttpPost("{userId:int}/items")]
        public async Task<ShoppingCart> Post(int userId, [FromBody] int[] productIds)
        {
            var shoppingCart = await shoppingCartStore.Get(userId);
            var shoppingCartItems = await this.productCatalogClient.GetShoppingCartItems(productIds);
            shoppingCart.AddItems(shoppingCartItems, eventStore);

            await this.shoppingCartStore.Save(shoppingCart);

            //serilog აქ ვლოგავთ მარტივად!!!!!!!!!!!
            this.logger.LogInformation("Successfully added products to shopping cart {@productIds},{ @shoppingCart}", productIds, shoppingCart);

            return shoppingCart;
        }

        [HttpDelete("{userid:int}/items")]
        public async Task<ShoppingCart> Delete(int userId, [FromBody] int[] productIds)
        {
            var shoppingCart = await this.shoppingCartStore.Get(userId);
            shoppingCart.RemoveItems(productIds, this.eventStore);
            await this.shoppingCartStore.Save(shoppingCart);

            return shoppingCart;
        }
    }
}