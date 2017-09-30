using MEIKReport.Common;
using MEIKReport.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Xml;

namespace MEIKReport.Views
{
    /// <summary>
    /// Examination Report Page 
    /// </summary>
    public partial class ExaminationReportPage : Window
    {
        private delegate void DoPrintMethod(PrintDialog pdlg, DocumentPaginator paginator);
        //public event CloseWindowHandler closeWindowEvent;
        private string dataFolder = AppDomain.CurrentDomain.BaseDirectory + "Data";
        private Person person = null;
        private ShortFormReport shortFormReportModel = new ShortFormReport();
        protected MouseHook mouseHook = new MouseHook();

        public ExaminationReportPage()
        {
            InitializeComponent();
        }
        public ExaminationReportPage(object data): this()
        {            
            mouseHook.MouseUp += new System.Windows.Forms.MouseEventHandler(mouseHook_MouseUp);
            //mouseHook.MouseMove += new System.Windows.Forms.MouseEventHandler(mouseHook_MouseMove);
            try { 
                this.person = data as Person;
                if (this.person == null)
                {
                    MessageBox.Show(this, "Please select a patient.");
                    this.Close();
                }
                else
                {                    
                    string dataFile = FindUserReportData(person.ArchiveFolder);
                    if (string.IsNullOrEmpty(dataFile))
                    {
                        dataFile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".dat";
                    }
                    //string dataFile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".dat";                    

                    if (File.Exists(dataFile))
                    {
                        try
                        {
                            this.shortFormReportModel = SerializeUtilities.Desrialize<ShortFormReport>(dataFile);
                        }
                        catch (Exception) {                                                        
                        }                        
                                                                  
                        if (shortFormReportModel.DataSignImg != null)
                        {
                            this.dataSignImg.Source = ImageTools.GetBitmapImage(shortFormReportModel.DataSignImg);
                        }                                                             
                        
                    }
                    else
                    {                        
                        shortFormReportModel.DataUserCode = person.Code;                                                                        
                        bool defaultSign = Convert.ToBoolean(OperateIniFile.ReadIniData("Report", "Use Default Signature", "false", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini"));
                        if (defaultSign)
                        {
                            string imgFile = AppDomain.CurrentDomain.BaseDirectory + "/Signature/temp.jpg";
                            if (File.Exists(imgFile))
                            {
                                this.dataSignImg.Source = ImageTools.GetBitmapImage(imgFile);
                                //dataScreenShotImg.Source = GetBitmapImage(AppDomain.CurrentDomain.BaseDirectory + "/Images/BigIcon.png");
                            }
                        }
                    }
                    if ("en-US".Equals(App.local))
                    {
                        shortFormReportModel.DataScreenDate = DateTime.ParseExact("20" + person.Code.Substring(0, 6), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture).ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        shortFormReportModel.DataScreenDate = DateTime.ParseExact("20" + person.Code.Substring(0, 6), "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture).ToString("yyyy年MM月dd日");
                    }
                    shortFormReportModel.DataClientNum = person.ClientNumber;
                    shortFormReportModel.DataName = person.SurName + "," + person.GivenName + " " + person.OtherName;
                    shortFormReportModel.DataAge = person.Age + "";
                    shortFormReportModel.DataAddress = person.Address;
                    shortFormReportModel.DataHeight = person.Height;
                    shortFormReportModel.DataWeight = person.Weight;
                    shortFormReportModel.DataMobile = person.Mobile;
                    shortFormReportModel.DataEmail = person.Email;
                    shortFormReportModel.DataScreenLocation = person.ScreenVenue;
                    this.reportDataGrid.DataContext = this.shortFormReportModel;
                    //以下是添加处理操作员和医生的名字的选择项                    
                    User doctorUser = new User();
                    if (!string.IsNullOrEmpty(shortFormReportModel.DataDoctor))
                    {
                        doctorUser.Name = shortFormReportModel.DataDoctor;
                        doctorUser.License = shortFormReportModel.DataDoctorLicense;
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(this.person.DoctorName)){
                            doctorUser.Name = this.person.DoctorName;
                            doctorUser.License = this.person.DoctorLicense;
                            shortFormReportModel.DataDoctor = this.person.DoctorName;
                            shortFormReportModel.DataDoctorLicense = this.person.DoctorLicense;
                        }                                                
                    }
                    this.dataDoctor.ItemsSource = App.reportSettingModel.DoctorNames;
                    if(!string.IsNullOrEmpty(doctorUser.Name)){                        
                        for (int i = 0; i < App.reportSettingModel.DoctorNames.Count; i++)
			            {
                            var item=App.reportSettingModel.DoctorNames[i];
                            if (doctorUser.Name.Equals(item.Name) && (string.IsNullOrEmpty(doctorUser.License)==string.IsNullOrEmpty(item.License)||doctorUser.License==item.License))
                            {                                
                                this.dataDoctor.SelectedIndex = i;
                                break;
                            }
			            }
                        //如果没有找到匹配的用户
                        if (this.dataDoctor.SelectedIndex == -1)
                        {
                            App.reportSettingModel.DoctorNames.Add(doctorUser);
                            this.dataDoctor.SelectedIndex = App.reportSettingModel.DoctorNames.Count - 1;
                        }
                    }                                      

                    User techUser = new User();
                    if (!string.IsNullOrEmpty(shortFormReportModel.DataMeikTech))
                    {
                        techUser.Name = shortFormReportModel.DataMeikTech;
                        techUser.License = shortFormReportModel.DataTechLicense;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.person.TechName))
                        {
                            techUser.Name = this.person.TechName;
                            techUser.License = this.person.TechLicense;
                            shortFormReportModel.DataMeikTech = this.person.TechName;
                            shortFormReportModel.DataTechLicense = this.person.TechLicense;
                        }
                    }
                    //this.dataMeikTech.ItemsSource = App.reportSettingModel.TechNames;
                    //if (!string.IsNullOrEmpty(techUser.Name))
                    //{
                    //    for (int i = 0; i < App.reportSettingModel.TechNames.Count; i++)
                    //    {
                    //        var item = App.reportSettingModel.TechNames[i];
                    //        if (techUser.Name.Equals(item.Name) && (string.IsNullOrEmpty(techUser.License) == string.IsNullOrEmpty(item.License) || techUser.License == item.License))
                    //        {
                    //            this.dataMeikTech.SelectedIndex = i;
                    //            break;
                    //        }
                    //    }
                    //    //如果没有找到匹配的用户
                    //    if (this.dataMeikTech.SelectedIndex == -1)
                    //    {
                    //        App.reportSettingModel.TechNames.Add(techUser);
                    //        this.dataMeikTech.SelectedIndex = App.reportSettingModel.TechNames.Count - 1;
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            //App.opendWin = null;            
            try
            {
                App.opendWin = null;
                IntPtr mainWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmMain", null);
                //如果主窗体存在
                if (mainWinHwnd != IntPtr.Zero)
                {
                    int WM_SYSCOMMAND = 0x0112;
                    int SC_CLOSE = 0xF060;
                    Win32Api.SendMessage(mainWinHwnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                }
                
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message);
            }
            //finally
            //{
            //    this.Owner.Show();
            //}
        }

        private void DoPrint(PrintDialog pdlg, DocumentPaginator paginator)
        {
            pdlg.PrintDocument(paginator, "Examination Report Document");
        }

        private void previewBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                LoadDataModel();
                var reportModel=CloneReportModel();
                //PrintPreviewWindow previewWnd = new PrintPreviewWindow("Views/ExaminationReportDocument.xaml", true, reportModel);
                PrintPreviewWindow previewWnd = new PrintPreviewWindow("Views/ExaminationReportFlow.xaml", false, reportModel);
                //PrintPreviewWindow previewWnd = new PrintPreviewWindow("Views/SummaryReportFlow.xaml", false, reportModel);                
                previewWnd.Owner = this;
                previewWnd.ShowInTaskbar = false;
                previewWnd.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void printBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                LoadDataModel();
                PrintDialog pdlg = new PrintDialog();                
                if (pdlg.ShowDialog() == true)
                {
                    FlowDocument page = (FlowDocument)PrintPreviewWindow.LoadFlowDocumentAndRender("Views/ExaminationReportFlow.xaml", shortFormReportModel);
                    if ("Letter".Equals(App.reportSettingModel.PrintPaper.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        //doc.PagePadding = new Thickness(20);
                        page.PageWidth = 96 * 8.5;
                        page.PageHeight = 96 * 11;
                        page.ColumnWidth = 734;
                    }
                    else if ("A4".Equals(App.reportSettingModel.PrintPaper.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        //doc.PagePadding = new Thickness(20);
                        page.PageWidth = 96 * 8.27;
                        page.PageHeight = 96 * 11.69;
                        page.ColumnWidth = 734;
                    }

                    //FixedDocument fixedDoc = new FixedDocument();//创建一个文档
                    //if ("Letter".Equals(App.reportSettingModel.PrintPaper.ToString(), StringComparison.OrdinalIgnoreCase))
                    //{
                    //    fixedDoc.DocumentPaginator.PageSize = new Size(96 * 8.5, 96 * 11);
                    //}
                    //else if ("A4".Equals(App.reportSettingModel.PrintPaper.ToString(), StringComparison.OrdinalIgnoreCase))
                    //{
                    //    fixedDoc.DocumentPaginator.PageSize = new Size(96 * 8.27, 96 * 11.69);
                    //}
                    
                    //PageContent pageContent = new PageContent(); 
                    //((IAddChild)pageContent).AddChild(page);
                    //fixedDoc.Pages.Add(pageContent);//将对象加入到当前文档中
                    //Dispatcher.BeginInvoke(new DoPrintMethod(DoPrint), DispatcherPriority.ApplicationIdle, pdlg, fixedDoc.DocumentPaginator);
                    Dispatcher.BeginInvoke(new DoPrintMethod(DoPrint), DispatcherPriority.ApplicationIdle, pdlg, ((IDocumentPaginatorSource)page).DocumentPaginator);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void btnOpenDiagn_Click(object sender, RoutedEventArgs e)
        {            
            try
            {
                IntPtr mainWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmMain", null);
                //如果主窗体不存在
                if (mainWinHwnd == IntPtr.Zero)
                {                    
                    IntPtr diagnosticsBtnHwnd = Win32Api.FindWindowEx(App.splashWinHwnd, IntPtr.Zero, null, App.strDiagnostics);
                    Win32Api.SendMessage(diagnosticsBtnHwnd, Win32Api.WM_CLICK, 0, 0);
                }                
                WinMinimized();
                var userListWin = this.Owner as UserList;                
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "System Exception: " + ex.Message);
            }
        }        

        private void save_Click(object sender, RoutedEventArgs e)
        {            
            SaveReport(null);
        }

        
        private void btnSignature_Click(object sender, RoutedEventArgs e)
        {
            SignatureBox signBox = new SignatureBox();
            signBox.Owner = this;
            //signBox.callbackMethod = ShowSignature;
            signBox.ShowDialog();
        }

        /// <summary>
        /// 保存报表数据到文件
        /// </summary>
        private void SaveReport(string otherDataFolder)
        {
            try
            {
                
                //保存到患者档案目录
                string datafile = null;
                if (!string.IsNullOrEmpty(otherDataFolder))
                {
                    datafile = otherDataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".dat";
                }
                else
                {
                    //保存到程序自身的Data目录
                    if (!Directory.Exists(dataFolder))
                    {
                        Directory.CreateDirectory(dataFolder);
                        FileHelper.SetFolderPower(dataFolder, "Everyone", "FullControl");
                        FileHelper.SetFolderPower(dataFolder, "Users", "FullControl");
                    }
                    datafile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".dat";
                }                
                LoadDataModel();
                
                if (string.IsNullOrEmpty(shortFormReportModel.DataSignDate))
                {
                    if ("en-US".Equals(App.local)){
                        shortFormReportModel.DataSignDate = DateTime.Today.ToString("MMMM d, yyyy");
                    }
                    else{
                        shortFormReportModel.DataSignDate = DateTime.Today.ToString("yyyy年MM月dd日");                        
                    }
                }
                SerializeUtilities.Serialize<ShortFormReport>(shortFormReportModel, datafile);
                File.Copy(datafile, person.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".dat", true);

                ////生成PDF报告
                //var reportModel = CloneReportModel();
                ////Save PDF file
                //string lfPdfFile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + "_LF.pdf";
                //string lfReportTempl = "Views/ExaminationReportDocument.xaml";
                //ExportPDF(lfReportTempl, lfPdfFile, reportModel);
                //File.Copy(lfPdfFile, person.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + person.Code + "_LF.pdf", true);
                //string sfPdfFile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + "_SF.pdf";
                //string sfReportTempl = "Views/SummaryReportDocument.xaml";                
                //if (shortFormReportModel.DataScreenShotImg != null)
                //{
                //    sfReportTempl = "Views/SummaryReportImageDocument.xaml";                                        
                //}
                //ExportPDF(sfReportTempl, sfPdfFile, reportModel);
                //File.Copy(sfPdfFile, person.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + person.Code + "_SF.pdf", true);

                MessageBox.Show(this, App.Current.FindResource("Message_2").ToString());
            }
            catch (Exception ex)
            {
                FileHelper.SetFolderPower(dataFolder, "Everyone", "FullControl");
                FileHelper.SetFolderPower(dataFolder, "Users", "FullControl");
                MessageBox.Show(this, App.Current.FindResource("Message_3").ToString() + ex.Message);
            }
        }

        /// <summary>
        /// 加载报表数据到模型对象
        /// </summary>
        private void LoadDataModel()
        {
            //shortFormReportModel.DataClientNum = this.dataClientNum.Text;
            //shortFormReportModel.DataUserCode = this.dataUserCode.Text;
            shortFormReportModel.DataAge = this.dataAge.Text;
            
            //if (this.dataMeikTech.SelectedItem != null)
            //{
            //    var technician = this.dataMeikTech.SelectedItem as User;
            //    shortFormReportModel.DataMeikTech = technician.Name;
            //    shortFormReportModel.DataTechLicense = technician.License;
            //}
                                    
            //shortFormReportModel.DataName = this.dataName.Text;
            
            shortFormReportModel.DataScreenDate = this.dataScreenDate.Text;
            //shortFormReportModel.DataScreenLocation = this.dataScreenLocation.Text;
            shortFormReportModel.DataScreenLocation = person.ScreenVenue;
            //shortFormReportModel.DataAddress = this.dataAddress.Text;
            shortFormReportModel.DataAddress = person.Address;

            //shortFormReportModel.DataGender = this.dataGender.SelectedIndex.ToString();
            shortFormReportModel.DataHealthCard = this.dataHealthCard.Text;
            shortFormReportModel.DataWeight = this.dataWeight.Text;
            //shortFormReportModel.DataWeightUnit = this.dataWeightUnit.SelectedIndex.ToString();
            shortFormReportModel.DataMenstrualCycle = this.dataMenstrualCycle.SelectedIndex.ToString();
            shortFormReportModel.DataHormones = this.dataHormones.Text;
            shortFormReportModel.DataSkinAffections = this.dataSkinAffections.SelectedIndex.ToString();
            //shortFormReportModel.DataPertinentHistory = this.dataPertinentHistory.SelectedIndex.ToString();
            //shortFormReportModel.DataPertinentHistory1 = this.dataPertinentHistory1.Text;
            shortFormReportModel.DataMotherUltra = this.dataMotherUltra.SelectedIndex.ToString();
            
            shortFormReportModel.DataLeftBreastH = this.dataLeftBreastH.Text;
            shortFormReportModel.DataRightBreastH = this.dataRightBreastH.Text;
            shortFormReportModel.DataLeftBreastM = this.dataLeftBreastM.Text;
            shortFormReportModel.DataRightBreastM = this.dataRightBreastM.Text;
            shortFormReportModel.DataLeftBreastAP = this.dataLeftBreastAP.Text;
            shortFormReportModel.DataRightBreastAP = this.dataRightBreastAP.Text;
            shortFormReportModel.DataLeftBreast = this.dataLeftBreastH.Text + ":" + this.dataLeftBreastM.Text + this.dataLeftBreastAP.Text;
            shortFormReportModel.DataRightBreast = this.dataRightBreastH.Text + ":" + this.dataRightBreastM.Text + this.dataRightBreastAP.Text; 

            shortFormReportModel.DataLeftPalpableMass = this.dataLeftPalpableMass.SelectedIndex.ToString();
            shortFormReportModel.DataRightPalpableMass = this.dataRightPalpableMass.SelectedIndex.ToString();
            shortFormReportModel.DataLeftChangesOfElectricalConductivity = this.dataLeftChangesOfElectricalConductivity.SelectedIndex.ToString();
            shortFormReportModel.DataRightChangesOfElectricalConductivity = this.dataRightChangesOfElectricalConductivity.SelectedIndex.ToString();
            shortFormReportModel.DataLeftMammaryStruct = this.dataLeftMammaryStruct.SelectedIndex.ToString();
            shortFormReportModel.DataRightMammaryStruct = this.dataRightMammaryStruct.SelectedIndex.ToString();
            shortFormReportModel.DataLeftLactiferousSinusZone = this.dataLeftLactiferousSinusZone.SelectedIndex.ToString();
            shortFormReportModel.DataRightLactiferousSinusZone = this.dataRightLactiferousSinusZone.SelectedIndex.ToString();
            shortFormReportModel.DataLeftMammaryContour = this.dataLeftMammaryContour.SelectedIndex.ToString();
            shortFormReportModel.DataRightMammaryContour = this.dataRightMammaryContour.SelectedIndex.ToString();

            shortFormReportModel.DataLeftNumber = this.dataLeftNumber.SelectedIndex.ToString();
            shortFormReportModel.DataRightNumber = this.dataRightNumber.SelectedIndex.ToString();

            shortFormReportModel.DataLeftLocation = this.dataLeftLocation.Text;
            shortFormReportModel.DataRightLocation = this.dataRightLocation.Text;
            shortFormReportModel.DataLeftSize = this.dataLeftSize.Text;
            shortFormReportModel.DataRightSize = this.dataRightSize.Text;
            //shortFormReportModel.DataLeftSizeLong = this.dataLeftSizeLong.Text;
            //shortFormReportModel.DataLeftSizeWidth = this.dataLeftSizeWidth.Text;
            //if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftSizeLong) || !string.IsNullOrEmpty(shortFormReportModel.DataLeftSizeWidth)) {
            //    shortFormReportModel.DataLeftSize = shortFormReportModel.DataLeftSizeLong + "x" + shortFormReportModel.DataLeftSizeWidth + App.Current.FindResource("ReportContext_225").ToString();
            //}
            //shortFormReportModel.DataRightSizeLong = this.dataRightSizeLong.Text;
            //shortFormReportModel.DataRightSizeWidth = this.dataRightSizeWidth.Text;
            //if (!string.IsNullOrEmpty(shortFormReportModel.DataRightSizeLong) || !string.IsNullOrEmpty(shortFormReportModel.DataRightSizeWidth))
            //{
            //    shortFormReportModel.DataRightSize = shortFormReportModel.DataRightSizeLong + "x" + shortFormReportModel.DataRightSizeWidth + App.Current.FindResource("ReportContext_225").ToString();
            //}
            shortFormReportModel.DataLeftShape = this.dataLeftShape.SelectedIndex.ToString();
            shortFormReportModel.DataRightShape = this.dataRightShape.SelectedIndex.ToString();
            shortFormReportModel.DataLeftContourAroundFocus = this.dataLeftContourAroundFocus.SelectedIndex.ToString();
            shortFormReportModel.DataRightContourAroundFocus = this.dataRightContourAroundFocus.SelectedIndex.ToString();
            shortFormReportModel.DataLeftInternalElectricalStructure = this.dataLeftInternalElectricalStructure.SelectedIndex.ToString();
            shortFormReportModel.DataRightInternalElectricalStructure = this.dataRightInternalElectricalStructure.SelectedIndex.ToString();
            shortFormReportModel.DataLeftSurroundingTissues = this.dataLeftSurroundingTissues.SelectedIndex.ToString();
            shortFormReportModel.DataRightSurroundingTissues = this.dataRightSurroundingTissues.SelectedIndex.ToString();
            
            shortFormReportModel.DataLeftOncomarkerHighlightBenignChanges = this.dataLeftOncomarkerHighlightBenignChanges.SelectedIndex.ToString();
            shortFormReportModel.DataRightOncomarkerHighlightBenignChanges = this.dataRightOncomarkerHighlightBenignChanges.SelectedIndex.ToString();
            shortFormReportModel.DataLeftOncomarkerHighlightSuspiciousChanges = this.dataLeftOncomarkerHighlightSuspiciousChanges.SelectedIndex.ToString();
            shortFormReportModel.DataRightOncomarkerHighlightSuspiciousChanges = this.dataRightOncomarkerHighlightSuspiciousChanges.SelectedIndex.ToString();
            shortFormReportModel.DataLeftMeanElectricalConductivity1 = this.dataLeftMeanElectricalConductivity1.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity1N1 = this.dataLeftMeanElectricalConductivity1N1.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity1N2 = this.dataLeftMeanElectricalConductivity1N2.Text;
            shortFormReportModel.DataRightMeanElectricalConductivity1 = this.dataRightMeanElectricalConductivity1.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity1N1 = this.dataRightMeanElectricalConductivity1N1.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity1N2 = this.dataRightMeanElectricalConductivity1N2.Text;
            if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMeanElectricalConductivity1))
            {
                shortFormReportModel.DataLeftMeanECOfLesion = shortFormReportModel.DataLeftMeanElectricalConductivity1;
            }
            if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMeanElectricalConductivity1))
            {
                shortFormReportModel.DataRightMeanECOfLesion = shortFormReportModel.DataRightMeanElectricalConductivity1;
            }

            shortFormReportModel.DataLeftMeanElectricalConductivity2 = this.dataLeftMeanElectricalConductivity2.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity2N1 = this.dataLeftMeanElectricalConductivity2N1.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity2N2 = this.dataLeftMeanElectricalConductivity2N2.Text;
            shortFormReportModel.DataRightMeanElectricalConductivity2 = this.dataRightMeanElectricalConductivity2.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity2N1 = this.dataRightMeanElectricalConductivity2N1.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity2N2 = this.dataRightMeanElectricalConductivity2N2.Text;
            if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMeanElectricalConductivity2))
            {
                shortFormReportModel.DataLeftMeanECOfLesion = shortFormReportModel.DataLeftMeanElectricalConductivity2;
            }
            if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMeanElectricalConductivity2))
            {
                shortFormReportModel.DataRightMeanECOfLesion = shortFormReportModel.DataRightMeanElectricalConductivity2;
            }

            shortFormReportModel.DataMeanElectricalConductivity3 = this.dataMeanElectricalConductivity3.SelectedIndex.ToString();
            shortFormReportModel.DataLeftMeanElectricalConductivity3 = this.dataLeftMeanElectricalConductivity3.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity3N1 = this.dataLeftMeanElectricalConductivity3N1.Text;
            //shortFormReportModel.DataLeftMeanElectricalConductivity3N2 = this.dataLeftMeanElectricalConductivity3N2.Text;
            shortFormReportModel.DataRightMeanElectricalConductivity3 = this.dataRightMeanElectricalConductivity3.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity3N1 = this.dataRightMeanElectricalConductivity3N1.Text;
            //shortFormReportModel.DataRightMeanElectricalConductivity3N2 = this.dataRightMeanElectricalConductivity3N2.Text;
            if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMeanElectricalConductivity3))
            {
                shortFormReportModel.DataLeftMeanECOfLesion = shortFormReportModel.DataLeftMeanElectricalConductivity3;
            }
            if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMeanElectricalConductivity3))
            {
                shortFormReportModel.DataRightMeanECOfLesion = shortFormReportModel.DataRightMeanElectricalConductivity3;
            }


            //shortFormReportModel.DataComparativeElectricalConductivityReference1 = this.dataComparativeElectricalConductivityReference1.Text;
            shortFormReportModel.DataLeftComparativeElectricalConductivity1 = this.dataLeftComparativeElectricalConductivity1.Text;
            //shortFormReportModel.DataRightComparativeElectricalConductivity1 = this.dataRightComparativeElectricalConductivity1.Text;
            //shortFormReportModel.DataComparativeElectricalConductivityReference2 = this.dataComparativeElectricalConductivityReference2.Text;
            shortFormReportModel.DataLeftComparativeElectricalConductivity2 = this.dataLeftComparativeElectricalConductivity2.Text;
            //shortFormReportModel.DataRightComparativeElectricalConductivity2 = this.dataRightComparativeElectricalConductivity2.Text;
            shortFormReportModel.DataComparativeElectricalConductivity3 = this.dataComparativeElectricalConductivity3.SelectedIndex.ToString();
            shortFormReportModel.DataLeftComparativeElectricalConductivity3 = this.dataLeftComparativeElectricalConductivity3.Text;
            //shortFormReportModel.DataRightComparativeElectricalConductivity3 = this.dataRightComparativeElectricalConductivity3.Text;

            //shortFormReportModel.DataDivergenceBetweenHistogramsReference1 = this.dataDivergenceBetweenHistogramsReference1.Text;
            shortFormReportModel.DataLeftDivergenceBetweenHistograms1 = this.dataLeftDivergenceBetweenHistograms1.Text;
            //shortFormReportModel.DataRightDivergenceBetweenHistograms1 = this.dataRightDivergenceBetweenHistograms1.Text;
            //shortFormReportModel.DataDivergenceBetweenHistogramsReference2 = this.dataDivergenceBetweenHistogramsReference2.Text;
            shortFormReportModel.DataLeftDivergenceBetweenHistograms2 = this.dataLeftDivergenceBetweenHistograms2.Text;
            //shortFormReportModel.DataRightDivergenceBetweenHistograms2 = this.dataRightDivergenceBetweenHistograms2.Text;
            shortFormReportModel.DataDivergenceBetweenHistograms3 = this.dataDivergenceBetweenHistograms3.SelectedIndex.ToString();
            shortFormReportModel.DataLeftDivergenceBetweenHistograms3 = this.dataLeftDivergenceBetweenHistograms3.Text;
            //shortFormReportModel.DataRightDivergenceBetweenHistograms3 = this.dataRightDivergenceBetweenHistograms3.Text;

            shortFormReportModel.DataLeftComparisonWithNorm = this.dataLeftComparisonWithNorm.Text;
            shortFormReportModel.DataRightComparisonWithNorm = this.dataRightComparisonWithNorm.Text;

            //shortFormReportModel.DataPhaseElectricalConductivityReference = this.dataPhaseElectricalConductivityReference.Text;
            shortFormReportModel.DataLeftPhaseElectricalConductivity = this.dataLeftPhaseElectricalConductivity.Text;
            shortFormReportModel.DataRightPhaseElectricalConductivity = this.dataRightPhaseElectricalConductivity.Text;

            shortFormReportModel.DataAgeElectricalConductivityReference = this.dataAgeElectricalConductivityReference.Text;
            shortFormReportModel.DataLeftAgeElectricalConductivity = this.dataLeftAgeElectricalConductivity.SelectedIndex.ToString();
            shortFormReportModel.DataRightAgeElectricalConductivity = this.dataRightAgeElectricalConductivity.SelectedIndex.ToString();
            shortFormReportModel.DataAgeValueOfEC = this.dataAgeValueOfEC.SelectedIndex.ToString();

            shortFormReportModel.DataExamConclusion = this.dataExamConclusion.SelectedIndex.ToString();
            shortFormReportModel.DataLeftMammaryGland = this.dataLeftMammaryGland.SelectedIndex.ToString();
            shortFormReportModel.DataLeftAgeRelated = this.dataLeftAgeRelated.SelectedIndex.ToString();
            //shortFormReportModel.DataLeftMeanECOfLesion = this.dataLeftMeanECOfLesion.Text;
            //shortFormReportModel.DataLeftFindings = this.dataLeftFindings.SelectedIndex.ToString();
            //shortFormReportModel.DataLeftMammaryGlandResult = this.dataLeftMammaryGlandResult.SelectedIndex.ToString();

            shortFormReportModel.DataRightMammaryGland = this.dataRightMammaryGland.SelectedIndex.ToString();
            shortFormReportModel.DataRightAgeRelated = this.dataRightAgeRelated.SelectedIndex.ToString();
            //shortFormReportModel.DataRightMeanECOfLesion = this.dataRightMeanECOfLesion.Text;
            //shortFormReportModel.DataRightFindings = this.dataRightFindings.SelectedIndex.ToString();
            //shortFormReportModel.DataRightMammaryGlandResult = this.dataRightMammaryGlandResult.SelectedIndex.ToString();

            shortFormReportModel.DataTotalPts = this.dataTotalPts.SelectedIndex.ToString();
            //shortFormReportModel.DataPoint = this.dataPoint.SelectedIndex.ToString();

            shortFormReportModel.DataBiRadsCategory = this.dataBiRadsCategory.SelectedIndex.ToString();
            shortFormReportModel.DataRecommendation = this.dataRecommendation.SelectedIndex.ToString();
            shortFormReportModel.DataComments = this.dataComments.Text;
            shortFormReportModel.DataConclusion = this.dataConclusion.SelectedIndex.ToString();
            shortFormReportModel.DataFurtherExam = this.dataFurtherExam.SelectedIndex.ToString();
            //shortFormReportModel.DataConclusion2 = this.dataConclusion2.Text;
            if (this.dataDoctor.SelectedItem != null)
            {
                var doctor = this.dataDoctor.SelectedItem as User;
                shortFormReportModel.DataDoctor = doctor.Name;
                shortFormReportModel.DataDoctorLicense = doctor.License;
            }
            
            var signImg = this.dataSignImg.Source as BitmapImage;
            if (signImg != null)
            {
                var stream = signImg.StreamSource;
                stream.Position = 0;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Flush();
                //stream.Close();
                shortFormReportModel.DataSignImg = buffer;
            }

            
                        
        }

        /// <summary>
        /// 克隆数据模型对象
        /// </summary>
        private ShortFormReport CloneReportModel()
        {
            ShortFormReport reportModel = shortFormReportModel.Clone();            
            reportModel = shortFormReportModel.Clone();
            //reportModel.DataGender = this.dataGender.Text;
            
            //reportModel.DataWeightUnit = this.dataWeightUnit.Text;
            reportModel.DataMenstrualCycle = this.dataMenstrualCycle.Text;
            reportModel.DataSkinAffections = this.dataSkinAffections.Text;
            //reportModel.DataPertinentHistory = this.dataPertinentHistory.Text;
            //reportModel.DataMotherUltra = this.dataMotherUltra.Text;
            //reportModel.DataLeftPalpableMass = this.dataLeftPalpableMass.Text;
            //reportModel.DataRightPalpableMass = this.dataRightPalpableMass.Text;

            reportModel.DataLeftBreastAP = this.dataLeftBreastAP.Text;
            reportModel.DataRightBreastAP = this.dataRightBreastAP.Text;
            reportModel.DataLeftBreast = this.dataLeftBreastH.Text + ":" + this.dataLeftBreastM.Text + this.dataLeftBreastAP.Text;
            reportModel.DataRightBreast = this.dataLeftBreastH.Text + ":" + this.dataRightBreastM.Text + this.dataRightBreastAP.Text;

            reportModel.DataLeftChangesOfElectricalConductivity = this.dataLeftChangesOfElectricalConductivity.Text;
            reportModel.DataRightChangesOfElectricalConductivity = this.dataRightChangesOfElectricalConductivity.Text;
            reportModel.DataLeftMammaryStruct = this.dataLeftMammaryStruct.Text;
            reportModel.DataRightMammaryStruct = this.dataRightMammaryStruct.Text;
            reportModel.DataLeftLactiferousSinusZone = this.dataLeftLactiferousSinusZone.Text;
            reportModel.DataRightLactiferousSinusZone = this.dataRightLactiferousSinusZone.Text;
            reportModel.DataLeftMammaryContour = this.dataLeftMammaryContour.Text;
            reportModel.DataRightMammaryContour = this.dataRightMammaryContour.Text;
            reportModel.DataLeftNumber = this.dataLeftNumber.Text;
            reportModel.DataRightNumber = this.dataRightNumber.Text;

            reportModel.DataLeftLocation = this.dataLeftLocation.Text;
            reportModel.DataRightLocation = this.dataRightLocation.Text;
            reportModel.DataLeftShape = this.dataLeftShape.Text;
            reportModel.DataRightShape = this.dataRightShape.Text;
            reportModel.DataLeftContourAroundFocus = this.dataLeftContourAroundFocus.Text;
            reportModel.DataRightContourAroundFocus = this.dataRightContourAroundFocus.Text;
            reportModel.DataLeftInternalElectricalStructure = this.dataLeftInternalElectricalStructure.Text;
            reportModel.DataRightInternalElectricalStructure = this.dataRightInternalElectricalStructure.Text;
            reportModel.DataLeftSurroundingTissues = this.dataLeftSurroundingTissues.Text;
            reportModel.DataRightSurroundingTissues = this.dataRightSurroundingTissues.Text;
            
            reportModel.DataLeftOncomarkerHighlightBenignChanges = this.dataLeftOncomarkerHighlightBenignChanges.Text;
            reportModel.DataRightOncomarkerHighlightBenignChanges = this.dataRightOncomarkerHighlightBenignChanges.Text;
            reportModel.DataLeftOncomarkerHighlightSuspiciousChanges = this.dataLeftOncomarkerHighlightSuspiciousChanges.Text;
            reportModel.DataRightOncomarkerHighlightSuspiciousChanges = this.dataRightOncomarkerHighlightSuspiciousChanges.Text;
            reportModel.DataMeanElectricalConductivity3 = this.dataMeanElectricalConductivity3.Text;
            reportModel.DataComparativeElectricalConductivity3 = this.dataComparativeElectricalConductivity3.Text;
            reportModel.DataDivergenceBetweenHistograms3 = this.dataDivergenceBetweenHistograms3.Text;
            reportModel.DataLeftAgeElectricalConductivity = this.dataLeftAgeElectricalConductivity.Text;
            reportModel.DataRightAgeElectricalConductivity = this.dataRightAgeElectricalConductivity.Text;
            reportModel.DataAgeValueOfEC = this.dataAgeValueOfEC.Text;
            reportModel.DataExamConclusion = this.dataExamConclusion.Text;
            reportModel.DataLeftMammaryGland = this.dataLeftMammaryGland.Text;
            reportModel.DataLeftAgeRelated = this.dataLeftAgeRelated.Text;
            //reportModel.DataLeftFindings = this.dataLeftFindings.Text;
            //reportModel.DataLeftMammaryGlandResult = this.dataLeftMammaryGlandResult.Text;
            reportModel.DataRightMammaryGland = this.dataRightMammaryGland.Text;
            reportModel.DataRightAgeRelated = this.dataRightAgeRelated.Text;
            //reportModel.DataRightFindings = this.dataRightFindings.Text;
            //reportModel.DataRightMammaryGlandResult = this.dataRightMammaryGlandResult.Text;
            reportModel.DataTotalPts = this.dataTotalPts.Text;
            //reportModel.DataLeftTotalPts = this.dataLeftTotalPts.Text;
            //reportModel.DataRightTotalPts = this.dataRightTotalPts.Text;
            //reportModel.DataPoint = this.dataPoint.Text;
            reportModel.DataBiRadsCategory = this.dataBiRadsCategory.Text;
            //reportModel.DataLeftBiRadsCategory = this.dataLeftBiRadsCategory.Text;
            //reportModel.DataRightBiRadsCategory = this.dataRightBiRadsCategory.Text;
            reportModel.DataRecommendation = this.dataRecommendation.Text;
            reportModel.DataConclusion = this.dataConclusion.Text;
            reportModel.DataFurtherExam = this.dataFurtherExam.Text;
                        

            return reportModel;            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, App.Current.FindResource("Message_4").ToString(), "Save Report", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                SaveReport(null);
            }
        }

        public void ShowSignature(Object obj)
        {
            dataSignImg.Source = ImageTools.GetBitmapImage(AppDomain.CurrentDomain.BaseDirectory + "Signature" + System.IO.Path.DirectorySeparatorChar + "temp.jpg");
        }

        private void savePdfBtn_Click(object sender, RoutedEventArgs e)
        {
            try { 
                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                    FileHelper.SetFolderPower(dataFolder, "Everyone", "FullControl");
                    FileHelper.SetFolderPower(dataFolder, "Users", "FullControl");
                }

                LoadDataModel();                
                //Clone生成全文本的报表数据对象模型
                var reportModel = CloneReportModel();
                //打开文件夹对话框，选择要保存的目录
                System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                folderBrowserDialog.SelectedPath = this.person.ArchiveFolder;
                System.Windows.Forms.DialogResult res = folderBrowserDialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {                    
                    string folderName = folderBrowserDialog.SelectedPath;
                    string strName = person.SurName + (string.IsNullOrEmpty(person.GivenName) ? "" : "," + person.GivenName) + (string.IsNullOrEmpty(person.OtherName) ? "" : " " + person.OtherName)+".pdf";  
                    //生成Examination报告的PDF文件
                    string lfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + " LF - " + strName;
                    string lfReportTempl = "Views/ExaminationReportFlow.xaml";
                    ExportFlowDocumentPDF(lfReportTempl, lfPdfFile, reportModel);

                    //生成Summary报告的PDF文件
                    string sfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + "SF - " + strName;
                    string sfReportTempl = "Views/SummaryReportDocument.xaml";
                    if (shortFormReportModel.DataScreenShotImg != null)
                    {
                        sfReportTempl = "Views/SummaryReportNuvoTekDocument.xaml";
                    }
                    ExportPDF(sfReportTempl, sfPdfFile, reportModel);                    

                    MessageBox.Show(this, App.Current.FindResource("Message_5").ToString());
                }
            }
            catch (Exception ex)
            {
                FileHelper.SetFolderPower(dataFolder, "Everyone", "FullControl");
                FileHelper.SetFolderPower(dataFolder, "Users", "FullControl");
                MessageBox.Show(this, ex.Message);
            }
        }

        /// <summary>
        ///针对FixedPage对象生成PDF 
        /// </summary>
        /// <param name="reportTempl"></param>
        /// <param name="pdfFile"></param>
        /// <param name="reportModel"></param>
        private void ExportPDF(string reportTempl,string pdfFile,ShortFormReport reportModel)
        {
            string xpsFile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".xps";
            //string userTempPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            //string xpsFile = userTempPath + System.IO.Path.DirectorySeparatorChar + person.Code + ".xps";
            if (File.Exists(xpsFile))
            {
                File.Delete(xpsFile);
            }
            
            FixedPage page = (FixedPage)PrintPreviewWindow.LoadFixedDocumentAndRender(reportTempl, reportModel);
            
            XpsDocument xpsDocument = new XpsDocument(xpsFile, FileAccess.ReadWrite);
            //将flow document写入基于内存的xps document中去
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            writer.Write(page);
            xpsDocument.Close();
            if (File.Exists(pdfFile))
            {
                File.Delete(pdfFile);
            }
            PDFTools.SavePDFFile(xpsFile, pdfFile);            
        }

        /// <summary>
        /// 针对FLowDocument对象生成PDF文件
        /// </summary>
        /// <param name="reportTempl"></param>
        /// <param name="pdfFile"></param>
        /// <param name="reportModel"></param>
        private void ExportFlowDocumentPDF(string reportTempl, string pdfFile, ShortFormReport reportModel,string pagesize=null)
        {
            string xpsFile = dataFolder + System.IO.Path.DirectorySeparatorChar + person.Code + ".xps";
            if (File.Exists(xpsFile))
            {
                File.Delete(xpsFile);
            }

            FlowDocument page = (FlowDocument)PrintPreviewWindow.LoadFlowDocumentAndRender(reportTempl, reportModel,pagesize);
            try
            {
                LoadDataForFlowDocument(page, reportModel);
            }
            catch { }
            XpsDocument xpsDocument = new XpsDocument(xpsFile, FileAccess.ReadWrite);
            //将flow document写入基于内存的xps document中去
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            writer.Write(((IDocumentPaginatorSource)page).DocumentPaginator);
            xpsDocument.Close();
            if (File.Exists(pdfFile))
            {
                File.Delete(pdfFile);
            }
            PDFTools.SavePDFFile(xpsFile, pdfFile);
        }

        private void LoadDataForFlowDocument(FlowDocument page, ShortFormReport reportModel)
        {
            if (reportModel != null)
            {
                var textBlock1 = page.FindName("dataClientNum") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataClientNum) ? "N/A" : reportModel.DataClientNum;
                }
                textBlock1 = page.FindName("dataUserCode") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataUserCode;
                }
                textBlock1 = page.FindName("dataScreenDate") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataScreenDate;
                }
                textBlock1 = page.FindName("dataName") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataName;
                }
                textBlock1 = page.FindName("dataAge") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataAge;
                }
                textBlock1 = page.FindName("dataHeight") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataHeight;
                }
                textBlock1 = page.FindName("dataWeight") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataWeight;
                }
                textBlock1 = page.FindName("dataScreenLocation") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataScreenLocation;
                }
                textBlock1 = page.FindName("dataMobile") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataMobile;
                }
                textBlock1 = page.FindName("dataEmail") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataEmail;
                }
                textBlock1 = page.FindName("dataLeftBreast") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftBreast;
                }
                textBlock1 = page.FindName("dataRightBreast") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightBreast;
                }
                textBlock1 = page.FindName("dataLeftPalpableMass") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftPalpableMass;
                }
                textBlock1 = page.FindName("dataRightPalpableMass") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightPalpableMass;
                }
                textBlock1 = page.FindName("dataLeftChangesOfElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftChangesOfElectricalConductivity;
                }
                textBlock1 = page.FindName("dataRightChangesOfElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightChangesOfElectricalConductivity;
                }
                textBlock1 = page.FindName("dataLeftMammaryStruct") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMammaryStruct;
                }
                textBlock1 = page.FindName("dataRightMammaryStruct") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMammaryStruct;
                }
                textBlock1 = page.FindName("dataLeftLactiferousSinusZone") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftLactiferousSinusZone;
                }
                textBlock1 = page.FindName("dataRightLactiferousSinusZone") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightLactiferousSinusZone;
                }
                textBlock1 = page.FindName("dataLeftMammaryContour") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMammaryContour;
                }
                textBlock1 = page.FindName("dataRightMammaryContour") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMammaryContour;
                }
                textBlock1 = page.FindName("dataLeftNumber") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftNumber;
                }
                textBlock1 = page.FindName("dataRightNumber") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightNumber;
                }
                textBlock1 = page.FindName("dataLeftLocation") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftLocation) ? "Nil" : reportModel.DataLeftLocation;
                }
                textBlock1 = page.FindName("dataRightLocation") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightLocation) ? "Nil" : reportModel.DataRightLocation;
                }
                textBlock1 = page.FindName("dataLeftSize") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSize) ? "Nil" : reportModel.DataLeftSize;
                }
                textBlock1 = page.FindName("dataRightSize") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSize) ? "Nil" : reportModel.DataRightSize;
                }
                textBlock1 = page.FindName("dataLeftShape") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftShape) ? "Nil" : reportModel.DataLeftShape;
                }
                textBlock1 = page.FindName("dataRightShape") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightShape) ? "Nil" : reportModel.DataRightShape;
                }
                textBlock1 = page.FindName("dataLeftContourAroundFocus") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftContourAroundFocus) ? "Nil" : reportModel.DataLeftContourAroundFocus;                    
                }
                textBlock1 = page.FindName("dataRightContourAroundFocus") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightContourAroundFocus) ? "Nil" : reportModel.DataRightContourAroundFocus;                    
                }
                textBlock1 = page.FindName("dataLeftInternalElectricalStructure") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftInternalElectricalStructure) ? "Nil" : reportModel.DataLeftInternalElectricalStructure;                    
                }
                textBlock1 = page.FindName("dataRightInternalElectricalStructure") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightInternalElectricalStructure) ? "Nil" : reportModel.DataRightInternalElectricalStructure;                    
                }
                textBlock1 = page.FindName("dataLeftSurroundingTissues") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSurroundingTissues) ? "Nil" : reportModel.DataLeftSurroundingTissues;                    
                }
                textBlock1 = page.FindName("dataRightSurroundingTissues") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSurroundingTissues) ? "Nil" : reportModel.DataRightSurroundingTissues;                    
                }
                textBlock1 = page.FindName("dataLeftLocation2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftLocation2) ? "Nil" : reportModel.DataLeftLocation2;                    
                }
                textBlock1 = page.FindName("dataRightLocation2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightLocation2) ? "Nil" : reportModel.DataRightLocation2;                    
                }
                textBlock1 = page.FindName("dataLeftSize2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSize2) ? "Nil" : reportModel.DataLeftSize2;                    
                }
                textBlock1 = page.FindName("dataRightSize2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSize2) ? "Nil" : reportModel.DataRightSize2;                    
                }
                textBlock1 = page.FindName("dataLeftShape2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftShape2) ? "Nil" : reportModel.DataLeftShape2;                    
                }
                textBlock1 = page.FindName("dataRightShape2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightShape2) ? "Nil" : reportModel.DataRightShape2;                    
                }
                textBlock1 = page.FindName("dataLeftContourAroundFocus2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftContourAroundFocus2) ? "Nil" : reportModel.DataLeftContourAroundFocus2;                    
                }
                textBlock1 = page.FindName("dataRightContourAroundFocus2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightContourAroundFocus2) ? "Nil" : reportModel.DataRightContourAroundFocus2;                    
                }
                textBlock1 = page.FindName("dataLeftInternalElectricalStructure2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftInternalElectricalStructure2) ? "Nil" : reportModel.DataLeftInternalElectricalStructure2;                    
                }
                textBlock1 = page.FindName("dataRightInternalElectricalStructure2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightInternalElectricalStructure2) ? "Nil" : reportModel.DataRightInternalElectricalStructure2;                    
                }
                textBlock1 = page.FindName("dataLeftSurroundingTissues2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSurroundingTissues2) ? "Nil" : reportModel.DataLeftSurroundingTissues2;                    
                }
                textBlock1 = page.FindName("dataRightSurroundingTissues2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSurroundingTissues2) ? "Nil" : reportModel.DataRightSurroundingTissues2;                    
                }
                textBlock1 = page.FindName("dataLeftLocation3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftLocation3) ? "Nil" : reportModel.DataLeftLocation3;                    
                }
                textBlock1 = page.FindName("dataRightLocation3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightLocation3) ? "Nil" : reportModel.DataRightLocation3;                    
                }
                textBlock1 = page.FindName("dataLeftSize3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSize3) ? "Nil" : reportModel.DataLeftSize3;                    
                }
                textBlock1 = page.FindName("dataRightSize3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSize3) ? "Nil" : reportModel.DataRightSize3;                    
                }
                textBlock1 = page.FindName("dataLeftShape3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftShape3) ? "Nil" : reportModel.DataLeftShape3;                    
                }
                textBlock1 = page.FindName("dataRightShape3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightShape3) ? "Nil" : reportModel.DataRightShape3;                    
                }
                textBlock1 = page.FindName("dataLeftContourAroundFocus3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftContourAroundFocus3) ? "Nil" : reportModel.DataLeftContourAroundFocus3;                    
                }
                textBlock1 = page.FindName("dataRightContourAroundFocus3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightContourAroundFocus3) ? "Nil" : reportModel.DataRightContourAroundFocus3;                    
                }
                textBlock1 = page.FindName("dataLeftInternalElectricalStructure3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftInternalElectricalStructure3) ? "Nil" : reportModel.DataLeftInternalElectricalStructure3;                    
                }
                textBlock1 = page.FindName("dataRightInternalElectricalStructure3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightInternalElectricalStructure3) ? "Nil" : reportModel.DataRightInternalElectricalStructure3;                    
                }
                textBlock1 = page.FindName("dataLeftSurroundingTissues3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataLeftSurroundingTissues3) ? "Nil" : reportModel.DataLeftSurroundingTissues3;                    
                }
                textBlock1 = page.FindName("dataRightSurroundingTissues3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataRightSurroundingTissues3) ? "Nil" : reportModel.DataRightSurroundingTissues3;                    
                }
                textBlock1 = page.FindName("dataLeftOncomarkerHighlightBenignChanges") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftOncomarkerHighlightBenignChanges;
                }
                textBlock1 = page.FindName("dataRightOncomarkerHighlightBenignChanges") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightOncomarkerHighlightBenignChanges;
                }
                textBlock1 = page.FindName("dataLeftOncomarkerHighlightSuspiciousChanges") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftOncomarkerHighlightSuspiciousChanges;
                }
                textBlock1 = page.FindName("dataRightOncomarkerHighlightSuspiciousChanges") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightOncomarkerHighlightSuspiciousChanges;
                }
                textBlock1 = page.FindName("dataLeftMeanElectricalConductivity1") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMeanElectricalConductivity1;
                }
                textBlock1 = page.FindName("dataRightMeanElectricalConductivity1") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMeanElectricalConductivity1;
                }
                textBlock1 = page.FindName("dataLeftMeanElectricalConductivity2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMeanElectricalConductivity2;
                }
                textBlock1 = page.FindName("dataRightMeanElectricalConductivity2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMeanElectricalConductivity2;
                }
                textBlock1 = page.FindName("dataMeanElectricalConductivity3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataMeanElectricalConductivity3;
                }
                textBlock1 = page.FindName("dataLeftMeanElectricalConductivity3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMeanElectricalConductivity3;
                }
                textBlock1 = page.FindName("dataRightMeanElectricalConductivity3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMeanElectricalConductivity3;
                }
                textBlock1 = page.FindName("dataLeftComparativeElectricalConductivity1") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftComparativeElectricalConductivity1;
                }
                textBlock1 = page.FindName("dataLeftComparativeElectricalConductivity2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftComparativeElectricalConductivity2;
                }
                textBlock1 = page.FindName("dataComparativeElectricalConductivity3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataComparativeElectricalConductivity3;
                }
                textBlock1 = page.FindName("dataLeftComparativeElectricalConductivity3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftComparativeElectricalConductivity3;
                }
                textBlock1 = page.FindName("dataLeftDivergenceBetweenHistograms1") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftDivergenceBetweenHistograms1;
                }
                textBlock1 = page.FindName("dataLeftDivergenceBetweenHistograms2") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftDivergenceBetweenHistograms2;
                }
                textBlock1 = page.FindName("dataDivergenceBetweenHistograms3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataDivergenceBetweenHistograms3;
                }
                textBlock1 = page.FindName("dataLeftDivergenceBetweenHistograms3") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftDivergenceBetweenHistograms3;
                }
                textBlock1 = page.FindName("dataLeftComparisonWithNorm") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftComparisonWithNorm;
                }
                textBlock1 = page.FindName("dataRightComparisonWithNorm") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightComparisonWithNorm;
                }
                textBlock1 = page.FindName("dataLeftPhaseElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftPhaseElectricalConductivity;
                }
                textBlock1 = page.FindName("dataRightPhaseElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightPhaseElectricalConductivity;
                }
                textBlock1 = page.FindName("dataAgeElectricalConductivityReference") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataAgeElectricalConductivityReference;
                }
                textBlock1 = page.FindName("dataLeftAgeElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftAgeElectricalConductivity;
                }
                textBlock1 = page.FindName("dataRightAgeElectricalConductivity") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightAgeElectricalConductivity;
                }
                textBlock1 = page.FindName("dataExamConclusion") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataExamConclusion;
                }
                textBlock1 = page.FindName("dataLeftMammaryGland") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMammaryGland;
                }

                textBlock1 = page.FindName("dataLeftMeanECOfLesion") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMeanECOfLesion;
                }
                
                textBlock1 = page.FindName("dataLeftAgeRelated") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftAgeRelated;
                }
                
                textBlock1 = page.FindName("dataLeftMammaryGlandResult") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftMammaryGlandResult;
                }
                textBlock1 = page.FindName("dataRightMammaryGland") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMammaryGland;
                }

                textBlock1 = page.FindName("dataRightMeanECOfLesion") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMeanECOfLesion;
                }
                
                textBlock1 = page.FindName("dataRightAgeRelated") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightAgeRelated;
                }
                
                textBlock1 = page.FindName("dataRightMammaryGlandResult") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightMammaryGlandResult;
                }
                textBlock1 = page.FindName("dataTotalPts") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataTotalPts;
                }
                textBlock1 = page.FindName("dataLeftTotalPts") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftTotalPts;
                }
                textBlock1 = page.FindName("dataRightTotalPts") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightTotalPts;
                }
                textBlock1 = page.FindName("dataPoint") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataPoint;
                }
                textBlock1 = page.FindName("dataBiRadsCategory") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataBiRadsCategory;
                }
                textBlock1 = page.FindName("dataLeftBiRadsCategory") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataLeftBiRadsCategory;
                }
                textBlock1 = page.FindName("dataRightBiRadsCategory") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRightBiRadsCategory;
                }
                textBlock1 = page.FindName("dataRecommendation") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataRecommendation;
                }
                textBlock1 = page.FindName("dataFurtherExam") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataFurtherExam) ? "N/A" : reportModel.DataFurtherExam;
                }
                textBlock1 = page.FindName("dataConclusion") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataConclusion;
                }
                textBlock1 = page.FindName("dataComments") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = string.IsNullOrEmpty(reportModel.DataComments) ? "N/A" : reportModel.DataComments;
                }
                textBlock1 = page.FindName("dataMeikTech") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataMeikTech;
                }
                textBlock1 = page.FindName("dataDoctor") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataDoctor;
                }
                textBlock1 = page.FindName("dataSignDate") as TextBlock;
                if (textBlock1 != null)
                {
                    textBlock1.Text = reportModel.DataSignDate;
                }            

                if(!string.IsNullOrEmpty(reportModel.DataLeftLocation)&&page.FindName("L1_Canvas")!=null){

                    if (reportModel.DataLeftLocation.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("L1_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("L1_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("L1_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("L1_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("L1_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("L1_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("L1_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("L1_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("L1_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("L1_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("L1_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("L1_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("L1".Equals(reportModel.DataLeftMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}
                }                

                if (!string.IsNullOrEmpty(reportModel.DataRightLocation) && page.FindName("L1_Canvas") != null)
                {
                    if (reportModel.DataRightLocation.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("R1_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("R1_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("R1_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("R1_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("R1_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("R1_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("R1_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("R1_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("R1_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("R1_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("R1_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("R1_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("R1".Equals(reportModel.DataRightMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }                

                if (!string.IsNullOrEmpty(reportModel.DataLeftLocation2) && page.FindName("L1_Canvas") != null)
                {
                    if (reportModel.DataLeftLocation2.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("L2_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("L2_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("L2_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("L2_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("L2_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("L2_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("L2_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("L2_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("L2_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("L2_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("L2_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation2.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("L2_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("L2".Equals(reportModel.DataLeftMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }                

                if (!string.IsNullOrEmpty(reportModel.DataRightLocation2) && page.FindName("L1_Canvas") != null)
                {
                    if (reportModel.DataRightLocation2.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("R2_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("R2_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("R2_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("R2_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("R2_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("R2_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("R2_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("R2_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("R2_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("R2_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("R2_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation2.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("R2_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("R2".Equals(reportModel.DataRightMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }                

                if (!string.IsNullOrEmpty(reportModel.DataLeftLocation3) && page.FindName("L1_Canvas") != null)
                {
                    if (reportModel.DataLeftLocation3.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("L3_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("L3_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("L3_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("L3_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("L3_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("L3_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("L3_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("L3_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("L3_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("L3_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("L3_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataLeftLocation3.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("L3_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("L3".Equals(reportModel.DataLeftMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}
                }
                

                if (!string.IsNullOrEmpty(reportModel.DataRightLocation3) &&page.FindName("L1_Canvas") != null)
                {
                    if (reportModel.DataRightLocation3.StartsWith("12"))
                    {
                        textBlock1 = page.FindName("R3_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("11"))
                    {
                        textBlock1 = page.FindName("R3_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("10"))
                    {
                        textBlock1 = page.FindName("R3_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("9"))
                    {
                        textBlock1 = page.FindName("R3_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("8"))
                    {
                        textBlock1 = page.FindName("R3_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("7"))
                    {
                        textBlock1 = page.FindName("R3_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("6"))
                    {
                        textBlock1 = page.FindName("R3_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("5"))
                    {
                        textBlock1 = page.FindName("R3_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("4"))
                    {
                        textBlock1 = page.FindName("R3_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("3"))
                    {
                        textBlock1 = page.FindName("R3_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("2"))
                    {
                        textBlock1 = page.FindName("R3_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (reportModel.DataRightLocation3.StartsWith("1"))
                    {
                        textBlock1 = page.FindName("R3_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("R3".Equals(reportModel.DataRightMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }                
            }
        }

        private void saveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.ShowDialog();
            string folderName = folderBrowserDialog.SelectedPath;
            if (!string.IsNullOrEmpty(folderName))
            {
                SaveReport(folderName);
                MessageBox.Show(this, App.Current.FindResource("Message_6").ToString());
            }

        }

        private string FindUserReportData(string folderName)
        {
            //遍历指定文件夹下所有文件
            DirectoryInfo theFolder = new DirectoryInfo(folderName);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            List<DirectoryInfo> list = dirInfo.ToList();
            list.Add(theFolder);
            dirInfo = list.ToArray();
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                FileInfo[] fileInfo = null;
                try
                {
                    fileInfo = NextFolder.GetFiles();
                    //遍历文件
                    foreach (FileInfo NextFile in fileInfo)
                    {
                        if ((person.Code + ".dat").Equals(NextFile.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            return NextFile.FullName;
                        }
                    }
                }
                catch (Exception) { }
            }
            return null;
        }

        private string FindUserReportWord(string folderName)
        {
            //遍历指定文件夹下所有文件
            DirectoryInfo theFolder = new DirectoryInfo(folderName);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            List<DirectoryInfo> list = dirInfo.ToList();
            list.Add(theFolder);
            dirInfo = list.ToArray();
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                FileInfo[] fileInfo = null;
                try
                {
                    fileInfo = NextFolder.GetFiles();
                    //遍历文件
                    foreach (FileInfo NextFile in fileInfo)
                    {                        
                        if ((person.Code + ".xml").Equals(NextFile.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            return NextFile.FullName;
                        }
                    }
                }
                catch (Exception) { }
            }
            return null;
        }

        private void btnScreenShot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Owner.Visibility = Visibility.Hidden;
                this.WindowState = WindowState.Minimized;
                App.opendWin = this;
                ScreenCapture screenCaptureWin = new ScreenCapture(this.person);
                screenCaptureWin.callbackMethod = LoadScreenShot;
                screenCaptureWin.ShowDialog();               
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "System Exception: " + ex.Message);
            } 
        }

        private void LoadScreenShot(Object imgFileName)
        {
            var screenShotImg = ImageTools.GetBitmapImage(imgFileName as string);
            if (screenShotImg != null)
            {
                var stream = screenShotImg.StreamSource;
                stream.Position = 0;
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Flush();
                shortFormReportModel.DataScreenShotImg = buffer;

                //this.btnScreenShot.Content = App.Current.FindResource("ReportContext_170").ToString();                
                //this.btnRemoveImg.Visibility = Visibility.Visible;
            }
           
            //dataScreenShotImg.Source = ImageTools.GetBitmapImage(imgFileName as string);
        }

        private void btnViewImg_Click(object sender, RoutedEventArgs e)
        {            
            if (shortFormReportModel.DataScreenShotImg == null)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_34").ToString());
            }
            else
            {
                ViewImagePage viewImagePage = new ViewImagePage(shortFormReportModel.DataScreenShotImg);
                viewImagePage.ShowDialog();
            }
        }

        //private void btnRemoveImg_Click(object sender, RoutedEventArgs e)
        //{
        //    this.btnScreenShot.Content = App.Current.FindResource("ReportContext_175").ToString();            
        //    shortFormReportModel.DataScreenShotImg = null;
        //    this.btnRemoveImg.Visibility = Visibility.Hidden;
        //}

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //判断是否已经从MEIK生成的DOC文档中导入检查数据，如果之前没有，则查找是否已在本地生成DOC文档，导入数据            
            string docFile = FindUserReportWord(person.ArchiveFolder);
            if (!string.IsNullOrEmpty(docFile) &&File.Exists(docFile))
            {
                //ShortFormReport shortFormReport = WordTools.ReadWordFile(docFile);
                ShortFormReport shortFormReport = XmlTools.ReadXmlFile(docFile);
                if (shortFormReport != null)
                {
                    try
                    {
                        /**因为ShortFormReport的属性没有添加依赖变更事件，所以这里为shortFormReportModel赋值不会影响页面显示效果。
                         * 不过即使为ShortFormReport的属性添加上依赖变更事件，但由于之前用户可能已经序列化过一些报表数据，
                         * 如果改变ShortFormReport对象的继承定义，会导致反序列化失败，所以只能暂时不处理，只在这里强制修改页面元素
                         * **/
                        shortFormReportModel.DataMenstrualCycle = shortFormReport.DataMenstrualCycle;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataMenstrualCycle))
                        {
                            this.dataMenstrualCycle.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataMenstrualCycle);
                        }
                        shortFormReportModel.DataHormones = shortFormReport.DataHormones;
                        this.dataHormones.Text = shortFormReportModel.DataHormones;
                        shortFormReportModel.DataSkinAffections = shortFormReport.DataSkinAffections;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataSkinAffections))
                        {
                            this.dataSkinAffections.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataSkinAffections);
                        }
                        shortFormReportModel.DataLeftChangesOfElectricalConductivity = shortFormReport.DataLeftChangesOfElectricalConductivity;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftChangesOfElectricalConductivity))
                        {
                            this.dataLeftChangesOfElectricalConductivity.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftChangesOfElectricalConductivity);
                        }
                        shortFormReportModel.DataRightChangesOfElectricalConductivity = shortFormReport.DataRightChangesOfElectricalConductivity;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightChangesOfElectricalConductivity))
                        {
                            this.dataRightChangesOfElectricalConductivity.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightChangesOfElectricalConductivity);
                        }

                        shortFormReportModel.DataLeftMammaryStruct = shortFormReport.DataLeftMammaryStruct;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMammaryStruct))
                        {
                            this.dataLeftMammaryStruct.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftMammaryStruct);
                        }
                        shortFormReportModel.DataRightMammaryStruct = shortFormReport.DataRightMammaryStruct;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMammaryStruct))
                        {
                            this.dataRightMammaryStruct.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightMammaryStruct);
                        }
                        shortFormReportModel.DataLeftLactiferousSinusZone = shortFormReport.DataLeftLactiferousSinusZone;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftLactiferousSinusZone))
                        {
                            this.dataLeftLactiferousSinusZone.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftLactiferousSinusZone);
                        }
                        shortFormReportModel.DataRightLactiferousSinusZone = shortFormReport.DataRightLactiferousSinusZone;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightLactiferousSinusZone))
                        {
                            this.dataRightLactiferousSinusZone.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightLactiferousSinusZone);
                        }
                        shortFormReportModel.DataLeftMammaryContour = shortFormReport.DataLeftMammaryContour;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMammaryContour))
                        {
                            this.dataLeftMammaryContour.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftMammaryContour);
                        }
                        shortFormReportModel.DataRightMammaryContour = shortFormReport.DataRightMammaryContour;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMammaryContour))
                        {
                            this.dataRightMammaryContour.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightMammaryContour);
                        }

                        shortFormReportModel.DataLeftNumber = shortFormReport.DataLeftNumber;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftNumber))
                        {
                            this.dataLeftNumber.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftNumber);
                        }
                        shortFormReportModel.DataRightNumber = shortFormReport.DataRightNumber;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightNumber))
                        {
                            this.dataRightNumber.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightNumber);
                        }

                        shortFormReportModel.DataLeftLocation = shortFormReport.DataLeftLocation;
                        this.dataLeftLocation.Text = shortFormReportModel.DataLeftLocation;
                        shortFormReportModel.DataRightLocation = shortFormReport.DataRightLocation;
                        this.dataRightLocation.Text = shortFormReportModel.DataRightLocation;
                        shortFormReportModel.DataLeftSize = shortFormReport.DataLeftSize;
                        this.dataLeftSize.Text = shortFormReportModel.DataLeftSize;
                        shortFormReportModel.DataRightSize = shortFormReport.DataRightSize;
                        this.dataRightSize.Text = shortFormReportModel.DataRightSize;

                        shortFormReportModel.DataLeftShape = shortFormReport.DataLeftShape;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftShape))
                        {
                            this.dataLeftShape.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftShape);
                        }
                        shortFormReportModel.DataRightShape = shortFormReport.DataRightShape;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightShape))
                        {
                            this.dataRightShape.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightShape);
                        }

                        shortFormReportModel.DataLeftContourAroundFocus = shortFormReport.DataLeftContourAroundFocus;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftContourAroundFocus))
                        {
                            this.dataLeftContourAroundFocus.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftContourAroundFocus);
                        }
                        shortFormReportModel.DataRightContourAroundFocus = shortFormReport.DataRightContourAroundFocus;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightContourAroundFocus))
                        {
                            this.dataRightContourAroundFocus.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightContourAroundFocus);
                        }
                        shortFormReportModel.DataLeftInternalElectricalStructure = shortFormReport.DataLeftInternalElectricalStructure;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftInternalElectricalStructure))
                        {
                            this.dataLeftInternalElectricalStructure.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftInternalElectricalStructure);
                        }
                        shortFormReportModel.DataRightInternalElectricalStructure = shortFormReport.DataRightInternalElectricalStructure;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightInternalElectricalStructure))
                        {
                            this.dataRightInternalElectricalStructure.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightInternalElectricalStructure);
                        }
                        shortFormReportModel.DataLeftSurroundingTissues = shortFormReport.DataLeftSurroundingTissues;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftSurroundingTissues))
                        {
                            this.dataLeftSurroundingTissues.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftSurroundingTissues);
                        }
                        shortFormReportModel.DataRightSurroundingTissues = shortFormReport.DataRightSurroundingTissues;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightSurroundingTissues))
                        {
                            this.dataRightSurroundingTissues.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightSurroundingTissues);
                        }

                        shortFormReportModel.DataLeftMeanElectricalConductivity1 = shortFormReport.DataLeftMeanElectricalConductivity1;
                        this.dataLeftMeanElectricalConductivity1.Text = shortFormReportModel.DataLeftMeanElectricalConductivity1;
                        shortFormReportModel.DataRightMeanElectricalConductivity1 = shortFormReport.DataRightMeanElectricalConductivity1;
                        this.dataRightMeanElectricalConductivity1.Text = shortFormReportModel.DataRightMeanElectricalConductivity1;
                        shortFormReportModel.DataLeftMeanElectricalConductivity2 = shortFormReport.DataLeftMeanElectricalConductivity2;
                        this.dataLeftMeanElectricalConductivity2.Text = shortFormReportModel.DataLeftMeanElectricalConductivity2;
                        shortFormReportModel.DataRightMeanElectricalConductivity2 = shortFormReport.DataRightMeanElectricalConductivity2;
                        this.dataRightMeanElectricalConductivity2.Text = shortFormReportModel.DataRightMeanElectricalConductivity2;
                        shortFormReportModel.DataMeanElectricalConductivity3 = shortFormReport.DataMeanElectricalConductivity3;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataMeanElectricalConductivity3))
                        {
                            this.dataMeanElectricalConductivity3.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataMeanElectricalConductivity3);
                        }
                        shortFormReportModel.DataLeftMeanElectricalConductivity3 = shortFormReport.DataLeftMeanElectricalConductivity3;
                        this.dataLeftMeanElectricalConductivity3.Text = shortFormReportModel.DataLeftMeanElectricalConductivity3;
                        shortFormReportModel.DataRightMeanElectricalConductivity3 = shortFormReport.DataRightMeanElectricalConductivity3;
                        this.dataRightMeanElectricalConductivity3.Text = shortFormReportModel.DataRightMeanElectricalConductivity3;
                        shortFormReportModel.DataLeftComparativeElectricalConductivity1 = shortFormReport.DataLeftComparativeElectricalConductivity1;
                        this.dataLeftComparativeElectricalConductivity1.Text = shortFormReportModel.DataLeftComparativeElectricalConductivity1;
                        shortFormReportModel.DataLeftComparativeElectricalConductivity2 = shortFormReport.DataLeftComparativeElectricalConductivity2;
                        this.dataLeftComparativeElectricalConductivity2.Text = shortFormReportModel.DataLeftComparativeElectricalConductivity2;
                        shortFormReportModel.DataLeftComparativeElectricalConductivity3 = shortFormReport.DataLeftComparativeElectricalConductivity3;
                        this.dataLeftComparativeElectricalConductivity3.Text = shortFormReportModel.DataLeftComparativeElectricalConductivity3;

                        shortFormReportModel.DataComparativeElectricalConductivity3 = shortFormReportModel.DataComparativeElectricalConductivity3;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataComparativeElectricalConductivity3))
                        {
                            this.dataComparativeElectricalConductivity3.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataComparativeElectricalConductivity3);
                        }

                        shortFormReportModel.DataDivergenceBetweenHistograms3 = shortFormReportModel.DataDivergenceBetweenHistograms3;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataDivergenceBetweenHistograms3))
                        {
                            this.dataDivergenceBetweenHistograms3.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataDivergenceBetweenHistograms3);
                        }

                        shortFormReportModel.DataLeftDivergenceBetweenHistograms1 = shortFormReport.DataLeftDivergenceBetweenHistograms1;
                        this.dataLeftDivergenceBetweenHistograms1.Text = shortFormReportModel.DataLeftDivergenceBetweenHistograms1;
                        shortFormReportModel.DataLeftDivergenceBetweenHistograms2 = shortFormReport.DataLeftDivergenceBetweenHistograms2; 
                        this.dataLeftDivergenceBetweenHistograms2.Text = shortFormReportModel.DataLeftDivergenceBetweenHistograms2;
                        shortFormReportModel.DataLeftDivergenceBetweenHistograms3 = shortFormReport.DataLeftDivergenceBetweenHistograms3; 
                        this.dataLeftDivergenceBetweenHistograms3.Text = shortFormReportModel.DataLeftDivergenceBetweenHistograms3;

                        shortFormReportModel.DataLeftPhaseElectricalConductivity = shortFormReport.DataLeftPhaseElectricalConductivity;
                        this.dataLeftPhaseElectricalConductivity.Text = shortFormReportModel.DataLeftPhaseElectricalConductivity;
                        shortFormReportModel.DataRightPhaseElectricalConductivity = shortFormReport.DataRightPhaseElectricalConductivity;
                        this.dataRightPhaseElectricalConductivity.Text = shortFormReportModel.DataRightPhaseElectricalConductivity;
                        shortFormReportModel.DataAgeElectricalConductivityReference = shortFormReport.DataAgeElectricalConductivityReference;
                        this.dataAgeElectricalConductivityReference.Text = shortFormReportModel.DataAgeElectricalConductivityReference;
                        shortFormReportModel.DataLeftAgeElectricalConductivity = shortFormReport.DataLeftAgeElectricalConductivity;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftAgeElectricalConductivity))
                        {
                            this.dataLeftAgeElectricalConductivity.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftAgeElectricalConductivity);
                        }
                        shortFormReportModel.DataRightAgeElectricalConductivity = shortFormReport.DataRightAgeElectricalConductivity;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightAgeElectricalConductivity))
                        {
                            this.dataRightAgeElectricalConductivity.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightAgeElectricalConductivity);
                        }
                        shortFormReportModel.DataExamConclusion = shortFormReport.DataExamConclusion;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataExamConclusion))
                        {
                            this.dataExamConclusion.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataExamConclusion);
                        }
                        shortFormReportModel.DataLeftMammaryGland = shortFormReport.DataLeftMammaryGland;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftMammaryGland))
                        {
                            this.dataLeftMammaryGland.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftMammaryGland);
                        }
                        shortFormReportModel.DataLeftAgeRelated = shortFormReport.DataLeftAgeRelated;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataLeftAgeRelated))
                        {
                            this.dataLeftAgeRelated.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataLeftAgeRelated);
                        }
                        shortFormReportModel.DataRightMammaryGland = shortFormReport.DataRightMammaryGland;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightMammaryGland))
                        {
                            this.dataRightMammaryGland.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightMammaryGland);
                        }
                        shortFormReportModel.DataRightAgeRelated = shortFormReport.DataRightAgeRelated;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataRightAgeRelated))
                        {
                            this.dataRightAgeRelated.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataRightAgeRelated);
                        }

                        shortFormReportModel.DataTotalPts = shortFormReport.DataTotalPts;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataTotalPts))
                        {
                            this.dataTotalPts.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataTotalPts);
                        }
                        shortFormReportModel.DataBiRadsCategory = shortFormReport.DataBiRadsCategory;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataBiRadsCategory))
                        {
                            this.dataBiRadsCategory.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataBiRadsCategory);
                        }                        
                        shortFormReportModel.DataFurtherExam = shortFormReport.DataFurtherExam;
                        if (!string.IsNullOrEmpty(shortFormReportModel.DataFurtherExam))
                        {
                            this.dataFurtherExam.SelectedIndex = Convert.ToInt32(shortFormReportModel.DataFurtherExam);
                        }
                        shortFormReportModel.DataComments = shortFormReport.DataComments;
                        this.dataComments.Text = shortFormReportModel.DataComments;

                        MessageBox.Show(this, App.Current.FindResource("Message_27").ToString());
                        //if (File.Exists(person.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + person.Code + "_protocal.doc"))
                        //{
                        //    File.Delete(person.ArchiveFolder + System.IO.Path.DirectorySeparatorChar + person.Code + "_protocal.doc");
                        //}
                        ////改名原始的Protocal文件名，解決不能重新生成一個新的Protocal文件的問題
                        //File.Move(docFile, person.ArchiveFolder+System.IO.Path.DirectorySeparatorChar +person.Code+"_protocal.doc");
                    }
                    catch (Exception ex) {
                        MessageBox.Show(this, ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show(this, string.Format(App.Current.FindResource("Message_33").ToString(), docFile));
                }
            }
            else
            {
                MessageBox.Show(this,App.Current.FindResource("Message_26").ToString());
            }
        }


        /// <summary>
        /// 窗口大小状态变化时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            try
            {
                var mainWin = this.Owner.Owner as MainWindow;
                if (this.WindowState == WindowState.Maximized)
                {
                    this.StopMouseHook();
                }
                if (this.WindowState == WindowState.Minimized)
                {
                    WinMinimized();                    
                }
                else
                {
                    this.StopMouseHook();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 最小化窗体以便显示MEIK程序窗体
        /// </summary>
        private void WinMinimized()
        {
            App.opendWin = this;
            this.WindowState = WindowState.Minimized;
            //this.WindowStartupLocation = WindowStartupLocation.Manual;//设置可手动指定窗体位置                
            int left = (int)(System.Windows.SystemParameters.PrimaryScreenWidth - 216);
            IntPtr winHandle = new WindowInteropHelper(this).Handle;
            Win32Api.MoveWindow(winHandle, left, 0, 0, 0, false);
            IntPtr mainWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmMain", null);
            //如果主窗体存在
            if (mainWinHwnd != IntPtr.Zero)
            {
                this.StartMouseHook();
            }
        }


        /// <summary>
        /// 启用鼠标钩子
        /// </summary>
        public void StartMouseHook()
        {
            //启动鼠标钩子            
            mouseHook.Start();
        }
        /// <summary>
        /// 停止鼠标钩子
        /// </summary>
        public void StopMouseHook()
        {
            mouseHook.Stop();
        }

        /// <summary>
        /// 鼠标按下的钩子回调方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mouseHook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                IntPtr buttonHandle = Win32Api.WindowFromPoint(e.X, e.Y);
                IntPtr winHandle = Win32Api.GetParent(buttonHandle);
                var owner = this.Owner.Owner as MainWindow;
                if (Win32Api.GetParent(winHandle) == owner.AppProc.MainWindowHandle)
                {
                    StringBuilder winText = new StringBuilder(512);
                    Win32Api.GetWindowText(buttonHandle, winText, winText.Capacity);
                    if (App.strExit.Equals(winText.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        IntPtr mainWinHwnd = Win32Api.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "TfmMain", null);
                        //如果主窗体存在
                        if (mainWinHwnd != IntPtr.Zero)
                        {
                            int WM_SYSCOMMAND = 0x0112;
                            int SC_CLOSE = 0xF060;
                            Win32Api.SendMessage(mainWinHwnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                        }
                        //this.Visibility = Visibility.Visible;
                        this.WindowState = WindowState.Normal;
                        //this.StopMouseHook();
                    }                    
                }
            }
        }

        /// <summary>
        /// 鼠标移动的钩子回调方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mouseHook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {                                     
            StringBuilder winText = new StringBuilder(512);
            try
            {
                IntPtr exitButtonHandle = Win32Api.WindowFromPoint(e.X, e.Y);  
                Win32Api.GetWindowText(exitButtonHandle, winText, winText.Capacity);
            }
            catch(Exception){}
            //如果鼠标移动到退出按钮上
            if (App.strExit.Equals(winText.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }                            
        }
        

        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) < 100)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_56").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }

                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_56").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
            
        }

        private void Hour_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try { 
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) < 24 && (textBox.Text.Length+1) <= 2)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_54").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }

        private void Minute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) < 60 && (textBox.Text.Length + 1) <= 2)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_55").ToString());
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }
                }
                else
                {
                    MessageBox.Show(this, App.Current.FindResource("Message_53").ToString());
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
                e.Handled = true;
            }
        }


        /// <summary>
        /// 選擇生成PDF報表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savePdfComb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox.SelectedIndex == 2)
            {
                try
                {
                    if (!Directory.Exists(dataFolder))
                    {
                        Directory.CreateDirectory(dataFolder);
                    }

                    LoadDataModel();
                    //Clone生成全文本的报表数据对象模型
                    var reportModel = CloneReportModel();
                    //打开文件夹对话框，选择要保存的目录
                    System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                    folderBrowserDialog.SelectedPath = this.person.ArchiveFolder;
                    System.Windows.Forms.DialogResult res = folderBrowserDialog.ShowDialog();
                    if (res == System.Windows.Forms.DialogResult.OK)
                    {
                        string folderName = folderBrowserDialog.SelectedPath;
                        string strName = person.SurName + (string.IsNullOrEmpty(person.GivenName) ? "" : "," + person.GivenName) + (string.IsNullOrEmpty(person.OtherName) ? "" : " " + person.OtherName) + ".pdf";
                        //生成Examination报告的PDF文件
                        string lfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + " LF - " + strName;
                        string lfReportTempl = "Views/ExaminationReportFlow.xaml";
                        ExportFlowDocumentPDF(lfReportTempl, lfPdfFile, reportModel);

                        //生成Summary报告的PDF文件
                        string sfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + "SF - " + strName;
                        string sfReportTempl = "Views/SummaryReportFlow.xaml";

                        ExportFlowDocumentPDF(sfReportTempl, sfPdfFile, reportModel, "A4");

                        MessageBox.Show(this, App.Current.FindResource("Message_5").ToString());
                    }
                }
                catch (Exception ex)
                {
                    FileHelper.SetFolderPower(dataFolder, "Everyone", "FullControl");
                    FileHelper.SetFolderPower(dataFolder, "Users", "FullControl");
                    MessageBox.Show(this, ex.Message);
                }
            }
            else if (comboBox.SelectedIndex == 1)
            {
                try
                {
                    if (!Directory.Exists(dataFolder))
                    {
                        Directory.CreateDirectory(dataFolder);
                    }

                    LoadDataModel();
                    //Clone生成全文本的报表数据对象模型
                    var reportModel = CloneReportModel();
                    //打开文件夹对话框，选择要保存的目录
                    System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
                    folderBrowserDialog.SelectedPath = this.person.ArchiveFolder;
                    System.Windows.Forms.DialogResult res = folderBrowserDialog.ShowDialog();
                    if (res == System.Windows.Forms.DialogResult.OK)
                    {
                        string folderName = folderBrowserDialog.SelectedPath;
                        string strName = person.SurName + (string.IsNullOrEmpty(person.GivenName) ? "" : "," + person.GivenName) + (string.IsNullOrEmpty(person.OtherName) ? "" : " " + person.OtherName) + ".pdf";
                        //生成Examination报告的PDF文件
                        string lfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + " LF - " + strName;
                        string lfReportTempl = "Views/ExaminationReportFlow.xaml";
                        ExportFlowDocumentPDF(lfReportTempl, lfPdfFile, reportModel);

                        //生成Summary报告的PDF文件
                        string sfPdfFile = folderName + System.IO.Path.DirectorySeparatorChar + person.Code + "SF - " + strName;
                        string sfReportTempl = "Views/SummaryReportDocument.xaml";
                        if (shortFormReportModel.DataScreenShotImg != null)
                        {
                            sfReportTempl = "Views/SummaryReportNuvoTekDocument.xaml";
                        }

                        ExportPDF(sfReportTempl, sfPdfFile, reportModel);

                        MessageBox.Show(this, App.Current.FindResource("Message_5").ToString());
                    }
                }
                catch (Exception ex)
                {
                    FileHelper.SetFolderPower(dataFolder, "Everyone", "FullControl");
                    FileHelper.SetFolderPower(dataFolder, "Users", "FullControl");
                    MessageBox.Show(this, ex.Message);
                }
            }

        }

        //private void analysis_Click(object sender, RoutedEventArgs e)
        //{
        //    generatePoints();
        //} 
               
    }
}
