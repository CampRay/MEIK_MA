using MEIKReport.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace MEIKReport.Common
{
    public class XmlTools
    {
        public static ShortFormReport ReadXmlFile(string xmlFilePath)
        {

            if (FileHelper.FileInUsed(xmlFilePath))
            {
                return null;
            }
            ShortFormReport shortFormReport = new ShortFormReport();
            XmlDocument doc = new XmlDocument();
            
                        
            try
            {
                doc.Load(xmlFilePath);                
                var nodeList=doc.GetElementsByTagName("CICL");
                var node=nodeList.Item(nodeList.Count - 1);
                string dataMenstrualCycle = node.InnerText;
                if (string.IsNullOrEmpty(dataMenstrualCycle))
                {
                    shortFormReport.DataMenstrualCycle = "0";
                }
                else if (dataMenstrualCycle.StartsWith("1 phase") || dataMenstrualCycle.StartsWith("phase 1") || dataMenstrualCycle.StartsWith("第一阶段") || dataMenstrualCycle.StartsWith("一期"))
                {
                    shortFormReport.DataMenstrualCycle = "1";// App.Current.FindResource("ReportContext_15").ToString();
                }
                else if (dataMenstrualCycle.StartsWith("2 phase") || dataMenstrualCycle.StartsWith("phase 2") || dataMenstrualCycle.StartsWith("第二阶段") || dataMenstrualCycle.StartsWith("二期"))
                {
                    shortFormReport.DataMenstrualCycle = "2";//App.Current.FindResource("ReportContext_16").ToString();
                }
                else if (dataMenstrualCycle.StartsWith("1 and 2 phase") || dataMenstrualCycle.StartsWith("第一和第二阶段") || dataMenstrualCycle.StartsWith("一期和二期"))
                {
                    shortFormReport.DataMenstrualCycle = "3";//App.Current.FindResource("ReportContext_17").ToString();
                }
                else if (dataMenstrualCycle.StartsWith("dysmenorrhea") || dataMenstrualCycle.StartsWith("痛经"))
                {
                    shortFormReport.DataMenstrualCycle = "4";// App.Current.FindResource("ReportContext_18").ToString();
                }
                else if (dataMenstrualCycle.StartsWith("missing"))
                {
                    shortFormReport.DataMenstrualCycle = "5";// App.Current.FindResource("ReportContext_19").ToString();
                    shortFormReport.DataMeanElectricalConductivity3 = "1";
                    shortFormReport.DataComparativeElectricalConductivity3 = "1";
                    shortFormReport.DataDivergenceBetweenHistograms3 = "1";
                }
                else if (dataMenstrualCycle.StartsWith("pregnancy") || dataMenstrualCycle.StartsWith("孕期"))
                {
                    shortFormReport.DataMenstrualCycle = "6";// App.Current.FindResource("ReportContext_20").ToString();
                    shortFormReport.DataMeanElectricalConductivity3 = "2";
                    shortFormReport.DataComparativeElectricalConductivity3 = "2";
                    shortFormReport.DataDivergenceBetweenHistograms3 = "2";
                }
                else if (dataMenstrualCycle.StartsWith("lactation") || dataMenstrualCycle.StartsWith("哺乳期"))
                {
                    shortFormReport.DataMenstrualCycle = "7";// App.Current.FindResource("ReportContext_21").ToString();
                    shortFormReport.DataMeanElectricalConductivity3 = "3";
                    shortFormReport.DataComparativeElectricalConductivity3 = "3";
                    shortFormReport.DataDivergenceBetweenHistograms3 = "3";
                }
                else if (dataMenstrualCycle.StartsWith("postmenopause") || dataMenstrualCycle.StartsWith("postmenopausal") || dataMenstrualCycle.StartsWith("绝经期"))
                {
                    shortFormReport.DataMenstrualCycle = "8";// App.Current.FindResource("ReportContext_21").ToString();
                    shortFormReport.DataMeanElectricalConductivity3 = "1";
                    shortFormReport.DataComparativeElectricalConductivity3 = "1";
                    shortFormReport.DataDivergenceBetweenHistograms3 = "1";
                }
                
                //shortFormReport.DataMenstrualCycle = doc.FormFields[11].Result;

                nodeList = doc.GetElementsByTagName("GORM");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataHormones = node.InnerText;

                nodeList = doc.GetElementsByTagName("PORAZH");
                node = nodeList.Item(nodeList.Count - 1);
                var skinAffections = node.InnerText;
                if (string.IsNullOrEmpty(skinAffections))
                {
                    shortFormReport.DataSkinAffections = "0";
                }
                else if (skinAffections.StartsWith("no"))
                {
                    shortFormReport.DataSkinAffections = "1";
                }
                else if (skinAffections.StartsWith("nevus"))
                {
                    shortFormReport.DataSkinAffections = "2";
                }
                else if (skinAffections.StartsWith("wart"))
                {
                    shortFormReport.DataSkinAffections = "3";
                }
                else if (skinAffections.StartsWith("acne"))
                {
                    shortFormReport.DataSkinAffections = "4";
                }
                else if (skinAffections.StartsWith("scar after surgery"))
                {
                    shortFormReport.DataSkinAffections = "5";
                }
                else if (skinAffections.StartsWith("consequences of burns"))
                {
                    shortFormReport.DataSkinAffections = "6";
                }
                else if (skinAffections.StartsWith("sunburns"))
                {
                    shortFormReport.DataSkinAffections = "7";
                }
                else
                {
                    shortFormReport.DataSkinAffections = "0";
                }

                nodeList = doc.GetElementsByTagName("OIE_L");
                node = nodeList.Item(nodeList.Count - 1);
                var leftChangesOfElectricalConductivity = node.InnerText;
                if (string.IsNullOrEmpty(leftChangesOfElectricalConductivity))
                {
                    shortFormReport.DataLeftChangesOfElectricalConductivity = "0";
                }
                else if (leftChangesOfElectricalConductivity.StartsWith("no"))
                {
                    shortFormReport.DataLeftChangesOfElectricalConductivity = "1";
                }
                else if (leftChangesOfElectricalConductivity.StartsWith("focal"))
                {
                    shortFormReport.DataLeftChangesOfElectricalConductivity = "2";
                }
                else if (leftChangesOfElectricalConductivity.StartsWith("diffuse"))
                {
                    shortFormReport.DataLeftChangesOfElectricalConductivity = "3";
                }
                
                nodeList = doc.GetElementsByTagName("OIE_R");
                node = nodeList.Item(nodeList.Count - 1);                
                var rightChangesOfElectricalConductivity = node.InnerText;
                if (string.IsNullOrEmpty(rightChangesOfElectricalConductivity))
                {
                    shortFormReport.DataRightChangesOfElectricalConductivity = "0";
                }
                else if (rightChangesOfElectricalConductivity.StartsWith("no"))
                {
                    shortFormReport.DataRightChangesOfElectricalConductivity = "1";
                }
                else if (rightChangesOfElectricalConductivity.StartsWith("focal"))
                {
                    shortFormReport.DataRightChangesOfElectricalConductivity = "2";
                }
                else if (rightChangesOfElectricalConductivity.StartsWith("diffuse"))
                {
                    shortFormReport.DataRightChangesOfElectricalConductivity = "3";
                }

                nodeList = doc.GetElementsByTagName("SMZH_L");
                node = nodeList.Item(nodeList.Count - 1);
                var leftMammaryStruc = node.InnerText;
                if (string.IsNullOrEmpty(leftMammaryStruc))
                {
                    shortFormReport.DataLeftMammaryStruct = "0";
                }
                else if (leftMammaryStruc.StartsWith("not changed"))
                {
                    shortFormReport.DataLeftMammaryStruct = "1";
                }
                else if (leftMammaryStruc.StartsWith("changed"))
                {
                    shortFormReport.DataLeftMammaryStruct = "2";
                }

                nodeList = doc.GetElementsByTagName("SMZH_R");
                node = nodeList.Item(nodeList.Count - 1);
                var rightMammaryStruc = node.InnerText;
                if (string.IsNullOrEmpty(rightMammaryStruc))
                {
                    shortFormReport.DataRightMammaryStruct = "0";
                }
                else if (rightMammaryStruc.StartsWith("not changed"))
                {
                    shortFormReport.DataRightMammaryStruct = "1";
                }
                else if (rightMammaryStruc.StartsWith("changed"))
                {
                    shortFormReport.DataRightMammaryStruct = "2";
                }
                //shortFormReport.DataLeftMammaryStruct = doc.FormFields[17].Result;
                //shortFormReport.DataRightMammaryStruct = doc.FormFields[18].Result;

                nodeList = doc.GetElementsByTagName("ZMS_L");
                node = nodeList.Item(nodeList.Count - 1);
                var leftLactiferousSinusZone = node.InnerText;
                if (string.IsNullOrEmpty(leftLactiferousSinusZone))
                {
                    shortFormReport.DataLeftLactiferousSinusZone = "0";
                }
                else if (leftLactiferousSinusZone.StartsWith("not represented"))
                {
                    shortFormReport.DataLeftLactiferousSinusZone = "1";
                }
                else if (leftLactiferousSinusZone.StartsWith("not expanded"))
                {
                    shortFormReport.DataLeftLactiferousSinusZone = "2";
                }
                else if (leftLactiferousSinusZone.StartsWith("expanded"))
                {
                    shortFormReport.DataLeftLactiferousSinusZone = "3";
                }
                else if (leftLactiferousSinusZone.StartsWith("fragmentated"))
                {
                    shortFormReport.DataLeftLactiferousSinusZone = "4";
                }

                nodeList = doc.GetElementsByTagName("ZMS_R");
                node = nodeList.Item(nodeList.Count - 1);
                var rightLactiferousSinusZone = node.InnerText;
                if (string.IsNullOrEmpty(rightLactiferousSinusZone))
                {
                    shortFormReport.DataRightLactiferousSinusZone = "0";
                }
                else if (rightLactiferousSinusZone.StartsWith("not represented"))
                {
                    shortFormReport.DataRightLactiferousSinusZone = "1";
                }
                else if (rightLactiferousSinusZone.StartsWith("not expanded"))
                {
                    shortFormReport.DataRightLactiferousSinusZone = "2";
                }
                else if (rightLactiferousSinusZone.StartsWith("expanded"))
                {
                    shortFormReport.DataRightLactiferousSinusZone = "3";
                }
                else if (rightLactiferousSinusZone.StartsWith("fragmentated"))
                {
                    shortFormReport.DataRightLactiferousSinusZone = "4";
                }
                //shortFormReport.DataLeftLactiferousSinusZone = doc.FormFields[19].Result;
                //shortFormReport.DataRightLactiferousSinusZone = doc.FormFields[20].Result;

                nodeList = doc.GetElementsByTagName("KMZH_L");
                node = nodeList.Item(nodeList.Count - 1);
                var leftMammaryContour = node.InnerText;
                if (string.IsNullOrEmpty(leftMammaryContour))
                {
                    shortFormReport.DataLeftMammaryContour = "0";
                }
                else if (leftMammaryContour.StartsWith("preserved"))
                {
                    shortFormReport.DataLeftMammaryContour = "1";
                }
                else if (leftMammaryContour.StartsWith("thickened"))
                {
                    shortFormReport.DataLeftMammaryContour = "2";
                }
                else if (leftMammaryContour.StartsWith("deformed"))
                {
                    shortFormReport.DataLeftMammaryContour = "3";
                }

                nodeList = doc.GetElementsByTagName("KMZH_R");
                node = nodeList.Item(nodeList.Count - 1);
                var rightMammaryContour = node.InnerText;
                if (string.IsNullOrEmpty(rightMammaryContour))
                {
                    shortFormReport.DataRightMammaryContour = "0";
                }
                else if (rightMammaryContour.StartsWith("preserved"))
                {
                    shortFormReport.DataRightMammaryContour = "1";
                }
                else if (rightMammaryContour.StartsWith("thickened"))
                {
                    shortFormReport.DataRightMammaryContour = "2";
                }
                else if (rightMammaryContour.StartsWith("deformed"))
                {
                    shortFormReport.DataRightMammaryContour = "3";
                }
                //shortFormReport.DataLeftMammaryContour = doc.FormFields[21].Result;
                //shortFormReport.DataRightMammaryContour = doc.FormFields[22].Result;

                nodeList = doc.GetElementsByTagName("S_L");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftSegment = node.InnerText;
                nodeList = doc.GetElementsByTagName("S_R");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataRightSegment = node.InnerText;
                //shortFormReport.DataLeftLocation = doc.FormFields[23].Result;
                //shortFormReport.DataRightLocation = doc.FormFields[24].Result;

                nodeList = doc.GetElementsByTagName("K_L");
                node = nodeList.Item(nodeList.Count - 1);
                var leftNumber = node.InnerText;
                if (string.IsNullOrEmpty(leftNumber))
                {
                    shortFormReport.DataLeftNumber = "0";
                }
                else if (leftNumber.StartsWith("1"))
                {
                    shortFormReport.DataLeftNumber = "1";
                }
                else if (leftNumber.StartsWith("2"))
                {
                    shortFormReport.DataLeftNumber = "2";
                }
                else if (leftNumber.StartsWith("3"))
                {
                    shortFormReport.DataLeftNumber = "3";
                }
                else if (leftNumber.StartsWith("numerous"))
                {
                    shortFormReport.DataLeftNumber = "4";
                }

                nodeList = doc.GetElementsByTagName("K_R");
                node = nodeList.Item(nodeList.Count - 1);
                var rightNumber = node.InnerText;
                if (string.IsNullOrEmpty(rightNumber))
                {
                    shortFormReport.DataRightNumber = "0";
                }
                else if (rightNumber.StartsWith("1"))
                {
                    shortFormReport.DataRightNumber = "1";
                }
                else if (rightNumber.StartsWith("2"))
                {
                    shortFormReport.DataRightNumber = "2";
                }
                else if (rightNumber.StartsWith("3"))
                {
                    shortFormReport.DataRightNumber = "3";
                }
                else if (rightNumber.StartsWith("numerous"))
                {
                    shortFormReport.DataRightNumber = "4";
                }
                //shortFormReport.DataLeftNumber = doc.FormFields[25].Result;
                //shortFormReport.DataRightNumber = doc.FormFields[26].Result;

                nodeList = doc.GetElementsByTagName("R_L");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftSize = node.InnerText;
                nodeList = doc.GetElementsByTagName("R_R");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataRightSize = node.InnerText;
                //shortFormReport.DataLeftSize = doc.FormFields[27].Result;
                //shortFormReport.DataRightSize = doc.FormFields[28].Result;

                nodeList = doc.GetElementsByTagName("F_L");
                node = nodeList.Item(nodeList.Count - 1);
                var leftShape = node.InnerText;
                if (string.IsNullOrEmpty(leftShape))
                {
                    shortFormReport.DataLeftShape = "0";
                }
                else if (leftShape.StartsWith("round"))
                {
                    shortFormReport.DataLeftShape = "1";
                }
                else if (leftShape.StartsWith("oval"))
                {
                    shortFormReport.DataLeftShape = "2";
                }
                else if (leftShape.StartsWith("lobular"))
                {
                    shortFormReport.DataLeftShape = "3";
                }
                else if (leftShape.StartsWith("irregular"))
                {
                    shortFormReport.DataLeftShape = "4";
                }

                nodeList = doc.GetElementsByTagName("F_R");
                node = nodeList.Item(nodeList.Count - 1);
                var rightShape = node.InnerText;
                if (string.IsNullOrEmpty(rightShape))
                {
                    shortFormReport.DataRightShape = "0";
                }
                else if (rightShape.StartsWith("round"))
                {
                    shortFormReport.DataRightShape = "1";
                }
                else if (rightShape.StartsWith("oval"))
                {
                    shortFormReport.DataRightShape = "2";
                }
                else if (rightShape.StartsWith("lobular"))
                {
                    shortFormReport.DataRightShape = "3";
                }
                else if (rightShape.StartsWith("irregular"))
                {
                    shortFormReport.DataRightShape = "4";
                }
                //shortFormReport.DataLeftShape = doc.FormFields[29].Result;
                //shortFormReport.DataRightShape = doc.FormFields[30].Result;

                nodeList = doc.GetElementsByTagName("KWO_L");
                node = nodeList.Item(nodeList.Count - 1);
                var leftContourAroundFocus = node.InnerText;
                if (string.IsNullOrEmpty(leftContourAroundFocus))
                {
                    shortFormReport.DataLeftContourAroundFocus = "0";
                }
                else if (leftContourAroundFocus.StartsWith("missing"))
                {
                    shortFormReport.DataLeftContourAroundFocus = "1";
                }
                else if (leftContourAroundFocus.StartsWith("distinct"))
                {
                    shortFormReport.DataLeftContourAroundFocus = "2";
                }
                else if (leftContourAroundFocus.StartsWith("hyperimpedance"))
                {
                    shortFormReport.DataLeftContourAroundFocus = "3";
                }
                else if (leftContourAroundFocus.StartsWith("indistinct"))
                {
                    shortFormReport.DataLeftContourAroundFocus = "4";
                }

                nodeList = doc.GetElementsByTagName("KWO_R");
                node = nodeList.Item(nodeList.Count - 1);
                var rightContourAroundFocus = node.InnerText;
                if (string.IsNullOrEmpty(rightContourAroundFocus))
                {
                    shortFormReport.DataRightContourAroundFocus = "0";
                }
                else if (rightContourAroundFocus.StartsWith("missing"))
                {
                    shortFormReport.DataRightContourAroundFocus = "1";
                }
                else if (rightContourAroundFocus.StartsWith("distinct"))
                {
                    shortFormReport.DataRightContourAroundFocus = "2";
                }
                else if (rightContourAroundFocus.StartsWith("hyperimpedance"))
                {
                    shortFormReport.DataRightContourAroundFocus = "3";
                }
                else if (rightContourAroundFocus.StartsWith("indistinct"))
                {
                    shortFormReport.DataRightContourAroundFocus = "4";
                }
                //shortFormReport.DataLeftContourAroundFocus = doc.FormFields[31].Result;
                //shortFormReport.DataRightContourAroundFocus = doc.FormFields[32].Result;

                nodeList = doc.GetElementsByTagName("VES_L");
                node = nodeList.Item(nodeList.Count - 1);
                var leftInternalElectricalStructure = node.InnerText;
                if (string.IsNullOrEmpty(leftInternalElectricalStructure))
                {
                    shortFormReport.DataLeftInternalElectricalStructure = "0";
                }
                else if (leftInternalElectricalStructure.StartsWith("hyperimpedance"))
                {
                    shortFormReport.DataLeftInternalElectricalStructure = "1";
                }
                else if (leftInternalElectricalStructure.StartsWith("isoimpedance"))
                {
                    shortFormReport.DataLeftInternalElectricalStructure = "2";
                }
                else if (leftInternalElectricalStructure.StartsWith("hypoimpedance"))
                {
                    shortFormReport.DataLeftInternalElectricalStructure = "3";
                }
                else if (leftInternalElectricalStructure.StartsWith("animpedance"))
                {
                    shortFormReport.DataLeftInternalElectricalStructure = "4";
                }

                nodeList = doc.GetElementsByTagName("VES_R");
                node = nodeList.Item(nodeList.Count - 1);
                var rightInternalElectricalStructure = node.InnerText;
                if (string.IsNullOrEmpty(rightInternalElectricalStructure))
                {
                    shortFormReport.DataRightInternalElectricalStructure = "0";
                }
                else if (rightInternalElectricalStructure.StartsWith("hyperimpedance"))
                {
                    shortFormReport.DataRightInternalElectricalStructure = "1";
                }
                else if (rightInternalElectricalStructure.StartsWith("isoimpedance"))
                {
                    shortFormReport.DataRightInternalElectricalStructure = "2";
                }
                else if (rightInternalElectricalStructure.StartsWith("hypoimpedance"))
                {
                    shortFormReport.DataRightInternalElectricalStructure = "3";
                }
                else if (rightInternalElectricalStructure.StartsWith("animpedance"))
                {
                    shortFormReport.DataRightInternalElectricalStructure = "4";
                }
                //shortFormReport.DataLeftInternalElectricalStructure = doc.FormFields[33].Result;
                //shortFormReport.DataRightInternalElectricalStructure = doc.FormFields[34].Result;

                nodeList = doc.GetElementsByTagName("OTIPK_L");
                node = nodeList.Item(nodeList.Count - 1);
                var leftSurroundingTissues = node.InnerText;
                if (string.IsNullOrEmpty(leftSurroundingTissues))
                {
                    shortFormReport.DataLeftSurroundingTissues = "0";
                }
                else if (leftSurroundingTissues.StartsWith("preserved"))
                {
                    shortFormReport.DataLeftSurroundingTissues = "1";
                }
                else if (leftSurroundingTissues.StartsWith("structure distubance"))
                {
                    shortFormReport.DataLeftSurroundingTissues = "2";
                }
                else if (leftSurroundingTissues.StartsWith("structure displacement"))
                {
                    shortFormReport.DataLeftSurroundingTissues = "3";
                }
                else if (leftSurroundingTissues.StartsWith("thikening"))
                {
                    shortFormReport.DataLeftSurroundingTissues = "4";
                }
                else if (leftSurroundingTissues.StartsWith("extrusion"))
                {
                    shortFormReport.DataLeftSurroundingTissues = "5";
                }
                else if (leftSurroundingTissues.StartsWith("retraction"))
                {
                    shortFormReport.DataLeftSurroundingTissues = "6";
                }

                nodeList = doc.GetElementsByTagName("OTIPK_R");
                node = nodeList.Item(nodeList.Count - 1);
                var rightSurroundingTissues = node.InnerText;
                if (string.IsNullOrEmpty(rightSurroundingTissues))
                {
                    shortFormReport.DataRightSurroundingTissues = "0";
                }
                else if (rightSurroundingTissues.StartsWith("preserved"))
                {
                    shortFormReport.DataRightSurroundingTissues = "1";
                }
                else if (rightSurroundingTissues.StartsWith("structure distubance"))
                {
                    shortFormReport.DataRightSurroundingTissues = "2";
                }
                else if (rightSurroundingTissues.StartsWith("structure displacement"))
                {
                    shortFormReport.DataRightSurroundingTissues = "3";
                }
                else if (rightSurroundingTissues.StartsWith("thikening"))
                {
                    shortFormReport.DataRightSurroundingTissues = "4";
                }
                else if (rightSurroundingTissues.StartsWith("extrusion"))
                {
                    shortFormReport.DataRightSurroundingTissues = "5";
                }
                else if (rightSurroundingTissues.StartsWith("retraction"))
                {
                    shortFormReport.DataRightSurroundingTissues = "6";
                }
                //shortFormReport.DataLeftSurroundingTissues = doc.FormFields[35].Result;
                //shortFormReport.DataRightSurroundingTissues = doc.FormFields[36].Result;



                nodeList = doc.GetElementsByTagName("SE1_L");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftMeanElectricalConductivity1 = node.InnerText;
                nodeList = doc.GetElementsByTagName("SE1_R");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataRightMeanElectricalConductivity1 = node.InnerText;

                nodeList = doc.GetElementsByTagName("SE2_L");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftMeanElectricalConductivity2 = node.InnerText;
                nodeList = doc.GetElementsByTagName("SE2_R");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataRightMeanElectricalConductivity2 = node.InnerText;

                
                //ShortFormReport.DataMeanElectricalConductivity3 = doc.FormFields[41].Result;

                nodeList = doc.GetElementsByTagName("SE_L");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftMeanElectricalConductivity3 = node.InnerText;
                nodeList = doc.GetElementsByTagName("SE_R");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataRightMeanElectricalConductivity3 = node.InnerText;

                nodeList = doc.GetElementsByTagName("SRE1");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftComparativeElectricalConductivity1 = node.InnerText;
                //ShortFormReport.DataRightComparativeElectricalConductivity1 = doc.FormFields[44].Result;
                nodeList = doc.GetElementsByTagName("SRE2");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftComparativeElectricalConductivity2 = node.InnerText;
                //ShortFormReport.DataRightComparativeElectricalConductivity2 = doc.FormFields[45].Result;
                nodeList = doc.GetElementsByTagName("SRE0");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftComparativeElectricalConductivity3 = node.InnerText;
                //ShortFormReport.DataRightComparativeElectricalConductivity3 = doc.FormFields[46].Result;

                nodeList = doc.GetElementsByTagName("RG1");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftDivergenceBetweenHistograms1 = node.InnerText;
                //ShortFormReport.DataRightDivergenceBetweenHistograms1 = doc.FormFields[47].Result;
                nodeList = doc.GetElementsByTagName("RG2");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftDivergenceBetweenHistograms2 = node.InnerText;
                //ShortFormReport.DataRightDivergenceBetweenHistograms2 = doc.FormFields[48].Result;
                nodeList = doc.GetElementsByTagName("RG0");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftDivergenceBetweenHistograms3 = node.InnerText;
                //ShortFormReport.DataRightDivergenceBetweenHistograms3 = doc.FormFields[49].Result;

                nodeList = doc.GetElementsByTagName("FE_L");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataLeftPhaseElectricalConductivity = node.InnerText;
                nodeList = doc.GetElementsByTagName("FE_R");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataRightPhaseElectricalConductivity = node.InnerText;

                nodeList = doc.GetElementsByTagName("RAST");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataAgeElectricalConductivityReference = node.InnerText;
                
                nodeList = doc.GetElementsByTagName("VE_L");
                node = nodeList.Item(nodeList.Count - 1);
                string dataLeftAgeElectricalConductivity = node.InnerText;
                if (string.IsNullOrEmpty(dataLeftAgeElectricalConductivity))
                {
                    shortFormReport.DataLeftAgeElectricalConductivity = "0";// "";
                }
                else if (dataLeftAgeElectricalConductivity.StartsWith("<5"))
                {
                    shortFormReport.DataLeftAgeElectricalConductivity = "1";// App.Current.FindResource("ReportContext_111").ToString();
                }
                else if (dataLeftAgeElectricalConductivity.StartsWith(">95"))
                {
                    shortFormReport.DataLeftAgeElectricalConductivity = "3";// App.Current.FindResource("ReportContext_113").ToString();
                }
                else 
                {
                    shortFormReport.DataLeftAgeElectricalConductivity = "2";// App.Current.FindResource("ReportContext_112").ToString();
                }
                //ShortFormReport.DataLeftAgeElectricalConductivity = doc.FormFields[57].Result;
                nodeList = doc.GetElementsByTagName("VE_R");
                node = nodeList.Item(nodeList.Count - 1);
                string dataRightAgeElectricalConductivity = node.InnerText;
                if (string.IsNullOrEmpty(dataRightAgeElectricalConductivity))
                {
                    shortFormReport.DataRightAgeElectricalConductivity = "0";// "";
                }
                else if (dataRightAgeElectricalConductivity.StartsWith("<5"))
                {
                    shortFormReport.DataRightAgeElectricalConductivity = "1";// App.Current.FindResource("ReportContext_111").ToString();
                }
                else if (dataRightAgeElectricalConductivity.StartsWith(">95"))
                {
                    shortFormReport.DataRightAgeElectricalConductivity = "3";// App.Current.FindResource("ReportContext_113").ToString();
                }
                else
                {
                    shortFormReport.DataRightAgeElectricalConductivity = "2";// App.Current.FindResource("ReportContext_112").ToString();
                }
                //ShortFormReport.DataRightAgeElectricalConductivity = doc.FormFields[58].Result;

                nodeList = doc.GetElementsByTagName("ZAKL");
                node = nodeList.Item(nodeList.Count - 1);
                string dataExamConclusion = node.InnerText;
                if (string.IsNullOrEmpty(dataExamConclusion))
                {
                    shortFormReport.DataExamConclusion = "0";//"";
                }
                else if (dataExamConclusion.StartsWith("Pubertal Period") || dataExamConclusion.StartsWith("青春期"))
                {
                    shortFormReport.DataExamConclusion = "1";// App.Current.FindResource("ReportContext_116").ToString();
                }
                else if (dataExamConclusion.StartsWith("Early reproductive period") || dataExamConclusion.StartsWith("育龄早期"))
                {
                    shortFormReport.DataExamConclusion = "2";// App.Current.FindResource("ReportContext_117").ToString();
                }
                else if (dataExamConclusion.StartsWith("Reproductive period") || dataExamConclusion.StartsWith("育龄期"))
                {
                    shortFormReport.DataExamConclusion = "3";// App.Current.FindResource("ReportContext_118").ToString();
                }
                else if (dataExamConclusion.StartsWith("Perimenopause period") || dataExamConclusion.StartsWith("围绝经期"))
                {
                    shortFormReport.DataExamConclusion = "4";// App.Current.FindResource("ReportContext_119").ToString();
                }
                else if (dataExamConclusion.StartsWith("Postmenopause period") || dataExamConclusion.StartsWith("Postmenopause period") || dataExamConclusion.StartsWith("绝经期"))
                {
                    shortFormReport.DataExamConclusion = "5";// App.Current.FindResource("ReportContext_120").ToString();
                }
                
                //ShortFormReport.DataExamConclusion = doc.FormFields[59].Result
                nodeList = doc.GetElementsByTagName("TIP_L");
                node = nodeList.Item(nodeList.Count - 1);
                string dataLeftMammaryGland = node.InnerText;
                if (string.IsNullOrEmpty(dataLeftMammaryGland))
                {
                    shortFormReport.DataLeftMammaryGland = "0";// "";
                }
                else if (dataLeftMammaryGland.StartsWith("Ductal type") || dataLeftMammaryGland.StartsWith("导管型乳腺结构") || dataLeftMammaryGland.StartsWith("导管式结构"))
                {
                    shortFormReport.DataLeftMammaryGland = "5";// App.Current.FindResource("ReportContext_126").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed structure with a") || dataLeftMammaryGland.StartsWith("混合型，导管型结构优势") || dataLeftMammaryGland.StartsWith("导管成分优先的"))
                {
                    shortFormReport.DataLeftMammaryGland = "4";// App.Current.FindResource("ReportContext_125").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed type of structure") || dataLeftMammaryGland.StartsWith("混合型乳腺结构") || dataLeftMammaryGland.StartsWith("混合式结构"))
                {
                    shortFormReport.DataLeftMammaryGland = "3";// App.Current.FindResource("ReportContext_124").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed with a predominance") || dataLeftMammaryGland.StartsWith("混合型，无定型结构优势") || ( dataLeftMammaryGland.Contains("无") && dataLeftMammaryGland.Contains("混合")))
                {
                    shortFormReport.DataLeftMammaryGland = "2";// App.Current.FindResource("ReportContext_123").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Amorphous type") || dataLeftMammaryGland.StartsWith("无定型乳腺结构") || (dataLeftMammaryGland.Contains("无") && !dataLeftMammaryGland.Contains("混合")))
                {
                    shortFormReport.DataLeftMammaryGland = "1";// App.Current.FindResource("ReportContext_122").ToString();
                }
                //ShortFormReport.DataLeftMammaryGland = doc.FormFields[60].Result;
                nodeList = doc.GetElementsByTagName("VE_GRANITSI_L");
                node = nodeList.Item(nodeList.Count - 1);
                string dataLeftAgeRelated = node.InnerText;
                if (string.IsNullOrEmpty(dataLeftAgeRelated))
                {
                    shortFormReport.DataLeftAgeRelated = "0";//"";
                }
                else if (dataLeftAgeRelated.StartsWith("<5"))
                {
                    shortFormReport.DataLeftAgeRelated = "1";// App.Current.FindResource("ReportContext_111").ToString();
                }
                else if (dataLeftAgeRelated.StartsWith(">95"))
                {
                    shortFormReport.DataLeftAgeRelated = "3";// App.Current.FindResource("ReportContext_113").ToString();
                }
                else
                {
                    shortFormReport.DataLeftAgeRelated = "2";// App.Current.FindResource("ReportContext_112").ToString();
                }
                //ShortFormReport.DataLeftAgeRelated = doc.FormFields[61].Result;
                nodeList = doc.GetElementsByTagName("TIP_R");
                node = nodeList.Item(nodeList.Count - 1);
                string dataRightMammaryGland = node.InnerText;
                if (string.IsNullOrEmpty(dataLeftMammaryGland))
                {
                    shortFormReport.DataRightMammaryGland = "0";// "";
                }
                else if (dataLeftMammaryGland.StartsWith("Ductal type") || dataLeftMammaryGland.StartsWith("导管型乳腺结构") || dataLeftMammaryGland.StartsWith("导管式结构"))
                {
                    shortFormReport.DataRightMammaryGland = "5";// App.Current.FindResource("ReportContext_126").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed structure with a") || dataLeftMammaryGland.StartsWith("混合型，导管型结构优势") || dataLeftMammaryGland.StartsWith("导管成分优先的"))
                {
                    shortFormReport.DataRightMammaryGland = "4";// App.Current.FindResource("ReportContext_125").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed type of structure") || dataLeftMammaryGland.StartsWith("Mixed type of structure") || dataLeftMammaryGland.StartsWith("混合型乳腺结构") || dataLeftMammaryGland.StartsWith("混合式结构"))
                {
                    shortFormReport.DataRightMammaryGland = "3";// App.Current.FindResource("ReportContext_124").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed with a predominance") || dataLeftMammaryGland.StartsWith("混合型，无定型结构优势") || (dataLeftMammaryGland.Contains("无") && dataLeftMammaryGland.Contains("混合")))
                {
                    shortFormReport.DataRightMammaryGland = "2";// App.Current.FindResource("ReportContext_123").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Amorphous type") || dataLeftMammaryGland.StartsWith("无定型乳腺结构") || (dataLeftMammaryGland.Contains("无") && !dataLeftMammaryGland.Contains("混合")))
                {
                    shortFormReport.DataRightMammaryGland = "1";// App.Current.FindResource("ReportContext_122").ToString();
                }
                //ShortFormReport.DataLeftMammaryGland = doc.FormFields[60].Result;
                nodeList = doc.GetElementsByTagName("VE_GRANITSI_R");
                node = nodeList.Item(nodeList.Count - 1);
                string dataRightAgeRelated = node.InnerText;
                if (string.IsNullOrEmpty(dataRightAgeRelated))
                {
                    shortFormReport.DataRightAgeRelated = "0";
                }
                else if (dataRightAgeRelated.StartsWith("<5"))
                {
                    shortFormReport.DataRightAgeRelated = "1";
                }
                else if (dataRightAgeRelated.StartsWith(">95"))
                {
                    shortFormReport.DataRightAgeRelated = "3";
                }
                else
                {
                    shortFormReport.DataRightAgeRelated = "2";
                }

                nodeList = doc.GetElementsByTagName("BAL");
                node = nodeList.Item(nodeList.Count - 1);
                string totalPts= node.InnerText;
                if (string.IsNullOrEmpty(totalPts))
                {
                    shortFormReport.DataTotalPts = "0";
                }
                else
                {
                    shortFormReport.DataTotalPts = totalPts.Substring(0,1);
                }

                nodeList = doc.GetElementsByTagName("BIEIM_KATEG");
                node = nodeList.Item(nodeList.Count - 1);
                string categoryId= node.InnerText;
                if (string.IsNullOrEmpty(categoryId))
                {
                    shortFormReport.DataBiRadsCategory = "0";
                }
                else
                {                    
                    shortFormReport.DataBiRadsCategory = (Convert.ToInt32(categoryId.Substring(0, 1))+1)+"";
                }

                nodeList = doc.GetElementsByTagName("RECOM_S");
                node = nodeList.Item(nodeList.Count - 1);
                string recommend = node.InnerText;
                if (string.IsNullOrEmpty(recommend))
                {
                    shortFormReport.DataFurtherExam = "0";
                }
                else if (recommend.StartsWith("Routine mammography"))
                {
                    shortFormReport.DataFurtherExam = "1";
                }
                else if (recommend.StartsWith("Re-examination in 6 months"))
                {
                    shortFormReport.DataFurtherExam = "2";
                }
                else if (recommend.StartsWith("Biopsy"))
                {
                    shortFormReport.DataFurtherExam = "3";
                }

                nodeList = doc.GetElementsByTagName("RECOM_T");
                node = nodeList.Item(nodeList.Count - 1);
                shortFormReport.DataComments= node.InnerText;
                                
            }
            catch(Exception){}
            
            return shortFormReport;
        }


    }
}
