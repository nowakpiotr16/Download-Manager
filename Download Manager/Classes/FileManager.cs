using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Download_Manager.Classes
{
    class FileManager
    {
        public static List<File> GetInnerFileList(List<File> fileList)
        {
            List<File> files = new List<File>();

            foreach (var file in fileList)
            {
                if (file.IsDirectory)
                    files.AddRange(GetInnerFileList(file.Items));
                else
                {
                    files.Add(file);
                }
            }
            return files;
        }

        public static List<string> GetInnerFileListNames(List<File> fileList)
        {
            List<string> fileNames = new List<string>();

            foreach (var file in fileList)
            {
                if (file.IsDirectory)
                    fileNames.AddRange(GetInnerFileListNames(file.Items));
                else
                {
                    fileNames.Add(file.Name);
                }
            }
            return fileNames;

        }
    }
}
