using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ApiTrader.DbClasses;
using ApiTrader.Models;
using ApiTrader.OtherClasses;
using spw;
using System.Text.Json.Nodes;
using System.Text.Json;


namespace ApiTrader.Controllers
{
    [Route("webhook/[action]")]
    public class WebhookController : Controller
    {
        private readonly DataContext data;
        private readonly SpWorlds sp;
        public WebhookController(DataContext _data, SpWorlds _sp)
        {
            data = _data;
            sp = _sp;
        }
        [HttpPost]
        public IActionResult checkpay([FromBody] Payer payment)
        {
            if (payment != null)
            {
                Request.Headers.TryGetValue("x-body-hash", out StringValues value);
                string xhash = value.ToString();
                string body = "{" + $"\"data\":\"{payment.data}\",\"amount\":{payment.amount},\"payer\":\"{payment.payer}\"" + "}";
                if (sp.ValidateWebhook(body, xhash))
                {
                    var upd = data.accounts.FirstOrDefault(x => x.name == payment.payer);
                    upd.balance += payment.amount;
                    data.accounts.Update(upd);
                    data.transactions_input.Add(new transactions_input { discordid = upd.discordid, amount = payment.amount, time = DateTime.UtcNow });
                    data.SaveChanges();
                    return Ok();
                }
            }
            return BadRequest();
        }
    }
}
