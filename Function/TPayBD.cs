using ApiTrader.DbClasses;
using ApiTrader.OtherClasses;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;


namespace ApiTrader.Function
{
    public class TPayBD
    {
        private readonly DataContext data;
        public TPayBD(DataContext data) {
            this.data = data;
        }
        public async Task<bool> CreateNewCard(string token, string name)
        {
            var acc = data.accounts.Include(x => x.accounts_tpay.cards).FirstOrDefault(x => x.token == token);
            var cards = acc?.accounts_tpay.cards;
            if (cards != null && cards.Count < 3) {
                using (SHA256 s = SHA256.Create())
                {
                    var id = s.ComputeHash(Encoding.UTF8.GetBytes($"{data.tpay_cards.Last().id + 1}TP::{acc?.discordid}"));
                    cards.Add(new tpay_cards() { name = name, id_tr = id.ToString()!});
                }
                await data.SaveChangesAsync();
                return true;
            }
            return false;
        }
        public async Task<tpay_cards> GetCard(string token, int card_id)
        {
            var acc = data.accounts.Include(x => x.accounts_tpay.cards).FirstOrDefault(x => x.token == token)?.accounts_tpay;
            if (acc != null)
            {
                return acc.cards.FirstOrDefault(x => x.id == card_id);
            }
            return null!;
        }
        public async Task<List<tpay_cards>> GetCards(string token)
        {
            return (await data.accounts.Include(x => x.accounts_tpay.cards).FirstOrDefaultAsync(x => x.token == token)).accounts_tpay.cards;
        }
        public async Task<bool> CreateTransaction(string token, int amount, int card_id, int to_card_id = 0, string to = null, string comment = null)
        {
            var acc = (await data.accounts.Include(x => x.accounts_tpay.cards).FirstOrDefaultAsync(x => x.token == token)).accounts_tpay;
            if (acc != null)
            {
                if(acc.cards.Any(x => x.id == card_id))
                {
                    acc.cards.FirstOrDefault(x => x.id == card_id)?.transactions.Add(new tpay_transactions() { amount = amount,card_id = card_id ,comment = comment, to = to, time = DateTime.UtcNow.ToString(), to_card_id = to_card_id});
                    await data.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }

        public async Task<accounts_tpay> GetAccount(string token)
        {
            var acc = await data.accounts.Include(x => x.accounts_tpay.cards).FirstOrDefaultAsync(x => x.token == token);
            
            
            return acc.accounts_tpay;
        }

        public async void SetBalance(int cardid, int amount, int tbonus = 0) {
            var card = await data.tpay_cards.FirstOrDefaultAsync(x => x.id == cardid);
            card.balance += amount;
            card.tbonus += tbonus;
            await data.SaveChangesAsync();
        }

        public async Task<bool> DeleteCard(string token, int card_id)
        {
            var acc = data.accounts.Include(x => x.accounts_tpay.cards).FirstOrDefault(x => x.token == token)?.accounts_tpay;
            if(acc != null)
            {
                var card = acc.cards.FirstOrDefault(x => x.id == card_id);
                if (card != null)
                {
                    data.tpay_cards.Remove(card);
                    await data.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }
    }
}
