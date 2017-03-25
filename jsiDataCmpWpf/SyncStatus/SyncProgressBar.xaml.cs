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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
