﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            var cn = new SqlServerConnectionBuilder();
            cn.ShowDialog();
            _job.SourceConnectionString = cn.ConnectionString;
        }

        private void DestinationConnection_Click(object sender, RoutedEventArgs e)
        {
            var cn = new SqlServerConnectionBuilder();
            cn.ShowDialog();
            _job.DestinationConnectionString = cn.ConnectionString;
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
            _job.UpdateDestination();
        }
    }
}