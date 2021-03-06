﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace jsiDataCmpCore
{
    public class SqlServerManager : IDatabaseManager
    {
        private readonly string _conString;
        public readonly string Server;
        public readonly string Database;
        public string Location { get; set; }
        public bool HasSchema => true;
        private Dictionary<string, string> _identityColumns = null;
        private Dictionary<string, List<string>> _primaryKeyColumns = null;
        public SqlServerManager(string connectionString, string server = "", string database = "")
        {
            _conString = connectionString;
            Location = database + " in " + server;
            _identityColumns = null;
            _primaryKeyColumns = null;
        }
        
        public Dictionary<string, Table> GetTables()
        {
            var ret = new Dictionary<string, Table>();
            var sql = "select * from INFORMATION_SCHEMA.TABLES";
            using (var cn = new SqlConnection(_conString))
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(reader.GetValue<string>("TABLE_SCHEMA") + "." + reader.GetValue<string>("TABLE_NAME")
                            ,new Table
                        {
                            SchemaName = reader.GetValue<string>("TABLE_SCHEMA"),
                            TableName = reader.GetValue<string>("TABLE_NAME")
                        });
                    }
                }
            }

            foreach (var t in ret.Values)
            {
                t.PrimaryKeyColumns = GetPrimaryKeyColumns(t);
                t.IdentityColumn = GetIdentityColumn(t);
                if (t.PrimaryKeyColumns == null)
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
            if (_primaryKeyColumns == null)
            {
                _primaryKeyColumns = new Dictionary<string, List<string>>();
                var sql = @"
select u.TABLE_SCHEMA, u.TABLE_NAME, COLUMN_NAME 
from INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE u
inner
join INFORMATION_SCHEMA.TABLE_CONSTRAINTS c on u.TABLE_SCHEMA = c.TABLE_SCHEMA and u.TABLE_NAME = c.TABLE_NAME and u.CONSTRAINT_NAME = c.CONSTRAINT_NAME
";

                
                using (var cn = new SqlConnection(_conString))
                {
                    using (var cmd = new SqlCommand(sql, cn))
                    {
                        cn.Open();
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var ret = new List<string>();
                            string fullName = reader.GetValue<string>("TABLE_SCHEMA") + "." +
                                              reader.GetValue<string>("TABLE_NAME");
                           
                            if(_primaryKeyColumns.ContainsKey(fullName))
                            {
                                ret = _primaryKeyColumns[fullName];
                            }
                            else
                            {
                                _primaryKeyColumns[fullName] = ret;
                            }

                            ret.Add(reader.GetValue<string>("COLUMN_NAME"));
                        }
                    }
                }
            }
            if (_primaryKeyColumns.ContainsKey(table.FullName))
            {
                return _primaryKeyColumns[table.FullName];
            }
            else
            {
                return new List<string>();
            }
        }


        private string GetIdentityColumn(Table table)
        {
            if (_identityColumns == null)
            {
                _identityColumns = new Dictionary<string, string>();

                string sql = @"
select s.name schemaName, t.name tableName, c.name columnName from sys.columns c
inner join sys.tables t on t.object_id = c.object_id
inner join sys.schemas s on t.schema_id = s.schema_id
 where t.type = 'U'
 and c.is_identity = 1
 --and t.name=@table
 --and s.name=@schema";
                using (var cn = new SqlConnection(_conString))
                {
                    using (var cmd = new SqlCommand(sql, cn))
                    {
                        cn.Open();
                        cmd.Parameters.AddWithValue("@table", table.TableName);
                        cmd.Parameters.AddWithValue("@schema", table.SchemaName);
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            _identityColumns.Add(reader.GetValue<string>("schemaName") + "." +  reader.GetValue<string>("tableName"), reader.GetValue<string>("columnName"));
                        }
                    }
                }
            }

            if (_identityColumns.ContainsKey(table.FullName))
            {
                return _identityColumns[table.FullName];
            }
            return null;
        }

        public double GetRowCount(Table table)
        {
            string sql = $"select count(*) from {table.SchemaName}.{table.TableName}";
            using (var cn = new SqlConnection(_conString))
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    var value = cmd.ExecuteScalar();
                    return double.Parse(value.ToString());
                }
            }
        }

        public void ReadSource(TablePair tablePair, IDatabaseManager destManager, Action<string, double, double> updateStatus)
        {
            File.AppendAllText(@"c:\temp\datacopy.log", tablePair.Title + " " + DateTime.Now.ToLongTimeString() + " ");

            destManager.PrepareDestination(tablePair);
            double maxCount = GetRowCount(tablePair.Source);
            File.AppendAllText(@"c:\temp\datacopy.log", Environment.NewLine + maxCount.ToString() + Environment.NewLine);


            int rowCount = 0;
            var sql = $"select * from {tablePair.Source.SchemaName}.{tablePair.Source.TableName}";

            using (var cn = new SqlConnection(_conString))
            {
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var values = reader.GetValues();
                        destManager.UpsertDestination(tablePair.Destination, values);
                        rowCount++;
                        updateStatus(tablePair.Title, maxCount, rowCount);
                    }
                }
                
            }
            destManager.FinalizeDestination();

        }

        public void FinalizeDestination()
        {
        }

        public void UpsertDestination(Table table, Dictionary<string, object> values)
        {
            var identityInsert = $"set identity_insert {table.SchemaName}.{table.TableName} ";
            var columnNames = string.Join(",", values.Keys.Select(k => "[" + k + "]"));

            var valueNames = "@" + string.Join(",", values.Keys).Replace(",", ",@");

            var reseed = $"declare @seed bigint = (select max(id) from {table.SchemaName}.{table.TableName}); DBCC CHECKIDENT ('{table.SchemaName}.{table.TableName}', RESEED, @seed); ";

            var insert = $"INSERT INTO {table.SchemaName}.{table.TableName} ({columnNames}) VALUES ({valueNames});";

            if (table.PrimaryKeyColumns == null || table.PrimaryKeyColumns.Count == 0)
            {
                table.PrimaryKeyColumns = values.Keys.ToList();
            }

            var checkIfUpdate = $"IF NOT EXISTS(SELECT * FROM {table.SchemaName}.{table.TableName} " + CreateWhere(table) + ") BEGIN ";
            var update = $" END ELSE BEGIN UPDATE {table.SchemaName}.{table.TableName} " +
                     GetUpdateColumns(table, values) +
                     CreateWhere(table) + "; END ";

            if (table.PrimaryKeyColumns.Count == values.Keys.Count)
            {
                update = " END ";
            }

            foreach (var column in values.Keys)
            {
                if (values[column].GetType().Name == "DBNull")
                {
                    insert = insert.Replace("@" + column + ",", "NULL,");
                    insert = insert.Replace("@" + column + " ", "NULL ");
                    insert = insert.Replace("@" + column + ")", "NULL)");
                    update = update.Replace("@" + column + ",", "NULL,");
                    update = update.Replace("@" + column + " ", "NULL ");
                    update = update.Replace("@" + column + ")", "NULL)");
                }
            }
            using (var cn = new SqlConnection(_conString))
            {
                string sql = checkIfUpdate + insert + update;
                if (!string.IsNullOrWhiteSpace(table.IdentityColumn))
                {
                    sql = identityInsert + "ON;" + sql + identityInsert + "OFF;" + reseed;
                }

                cn.Open();
                using (var cmd = new SqlCommand(sql, cn))
                {
                    foreach (var column in values.Keys)
                    {
                        if (values[column].GetType().Name != "DBNull")
                        {
                            cmd.Parameters.AddWithValue(column, values[column]);
                        }

                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        public Dictionary<string, object> GetRow(Table table, Dictionary<string, object> values)
        {
            //If there is no way of finding a unique row don't even try.
            if (table.PrimaryKeyColumns == null || table.PrimaryKeyColumns.Count == 0) return null;

            var sql = $"select * from {table.SchemaName}.{table.TableName}" + CreateWhere(table);
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

        private string CreateWhere(Table table)
        {
            var ret = " WHERE ";
            foreach (var colName in table.PrimaryKeyColumns)
            {
                ret += "[" + colName + "]=@" + colName + " AND ";
            }
            ret = ret.Remove(ret.Length - 5);
            ret += " ";
            return ret;
        }

        private string GetUpdateColumns(Table table,  Dictionary<string, object> values)
        {
            var ret = "";
            foreach (var column in values.Keys)
            {
                //if (!table.PrimaryKeyColumns.Contains(column) && table.IdentityColumn != column)
                if (table.IdentityColumn != column)
                {
                    ret += "[" + column + "]" + "=@" + column + ", ";
                }
            }
            ret = ret.Remove(ret.Length - 2);
            return " SET " + ret + " ";
        }

        public void PrepareDestination(TablePair tablePair)
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

