
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DapperExtensions;
using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace OracleTableAnalysis
{
    /// <summary>
    /// dapper 帮助类
    /// </summary>
    public class DapperHelper
    {
        private Database Connection = null;
        private OracleCommand Cmd = new OracleCommand();
        public DapperHelper(string conn)
        {
            var orcalConn = new OracleConnection(conn);
            var orcaleconfig = new DapperExtensionsConfiguration(typeof(AutoClassMapper<>), new List<Assembly>(), new OracleDialect());
            var orcaleGenerator = new SqlGeneratorImpl(orcaleconfig);
            Connection = new Database(orcalConn, orcaleGenerator);
            Cmd.Connection = orcalConn;
        }
        public IDbConnection Conn
        {
            get
            {
                return Connection.Connection;
            }
        }

        public string  Reader(string commandText)
        {
            string readerXX = "";
            int actual = 0;
            Cmd.CommandText = commandText;
            var reader =Cmd.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    OracleClob myOracleClob = reader.GetOracleClob(0);

                    StreamReader streamreader = new StreamReader(myOracleClob, Encoding.Unicode);
                    char[] cbuffer = new char[10000];
                    while ((actual = streamreader.Read(cbuffer, 0, cbuffer.Length)) > 0)
                    {
                        readerXX = new string(cbuffer, 0, actual);
                    }
                }
            }
            catch (Exception e)
            {
                return "";
            }
           
            return readerXX;
        }
    }
}