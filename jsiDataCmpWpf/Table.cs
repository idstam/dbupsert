using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

    public class TablePair: INotifyPropertyChanged
    {
        public string Title { get; set; }
        public Table Source { get; set; }
        public Table Destination { get; set; }

        private bool _include;
        public bool Include
        {
            get { return _include; }
            set
            {
                if (value == _include)
                    return;

                _include = value;
                this.OnPropertyChanged("Include");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
