using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace ApiTrader.DbClasses
{
    public class stocks 
    {
        [Key] public int id { get; set; }
        public string name { get; set; }
        public int price { get; set; }
        public float coefficient { get; set; }
        public string? token { get; set; } = null!;
        public string? description { get; set; } = null!;
        public string discordid { get; set; }

        [Column(TypeName = "jsonb")]
        public string? history { get; set; }
        public string? created_at { get; set; }
        public byte[]? icon { get; set; } = null!;

        [JsonIgnore]
        public accounts_stock accounts_stock { get; set; } = null!;
    }
}
