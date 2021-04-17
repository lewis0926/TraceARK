using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceARK.Service;

namespace TraceARK
{
    class ApplicationConfig
    {
        public static bool DevelopmentFlag = false;

        public static bool ArchiveFlag = false;

        public static string ArchivePath;

        public static readonly string IniFilename = "application.ini";

        public static string InPath;

        public static string OutPath;

        public static string WorkPath;

        public static readonly string DatabasePath = Path.Combine(Environment.CurrentDirectory, @"Database\ARKTransaction.db");

        public static int SkipLine;

        public static string DataPrefix;

        public static string DataExtension;

        public static void LoadIni()
        {
            Log4net.log.Info("Start Loading Ini");

            ReadIni();
            InitializeDataFolders();
            DisplayIni();
        }

        private static void ReadIni()
        {
            Log4net.log.Info("Checking Ini file");

            FileInfo iniFileInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, IniFilename));

            if (!iniFileInfo.Exists)
                throw new FileNotFoundException("Ini file does not exist");

            Log4net.log.Info("Reading Ini");

            using (FileStream fileStream = iniFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                StreamReader streamReader = new StreamReader(fileStream);
                string line = string.Empty;

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Trim() == "")
                        continue;

                    if (CompareString(line, "Development"))
                        DevelopmentFlag = BooleanValue("Development", line, false, true);
                    if (CompareString(line, "Archive"))
                        ArchiveFlag = BooleanValue("Archive", line, false, true);
                    if (CompareString(line, "SkipLine"))
                        SkipLine = IntValue("SkipLine", line, 0, true);
                    if (CompareString(line, "DataPrefix"))
                        DataPrefix = StringValue("DataPrefix", line, "", true);
                    if (CompareString(line, "DataExtension"))
                        DataExtension = StringValue("DataExtension", line, "", true);
                }
            }
        }

        public static void DisplayIni()
        {
            Log4net.log.Info("==============================================================================================");
            Log4net.log.Info("Ini Setting");
            Log4net.log.InfoFormat("Development Flag = {0}", DevelopmentFlag);
            Log4net.log.InfoFormat("Archive Flag = {0}", ArchiveFlag);
            Log4net.log.InfoFormat("Data Input Path = {0}", InPath);
            Log4net.log.InfoFormat("Data Outuput Path = {0}", OutPath);
            Log4net.log.InfoFormat("Data Work Path = {0}", WorkPath);
            Log4net.log.InfoFormat("Database Path = {0}", DatabasePath);
            Log4net.log.InfoFormat("Skip Line = {0}", SkipLine);
            Log4net.log.InfoFormat("Data Prefix = {0}", DataPrefix);
            Log4net.log.InfoFormat("Data Extension = {0}", DataExtension);
            Log4net.log.Info("==============================================================================================");
        }

        private static bool CompareString(string line, string parameter)
        {
            return line.StartsWith(parameter, StringComparison.OrdinalIgnoreCase);
        }

        private static string StringValue(string key, string rowData, string defaultValue = "", bool exceptionFlag = false)
        {
            string value = CheckValue(key, rowData, exceptionFlag);

            if (Utility.IsBlank(value))
                value = defaultValue;

            return value;
        }

        private static int IntValue(string key, string rowData, int defaultValue = 0, bool exceptionFlag = false)
        {
            int value = defaultValue;

            string tempStr = CheckValue(key, rowData, exceptionFlag);

            int.TryParse(tempStr, out value);

            return value;
        }

        private static bool BooleanValue(string key, string rowData, bool defaultValue = false, bool exceptionFlag = false)
        {
            bool value = defaultValue;
            string tempStr = CheckValue(key, rowData, exceptionFlag);

            if (!Utility.IsBlank(tempStr))
            {
                if (tempStr.Equals("Y", StringComparison.OrdinalIgnoreCase)
                    || tempStr.Equals("N", StringComparison.OrdinalIgnoreCase))
                    value = tempStr.Equals("Y", StringComparison.OrdinalIgnoreCase);
                else
                {
                    if (exceptionFlag)
                        throw new MissingFieldException("Invalid field or empty field");
                }
            }
            return value;
        }

        private static string CheckValue(string key, string rowData, bool exceptionFlag = false)
        {
            Log4net.log.InfoFormat("Setting {0} value based on {1}", key, rowData);

            string value = rowData.Replace(string.Format("{0}=", key), "");

            if (Utility.IsBlank(value) && exceptionFlag)
                throw new MissingFieldException(string.Format("{0} value is missing in Ini", key));

            return value;
        }

        private static void InitializeDataFolders()
        {
            Log4net.log.Info("Initialize data folders");

            if (DevelopmentFlag)
            {
                string developmentRootPath = @"D:\Development";

                InPath = Path.Combine(developmentRootPath, @"data\TraceARK\in");
                OutPath = Path.Combine(developmentRootPath, @"data\TraceARK\out");
                WorkPath = Path.Combine(developmentRootPath, @"data\TraceARK\work");
                ArchivePath = Path.Combine(developmentRootPath, @"backup\TraceARK");
            }
            else
            {
                string productionRootPath = @"D:\Programs";

                InPath = Path.Combine(productionRootPath, @"data\TraceARK\in");
                OutPath = Path.Combine(productionRootPath, @"data\TraceARK\out");
                WorkPath = Path.Combine(productionRootPath, @"data\TraceARK\work");
                ArchivePath = Path.Combine(productionRootPath, @"backup\TraceARK");
            }

            Utility.CheckDirectory(InPath);
            Utility.CheckDirectory(OutPath);
            Utility.CheckDirectory(WorkPath);
            if (ArchiveFlag)
                Utility.CheckDirectory(ArchivePath);
        }
    }
}
