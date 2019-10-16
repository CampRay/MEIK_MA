using MEIKReport.Common;
using MEIKReport.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
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
using System.Windows.Threading;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace MEIKReport
{
    /// <summary>
    /// PrintPreviewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PrintPreviewWindow : Window
    {
        private delegate void LoadXpsMethod();
        private readonly Object m_data;
        private readonly Object m_doc;
        private readonly bool isFixedPage;
        private MemoryStream ms = new MemoryStream();

        /// <summary>
        /// 使用FixedPage模板并加载渲染数据
        /// </summary>
        /// <param name="strTmplName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static FixedPage LoadFixedDocumentAndRender(string strTmplName, Object data)
        {
            FixedPage doc = (FixedPage)Application.LoadComponent(new Uri(strTmplName, UriKind.RelativeOrAbsolute));
            if ("Letter".Equals(App.reportSettingModel.PrintPaper.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                doc.Width = 96 * 8.5;
                doc.Height = 96 * 11;
            }
            else if ("A4".Equals(App.reportSettingModel.PrintPaper.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                doc.Width = 96 * 8.27;
                doc.Height = 96 * 11.69;                
            }
            doc.DataContext = data;
            //加载图片到文档中
            if (data.GetType() == typeof(ShortFormReport))
            {
                var shortFormReport = data as ShortFormReport;
                if (shortFormReport != null)
                {
                     
                    var topImage = doc.FindName("imgTitleLog") as Image;
                    if (topImage != null)
                    {
                        if (!App.reportSettingModel.DefaultLogo)
                        {
                            topImage.Source = ImageTools.GetBitmapImage(AppDomain.CurrentDomain.BaseDirectory + "logo.png");
                        }                        
                    }
                    
                    var signImage = doc.FindName("dataSignImg") as Image;
                    if (signImage != null && shortFormReport.DataSignImg!=null)
                    {
                        signImage.Source = ImageTools.GetBitmapImage(shortFormReport.DataSignImg);
                    }
                    var screenShotImg = doc.FindName("dataScreenShotImg") as Image;
                    if (screenShotImg != null && shortFormReport.DataScreenShotImg != null)
                    {
                        screenShotImg.Source = ImageTools.GetBitmapImage(shortFormReport.DataScreenShotImg);
                    }
                    if (!App.reportSettingModel.ShowTechSignature)
                    {
                        var techSignPanel = doc.FindName("techSignPanel") as Panel;
                        if (techSignPanel != null)
                        {
                            techSignPanel.Visibility = Visibility.Collapsed;
                        }
                    }
                    if (!App.reportSettingModel.ShowDoctorSignature)
                    {
                        var doctorSignPanel = doc.FindName("doctorSignPanel") as Panel;
                        if (doctorSignPanel != null)
                        {
                            doctorSignPanel.Visibility = Visibility.Collapsed;
                        }
                        var doctorSignGrid = doc.FindName("doctorSignGrid") as Panel;
                        if (doctorSignGrid != null)
                        {
                            doctorSignGrid.Visibility = Visibility.Collapsed;
                        }
                    }

                }
            }            
            return doc;
        }
        /// <summary>
        /// 使用FlowDocument模板并加载渲染数据
        /// </summary>
        /// <param name="strTmplName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static FlowDocument LoadFlowDocumentAndRender(string strTmplName, Object data,string pagesize=null)
        {
            FlowDocument doc = (FlowDocument)Application.LoadComponent(new Uri(strTmplName, UriKind.RelativeOrAbsolute));
            if (string.IsNullOrEmpty(pagesize))
            {
                if ("Letter".Equals(App.reportSettingModel.PrintPaper.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    //doc.PagePadding = new Thickness(20);
                    doc.PageWidth = 96 * 8.5;
                    doc.PageHeight = 96 * 11;
                    doc.ColumnWidth = 734;
                }
                else if ("A4".Equals(App.reportSettingModel.PrintPaper.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    //doc.PagePadding = new Thickness(20);
                    doc.PageWidth = 96 * 8.27;
                    doc.PageHeight = 96 * 11.69;
                    doc.ColumnWidth = 734;
                }
            }
            else
            {
                if ("Letter".Equals(pagesize, StringComparison.OrdinalIgnoreCase))
                {
                    //doc.PagePadding = new Thickness(20);
                    doc.PageWidth = 96 * 8.5;
                    doc.PageHeight = 96 * 11;
                    doc.ColumnWidth = 734;
                }
                else if ("A4".Equals(pagesize, StringComparison.OrdinalIgnoreCase))
                {
                    //doc.PagePadding = new Thickness(20);
                    doc.PageWidth = 96 * 8.27;
                    doc.PageHeight = 96 * 11.69;
                    doc.ColumnWidth = 734;
                }
            }
            doc.DataContext = data;            

            var shortFormReport = data as ShortFormReport;
            if (shortFormReport != null)
            {
                #region 顯示定位病變位置
                var textBlock1 = new TextBlock();
                if (!string.IsNullOrEmpty(shortFormReport.DataLeftLocation) && doc.FindName("L1_Canvas") != null)
                {                   
                    if (shortFormReport.DataLeftLocation.StartsWith("12"))
                    {
                        textBlock1 = doc.FindName("L1_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("11"))
                    {
                        textBlock1 = doc.FindName("L1_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("10"))
                    {
                        textBlock1 = doc.FindName("L1_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("9"))
                    {
                        textBlock1 = doc.FindName("L1_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("8"))
                    {
                        textBlock1 = doc.FindName("L1_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("7"))
                    {
                        textBlock1 = doc.FindName("L1_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("6"))
                    {
                        textBlock1 = doc.FindName("L1_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("5"))
                    {
                        textBlock1 = doc.FindName("L1_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("4"))
                    {
                        textBlock1 = doc.FindName("L1_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("3"))
                    {
                        textBlock1 = doc.FindName("L1_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("2"))
                    {
                        textBlock1 = doc.FindName("L1_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation.StartsWith("1"))
                    {
                        textBlock1 = doc.FindName("L1_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("L1".Equals(shortFormReport.DataLeftMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}
                }

                if (!string.IsNullOrEmpty(shortFormReport.DataRightLocation) && doc.FindName("R1_Canvas") != null)
                {
                    if (shortFormReport.DataRightLocation.StartsWith("12"))
                    {
                        textBlock1 = doc.FindName("R1_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("11"))
                    {
                        textBlock1 = doc.FindName("R1_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("10"))
                    {
                        textBlock1 = doc.FindName("R1_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("9"))
                    {
                        textBlock1 = doc.FindName("R1_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("8"))
                    {
                        textBlock1 = doc.FindName("R1_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("7"))
                    {
                        textBlock1 = doc.FindName("R1_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("6"))
                    {
                        textBlock1 = doc.FindName("R1_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("5"))
                    {
                        textBlock1 = doc.FindName("R1_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("4"))
                    {
                        textBlock1 = doc.FindName("R1_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("3"))
                    {
                        textBlock1 = doc.FindName("R1_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("2"))
                    {
                        textBlock1 = doc.FindName("R1_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation.StartsWith("1"))
                    {
                        textBlock1 = doc.FindName("R1_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("R1".Equals(shortFormReport.DataRightMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }

                if (!string.IsNullOrEmpty(shortFormReport.DataLeftLocation2) && doc.FindName("L2_Canvas") != null)
                {
                    if (shortFormReport.DataLeftLocation2.StartsWith("12"))
                    {
                        textBlock1 = doc.FindName("L2_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("11"))
                    {
                        textBlock1 = doc.FindName("L2_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("10"))
                    {
                        textBlock1 = doc.FindName("L2_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("9"))
                    {
                        textBlock1 = doc.FindName("L2_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("8"))
                    {
                        textBlock1 = doc.FindName("L2_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("7"))
                    {
                        textBlock1 = doc.FindName("L2_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("6"))
                    {
                        textBlock1 = doc.FindName("L2_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("5"))
                    {
                        textBlock1 = doc.FindName("L2_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("4"))
                    {
                        textBlock1 = doc.FindName("L2_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("3"))
                    {
                        textBlock1 = doc.FindName("L2_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("2"))
                    {
                        textBlock1 = doc.FindName("L2_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation2.StartsWith("1"))
                    {
                        textBlock1 = doc.FindName("L2_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("L2".Equals(shortFormReport.DataLeftMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }

                if (!string.IsNullOrEmpty(shortFormReport.DataRightLocation2) && doc.FindName("R2_Canvas") != null)
                {
                    if (shortFormReport.DataRightLocation2.StartsWith("12"))
                    {
                        textBlock1 = doc.FindName("R2_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("11"))
                    {
                        textBlock1 = doc.FindName("R2_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("10"))
                    {
                        textBlock1 = doc.FindName("R2_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("9"))
                    {
                        textBlock1 = doc.FindName("R2_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("8"))
                    {
                        textBlock1 = doc.FindName("R2_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("7"))
                    {
                        textBlock1 = doc.FindName("R2_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("6"))
                    {
                        textBlock1 = doc.FindName("R2_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("5"))
                    {
                        textBlock1 = doc.FindName("R2_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("4"))
                    {
                        textBlock1 = doc.FindName("R2_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("3"))
                    {
                        textBlock1 = doc.FindName("R2_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("2"))
                    {
                        textBlock1 = doc.FindName("R2_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation2.StartsWith("1"))
                    {
                        textBlock1 = doc.FindName("R2_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("R2".Equals(shortFormReport.DataRightMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }

                if (!string.IsNullOrEmpty(shortFormReport.DataLeftLocation3) && doc.FindName("L3_Canvas") != null)
                {
                    if (shortFormReport.DataLeftLocation3.StartsWith("12"))
                    {
                        textBlock1 = doc.FindName("L3_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("11"))
                    {
                        textBlock1 = doc.FindName("L3_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("10"))
                    {
                        textBlock1 = doc.FindName("L3_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("9"))
                    {
                        textBlock1 = doc.FindName("L3_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("8"))
                    {
                        textBlock1 = doc.FindName("L3_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("7"))
                    {
                        textBlock1 = doc.FindName("L3_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("6"))
                    {
                        textBlock1 = doc.FindName("L3_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("5"))
                    {
                        textBlock1 = doc.FindName("L3_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("4"))
                    {
                        textBlock1 = doc.FindName("L3_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("3"))
                    {
                        textBlock1 = doc.FindName("L3_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("2"))
                    {
                        textBlock1 = doc.FindName("L3_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataLeftLocation3.StartsWith("1"))
                    {
                        textBlock1 = doc.FindName("L3_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("L3".Equals(shortFormReport.DataLeftMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}
                }


                if (!string.IsNullOrEmpty(shortFormReport.DataRightLocation3) && doc.FindName("R3_Canvas") != null)
                {
                    if (shortFormReport.DataRightLocation3.StartsWith("12"))
                    {
                        textBlock1 = doc.FindName("R3_12") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("11"))
                    {
                        textBlock1 = doc.FindName("R3_11") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("10"))
                    {
                        textBlock1 = doc.FindName("R3_10") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("9"))
                    {
                        textBlock1 = doc.FindName("R3_9") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("8"))
                    {
                        textBlock1 = doc.FindName("R3_8") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("7"))
                    {
                        textBlock1 = doc.FindName("R3_7") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("6"))
                    {
                        textBlock1 = doc.FindName("R3_6") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("5"))
                    {
                        textBlock1 = doc.FindName("R3_5") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("4"))
                    {
                        textBlock1 = doc.FindName("R3_4") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("3"))
                    {
                        textBlock1 = doc.FindName("R3_3") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("2"))
                    {
                        textBlock1 = doc.FindName("R3_2") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    else if (shortFormReport.DataRightLocation3.StartsWith("1"))
                    {
                        textBlock1 = doc.FindName("R3_1") as TextBlock;
                        textBlock1.Visibility = Visibility.Visible;
                    }
                    //if ("R3".Equals(shortFormReport.DataRightMaxFlag))
                    //{
                    //    textBlock1.Text = "●";
                    //}

                }
                #endregion

                
                var signImage = doc.FindName("dataSignImg") as Image;
                if (signImage != null && shortFormReport.DataSignImg != null)
                {
                    signImage.Source = ImageTools.GetBitmapImage(shortFormReport.DataSignImg);
                }
                var screenShotImg = doc.FindName("dataScreenShotImg") as Image;
                if (screenShotImg != null && shortFormReport.DataScreenShotImg != null)
                {
                    screenShotImg.Source = ImageTools.GetBitmapImage(shortFormReport.DataScreenShotImg);
                }
                if (!App.reportSettingModel.ShowTechSignature )
                {
                    var techSignPanel = doc.FindName("techSignPanel") as Panel;
                    if (techSignPanel != null)
                    {
                        techSignPanel.Visibility = Visibility.Collapsed;
                    }
                }
                if (!App.reportSettingModel.ShowDoctorSignature)
                {
                    var doctorSignPanel = doc.FindName("doctorSignPanel") as Panel;
                    if (doctorSignPanel != null)
                    {
                        doctorSignPanel.Visibility = Visibility.Collapsed;
                    }
                    var doctorSignGrid = doc.FindName("doctorSignGrid") as Panel;
                    if (doctorSignGrid != null)
                    {
                        doctorSignGrid.Visibility = Visibility.Collapsed;
                    }
                }

                //電導率顯示紅色
                var leftMeanECOfLesionTextBlock = doc.FindName("dataLeftMeanECOfLesion") as TextBlock;
                if (leftMeanECOfLesionTextBlock != null && !shortFormReport.DataLeftAgeElectricalConductivity.Equals(App.Current.FindResource("ReportContext_112").ToString()))
                {
                    leftMeanECOfLesionTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xFF,0x00,0x00));
                }
                var rightMeanECOfLesionTextBlock = doc.FindName("dataRightMeanECOfLesion") as TextBlock;
                if (rightMeanECOfLesionTextBlock != null && !shortFormReport.DataRightAgeElectricalConductivity.Equals(App.Current.FindResource("ReportContext_112").ToString()))
                {
                    rightMeanECOfLesionTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00));
                }

                if (!String.IsNullOrEmpty(shortFormReport.DataComparativeElectricalConductivity3))
                {
                    var dataComparativeElectricalConductivity3 = doc.FindName("dataComparativeElectricalConductivity3") as TextBlock;
                    var selectedIndex = Convert.ToInt32(shortFormReport.DataComparativeElectricalConductivity3);
                    switch (selectedIndex)
                    {
                        case 1:
                            dataComparativeElectricalConductivity3.Text = App.Current.FindResource("ReportContext_103").ToString();
                            break;
                        case 2:
                            dataComparativeElectricalConductivity3.Text = App.Current.FindResource("ReportContext_104").ToString();
                            break;
                        case 3:
                            dataComparativeElectricalConductivity3.Text = App.Current.FindResource("ReportContext_105").ToString();
                            break;
                        default:
                            dataComparativeElectricalConductivity3.Text = "";
                            break;
                    }
                }

                if (!String.IsNullOrEmpty(shortFormReport.DataDivergenceBetweenHistograms3))
                {
                    var dataDivergenceBetweenHistograms3 = doc.FindName("dataDivergenceBetweenHistograms3") as TextBlock;
                    var selectedIndex1 = Convert.ToInt32(shortFormReport.DataDivergenceBetweenHistograms3);
                    switch (selectedIndex1)
                    {
                        case 1:
                            dataDivergenceBetweenHistograms3.Text = App.Current.FindResource("ReportContext_103").ToString();
                            break;
                        case 2:
                            dataDivergenceBetweenHistograms3.Text = App.Current.FindResource("ReportContext_104").ToString();
                            break;
                        case 3:
                            dataDivergenceBetweenHistograms3.Text = App.Current.FindResource("ReportContext_105").ToString();
                            break;
                        default:
                            dataDivergenceBetweenHistograms3.Text = "";
                            break;
                    }
                }
                //替换头部图片
                var titleRow = doc.FindName("titleImg") as TableRow;
                if (titleRow != null && !App.reportSettingModel.DefaultLogo)
                {
                    var brush = new ImageBrush();
                    brush.ImageSource = ImageTools.GetBitmapImage(AppDomain.CurrentDomain.BaseDirectory + "logo.png");
                    brush.AlignmentY = AlignmentY.Top;
                    brush.Stretch = Stretch.Uniform;
                    titleRow.Background = brush;
                }
                var titleRow1 = doc.FindName("titleImg1") as TableRow;
                if (titleRow1 != null && !App.reportSettingModel.DefaultLogo)
                {
                    var brush = new ImageBrush();
                    brush.ImageSource = ImageTools.GetBitmapImage(AppDomain.CurrentDomain.BaseDirectory + "logo.png");
                    brush.AlignmentY = AlignmentY.Top;
                    brush.Stretch = Stretch.Uniform;

                    var titleRow2 = doc.FindName("titleImg2") as TableRow;
                    var titleRow3 = doc.FindName("titleImg3") as TableRow;
                    var titleRow4 = doc.FindName("titleImg4") as TableRow;
                    titleRow1.Background = brush;
                    titleRow2.Background = brush;
                    titleRow3.Background = brush;
                    titleRow4.Background = brush;
                }
                //替换头部logo
                var logoCell1 = doc.FindName("logoImg1") as TableCell;
                if (logoCell1 != null && App.reportSettingModel.DeciceLogo.Count>0)
                {
                    foreach (var item in App.reportSettingModel.DeciceLogo)
                    {
                        if (item.Device.Equals(shortFormReport.DataUserCode.Substring(6,3)))
                        {
                            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + item.Device + ".png"))
                            {
                                var brush = new ImageBrush();
                                brush.ImageSource = ImageTools.GetBitmapImage(AppDomain.CurrentDomain.BaseDirectory + item.Device + ".png");
                                brush.AlignmentY = AlignmentY.Top;
                                brush.Stretch = Stretch.Uniform;

                                var logoCell2 = doc.FindName("logoImg2") as TableCell;
                                var logoCell3 = doc.FindName("logoImg3") as TableCell;
                                var logoCell4 = doc.FindName("logoImg4") as TableCell;
                                logoCell1.Background = brush;
                                if (logoCell2 != null)
                                {
                                    logoCell2.Background = brush;
                                }
                                if (logoCell3 != null)
                                {
                                    logoCell3.Background = brush;
                                }
                                if (logoCell4 != null)
                                {
                                    logoCell4.Background = brush;
                                }
                            }

                            var footText1 = doc.FindName("footerInfo1") as TextBlock;
                            var footText2 = doc.FindName("footerInfo2") as TextBlock;
                            var footText3 = doc.FindName("footerInfo3") as TextBlock;
                            var footText4 = doc.FindName("footerInfo4") as TextBlock;
                            if (footText1 != null)
                            {
                                footText1.Text = item.Address;
                            }
                            if (footText2 != null)
                            {
                                footText2.Text = item.Address;
                            }
                            if (footText3 != null)
                            {
                                footText3.Text = item.Address;
                            }
                            if (footText4 != null)
                            {
                                footText4.Text = item.Address;
                            }

                            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + item.Device + "_s.png"))
                            {
                                //替换公章
                                var sealCanvas = doc.FindName("sealCanvas") as Image;
                                BitmapImage sealImage = new BitmapImage();
                                sealImage.BeginInit();
                                sealImage.CacheOption = BitmapCacheOption.OnLoad;
                                sealImage.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + item.Device + "_s.png");
                                sealImage.EndInit();
                                sealCanvas.Source = sealImage;
                            }
                            break;
                        }
                    }                    
                }                

            }
            return doc;
        }
        public PrintPreviewWindow()
        {
            InitializeComponent();            
        }
        public PrintPreviewWindow(string strTmplName, bool isFixedPage,Object data)
        {
            InitializeComponent();
            m_data = data;
            this.isFixedPage = isFixedPage;
            if (isFixedPage)
            {
                m_doc = LoadFixedDocumentAndRender(strTmplName, data);
            }
            else
            {
                m_doc = LoadFlowDocumentAndRender(strTmplName, data,"A4");
            }
            Dispatcher.BeginInvoke(new LoadXpsMethod(LoadXps), DispatcherPriority.ApplicationIdle);
        }        

        public void LoadXps()
        {
            //构造一个基于内存的xps document
            //MemoryStream ms = new MemoryStream();
            Package package = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite);
            Uri DocumentUri = new Uri("pack://InMemoryDocument.xps");
            PackageStore.RemovePackage(DocumentUri);
            PackageStore.AddPackage(DocumentUri, package);
            XpsDocument xpsDocument = new XpsDocument(package, CompressionOption.Fast, DocumentUri.AbsoluteUri);

            //将flow document写入基于内存的xps document中去
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            if (this.isFixedPage)
            {
                writer.Write((FixedPage)m_doc);

            }
            else
            {
                writer.Write(((IDocumentPaginatorSource)m_doc).DocumentPaginator);
            }

            //获取这个基于内存的xps document的fixed document
            docViewer.Document = xpsDocument.GetFixedDocumentSequence();
            //关闭基于内存的xps document
            xpsDocument.Close();
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.ms != null)
            {
                this.ms.Close();
            }
        }
        
    }
}
