using System.Collections.Generic;

namespace jsiDataCmpCore
{
    public class Table
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string FullName => SchemaName + "." + TableName;
        public bool Include { get; set; }
        public string IdentityColumn { get; set; }
        public List<string> PrimaryKeyColumns { get; set; }
    }
}
