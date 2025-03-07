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


        private TableOfContent simplifiedTOC = new TableOfContent();
        private TableOfContent traditionalTOC = new TableOfContent();

        public TextToJsonParser(string inputFolder, string outputFolder, int volumeNumber)
        {
            InputFolder = inputFolder;
            OutputFolder = outputFolder;
            VolumeNumber = volumeNumber;
        }

        public bool Parse()
        {

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
                    string[] tokens = Path.GetFileNameWithoutExtension(file).Split('_');
                    string version = tokens.LastOrDefault();
                    int index;
                    int.TryParse(tokens.First(), out index);

                    // Generate the JSON file for this file
                    GenerateJSONFile(file, version);

                    ArticleInfo articleInfo = GetArticleInfo(file);

                    // Add file to dictionaries bases on its version.
                    if (version == "s" || version == "e")
                    {
                        simplifiedTOC.AddNewArticleInfo(articleInfo, index);
                    }

                    if (version == "t" || version == "e")
                    {
                        traditionalTOC.AddNewArticleInfo(articleInfo, index);
                    }
                }
            }

            // Generate table of content for both dictionaries.
            simplifiedTOC.GenerateJSON(OutputFolder + "\\table_of_content_s.json");
            traditionalTOC.GenerateJSON(OutputFolder + "\\table_of_content_t.json");

            return true;
        }

        private ArticleInfo GetArticleInfo(string file)
        {
            ArticleInfo articleInfo = new ArticleInfo();
            // Using stream reader to read article info from file to an ArticleInfo object
            using (StreamReader sr = new StreamReader(file))
            {
                articleInfo.Category = GetNextNonEmptyLine(sr);
                articleInfo.Title = GetNextNonEmptyLine(sr);
                articleInfo.Author = GetNextNonEmptyLine(sr);
                articleInfo.ID = Path.GetFileNameWithoutExtension(file);
            }

            return articleInfo;
        }

        // Read metadata file into a dictionary of key value pairs
        private void ReadMetadata(string filePath)
        {
            Dictionary<string, string> metaDataDictionary = new Dictionary<string, string>();

            using (StreamReader sr = new StreamReader(filePath))
            {
                string line = GetNextNonEmptyLine(sr);
                while (line != null)
                {
                    string[] tokens = line.Split(':');
                    if (metaDataDictionary != null && tokens.Length == 2)
                    {
                        tokens[0] = tokens[0].Trim();
                        tokens[1] = tokens[1].Trim();
                        metaDataDictionary[tokens[0]] = tokens[1];
                    }
                    line = GetNextNonEmptyLine(sr);
                }
            }

            int year = 0;
            if (metaDataDictionary.ContainsKey("year"))
            {
                int.TryParse(metaDataDictionary["year"], out year);
            }

            int month = 0;
            if (metaDataDictionary.ContainsKey("month"))
            {
                int.TryParse(metaDataDictionary["month"], out month);
            }

            string simplifiedTheme = "";
            if (metaDataDictionary.ContainsKey("theme_simplified"))
            {
                simplifiedTheme = metaDataDictionary["theme_simplified"];
            }

            string traditionalTheme = "";
            if (metaDataDictionary.ContainsKey("theme_traditional"))
            {
                traditionalTheme = metaDataDictionary["theme_traditional"];
            }

            string englishTheme = "";
            if (metaDataDictionary.ContainsKey("theme_english"))
            {
                englishTheme = metaDataDictionary["theme_english"];
            }

            if (simplifiedTOC != null)
            {
                simplifiedTOC.SetInfo(VolumeNumber, "simplified", simplifiedTheme + " " + englishTheme, year, month);
            }

            if (traditionalTOC != null)
            {
                traditionalTOC.SetInfo(VolumeNumber, "traditional", traditionalTheme + " " + englishTheme, year, month);
            }

        }

        // Generate JSON file
        public void GenerateJSONFile(string filePath, string version)
        {
            // Get filename without extension
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string outputFileName = OutputFolder + "\\" + fileName + ".json";

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
                        line = line.Replace("\"", "\\\"");
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

    }
}
