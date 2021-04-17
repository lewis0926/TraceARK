using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceARK.DataClass;
using TraceARK.Service;
using Excel = Microsoft.Office.Interop.Excel;

namespace TraceARK.Handler
{
    class MainHandler
    {
        public static void Start()
        {
            try
            {
                Log4net.log.Info("------------Process Start------------");
                if (LoadData())
                {
                    List<Transaction> transactionList = new List<Transaction>();
                    transactionList = ReadExcel(ApplicationConfig.SkipLine);
                    UpdateARKDB(transactionList);
                }
                Utility.ClearAll();
                Log4net.log.Info("------------Process End------------");
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in main process: {0}", ex));
            }
        }

        private static void UpdateARKDB(List<Transaction> transactionList)
        {
            Log4net.log.Info("Start updating Database");

            SqliteConnection sqliteConnection = null;
            string connectionString;
            string insertValues;
            StringBuilder sqlBuilder = null;

            connectionString = string.Format("Data Source={0}", ApplicationConfig.DatabasePath);
            using (sqliteConnection = new SqliteConnection(connectionString))
            {
                sqliteConnection.Open();

                transactionList.ForEach(delegate (Transaction transaction)
                {
                    var sqliteCommand = sqliteConnection.CreateCommand();

                    insertValues = string.Format(" VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                        transaction.Fund, transaction.Date, transaction.Direction, transaction.Ticker,
                        transaction.Cusip, transaction.Name, transaction.Shares, transaction.PercentETD);

                    sqlBuilder = new StringBuilder();
                    sqlBuilder.Append("INSERT INTO [Transaction] (Fund, Date, Direction, Ticker, Cusip, Name, Shares, PercentETF)")
                        .Append(insertValues);

                    sqliteCommand.CommandText = sqlBuilder.ToString();
                    sqliteCommand.ExecuteNonQuery();
                    Log4net.log.InfoFormat("SQL Command Execute: {0}", sqlBuilder.ToString());

                    sqliteCommand.Dispose();
                    sqlBuilder = null;
                });
                sqliteConnection.Close();
            }
            Log4net.log.Info("Finish updating Database");
        }

        //private static void UpdateARKDB(List<Transaction> transactionList)
        //{
        //    Log4net.log.Info("Start updating Database");

        //    SqlConnection sqlConnection;
        //    string connectionString;
        //    SqlDataAdapter sqlDataAdapter = null;
        //    StringBuilder sqlBuilder = null;
        //    string insertValues;

        //    connectionString = string.Format("Database=ARKTransaction;Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={0};Integrated Security=True", Path.Combine(Environment.CurrentDirectory, "Database", ApplicationConfig.DatabasePath));

        //    using (sqlConnection = new SqlConnection(connectionString))
        //    {
        //        sqlConnection.Open();
        //        Log4net.log.InfoFormat("Open Datebase: {0}", ApplicationConfig.DatabasePath);

        //        transactionList.ForEach(delegate (Transaction transaction)
        //        {                    
        //            sqlBuilder = new StringBuilder();
        //            sqlDataAdapter = new SqlDataAdapter();

        //            insertValues = string.Format(" VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
        //                transaction.Fund, transaction.Date, transaction.Direction, transaction.Ticker,
        //                transaction.Cusip, transaction.Name, transaction.Shares, transaction.PercentETD);

        //            sqlBuilder.Append("INSERT INTO [Transaction] (Fund, Date, Direction, Ticker, Cusip, Name, Shares, PercentETF)")
        //                .Append(insertValues);

        //            sqlDataAdapter.InsertCommand = new SqlCommand(sqlBuilder.ToString(), sqlConnection);
        //            sqlDataAdapter.InsertCommand.ExecuteNonQuery();

        //            sqlDataAdapter = null;
        //            sqlBuilder = null;
        //        });
        //        sqlConnection.Close();
        //    }
        //    Log4net.log.Info("Finish updating Database");
        //}

