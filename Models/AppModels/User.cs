using System.Collections.Generic;
using Azure.Data.Tables;
using Newtonsoft.Json;
using Wishlist.Models.Entities;

namespace Wishlist.Models.AppModels
{
    public class User:AppModel<UserEntity>
    {
        public string Name { get; set; }
        public string Role { get; set; }
        

        protected override UserEntity MapProps(UserEntity entity)
        {
            entity.PartitionKey= this.Role;
            entity.RowKey = this.Id;
            entity.Name=this.Name;
            return entity;
        }
    }
}