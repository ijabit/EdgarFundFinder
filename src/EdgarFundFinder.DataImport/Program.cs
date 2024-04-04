using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace EdgarFundFinder.DataImport
{
    internal class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        static async Task Main(string[] args)
        {
            // Initialize HttpClient with default headers
            httpClient.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.34.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));


            var apiSettings = new EdgarApiSettings();
            Configuration.GetSection("EdgarApiSettings").Bind(apiSettings);
            if (string.IsNullOrEmpty(apiSettings.BaseUrl) ||
                apiSettings.CIKNumbers == null || !apiSettings.CIKNumbers.Any())
            {
                Console.WriteLine("API settings are missing or incomplete.");
                return;
            }

            var cosmosDbSettings = new CosmosDbSettings();
            Configuration.GetSection("CosmosDbSettings").Bind(cosmosDbSettings);

            if (string.IsNullOrEmpty(cosmosDbSettings.ConnectionString) ||
                string.IsNullOrEmpty(cosmosDbSettings.DatabaseName) ||
                string.IsNullOrEmpty(cosmosDbSettings.ContainerName))
            {
                Console.WriteLine("CosmosDB settings are missing or incomplete.");
                return;
            }

            await ImportDataFromApiToCosmos(apiSettings, cosmosDbSettings);
        }

        private static async Task ImportDataFromApiToCosmos(EdgarApiSettings apiSettings, CosmosDbSettings cosmosDbSettings)
        {            
            CosmosClient cosmosClient = new CosmosClient(cosmosDbSettings.ConnectionString);
            var container = cosmosClient.GetContainer(cosmosDbSettings.DatabaseName, cosmosDbSettings.ContainerName);

            foreach (var cikNumber in apiSettings.CIKNumbers)
            {
                string paddedCikNumber = cikNumber.PadLeft(10, '0');
                Uri baseUri = new Uri(apiSettings.BaseUrl);
                Uri combinedUri = new Uri(baseUri, $"CIK{paddedCikNumber}.json");

                try
                {
                    using (var response = await httpClient.GetAsync(combinedUri))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            Console.WriteLine($"CIK number {paddedCikNumber} not found, skipping.");
                            continue; // Skip to the next CIK number
                        }

                        response.EnsureSuccessStatusCode();
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                var companyInfo = JsonConvert.DeserializeObject<EdgarCompanyInfo>(streamReader.ReadToEndAsync().Result);
                                if (companyInfo is null)
                                {
                                    throw new JsonException($"Failed to deserialize the JSON stream for CIK number {paddedCikNumber}.");
                                }

                                try
                                {
                                    await container.UpsertItemAsync(companyInfo, new PartitionKey(companyInfo.Cik.ToString()));
                                    Console.WriteLine($"Successfully loaded CIK number {paddedCikNumber}.");
                                }
                                catch (CosmosException ex)
                                {
                                    Console.WriteLine($"Error uploading CIK number {paddedCikNumber} to CosmosDB");
                                    Console.WriteLine($"CosmosException: {ex.Message}");
                                    Console.WriteLine($"Status Code: {ex.StatusCode}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing CIK number {paddedCikNumber}: {ex.Message}");
                    // Optionally, log the exception details for further analysis
                }
            }
        }
    }
}