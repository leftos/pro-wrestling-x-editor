using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace WrasslingImpExp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mwInstance;

        public MainWindow()
        {
            InitializeComponent();

            mwInstance = this;
        }

        private void btnExportModels_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Pick the models file, as in NOT PORN";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            GraphicsImportExport.ExportAll(ofd.FileName, new char[] { 'x', 'o', 'f', ' ', '0', '3', '0', '3' }, 72, "PWXMDP.dat");
        }

        private void btnExportTextures_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Pick the textures file";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            GraphicsImportExport.ExportAll(ofd.FileName, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 72, "PWXTX.dat");
        }

        private void btnExportSound_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Pick the sound file";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            SoundImportExport.ExportAll(ofd.FileName);
        }
    }
}
