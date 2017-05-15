using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Download_Manager.Classes;
using File = Download_Manager.Classes.File;

namespace Download_Manager.Controllers
{
    class DownloadControl
    {
        public event EventHandler<FileDownloadUpdateArgs> FileDownloadUpdate;
        public event EventHandler<FileListDownloadStartArgs> FileListDownloadStart;

        //List<File> directoryItemList;
        private string address;
        private string username;
        private string password;

        //internal List<File> DirectoryItemList
        //{
        //    get
        //    {
        //        return directoryItemList;
        //    }

        //    set
        //    {
        //        directoryItemList = value;
        //    }
        //}

        public string Username
        {
            get
            {
                return username;
            }

            set
            {
                username = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                password = value;
            }
        }

        public DownloadControl()
        {
            //DirectoryItemList = new List<File>();
        }

        protected void OnFileDownloadUpdate(FileDownloadUpdateArgs args)
        {
            FileDownloadUpdate?.Invoke(this, args);
        }
        protected void OnFileListDownloadStart(FileListDownloadStartArgs args)
        {
            FileListDownloadStart?.Invoke(this, args);
        }

        public List<File> Search(string link)
        {
            address = link;

            List<File> directoryItemList = GetDirectoryInformation(address);
            return FileManager.GetInnerFileList(directoryItemList);
        }
        
        private List<File> GetDirectoryInformation(string address)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(username, password);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;

            List<File> returnValue = new List<File>();
            string[] list = null;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                list = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }

            foreach (string line in list)
            {
                // Windows FTP Server Response Format
                // DateCreated    IsDirectory    Name
                DateTime dateTime = new DateTime();
                               
                // Parse name
                string name = line;

                // Create directory info
                File item = new File();
                item.BaseUri = new Uri(address);
                item.DateCreated = dateTime;
                item.IsDirectory = name.Contains('.')? false : true;
                item.Name = name;

                Console.WriteLine(item.AbsolutePath);
                item.Items = item.IsDirectory ? GetDirectoryInformation(item.AbsolutePath) : null;

                returnValue.Add(item);
            }

            return returnValue;
        }

        private List<File> GetDirectoryInformationDetails(string address)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(address);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(username, password);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;

            List<File> returnValue = new List<File>();
            string[] list = null;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                list = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }

            foreach (string line in list)
            {
                // Windows FTP Server Response Format
                // DateCreated    IsDirectory    Name
                string data = line;
                string date = null;
                DateTime dateTime = new DateTime();
                string dir = null;
                bool isDirectory = false;

                try
                {
                    // Parse date
                    date = data.Substring(0, 17);
                    dateTime = DateTime.Parse(date);
                    data = data.Remove(0, 24);

                    // Parse <DIR>
                    dir = data.Substring(0, 5);
                    isDirectory = dir.Equals("<dir>", StringComparison.InvariantCultureIgnoreCase);
                    data = data.Remove(0, 5);
                    data = data.Remove(0, 10);
                }
                catch
                {
                    // Parse <DIR>
                    dir = data.Substring(0, 5);
                    isDirectory = dir.Equals("<dir>", StringComparison.InvariantCultureIgnoreCase);
                    data = data.Remove(0, 5);
                    data = data.Remove(0, 10);

                    // Parse date
                    date = data.Substring(0, 17);
                    dateTime = DateTime.Parse(date);
                    data = data.Remove(0, 24);
                }

                // Parse name
                string name = data;

                // Create directory info
                File item = new File();
                item.BaseUri = new Uri(address);
                item.DateCreated = dateTime;
                item.IsDirectory = isDirectory;
                item.Name = name;

                Console.WriteLine(item.AbsolutePath);
                item.Items = item.IsDirectory ? GetDirectoryInformation(item.AbsolutePath) : null;

                returnValue.Add(item);
            }

            return returnValue;
        }
        
        public void Download(List<File> requestedFiles, string destination)
        {
            for(int i = 0; i < requestedFiles.Count; i++)
            {
                OnFileListDownloadStart(new FileListDownloadStartArgs() { NumberOfSingleFile = i+1, NumberOfAllFiles = requestedFiles.Count });
                DownloadSingleFile(requestedFiles[i], destination);
            }
        }

        private void DownloadSingleFile(File file, string destination)
        {
            byte[] buffer = new byte[2048];
            int bytesRead = 0;
            FtpWebRequest request = CreateFtpWebRequest(file.AbsolutePath, true);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            Stream reader = request.GetResponse().GetResponseStream();
            FileStream fileStream = new FileStream(destination + "/" + file.Name, FileMode.Create);

            double alreadyDownloaded = 0.0f;
            //Get from List of Sizes from method below 
            //double fileLength = GetSizeSingleFile(file.AbsolutePath);

            while (true)
            {
                bytesRead = reader.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    break;

                fileStream.Write(buffer, 0, bytesRead);

                alreadyDownloaded += bytesRead;
                //double percentDone = (alreadyDownloaded / file.Size) * 100.0;
                OnFileDownloadUpdate(new FileDownloadUpdateArgs() {
                    CurrentFileSize = file.Size,
                    CurrentFileDownloadedSize = alreadyDownloaded,                    
                    FileDownloadedSize = alreadyDownloaded }); 
            }
            fileStream.Close();

        }

        public long GetSizeAllFiles(List<File> files)
        {
            long size = 0;
            
            foreach (var file in files)
            {
                file.Size = GetSizeSingleFile(file.AbsolutePath);
                size += file.Size;

            }

            return size;
        }

        private long GetSizeSingleFile(string path)
        {
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            request.Proxy = null;
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            long size = response.ContentLength;
            response.Close();

            return size;
        }

        private FtpWebRequest CreateFtpWebRequest(string ftpDirectoryPath, bool keepAlive = false)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpDirectoryPath));

            //Set proxy to null. Under current configuration if this option is not set then the proxy that is used will get an html response from the web content gateway (firewall monitoring system)
            request.Proxy = null;

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = keepAlive;

            request.Credentials = new NetworkCredential(username, password);

            return request;
        }
    }
}
