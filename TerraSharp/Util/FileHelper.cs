using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraSharp.Util
{
    public class FileHelper
    {
        public static string LoadFile(string path)
        {
            StringBuilder result = new StringBuilder();

            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        result.Append(line).Append("\n");
                    }
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine("Couldn't find the file at " + path);
                Console.Error.WriteLine(e.Message);
            }

            return result.ToString();
        }
    }
}
