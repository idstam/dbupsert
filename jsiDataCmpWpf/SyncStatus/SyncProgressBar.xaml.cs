using System.Windows.Controls;

namespace jsiDataCmpWpf.SyncStatus
{
    /// <summary>
    /// Interaction logic for SyncProgressBar.xaml
    /// </summary>
    public partial class SyncProgressBar : UserControl
    {
        public SyncProgressBar()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return CurrentTableText.Text; } 
            set { CurrentTableText.Text = value; }
        }

        public void UpdateProgress(double max, double value)
        {
            Progress.Maximum = max;
            Progress.Value = value;
        }

    }
}
