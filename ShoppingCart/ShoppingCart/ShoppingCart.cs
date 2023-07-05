using ShoppingCart.EventFeed;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCart
    {
        private readonly HashSet<ShoppingCartItem> items = new HashSet<ShoppingCartItem>();
        public int? Id { get; }

        public int UserId { get; }
        public IEnumerable<ShoppingCartItem> Items()
        {
            return this.items;
        }
        public ShoppingCart(int userId)
        {
            this.UserId = userId;
        }

        public ShoppingCart(int? id, int userId, IEnumerable<ShoppingCartItem> _items)
        {
            this.Id = id;
            this.UserId = userId;
            this.items = _items.ToHashSet();
        }


        public void AddItems(IEnumerable<ShoppingCartItem> shoppingCartItems, IEventStore eventStore)
        {
            foreach (var item in shoppingCartItems)
                if (this.items.Add(item))
                    //აქ ვიძახებთ ივენთებს, ჩვეულებრივი ლოგირების პონტია სინამდვილეში
                    eventStore.Raise("ShoppingCartItemAdded", new { UserId, item });
        }
        public void RemoveItems(int[] productCatalogueIds, IEventStore eventStore)
        {
            // this.items.RemoveWhere(i => productCatalogueIds.Contains(i.ProductCatalogueId));

            foreach (var item in this.items)
            {
                if (productCatalogueIds.Contains(item.ProductCatalogueId))
                {
                    eventStore.Raise("ShoppingCartItemDeleted", new { UserId, item });
                    this.items.Remove(item);
                }
            }
        }
    }

    public record ShoppingCartItem(int ProductCatalogueId, string ProductName, string Description, Money Price)
    {
        public virtual bool Equals(ShoppingCartItem? obj) => obj != null && this.ProductCatalogueId.Equals(obj.ProductCatalogueId);

        public override int GetHashCode() => this.ProductCatalogueId.GetHashCode();
    }

    public record Money(string Currency, decimal Amount);
}
