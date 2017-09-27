using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MEIKScreen.Common
{
    public class PDFTools
    {
        /// <summary>
        /// 保存XPS文件到PDF文件
        /// </summary>
        /// <param name="xpsPath"></param>
        /// <param name="pdfFilePath"></param>
        public  static void SavePDFFile(string xpsPath, string pdfFilePath)
        {            
            if (File.Exists(xpsPath))
            {                
                //pdfFilePath = this.GetContainerPathFromDialog();         
                var excuteDll = Path.Combine(System.Environment.CurrentDirectory, "gxpswin32.exe");

                ProcessStartInfo gxpsArguments = new ProcessStartInfo(excuteDll, String.Format("-sDEVICE=pdfwrite -sOutputFile={0} -dNOPAUSE {1}", "\"" + pdfFilePath + "\"", "\"" + xpsPath + "\""));

                gxpsArguments.WindowStyle = ProcessWindowStyle.Hidden;

                using (var gxps = Process.Start(gxpsArguments))
                {
                    gxps.WaitForExit();
                }
                if (File.Exists(xpsPath))
                {
                    File.Delete(xpsPath);//删除临时文件
                }
            }
            else
            {
                MessageBox.Show("The file not find.");
            }

        }
    }
}
