

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace ApiTrader.DbClasses
{
    public class accounts_stock
    {
        [Key] public string discordid { get; set; }

        [Column(TypeName = "jsonb")]
        public string stocks_json { get; set; } = null!;

        [JsonIgnore]
        public accounts accounts { get; set; } = null!;
        public List<stocks> stocks { get; set; } = [];
        public List<stocks_buy> stocks_buy { get; set; } = [];
    }
}
