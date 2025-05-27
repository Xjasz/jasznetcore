using JaszCore.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Jazatar.Common
{
    public static class Statics
    {
        public static readonly string JAZATAR_FILE = "JazatarFile";
        public static readonly string REQ_DIR = $"{Directory.GetCurrentDirectory()}\\Requests";
        public static readonly string DESKTOP_DIR = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static readonly string PDF_Path = Path.Combine(S.RES_DIR, JAZATAR_FILE + ".pdf");
        public static readonly string IMG_Path = Path.Combine(S.RES_DIR, JAZATAR_FILE + ".png");
        public static string SITE_TOKEN;
        public static string SITE_ID;
        public static IList<string> BTC_TRUE_LIST = new List<string>();
    }
}