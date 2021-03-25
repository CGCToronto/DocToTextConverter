﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Doc;
using Spire.Pdf;

namespace XishuipangUploadUtility
{
    class XishuipangUploadUtility
    {
        static void Main(string[] args)
        {

            Console.WriteLine("What would you like to do?\n" +
                "1 - Convert .doc or .docx files to text files.\n" +
                "2 - Convert text files to JSON files. \n" +
                "3 - Upload JSON files to MongoDB database.");

            string input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    ConvertDocToText();
                    break;
                case "2":
                    ConvertTextToJSON();
                    break;
                case "3":
                    UploadJSON();
                    break;
                default:
                    Console.WriteLine("Input Invalid, please try running the program again.");
                    break;
            }

            Console.WriteLine("Folder to convert: ");
            string folderPath = Console.ReadLine();
            if (Directory.Exists(folderPath))
            {
                var intermediateFolderInfo = Directory.CreateDirectory($"{folderPath}\\intermediate");
                var intermediatePath = intermediateFolderInfo.FullName;
                var outputFolderInfo = Directory.CreateDirectory($"{folderPath}\\output");
                var outputPath = outputFolderInfo.FullName;
                string[] files = Directory.GetFiles(folderPath);

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
                    }
                }

                Console.WriteLine("Conversion succeeded.");
            }
            else
            {
                Console.WriteLine("The directory you provided doesn't exist.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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
