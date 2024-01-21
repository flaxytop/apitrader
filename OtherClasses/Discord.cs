using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RestSharp;
using spw;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiTrader.OtherClasses
{
    public class Discord
    {
        private static string url = "http://localhost:8081/account/login";
        public static string GetFullId(string code)
        {
            try
            {
                using (RestClient client = new RestClient())
                {
                    RestRequest request = new RestRequest($"https://discord.com/api/oauth2/token");
                    request.AddParameter("client_id", "1141777588474355812");
                    request.AddParameter("client_secret", "dTQNZ7GEQBjR2YfFR3aO2Whr_WtagGSU");
                    request.AddParameter("grant_type", "authorization_code");
                    request.AddParameter("code", code);
                    request.AddParameter("redirect_uri", url);
                    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    var t = client.Post(request);
                    var token = JsonNode.Parse(t.Content);
                    RestRequest request2 = new RestRequest("https://discord.com/api/users/@me");
                    request2.AddHeader("Authorization", $"Bearer {token["access_token"]}");
                    return JsonNode.Parse(client.Get(request2).Content)["id"].ToString();

                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string GetToken(string code)
        {
            try
            {
                using (RestClient client = new RestClient())
                {
                    RestRequest request = new RestRequest($"https://discord.com/api/oauth2/token");
                    request.AddParameter("client_id", "1141777588474355812");
                    request.AddParameter("client_secret", "dTQNZ7GEQBjR2YfFR3aO2Whr_WtagGSU");
                    request.AddParameter("grant_type", "authorization_code");
                    request.AddParameter("code", code);
                    request.AddParameter("redirect_uri", url);
                    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    return JsonNode.Parse(client.Post(request).Content)["access_token"].ToString();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string GetId(string token)
        {
            try
            {
                using (RestClient client = new RestClient())
                {
                    RestRequest request2 = new RestRequest("https://discord.com/api/users/@me");
                    request2.AddHeader("Authorization", $"Bearer {token}");
                    var res = client.Get(request2);
                    return JsonNode.Parse(res.Content)["id"].ToString();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

}
