using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace jsiDataCmpCore
{
    public class DataCompareJob
    {
        public IDatabaseManager SourceManager { get; set; }
        public IDatabaseManager DestinationManager { get; set; }
        public List<Table>Tables { get; set; } = new List<Table>();

       
        public ObservableCollection<Table> SameTables()
        {
            var srcTables = SourceManager.GetTables();
            var destTables = DestinationManager.GetTables();

            var result = srcTables.Where(source => destTables.Any(dest => dest.FullName == source.FullName)).ToList();

            return new ObservableCollection<Table>(result);
        }

        public void UpdateDestination(Action<string, int, int> updateStatus)
        {
            foreach (var table in Tables)
            {
                SourceManager.ReadSource(table, DestinationManager, updateStatus);
            }
        }
    }
}
