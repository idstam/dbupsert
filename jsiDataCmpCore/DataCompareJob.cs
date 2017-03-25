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
            List<Table> result;
            if (SourceManager.HasSchema && DestinationManager.HasSchema)
            {
                result = srcTables.Where(source => destTables.Any(dest => dest.FullName == source.FullName)).ToList();
            }
            else
            {
                result = srcTables.Where(source => destTables.Any(dest => dest.TableName == source.TableName)).ToList();
            }

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
