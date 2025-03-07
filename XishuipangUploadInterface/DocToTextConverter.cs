using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Doc;
using Spire.Pdf;

namespace XishuipangUploadInterface
{
    class DocToTextConverter
    {
        public static bool ConvertDocsToTextFiles(string inputPath, string outputPath)
        {
            if (Directory.Exists(inputPath))
            {
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                    Logger.Instance.WriteLine($"Output path doesn't exist: {outputPath}");
                    Logger.Instance.WriteLine($"Output path created: {outputPath}");
                }

                var intermediateFolderInfo = Directory.CreateDirectory($"{inputPath}\\intermediate");
                var intermediatePath = intermediateFolderInfo.FullName;
                string[] files = Directory.GetFiles(inputPath);

                foreach (string file in files)
                {
                    SaveDocFileToHTML(intermediatePath, file);
                }

                string[] imageFolders = Directory.GetDirectories(intermediatePath);

                foreach (var folder in imageFolders)
                {
                    var directoryInfo = new DirectoryInfo(folder);
                    var folderName = directoryInfo.Name;
                    string newLocation = Path.Combine(outputPath, folderName);
                    Directory.CreateDirectory(newLocation);
                    foreach (var image in directoryInfo.GetFiles())
                    {
                        image.CopyTo(Path.Combine(newLocation, image.Name), true);
                        Console.WriteLine($"Copied image {image} to {newLocation}");
                        Logger.Instance.WriteLine($"Copied image {image} to {newLocation}");
                    }
                }

                var contentFiles = Directory.GetFiles(intermediatePath);

                foreach (var contentFile in contentFiles)
                {
                    if (Path.GetExtension(contentFile) == ".html")
                    {
                        var outputFile = $"{outputPath}\\{Path.GetFileNameWithoutExtension(contentFile)}.txt";
                        ConvertHTMLToTXTKeepImages(contentFile, outputFile);
                        Console.WriteLine($"Converted {contentFile} from html to {outputFile} with image tags.");
                        Logger.Instance.WriteLine($"Converted {contentFile} from html to {outputFile} with image tags.");
                    }
                }

                Console.WriteLine("Conversion succeeded.");
                Logger.Instance.WriteLine("Conversion succeeded.");
                return true;
            }
            else
            {
                Console.WriteLine("The directory you provided don't exist.");
                Logger.Instance.WriteLine("The directory you provided don't exist.");
                return false;
            }
        }

        private static void SaveDocFileToHTML(string outputPath, string file)
        {
            if (Path.GetExtension(file) == ".doc" || Path.GetExtension(file) == ".docx")
            {
                Document document = new Document();

                document.LoadFromFile(file);
                string name = Path.GetFileNameWithoutExtension(file);
                string destinationName = $"{outputPath}\\{name}.html";
                document.SaveToFile(destinationName, Spire.Doc.FileFormat.Html);
                Console.WriteLine($"Converted {file} to {destinationName}");
                Logger.Instance.WriteLine($"Converted {file} to {destinationName}");
            }
        }

        private static void SavePDFFileToHTML(string outputPath, string file)
        {
            if (Path.GetExtension(file) == ".pdf")
            {
                PdfDocument pdf = new PdfDocument();

                pdf.LoadFromFile(file);
                string name = Path.GetFileNameWithoutExtension(file);
                string destinationName = $"{outputPath}\\{name}.html";
                pdf.SaveToFile(destinationName, Spire.Pdf.FileFormat.HTML);
                Console.WriteLine($"Converted {file} to {destinationName}");
                Logger.Instance.WriteLine($"Converted {file} to {destinationName}");
            }
        }

        private static void ConvertHTMLToTXTKeepImages(string contentFile, string outputFile)
        {
            var content = new List<string>();
            var output = new List<string>();
            try
            {
                using (var inputFileReader = new StreamReader(contentFile))
                {
                    string line;
                    while ((line = inputFileReader.ReadLine()) != null)
                    {
                        content.Add(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot read file {contentFile} due to {e.Message}.");
                Logger.Instance.WriteLine($"Cannot read file {contentFile} due to {e.Message}.");
            }

            foreach (var line in content)
            {
                TranslateHTMLLineToTXTLine(line, out string[] outputLines);
                output.AddRange(outputLines);
            }
            try
            {
                using (var outputFileWriter = new StreamWriter(outputFile))
                {
                    foreach (var line in output)
                    {
                        if (line != "Evaluation Warning: The document was created with Spire.Doc for .NET.")
                        {
                            outputFileWriter.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Cannot write file {contentFile} due to {e.Message}.");
                Logger.Instance.WriteLine($"Cannot write file {contentFile} due to {e.Message}.");
            }
        }

        enum TranslatorState
        {
            Neutral,
            ReadingTag,
            ExpectImagePath,
            ReadingImagePath,
            ReadingHTMLSymbol,
        }

        private static void TranslateHTMLLineToTXTLine(string line, out string[] outputLines)
        {
            var outputList = new List<string>();
            string tag = "";
            string outputStr = "";
            TranslatorState translatorState = TranslatorState.Neutral;
            foreach (var character in line)
            {
                switch (translatorState)
                {
                    case TranslatorState.Neutral:
                        {
                            if (character == '<')
                            {
                                tag = "";
                                translatorState = TranslatorState.ReadingTag;
                            }
                            else if (character == '&')
                            {
                                translatorState = TranslatorState.ReadingHTMLSymbol;
                            }
                            else
                            {
                                outputStr += character;
                            }
                            break;
                        }
                    case TranslatorState.ReadingHTMLSymbol:
                        {
                            if (character == ';')
                            {
                                translatorState = TranslatorState.Neutral;
                            }
                            break;
                        }
                    case TranslatorState.ReadingTag:
                        {
                            if (character == ' ' && tag == "img")
                            {
                                tag = "";
                                if (!string.IsNullOrEmpty(outputStr))
                                {
                                    outputList.Add(outputStr);
                                    outputStr = "";
                                }
                                translatorState = TranslatorState.ExpectImagePath;
                            }
                            else if (character == '>')
                            {
                                translatorState = TranslatorState.Neutral;
                            }
                            else
                            {
                                tag += character;
                            }
                            break;
                        }
                    case TranslatorState.ExpectImagePath:
                        {
                            if (character == '"')
                            {
                                translatorState = TranslatorState.ReadingImagePath;
                            }
                            else if (character == '>')
                            {
                                translatorState = TranslatorState.Neutral;
                            }
                            break;
                        }
                    case TranslatorState.ReadingImagePath:
                        {
                            if (character == '"')
                            {
                                if (!string.IsNullOrEmpty(outputStr))
                                {
                                    outputList.Add($"<{outputStr}>");
                                    outputStr = "";
                                }
                                translatorState = TranslatorState.ReadingTag;
                            }
                            else
                            {
                                outputStr += character;
                            }
                            break;
                        }
                    default:
                        Debug.Fail("This case doesn't exist yet.");
                        break;
                }
            }
            outputList.Add(outputStr);
            outputLines = outputList.ToArray();
        }
    }
}
