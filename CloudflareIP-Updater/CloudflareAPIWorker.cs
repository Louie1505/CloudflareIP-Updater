using CloudflareIP_Updater.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareIP_Updater
{
    class CloudflareAPIWorker
    {
        public string Email { get; set; }
        public string APIKey { get; set; }
        public string URLBase { get; set; }

        private HttpClient client = new HttpClient();

        public CloudflareAPIWorker(string urlBase, string email, string key)
        {
            this.URLBase = (urlBase.EndsWith("/") ? urlBase : urlBase + "/");
            this.APIKey = key;
            this.Email = email;
        }
        private async Task<object> CloudflareAPIRequest(string reqURL, HttpMethod method, Type type, object body = null) 
        {
            try
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(reqURL),
                    Method = method,
                };

                if (body != null && method != HttpMethod.Get)
                    request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                
                request.Headers.Add("X-Auth-Email", $"{this.Email}");
                request.Headers.Add("X-Auth-Key", $"{this.APIKey}");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var task = client.SendAsync(request).ContinueWith((taskwithmsg) =>
                    {
                        return taskwithmsg.Result;
                    });
                var res = await task;
                if (res.IsSuccessStatusCode)
                {
                    CFResp respObj = (CFResp)JsonConvert.DeserializeObject(res.Content.ReadAsStringAsync().Result, typeof(CFResp));
                    return respObj.result;
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<List<CFZone>> GetZones()
        {
            object o = await this.CloudflareAPIRequest(this.URLBase + "zones?status=active&match=all", HttpMethod.Get, typeof(List<CFZone>));
            if (o != null)
                return ((JArray)o).ToObject<List<CFZone>>();
            return new List<CFZone>();
        }
        public async Task<List<CFDNS>> GetDNSRecords(string id)
        {
            object o = await this.CloudflareAPIRequest(this.URLBase + $"zones/{id}/dns_records?type=A&match=all", HttpMethod.Get, typeof(List<CFDNS>));
            if (o != null)
                return ((JArray)o).ToObject<List<CFDNS>>();
            return new List<CFDNS>();
        }
        public async Task<bool> UpdateDNSRecords(CFDNS oldRecord, CFDNS newRecord)
        {
            object o = await this.CloudflareAPIRequest(this.URLBase + $"zones/{oldRecord.zone_id}/dns_records/{oldRecord.id}", HttpMethod.Patch, typeof(CFDNS), newRecord);
            if (o != null)
                return o is CFDNS;
            return false;
        }
    }
}
