using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MEIKScreen.Common
{
    public class HttpWebTools
    {
        public static string Post(string url, NameValueCollection postDict, string token = null, int timeout = 100000)
        {
            string responseStr = null;
            JObject jObject = new JObject();            
            foreach (string key in postDict.Keys)
            {
                jObject[key] = postDict.Get(key);                
            }

            byte[] reqContent = Encoding.UTF8.GetBytes(jObject.ToString());
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = timeout;
            request.Method = "POST";
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", token);
            }
            //string contentType = "application/x-www-form-urlencoded;charset=UTF-8";
            request.ContentType = "application/json;charset=UTF-8";
            request.ContentLength = reqContent.Length;
            using (Stream newStream = request.GetRequestStream())
            {
                newStream.Write(reqContent, 0, reqContent.Length);                    
            }                        
            var response = request.GetResponse();
                
            using (var reader = new StreamReader(response.GetResponseStream())){
                responseStr = reader.ReadToEnd();
                reader.Close();
            }                
            response.Close();
            request.Abort();
                        
            return responseStr;
        }

        /// <summary>
        /// 处理http GET请求，返回数据
        /// </summary>
        /// <param name="url">请求的url地址</param>
        /// <returns>http GET成功后返回的数据，失败抛WebException异常</returns>
        public static string Get(string url, string token = null)
        {            
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", token);
            }
            WebResponse response = request.GetResponse();
            string rspContent=null;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                rspContent = reader.ReadToEnd();
                reader.Close();
            }
            request.Abort();
            response.Close();
            return rspContent;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="uploadfile"></param>
        /// <param name="url"></param>
        /// <param name="fileFormName"></param>
        /// <param name="contenttype"></param>
        /// <param name="querystring"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static string UploadFile(string url, string uploadfile, NameValueCollection postDict = null, string token = null, int timeout = 100000)
        {                                                                     
            //边界符
            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);            
            webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webrequest.Method = "POST";
            webrequest.Timeout = timeout;
            if (!string.IsNullOrEmpty(token))
            {
                webrequest.Headers.Add("Authorization", token);
            }

            byte[] beginBoundaryBytes = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
            
            //文件头
            StringBuilder sb = new StringBuilder();        
            sb.Append(string.Format("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\n", Path.GetFileName(uploadfile)));
            sb.Append("Content-Type: application/octet-stream\r\n\r\n");                        
            byte[] fileHeaderBytes = Encoding.UTF8.GetBytes(sb.ToString());

            //生成post表單數據
            StringBuilder stringBuilder = new StringBuilder();     
            foreach (string key in postDict.Keys)
            {
                stringBuilder.Append("\r\n--" + boundary + "\r\n");
                stringBuilder.Append(string.Format("Content-Disposition: form-data; name=\"{0}\"\r\n", key));
                //sb.Append("Content-Type: text/plain;charset=UTF-8\r\n");
                //stringBuilder.Append("Content-Type: application/json;charset=UTF-8");
                stringBuilder.Append(string.Format("\r\n\r\n{0}\r\n", postDict.Get(key)));
            }
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            // Build the trailing boundary string as a byte array  
            // ensuring the boundary appears on a line by itself  
            byte[] endBoundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");

            FileStream fileStream = new FileStream(uploadfile,FileMode.Open, FileAccess.Read);
            long length = beginBoundaryBytes.Length + fileHeaderBytes.Length + fileStream.Length + postHeaderBytes.Length + endBoundaryBytes.Length;
            webrequest.ContentLength = length;

            Stream requestStream = webrequest.GetRequestStream();
            //写入开始边界
            requestStream.Write(beginBoundaryBytes, 0, beginBoundaryBytes.Length);
            // 写入文件头
            requestStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);
            // 写入文件 
            byte[] buffer = new Byte[checked((uint)Math.Min(4096,(int)fileStream.Length))];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                requestStream.Write(buffer, 0, bytesRead);
            }
            //写入表单数据
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

            // 写入结束边界 
            requestStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);

            WebResponse responce = webrequest.GetResponse();
            Stream s = responce.GetResponseStream();
            StreamReader sr = new StreamReader(s);

            return sr.ReadToEnd();
        }

        /// <summary>
        /// Http下载文件
        /// </summary>
        public static string DownloadFile(string url, string path, string filename,NameValueCollection postDict, string token = null, int timeout = 100000)
        {
            JObject jObject = new JObject();
            foreach (string key in postDict.Keys)
            {
                jObject[key] = postDict.Get(key);
            }
            byte[] reqContent = Encoding.UTF8.GetBytes(jObject.ToString());

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.Timeout = timeout;
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", token);
            }
            request.ContentType = "application/json;charset=UTF-8";
            request.ContentLength = reqContent.Length;
            using (Stream newStream = request.GetRequestStream())
            {
                newStream.Write(reqContent, 0, reqContent.Length);
                newStream.Close();
            }

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            Stream responseStream = response.GetResponseStream();
            string downloadFilePath = path + System.IO.Path.DirectorySeparatorChar + filename;
            if (filename.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                downloadFilePath = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + filename;                                
            }            

            //创建本地文件写入流
            Stream stream = new FileStream(downloadFilePath, FileMode.Create);

            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, bArr.Length);
            }
            stream.Close();
            responseStream.Close();

            if (filename.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                ZipTools.Instance.UnZip(downloadFilePath, path);
                File.Delete(downloadFilePath);
            }             

            return path;
        }
    }
}
