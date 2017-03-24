using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace jsiDataCmpWpf
{
    /// <summary>
    /// Interaction logic for SyncStatus.xaml
    /// </summary>
    public partial class SyncStatus : Window
    {
        public SyncStatus()
        {
            InitializeComponent();
        }

        public void UpdateStatus(string fullTableName, int totalRows, int currentRow)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                Title = fullTableName;
                Progress.Maximum = (double)totalRows;
                Progress.Value = (double)currentRow;
            }));

        }
    }
}
