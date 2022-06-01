using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Collections;
using Wishlist.Models.Entities;

namespace Wishlist{

    public class BlobStorageContext
    {
        private BlobServiceClient blobServiceClient;
    

        public BlobStorageContext()
        {
            string connStr = Environment.GetEnvironmentVariable("connStr");
            blobServiceClient = new BlobServiceClient(connStr);
        }

        public async Task UploadStreamAsync (string containerName, string blobName, Stream fileStream)
        {
        var container = blobServiceClient.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();
        BlobClient blob = container.GetBlobClient(blobName);
            await blob.UploadAsync(fileStream, true);
        }

        public Task<Stream> GetBlobStreamAsync(string containerName,string blobName)
        {
            var container = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blob = container.GetBlobClient(blobName);
            return blob.OpenReadAsync();
        }


        public async Task<string> GetBlobNameFromProductAsync(TableStorageContext tableStorage, string id)
        {
            ProductEntity product = await tableStorage.GetEntityByKeyAsync<ProductEntity>(WC.ProdcutTable,id);
            if(product is null)
            {
                return null;
            }
            string blobName = $"{product.PartitionKey}-{product.RowKey}";
            return blobName;
        }
    }
}
