using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEIKScreen.Common
{
    public class ZipTools
    {
        public static readonly ZipTools Instance = new ZipTools();
        /// <summary>
        /// 打包压缩文件夹
        /// </summary>
        /// <param name="SourceFolderPath"></param>
        /// <param name="zipFilePath"></param>
        /// <param name="deviceType"></param>
        public void ZipFolder(string SourceFolderPath,string zipFilePath,int deviceType)
        {                                           
            Package package = Package.Open(zipFilePath, System.IO.FileMode.Create);
            ZipDirectory(SourceFolderPath, package, deviceType);
            package.Close();            
        }

        /// <summary>
        /// 解压缩zip文件到目标目录
        /// </summary>
        /// <param name="zipFilePath"></param>
        /// <param name="targetFolderPath"></param>
        public void UnzipToFolder(string zipFilePath,string targetFolderPath)
        {
            Package package = Package.Open(zipFilePath, System.IO.FileMode.Open);
            UnzipTo(package,targetFolderPath);
            package.Close();
        }

        private void ZipDirectory(string SourceFolderPath, Package package,int deviceType)
        {
            DirectoryInfo dir = new DirectoryInfo(SourceFolderPath);
            FileInfo[] files=dir.GetFiles();
            foreach (FileInfo fi in files)
            {                
                if ((".tdb".Equals(fi.Extension, StringComparison.OrdinalIgnoreCase) && deviceType == 1) || (".dat".Equals(fi.Extension, StringComparison.OrdinalIgnoreCase) && deviceType == 2)
                    || (".doc".Equals(fi.Extension, StringComparison.OrdinalIgnoreCase) && deviceType == 2) || ".crd".Equals(fi.Extension, StringComparison.OrdinalIgnoreCase) || deviceType == 3)
                {
                    string relativePath = fi.FullName.Replace(SourceFolderPath, string.Empty);
                    relativePath = relativePath.Replace("\\", "/");
                    Uri partUriDocument = PackUriHelper.CreatePartUri(new Uri(relativePath, UriKind.Relative));

                    //resourcePath="Resource\Image.jpg"
                    //Uri partUriResource = PackUriHelper.CreatePartUri(new Uri(resourcePath, UriKind.Relative));

                    PackagePart part = package.CreatePart(partUriDocument, System.Net.Mime.MediaTypeNames.Application.Zip);
                    using (FileStream fs = fi.OpenRead())
                    {
                        using (Stream partStream = part.GetStream())
                        {
                            CopyStream(fs, partStream);
                            fs.Close();
                            partStream.Close();
                        }
                    }
                }                

            }
            DirectoryInfo[] directories = dir.GetDirectories();
            foreach (DirectoryInfo subDi in directories)
            {
                ZipDirectory(SourceFolderPath, package, deviceType);
            }
        }

        private void UnzipTo(Package package, string outPath)
        {
            var packageParts=package.GetParts();
            foreach (PackagePart part in packageParts)
            {
                string outFileName = Path.Combine(outPath, part.Uri.OriginalString.Substring(1));
                using (FileStream outFileStream = new FileStream(outFileName, FileMode.Create))
                {
                    using (Stream inFileStream = part.GetStream())
                    {
                        CopyStream(inFileStream, outFileStream);
                    }
                }
            }
        }

        private void CopyStream(Stream source, Stream target)
        {
            const int bufSize = 0x1000;
            byte[] buf = new byte[bufSize];
            int bytesRead = 0;
            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
            {
                target.Write(buf, 0, bytesRead);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SourceFolderPath"></param>
        /// <param name="zipFile"></param>
        /// <param name="deviceType"></param>
        public void ZipFiles(string SourceFolderPath, string zipFile, int deviceType)
        {
            string fileFilter = "";
            if (deviceType == 1)
            {
                fileFilter = @".+(\.crd)|(\.ini)|(\.tdb)|(\.pdf)|(\.xml)|(\.jpg)|(\.csv)";
            }
            if (deviceType == 2)
            {
                fileFilter = @".+(\.pdf)|(\.dat)|(\.xml)";
            }
            (new FastZip()).CreateZip(zipFile, SourceFolderPath, true, fileFilter);
            //using (ZipFile zip = ZipFile.Create(zipFile))
            //{
            //    DirectoryInfo dir = new DirectoryInfo(SourceFolderPath);
            //    FileInfo[] files = dir.GetFiles();
            //    foreach (FileInfo fi in files)
            //    {
            //        zip.BeginUpdate();
            //        if ((".tdb".Equals(fi.Extension, StringComparison.OrdinalIgnoreCase) && deviceType == 1) || (".dat".Equals(fi.Extension, StringComparison.OrdinalIgnoreCase) && deviceType == 2)
            //        || (".doc".Equals(fi.Extension, StringComparison.OrdinalIgnoreCase) && deviceType == 2) || ".crd".Equals(fi.Extension, StringComparison.OrdinalIgnoreCase) || deviceType == 3)
            //        {
            //            zip.Add(fi.FullName,fi.Name); 
            //        }
            //    }                
            //    zip.CommitUpdate();
            //}
        }

        public void UnZip(string zipFile, string targetFolderPath)
        {
            (new FastZip()).ExtractZip(zipFile, targetFolderPath, "");
            //using (ZipFile zip = new ZipFile(zipFile))
            //{
            //    foreach (ZipEntry z in zip)
            //    {
            //        Console.WriteLine(z);
            //    }
            //    ZipEntry z1 = zip[0];
            //    Console.WriteLine(z1.Name);
            //}
        }

        
    }
}
