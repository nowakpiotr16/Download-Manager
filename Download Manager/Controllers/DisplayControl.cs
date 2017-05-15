using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Download_Manager.Classes;

namespace Download_Manager.Controllers
{
    class DisplayControl
    {
        List<File> fileList;
        private MainWindow mainView;
        private long filesSize;
        private long singleFilesize;

        public MainWindow MainView
        {
            get
            {
                return mainView;
            }

            set
            {
                mainView = value;
            }
        }

        public long FilesSize
        {
            get
            {
                return filesSize;
            }

            set
            {
                filesSize = value;
            }
        }

        public DisplayControl()
        {
            fileList = new List<File>();
            singleFilesize = 0;
        }
        
        public void RespondToSearch()
        {

        }

        public void DisplayFilesList(List<File> fileList)
        {
            this.fileList = fileList; // Good? Needed?
            //List<string> fileListNames = new List<string>();
            //fileListNames = FileManager.GetInnerFileListNames(fileList);

            mainView.filesListBox.ItemsSource = fileList;
        }

        public void UpdateProgressBars(object sender, FileDownloadUpdateArgs args)
        {
            mainView.Dispatcher.Invoke(() =>
            {
                double percentDone = (args.CurrentFileDownloadedSize / args.CurrentFileSize) * 100.0;
                mainView.downloadSingleProgressBar.Value = percentDone;

                mainView.downloadAllProgressBar.Value = ((singleFilesize + args.CurrentFileDownloadedSize) / filesSize) * 100.0;
                if (percentDone == 100.0)
                    singleFilesize += (long)args.CurrentFileSize; //TODO FIX                
            });
        }
        //TODO Change from separated label to something within progress bar? Or just adjust label (stuck to progress bar, in the middle etc)
        //TODO Add something to second progress bar?
        public void UpdateProgressBarsText(object sender, FileListDownloadStartArgs args)
        {
            mainView.Dispatcher.Invoke(() =>
            {
                mainView.allProgressBarLabel.Content = args.NumberOfSingleFile + "/" + args.NumberOfAllFiles;
            });
        }
    }
}
