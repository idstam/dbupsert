using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using jsiDataCmpCore;
using jsiDataCmpWpf.ConnectionBuilders;
using Table = jsiDataCmpCore.Table;

namespace jsiDataCmpWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataCompareJob _job = new DataCompareJob();
        public ObservableCollection<jsiDataCmpCore.Table> IncludedTables;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SourceConnection_Click(object sender, RoutedEventArgs e)
        {
            IConnectionBuilder cn = new SqlServerConnectionBuilder("Source database");
            cn.ShowDialog();
            _job.SourceManager = cn.Manager;
            ToLabel.Text = "To " + _job.SourceManager.Location;
        }

        private void DestinationConnection_Click(object sender, RoutedEventArgs e)
        {
            IConnectionBuilder cn = new SqlServerConnectionBuilder("Destination database");
            cn.ShowDialog();
            _job.DestinationManager = cn.Manager;
            ToLabel.Text = "To " + _job.DestinationManager.Location;
        }

        private void FetchTables_Click(object sender, RoutedEventArgs e)
        {
            IncludedTables = new ObservableCollection<Table>(_job.SameTables());
            TablesList.ItemsSource = IncludedTables;
            DataContext = this;

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _job.Tables = IncludedTables.Where(t => t.Include).ToList();
            var status = new SyncStatusWindow();
            status.Show();

            Task.Factory.StartNew(() =>
                {
                    _job.UpdateDestination(status.UpdateStatus);
                }
            );
        }
    }
}
