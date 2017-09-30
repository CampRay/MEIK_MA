using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MEIKReport.Common
{
    public class FtpHelper
    {
        public static readonly FtpHelper Instance = new FtpHelper();

        /// <summary>
        /// 获取当前FTP目录下文件列表(仅文件)
        /// </summary>
        /// <param name="ftpPath">ftp路径</param>
        /// <returns></returns>
        public List<string> GetFileList(string userId, string pwd, string ftpPath)
        {
            List<string> downloadFiles = new List<string>();
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(userId, pwd);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                reqFTP.UsePassive = false;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    downloadFiles.Add(line);   
                    line = reader.ReadLine();
                }                
                reader.Close();
                response.Close();
                return downloadFiles;
            }
            catch (Exception)
            {                
                return downloadFiles; 
            }
        }

        /// <summary>
        /// 获取当前目录下明细(包含文件和文件夹)
        /// </summary>
        /// <returns></returns>
        public string[] GetFilesDetailList(string userId, string pwd, string ftpPath)
        {            
            List<string> downloadFiles = new List<string>();
            try
            {                
                FtpWebRequest ftp;
                ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath));
                ftp.Credentials = new NetworkCredential(userId, pwd);
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
               
                string line = reader.ReadLine(); 
                while (line != null)
                {
                    downloadFiles.Add(line);                    
                    line = reader.ReadLine();
                }                
                reader.Close();
                response.Close();
                return downloadFiles.ToArray();
            }
            catch (Exception)
            {                
                return downloadFiles.ToArray();
            }
        }

        /// <summary>
        /// 获取当前目录下所有的文件夹列表(仅文件夹)
        /// </summary>
        /// <returns></returns>
        public string[] GetDirectoryList(string userId, string pwd, string ftpPath)
        {
            List<string> downloadFolder = new List<string>();
            try
            {
                FtpWebRequest ftp;
                ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath));
                ftp.Credentials = new NetworkCredential(userId, pwd);
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                string line = reader.ReadLine();
                while (line != null)
                {
                    int dirPos = line.IndexOf("<DIR>");
                    if (dirPos > 0)
                    {
                        /*判断 Windows 风格*/                        
                        downloadFolder.Add(line.Substring(dirPos + 5).Trim());
                    }
                    else if (line.Trim().Substring(0, 1).ToUpper() == "D")
                    {
                        /*判断 Unix 风格*/
                        string[] arr = line.Split(' ');
                        string dir = arr[arr.Length - 1];
                        //string dir = line.Substring(58).Trim();
                        if (dir != "." && dir != "..")
                        {
                            downloadFolder.Add(dir);
                        }
                    }
                    
                    line = reader.ReadLine();
                }
                reader.Close();
                response.Close();
                return downloadFolder.ToArray();
            }
            catch (Exception)
            {
                return downloadFolder.ToArray();
            }            
        }

        //ftp的上传功能
        public void Upload(string userId, string pwd, string ftpPath, string filename)
        {
            FileInfo fileInf = new FileInfo(filename);
            FtpWebRequest reqFTP;
            // 根据uri创建FtpWebRequest对象 
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath + fileInf.Name));
            // ftp用户名和密码
            reqFTP.Credentials = new NetworkCredential(userId, pwd);

            reqFTP.UsePassive = false;
            // 默认为true，连接不会被关闭
            // 在一个命令之后被执行
            reqFTP.KeepAlive = false;
            // 指定执行什么命令
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            // 指定数据传输类型
            reqFTP.UseBinary = true;
            // 上传文件时通知服务器文件的大小
            reqFTP.ContentLength = fileInf.Length;
            // 缓冲大小设置为2kb
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            // 打开一个文件流 (System.IO.FileStream) 去读上传的文件
            FileStream fs = fileInf.OpenRead();
            // 把上传的文件写入流
            Stream strm = reqFTP.GetRequestStream();
            // 每次读文件流的2kb
            contentLen = fs.Read(buff, 0, buffLength);
            // 流内容没有结束
            while (contentLen != 0)
            {
                // 把内容从file stream 写入 upload stream
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
            }
            // 关闭两个流
            strm.Close();
            fs.Close();    
        }

        //从ftp服务器上下载文件的功能
        public void Download(string userId, string pwd, string ftpPath, string filePath, string fileName)
        {
            FtpWebRequest reqFTP;
            try
            {
                FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath + fileName));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(userId, pwd);
                reqFTP.UsePassive = false;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(string userId, string pwd, string ftpPath, string fileName)
        {
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath + fileName));
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(userId, pwd);
                reqFTP.UsePassive = false;
                FtpWebResponse listResponse = (FtpWebResponse)reqFTP.GetResponse();
                string sStatus = listResponse.StatusDescription;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="folderName"></param>
        public void RemoveDirectory(string userId, string pwd, string ftpPath, string folderName)
        {
            try
            {
                string uri = ftpPath + folderName;
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));

                reqFTP.Credentials = new NetworkCredential(userId, pwd);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;

                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// 判断当前目录下指定的子目录是否存在
        /// </summary>
        /// <param name="RemoteDirectoryName">指定的目录名</param>
        public bool DirectoryExist(string userId, string pwd, string ftpPath,string RemoteDirectoryName)
        {
            string[] dirList = GetDirectoryList(userId, pwd, ftpPath);
            foreach (string str in dirList)
            {
                if (str.Trim() == RemoteDirectoryName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断当前目录下指定的文件是否存在
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>
        public bool FileExist(string userId, string pwd, string ftpPath, string RemoteFileName)
        {
            var fileList = GetFileList(userId, pwd, ftpPath);
            foreach (string str in fileList)
            {
                if (str.Trim() == RemoteFileName.Trim())
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断当前目录下指定的目录是否存在
        /// </summary>
        /// <param name="RemoteFileName">远程文件夹名</param>
        public bool FolderExist(string userId, string pwd, string ftpPath, string RemoteFolderName)
        {
            string[] folderList = GetDirectoryList(userId, pwd, ftpPath);
            foreach (string str in folderList)
            {
                if (str.Trim() == RemoteFolderName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="dirName"></param>
        public void MakeDir(string userId, string pwd, string ftpPath,string dirName)
        {
            FtpWebRequest reqFTP;
            try
            {
                // dirName = name of the directory to create.
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath + dirName));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(userId, pwd);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// 获取指定文件大小
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public long GetFileSize(string userId, string pwd, string ftpPath, string filename)
        {
            FtpWebRequest reqFTP;
            long fileSize = 0;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath + filename));
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(userId, pwd);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                fileSize = response.ContentLength;

                ftpStream.Close();
                response.Close();
            }
            catch (Exception)
            {
                
            }
            return fileSize;
        }

        /// <summary>
        /// 改名
        /// </summary>
        /// <param name="currentFilename"></param>
        /// <param name="newFilename"></param>
        public void ReName(string userId, string pwd, string ftpPath, string currentFilename, string newFilename)
        {
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpPath + currentFilename));
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFilename;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(userId, pwd);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="currentFilename"></param>
        /// <param name="newFilename"></param>
        public void MovieFile(string userId, string pwd, string ftpPath, string currentFilename, string newDirectory)
        {
            ReName(userId, pwd, ftpPath,currentFilename, newDirectory);
        }        

    }
}
