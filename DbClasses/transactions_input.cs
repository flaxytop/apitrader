using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace ApiTrader.DbClasses
{
    public class transactions_input
    {
        [Key] public int id { get; set; }
        public string discordid { get; set; }
        public DateTime time { get; set; }
        public int amount { get; set; }

        [JsonIgnore]
        public accounts accounts { get; set; } = null!;
        
    }
}
