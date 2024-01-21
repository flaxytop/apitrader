using ApiTrader.DbClasses;

namespace ApiTrader.Models
{
    public class TransacrionAll
    {
        public List<transactions_input> input { get; set; }
        public List<transactions_output> output { get; set; }
    }
}
