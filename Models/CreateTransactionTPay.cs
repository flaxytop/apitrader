using System.Diagnostics.CodeAnalysis;

namespace ApiTrader.Models
{
    public class CreateTransactionTPay
    {
        public int amount { get; set; }
        [AllowNull] public string to { get; set; } = null!;
        [AllowNull] public int to_card_id { get; set; } = 0;
        [AllowNull] public string comment { get; set; } = null!;
        public int card_id { get; set; }
    }
}
