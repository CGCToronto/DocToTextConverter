using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XishuipangUploadInterface
{
    public class Logger : INotifyPropertyChanged
    {
        private static Logger instance;
        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
                return instance;
            }
        }

        private string log;
        public string Log
        {
            get
            {
                return log;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void WriteLine(string line)
        {
            log += line + "\n";
            OnPropertyChange("Log");
        }
    }
}
