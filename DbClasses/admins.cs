using System.ComponentModel.DataAnnotations;

namespace ApiTrader.DbClasses
{
    public class admins
    {
        [Key] public int id { get; set; }

        public string login { get; set; }
        public string token { get; set; }
    }
}
