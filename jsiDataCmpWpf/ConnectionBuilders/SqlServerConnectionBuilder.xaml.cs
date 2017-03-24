using System;
using System.Collections.Generic;
using System.Linq;
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
using jsiDataCmpCore;

namespace jsiDataCmpWpf.ConnectionBuilders
{
    /// <summary>
    /// Interaction logic for SqlServerConnectionBuilder.xaml
    /// </summary>
    public partial class SqlServerConnectionBuilder : Window
    {
        public string ConnectionString { get; set; }

        public SqlServerConnectionBuilder()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SetConnectionString();

            this.Close();
        }

        private void SetConnectionString()
        {
            if (WindowsAuth.IsChecked.HasValue && WindowsAuth.IsChecked.Value)
            {
                ConnectionString = $"Server={ServerNameText.Text};Database={DatabasesComboo.Text};Trusted_Connection=True;";
            }

            if (SqlServerAuth.IsChecked.HasValue && SqlServerAuth.IsChecked.Value)
            {
                ConnectionString =
                    $"Server={ServerNameText.Text};User Id={UserNameText.Text};Password ={PasswordText.Password};Database={DatabasesComboo.Text};";
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
                DatabasesComboo.ItemsSource = m.GetDatabases();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}
