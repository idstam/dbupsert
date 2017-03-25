using System;
using System.Collections.Generic;
using System.Windows;
using jsiDataCmpWpf.SyncStatus;

namespace jsiDataCmpWpf
{
    /// <summary>
    /// Interaction logic for SyncStatus.xaml
    /// </summary>
    public partial class SyncStatusWindow : Window
    {
        private Dictionary<string, SyncProgressBar> _progressBars = new Dictionary<string, SyncProgressBar>();
        public SyncStatusWindow()
        {
            InitializeComponent();
        }

        public void UpdateStatus(string fullTableName, int totalRows, int currentRow)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                SyncProgressBar pb;
                if (_progressBars.ContainsKey(fullTableName))
                {
                    pb = _progressBars[fullTableName];
                }
                else
                {
                    pb = new SyncProgressBar {Title = fullTableName};
                    _progressBars.Add(fullTableName, pb);
                    ProgressPanel.Children.Add(pb);
                }

                pb.UpdateProgress((double) totalRows, (double) currentRow);
            }));

        }
    }
}
