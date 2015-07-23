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

namespace PervasiveDigital.Scratch.DeploymentHelper.Views
{
    /// <summary>
    /// Interaction logic for DeploymentLogWindow.xaml
    /// </summary>
    public partial class DeploymentLogWindow : Window
    {
        public DeploymentLogWindow()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            logText.Text = "";
        }

        public void WriteLine(string msg)
        {
            if (msg.EndsWith("\r\n"))
                msg += "\r\n";
            logText.AppendText(msg);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
