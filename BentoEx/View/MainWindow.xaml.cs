using BentoEx.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace BentoEx.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => { ((MainViewModel)DataContext).OnLoaded(); };
        }

        private void hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private async void hyperlink_TurnOffVpn(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            string batchFile = AppDomain.CurrentDomain.BaseDirectory + @"TurnOffVpn.bat";
            if (File.Exists(batchFile))
            {
                Process p = Process.Start(batchFile);
                p.WaitForExit();
                await Task.Delay(2000);

                ((MainViewModel)DataContext).OnLoaded();
            }

            e.Handled = true;
        }

        private void hyperlink_ShowAbout(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            //TODO https://archive.codeplex.com/?p=aboutbox
            MessageBox.Show("BentoEx  built at ", "About BentoEx");

            e.Handled = true;
        }
    }
}
