using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xrm.Tools.WebAPI.Metadata;
using Xrm.Tools.WebAPI.Requests;
using Xrm.Tools.WebAPI.Results;

namespace DynamicsCRMWebAPI
{
    public class DynamicsCRMWebAPI
    {
        #region Global private members
        private readonly CRMConnection _crm;
        private static IConfiguration _configuration { get; set; }
        private IMemoryCache _cache;
        #endregion

        #region Initialize the members in constructor
        public CommonCRMQuerry(CRMConnection crm)
        {
            _crm = crm;
            _configuration = new ConfigurationBuilder()
                         .SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("appsettings.json").Build();
        }
        public CommonCRMQuerry(CRMConnection crm, IMemoryCache cache)
        {
            _crm = crm;
            _cache = cache;
            _configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json").Build();
        }
        #endregion

        /// <summary>
        /// Delete/Remove the lookup value from the entity
        /// </summary>
        /// <param name="entityCollectionName">Logical name of entity</param>
        /// <param name="entityId">Entity Id</param>
        /// <param name="lookupName">logical name of lookup to set null</param>
        internal async Task<int> GetCount(string fetchXML)
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                var OrganizationAPI = _configuration.GetSection("appSettings").GetSection("OrganizationWebApiURL").Value;
                var AccessToken = _cache.Get<string>("AccessToken");

                httpClient.BaseAddress = new Uri(OrganizationUrl + OrganizationAPI + "$batch");

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

                httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");

                var body = "--batch_accountFetch" + Environment.NewLine;
                body += "Content-Type: application/http" + Environment.NewLine;
                body += "Content-Transfer-Encoding: binary" + Environment.NewLine;
                body += Environment.NewLine;
                body += "GET " + OrganizationUrl + OrganizationAPI + "accounts?fetchXml=" + fetchXML + " HTTP/1.1" + Environment.NewLine;
                body += "Content-Type: application/json" + Environment.NewLine;
                body += "OData-Version: 4.0" + Environment.NewLine;
                body += "OData-MaxVersion: 4.0" + Environment.NewLine;
                body += Environment.NewLine;
                body += "--batch_accountFetch--";

                var multipartContent = new MultipartContent("mixed");
                multipartContent.Headers.Remove("Content-Type");
                multipartContent.Headers.TryAddWithoutValidation("Content-Type", "multipart/mixed; boundary=batch_accountFetch");

                var myContent = JsonConvert.SerializeObject(body);
                var stringContent = new StringContent(body, Encoding.UTF8, "application/json");
                multipartContent.Add(stringContent);

                var response = await httpClient.PostAsync(OrganizationUrl + OrganizationAPI + "$batch", multipartContent);

                response.EnsureSuccessStatusCode();

                var data = await response.Content.ReadAsStringAsync();              

                var values = JObject.Parse(data.Substring(data.IndexOf('{'), data.LastIndexOf('}') - data.IndexOf('{') + 1));
                var valueList = values["value"].ToList();
                int recordCount = 0;
                string key = string.Empty;
                string value = string.Empty;
                foreach (var val in valueList)
                {
                   //Your logic
                }
                return recordCount;
            }
            catch (Exception Ex)
            {
                throw new Exception(Ex.Message);
            }
        }

    }
}
