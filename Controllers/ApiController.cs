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
        [Route("[controller]/transaction/get/{token}")]
        public ActionResult GetTransaction(string token)
        { 
            if (data.admins.Any(x => x.token == Request.Headers["Admin"].ToString()))
            {
                var a = data.accounts.Include(x => x.transactions_output).Include(x => x.transactions_output).FirstOrDefault(x => x.token == token);
                if(a.transactions_output == null && a.transactions_input == null)
                {
                    return null;
                }
                return Json(new TransacrionAll() { input = a.transactions_input, output = a.transactions_output });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("[controller]/transaction/accept")]
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
                    var b = new transactions_output() { discordid = account.discordid, amount = transaction.amount, name = account.name, time = DateTime.UtcNow.ToString(), status = 2, comment = "null", isread = false };
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
        [Route("[controller]/transaction/search")]
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
        [Route("[controller]/transaction/decline")]
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

        [HttpGet]
        [Route("[controller]/account/login/{code}")]
        public IActionResult login(string code)
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
                        data.accounts.Add(new accounts { name = sp.GetUser(id), discordid = id, token = token, balance = 0 });
                        data.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
            return Ok(Json(new Token() { token = token }));
        }
    }



}

