using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Sokoban.Core.Library.DB
{
    public static class DBHelper
    {
      

        public class SqlText
        {
            public SqlText(string text)
            {
                Text = text;
            }

            public string Text { get; private set; }

            public override string ToString()
            {
                return Text;
            }
        }


        public static T ExecuteScalarCommand<T>(string connectionString, string sqlFormat, params object[] args)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            var sqlText = Utils.SafeStringFormat(sqlFormat, Utils.Escape(args));
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }
                    using (var command = con.CreateCommand())
                    { 
                        command.CommandText = sqlText;
                        var id = command.ExecuteScalar();
                        try
                        {
                            if (id == DBNull.Value) return default(T);
                            return (T)id;
                        }
                        catch (InvalidCastException)
                        {
                            if (id == null) throw;
                            throw new InvalidCastException(string.Format("Expected {0} but got {1}", typeof(T).FullName, id.GetType().FullName));
                        }
                    }
                }
            }
            catch (SqlException sql)
            {
                throw HandleException(sql, connectionString, sqlText);
            }
        }


        public static void ExecuteCommand(string connectionString, string sqlFormat, params object[] args)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            var sqlText = Utils.SafeStringFormat(sqlFormat, Utils.Escape(args));
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }

                    using (var command = con.CreateCommand())
                    {
                        
                        command.CommandText = sqlText;
                        command.ExecuteNonQuery();
                        
                    }
                }
            }
            catch (SqlException sql)
            {
               
                throw HandleException(sql, connectionString, sqlText);
            }
        }

     

        public static List<T> ExecuteQuery<T>(string connectionString, Func<SqlDataReader, T> readRow, string sql, params object[] sqlParams)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");

            var sqlText = Utils.SafeStringFormat(sql, Utils.Escape(sqlParams));
           
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                   
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    using (var command = conn.CreateCommand())
                    {
                       
                        command.CommandText = sqlText;
                        using (var reader = command.ExecuteReader())
                        {
                            var result = new List<T>();
                            while (reader.Read())
                            {
                                result.Add(readRow(reader));
                            }
                          
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               
                throw HandleException(ex, connectionString, sqlText);
            }
        }
        public static T ExecuteQuerySingle<T>(string connectionString, Func<SqlDataReader, T> readRow, string sql, params object[] sqlParams)
        {
            var data = ExecuteQuery(connectionString, readRow, sql, sqlParams);
            if (data == null) return default(T);
            return data.FirstOrDefault();
        }



        private static Exception HandleException(Exception sql, string connectionString, string sqlText)
        {
            return new Exception(sql.Message + Environment.NewLine + Environment.NewLine + sqlText.ToString());
        }


        public static class Utils
        {

            public static string SafeStringFormat(string stringFormat, object[] args)
            {
                if (stringFormat == null) return null;
                if (args == null) return stringFormat;
                if (args.Length == 0) return stringFormat;
                return String.Format(stringFormat, args);
            }


            public static string MakeConnectionString(string name, string machine = "localhost")
            {
                return string.Format("Data Source={1};Initial Catalog={0};Integrated Security=True",
                    name, machine);
            }


            public static string FormatSQL(string sqlTemplate, params object[] args)
            {
                return SafeStringFormat(sqlTemplate, Escape(args));
            }



            public static object[] Escape(object[] args)
            {
                if (args == null) return null;
                if (args.Length == 0) return args;

                var clone = new object[args.Length];
                for (int cc = 0; cc < args.Length; cc++)
                {
                    clone[cc] = Escape(args[cc]);
                }
                return clone;
            }

            /// <summary>
            /// For safe date comparisons in sql use ISO 8601 date formats
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static string Make8601LongFormat(DateTime input)
            {
                return input.ToString("yyyy-MM-dd'T'HH:mm:ss.fff", DateTimeFormatInfo.InvariantInfo);
            }

            public static string Escape(object arg)
            {
                if (arg == null) return "NULL";

                if (arg is SqlText)
                {
                    // Don't escape
                    return arg.ToString();
                }
                if (arg is DateTime || arg is DateTime?)
                {
                    var dt = (DateTime)arg;
                    if (dt == DateTime.MinValue) return "NULL";
                    return "'" + Make8601LongFormat((DateTime)arg) + "'";
                }
                if (arg is Enum)
                {
                    return ((int)arg).ToString();
                }
                if (arg is bool)
                {
                    return (bool)arg ? "1" : "0";
                }
                var toStr = arg.ToString();
                var escaped = toStr.Replace("'", "''");
                return "N'" + escaped + "'";
            }

            public static string EscapeStringNoQuotes(string raw)
            {
                if (raw == null) return null;
                return raw.Replace("'", "''");
            }


            public static string EscapeLIKE(string filter)
            {
                filter = filter ?? string.Empty;
                return string.Format("N'%{0}%'", filter.Replace("\'", "\'\'"));
            }


        }


        public static bool ExistsTable(string db, string table, string schema = "dbo")
        {
            var sqlText = string.Format("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='{0}' AND TABLE_NAME='{1}'", schema, table);
            try
            {
                using (var conn = new SqlConnection(db))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = sqlText;
                        using (var reader = command.ExecuteReader())
                        {
                            return reader.HasRows;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw HandleException(ex, db, sqlText);
            }
        }




        public static DbFieldLookup GetColumnNames(IDataReader reader)
        {
            var res = new DbFieldLookup();
            for (int cc = 0; cc < reader.FieldCount; cc++)
                res.Add(reader.GetName(cc));
            return res;
        }

    }



    public class DbFieldLookup : List<string>
    {
        private Dictionary<string, int> indexes = new Dictionary<string, int>();

        public bool HasField(string field)
        {
            return indexes.ContainsKey(field.ToLowerInvariant());
        }

        public int this[string field]
        {
            get
            {
                if (!HasField(field)) return -1;
                return indexes[field.ToLowerInvariant()];
            }
        }

        new public void Add(string field)
        {
            base.Add(field);
            if (!HasField(field)) // covers the case where a dataset has more than one instance of this col name
            {
                indexes.Add(field.ToLowerInvariant(), Count - 1);
            }
        }

    }

    public abstract class DBTableReader<T>
    {
        protected DbFieldLookup Fields { get; private set; }
        protected IDataReader Reader { get; private set; }

        public T ReadRow(IDataReader reader)
        {
            if (Fields == null)
            {
                Reader = reader;
                Fields = DBHelper.GetColumnNames(reader);
            }

            return ReadRowImpl();
        }

        protected abstract T ReadRowImpl();

        protected string ReadString(string name)
        {
            var idx = Fields[name];
            if (idx < 0) return null;
            if (Reader.IsDBNull(idx)) return null;
            return Reader.GetString(idx);
        }

        protected DateTime ReadDateTime(string name)
        {
            var idx = Fields[name];
            if (idx < 0) return DateTime.MinValue;
            if (Reader.IsDBNull(idx)) return DateTime.MinValue;
            return Reader.GetDateTime(idx);
        }

        protected int ReadInteger(string name)
        {
            var idx = Fields[name];
            if (idx < 0) return int.MinValue;
            if (Reader.IsDBNull(idx)) return int.MinValue;
            return Reader.GetInt32(idx);
        }

        protected int? ReadIntegerNullable(string name)
        {
            var idx = Fields[name];
            if (idx < 0) return null;
            if (Reader.IsDBNull(idx)) return null;
            return Reader.GetInt32(idx);
        }

        protected long ReadLong(string name)
        {
            var idx = Fields[name];
            if (idx < 0) return long.MinValue;
            if (Reader.IsDBNull(idx)) return long.MinValue;
            return Reader.GetInt64(idx);
        }

        protected long? ReadLongNullable(string name)
        {
            var idx = Fields[name];
            if (idx < 0) return null;
            if (Reader.IsDBNull(idx)) return null;
            return Reader.GetInt64(idx);
        }

        protected double? ReadDouble(string name)
        {
            var idx = Fields[name];
            if (idx < 0) return null;
            if (Reader.IsDBNull(idx)) return null;
            return (float)Reader.GetDouble(idx);
        }



    }

}