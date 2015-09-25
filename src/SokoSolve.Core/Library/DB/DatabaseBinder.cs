using System;
using System.Data;
using System.Data.SqlClient;

namespace Sokoban.Core.Library.DB
{
    public class DatabaseBinder
    {
        private DbFieldLookup lookup;

        private int GetField(IDataReader row, string field)
        {
            if (lookup == null)
            {
                lookup = DBHelper.GetColumnNames(row);
            }
            return lookup[field];
        }

        public int GetInt(IDataReader row, string field)
        {
            try
            {
                var i = GetField(row, field);
                if (row.IsDBNull(i)) return 0;
                return row.GetInt32(i);
            }
            catch (Exception ex)
            {
                throw new Exception("BindingError: "+field, ex);
            }
           
        }

        public string GetString(IDataReader row, string field)
        {
            try
            {
                var i = GetField(row, field);
                if (row.IsDBNull(i)) return null;
                return row.GetString(i);
            }
            catch (Exception ex)
            {
                throw new Exception("BindingError: " + field, ex);
            }
           
        }
        public double GetDouble(IDataReader row, string field)
        {
            try
            {
                var i = GetField(row, field);
                if (row.IsDBNull(i)) return 0d;
                return row.GetDouble(i);
            }
            catch (Exception ex)
            {
                throw new Exception("BindingError: " + field, ex);
            }
           
        }
        public DateTime GetDateTime(IDataReader row, string field)
        {
            try
            {
                var i = GetField(row, field);
                if (row.IsDBNull(i)) return new DateTime();
                return row.GetDateTime(i);
            }
            catch (Exception ex)
            {
                throw new Exception("BindingError: " + field, ex);
            }
         
        }

        public float GetFloat(SqlDataReader row, string field)
        {
            try
            {
                var i = GetField(row, field);
                if (row.IsDBNull(i)) return 0f;
                return row.GetFloat(i);

            }
            catch (Exception ex)
            {
                throw new Exception("BindingError: " + field, ex);
            }
            
        }

        public decimal GetDecimal(SqlDataReader row, string field)
        {
            try
            {
                var i = GetField(row, field);
                if (row.IsDBNull(i)) return 0m;
                return row.GetDecimal(i);

            }
            catch (Exception ex)
            {
                throw new Exception("BindingError: " + field, ex);
            }
        }
    }
}