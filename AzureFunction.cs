using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Wishlist;
using Azure.Data.Tables;
using System.Collections.Generic;
using Wishlist.Models;
using System.Linq;
using Wishlist.Models.AppModels;
using Wishlist.Models.Entities;
using System.Linq.Expressions;
using Wishlist.Models.Dtos;

namespace Wishlist
{
    public static class AzureFunction
    {
       

      

        [FunctionName("UploadFile")]
        public static async Task<IActionResult> UploadFile(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["productId"];
            BlobStorageContext blobStorage = new BlobStorageContext();
            var file = req.Form.Files[0];
            
            string blobName = await GetBlobNameAsync(id);
            if(blobName is null)
            {
                return new NotFoundResult();
            }

            using(var stream = file.OpenReadStream()){
                await blobStorage.UploadStreamAsync(WC.ProdutBlog,blobName,stream);
            }
            return new OkResult();
        }


        private static async Task<string> GetBlobNameAsync(string id)
        {
            TableStorageContext tableStorage = new TableStorageContext();
            ProductEntity product = await tableStorage.GetEntityByKeyAsync<ProductEntity>(WC.ProdcutTable,id);
            if(product is null)
            {
                return null;
            }
            string blobName = $"{product.PartitionKey}-{product.RowKey}";
            return blobName;
        }





        [FunctionName("Download")]
        public static async Task<IActionResult> Download(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["productId"];
            BlobStorageContext blobStorage = new BlobStorageContext();

            string blobName = await GetBlobNameAsync(id);
            if(blobName is null)
            {
                return new NotFoundResult();
            }
            var fileStream = await blobStorage.GetBlobStreamAsync(WC.ProdutBlog,blobName);

            MemoryStream ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);

            return new FileContentResult(ms.ToArray(), "image/jpeg");
        }
        











        [FunctionName("GetProduct")]
        public static async Task<IActionResult> GetProduct(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["productId"];
            TableStorageContext storage = new TableStorageContext();
            ProductEntity entity;
           try{
              entity =  await storage.GetEntityByKeyAsync<ProductEntity>(WC.ProdcutTable,id);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }
           if(entity is null)
           {
               return new NotFoundResult();
           }
            Product product = entity.MapEntityToModel();
            return new OkObjectResult(product);
        }
        

        
        [FunctionName("GetAllProducts")]
        public static async Task<IActionResult> GetAllProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            TableStorageContext storage = new TableStorageContext();
            List<ProductEntity> entities = new List<ProductEntity>();
           try{
              entities =  await storage.GetAllEntitiesAsync<ProductEntity>(WC.ProdcutTable);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }
           var products = entities.Select(ent => ent.MapEntityToModel());
            
