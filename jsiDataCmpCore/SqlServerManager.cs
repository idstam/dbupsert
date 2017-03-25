using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;

namespace jsiDataCmpCore
{
    public class SqlServerManager : IDatabaseManager
    {
        private readonly string _conString;
        public readonly string Server;
        public readonly string Database;
        public string Location { get; set; }
        public bool HasSchema => true;
        public SqlServerManager(string connectionString, string server = "", string database = "")
        {
            _conString = connectionString;
            Location = database + " in " + server;
        }
        
        public List<Table> GetTables()
        {
            var ret = new List<Table>();
            var sql = "select * from INFORMATION_SCHEMA.TABLES";
            using (var cn = new SqlConnection(_conString))
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new Table
                        {
                            SchemaName = reader.GetValue<string>("TABLE_SCHEMA"),
                            TableName = reader.GetValue<string>("TABLE_NAME")
                        });
                    }
                }
            }

            foreach (var t in ret)
            {
                t.PrimaryKeyColumns = GetPrimaryKeyColumns(t);
                t.IdentityColumn = GetIdentityColumn(t);
                if (t.PrimaryKeyColumns == null && !string.IsNullOrEmpty(t.IdentityColumn))
                {
                    t.PrimaryKeyColumns = new List<string> {t.IdentityColumn};
                }
            }
            return ret;
        }

        public List<string> GetDatabases()
        {
            var ret = new List<string>();
            var sql = "select * from sys.databases";
            using (var cn = new SqlConnection(_conString))
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(reader.GetValue<string>("name"));
                    }
                }
            }
            return ret;
        }

        private List<string> GetPrimaryKeyColumns(Table table)
        {
            var sql = @"
select COLUMN_NAME 
from INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE u
inner
join INFORMATION_SCHEMA.TABLE_CONSTRAINTS c on u.TABLE_SCHEMA = c.TABLE_SCHEMA and u.TABLE_NAME = c.TABLE_NAME and u.CONSTRAINT_NAME = c.CONSTRAINT_NAME
where u.TABLE_SCHEMA=@schema AND u.TABLE_NAME=@name
";

            var ret = new List<string>();
            using (var cn = new SqlConnection(_conString))
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    cmd.Parameters.AddWithValue("@schema", table.SchemaName);
                    cmd.Parameters.AddWithValue("@name", table.TableName);

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(reader.GetValue<string>("COLUMN_NAME"));
                    }
                }
            }
            return ret;

        }

        private string GetIdentityColumn(Table table)
        {

            string sql = @"
select top 1 c.name from sys.columns c
inner join sys.tables t on t.object_id = c.object_id
inner join sys.schemas s on t.schema_id = s.schema_id
 where t.type = 'U'
 and c.is_identity = 1
 and t.name=@table
 and s.name=@schema";
            using (var cn = new SqlConnection(_conString))
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    cmd.Parameters.AddWithValue("@table", table.TableName);
                    cmd.Parameters.AddWithValue("@schema", table.SchemaName);
                    var value = cmd.ExecuteScalar();
                    return value?.ToString();
                }
            }
            
        }

        public int GetRowCount(Table table)
        {
            string sql = $"select count(*) from {table.SchemaName}.{table.TableName}";
            using (var cn = new SqlConnection(_conString))
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    cmd.Parameters.AddWithValue("@table", table.TableName);
                    cmd.Parameters.AddWithValue("@schema", table.SchemaName);
                    var value = cmd.ExecuteScalar();
                    return (int)value;
                }
            }
        }

        public void ReadSource(Table table, IDatabaseManager destManager, Action<string, int, int> updateStatus)
        {
            var sql = $"select * from {table.SchemaName}.{table.TableName}";
            int maxCount = GetRowCount(table);
            int rowCount= 0;

            using (var cn = new SqlConnection(_conString))
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {

                        var values = reader.GetValues();
                        var destValues = destManager.GetRow(table, values);
                        if (destValues == null)
                        {
                            destManager.Insert(table, values);
                        }
                        else
                        {
                            destManager.UpdateDestination(table, values);
                        }
                        rowCount++;
                        updateStatus(table.FullName, maxCount, rowCount);
                    }
                }
            }
        }

        public void Insert(Table table, Dictionary<string, object> values)
        {
            var identityInsert = $"set identity_insert {table.SchemaName}.{table.TableName} ";
            var columnNames = string.Join(",", values.Keys);
            var valueNames = "@" + columnNames.Replace(",", ",@");
            var reseed = $"declare @seed bigint = (select max(id) from {table.SchemaName}.{table.TableName}); DBCC CHECKIDENT ('{table.SchemaName}.{table.TableName}', RESEED, @seed); ";

            var insert = $"INSERT INTO {table.SchemaName}.{table.TableName} ({columnNames}) VALUES ({valueNames});";
            using (var cn = new SqlConnection(_conString))
            {
                string sql=insert;
                if (!string.IsNullOrWhiteSpace(table.IdentityColumn))
                {
                    sql = identityInsert + "ON;" + insert + identityInsert + "OFF;" + reseed;
                }
                cn.Open();
                using (var cmd = new SqlCommand(sql, cn))
                {
                    foreach (var column in values.Keys)
                    {
                        cmd.Parameters.AddWithValue(column, values[column]);
                    }
                    cmd.ExecuteNonQuery();
                }
            }


        }
        
        public Dictionary<string, object> GetRow(Table table, Dictionary<string, object> values)
        {
            //If there is no way of finding a unique row don't even try.
            if (table.PrimaryKeyColumns == null || table.PrimaryKeyColumns.Count == 0) return null;

            var sql = $"select * from {table.SchemaName}.{table.TableName}" + CreateWhere(table, values);
            using (var cn = new SqlConnection(_conString))
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    foreach (var primaryKey in table.PrimaryKeyColumns)
                    {
                        cmd.Parameters.AddWithValue(primaryKey, values[primaryKey]);
                    }
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        return reader.GetValues();
                    }
                }
            }
            return null;
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
        public void UpdateDestination(Table table, Dictionary<string, object> values)
        {
            
        }

    }

    public static class SqlServerExtensions
    {
        public static T GetValue<T>(this SqlDataReader reader, string columnName)
        {
            var colNumber = reader.GetOrdinal(columnName);
            return reader.GetFieldValue<T>(colNumber);
        }
        public static Dictionary<string, object> GetValues(this SqlDataReader reader)
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

