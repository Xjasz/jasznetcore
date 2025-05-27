using JaszCore.Common;
using JaszCore.Models;
using JaszCore.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace JaszCore.Services
{
    [Service(typeof(WebService))]
    public interface IWebService
    {
        string PostRequest(string requestUri, string requestBody, int timeout = -1);
        string GetRequest(string requestUri);
    }
    public class WebService : IWebService
    {
        private readonly Dictionary<string, string> REQUEST_TOKENS = new Dictionary<string, string>();
        private readonly Dictionary<string, string> REQUEST_IDS = new Dictionary<string, string>();

        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        public WebService()
        {
            Log.Debug($"WebService starting....");
        }

        public string PostRequest(string requestUri, string requestBody, int timeout = -1)
        {
            Log.Debug($"PostRequest running.... {requestUri}");
            if (!ValidateRequest(requestUri))
            {
                throw new WebException($"The protocol isn't valid.... Please use http:// or https://!!");
            }
            var pathArray = requestUri.Split('/');
            var baseUrl = pathArray[0] + "//" + pathArray[2];
            var siteToken = REQUEST_TOKENS.ContainsKey(baseUrl) ? REQUEST_TOKENS.GetValueOrDefault(baseUrl) : null;
            var siteRequestModel = CreateRequestModel(requestBody);

            var requestContent = new StringContent(siteRequestModel[0], Encoding.UTF8, siteRequestModel[1]);
            var header = requestContent.Headers.ToArray().Select(h => h.Key + ": " + string.Join(",", h.Value).ToString());
            Log.Debug($"Request Uri: {requestUri}");
            Log.Debug($"Request Headers: {string.Join(",", header)}");
            Log.Debug($"Request Content: {siteRequestModel[0]}");
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", siteToken);
            httpClient.Timeout = TimeSpan.FromSeconds(timeout < 0 ? 100 : timeout);
            var response = httpClient.PostAsync(requestUri, requestContent).GetAwaiter().GetResult();
            var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var trimmedContent = StringUtils.TrimStringToSize(responseContent, 200);
            Log.Debug($"Response StatusCode: {response.StatusCode}");
            Log.Debug($"Response Content: {trimmedContent}");

            if (response.IsSuccessStatusCode)
            {
                if (siteToken == null && (requestUri.Contains("oauth2/token") || requestUri.Contains("auth/signin")))
                {
                    SetSiteToken(requestUri, responseContent);
                }
            }
            return responseContent;
        }

        public string GetRequest(string requestUri)
        {
            Log.Debug($"PostRequest running.... {requestUri}");
            if (!ValidateRequest(requestUri))
            {
                throw new WebException($"The protocol isn't valid.... Please use http:// or https://!!");
            }
            var pathArray = requestUri.Split('/');
            var baseUrl = pathArray[0] + "//" + pathArray[2];
            var siteToken = REQUEST_TOKENS.ContainsKey(baseUrl) ? REQUEST_TOKENS.GetValueOrDefault(baseUrl) : null;

            Log.Debug($"Request Uri: {requestUri}");
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", siteToken);
            var response = httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
            var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var trimmedContent = StringUtils.TrimStringToSize(responseContent, 200);
            Log.Debug($"Response StatusCode: {response.StatusCode}");
            Log.Debug($"Response Content: {trimmedContent}");

            if (response.IsSuccessStatusCode)
            {
                if (siteToken == null && (requestUri.Contains("oauth2/token") || requestUri.Contains("auth/signin")))
                {
                    SetSiteToken(requestUri, responseContent);
                }
            }
            return responseContent;
        }

        private void SetSiteToken(string requestUri, string responseContent)
        {
            Log.Debug($"Setting token for: {requestUri}");
            var pathArray = requestUri.Split('/');
            var baseUrl = pathArray[0] + "//" + pathArray[2];

            if (requestUri.Contains("oauth2/token"))
            {
                var json = JObject.Parse(responseContent);
                var bearer = json["access_token"].Value<string>();
                REQUEST_TOKENS.Add(baseUrl, bearer);

            }
            if (requestUri.Contains("auth/signin"))
            {
                var nameSpaceStart = responseContent.Contains("xmlns=\"") ? responseContent.IndexOf("xmlns=\"" + 7) : 0;
                var nameSpaceEnd = responseContent.IndexOf("\"", nameSpaceStart);
                if (nameSpaceStart > 0)
                {
                    var nameSpace = responseContent.Substring(nameSpaceStart, nameSpaceEnd - nameSpaceStart);
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(responseContent);
                    var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                    nsmgr.AddNamespace(baseUrl, nameSpace);
                    var xmlNode = xmlDocument.SelectSingleNode($"//{baseUrl}:credentials", nsmgr);
                    REQUEST_TOKENS.Add(baseUrl, xmlNode?.Attributes["token"]?.Value);
                    REQUEST_IDS.Add(baseUrl, xmlNode?.FirstChild?.Attributes["id"]?.Value);
                }
            }
        }

        private bool ValidateRequest(string requestUrl)
        {
            var result = false;
            if (requestUrl.Contains("//"))
            {
                result = true;
            }
            return result;
        }

        private string[] CreateRequestModel(string requestBody)
        {
            var requestModel = new string[] { requestBody, S.MIME_UNKNOWN };
            if (string.IsNullOrWhiteSpace(requestModel[0])) { return requestModel; }
            requestBody = requestBody.Trim();
            requestModel[0] = requestBody;
            if (requestBody.StartsWith("<"))
            {
                try
                {
                    var obj = XDocument.Parse(requestBody);
                    requestModel[1] = S.MIME_XML;
                    return requestModel;
                }
                catch (Exception) { }
                ;
            }
            if ((requestBody.StartsWith("{") && requestBody.EndsWith("}")) || (requestBody.StartsWith("[") && requestBody.EndsWith("]")))
            {
                try
                {
                    var obj = JToken.Parse(requestBody);
                    requestModel[1] = S.MIME_JSON;
                    return requestModel;
                }
                catch (Exception) { }
                ;
            }
            if (requestBody.Contains(".json") || requestBody.Contains(".xml"))
            {
                var getFilePath = Path.Combine(S.REQ_DIR, requestBody);
                if (File.Exists(getFilePath))
                {
                    var requestFile = File.ReadAllText(getFilePath, Encoding.UTF8);
                    requestModel[0] = requestFile;
                    requestModel[1] = requestBody.Contains(".json") ? S.MIME_JSON : S.MIME_XML;
                    return requestModel;
                }
            }
            return requestModel;
        }
    }
}