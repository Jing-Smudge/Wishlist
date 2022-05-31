using Wishlist.Models.AppModels;

namespace Wishlist.Models.Entities
{
    public class ProductEntity:AppEntity
    {
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string ProductName { get; set; }

        public Product MapEntityToModel()
        {
            Product product = new Product();
            product.Category=this.PartitionKey;
            product.Id=this.RowKey;
            product.Price=this.Price;
            product.Quantity=this.Quantity;
            product.ProductName=this.ProductName;
            return product;
        }
    }
}