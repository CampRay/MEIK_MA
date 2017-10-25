using MEIKReport.Common;
using MEIKReport.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Shapes;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace MEIKReport.Views
{
    /// <summary>
    /// ScreenResultReport.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenResultReport : Window
    {
        private Person selectedUser;
        private ShortFormReport reportModel = new ShortFormReport();
        private string imgFileName;
        public ScreenResultReport(Person person)
        {            
            InitializeComponent();
            this.selectedUser = person;            
            textScreenCode.Content = person.Code;
            string screenshotFolder = AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "Screenshot";
            this.imgFileName = screenshotFolder + System.IO.Path.DirectorySeparatorChar + person.Code;
            if (File.Exists(imgFileName))
            {
                this.dataScreenShotImg.Source = ImageTools.GetBitmapImage(imgFileName);
            }            
        }

        private void btnSendScreenReport_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(imgFileName))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_90").ToString());
                return;
            }

            if (string.IsNullOrEmpty(App.reportSettingModel.CloudUser) || string.IsNullOrEmpty(App.reportSettingModel.CloudToken))
            {
                LoginPage loginPage = new LoginPage();
                var loginResult = loginPage.ShowDialog();
                if (!loginResult.HasValue || !loginResult.Value)
                {
                    return;
                }
            }

            try
            {
                selectedUser.LeftResult = leftBreastResult.SelectedIndex;
                selectedUser.RightResult = rightBreastResult.SelectedIndex;
                selectedUser.Result = selectedUser.LeftResult > selectedUser.RightResult ? selectedUser.LeftResult : selectedUser.RightResult;
                OperateIniFile.WriteIniData("Report", "Result", selectedUser.Result + "", selectedUser.IniFilePath);

                //ShortFormReport reportModel = new ShortFormReport();
                reportModel.DataClientNum = string.IsNullOrEmpty(selectedUser.ClientNumber) ? "N/A" : selectedUser.ClientNumber;
                reportModel.DataName = selectedUser.SurName + (string.IsNullOrEmpty(selectedUser.GivenName) ? "" : "," + selectedUser.GivenName) + (string.IsNullOrEmpty(selectedUser.OtherName) ? "" : " " + selectedUser.OtherName); 
                reportModel.DataAge = selectedUser.Age + "";
                reportModel.DataHeight = string.IsNullOrEmpty(selectedUser.Height) ? "N/A" : selectedUser.Height;
                reportModel.DataWeight = string.IsNullOrEmpty(selectedUser.Weight) ? "N/A" : selectedUser.Weight;
                reportModel.DataMobile = string.IsNullOrEmpty(selectedUser.Mobile) ? "N/A" : selectedUser.Mobile;
                reportModel.DataEmail = string.IsNullOrEmpty(selectedUser.Email) ? "N/A" : selectedUser.Email;
                reportModel.DataUserCode = selectedUser.Code;
                reportModel.DataScreenDate = selectedUser.RegYear + "-" + selectedUser.RegMonth + "-" + selectedUser.RegDate;
                reportModel.DataScreenLocation = string.IsNullOrEmpty(selectedUser.ScreenVenue) ? "N/A" : selectedUser.ScreenVenue;
                reportModel.DataMeikTech = string.IsNullOrEmpty(selectedUser.TechName) ? "N/A" : selectedUser.TechName;                
                reportModel.DataSignDate = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day;
                reportModel.DataPoint = (selectedUser.LeftResult > selectedUser.RightResult ? selectedUser.LeftResult : selectedUser.RightResult) + "";
                reportModel.DataLeftTotalPts = selectedUser.LeftResult + "";
                reportModel.DataRightTotalPts = selectedUser.RightResult + "";
                reportModel.DataLeftBiRadsCategory = selectedUser.LeftResult == 2 ? "發現可疑病理性改變 Suspicious changes detected" : (selectedUser.LeftResult == 1 ? "發現良性病理性改變 Benign changes detected" : "正常 Normal");
                reportModel.DataRightBiRadsCategory = selectedUser.RightResult == 2 ? "發現可疑病理性改變 Suspicious changes detected" : (selectedUser.RightResult == 1 ? "發現良性病理性改變 Benign changes detected" : "正常 Normal");
                //上传报告
                sendPDFReport(createPDF());                
                sendCSVReport(createCsv());
                MessageBox.Show(this, "Upload screen summary report successfully.");
            }
            catch (Exception exe)
            {
                MessageBox.Show(this, exe.Message);
            }
        }


        /// <summary>
        /// 創建自動化的PDF報告
        /// </summary>
        private string createPDF()
        {
            try
            {
                //報告文件路徑                
                string sfPdfFile = selectedUser.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + " - " + reportModel.DataName + ".pdf";
                
                //生成Summary报告的PDF文件                            
                string sfReportTempl = "Views/SummaryReportImageDocument.xaml";
                string xpsFile = selectedUser.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + ".xps";
                if (File.Exists(xpsFile))
                {
                    File.Delete(xpsFile);
                }

                if (File.Exists(this.imgFileName))
                {
                    reportModel.DataScreenShotImg = ImageTools.GetBytesImage(this.imgFileName);
                }

                FixedPage page = (FixedPage)PrintPreviewWindow.LoadFixedDocumentAndRender(sfReportTempl, reportModel);

                XpsDocument xpsDocument = new XpsDocument(xpsFile, FileAccess.ReadWrite);
                //将flow document写入基于内存的xps document中去
                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                writer.Write(page);
                xpsDocument.Close();
                if (File.Exists(sfPdfFile))
                {
                    File.Delete(sfPdfFile);
                }
                PDFTools.SavePDFFile(xpsFile, sfPdfFile);
                return sfPdfFile;
            }
            catch (Exception e){
                throw new Exception("Failed to create screen summary report. Error message: "+e.Message);
            }            
        }

        /// <summary>
        /// 上傳快速報告到雲服務
        /// </summary>
        private void sendPDFReport(string sfPdfFile)
        {
            try
            {
                //上传自动生成的PDF报告
                if (File.Exists(sfPdfFile))
                {
                    JObject jObject = new JObject();
                    jObject["code"] = selectedUser.Code;
                    jObject["result"] = selectedUser.Result;
                    NameValueCollection nvlist = new NameValueCollection();
                    nvlist.Add("jsonStr", jObject.ToString());
                    string resStr = HttpWebTools.UploadFile(App.reportSettingModel.CloudPath + "/api/sendScreenPDF", sfPdfFile, nvlist, App.reportSettingModel.CloudToken);
                    var json = JObject.Parse(resStr);
                    bool res = (bool)json["status"];
                    if (!res)
                    {
                        throw new Exception(selectedUser.Code + " :: " + App.Current.FindResource("Message_52").ToString() + json["info"]);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to upload screen summary report to server. Error message: " + e.Message);
            }
        }

        /// <summary>
        ///创建csv文件，并生成包含jpg和csv文件的zip 
        /// </summary>        
        private string createCsv()
        {
            try
            {
                string csvFile = selectedUser.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + ".csv ";
                if (File.Exists(csvFile))
                {
                    File.Delete(csvFile);
                }                

                StringBuilder titleData = new StringBuilder();
                titleData.Append("Client Number,");
                titleData.Append("Name,");
                titleData.Append("Age,");
                titleData.Append("Height,");
                titleData.Append("Weight,");
                titleData.Append("Mobile Number,");
                titleData.Append("Email,");
                titleData.Append("Screen code,");
                titleData.Append("Screen date,");
                titleData.Append("Screen venue,");
                titleData.Append("Technician Name,");
                titleData.Append("Left Breast Result,");
                titleData.Append("Right Breast Result");

                StringBuilder data = new StringBuilder();
                data.Append("\"" + reportModel.DataClientNum + "\",");
                data.Append("\"" + reportModel.DataName + "\",");
                data.Append("\"" + reportModel.DataAge + "\",");
                data.Append("\"" + reportModel.DataHeight + "\",");
                data.Append("\"" + reportModel.DataWeight + "\",");
                data.Append("\"" + reportModel.DataMobile + "\",");
                data.Append("\"" + reportModel.DataEmail + "\",");
                data.Append("\"" + reportModel.DataUserCode + "\",");
                data.Append("\"" + reportModel.DataScreenDate + "\",");
                string str = reportModel.DataScreenLocation;
                if (str.Contains('"'))
                {
                    str = str.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号                  
                }
                if (str.Contains('"') || str.Contains('\r') || str.Contains('\n') || str.Contains(' '))
                {
                    data.Append("\"" + str + "\",");
                }
                else
                {
                    data.Append(str + ",");
                }
                data.Append("\"" + reportModel.DataMeikTech + "\",");
                data.Append("\"" + reportModel.DataLeftBiRadsCategory + "\",");
                data.Append("\"" + reportModel.DataRightBiRadsCategory + "\"");
                
                FileStream fs = new FileStream(csvFile, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine(titleData.ToString());
                sw.WriteLine(data.ToString());
                sw.Close();
                fs.Close();

                //添加L1-L7的原始圖片
                 try
                 {
                     FileInfo[] fileInfo = new DirectoryInfo(selectedUser.ArchiveFolder).GetFiles();
                     //遍历文件
                     foreach (FileInfo NextFile in fileInfo)
                     {
                         if (".jpg".Equals(NextFile.Extension, StringComparison.OrdinalIgnoreCase))
                         {
                             NextFile.Delete();
                         }
                     }
                 }
                 catch { }

                //複製截圖到檔案目錄
                string screenshotFile = selectedUser.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + ".jpg ";
                File.Copy(this.imgFileName, screenshotFile,true);

                string zipFile = AppDomain.CurrentDomain.BaseDirectory + "Data" + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + ".zip";
                ZipTools.Instance.ZipFiles(selectedUser.ArchiveFolder, zipFile, 4);
                return zipFile;               
            }
            catch (Exception exe)
            {
                throw new Exception("Failed to create screen csv file. Error message: " + exe.Message);
            }
        }

        /// <summary>
        /// 上傳Csv文件到雲服務
        /// </summary>
        private void sendCSVReport(string csvFile)
        {
            try
            {
                //上传自动生成的PDF报告
                if (File.Exists(csvFile))
                {                    
                    JObject jObject = new JObject();
                    jObject["code"] = selectedUser.Code;
                    
                    NameValueCollection nvlist = new NameValueCollection();
                    nvlist.Add("jsonStr", jObject.ToString());
                    string resStr = HttpWebTools.UploadFile(App.reportSettingModel.CloudPath + "/api/sendScreenCsv", csvFile, nvlist, App.reportSettingModel.CloudToken);
                    var json = JObject.Parse(resStr);
                    bool res = (bool)json["status"];
                    if (!res)
                    {
                        throw new Exception(selectedUser.Code + " :: " + App.Current.FindResource("Message_52").ToString() + json["info"]);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to upload screen summary csv file to server. Error message: " + e.Message);
            }
            finally
            {
                if (File.Exists(csvFile))
                {
                    try
                    {
                        File.Delete(csvFile);
                    }
                    catch {}                    
                }
            }
        }
    }
}
