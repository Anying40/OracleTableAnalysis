using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OracleTableAnalysis.Entity
{
    public class TableColumns
    {
        public string COLUMN_NAME { get; set; }
        public string DATA_TYPE { get; set; }
        public string DATA_LENGTH { get; set; }
    }
}
