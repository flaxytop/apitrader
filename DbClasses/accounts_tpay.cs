using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiTrader.DbClasses
{
    public class accounts_tpay
    {
        [Key] public string discordid { get; set; }
        public bool status_vip { get; set; }

        public List<tpay_cards> cards { get; set; } = [];

        [JsonIgnore]
        public accounts account { get; set; }
    }
}
