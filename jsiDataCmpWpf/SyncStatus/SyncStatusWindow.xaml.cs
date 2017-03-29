using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using jsiDataCmpCore;
using jsiDataCmpWpf.SyncStatus;

namespace jsiDataCmpWpf
{
    /// <summary>
    /// Interaction logic for SyncStatus.xaml
    /// </summary>
    public partial class SyncStatusWindow : Window
    {
        private Dictionary<string, SyncProgressBar> _progressBars = new Dictionary<string, SyncProgressBar>();
        public  ObservableCollection<SyncProgressBar> ProgressBarsGui = new ObservableCollection<SyncProgressBar>();
        public SyncStatusWindow(List<TablePair> jobTables)
        {
            InitializeComponent();
            OverallProgress.Title = "Overall progress";
        }

        public void UpdateStatus(string fullTableName, double totalRows, double currentRow)
        {

            Application.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Normal,
              new Action(() => {
                  TableProgress.Title = fullTableName;
                  TableProgress.UpdateProgress(totalRows, currentRow);

              }));

   
        }
        public void UpdateOverall(double totalTables, double currentTable)
        {

            Application.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Normal,
              new Action(() => {
                  OverallProgress.UpdateProgress(totalTables, currentTable);

              }));


        }
    }
}
