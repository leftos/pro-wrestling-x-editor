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
        private static string DocsPath = MainWindow.DocsPath;
        private static string FilesPath = MainWindow.FilesPath;

        public static void ExportAll(string filename, char[] header, int namesLength, string namesFile, string folderName)
        {
            byte[] array = new byte[header.Length];
            for (int i = 0; i < header.Length; i++)
            {
                array[i] = (byte) header[i];
            }
            ExportAll(filename, array, namesLength, namesFile, folderName);
        }

        private static List<string> GrabNames(string filename, int namesLength, string namesFile)
        {
            string directory = filename.Replace(Path.GetFileName(filename), "");
            string pathToFile = namesFile;

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

        public static void ImportAll(string pathToFiles, string datFile, string namesFileOutput, string namesFileInput, int namesLength)
        {
            if (!Directory.Exists(pathToFiles))
            {
                MessageBox.Show("You beautiful you, that folder doesn't exist!");
                return;
            }

            List<string> filesToImport = Directory.GetFiles(pathToFiles).ToList();
            List<string> filesToImport_Names = new List<string>();
            filesToImport.ForEach(f => filesToImport_Names.Add(Path.GetFileName(f)));
            
            List<string> names = GrabNames(namesFileInput, namesLength, namesFileInput);

            List<byte> data = new List<byte>();
            List<int> offsets = new List<int>();
            offsets.Add(0);
            List<int> lengths = new List<int>();

            for (int i = 0; i < names.Count; i++)
            {
                var name = names[i];
                if (filesToImport_Names.Contains(name))
                {
                    data.AddRange(File.ReadAllBytes(pathToFiles + @"\" + name).ToList());
                    lengths.Add(Convert.ToInt32(new FileInfo(pathToFiles + @"\" + name).Length));
                    offsets.Add(offsets[i] + lengths[i]);
                }
            }
            File.WriteAllBytes(datFile, data.ToArray());
            offsets.RemoveAt(offsets.Count - 1);

            List<string> hexOffsets = new List<string>();
            List<string> hexLengths = new List<string>();
            for (int i = 0; i < offsets.Count; i++)
            {
                hexOffsets.Add(String.Format("{0:X}", offsets[i]).PadLeft(8, '0'));
                hexLengths.Add(String.Format("{0:X}", lengths[i]).PadLeft(8, '0'));
            }

            BinaryWriter bw = new BinaryWriter(File.OpenWrite(namesFileOutput));
            for (int j = 0; j < names.Count; j++)
            {
                var name = names[j];
                bw.Write(name.ToCharArray());
                bw.Write((byte) 0);
                int countOfFEs = namesLength - name.Length - 9;
                for (int i = 0; i < countOfFEs; i++)
                {
                    bw.Write((byte) 254);
                }
                var hexOffset = ReverseByteOrder(hexOffsets[j]);
                for (int i = 0; i <= 6; i += 2)
                {
                    bw.Write(Convert.ToByte(hexOffset.Substring(i, 2), 16));
                }

                var hexLength = ReverseByteOrder(hexLengths[j]);
                for (int i = 0; i <= 6; i += 2)
                {
                    bw.Write(Convert.ToByte(hexLength.Substring(i, 2), 16));
                }
            }
            bw.Close();

            MessageBox.Show("The files are packed.");
        }

        private static string ReverseByteOrder(string hexString)
        {
            string b1 = hexString.Substring(0, 2);
            string b2 = hexString.Substring(2, 2);
            string b3 = hexString.Substring(4, 2);
            string b4 = hexString.Substring(6, 2);
            string result = b4 + b3 + b2 + b1;
            return result;
        }

        public static void ExportAll(string filename, byte[] header, int namesLength, string namesFile, string folderName)
        {
            List<string> names = GrabNames(filename, namesLength, filename.Replace(Path.GetFileName(filename), "") + @"\" + namesFile);
            var folder = FilesPath + @"\" + folderName;
            if (!Directory.Exists(folder))
            {
                if (!Directory.Exists(FilesPath))
                {
                    Directory.CreateDirectory(FilesPath);
                }
                Directory.CreateDirectory(folder);
            }
            else
            {
                Directory.GetFiles(folder).ToList().ForEach(File.Delete);
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
                                     BinaryWriter bw = new BinaryWriter(File.OpenWrite(folder + @"\" + names[i]));
                                     bw.Write(data);
                                     bw.Close();
                                 }

                                 br.Close();

                                 MessageBox.Show("The models are in your My Documents folder.");
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
        private static string DocsPath = MainWindow.DocsPath;
        private static string FilesPath = MainWindow.FilesPath;

        public static void ExportAll(string filename, string folderName)
        {
            var folder = FilesPath + @"\" + folderName;
            if (!Directory.Exists(folder))
            {
                if (!Directory.Exists(FilesPath))
                {
                    Directory.CreateDirectory(FilesPath);
                }
                Directory.CreateDirectory(folder);
            }
            else
            {
                Directory.GetFiles(folder).ToList().ForEach(File.Delete);
            }

            char[] header = new char[] {'R', 'I', 'F', 'F'};
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
                                     BinaryWriter bw = new BinaryWriter(File.OpenWrite(folder + @"\" + i.ToString().PadLeft(8, '0') + ".wav"));
                                     bw.Write(data);
                                     bw.Close();
                                 }
                                 br.Close();

                                 MessageBox.Show("The models are in your My Documents folder.");
                             };

            worker.ProgressChanged += GraphicsImportExport.OnProgressChanged;

            worker.RunWorkerAsync();
        }

        public static void ImportAll(string pathToFiles, string datFile)
        {
            if (!Directory.Exists(pathToFiles))
            {
                MessageBox.Show("You beautiful you, that folder doesn't exist!");
                return;
            }

            List<string> filesToImport = Directory.GetFiles(pathToFiles).ToList();
            filesToImport.Sort();

            List<byte> data = new List<byte>();
            for (int i = 0; i < filesToImport.Count; i++)
            {
                data.AddRange(File.ReadAllBytes(filesToImport[i]).ToList());
            }
            File.WriteAllBytes(datFile, data.ToArray());

            MessageBox.Show("The files are packed.");
        }
    }
}