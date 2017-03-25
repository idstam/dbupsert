using System;
using System.Collections.Generic;

namespace jsiDataCmpCore
{
    public class SqliteManager: IDatabaseManager
    {
        public string Location { get; set; }

        private string CreateWhere(Table table, Dictionary<string, object> values)
        {
            throw new NotImplementedException();
        }

        public List<string> GetDatabases()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetRow(Table table, Dictionary<string, object> values)
        {
            throw new NotImplementedException();
        }

        public int GetRowCount(Table table)
        {
            throw new NotImplementedException();
        }

        public List<Table> GetTables()
        {
            throw new NotImplementedException();
        }

        public void Insert(Table table, Dictionary<string, object> values)
        {
            throw new NotImplementedException();
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
}
