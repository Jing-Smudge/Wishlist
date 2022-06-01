using Azure.Data.Tables;
using Wishlist.Models.Entities;

namespace Wishlist.Models.AppModels
{
    public class WishItem:AppModel<ItemEntity>
    {
      public string CustomerId { get; set; }
      public string ProductId { get; set; }
      public bool IsFavorate { get; set; }
        protected override ItemEntity MapProps(ItemEntity entity)
        {
            entity.PartitionKey= this.ProductId;
            entity.RowKey = this.Id;
            entity.CustomerId=this.CustomerId;
            entity.IsFavorate=this.IsFavorate;
            return entity;
        }
    }
}
