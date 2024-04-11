using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiTrader.DbClasses
{
    public class tpay_cards
    {
        [Key] public int id { get; set; }
        public string discordid { get; set; }
        public string name { get; set; }
        public int balance { get; set; }
        public int tbonus { get; set; }
        public string token { get; set; }
        public string id_tr {  get; set; } 
        public List<tpay_transactions> transactions { get; set; } = [];

        [JsonIgnore]
        public accounts_tpay account { get; set; } = null!;
    }
}
