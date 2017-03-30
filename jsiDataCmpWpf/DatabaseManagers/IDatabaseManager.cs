using System;
using System.Collections.Generic;

namespace jsiDataCmpCore
{
    public interface IDatabaseManager
    {
        bool HasSchema { get; }
        string Location { get; set; }
        double GetRowCount(Table table);
        Dictionary<string, Table> GetTables();
        void UpsertDestination(Table table, Dictionary<string, object> values);
        void ReadSource(TablePair tablePair, IDatabaseManager destManager, Action<string, double, double> updateStatus);

        void PrepareDestination(TablePair tablePair);
        void FinalizeDestination();
    }
}