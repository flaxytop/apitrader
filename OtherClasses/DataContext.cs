using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Options;
using ApiTrader.DbClasses;
using System.Linq;

namespace ApiTrader.OtherClasses
{
    public class DataContext : DbContext
    {
        public DbSet<accounts> accounts { get; set; }
        public DbSet<accounts_stock> accounts_stock { get; set; }
        public DbSet<admins> admins { get; set; }
        public DbSet<stocks> stocks { get; set; }
        public DbSet<transactions_output> transactions_output { get; set; }
        public DbSet<transactions_input> transactions_input { get; set; }
        public DbSet<tpay_cards> tpay_cards { get; set; }
        public DbSet<tpay_transactions> tpay_transactions { get; set; }
        public DbSet<accounts_tpay> accounts_tpay { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Has, with
            modelBuilder.Entity<accounts>().HasMany(x => x.transactions_input).WithOne(x => x.accounts).HasForeignKey(x => x.discordid);
            modelBuilder.Entity<accounts>().HasMany(x => x.transactions_output).WithOne(x => x.accounts).HasForeignKey(x => x.discordid);
            modelBuilder.Entity<accounts>().HasOne(x => x.accounts_stock).WithOne(x => x.accounts).HasForeignKey<accounts_stock>(x => x.discordid);
            modelBuilder.Entity<accounts_tpay>().HasOne(x => x.account).WithOne(x => x.accounts_tpay).HasForeignKey<accounts_tpay>(x => x.discordid);
            modelBuilder.Entity<accounts_stock>().HasMany(x => x.stocks).WithOne(x => x.accounts_stock).HasForeignKey(x => x.discordid);
            modelBuilder.Entity<accounts_stock>().HasMany(x => x.stocks_buy).WithOne(x => x.accounts_stock).HasForeignKey(x => x.discordid);
            modelBuilder.Entity<accounts_tpay>().HasMany(x => x.cards).WithOne(x => x.account).HasForeignKey(x => x.discordid);
            modelBuilder.Entity<tpay_cards>().HasMany(x => x.transactions).WithOne(x => x.card).HasForeignKey(x => x.id);

            // Keys
            modelBuilder.Entity<accounts>().HasKey(x => new {x.discordid});
            modelBuilder.Entity<transactions_output>().HasKey(x => new {x.id});
            modelBuilder.Entity<transactions_input>().HasKey(x => new { x.id });
            modelBuilder.Entity<stocks>().HasKey(x => new {x.id});
            modelBuilder.Entity<accounts_stock>().HasKey(x => new { x.discordid });
            modelBuilder.Entity<stocks_buy>().HasKey(x => new { x.id });
            modelBuilder.Entity<accounts_tpay>().HasKey(x => new { x.discordid });
            modelBuilder.Entity<tpay_cards>().HasKey(x => new {x.id});
            modelBuilder.Entity<tpay_transactions>().HasKey(x => new {x.id});
        }
    }
}   