using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Download_Manager.Classes;

namespace Download_Manager.Controllers
{
    public sealed class BossControl
    {
        static readonly BossControl instance = new BossControl();
        static private List<File> fileList;
        static private DisplayControl displayer;
        static private DownloadControl downloader;
        private MainWindow mainView;

        public static BossControl Instance
        {
            get
            {
                return instance;
            }
        }

        public MainWindow MainView
        {
            //get
            //{
            //    return mainView;
            //}

            set
            {
                mainView = value;
                displayer.MainView = value;
            }
        }
        
        BossControl()
        {
            fileList = new List<File>();
            displayer = new DisplayControl();
            downloader = new DownloadControl();

            downloader.FileDownloadUpdate += displayer.UpdateProgressBars;
            downloader.FileListDownloadStart += displayer.UpdateProgressBarsText;
        }

        public void Search(string link, string username = null, string password = null)
        {
            //TODO Add downloading single file instantly - so no list, no displaying, just instant download.
            //displayer.Search();

            downloader.Username = username ?? string.Empty;
            downloader.Password = password ?? string.Empty;
            List<File> files = downloader.Search(link);

            displayer.DisplayFilesList(files);
        }

        public void DownloadFile(List<File> requestedFiles, string destination = null)
        {
            destination = destination ?? Environment.CurrentDirectory;
            //displayer.RespondToDownload();

            displayer.FilesSize = downloader.GetSizeAllFiles(requestedFiles);
            downloader.Download(requestedFiles, destination);
            
        }
    }
}
