using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiTrader.DbClasses
{
    public class transactions_output
    {
        [Key] public int id { get; set; }
        public string name { get; set; }
        public string discordid { get; set; }
        public int amount { get; set; }
        public string time { get; set; }
        public bool isread { get; set; }
        public int status { get; set; }
        public string comment { get; set; } = null!;

        [JsonIgnore]
        public accounts accounts { get; set; } = null!;
    }
}
