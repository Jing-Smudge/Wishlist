using System;
using System.Collections.Generic;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using Wishlist.Models.AppModels;

namespace Wishlist.Models.Entities
{
    
    public class AppEntity: ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
