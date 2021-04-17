using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceARK
{
    class Log4net
    {
        public static log4net.ILog log;

        public static void InitiateLog()
        {
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("Log4net initiated");
        }
    }
}
