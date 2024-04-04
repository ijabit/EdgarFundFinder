
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Configuration;

namespace EdgarFundFinder.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var cosmosDbSettings = builder.Configuration.GetSection("CosmosDbSettings").Get<CosmosDbSettings>();

            builder.Services.AddSingleton<CosmosClient>(sp =>
            {
                return new CosmosClient(cosmosDbSettings.ConnectionString);
            });

            builder.Services.AddSingleton<IOptions<CosmosDbSettings>>(sp =>
            {
                return Options.Create(cosmosDbSettings);
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
