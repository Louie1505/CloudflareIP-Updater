using FunFacts;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace CloudflareIP_Updater
{
    class Helpers
    {
        public static string CurrentPublicIP() 
        {
            string url = Program.configuration["myIpUrl"];
            if (string.IsNullOrEmpty(url))
                return "";
            HttpClient client = new HttpClient();
            try
            {
                using (var resp = client.GetAsync(url).Result) 
                {
                    return resp.Content.ReadAsStringAsync().Result.Replace("\n", "").Replace(Environment.NewLine, "");
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static async void LogOutWarning(string warning) 
        {
            HttpClient client = new HttpClient();
            try
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://api.1505.xyz/api/log/{warning}"),
                    Method = HttpMethod.Post,
                };
                var resp = await client.SendAsync(request);
            }
            catch (Exception e)
            {
                
            }
        }
    }
}
