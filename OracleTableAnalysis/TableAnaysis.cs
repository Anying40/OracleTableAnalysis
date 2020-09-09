using Dapper;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using OracleTableAnalysis.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OracleTableAnalysis
{
    public partial class TableAnaysis : Form
    {
        //实体模型路径
        //public static string modelpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DAO");
        public static string modelpath = Path.Combine("D:\\Work\\开发库\\mzzj\\03.服务层\\02.Domain", "");

        public string TableType => textBoxBiaoLX.Text;
        //实际数据结构获取源
        public string sourcedbconnStr => textBoxYuan.Text;
        //要比较的数据库连接串
        public string todbconnStr => textBoxMuBiao.Text;
        //缓存数据结构对象  如果存在 则不再去sourcedb中获取结构
        public static string SaveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sourceList.txt");
        public static string SaveTablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tableList.txt");

        //生成的添加字段语句
        public static string SqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "SQL");
        public static string SqlAddFilePath = Path.Combine(SqlFilePath, "add.txt");
        public static string SqlModifyFilePath = Path.Combine(SqlFilePath, "modify.txt");
        public static string SqlTableFilePath = Path.Combine(SqlFilePath, "table.txt");
        public static string SqlIndexFilePath = Path.Combine(SqlFilePath, "index.txt");
        public static string SqlViewFilePath = Path.Combine(SqlFilePath, "view.txt");
        public List<string> GetAllTableName()
        {
            var tableType = TableType.Split(',');
            DirectoryInfo di = new DirectoryInfo(modelpath);
            var res = new List<string>();
            foreach (var directoryInfo in di.GetDirectories())
            {
                if (!directoryInfo.Name.Contains("MZZJ")) continue;
                 
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    foreach (var s in tableType)
                    {
                        if (directory.Name.Contains(s))
                        {
                            res.Add(directory.Name);
                        }
                    } 
                }
            } 
            return res;
        }
        
        public string PATH => AppDomain.CurrentDomain.BaseDirectory;
        const string _SEQUENCE_SQL = "SELECT * FROM kf_shengjijb where jiaobenlx ='SEQUENCE' and JIAOBENNR like '%{0}%'";
        public TableAnaysis()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>();
            using (StreamReader sr = new StreamReader(PATH + "tableInfo.txt"))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length > 0 && !line.Contains("--"))
                    {
                        list.Add(line);
                    }

                }
            }
            list = list.Where(w => !w.Contains("drop index")).ToList();


            //获取重命名信息
            var rename = list.Where(w => w.Contains(" rename ")).ToList();
            list = list.Where(w => !(w.Contains(" rename "))).ToList();
            //获取注释信息
            var comments = list.Where(w => w.Contains("comment on ") || w.Contains("is '")).ToList();
            list = list.Where(w => !(w.Contains("comment on ") || w.Contains("is '"))).ToList();
            //获取修改字段
            var modify = list.Where(w => w.Contains(" modify ")).ToList();
            modify = modify.Where(w => !(w.Contains("default") || w.Contains("null") || w.Contains("BINGRENID"))).ToList();

            list = list.Where(w => !w.Contains(" modify ")).ToList();
            //获取增加字段
            var add = list.Where(w => w.Contains(" add ") && w.Contains("alter table")).ToList();
            list = list.Where(w => !(w.Contains(" add ") && w.Contains("alter table"))).ToList();
            //获取删除字段
            var drop = list.Where(w => w.Contains("drop ")).ToList();
            list = list.Where(w => !w.Contains("drop ")).ToList();
            //新增索引create unique index
            var temp = list.Where(w => w.Contains("create index") || w.Contains("create bitmap index") || w.Contains("create unique index")).ToList();
            List<string> index = new List<string>();
            temp.ForEach(f =>
            {
                index.Add(f + ";");
            });
            list = list.Where(w => !w.Contains("create index") || w.Contains("create bitmap index") || w.Contains("create unique index")).ToList();
            //获取新增表
            var table = list.Where(w => w.Contains("create ") || w == "(" || w.Substring(w.Length - 1, 1) == "," || w == "  );"
            || w.Contains("add ") || w.Contains("constraint ") || w.Contains("alter table") || w.Contains("VARCHAR2") || w.Contains("DATE")
            || w.Contains("NUMBER") || w.Contains("CLOB")).ToList();

            list = list.Where(w => !(w.Contains("create ") || w == "(" || w.Substring(w.Length - 1, 1) == "," || w == "  );"
             || w.Contains("add ") || w.Contains("constraint ") || w.Contains("alter table") || w.Contains("VARCHAR2")
             || w.Contains("DATE") || w.Contains("NUMBER") || w.Contains("CLOB"))).ToList();

            Write(rename, "rename");
            Write(comments, "comments");
            Write(modify, "modify");
            Write(add, "add");
            Write(index, "index");
            Write(drop, "drop");
            Write(table, "table");
            Write(list, "other");
            MessageBox.Show("生成表结构信息成功");
        }
        private void Write(List<string> list, string name)
        {
            using (StreamWriter streamW = new StreamWriter(@PATH + name + ".txt", true, Encoding.UTF8))
            {
                foreach (var item in list)
                {
                    streamW.WriteLine(item);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>();
            using (StreamReader sr = new StreamReader(PATH + "table.txt"))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length > 0 && line.Contains("create table "))
                    {
                        line = line.Replace("create table ", "");
                        list.Add(line);
                    }
                }
            }
            string allName = list.Aggregate("", (current, t) => current + ("'" + t + "',")).TrimEnd(',');
            list.Add(allName);
            Write(list, "tableName");
            MessageBox.Show("生成表名称信息成功");

        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>();
            using (StreamReader sr = new StreamReader(PATH + "table.txt"))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length > 0 && (line.Contains("create table ")|| line.Contains("CREATE TABLE ")))
                    {
                        line = line.Replace("create table ", "");
                        line = line.Replace("CREATE TABLE ", "");
                        line = line.Replace("CREATE TABLE ", "");
                        line = line.Replace("  \"", "");
                        line = line.Replace("\" ", "");
                        list.Add(line);
                    }
                }
            }
            List<string> SEQSQL = new List<string>();
            list.ForEach(f =>
            {
                string sql;
                sql = string.Format(_SEQUENCE_SQL, f);
                SEQSQL.Add(sql);
                SEQSQL.Add("union all");
            });
            Write(SEQSQL, "seqsql");
            MessageBox.Show("生成序号SQL信息成功");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            List<string> seq = new List<string>();
            int n = 10;
            List<string> list = new List<string>();
            using (StreamReader sr = new StreamReader(PATH + "seqinfo.txt"))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    list.Add(line);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Contains("-- Create sequence"))
                    {
                        int k = 2;
                        for (int j = 0; j < n; j++)
                        {
                            if (i + n > list.Count) goto WRITE;
                           
                            if (list[i + j].Contains(";"))
                            {
                                k += j;
                                break;
                            }
                        }
                        for (int p = i; p < i + k; p++)
                        {
                            seq.Add(list[p]);

                        }
                    }
                }
            }
            WRITE:
            Write(seq, "seq");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //记录所有的源数据库 数据结构
            List<SourceSaveList> sl = new List<SourceSaveList>();
            List<string> tableNames = new List<string>();

            var sourceDB = new DapperHelper(sourcedbconnStr);
            if (File.Exists(SaveTablePath))
            {
                using (FileStream fs = new FileStream(SaveTablePath, FileMode.Open))
                {
                    using (StreamReader sw = new StreamReader(fs))
                    {
                        var ss = sw.ReadToEnd();
                        tableNames = JsonConvert.DeserializeObject<List<string>>(ss);
                    }
                }
            }
            else
            {
                tableNames = GetAllTableName();
            }

               
            if (File.Exists(SaveFilePath))
            {
                using (FileStream fs = new FileStream(SaveFilePath, FileMode.Open))
                {
                    using (StreamReader sw = new StreamReader(fs))
                    {
                        var ss = sw.ReadToEnd();
                        sl = JsonConvert.DeserializeObject<List<SourceSaveList>>(ss);
                    }
                }
            }
            else
            {
                
                foreach (var item in tableNames)
                {
                    var source = sourceDB.Conn.Query<TableColumns>(
                                $"select column_name,data_type,data_length from user_tab_columns where table_name='{item}'")
                            .ToList();
                    sl.Add(new SourceSaveList() { cols = source, tableName = item });
                }
            }
            var toDB = new DapperHelper(todbconnStr);
            StringBuilder sb = new StringBuilder();
            StringBuilder sbmodify = new StringBuilder();
            StringBuilder sbtable = new StringBuilder();
            StringBuilder sbindex = new StringBuilder();
            int i = 0;
            int count = tableNames.Count;
            progressBar1.Value = 0;
            foreach (var item in tableNames)
            {
                progressBar1.Value = ++i * 100 / tableNames.Count;
                var source = sl.Find(c => c.tableName == item)?.cols;
                if (source == null) continue;

                var to =
                    toDB.Conn.Query<TableColumns>($"select column_name,data_type,data_length from user_tab_columns where table_name='{item}'")
                    .ToList();
                if (to.Count == 0) //目标数据库无表
                {
                    string tableXX = sourceDB.Reader($"select dbms_metadata.get_ddl('TABLE','{item}') from dual");

                    //tableXX = tableXX.Replace(""HIS3".","");
                    List<string> list = GetList(tableXX);

                    tableXX = "";
                    foreach (var s in list)
                    {

                        tableXX += s + "\r\n";
                        if (s.Contains("PRIMARY KEY"))
                        {
                            tableXX += ");\r\n";
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(tableXX)) continue;
                    tableXX = ReplaceHis(tableXX);
                    var sourceIndexList = sourceDB.Conn.Query<Indexes>(
                          $"select index_name from user_indexes where table_name='{item}'And uniqueness != 'UNIQUE'")
                      .ToList();
                    string indexXX = "";
                    foreach (var indexese in sourceIndexList)
                    {
                        var index = sourceDB.Reader($"select dbms_metadata.get_ddl('INDEX','{indexese.Index_Name}') from dual");

                        list = GetList(index);
                        foreach (var s in list)
                        {
                            if (s.Contains("CREATE INDEX"))
                            {
                                indexXX += s + ";\t\n";
                                break;
                            }
                        }
                    }

                    string tableCommentXX = "";
                    var tableComments =
                        sourceDB.Conn.Query<TableComments>($"select table_name,table_type,comments from user_tab_comments where table_name='{item}'")
                            .FirstOrDefault();
                    if (tableComments != null)
                    {
                        tableCommentXX = $"comment  on  table {item}   is '{tableComments.COMMENTS}';\r\n ";
                    }

                    string colCommentXX = "";
                    var colCommentsList =
                        sourceDB.Conn.Query<ColumnComment>($"select table_name,column_name,comments from user_col_comments where table_name='{item}'")
                            .ToList();
                    foreach (var columnComment in colCommentsList)
                    {
                        colCommentXX += $"comment  on column {item}  add  {columnComment.COLUMN_NAME} is '{columnComment.COMMENTS}';\r\n ";

                    }
                    sbtable.Append(tableXX);
                    sbtable.Append(tableCommentXX);
                    sbtable.Append(indexXX);
                    sbtable.Append(colCommentXX);
                    continue;
                }

                string otherIndexXX = "";
                var sourceIndex = sourceDB.Conn.Query<Indexes>(
                        $"select index_name from user_indexes where table_name='{item}'And uniqueness != 'UNIQUE'")
                    .ToList();
                var toIndex = toDB.Conn.Query<Indexes>(
                        $"select index_name from user_indexes where table_name='{item}'And uniqueness != 'UNIQUE'")
                    .ToList();

                var otherIndex = sourceIndex.Where(w => !toIndex.Exists(o => o.Index_Name == w.Index_Name)).ToList();
                foreach (var indexese in otherIndex)
                {
                    var index = sourceDB.Reader($"select dbms_metadata.get_ddl('INDEX','{indexese.Index_Name}') from dual");

                    var list = GetList(index);
                    foreach (var s in list)
                    {
                        if (s.Contains("CREATE INDEX"))
                        {
                            otherIndexXX += s + ";\t\n";
                            break;
                        }
                    }
                }

                otherIndexXX = ReplaceHis(otherIndexXX);
                sbindex.Append(otherIndexXX);

                if (source.Count == to.Count) continue;


                var colComments =
                    sourceDB.Conn.Query<ColumnComment>($"select table_name,column_name,comments from user_col_comments where table_name='{item}'")
                        .ToList();
                foreach (var source_column in source)
                {
                    if (to.Any(c => c.COLUMN_NAME == source_column.COLUMN_NAME))
                    {
                        var toColumnLength = to.Find(f => f.COLUMN_NAME == source_column.COLUMN_NAME).DATA_LENGTH;
                        if (Convert.ToDecimal(toColumnLength) < Convert.ToDecimal(source_column.DATA_LENGTH))
                        {
                            sbmodify.Append($"alter  table {item}  modify  {source_column.COLUMN_NAME}  {source_column.DATA_TYPE}({source_column.DATA_LENGTH}); --长度从{toColumnLength}改为{source_column.DATA_LENGTH} ");
                            sbmodify.Append("\r\n");
                        }
                        continue;
                    }
                    switch (source_column.DATA_TYPE)
                    {
                        case "DATE":
                            sb.Append($"alter  table {item}  add  {source_column.COLUMN_NAME}  DATE;");
                            break;
                        case "TIMESTAMP(6)":
                            sb.Append($"alter  table {item}  add  {source_column.COLUMN_NAME}  TIMESTAMP(6);");
                            break;
                        case "TIMESTAMP":
                            sb.Append($"alter  table {item}  add  {source_column.COLUMN_NAME}  TIMESTAMP({source_column.DATA_LENGTH});");
                            break;
                        default:
                            sb.Append($"alter  table {item}  add  {source_column.COLUMN_NAME}  {source_column.DATA_TYPE}({source_column.DATA_LENGTH});");

                            break;
                    }
                    sb.Append("\r\n");
                    var comment = colComments.Find(f => f.COLUMN_NAME == source_column.COLUMN_NAME)?.COMMENTS;
                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        sb.Append($"comment  on column {item}  add  {source_column.COLUMN_NAME} is '{comment}'; ");
                        sb.Append("\r\n");
                        sb.Append("\r\n");
                    }

                }
            }

            //记录所有的源数据库 数据结构
            List<Views> viewList = new List<Views>();
            var sourceViewTemp = sourceDB.Conn.Query<Views>($"select view_name from user_views").ToList();
            var toViewTemp = toDB.Conn.Query<Views>($"select view_name from user_views ").ToList();
            var tableType = TableType.Split(',');
            foreach (var viewse in sourceViewTemp)
            {
                foreach (var s in tableType)
                {
                    if (viewse.VIEW_NAME.Contains(s))
                    {
                        viewList.Add(viewse);
                    }
                }
            }

            var otherView = viewList.Where(w => !toViewTemp.Exists(o => o.VIEW_NAME == w.VIEW_NAME)).ToList();
            StringBuilder sbView = new StringBuilder();
            foreach (var viewse in otherView)
            {
                sbView.Append(viewse.VIEW_NAME);
                sbView.Append("\r\n"); 
            }
            if (false == System.IO.Directory.Exists(SqlFilePath))
            {
                //创建文件夹
                System.IO.Directory.CreateDirectory(SqlFilePath);
            }
            using (FileStream fs = new FileStream(SqlViewFilePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(sbView.ToString());
                }
            }

            using (FileStream fs = new FileStream(SaveFilePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(sl));
                }
            }
            using (FileStream fs = new FileStream(SaveTablePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(tableNames));
                }
            }
            using (FileStream fs = new FileStream(SqlAddFilePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(sb.ToString());
                }
            }
            using (FileStream fs = new FileStream(SqlModifyFilePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(sbmodify.ToString());
                }
            }
            using (FileStream fs = new FileStream(SqlTableFilePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(sbtable.ToString());
                }
            }
            using (FileStream fs = new FileStream(SqlIndexFilePath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(sbindex.ToString());
                }
            }

            MessageBox.Show("拉取差异结构数据成功！");
            System.Diagnostics.Process.Start(SqlFilePath);
        }

        private List<string> GetList(string readerXX)
        {
            List<string> list = new List<string>();
            using (StringReader sr = new StringReader(readerXX))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }

            return list;
        }

        private string ReplaceHis(string xinXi)
        {
            xinXi = xinXi.Replace("\"HIS3\".", "");
            xinXi = xinXi.Replace("\"HIS4\".", "");
            xinXi = xinXi.Replace("\"HIS5\".", "");
            xinXi = xinXi.Replace("\"HIS6\".", "");
            xinXi = xinXi.Replace("\"HIS6YS\".", "");
            return xinXi;
        }

         
    }
    public class Model
    {
        public string XinXi { get; set; }
    }
}
