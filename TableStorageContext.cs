using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Wishlist.Models;
using System.Linq;
using System.Linq.Expressions;
using Wishlist.Models.AppModels;
using Wishlist.Models.Entities;

namespace Wishlist
{
    public class TableStorageContext
    {
        private const string connStr = @"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

        public TableServiceClient serviceClient;
        

        public TableStorageContext()
        {
            serviceClient = new TableServiceClient(connStr);
        }


        public async Task<TableClient> GetTableClientAsync(string tableName)
        {
           await serviceClient.CreateTableIfNotExistsAsync(tableName);
           
           return serviceClient.GetTableClient(tableName);
        }

    
      


        public async Task DeleteFromTable<T>(string tableName,TableEntity entity)
        {
                TableClient client = await this.GetTableClientAsync(tableName);
                await client.DeleteEntityAsync(entity.PartitionKey,entity.RowKey);
        }


        public async Task<List<T>> GetAllEntitiesAsync<T>(string tableName) where T:AppEntity,new()
        {
                TableClient client = await this.GetTableClientAsync(tableName);
                List<T> result = client.Query<T>().ToList();
                return result;
        }




        public async Task<T> GetEntityByKeyAsync<T>(string tableName, string rowKey) where T:AppEntity,new()
        {
            TableClient client = await this.GetTableClientAsync(tableName);
            Pageable<T> result = client.Query<T>(ent=>ent.RowKey==rowKey);
            T entity = result.FirstOrDefault();
            return entity;
        
        }



        public async Task<Pageable<T>> GetEntityByFilter<T>(string tableName,Expression<Func<T,bool>> filter)where T:AppEntity,new()
        {
            TableClient client = await this.GetTableClientAsync(tableName);
            Pageable<T> result =  client.Query<T>(filter);
            return result;

        }

        public async Task DeleteEntityAsync<T>(string tableName, T entity)where T:AppEntity,new()
        {
            TableClient client = await this.GetTableClientAsync(tableName);
            await client.DeleteEntityAsync(entity.PartitionKey,entity.RowKey);
        
        }


  
        public async Task AddEntityAsync<T>(string tableName,AppModel<T> model) where T:AppEntity,new()
        {
            TableClient client = await this.GetTableClientAsync(tableName);
            var entity = model.MapToEntity();
            await client.AddEntityAsync(entity);

        }

        public async Task UpdateEntityAsync<T>(string tableName,AppModel<T> model) where T:AppEntity,new()
        {
            TableClient client = await this.GetTableClientAsync(tableName);
            T entityFromTable = await this.GetEntityByKeyAsync<T>(tableName,model.Id);
            if(entityFromTable is null)
            {
                throw new Exception("Not found");
            }
            T mappedEntity = model.MapToEntity(entityFromTable);
            await client.UpsertEntityAsync<T>(mappedEntity);
        }





    }
}
