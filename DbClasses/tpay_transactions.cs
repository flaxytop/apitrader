using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ApiTrader.DbClasses
{
    public class tpay_transactions
    {
        [Key]public int id { get; set; }
        public string time { get; set; }
        public int amount { get; set; }
        public string to {  get; set; }
        public string comment { get; set; } = null!;
        public int card_id { get; set; }
        public int to_card_id { get; set; }
        public tpay_cards card { get; set; } = null!;
    }
}
