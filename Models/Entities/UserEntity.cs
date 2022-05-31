using System;
using Azure;
using Azure.Data.Tables;
using Wishlist.Models.AppModels;
using Wishlist.Models.Entities;

namespace Wishlist.Models
{
    public class UserEntity : AppEntity
    {
       
      public string Name { get; set; }

       public User MapEntityToModel()
       {
         User customer = new User()
         {
           Name=this.Name,
           Id=this.RowKey,
           Role=this.PartitionKey
         };

         return customer;
       }
    }
}