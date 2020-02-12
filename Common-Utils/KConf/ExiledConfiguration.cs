using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EXILED;

namespace Common_Utils.KConf
{
    public class ExiledConfiguration
    {
        /// <summary>
        /// Usage:
        /// static Dictionary<string,string> dict = GetDictonaryValue(Plugin.Config.GetString(value));
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Dictionary<string,string> GetDictonaryValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (!value.Contains(","))
            {
                var splitted = value.Split(':');
                if (splitted.Length != 2)
                    return null;
                dict.Add(value.Split(':')[0], value.Split(':')[1]);
                return dict;
            }
            string[] tl = value.Split(',');
            foreach(string t in tl)
            {
                string[] vl = t.Split(':');
                dict.Add(vl[0],vl[1]);
            }
            return dict;
        }

        public static List<string> GetListStringValue(string value)
        {
            if (value == null)
                return null;
            List<string> dict = new List<string>();
            Dictionary<string, int> rDict = new Dictionary<string, int>();
            if (!value.Contains(","))
            {
                dict.Add(value);
                return dict;
            }
            string[] tl = value.Split(',');
            foreach (string t in tl)
            {
                dict.Add(t);
            }
            return dict;
        }

        public static Dictionary<string, int> GetRandomListValue(string value)
        {
            if (value == null)
                return null;
            Dictionary<string, int> dict = new Dictionary<string, int>();
            string[] items = value.Split(',');
            foreach (string s in items)
            {
                string[] c = s.Split(':');
                if (!int.TryParse(c[1], out int chance))
                {
                    Log.Error($"Unable to parse item chance for: {s}, invalid integer.");
                    continue;
                }
                dict.Add(c[0], chance);
            }

            return dict;
        }
    }
}
