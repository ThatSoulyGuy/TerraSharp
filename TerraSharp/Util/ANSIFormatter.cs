using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraSharp.Util
{
    public class ANSIFormatter
    {
        public static string Format(string? text, params string[] args)
        {
            ArgumentNullException.ThrowIfNull(text);

            string format = args != null && args.Length > 0 ? string.Format(text, args) : text;

            var replacements = new Dictionary<string, string>
            {
                ["&0"] = "\u001B[30m",
                ["&1"] = "\u001B[34m",
                ["&2"] = "\u001B[32m",
                ["&3"] = "\u001B[36m",
                ["&4"] = "\u001B[31m",
                ["&5"] = "\u001B[35m",
                ["&6"] = "\u001B[33m",
                ["&7"] = "\u001B[37m",
                ["&8"] = "\u001B[90m",
                ["&9"] = "\u001B[94m",
                ["&a"] = "\u001B[92m",
                ["&b"] = "\u001B[96m",
                ["&c"] = "\u001B[91m",
                ["&d"] = "\u001B[95m",
                ["&e"] = "\u001B[93m",
                ["&f"] = "\u001B[97m",
                ["&r"] = "\u001B[0m"
            };

            foreach (var pair in replacements)
                format = format.Replace(pair.Key, pair.Value);
            
            return format;
        }

        public static string DeFormat(string text)
        {
            string format = text;

            format = format.Replace("\u001B[30m", "&0");
            format = format.Replace("\u001B[34m", "&1");
            format = format.Replace("\u001B[32m", "&2");
            format = format.Replace("\u001B[36m", "&3");
            format = format.Replace("\u001B[31m", "&4");
            format = format.Replace("\u001B[35m", "&5");
            format = format.Replace("\u001B[33m", "&6");
            format = format.Replace("\u001B[37m", "&f");
            format = format.Replace("\u001B[0m", "&r");

            return format;
        }
    }
}
