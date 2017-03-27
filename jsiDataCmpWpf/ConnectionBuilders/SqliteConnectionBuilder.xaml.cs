using System.Windows;
using jsiDataCmpCore;
using Microsoft.Win32;

namespace jsiDataCmpWpf.ConnectionBuilders
{
    /// <summary>
    /// Interaction logic for SqliteConnectionBuilder.xaml
    /// </summary>
    public partial class SqliteConnectionBuilder : Window, IConnectionBuilder
    {
        public IDatabaseManager Manager { get; set; }
        public SqliteConnectionBuilder(string databaseRole)
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Manager = new SqliteManager($"Data Source={FileNameText.Text};Version=3;", FileNameText.Text);
            Close();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog(this);
            FileNameText.Text = dialog.FileName;
        }
    }
}
