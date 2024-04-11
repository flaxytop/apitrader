using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ApiTrader.DbClasses;
using ApiTrader.Models;
using ApiTrader.OtherClasses;
using spw;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Primitives;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ApiTrader.Function;

namespace Site.Controllers
{

    [EnableCors]
    [ApiController]
    public class apiController : Controller
    {
        private readonly DataContext data;
        private readonly SpWorlds sp;
        private readonly TPayBD TPay;

        public apiController(DataContext _data, SpWorlds _sp)
        {
            data = _data;
            sp = _sp;
            TPay = new TPayBD(data);
        }

        [HttpPost]
        [Route("[controller]/transaction/remove")]
        public async Task<IActionResult> RemoveTransaction([FromBody] AcceptDeleteTransaction tr)
        {
            try
            {
                if (Request.Headers["User"] != string.Empty) {
                    var account = data.accounts.Include(x => x.transactions_output).FirstOrDefault(x => x.token == Request.Headers["User"].ToString());
                    if (account == null)
                    {
                        return Unauthorized();
                    }
                    transactions_output t = account.transactions_output.FirstOrDefault(x => x.id == tr.id);
                    account.balance += t.amount;
                    account.transactions_output.Remove(t);
                    data.accounts.Update(account);
                    await data.SaveChangesAsync();
                    return Json(t);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch(Exception ex) { return Unauthorized(); }
        }

        [HttpGet]
        [Route("[controller]/admin/transaction/get")]
        public IActionResult LoadTransactionsAdmin()
        {
            if (data.admins.Any(x => x.token == Request.Headers["Admin"].ToString()))
            {
                return Json(data.transactions_output.Where(x => x.status == 2));
            }
            return Unauthorized();
        }

        [HttpGet]
        [Route("[controller]/transaction/get/{token}")]
        public ActionResult GetTransactions(string token, int timezone)
        {
            var a = data.accounts.Include(x => x.transactions_output).Include(x => x.transactions_output).FirstOrDefault(x => x.token == token);
            if (a.transactions_output == null && a.transactions_input == null)
            {
                return StatusCode(504);
            }
            a.transactions_output.ForEach(x => {
                x.time = DateTime.Parse(x.time).AddHours(timezone).ToString();
            });
            a.transactions_input.ForEach(x => x.time.AddHours(timezone));
            return Json(new TransacrionAll() { input = a.transactions_input, output = a.transactions_output });
        }

        [HttpPost]
        [Route("[controller]/admin/transaction/accept")]
        public async Task<IActionResult> AcceptTransaction([FromBody] AcceptDeleteTransaction transaction)
        {
            if (data.admins.Any(x => x.token == Request.Headers["Admin"].ToString()))
            {
                var tr = data.transactions_output.FirstOrDefault(x => x.id == transaction.id);
                if(tr == null)
                {
                    return StatusCode(502);
                }
                tr.status = 1;
                data.transactions_output.Update(tr);
                await data.SaveChangesAsync();
                return Ok();
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("[controller]/transaction/create")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransaction transaction, int timezone)
        {
            if (Request.Headers["User"] != string.Empty)
            {
                var account = data.accounts.FirstOrDefault(x => x.token == Request.Headers["User"].ToString());
                if (account == null)
                {
                    return Unauthorized();
                }
                if (transaction.amount <= account.balance)
                {
                    var date = DateTime.UtcNow;
                    var b = new transactions_output() { discordid = account.discordid, amount = transaction.amount, name = account.name, time = date.ToString(), status = 2};
                    account.transactions_output.Add(b);
                    account.balance -= transaction.amount;
                    data.accounts.Update(account);
                    await data.SaveChangesAsync();
                    b.time = date.AddHours(timezone).ToString();
                    return Json(b);
                }
                return BadRequest();
            }
            return Unauthorized();
        }
        [HttpPost]
        [Route("[controller]/admin/transaction/search")]
        public IActionResult SearchTransaction([FromBody] SearchOrDeleteTransaction tr)
        {
            if (data.admins.Any(x => x.token == Request.Headers["Admin"].ToString()))
            {
                if (TransactionEnum.Input == tr.method)
                {
                    var a = data.transactions_input.FirstOrDefault(x => x.id == tr.id);
                    if(a != null)
                    {
                        return Json(a);
                    }
                }
                else if (TransactionEnum.Output == tr.method)
                {
                    var a = data.transactions_output.FirstOrDefault(x => x.id == tr.id);
                    if (a != null)
                    {
                        return Json(a);
                    }
                }
                return StatusCode(502);
            }
            return Unauthorized();
        }
        [HttpPost]
        [Route("[controller]/admin/transaction/decline")]
        public async Task<IActionResult> DeclineTransaction([FromBody] DeclineTransaction tr)
        {
            if (data.admins.Any(x => x.token == Request.Headers["Admin"].ToString()))
            {
                transactions_output t = data.transactions_output.FirstOrDefault(x => x.id == tr.id);
                var a = data.accounts.FirstOrDefault(x => x.discordid == t.discordid);
                if(a == null)
                {
                    return StatusCode(502);
                }
                a.balance += t.amount;
                t.comment = tr.comment;
                t.status = 0;
                data.transactions_output.Update(t);
                data.accounts.Update(a);
                await data.SaveChangesAsync();
                return Ok();
            }
            return Unauthorized();
        }

        [HttpGet]
        [Route("[controller]/account/pay/{amount}")]
        public async Task<IActionResult> Pay(int amount)
        {
             return Ok(await sp.CreatePaymentAsync(amount, "http://localhost:8081/", "https://dw3lrs49-7021.euw.devtunnels.ms/webhook/checkpay", "payment"));
        }

        [HttpGet]
        [Route("[controller]/acccont/{token}")]
        public IActionResult Account(string token)
        {

            var cl = data.accounts.FirstOrDefault(x => token == x.token);
            if(cl == null){
                return StatusCode(502);
            }
            return Json(new AccountInfo() { name = cl.name, balance = cl.balance, uuid = cl.uuid}) ;
        }
        [HttpPost]
        [Route("[controller]/admin/token")]
        public IActionResult AdminToken([FromBody]AdminForm ad)
        {
            MD5 md5 = MD5.Create();
            string hash = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(ad.password)));
            var res = data.admins.FirstOrDefault(x => x.login == ad.login);
            if(res != null && res.token == hash)
            {
                return Json(res);
            }
            return BadRequest("Not fined");
        
        }

        [HttpGet]
        [Route("[controller]/admin/login")]
        public IActionResult AdminLogin()
        {
            var tk = Request.Headers["Admin"].ToString();
            var res = data.admins.FirstOrDefault(x => x.token == tk);
            if (res != null)
            {
                return Json(res);
            }
            return BadRequest();
        }

        [HttpGet]
        [Route("[controller]/account/login/{code}")]
        public async Task<IActionResult> login(string code)
        {
            string token = string.Empty;
            try
            {
                if (code != null)
                {
                    token = Discord.GetToken(code);
                    string id = Discord.GetId(token);
                    if(token == null)
                    {
                        return Unauthorized();
                    }
                    var tmp = data.accounts.FirstOrDefault(x => x.discordid == id);
                    if (tmp == null)
                    {
                        var name = await sp.GetUserAsync(id);
                        data.accounts.Add(new accounts { name = name, discordid = id, token = token, uuid = await sp.GetMojangUuidAsync(name), balance = 0 });
                        data.accounts_stock.Add(new accounts_stock { discordid = id });
                        await data.SaveChangesAsync();
                    }
                    else if (tmp.token != token) {
                        tmp.token = token;
                        data.accounts.Update(tmp);
                        await data.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
            return Ok(Json(new Token() { token = token }));
        }





        // -----STOCKS-------
        [HttpPost]
        [Route("[controller]/stock/buy")]
        public async Task<IActionResult> BuyStock([FromBody]BuyOrSellStock stock)
        {
            if (Request.Headers["User"] != StringValues.Empty)
            {
                var account = data.accounts.Include(x => x.accounts_stock).FirstOrDefault(x => x.token == Request.Headers["User"].ToString());
                if(account !=  null)
                {
                    var th_stock = data.stocks.FirstOrDefault(x => x.id == stock.stock_id);
                    if (th_stock != null)
                    {
                        var js = account.accounts_stock.stocks_json != null ? JsonNode.Parse(account.accounts_stock.stocks_json) : null;
                        if (js != null && js[$"{th_stock.id}"] != null)
                        {
                            js[$"{th_stock.id}"] = (int.Parse(js[$"{th_stock.id}"].ToString()) + stock.amount).ToString();
                        } else {
                            var strjs = js.ToString();
                            js = strjs.Insert(strjs.Length - 1, $",\"{stock.stock_id}\":\"{stock.amount}\"");
                        }
                        account.accounts_stock.stocks_json = js.ToString();
                        account.balance -= (th_stock.price * th_stock.coefficient * stock.amount + (stock.amount / 100));
                        account.accounts_stock.stocks_buy.Add(new stocks_buy() { stock_price_withcoef = th_stock.price * th_stock.coefficient, date = DateTime.UtcNow.ToString(), stock_id = stock.stock_id, stock_amount = stock.amount });
                        th_stock.coefficient += (float)stock.amount / 100;
                        th_stock.history = th_stock.history != null ? th_stock.history.Insert(th_stock.history.Length - 1, $",\"{DateTime.UtcNow}\":\"{th_stock.coefficient}\"") : "{" + $"\"{DateTime.UtcNow}\":\"{th_stock.coefficient}\"" + "}";

                        await data.SaveChangesAsync();
                        return Ok();
                    }
                }
            }
            return BadRequest();
        }


        [Route("[controller]/tpay/card/{name}")]
        [HttpPost]
        public async Task<IActionResult> CreateCard(string name)
        {
            if (await TPay.CreateNewCard(Request.Headers["User"].ToString(), name))
            {
                return Ok();
            }
            return BadRequest();
        }

        [Route("[controller]/tpay/card/{card_id}")]
        [HttpGet]
        public async Task<IActionResult> GetCard(int card_id)
        {
            return Json(await TPay.GetCard(Request.Headers["User"].ToString(), card_id));
        }

        [Route("[controller]/tpay/cards")]
        [HttpGet]
        public async Task<IActionResult> GetCards()
        {
            return Json(await TPay.GetCards(Request.Headers["User"].ToString()));
        }
        [Route("[controller]/tpay/card/{card_id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCard(int card_id)
        {
            return (await TPay.DeleteCard(Request.Headers["User"].ToString(), card_id)) ? Ok() : BadRequest(); 
        }

        [Route("[controller]/tpay/account")]
        [HttpGet]
        public async Task<IActionResult> GetTpayAccount()
        {
            return Json(await TPay.GetAccount(Request.Headers["User"].ToString()));
        }

        [Route("[controller]/tpay/transaction")]
        [HttpPost]
        public async Task<IActionResult> CreateTransactionTpay([FromBody] CreateTransactionTPay tr)
        {
            if(await TPay.CreateTransaction(Request.Headers["User"].ToString(), tr.amount, tr.card_id, comment: tr.comment, to_card_id: tr.to_card_id, to: tr.to))
            {
                if (tr.to.Contains("spworlds"))
                {
                   var reciver = tr.to.Substring(tr.to.IndexOf(':') + 1);
                   sp.SendPayment(tr.amount, reciver, $"TRADER (TPay). From {tr.card_id}.");
                   TPay.SetBalance(tr.card_id, tr.amount * -1);
                }
                else if (tr.to.Contains("trader")){
                    TPay.SetBalance(tr.card_id, tr.amount);
                }
                else
                {
                    TPay.SetBalance(tr.to_card_id, tr.amount);
                    TPay.SetBalance(tr.card_id, tr.amount * -1);
                }
                return Ok();
            }
            return BadRequest();
        }
    }








}

