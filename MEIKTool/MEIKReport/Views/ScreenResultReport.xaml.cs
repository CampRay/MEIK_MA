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
        public ScreenResultReport(Person person)
        {            
            InitializeComponent();
            this.selectedUser = person;
            textScreenCode.Content = person.Code;

            string imgFileName = person.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".png";
            if (File.Exists(imgFileName))
            {
                this.dataScreenShotImg.Source = ImageTools.GetBitmapImage(imgFileName);
            }
        }

        private void btnSendScreenReport_Click(object sender, RoutedEventArgs e)
        {

            selectedUser.LeftResult = rightBreastResult.SelectedIndex;
            selectedUser.RightResult = leftBreastResult.SelectedIndex;
            selectedUser.Result = selectedUser.LeftResult > selectedUser.RightResult ? selectedUser.LeftResult : selectedUser.RightResult;
            OperateIniFile.WriteIniData("Report", "Result", selectedUser.Result + "", selectedUser.IniFilePath);
            
            string filePath=createPDF();
            sendPDFReport(filePath);
        }


        /// <summary>
        /// 創建自動化的PDF報告
        /// </summary>
        private string createPDF()
        {
            try
            {
                //報告文件路徑
                string personName = selectedUser.SurName + (string.IsNullOrEmpty(selectedUser.GivenName) ? "" : "," + selectedUser.GivenName) + (string.IsNullOrEmpty(selectedUser.OtherName) ? "" : " " + selectedUser.OtherName);
                string sfPdfFile = selectedUser.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + " - " + personName + ".pdf";

                ShortFormReport reportModel = new ShortFormReport();
                reportModel.DataClientNum = string.IsNullOrEmpty(selectedUser.ClientNumber) ? "N/A" : selectedUser.ClientNumber;
                reportModel.DataName = personName;
                reportModel.DataAge = selectedUser.Age + "";
                reportModel.DataHeight = string.IsNullOrEmpty(selectedUser.Height) ? "N/A" : selectedUser.Height;
                reportModel.DataWeight = string.IsNullOrEmpty(selectedUser.Weight) ? "N/A" : selectedUser.Weight;
                reportModel.DataMobile = string.IsNullOrEmpty(selectedUser.Mobile) ? "N/A" : selectedUser.Mobile;
                reportModel.DataEmail = string.IsNullOrEmpty(selectedUser.Email) ? "N/A" : selectedUser.Email;
                reportModel.DataUserCode = selectedUser.Code;
                reportModel.DataScreenDate = selectedUser.RegYear + "-" + selectedUser.RegMonth + "-" + selectedUser.RegDate;
                reportModel.DataScreenLocation = string.IsNullOrEmpty(selectedUser.ScreenVenue) ? "N/A" : selectedUser.ScreenVenue;
                reportModel.DataMeikTech = string.IsNullOrEmpty(selectedUser.TechName) ? "N/A" : selectedUser.TechName;
                reportModel.DataSignDate = reportModel.DataScreenDate;
                reportModel.DataPoint = (selectedUser.LeftResult > selectedUser.RightResult ? selectedUser.LeftResult : selectedUser.RightResult) + "";
                reportModel.DataLeftTotalPts = selectedUser.LeftResult + "";
                reportModel.DataRightTotalPts = selectedUser.RightResult + "";
                reportModel.DataLeftBiRadsCategory = selectedUser.LeftResult == 2 ? "發現可疑病理性改變 Suspicious changes detected" : (selectedUser.LeftResult == 1 ? "發現良性病理性改變 Benign changes detected" : "正常 Normal");
                reportModel.DataRightBiRadsCategory = selectedUser.RightResult == 2 ? "發現可疑病理性改變 Suspicious changes detected" : (selectedUser.RightResult == 1 ? "發現良性病理性改變 Benign changes detected" : "正常 Normal");

                //生成Summary报告的PDF文件                            
                string sfReportTempl = "Views/SummaryReportImageDocument.xaml";
                string xpsFile = selectedUser.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + ".xps";
                if (File.Exists(xpsFile))
                {
                    File.Delete(xpsFile);
                }

                string imgFileName = selectedUser.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + ".png";
                if (File.Exists(imgFileName))
                {                    
                    reportModel.DataScreenShotImg = ImageTools.GetBytesImage(imgFileName);
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
            catch { }
            return null;
        }

        /// <summary>
        /// 上傳快速報告到雲服務
        /// </summary>
        private void sendPDFReport(string sfPdfFile)
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
                    MessageBox.Show(this,selectedUser.Code + " :: " + App.Current.FindResource("Message_52").ToString() + json["info"]);
                }
            }
        }
    }
}
