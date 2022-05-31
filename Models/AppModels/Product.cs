using Wishlist.Models.Entities;

namespace Wishlist.Models.AppModels{



    public class Product : AppModel<ProductEntity>
    {
        
        public string Category {get;set;}
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

       



        protected override ProductEntity MapProps(ProductEntity entity)
        {
            entity.PartitionKey=this.Category;
            entity.RowKey=this.Id;
            entity.ProductName=this.ProductName;
            entity.Price=this.Price;
            entity.Quantity=this.Quantity;
            return entity;
        }

    }
}