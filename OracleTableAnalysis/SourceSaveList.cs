﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OracleTableAnalysis
{
    public class SourceSaveList
    {
        public string tableName { get; set; }
        public List<TableColumns> cols { get; set; }
    }
}
