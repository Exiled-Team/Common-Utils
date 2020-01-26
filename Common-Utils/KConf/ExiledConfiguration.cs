using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
