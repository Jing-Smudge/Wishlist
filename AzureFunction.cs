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
    public class AzureFunction
    {
       private  readonly BlobStorageContext blobStorage;
       private readonly TableStorageContext tableStorage;

      public AzureFunction(BlobStorageContext _blobStorage,TableStorageContext _tableStorage)
      {
          blobStorage=_blobStorage;
          tableStorage = _tableStorage;
      }


        [FunctionName("UploadFile")]
        public async Task<IActionResult> UploadFile(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["productId"];
            var file = req.Form.Files[0];
            
            string blobName = await blobStorage.GetBlobNameFromProductAsync(tableStorage,id);
            if(blobName is null)
            {
                return new NotFoundResult();
            }
            using(var stream = file.OpenReadStream()){
                await blobStorage.UploadStreamAsync(WC.ProdutBlog,blobName,stream);
            }
            return new OkResult();
        }



        [FunctionName("Download")]
        public async Task<IActionResult> Download(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["productId"];
            string blobName = await blobStorage.GetBlobNameFromProductAsync(tableStorage, id);
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
        public async Task<IActionResult> GetProduct(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["productId"];
            ProductEntity entity;
           try{
              entity =  await tableStorage.GetEntityByKeyAsync<ProductEntity>(WC.ProdcutTable,id);
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
        public async Task<IActionResult> GetAllProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<ProductEntity> entities = new List<ProductEntity>();
           try{
              entities =  await tableStorage.GetAllEntitiesAsync<ProductEntity>(WC.ProdcutTable);
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
        public  async Task<IActionResult> CreateProduct(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<Product>(requestBody);
            data.Id=Guid.NewGuid().ToString();
           try{
               await tableStorage.AddEntityAsync(WC.ProdcutTable, data);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }
            return new OkObjectResult(data);
        }

        
         [FunctionName("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["productId"];
            ProductEntity entity;
           try{
              entity =  await tableStorage.GetEntityByKeyAsync<ProductEntity>(WC.ProdcutTable,id);
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
            await tableStorage.DeleteEntityAsync<ProductEntity>(WC.ProdcutTable,entity);
            return new OkResult();
        }
        

        [FunctionName("UpdateProduct")]
        public  async Task<IActionResult> UpdateProduct(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<Product>(requestBody);
           try{
               await tableStorage.UpdateEntityAsync<ProductEntity>(WC.ProdcutTable, data);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new NotFoundResult();
           }
            return new OkObjectResult(data);
        }


        [FunctionName("CreateUser")]
        public  async Task<IActionResult> CreateUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            dynamic data = JsonConvert.DeserializeObject<User>(requestBody);
            data.Id=Guid.NewGuid().ToString();
           try{
               await tableStorage.AddEntityAsync(WC.UserTable, data);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }
            return new OkObjectResult(data);
        }


        [FunctionName("GetList")]
        public  async Task<IActionResult> GetList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
          string id = req.Query["userId"];
          var entities =  await tableStorage.GetEntityByFilter<ItemEntity>(WC.ItemTable,ent=>ent.CustomerId==id);
          List<WishItem> items = default;
          if(entities.Count()>0)
          {
              items = entities.Select(ent => ent.MapEntityToModel()).ToList();
          }
            return new OkObjectResult(items);
        }


        [FunctionName("AddToList")]
        public async Task<IActionResult> AddToList(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<WishItem>(requestBody);
            data.Id=Guid.NewGuid().ToString();
            UserEntity user = await tableStorage.GetEntityByKeyAsync<UserEntity>(WC.UserTable,data.CustomerId);
            ProductEntity product = await tableStorage.GetEntityByKeyAsync<ProductEntity>(WC.ProdcutTable,data.ProductId);
            if(product is null || user is null)
            {
                return new NotFoundResult();
            }
           try{
               await tableStorage.AddEntityAsync(WC.ItemTable, data);
           }    
           catch(Exception ex)
           {
               log.LogError(ex,ex.ToString());
               return new BadRequestResult();
           }
            return new OkObjectResult(data);
        }


         [FunctionName("DeleteFromList")]
        public  async Task<IActionResult> DeleteFromList(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["itemId"];
            ItemEntity entity;
           try{
              entity =  await tableStorage.GetEntityByKeyAsync<ItemEntity>(WC.ItemTable,id);
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
            await tableStorage.DeleteEntityAsync<ItemEntity>(WC.ItemTable,entity);
            return new OkResult();
        }


        [FunctionName("TogFavorate")]
        public async Task<IActionResult> TogFavorate(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req,
            ILogger log)
        {
            string id = req.Query["itemId"];
            ItemEntity entity;
           try{
              entity =  await  tableStorage.GetEntityByKeyAsync<ItemEntity>(WC.ItemTable,id);
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
            await tableStorage.UpdateEntityAsync<ItemEntity>(WC.ItemTable,item);
            return new OkResult();
        }


        [FunctionName("GetLowStock")]
        public  async Task<IActionResult> GetLowStock(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {            
            var favorates = await tableStorage.GetEntityByFilter<ItemEntity>(WC.ItemTable,ent=>ent.IsFavorate==true);                      
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
                    ProductEntity product = await tableStorage.GetEntityByKeyAsync<ProductEntity>(WC.ProdcutTable,ent.PartitionKey);
                    dto.Quantity=product.Quantity;
                    dtoList.Add(dto);
                }
                dto.FavorateNumber +=1;
            }
           // var lowStockList = dtoList.Where(dto=>dto.FavorateNumber>=dto.Quantity);
            return new OkObjectResult(dtoList);
        }
    }
}
