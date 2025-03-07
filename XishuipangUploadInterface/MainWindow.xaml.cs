using Microsoft.WindowsAPICodePack.Dialogs;
using ParseTextToJson;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Reflection;

namespace XishuipangUploadInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string docInputFolderString;
        public string DocInputFolderString
        {
            get
            {
                return docInputFolderString;
            }

            set
            {
                if (value != docInputFolderString)
                {
                    docInputFolderString = value;
                    OnPropertyChange("DocInputFolderString");
                }
            }
        }

        // field TextOutputFolderString
        private string textOutputFolderString;
        public string TextOutputFolderString
        {
            get
            {
                return textOutputFolderString;
            }

            set
            {
                if (value != textOutputFolderString)
                {
                    textOutputFolderString = value;
                    OnPropertyChange("TextOutputFolderString");
                }
            }
        }

        // Field: JsonFolderString
        private string jsonFolderString;
        public string JsonFolderString
        {
            get
            {
                return jsonFolderString;
            }

            set
            {
                if (value != jsonFolderString)
                {
                    jsonFolderString = value;
                    OnPropertyChange("JsonFolderString");
                }
            }
        }


        // Field: VolumeNumber
        private int volumeNumber;
        public int VolumeNumber
        {
            get
            {
                return volumeNumber;
            }

            set
            {
                if (value != volumeNumber)
                {
                    volumeNumber = value;
                    OnPropertyChange("VolumeNumber");
                }
            }
        }

        public string VersionNumber { get; set; }

        public Logger InfoLogger { get; set; } = Logger.Instance;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            string appName = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            InfoLogger.WriteLine($"{appName} {version}");
            VersionNumber = $"Version Number: {version}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DocInputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the common open file dialog
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                DocInputFolderString = dialog.FileName;
            }
        }

        // Text file output folder button clicked
        private void TextOutputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            // Define open file dialog
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            // if open file dialog is opened successfully
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Set TextOutputFolderString to the path from the open file dialog
                TextOutputFolderString = dialog.FileName;
            }
        }

        // JSON folder button clicked
        private void JSONFolderButton_Click(object sender, RoutedEventArgs e)
        {
            // Define open file dialog
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            // if open file dialog is opened successfully
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Set TextOutputFolderString to the path from the open file dialog
                JsonFolderString = dialog.FileName;
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if both DocInputFolderString and TextOutputFolderString are set
            if (!String.IsNullOrEmpty(DocInputFolderString) && !String.IsNullOrEmpty(TextOutputFolderString))
            {
                // If so, Call function to convert .docx files to .txt files using the two pathes defined.
                bool result = DocToTextConverter.ConvertDocsToTextFiles(DocInputFolderString, TextOutputFolderString);

                if (result)
                {
                    MessageBox.Show(".doc files are converted successfully.");
                }
                else
                {
                    // TODO: Write better error messages.
                    MessageBox.Show("Error: Can't convert .doc files.");
                }
            }
            else
            {
                // If not, show an error and return
                MessageBox.Show("Make sure that input and output pathes are correct.");
            }
        }

        private void ConvertJSONButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeNumber == 0)
            {
                MessageBox.Show("Please provide a valid volume number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if text folder path and json folder path are set
            if (Directory.Exists(TextOutputFolderString) && Directory.Exists(JsonFolderString))
            {
                // Convert all .txt files to .json files
                TextToJsonParser jsonParser = new TextToJsonParser(TextOutputFolderString, JsonFolderString, VolumeNumber);
                if (jsonParser.Parse())
                {
                    MessageBox.Show("JSON files generated successfully!", "Success", MessageBoxButton.OK);
                }
                
            }
            else
            {
                // show some error messages.
                MessageBox.Show("Either pathes are invalid, please verify these pathes.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuickSetButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Use volume number to quickly set input and output folder pathes so they don't have to be hand-picked.
            // Assuming that this application is located within Xishuipang web project.
            TextOutputFolderString = @"..\text\volume_" + VolumeNumber;
            JsonFolderString = @"..\content\volume_" + VolumeNumber;
        }

        private void UploadToMongoDBButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(JsonFolderString))
            {
                MongoDBUploader uploader = new MongoDBUploader();
                uploader.Upload(JsonFolderString);
            }
            else
            {
                // show some error messages.
                InfoLogger.WriteLine("JSON file folder path is invalid.");
                MessageBox.Show("JSON file folder path is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogUpdated(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            textBox.ScrollToEnd();
        }

        private void OpenTextFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TextOutputFolderString))
            {
                Process.Start("explorer.exe", TextOutputFolderString);
            }
            else
            {
                InfoLogger.WriteLine("Error: Text file folder path not valid.");
            }
        }

        private void OpenJSONFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(JsonFolderString))
            {
                Process.Start("explorer.exe", JsonFolderString);
            }
            else
            {
                InfoLogger.WriteLine("Error: JSON file folder path not valid.");
            }
        }
    }
}
