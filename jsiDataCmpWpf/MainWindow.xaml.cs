using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using jsiDataCmpCore;
using jsiDataCmpWpf.ConnectionBuilders;

namespace jsiDataCmpWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataCompareJob _job = new DataCompareJob();
        public ObservableCollection<jsiDataCmpCore.TablePair> IncludedTables;

        public MainWindow()
        {
            InitializeComponent();
            
            
        }

        private void FetchTables_Click(object sender, RoutedEventArgs e)
        {
            IncludedTables = new ObservableCollection<TablePair>(_job.SameTables());
            TablesList.ItemsSource = IncludedTables;
            DataContext = this;

        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (IncludedTables == null || IncludedTables.Count == 0) return;

            _job.Tables = IncludedTables.Where(t => t.Include).ToList();
            var status = new SyncStatusWindow();
            status.Show();

            _job.UpdateDestination(status.UpdateStatus);
            status.Close();
        }

        private void SourceCombo_DropDownClosed(object sender, System.EventArgs e)
        {
            _job.SourceManager = GetDatabaseManager("Source database", SourceCombo);
            if (_job.SourceManager == null) return;

            FromLabel.Text = "From " + _job.SourceManager.Location;
        }
        private void DestCombo_DropDownClosed(object sender, System.EventArgs e)
        {
            _job.DestinationManager= GetDatabaseManager("Destination database", DestCombo);
            if (_job.DestinationManager == null) return;

            ToLabel.Text = "To " + _job.DestinationManager.Location;
        }

        private IDatabaseManager GetDatabaseManager(string databaseRole, ComboBox combo)
        {
            IConnectionBuilder cn;
            switch (combo.SelectedIndex)
            {
                case 1:
                    cn = new SqlServerConnectionBuilder(databaseRole);
                    break;
                case 2:
                    cn = new SqliteConnectionBuilder(databaseRole);
                    break;
                default:
                    return null;
            }
            cn.ShowDialog();
            return cn.Manager;
        }

    }
}
