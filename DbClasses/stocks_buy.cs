using System.ComponentModel.DataAnnotations;

namespace ApiTrader.DbClasses
{
    public class stocks_buy
    {
        [Key] public int id { get; set; }
        public int stock_id { get; set; }
        public int stock_amount { get; set; }
        public string discordid { get; set; }
        public float stock_price_withcoef { get; set; }
        public string date {  get; set; }

        public accounts_stock accounts_stock { get; set; } = null!;
    }
}
