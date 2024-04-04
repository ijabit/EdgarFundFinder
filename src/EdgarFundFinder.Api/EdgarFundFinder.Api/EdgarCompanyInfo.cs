using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EdgarFundFinder.Api
{
    public class EdgarCompanyInfo
    {
        [JsonProperty("cik")]
        [Required]
        public required string Cik { get; set; }

        [JsonProperty("entityName")]
        public required string EntityName { get; set; }

        [JsonProperty("filteredIncomeData")]
        public List<IncomeData> FilteredIncomeData { get; set; }
    }

    public class IncomeData
    {
        [JsonProperty("form")]
        public string Form { get; set; }

        [JsonProperty("frame")]
        public string Frame { get; set; }

        [JsonProperty("val")]
        public decimal Val { get; set; }
    }
}