            return new OkObjectResult(products);
        }


        [FunctionName("CreateProduct")]
        public static async Task<IActionResult> CreateProduct(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            dynamic data = JsonConvert.DeserializeObject<Product>(requestBody);
            data.Id=Guid.NewGuid().ToString();
        
            TableStorageContext storage = new TableStorageContext();
           try{
               await storage.AddEntityAsync(WC.ProdcutTable, data);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }

            return new OkObjectResult(data);
        }

        
         [FunctionName("DeleteProduct")]
        public static async Task<IActionResult> DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["productId"];
            TableStorageContext storage = new TableStorageContext();
            ProductEntity entity;
           try{
              entity =  await storage.GetEntityByKeyAsync<ProductEntity>(WC.ProdcutTable,id);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }
           if(entity is null)
           {
               return new NotFoundResult();
           }

            await storage.DeleteEntityAsync<ProductEntity>(WC.ProdcutTable,entity);
            return new OkResult();
        }
        

        [FunctionName("UpdateProduct")]
        public static async Task<IActionResult> UpdateProduct(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<Product>(requestBody);
            
            TableStorageContext storage = new TableStorageContext();
           try{
               await storage.UpdateEntityAsync<ProductEntity>(WC.ProdcutTable, data);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new NotFoundResult();
           }
            
            return new OkObjectResult(data);
        }







        [FunctionName("CreateUser")]
        public static async Task<IActionResult> CreateUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            dynamic data = JsonConvert.DeserializeObject<User>(requestBody);
            data.Id=Guid.NewGuid().ToString();
        
            TableStorageContext storage = new TableStorageContext();
           try{
               await storage.AddEntityAsync(WC.UserTable, data);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }

            return new OkObjectResult(data);
        }





        [FunctionName("GetList")]
        public static async Task<IActionResult> GetList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["userId"];
            TableStorageContext storage = new TableStorageContext();
            
           
          var entities =  await storage.GetEntityByFilter<ItemEntity>(WC.ItemTable,ent=>ent.CustomerId==id);
          List<WishItem> items = default;
          if(entities.Count()>0)
          {
              items = entities.Select(ent => ent.MapEntityToModel()).ToList();
          }
        
            return new OkObjectResult(items);
        }





        [FunctionName("AddToList")]
        public static async Task<IActionResult> AddToList(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            dynamic data = JsonConvert.DeserializeObject<WishItem>(requestBody);
            data.Id=Guid.NewGuid().ToString();
        
            TableStorageContext storage = new TableStorageContext();
            UserEntity user = await storage.GetEntityByKeyAsync<UserEntity>(WC.UserTable,data.CustomerId);
            ProductEntity product = await storage.GetEntityByKeyAsync<ProductEntity>(WC.ProdcutTable,data.ProductId);
            if(product is null || user is null)
            {
                return new NotFoundResult();
            }
           try{
               await storage.AddEntityAsync(WC.ItemTable, data);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }

            return new OkObjectResult(data);
        }


         [FunctionName("DeleteFromList")]
        public static async Task<IActionResult> DeleteFromList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["itemId"];
            TableStorageContext storage = new TableStorageContext();
            ItemEntity entity;
           try{
              entity =  await storage.GetEntityByKeyAsync<ItemEntity>(WC.ItemTable,id);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }
           if(entity is null)
           {
               return new NotFoundResult();
           }

            await storage.DeleteEntityAsync<ItemEntity>(WC.ItemTable,entity);
            return new OkResult();
        }


        [FunctionName("TogFavorate")]
        public static async Task<IActionResult> TogFavorate(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["itemId"];
            TableStorageContext storage = new TableStorageContext();
            ItemEntity entity;
           try{
              entity =  await storage.GetEntityByKeyAsync<ItemEntity>(WC.ItemTable,id);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }
           if(entity is null)
           {
               return new NotFoundResult();
           }
            WishItem item = entity.MapEntityToModel();
            item.IsFavorate = !item.IsFavorate;
            await storage.UpdateEntityAsync<ItemEntity>(WC.ItemTable,item);
            return new OkResult();
        }




        [FunctionName("GetLowStock")]
        public static async Task<IActionResult> GetLowStock(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            TableStorageContext storage = new TableStorageContext();
            

            var favorates = await storage.GetEntityByFilter<ItemEntity>(WC.ItemTable,ent=>ent.IsFavorate==true);
            
            Dictionary<string,int> favorateDict = new Dictionary<string, int>();

            foreach( var i in favorates)
            {
                if(!favorateDict.ContainsKey(i.PartitionKey))
                {
                    favorateDict.Add(i.PartitionKey,0);
                }
                favorateDict[i.PartitionKey] += 1;
            }

            var products = await storage.GetAllEntitiesAsync<ProductEntity>(WC.ProdcutTable);

            foreach(string key in favorateDict.Keys)
            {
                if(favorateDict[key]<products.FirstOrDefault(ent=>ent.RowKey==key).Quantity)
                {
                    favorateDict.Remove(key);
                }
            }








            List<LowStockDto> dtoList = new List<LowStockDto>();
            
            foreach( var ent in favorates)
            {
                LowStockDto dto = dtoList.FirstOrDefault(dto=>dto.ProductId==ent.PartitionKey);
                if(dto is null)
                {
                    dto = new LowStockDto()
                    {
                        ProductId=ent.PartitionKey,
                    };
                    dtoList.Add(dto);
                }
                ProductEntity product = await storage.GetEntityByKeyAsync<ProductEntity>(WC.ProdcutTable,ent.PartitionKey);
                dto.FavorateNumber +=1;
                dto.Quantity=product.Quantity;
            }

            foreach( var i in dtoList)
            {
                
                System.Console.WriteLine(dtoList.ToString());

            }












            return new OkObjectResult(dtoList);
        }











    }















}

