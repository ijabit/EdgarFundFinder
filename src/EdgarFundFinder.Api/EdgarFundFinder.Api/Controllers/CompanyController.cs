using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace EdgarFundFinder.Api
{
    [ApiController]
    [Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseId;
        private readonly string _containerId;

        public CompanyController(CosmosClient cosmosClient, IOptions<CosmosDbSettings> cosmosDbSettings)
        {
            // In a production application I'd move these dependencies into the Application layer and all of the logic below as well.
            // I'd use Mediatr to pass the query request on to the Application layer leaving the controller with minimal code.
            var settings = cosmosDbSettings.Value;
            _cosmosClient = cosmosClient;
            _databaseId = settings.DatabaseName;
            _containerId = settings.ContainerName;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies([FromQuery] string companyName = null)
        {
            var container = _cosmosClient.GetContainer(_databaseId, _containerId);
            var baseQuery = "SELECT c.cik, c.entityName, ARRAY(SELECT VALUE t FROM t IN c.facts[\"us-gaap\"].NetIncomeLoss.units.USD WHERE t.frame >= 'CY2018' AND t.frame <= 'CY2022' AND t.form = '10-K') AS filteredIncomeData FROM c";
            var query = companyName != null ? $"{baseQuery} WHERE LOWER(c.entityName) LIKE LOWER('{companyName}%')" : baseQuery;

            var queryDefinition = new QueryDefinition(query);
            var queryResultSetIterator = container.GetItemQueryIterator<EdgarCompanyInfo>(queryDefinition);

            var results = new List<CompanyFundingInfo>();
            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var item in currentResultSet)
                {
                    var companyFundingInfo = GetCompanyFundingInfo(item);
                    results.Add(companyFundingInfo);
                }
            }

            return Ok(results);
        }

        private CompanyFundingInfo GetCompanyFundingInfo(EdgarCompanyInfo item)
        {
            var incomeData = item.FilteredIncomeData;
            var requiredYears = new[] { "CY2018", "CY2019", "CY2020", "CY2021", "CY2022" };

            var yearlyIncomeData = requiredYears.Select(year =>
            {
                var yearlyData = incomeData.Find(d => d.Frame == year);
                if (yearlyData != null)
                {
                    return new { Year = year, Amount = yearlyData.Val };
                }
                else
                {
                    // Sometimes only quarterly reports are available. If all 4 quarters are listed, sum them and treat that as the yearly value
                    var quarterlyData = incomeData.Where(d => d.Frame.StartsWith(year + "Q")).ToList();
                    var sum = quarterlyData.Count == 4 ? quarterlyData.Sum(d => d.Val) : 0m;
                    return new { Year = year, Amount = sum };
                }
            }).ToList();

            var hasIncomeDataForAllYears = yearlyIncomeData.All(data => data.Amount > 0m);
            var hasPositiveIncomeFor2021And2022 = incomeData.Exists(d => d.Frame == "CY2021" && d.Val > 0m) && incomeData.Exists(d => d.Frame == "CY2022" && d.Val > 0m);

            if (!hasIncomeDataForAllYears || !hasPositiveIncomeFor2021And2022)
            {
                return new CompanyFundingInfo(
                    item.Cik,
                    item.EntityName,
                    0m, // Standard Fundable Amount is $0 if the company does not meet the criteria
                    0m // Special Fundable Amount is $0 if the company does not meet the criteria
                );
            }

            var highestIncome = yearlyIncomeData.Max(data => data.Amount);
            var standardFundableAmount = highestIncome >= 10_000_000_000m ? highestIncome * 0.1233m : highestIncome * 0.2151m;

            var specialFundableAmount = standardFundableAmount;
            if (new List<char> { 'a', 'e', 'i', 'o', 'u' }.Exists(c => item.EntityName.ToLower().StartsWith(c.ToString())))
            {
                specialFundableAmount += standardFundableAmount * 0.15m;
            }
            if (incomeData.Find(d => d.Frame == "CY2022")?.Val < incomeData.Find(d => d.Frame == "CY2021")?.Val)
            {
                specialFundableAmount -= standardFundableAmount * 0.25m;
            }

            return new CompanyFundingInfo(
                item.Cik,
                item.EntityName,
                standardFundableAmount,
                specialFundableAmount
            );
        }
    }
}