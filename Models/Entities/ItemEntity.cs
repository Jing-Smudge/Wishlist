using System;
using Azure;
using Azure.Data.Tables;
using Wishlist.Models.AppModels;
using Wishlist.Models.Entities;

namespace Wishlist.Models
{
    public class ItemEntity : AppEntity
    {
        
      public bool IsFavorate { get; set; }
      public string CustomerId { get; set; }

        public WishItem MapEntityToModel()
        {
            WishItem item = new WishItem(){
                IsFavorate = this.IsFavorate,
                CustomerId = this.CustomerId,
                ProductId=this.PartitionKey,
                Id = this.RowKey

            };
            return item;
        }
    }
}