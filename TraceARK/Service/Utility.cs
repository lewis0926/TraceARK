using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceARK.Service
{
    class Utility
    {
        public static void CheckDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Log4net.log.InfoFormat("Create directory: {0}", directory);
                Directory.CreateDirectory(directory);
            }
        }

        public static bool IsBlank(string tempStr)
        {
            return String.Compare(tempStr.Trim(), "") == 0;
        }

        public static void RemoveFiles(string directory)
        {
            Log4net.log.InfoFormat("Delete files in: {0}", directory);
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            directoryInfo.EnumerateFiles().ToList().ForEach(delegate (FileInfo file)
            {
                file.Delete();
            });
        }

        public static void ClearAll()
        {
            Utility.RemoveFiles(ApplicationConfig.InPath);
            Utility.RemoveFiles(ApplicationConfig.OutPath);
            Utility.RemoveFiles(ApplicationConfig.WorkPath);
        }
    }
}
