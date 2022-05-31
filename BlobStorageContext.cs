using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows;


namespace Wishlist{

    public class BlobStorageContext
    {
        private BlobServiceClient blobServiceClient;
        private const string connStr = @"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
    

        public BlobStorageContext()
        {
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






        
        



    }


}