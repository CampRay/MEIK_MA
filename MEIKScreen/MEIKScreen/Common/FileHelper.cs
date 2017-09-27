using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace MEIKScreen.Common
{
    public class FileHelper
    {
        public static string GetXPSFromDialog(bool isSaved)
        {
            if (isSaved)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();

                saveFileDialog.Filter = "XPS Document files (*.xps)|*.xps|Txt Document files(*.txt)|*.txt";
                saveFileDialog.FilterIndex = 1;

                if (saveFileDialog.ShowDialog() == true)
                {
                    return saveFileDialog.FileName;
                }
                else
                {
                    return null;
                }
            }
            else return string.Format("{0}\\temp.xps", Environment.CurrentDirectory);//制造一个临时存储
        }

        public static string OpenXPSFileFromDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XPS Document Files(*.xps)|*.xps";

            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileName;
            else
                return null;
        }
        public static bool SaveXPS(FixedPage page, bool isSaved)
        {
            FixedDocument fixedDoc = new FixedDocument();//创建一个文档
            fixedDoc.DocumentPaginator.PageSize = new Size(96 * 8.5, 96 * 11);

            PageContent pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(page);
            fixedDoc.Pages.Add(pageContent);//将对象加入到当前文档中

            string containerName = GetXPSFromDialog(isSaved);
            if (containerName != null)
            {
                try
                {
                    File.Delete(containerName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }

                XpsDocument _xpsDocument = new XpsDocument(containerName, FileAccess.Write);

                XpsDocumentWriter xpsdw = XpsDocument.CreateXpsDocumentWriter(_xpsDocument);
                xpsdw.Write(fixedDoc);//写入XPS文件
                _xpsDocument.Close();
                return true;
            }
            else return false;
        }

        static XpsDocument xpsPackage = null;
        public static void LoadDocumentViewer(string xpsFileName, DocumentViewer viewer)
        {
            XpsDocument oldXpsPackage = xpsPackage;//保存原来的XPS包
            xpsPackage = new XpsDocument(xpsFileName, FileAccess.Read, CompressionOption.NotCompressed);//从文件中读取XPS文档

            FixedDocumentSequence fixedDocumentSequence = xpsPackage.GetFixedDocumentSequence();//从XPS文档对象得到FixedDocumentSequence
            viewer.Document = fixedDocumentSequence as IDocumentPaginatorSource;

            if (oldXpsPackage != null)
                oldXpsPackage.Close();
            xpsPackage.Close();
        }

        /// <summary>
        /// 设置文件夹的访问权限
        /// </summary>
        /// <param name="pathname">文件夹路径</param>
        /// <param name="username">访问用户名</param>
        /// <param name="power">权限</param>
        public static void SetFolderPower(string pathname, string username, string power)
        {

            DirectoryInfo dirinfo = new DirectoryInfo(pathname);

            if ((dirinfo.Attributes & FileAttributes.ReadOnly) != 0)
            {
                dirinfo.Attributes = FileAttributes.Normal;
            }

            //取得访问控制列表   
            DirectorySecurity dirsecurity = dirinfo.GetAccessControl();
            
            switch (power)
            {
                case "FullControl":
                    dirsecurity.AddAccessRule(new FileSystemAccessRule(username, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
                    break;
                case "ReadOnly":
                    dirsecurity.AddAccessRule(new FileSystemAccessRule(username, FileSystemRights.Read, AccessControlType.Allow));
                    break;
                case "Write":
                    dirsecurity.AddAccessRule(new FileSystemAccessRule(username, FileSystemRights.Write, AccessControlType.Allow));
                    break;
                case "Modify":
                    dirsecurity.AddAccessRule(new FileSystemAccessRule(username, FileSystemRights.Modify, AccessControlType.Allow));
                    break;
            }
            dirinfo.SetAccessControl(dirsecurity);
        }

        /// <summary>
        /// 判断文件是否已被锁定
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileInUsed(string filePath)
        {
            bool inUse = true;
            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                inUse = false;
            }
            catch { }
            finally { if (fs != null)fs.Close(); }
            return inUse;
        }
        
    }
}
