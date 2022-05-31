using System.Collections.Generic;
using Azure.Data.Tables;
using Newtonsoft.Json;
using Wishlist.Models.Entities;

namespace Wishlist.Models.AppModels
{
    abstract public class AppModel<T> where T:AppEntity,new()
    {
        public string Id { get; set; }

        public virtual T MapToEntity( T entity)
        {
            return this.MapProps(entity);
        }
        public virtual T MapToEntity()
        {
            T t = new T();
            return this.MapProps(t);
        }
      abstract protected T MapProps(T entity);
        
        
    }
}