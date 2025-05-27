using JaszCore.Common;
using JaszCore.Events;
using JaszCore.Models.Main;
using JaszCore.Objects;
using JaszCore.Services;
using JaszCore.Utils;
using Jazatar.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Jazatar.Events
{
    public class BTCLookupEvent : IEvent
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();
        private static IDatabaseService DatabaseService => ServiceLocator.Get<IDatabaseService>();

        private static IList<string> BTC_LIST = new List<string>();
        private static string _urlRoot = "https://blockchain.info/balance?active=";

        private static FileInfo[] LOOKUP_FILES = DirectoryUtils.GetFilesByCreationTime(Statics.DESKTOP_DIR);
        private static IList<ConfigApplication> _CFGAPPLICATIONS;
        private static string JS_STRING = File.ReadAllText(S.RES_DIR + "//" + "keymapper.js");


        public void StartEvent(string[] args = null)
        {
            Log.Debug($"BTCLookupEvent starting....");
            var connection = DatabaseService.GetMainConnection();
            connection.Open();
            _CFGAPPLICATIONS = DatabaseService.GetAll<ConfigApplication>();
            connection.Close();
            var result = JS_STRING;
            //ReadFromDesktopToDatabase();
            //RunBTCEvent();
            Log.Debug($"BTCLookupEvent finished....");
        }

        private void ReadFromDesktopToDatabase()
        {
            var model = new XjzApplication();
            var bulkObject = new BulkObject(null, XjzApplication.GetTableName(), null, null, model.GetMergedEntity());

            foreach (FileInfo item in LOOKUP_FILES)
            {
                var fileName = item.FullName;
                if (fileName.ToUpper().Contains(".JASZ"))
                {
                    var result = _CFGAPPLICATIONS.Where(a => a.AppName == "BTCLookupEvent" && a.MainValue == fileName).Count();
                    if (result < 1)
                    {
                        Log.Debug($"File to be saved: {fileName}");
                        foreach (var line in File.ReadLines(fileName))
                        {
                            if (line.Length > 55)
                            {
                                Log.Debug($"Skipping p_name: {line}  length to large ({line.Length})");
                            }
                            else
                            {
                                var objectArray = new object[bulkObject.DestinationTableColumns.Count];
                                objectArray[0] = line;
                                bulkObject.Items.Add(objectArray);
                            }
                        }
                        //DatabaseService.BulkSave<XjzApplication>(bulkObject.DestinationTableColumns, bulkObject.Items, 5000);

                        var connection = DatabaseService.GetMainConnection();
                        connection.Open();
                        var cfgApp = new ConfigApplication("BTCLookupEvent", fileName);
                        DatabaseService.BeginTransaction<ConfigApplication>();
                        DatabaseService.Save<ConfigApplication>(cfgApp);
                        DatabaseService.CommitTransaction<ConfigApplication>();
                        connection.Close();
                    }
                    else
                    {
                        Log.Debug($"Skipping file: {fileName}");
                    }
                }
            }
        }

        public void RunBTCEvent()
        {
            Log.Debug($"RunBTCEvent starting....");
            var fullLine = _urlRoot;
            var count = 0;
            var fileName = ""; //item.Name.ToUpper();
            foreach (var line in File.ReadLines(fileName))
            {
                if (line.Length < 10)
                {
                    break;
                }
                fullLine += line + "|";
                count++;
                if (count > 10)
                {
                    fullLine = fullLine.Substring(0, fullLine.Length - 1);
                    BTC_LIST.Add(fullLine);
                    fullLine = _urlRoot;
                    count = 0;
                }
            }
            if (count != 0)
            {
                fullLine = fullLine.Substring(0, fullLine.Length - 1);
                BTC_LIST.Add(fullLine);
                fullLine = _urlRoot;
                count = 0;
            }
            Log.Debug($"Finished loading list...");
            var _request = new Request();
            foreach (var item in BTC_LIST)
            {
                Thread.Sleep(3000);
                _request.BasicRequest(item, null, null);
            }
            Log.Debug($"RunBTCEvent finished....");
        }
    }
}
