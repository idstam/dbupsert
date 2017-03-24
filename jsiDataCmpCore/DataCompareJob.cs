using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiDataCmpCore
{
    public class DataCompareJob
    {
        public string SourceConnectionString { get; set; }
        public string DestinationConnectionString { get; set; }
        public List<Table>Tables { get; set; } = new List<Table>();

        private SqlServerManager _sourceManager = null;
        private SqlServerManager _destManager = null;


        public SqlServerManager SourceManager
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SourceConnectionString))
                {
                    if (_sourceManager == null)
                    {
                        _sourceManager = new SqlServerManager(SourceConnectionString);
                    }
                    return _sourceManager;
                }
                else
                {
                    return null;
                }
            }
        }
        public SqlServerManager DestinationManager
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(DestinationConnectionString))
                {
                    if (_destManager == null)
                    {
                        _destManager = new SqlServerManager(DestinationConnectionString);
                    }
                    return _destManager;
                }
                else
                {
                    return null;
                }
            }
        }

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
