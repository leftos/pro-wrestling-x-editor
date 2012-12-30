using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;

namespace WrasslingImpExp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mwInstance;

        public static string DocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string FilesPath = DocsPath + @"\Pro Wrestling X Editor\";

        public MainWindow()
        {
            InitializeComponent();

            mwInstance = this;
        }

        private void btnExportModels_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Pick the models file";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            GraphicsImportExport.ExportAll(ofd.FileName, new char[] { 'x', 'o', 'f', ' ', '0', '3', '0', '3' }, 72, "PWXMDP.dat", "Models");
        }

        private void btnExportTextures_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Pick the textures file";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            GraphicsImportExport.ExportAll(ofd.FileName, new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 72, "PWXTX.dat", "Textures");
        }

        private void btnExportSound_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Pick the sound file";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            SoundImportExport.ExportAll(ofd.FileName, "Sounds");
        }

        private void btnImportModels_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Pick one of the models you want to import";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            var path = ofd.FileName.Replace(Path.GetFileName(ofd.FileName), "");

            ofd = new OpenFileDialog();
            ofd.Title = "Pick the parameters file";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            var namesFileInput = ofd.FileName;

            var datFile = FilesPath + @"\PWXMD.dat";
            var namesFileOutput = FilesPath + @"\PWXMDP.dat";
            /*
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Give me a fucking name, talk MOTHERFUCKER";
            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName))
                return;

            var datFile = sfd.FileName;

            sfd = new SaveFileDialog();
            sfd.Title = "Give me the name of the parameters file, or I'm killing your wife";
            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName))
                return;

            var namesFile = sfd.FileName;
            */

            GraphicsImportExport.ImportAll(path, datFile, namesFileOutput, namesFileInput, 72);
        }

        private void btnImportTextues_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Pick one of the textures you want to import";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            var path = ofd.FileName.Replace(Path.GetFileName(ofd.FileName), "");

            ofd = new OpenFileDialog();
            ofd.Title = "Pick the parameters file";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            var namesFileInput = ofd.FileName;

            var datFile = FilesPath + @"\PWXLEVTEX.dat";
            var namesFileOutput = FilesPath + @"\PWXTX.dat";
            /*
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Give me a fucking name, talk MOTHERFUCKER";
            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName))
                return;

            var datFile = sfd.FileName;

            sfd = new SaveFileDialog();
            sfd.Title = "Give me the name of the parameters file, or I'm killing your wife";
            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName))
                return;

            var namesFile = sfd.FileName;
            */

            GraphicsImportExport.ImportAll(path, datFile, namesFileOutput, namesFileInput, 72);
        }

        private void btnImportSounds_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Pick one of the sounds you want to import";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            var path = ofd.FileName.Replace(Path.GetFileName(ofd.FileName), "");
            var datFile = FilesPath + @"\PWXSD.dat";

            SoundImportExport.ImportAll(path, datFile);
        }
    }
}
