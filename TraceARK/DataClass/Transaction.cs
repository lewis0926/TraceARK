using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TraceARK.DataClass
{
    class Transaction
    {
        public string Fund { get; set; }

        public DateTime Date { get; set; }

        public string Direction { get; set; }

        public string Ticker { get; set; }

        public string Cusip { get; set; }

        public string Name { get; set; }

        public int Shares { get; set; }

        public double PercentETD { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (PropertyInfo propertyInfo in typeof(Transaction).GetProperties())
            {
                if (stringBuilder.ToString().Length > 0)
                    stringBuilder.Append(", ");

                stringBuilder.Append(string.Format("{0}={1}", propertyInfo.Name, propertyInfo.GetValue(this, null)));
            }

            return stringBuilder.ToString();
        }
    }
}
