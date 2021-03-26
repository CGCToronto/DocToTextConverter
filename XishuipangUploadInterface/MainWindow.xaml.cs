using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

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

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
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
                    MessageBox.Show("Error: Can't converting .doc files.");
                }
            }
            else
            {
                // If not, show an error and return
                MessageBox.Show("Make sure that input and output pathes are correct.");
            }
        }
    }
}
