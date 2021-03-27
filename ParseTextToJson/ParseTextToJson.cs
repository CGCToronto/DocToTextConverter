using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseTextToJson
{
    public class TextToJsonParser
    {
        public string InputFolder { get; set; }
        public string OutputFolder { get; set; }
        public int VolumeNumber { get; set; }

        public TextToJsonParser(string inputFolder, string outputFolder, int volumeNumber)
        {
            InputFolder = inputFolder;
            OutputFolder = outputFolder;
            VolumeNumber = volumeNumber;
        }

        public bool Parse()
        {
            // TODO: Create two dictionaries, one contains all version s files,
            // TODO: the other contains all version t files.

            string[] allFiles = Directory.GetFiles(InputFolder);
            foreach (string file in allFiles)
            {
                if (Path.GetFileNameWithoutExtension(file) == "metadata")
                {
                    // Read metadata file into a dictionary of key value pairs
                    ReadMetadata(file);

                }
                else if (Path.GetExtension(file) == ".txt")
                {

                    // Check for the version of the file, it is the suffix of the file, either a 's', 't' or 'e'.
                    string version = Path.GetFileNameWithoutExtension(file).Split('_').LastOrDefault();

                    // Generate the JSON file for this file
                    GenerateJSONFile(file, version);

                    // Add file to dictionaries bases on its version.
                    // if it's english version, add to both dictionary

                }
            }

            // Generate table of content for both dictionaries.

            return true;
        }

        // Read metadata file into a dictionary of key value pairs
        private Dictionary<string, string> ReadMetadata(string filename)
        {
            return null;
        }

        // Generate JSON file
        public void GenerateJSONFile(string filePath, string version)
        {
            // Get filename without extension
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string outputFileName = OutputFolder + "/" + fileName + ".json";

            // Set versionText from 's', 't', 'e' to simplified, traditional, english
            string versionText = "simplified";

            if (version == "t")
            {
                versionText = "traditional";
            }
            else if (version == "e")
            {
                versionText = "english";
            }

            // open input file and output file
            using (StreamReader sr = new StreamReader(filePath))
            {
                using (StreamWriter sw = new StreamWriter(outputFileName))
                {
                    // Start writing output file
                    // Write volume, id, character
                    sw.WriteLine("{");
                    sw.WriteLine("    \"volume\": \"" + VolumeNumber + "\",");
                    sw.WriteLine("    \"id\": \"" + fileName + "\",");
                    sw.WriteLine("    \"character\": \"" + versionText + "\",");

                    // Read theme, title, author from the original file and write them in the output file with the key names
                    string theme = GetNextNonEmptyLine(sr);
                    if (!string.IsNullOrEmpty(theme))
                    {
                        sw.WriteLine("    \"category\": \"" + theme + "\",");
                    }

                    string title = GetNextNonEmptyLine(sr);
                    if (!string.IsNullOrEmpty(title))
                    {
                        sw.WriteLine("    \"title\": \"" + title + "\",");
                    }

                    string author = GetNextNonEmptyLine(sr);
                    if (!string.IsNullOrEmpty(author))
                    {
                        sw.WriteLine("    \"author\": \"" + author + "\",");
                    }

                    // Read content from the original file and write them in output file like this:
                    // "content" : [
                    //     "....",
                    //     "....",
                    //     "...."
                    // ]

                    sw.WriteLine("    \"content\": [");

                    string line = GetNextNonEmptyLine(sr);
                    while (line != null)
                    {
                        line.Replace("\"", "\\\"");
                        string contentLine = "        \"" + line + "\"";
                        line = GetNextNonEmptyLine(sr);
                        if (line != null)
                        {
                            contentLine += ",";
                        }

                        sw.WriteLine(contentLine);
                    }

                    sw.WriteLine("    ]");
                    sw.WriteLine("}");
                    sw.WriteLine("");

                    // Close both files
                }
            }
        }

        private string GetNextNonEmptyLine(StreamReader sr)
        {
            string line = null;
            do
            {
                line = sr.ReadLine();
            } while (line == string.Empty);

            return line;
        }

        // Generate table of contents
        private void GenerateTableOfContent()
        {

        }


    }
}
