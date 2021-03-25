using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseTextToJson
{
    public class ParseTextToJson
    {
        public static bool Parse(string inputFolder, string outputFolder)
        {
            string[] allFiles = Directory.GetFiles(inputFolder);
            foreach (string file in allFiles)
            {
                if (Path.GetFileNameWithoutExtension(file) == "metadata")
                {

                }
                if (Path.GetExtension(file) == ".txt")
                {
                    
                }
            }

            return true;
        }
    }
}
