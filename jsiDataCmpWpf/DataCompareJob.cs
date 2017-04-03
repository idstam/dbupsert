using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace jsiDataCmpCore
{
    public class DataCompareJob
    {
        public IDatabaseManager SourceManager { get; set; }
        public IDatabaseManager DestinationManager { get; set; }
        public List<TablePair> Tables { get; set; } = new List<TablePair>();


        public ObservableCollection<TablePair> SameTables()
        {
            var srcTables = SourceManager.GetTables();
            var destTables = DestinationManager.GetTables();

            var ret = new List<TablePair>();
            foreach (var srcTableName in srcTables.Keys)
            {
                Table destTable;
                if (SourceManager.HasSchema && DestinationManager.HasSchema)
                {
                    var srcTable = srcTables[srcTableName];
                    if (!destTables.ContainsKey(srcTableName))
                    {
                        continue;
                    }
                    destTable = destTables[srcTableName];

                    ret.Add(new TablePair
                    {
                        Title = srcTable.SchemaName + "." + srcTable.TableName,
                        Source = srcTable,
                        Destination = destTable,
                        Include = false
                    });
                }
                else
                {
                    var srcTable = srcTables[srcTableName];
                    destTable = destTables[srcTable.TableName];
                    if (destTable != null)
                    {
                        ret.Add(new TablePair
                        {
                            Title = srcTable.TableName,
                            Source = srcTable,
                            Destination = destTable,
                            Include = true
                        });
                    }
                }

            }


            return new ObservableCollection<TablePair>(ret.OrderBy(t => t.Title).ToList());
        }

        public void UpdateDestination(Action<string, double, double> updateStatus, Action<double, double> overallStatus)
        {
            for(int i = 0; i < Tables.Count; i++)
            {
                var tablePair = Tables[i];
                SourceManager.ReadSource(tablePair, DestinationManager, updateStatus);
                overallStatus(Tables.Count, i+1);
            }

        }
    }
}
