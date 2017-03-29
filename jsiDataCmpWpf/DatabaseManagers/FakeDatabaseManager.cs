using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using jsiDataCmpCore;

namespace jsiDataCmpWpf.DatabaseManagers
{
    public class FakeDatabaseManager:IDatabaseManager
    {
        public bool HasSchema { get; }
        public string Location { get; set; }
        public double RowCount { get; set; }
        public Dictionary<string, Table> Tables = new Dictionary<string, Table>();
        public List<Dictionary<string, object>> Values = new List<Dictionary<string, object>>();
        private int row = 0;
        public FakeDatabaseManager()
        {
            HasSchema = false;
        }
        public double GetRowCount(Table table)
        {
            return RowCount;
        }

        public Dictionary<string, Table> GetTables()
        {
            return Tables;
        }

        public void UpsertDestination(Table table, Dictionary<string, object> values)
        {
            Thread.Sleep(100);
            //throw new NotImplementedException();
        }

        public void ReadSource(TablePair tablePair, IDatabaseManager destManager, Action<string, double, double> updateStatus)
        {
            for (int i = 0; i < Values.Count; i++)
            {
                row = i;
                destManager.UpsertDestination(tablePair.Destination, Values[row]);
                updateStatus(tablePair.Title, Values.Count, row+1);
            }
        }
    }
}
