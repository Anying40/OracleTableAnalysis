using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OracleTableAnalysis.Entity
{
    public class ColumnComment
    {
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public string COMMENTS { get; set; }
    }
}
