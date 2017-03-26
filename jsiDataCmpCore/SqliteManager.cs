using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace jsiDataCmpCore
{
    public class SqliteManager: IDatabaseManager
    {
        public string Location { get; set; }
        public bool HasSchema => false;
        private readonly string _conString;

        public SqliteManager(string connectionString, string fileName)
        {
            Location = fileName;
            _conString = connectionString;
        }
        private string CreateWhere(Table table, Dictionary<string, object> values)
        {
            var ret = " WHERE ";
            foreach (var colName in table.PrimaryKeyColumns)
            {
                ret += colName + "=@" + colName + " AND ";
            }
            ret = ret.Remove(ret.Length - 5);

            return ret;
        }

        public List<Table> GetTables()
        {
            var sql = "select * from sqlite_master where type='table'";
            var ret = new List<Table>();
            using (var cn = new SQLiteConnection(_conString))
            {
                using (var cmd = new SQLiteCommand(sql, cn))
                {
                    cn.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var tableSql = reader.GetValue<string>("sql");

                        ret.Add(new Table
                        {
                            SchemaName = "",
                            TableName = reader.GetValue<string>("name")
                            ,IdentityColumn = GetIdentityColumn(tableSql)
                            ,PrimaryKeyColumns = GetPrimaryKeys(tableSql)
                            ,
                            Include = true
                        });
                    }
                }
            }

            return ret;
        }

        private List<string> GetColumnDefinitions(string tableSql)
        {
            var startPos = tableSql.IndexOf("(") +1;
            var endPos = tableSql.LastIndexOf(")");
            var cols = tableSql.Substring(startPos, endPos - startPos);

            var ret= cols.Split(new char[] {','}).ToList();
            return ret;

        }
        private string GetIdentityColumn(string tableSql)
        {
            return "";
        }

        private List<string> GetPrimaryKeys(string tableSql)
        {
            var colDefs = GetColumnDefinitions(tableSql);
            var ret = new List<string>();
            foreach (var colDef in colDefs)
            {
                if (colDef.Contains("PRIMARY KEY")) ret.Add(GetColName(colDef));
            }
            return ret;

        }
        private string GetColName(string colDef)
        {
            // `
            var startPos = colDef.IndexOf("`") + 1;
            var endPos = colDef.IndexOf("`", startPos +1);
            var colName = colDef.Substring(startPos, endPos - startPos);
            return colName;
        }

        public Dictionary<string, object> GetRow(Table table, Dictionary<string, object> values)
        {
            //If there is no way of finding a unique row don't even try.
            if (table.PrimaryKeyColumns == null || table.PrimaryKeyColumns.Count == 0) return null;

            var sql = $"select * from {table.TableName}" + CreateWhere(table, values);
            using (var cn = new SQLiteConnection(_conString))
            {
                using (var cmd = new SQLiteCommand(sql, cn))
                {
                    cn.Open();
                    foreach (var primaryKey in table.PrimaryKeyColumns)
                    {
                        cmd.Parameters.AddWithValue(primaryKey, values[primaryKey]);
                    }
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        return reader.GetValuesDictionary();
                    }
                }
            }
            return null;
        }

        public int GetRowCount(Table table)
        {
            throw new NotImplementedException();
        }


        public void Insert(Table table, Dictionary<string, object> values)
        {
            var columnNames = string.Join(",", values.Keys);
            var valueNames = "@" + columnNames.Replace(",", ",@");

            var insert = $"INSERT INTO {table.TableName} ({columnNames}) VALUES ({valueNames});";
            using (var cn = new SQLiteConnection(_conString))
            {
                string sql = insert;
                cn.Open();
                using (var cmd = new SQLiteCommand(sql, cn))
                {
                    foreach (var column in values.Keys)
                    {
                        cmd.Parameters.AddWithValue(column, values[column]);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ReadSource(Table table, IDatabaseManager destManager, Action<string, int, int> updateStatus)
        {
            throw new NotImplementedException();
        }

        public void UpdateDestination(Table table, Dictionary<string, object> values)
        {
            throw new NotImplementedException();
        }

        
    }
    public static class SqliteExtensions
    {
        public static T GetValue<T>(this SQLiteDataReader reader, string columnName)
        {
            var colNumber = reader.GetOrdinal(columnName);
            return reader.GetFieldValue<T>(colNumber);
        }
        public static Dictionary<string, object> GetValuesDictionary(this SQLiteDataReader reader)
        {
            var ret = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                ret.Add(reader.GetName(i), reader.GetValue(i));
            }

            return ret;
        }

    }
}
