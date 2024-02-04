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

namespace Site.Controllers
{

    [EnableCors]
    [ApiController]
    public class apiController : Controller
    {
        private readonly DataContext data;
        private readonly SpWorlds sp;
        
        public apiController(DataContext _data, SpWorlds _sp)
        {
            data = _data;
            sp = _sp;
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
        [Route("[controller]/admin/transaction/get/{token}")]
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
        public ActionResult GetTransaction(string token)
        {
            var a = data.accounts.Include(x => x.transactions_output).Include(x => x.transactions_output).FirstOrDefault(x => x.token == token);
            if (a.transactions_output == null && a.transactions_input == null)
            {
                return StatusCode(504);
            }
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
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransaction transaction)
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
                    var b = new transactions_output() { discordid = account.discordid, amount = transaction.amount, name = account.name, time = DateTime.UtcNow.ToString()};
                    account.transactions_output.Add(b);
                    account.balance -= transaction.amount;
                    data.accounts.Update(account);
                    await data.SaveChangesAsync();
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
                    if(token == null)
                    {
                        return Unauthorized();
                    }
                    if (!data.accounts.Any(x => x.token == token))
                    {
                        var id = Discord.GetId(token);
                        var name = await sp.GetUserAsync(id);
                        data.accounts.Add(new accounts { name = name, discordid = id, token = token, uuid = await sp.GetMojangUuidAsync(name), balance = 0 });
                        data.accounts_stock.Add(new accounts_stock { discordid = id });
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


    }








}

