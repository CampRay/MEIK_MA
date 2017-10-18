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
        private ShortFormReport reportModel;
        private string imgFileName;
        public ScreenResultReport(Person person,ShortFormReport shortFormReportModel)
        {            
            InitializeComponent();
            this.selectedUser = person;
            this.reportModel = shortFormReportModel;
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
                selectedUser.LeftResult = rightBreastResult.SelectedIndex;
                selectedUser.RightResult = leftBreastResult.SelectedIndex;
                selectedUser.Result = selectedUser.LeftResult > selectedUser.RightResult ? selectedUser.LeftResult : selectedUser.RightResult;
                OperateIniFile.WriteIniData("Report", "Result", selectedUser.Result + "", selectedUser.IniFilePath);
                string filePath = createPDF();
                sendPDFReport(filePath);
                sendCsvReport();
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
                        MessageBox.Show(this, selectedUser.Code + " :: " + App.Current.FindResource("Message_52").ToString() + json["info"]);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to upload screen summary report to server. Error message: " + e.Message);
            }
        }

        /// <summary>
        ///导出Long Form到Excel文件 
        /// </summary>
        /// <param name="reportTempl"></param>
        /// <param name="pdfFile"></param>
        /// <param name="reportModel"></param>
        private void sendCsvReport()
        {
            try
            {
                string csvFile = selectedUser.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + selectedUser.Code + ".csv ";
                if (File.Exists(csvFile))
                {
                    File.Delete(csvFile);
                }
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add(App.Current.FindResource("ReportContext_202").ToString(), reportModel.DataClientNum);
                nvc.Add(App.Current.FindResource("ReportContext_9").ToString(), reportModel.DataUserCode);
                nvc.Add(App.Current.FindResource("ReportContext_2").ToString(), reportModel.DataScreenDate);
                nvc.Add(App.Current.FindResource("ReportContext_1").ToString(), reportModel.DataName);
                nvc.Add(App.Current.FindResource("ReportContext_10").ToString(), reportModel.DataAge);
                nvc.Add(App.Current.FindResource("ReportContext_203").ToString(), reportModel.DataHeight);
                nvc.Add(App.Current.FindResource("ReportContext_11").ToString(), reportModel.DataWeight + " Kgs");
                nvc.Add(App.Current.FindResource("ReportContext_204").ToString(), reportModel.DataMobile);
                nvc.Add(App.Current.FindResource("ReportContext_205").ToString(), reportModel.DataEmail);
                nvc.Add(App.Current.FindResource("ReportContext_6").ToString(), reportModel.DataScreenLocation);
                nvc.Add(App.Current.FindResource("ReportContext_149").ToString(), reportModel.DataMeikTech);
                nvc.Add(App.Current.FindResource("ReportContext_14").ToString(), reportModel.DataMenstrualCycle);
                nvc.Add(App.Current.FindResource("ReportContext_23").ToString(), reportModel.DataSkinAffections);
                nvc.Add(App.Current.FindResource("ReportContext_22").ToString(), reportModel.DataHormones);
                nvc.Add(App.Current.FindResource("ReportContext_40").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftBreast);
                nvc.Add(App.Current.FindResource("ReportContext_40").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightBreast);
                nvc.Add(App.Current.FindResource("ReportContext_47").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftChangesOfElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_47").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightChangesOfElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_51").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMammaryStruct);
                nvc.Add(App.Current.FindResource("ReportContext_51").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMammaryStruct);
                nvc.Add(App.Current.FindResource("ReportContext_54").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftLactiferousSinusZone);
                nvc.Add(App.Current.FindResource("ReportContext_54").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightLactiferousSinusZone);
                nvc.Add(App.Current.FindResource("ReportContext_59").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMammaryContour);
                nvc.Add(App.Current.FindResource("ReportContext_59").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMammaryContour);
                nvc.Add(App.Current.FindResource("ReportContext_65").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftNumber);
                nvc.Add(App.Current.FindResource("ReportContext_65").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightNumber);
                nvc.Add(App.Current.FindResource("ReportContext_64").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftLocation);
                nvc.Add(App.Current.FindResource("ReportContext_64").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightLocation);
                nvc.Add(App.Current.FindResource("ReportContext_66").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", string.IsNullOrEmpty(reportModel.DataLeftSize) ? "" : reportModel.DataLeftSize + " mm");
                nvc.Add(App.Current.FindResource("ReportContext_66").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", string.IsNullOrEmpty(reportModel.DataRightSize) ? "" : reportModel.DataLeftSize + " mm");
                nvc.Add(App.Current.FindResource("ReportContext_67").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftShape);
                nvc.Add(App.Current.FindResource("ReportContext_67").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightShape);
                nvc.Add(App.Current.FindResource("ReportContext_72").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftContourAroundFocus);
                nvc.Add(App.Current.FindResource("ReportContext_72").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightContourAroundFocus);
                nvc.Add(App.Current.FindResource("ReportContext_76").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftInternalElectricalStructure);
                nvc.Add(App.Current.FindResource("ReportContext_76").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightInternalElectricalStructure);
                nvc.Add(App.Current.FindResource("ReportContext_81").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftSurroundingTissues);
                nvc.Add(App.Current.FindResource("ReportContext_81").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightSurroundingTissues);

                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMeanElectricalConductivity1);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMeanElectricalConductivity1);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMeanElectricalConductivity2);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMeanElectricalConductivity2);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + reportModel.DataMeanElectricalConductivity3 + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMeanElectricalConductivity3);
                nvc.Add(App.Current.FindResource("ReportContext_100").ToString() + " - " + reportModel.DataMeanElectricalConductivity3 + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMeanElectricalConductivity3);
                nvc.Add(App.Current.FindResource("ReportContext_106").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString(), reportModel.DataLeftComparativeElectricalConductivity1);
                nvc.Add(App.Current.FindResource("ReportContext_106").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString(), reportModel.DataLeftComparativeElectricalConductivity2);
                nvc.Add(App.Current.FindResource("ReportContext_106").ToString() + " - " + reportModel.DataComparativeElectricalConductivity3, reportModel.DataLeftComparativeElectricalConductivity3);
                nvc.Add(App.Current.FindResource("ReportContext_107").ToString() + " - " + App.Current.FindResource("ReportContext_101").ToString(), reportModel.DataLeftDivergenceBetweenHistograms1);
                nvc.Add(App.Current.FindResource("ReportContext_107").ToString() + " - " + App.Current.FindResource("ReportContext_102").ToString(), reportModel.DataLeftDivergenceBetweenHistograms2);
                nvc.Add(App.Current.FindResource("ReportContext_107").ToString() + " - " + reportModel.DataDivergenceBetweenHistograms3, reportModel.DataLeftDivergenceBetweenHistograms3);
                nvc.Add(App.Current.FindResource("ReportContext_109").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftPhaseElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_109").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightPhaseElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_110").ToString() + "[" + App.Current.FindResource("ReportContext_110").ToString() + "]" + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftAgeElectricalConductivity);
                nvc.Add(App.Current.FindResource("ReportContext_110").ToString() + "[" + App.Current.FindResource("ReportContext_110").ToString() + "]" + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightAgeElectricalConductivity);

                nvc.Add(App.Current.FindResource("ReportContext_115").ToString(), reportModel.DataExamConclusion);

                nvc.Add(App.Current.FindResource("ReportContext_121").ToString(), reportModel.DataLeftMammaryGland);
                nvc.Add(App.Current.FindResource("ReportContext_139").ToString(), reportModel.DataRightMammaryGland);

                nvc.Add(App.Current.FindResource("ReportContext_127").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftAgeRelated);
                nvc.Add(App.Current.FindResource("ReportContext_127").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightAgeRelated);

                nvc.Add(App.Current.FindResource("ReportContext_182").ToString() + "(" + App.Current.FindResource("ReportContext_44").ToString() + ")", reportModel.DataLeftMammaryGlandResult);
                nvc.Add(App.Current.FindResource("ReportContext_182").ToString() + "(" + App.Current.FindResource("ReportContext_45").ToString() + ")", reportModel.DataRightMammaryGlandResult);

                nvc.Add(App.Current.FindResource("ReportContext_141").ToString() + App.Current.FindResource("ReportContext_142").ToString(), reportModel.DataTotalPts);
                nvc.Add(App.Current.FindResource("ReportContext_150").ToString(), reportModel.DataBiRadsCategory);
                nvc.Add(App.Current.FindResource("ReportContext_226").ToString(), reportModel.DataConclusion);
                nvc.Add(App.Current.FindResource("ReportContext_157").ToString(), reportModel.DataRecommendation);
                nvc.Add(App.Current.FindResource("ReportContext_227").ToString(), reportModel.DataComments);
                nvc.Add(App.Current.FindResource("ReportContext_173").ToString(), reportModel.DataDoctor);
                nvc.Add(App.Current.FindResource("ReportContext_200").ToString(), reportModel.DataDoctorLicense);

                nvc.Add(App.Current.FindResource("ReportContext_190").ToString(), reportModel.DataSignDate);

                StringBuilder sb = new StringBuilder();
                StringBuilder sb1 = new StringBuilder();
                for (int i = 0; i < nvc.Count; i++)
                {
                    var key = nvc.GetKey(i);
                    var value = nvc[i];
                    if (key.Contains('"'))
                    {
                        key = key.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号                  
                    }
                    if (key.Contains('"') || key.Contains('\r') || key.Contains('\n') || key.Contains(' '))
                    {
                        sb.Append("\"" + key + "\",");
                    }
                    else
                    {
                        sb.Append(key + ",");
                    }

                    if (value.Contains('"'))
                    {
                        value = value.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号                  
                    }
                    if (value.Contains('"') || value.Contains('\r') || value.Contains('\n') || value.Contains(' '))
                    {
                        sb1.Append("\"" + value + "\",");
                    }
                    else
                    {
                        sb1.Append(value + ",");
                    }
                }
                FileStream fs = new FileStream(csvFile, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                sw.WriteLine(sb.ToString());
                sw.WriteLine(sb1.ToString());
                sw.Close();
                fs.Close();

                JObject jObject = new JObject();
                jObject["code"] = selectedUser.Code;                
                NameValueCollection nvlist = new NameValueCollection();
                nvlist.Add("jsonStr", jObject.ToString());
                string resStr = HttpWebTools.UploadFile(App.reportSettingModel.CloudPath + "/api/sendScreenCsv", csvFile, nvlist, App.reportSettingModel.CloudToken);
                var json = JObject.Parse(resStr);
                bool res = (bool)json["status"];
                if (!res)
                {
                    MessageBox.Show(this, selectedUser.Code + " :: " + App.Current.FindResource("Message_52").ToString() + json["info"]);
                }
            }
            catch (Exception exe)
            {
                throw new Exception("Failed to upload screen summary csv file to server. Error message: " + exe.Message);
            }
        }
    }
}
