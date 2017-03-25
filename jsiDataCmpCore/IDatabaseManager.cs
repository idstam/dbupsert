using System;
using System.Collections.Generic;

namespace jsiDataCmpCore
{
    public interface IDatabaseManager
    {
        bool HasSchema { get; }
        string Location { get; set; }
        Dictionary<string, object> GetRow(Table table, Dictionary<string, object> values);
        int GetRowCount(Table table);
        List<Table> GetTables();
        void Insert(Table table, Dictionary<string, object> values);
        void ReadSource(Table table, IDatabaseManager destManager, Action<string, int, int> updateStatus);
        void UpdateDestination(Table table, Dictionary<string, object> values);
    }
}