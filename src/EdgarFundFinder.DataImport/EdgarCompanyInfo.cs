using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EdgarFundFinder.DataImport
{
    public class EdgarCompanyInfo
    {
        [JsonProperty("id")]
        public string Id => Cik.ToString();

        [JsonProperty("cik")]
        [Required]
        [JsonConverter(typeof(CikConverter))]
        public required object Cik { get; set; }

        [JsonProperty("entityName")]
        public required string EntityName { get; set; }

        [JsonProperty("facts")]
        public required InfoFact Facts { get; set; }
    }

    public class InfoFact
    {
        [JsonProperty("us-gaap")]
        public InfoFactUsGaap UsGaap { get; set; }
    }

    public class InfoFactUsGaap
    {
        public InfoFactUsGaapNetIncomeLoss NetIncomeLoss { get; set; }
    }

    public class InfoFactUsGaapNetIncomeLoss
    {
        [JsonProperty("units")]
        public required InfoFactUsGaapIncomeLossUnits Units { get; set; }
    }

    public class InfoFactUsGaapIncomeLossUnits
    {
        [JsonProperty("USD")]
        public InfoFactUsGaapIncomeLossUnitsUsd[] Usd { get; set; }
    }

    public class InfoFactUsGaapIncomeLossUnitsUsd
    {
        /// <summary>
        /// Possibilities include 10-Q, 10-K,8-K, 20-F, 40-F, 6-K, and their variants.YOU ARE INTERESTED ONLY IN 10-K DATA!
        /// </summary>
        [JsonProperty("form")]
        public required string Form { get; set; }

        /// <summary>
        /// For yearly information, the format is CY followed by the year number.For example: CY2021.
        /// YOU ARE INTERESTED ONLY IN YEARLY INFORMATION WHICH FOLLOWS THIS FORMAT!
        /// </summary>
        [JsonProperty("frame")]
        public string Frame { get; set; }

        /// <summary>
        /// The income/loss amount.
        /// </summary>
        [JsonProperty("val")]
        public decimal Val { get; set; }
    }
}