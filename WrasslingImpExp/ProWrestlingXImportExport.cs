using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace WrasslingImpExp
{
    internal static class GraphicsImportExport
    {
        private static string DocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string FilesPath = DocsPath + @"\ProWrestlingXModels";

        public static void ExportAll(string filename, char[] header, int namesLength, string namesFile)
        {
            byte[] array = new byte[header.Length];
            for (int i = 0; i < header.Length; i++)
            {
                array[i] = (byte) header[i];
            }
            ExportAll(filename, array, namesLength, namesFile);
        }

        private static List<string> GrabNames(string filename, int namesLength, string namesFile)
        {
            string directory = filename.Replace(Path.GetFileName(filename), "");
            string pathToFile = directory + @"\" + namesFile;

            List<string> listOfNames = new List<string>();

            BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(pathToFile)));
            while (br.BaseStream.Length - br.BaseStream.Position >= namesLength)
            {
                string name = "";
                byte b;
                while ((b = br.ReadByte()) != 0)
                {
                    name += (char) b;
                }
                listOfNames.Add(name);
                br.BaseStream.Position -= (name.Length + 1);
                br.BaseStream.Position += namesLength;
            }

            br.Close();

            return listOfNames;
        }

        public static void ExportAll(string filename, byte[] header, int namesLength, string namesFile)
        {
            List<string> names = GrabNames(filename, namesLength, namesFile);
            if (!Directory.Exists(FilesPath))
            {
                Directory.CreateDirectory(FilesPath);
            }
            else
            {
                Directory.GetFiles(FilesPath).ToList().ForEach(File.Delete);
            }

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += delegate(object sender, DoWorkEventArgs args)
                             {
                                 /* Here's the trick that makes everything faster. In the line below, we had File.OpenRead(), which
                                  * makes the BinaryReader use the file in the hard-drive. Now, hard-drive access is much slower than
                                  * RAM access, so why not load the whole file into the RAM, so that we can use the GB/sec of bandwidth
                                  * using RAM offers? That's where MemoryStream comes into use. All I'm doing is reading the whole file
                                  * into RAM, and then giving it to BinaryReader. We work with the file the same way, just much faster. */
                                 BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(filename)), Encoding.ASCII);
                                 List<long> headerOffsets = new List<long>();
                                 while (br.BaseStream.Length - br.BaseStream.Position >= header.Length)
                                 {
                                     if (br.ReadBytes(header.Length).SequenceEqual(header))
                                     {
                                         headerOffsets.Add(br.BaseStream.Position - header.Length);
                                         worker.ReportProgress(Convert.ToInt32((double) 100*br.BaseStream.Position/br.BaseStream.Length));
                                     }
                                     else
                                     {
                                         br.BaseStream.Position -= (header.Length - 1);
                                             // This should be dynamic by header.Length, not hard-coded
                                     }
                                 }

                                 for (int i = 0; i < headerOffsets.Count; i++)
                                 {
                                     var headerOffset = headerOffsets[i];
                                     br.BaseStream.Position = headerOffset;
                                     byte[] data;
                                     if (i == headerOffsets.Count - 1)
                                     {
                                         data = br.ReadBytes(Convert.ToInt32(br.BaseStream.Length - headerOffsets[i]));
                                     }
                                     else
                                     {
                                         data = br.ReadBytes(Convert.ToInt32(headerOffsets[i + 1] - headerOffsets[i]));
                                     }
                                     BinaryWriter bw = new BinaryWriter(File.OpenWrite(FilesPath + @"\" + names[i]));
                                     bw.Write(data);
                                     bw.Close();
                                 }

                                 br.Close();

                                 MessageBox.Show("Your tits are nice, and the models are in your My Documents folder.");
                             };

            worker.ProgressChanged += OnProgressChanged;

            worker.RunWorkerAsync();
        }

        public static void OnProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            if (args.ProgressPercentage < 100)
            {
                MainWindow.mwInstance.pb.Value = args.ProgressPercentage;
            }
            else
            {
                MainWindow.mwInstance.pb.Value = 0;
            }
        }
    }

    internal static class SoundImportExport
    {
        private static string DocsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string FilesPath = DocsPath + @"\ProWrestlingXModels";

        public static void ExportAll(string filename)
        {
            if (!Directory.Exists(FilesPath))
            {
                Directory.CreateDirectory(FilesPath);
            }
            char[] header = new char[] {'r', 'i', 'f', 'f'};
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += delegate(object sender, DoWorkEventArgs args)
                             {
                                 BinaryReader br = new BinaryReader(new MemoryStream(File.ReadAllBytes(filename)), Encoding.ASCII);
                                 List<long> headerOffsets = new List<long>();
                                 while (br.BaseStream.Length - br.BaseStream.Position >= header.Length)
                                 {
                                     if (br.ReadChars(header.Length).SequenceEqual(header))
                                     {
                                         headerOffsets.Add(br.BaseStream.Position - header.Length);
                                     }
                                     else
                                     {
                                         br.BaseStream.Position -= (header.Length - 1);
                                             // This should be dynamic by header.Length, not hard-coded
                                     }
                                 }
                                 for (int i = 0; i < headerOffsets.Count; i++)
                                 {
                                     var headerOffset = headerOffsets[i];
                                     br.BaseStream.Position = headerOffset;
                                     byte[] data;
                                     if (i == headerOffsets.Count - 1)
                                     {
                                         data = br.ReadBytes(Convert.ToInt32(br.BaseStream.Length - headerOffsets[i]));
                                     }
                                     else
                                     {
                                         data = br.ReadBytes(Convert.ToInt32(headerOffsets[i + 1] - headerOffsets[i]));
                                     }
                                     BinaryWriter bw = new BinaryWriter(File.OpenWrite(FilesPath + @"\" + i + ".wav"));
                                     bw.Write(data);
                                     bw.Close();
                                 }
                                 br.Close();

                                 MessageBox.Show("Your tits are nice, and the models are in your My Documents folder.");
                             };

            worker.ProgressChanged += GraphicsImportExport.OnProgressChanged;

            worker.RunWorkerAsync();
        }
    }
}