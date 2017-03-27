using System.Collections.Generic;

namespace jsiDataCmpCore
{
    public class Table
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string FullName => SchemaName + "." + TableName;
        public string IdentityColumn { get; set; }
        public List<string> PrimaryKeyColumns { get; set; }
    }

    public class TablePair
    {
        public string Title { get; set; }
        public Table Source { get; set; }
        public Table Destination { get; set; }
        public bool Include { get; set; }
    }
}