        private static Boolean LoadData()
        {
            Log4net.log.Info("Initial checking of data");

            Utility.CheckDirectory(ApplicationConfig.InPath);
            Utility.CheckDirectory(ApplicationConfig.OutPath);
            Utility.CheckDirectory(ApplicationConfig.WorkPath);

            DirectoryInfo directoryInfo = new DirectoryInfo(ApplicationConfig.InPath);
            List<FileInfo> fileList = directoryInfo.EnumerateFiles(String.Format("{0}*{1}", ApplicationConfig.DataPrefix, ApplicationConfig.DataExtension)).ToList();

            if (fileList == null || fileList.Count == 0)
            {
                Log4net.log.Info("No ARK Trade files found");
                return false;
            }
            else
            {
                fileList.ForEach(delegate (FileInfo file)
                {
                    if (ApplicationConfig.ArchiveFlag)
                        File.Copy(file.FullName, Path.Combine(ApplicationConfig.ArchivePath, file.Name));
                    File.Move(file.FullName, Path.Combine(ApplicationConfig.WorkPath, file.Name));
                    Log4net.log.InfoFormat("Valid file: {0} moved to Work Path", file.Name);
                });
                return true;
            }
        }

        private static List<Transaction> ReadExcel(int skipLine)
        {
            Transaction transaction = null;
            List<Transaction> transactionList = null;

            Excel.Application xlApp = null ;
            Excel.Workbook xlWorkbook = null;
            Excel.Worksheet xlWorksheet = null;            
            Excel.Range range = null;
            //object misValue = System.Reflection.Missing.Value;
            int rowTotal = 0;

            DirectoryInfo directoryInfo = new DirectoryInfo(ApplicationConfig.WorkPath);
            List<FileInfo> fileList = directoryInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).ToList();

            if (fileList == null || fileList.Count == 0)
                throw new Exception("No excel files found");
            else
            {
                fileList.ForEach(delegate (FileInfo file)
                {
                    Log4net.log.InfoFormat("Read excel file: {0}", file.Name);
                    if (xlApp == null)
                        xlApp = new Excel.Application();
                    else
                        throw new Exception("Previous excel file has not been disposed");
                    xlWorkbook = xlApp.Workbooks.Open(file.FullName, 0, true);
                    if (xlWorkbook.Sheets.Count > 1)
                        throw new Exception("More than 1 worksheet in excel file");
                    xlWorksheet = (Excel.Worksheet)xlWorkbook.Worksheets.get_Item(1);

                    range = xlWorksheet.UsedRange;
                    rowTotal = range.Rows.Count;
                    Log4net.log.InfoFormat("Number of row of data: {0}", rowTotal - skipLine);

                    if (transactionList == null)
                        transactionList = new List<Transaction>();
                    for (int rowCount = skipLine + 1; rowCount <= rowTotal; rowCount++)
                    {
                        transaction = new Transaction();
                        transaction.Fund = range.Cells[rowCount, 1].Value.ToString().Trim();
                        transaction.Date = DateTime.Parse(range.Cells[rowCount, 2].Value.ToString().Trim());
                        transaction.Direction = range.Cells[rowCount, 3].Value.ToString().Trim();
                        transaction.Ticker = range.Cells[rowCount, 4].Value.ToString().Trim();
                        transaction.Cusip = range.Cells[rowCount, 5].Value.ToString().Trim();
                        transaction.Name = range.Cells[rowCount, 6].Value.ToString().Trim();
                        transaction.Shares = Convert.ToInt32(range.Cells[rowCount, 7].Value.ToString().Trim());
                        transaction.PercentETD = Convert.ToDouble(range.Cells[rowCount, 8].Value.ToString().Trim());

                        Log4net.log.InfoFormat("Row{0}: {1}", rowCount - skipLine, transaction.ToString());

                        if (transaction == null)
                            throw new Exception("Empty transaction");
                        transactionList.Add(transaction);
                        transaction = null;
                    }
                    xlApp.Workbooks.Close();

                    range = null;
                    xlWorksheet = null;
                    xlWorkbook = null;
                    xlApp = null;
                });
            }
            return transactionList;
        }
    }
}
