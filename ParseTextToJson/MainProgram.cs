using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseTextToJson
{
    class MainProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please provide volume number:");
            string volumeNumber = Console.ReadLine();

            Console.WriteLine("Please provide directory that contains all the text files:");
            string inputFolder = Console.ReadLine();

            if (!Directory.Exists(inputFolder))
            {
                Console.Error.WriteLine($"Input folder {inputFolder} doesn't exist.");
            }

            Console.WriteLine("Please provide directory that contains all the output JSON files:");
            string outputFolder = Console.ReadLine();

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            ParseTextToJson.Parse(inputFolder, outputFolder);
        }
    }
}
