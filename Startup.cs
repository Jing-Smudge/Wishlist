using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(Wishlist.Startup))]

namespace Wishlist
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddHttpClient();

            builder.Services.AddScoped<BlobStorageContext>((s) => {
                return new BlobStorageContext();
            });

            builder.Services.AddScoped<TableStorageContext>((s) => {
                return new TableStorageContext();
            });

        }
    }
}