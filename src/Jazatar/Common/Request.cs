using JaszCore.Common;
using JaszCore.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Jazatar.Common
{
    public class Request
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        private readonly StringBuilder RequestLogger = new StringBuilder();

        public bool BasicRequest(string request, string jsonBody, string requestType)
        {
            var httpClient = new HttpClient();
            var response = httpClient.GetAsync(request).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var jsonResponseContent = JObject.Parse(responseContent);
                foreach (var item in jsonResponseContent)
                {
                    var pubName = item.Key;
                    var jtok = item.Value;
                    var final_balance = jtok.Value<int>("final_balance");
                    var total_received = jtok.Value<int>("total_received");
                    var n_tx = jtok.Value<int>("n_tx");
                    if (total_received > 0)
                    {
                        Log.Debug($"WALLET: {pubName} balance->({final_balance}) transactions->({n_tx}) received->({total_received})");
                    }
                    if (final_balance > 0)
                    {
                        Log.Debug("FOUND LOST BTC WALLET: " + pubName);
                        Statics.BTC_TRUE_LIST.Add(pubName);
                        SaveFoundToFile(pubName);
                    }
                }
                Thread.Sleep(2000);
                return true;
            }
            else
            {
                Log.Debug($"Bad Response StatusCode: {response.StatusCode}");
                Thread.Sleep(30000);
                return false;
            }
        }

        public void SaveFoundToFile(string pubName)
        {
            Log.Debug($"SaveFoundToFile....");
            var filePath = Path.Combine(S.RES_DIR, "tempfile.jasz");
            if (!File.Exists(filePath))
            {
                var createFileStream = File.Create(filePath);
                createFileStream.Close();
            }
            RequestLogger?.AppendLine("FOUND LOST BTC WALLET: " + pubName);
            var bytes = Encoding.ASCII.GetBytes(RequestLogger?.ToString());
            var fileStream = File.Open(filePath, FileMode.Append);
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Close();
            RequestLogger?.Clear();
        }
    }
}