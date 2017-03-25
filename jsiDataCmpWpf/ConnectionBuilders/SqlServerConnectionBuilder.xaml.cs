using System;
using System.Windows;
using jsiDataCmpCore;

namespace jsiDataCmpWpf.ConnectionBuilders
{
    /// <summary>
    /// Interaction logic for SqlServerConnectionBuilder.xaml
    /// </summary>
    public partial class SqlServerConnectionBuilder : Window, IConnectionBuilder
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string Server { get; set; }

        public IDatabaseManager Manager { get; set; }

        public SqlServerConnectionBuilder(string title)
        {
            InitializeComponent();
            this.Title = title;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SetConnectionString();
            Database = DatabasesCombo.Text;
            Server = ServerNameText.Text;

            Manager = new SqlServerManager(ConnectionString, Server, Database);
            this.Close();
        }

        private void SetConnectionString()
        {
            if (WindowsAuth.IsChecked.HasValue && WindowsAuth.IsChecked.Value)
            {
                ConnectionString = $"Server={ServerNameText.Text};Database={DatabasesCombo.Text};Trusted_Connection=True;";
            }

            if (SqlServerAuth.IsChecked.HasValue && SqlServerAuth.IsChecked.Value)
            {
                ConnectionString =
                    $"Server={ServerNameText.Text};User Id={UserNameText.Text};Password ={PasswordText.Password};Database={DatabasesCombo.Text};";
            }
            //Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;
            //Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password = myPassword;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            SetConnectionString();
            
            try
            {
                var m = new SqlServerManager(ConnectionString);
                DatabasesCombo.ItemsSource = m.GetDatabases();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        
    }
}
