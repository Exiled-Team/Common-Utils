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
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string[] tl = value.Split(',');
            foreach(string t in tl)
            {
                string[] vl = t.Split(':');
                dict.Add(vl[0],vl[1]);
            }
            return dict;
        }
    }
}
