
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiTrader.DbClasses
{
    public class accounts
    {
        public string name { get; set; }
        [Key] public string discordid { get; set; }
        public string token { get; set; }
        public float balance { get; set; }
        public string uuid { get; set; }

        
        public List<transactions_input> transactions_input { get; set; } = [];
        public List<transactions_output> transactions_output { get; set; } = [];
        public accounts_stock accounts_stock { get; set; }
        public accounts_tpay accounts_tpay { get; set; }
    }
}
