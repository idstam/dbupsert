using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
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
            var currentCursor = Cursor;
            try
            {
                Cursor = Cursors.Wait;
                var m = new SqlServerManager(ConnectionString);
                DatabasesCombo.ItemsSource = m.GetDatabases();
                DatabasesCombo.IsEnabled = true;
                ConnectCheckMark.Visibility = Visibility.Visible;
               
            }
            catch (Exception exception)
            {
                ConnectCheckMark.Visibility = Visibility.Collapsed;
                DatabasesCombo.ItemsSource = new List<string>();
                DatabasesCombo.IsEnabled = false;
                MessageBox.Show(this, exception.Message, "Connection failure", MessageBoxButton.OK, MessageBoxImage.Error);
                
            }
            finally
            {
                Cursor = currentCursor;
            }
        }

        private void DatabasesCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DatabasesCombo.SelectedIndex == -1)
            {
                DatabaseCheckMark.Visibility = Visibility.Collapsed;
            }
            else
            {
                DatabaseCheckMark.Visibility = Visibility.Visible;
            }

        }

        private void SqlServerAuth_Checked(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(UserNameText);
        }
    }
}
