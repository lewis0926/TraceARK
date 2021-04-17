using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using TraceARK.Handler;
using TraceARK.Service;

namespace TraceARK
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Log4net.InitiateLog();

                ApplicationConfig.LoadIni();

                MainHandler.Start();
            }
            catch (Exception ex)
            {
                Log4net.log.ErrorFormat("Error: {0}", ex);
                Utility.ClearAll();
            }
        }
    }
}
