using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OracleTableAnalysis.Entity
{
    public class TableComments
    {
        public string TABLE_NAME { get; set; }
        public string TABLE_TYPE { get; set; }
        public string COMMENTS { get; set; }
    }
}
