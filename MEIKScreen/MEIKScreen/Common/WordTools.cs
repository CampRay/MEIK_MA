using MEIKScreen.Model;
using System;
using Word = Microsoft.Office.Interop.Word;

namespace MEIKScreen.Common
{
    public class WordTools
    {
        public static ShortFormReport ReadWordFile(string wordFilePath)
        {
            
            if (FileHelper.FileInUsed(wordFilePath))
            {
                return null;
            }
            ShortFormReport shortFormReport = new ShortFormReport();
            Word.Application wordApp = new Word.Application();
            object unknow = Type.Missing;
            wordApp.Visible = false;
            wordApp.DisplayAlerts = Word.WdAlertLevel.wdAlertsNone;
            Object file = wordFilePath;
            Word.Document doc = null;
                        
            try
            {
                doc = wordApp.Documents.Open(ref file);
                string dataMenstrualCycle = doc.FormFields[11].Result;
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
                else if (dataMenstrualCycle.StartsWith("missing") || dataMenstrualCycle.StartsWith("postmenopausal") || dataMenstrualCycle.StartsWith("postmenopause") || dataMenstrualCycle.StartsWith("绝经期"))
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
                //shortFormReport.DataMenstrualCycle = doc.FormFields[11].Result;
                //shortFormReport.DataLeftChangesOfElectricalConductivity = doc.FormFields[15].Result;
                //shortFormReport.DataRightChangesOfElectricalConductivity = doc.FormFields[16].Result;
                //shortFormReport.DataLeftMammaryStruct = doc.FormFields[17].Result;
                //shortFormReport.DataRightMammaryStruct = doc.FormFields[18].Result;
                //shortFormReport.DataLeftLactiferousSinusZone = doc.FormFields[19].Result;
                //shortFormReport.DataRightLactiferousSinusZone = doc.FormFields[20].Result;
                //shortFormReport.DataLeftMammaryContour = doc.FormFields[21].Result;
                //shortFormReport.DataLeftMammaryContour = doc.FormFields[22].Result;

                //shortFormReport.DataLeftLocation = doc.FormFields[23].Result;
                //shortFormReport.DataRightLocation = doc.FormFields[24].Result;
                //shortFormReport.DataLeftNumber = doc.FormFields[25].Result;
                //shortFormReport.DataRightNumber = doc.FormFields[26].Result;
                //shortFormReport.DataLeftSize = doc.FormFields[27].Result;
                //shortFormReport.DataRightSize = doc.FormFields[28].Result;
                //shortFormReport.DataLeftShape = doc.FormFields[29].Result;
                //shortFormReport.DataRightShape = doc.FormFields[30].Result;
                //shortFormReport.DataLeftContourAroundFocus = doc.FormFields[31].Result;
                //shortFormReport.DataRightContourAroundFocus = doc.FormFields[32].Result;
                //shortFormReport.DataLeftInternalElectricalStructure = doc.FormFields[33].Result;
                //shortFormReport.DataRightInternalElectricalStructure = doc.FormFields[34].Result;
                //shortFormReport.DataLeftSurroundingTissues = doc.FormFields[35].Result;
                //shortFormReport.DataRightSurroundingTissues = doc.FormFields[36].Result;


                shortFormReport.DataLeftMeanElectricalConductivity1 = doc.FormFields[37].Result;
                shortFormReport.DataRightMeanElectricalConductivity1 = doc.FormFields[38].Result;
                shortFormReport.DataLeftMeanElectricalConductivity2 = doc.FormFields[39].Result;
                shortFormReport.DataRightMeanElectricalConductivity2 = doc.FormFields[40].Result;
                string dataMeanElectricalConductivity3 = doc.FormFields[41].Result;
                if (string.IsNullOrEmpty(dataMeanElectricalConductivity3))
                {
                    shortFormReport.DataMeanElectricalConductivity3 = "0";//"";
                }
                else if (dataMeanElectricalConductivity3.StartsWith("postmenopause") || dataMeanElectricalConductivity3.StartsWith("绝经后期"))
                {
                    shortFormReport.DataMeanElectricalConductivity3 = "1";// App.Current.FindResource("ReportContext_103").ToString();
                }
                else if (dataMeanElectricalConductivity3.StartsWith("pregnancy") || dataMeanElectricalConductivity3.StartsWith("妊娠"))
                {
                    shortFormReport.DataMeanElectricalConductivity3 = "2";// App.Current.FindResource("ReportContext_104").ToString();
                }
                else if (dataMeanElectricalConductivity3.StartsWith("lactation") || dataMeanElectricalConductivity3.StartsWith("哺乳期"))
                {
                    shortFormReport.DataMeanElectricalConductivity3 = "3";// App.Current.FindResource("ReportContext_105").ToString();
                }
                //ShortFormReport.DataMeanElectricalConductivity3 = doc.FormFields[41].Result;
                shortFormReport.DataLeftMeanElectricalConductivity3 = doc.FormFields[42].Result;
                shortFormReport.DataRightMeanElectricalConductivity3 = doc.FormFields[43].Result;

                shortFormReport.DataLeftComparativeElectricalConductivity1 = doc.FormFields[44].Result;
                //ShortFormReport.DataRightComparativeElectricalConductivity1 = doc.FormFields[44].Result;
                shortFormReport.DataLeftComparativeElectricalConductivity2 = doc.FormFields[45].Result;
                //ShortFormReport.DataRightComparativeElectricalConductivity2 = doc.FormFields[45].Result;
                shortFormReport.DataLeftComparativeElectricalConductivity3 = doc.FormFields[46].Result;
                //ShortFormReport.DataRightComparativeElectricalConductivity3 = doc.FormFields[46].Result;
                shortFormReport.DataLeftDivergenceBetweenHistograms1 = doc.FormFields[47].Result;
                //ShortFormReport.DataRightDivergenceBetweenHistograms1 = doc.FormFields[47].Result;
                shortFormReport.DataLeftDivergenceBetweenHistograms2 = doc.FormFields[48].Result;
                //ShortFormReport.DataRightDivergenceBetweenHistograms2 = doc.FormFields[48].Result;
                shortFormReport.DataLeftDivergenceBetweenHistograms3 = doc.FormFields[49].Result;
                //ShortFormReport.DataRightDivergenceBetweenHistograms3 = doc.FormFields[49].Result;

                shortFormReport.DataLeftPhaseElectricalConductivity = doc.FormFields[54].Result;
                shortFormReport.DataRightPhaseElectricalConductivity = doc.FormFields[55].Result;

                shortFormReport.DataAgeElectricalConductivityReference = doc.FormFields[56].Result;

                string dataLeftAgeElectricalConductivity = doc.FormFields[57].Result;
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
                string dataRightAgeElectricalConductivity = doc.FormFields[58].Result;
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
                string dataExamConclusion = doc.FormFields[59].Result;
                if (string.IsNullOrEmpty(dataExamConclusion))
                {
                    shortFormReport.DataExamConclusion = "0";//"";
                }
                else if (dataExamConclusion.StartsWith("Pubertal Period") || dataExamConclusion.StartsWith("青春期"))
                {
                    shortFormReport.DataExamConclusion = "1";// App.Current.FindResource("ReportContext_116").ToString();
                }
                else if (dataExamConclusion.StartsWith("Early childbearing age") || dataExamConclusion.StartsWith("育龄早期"))
                {
                    shortFormReport.DataExamConclusion = "2";// App.Current.FindResource("ReportContext_117").ToString();
                }
                else if (dataExamConclusion.StartsWith("Childbearing age") || dataExamConclusion.StartsWith("育龄期"))
                {
                    shortFormReport.DataExamConclusion = "3";// App.Current.FindResource("ReportContext_118").ToString();
                }
                else if (dataExamConclusion.StartsWith("Perimenopausal period") || dataExamConclusion.StartsWith("围绝经期"))
                {
                    shortFormReport.DataExamConclusion = "4";// App.Current.FindResource("ReportContext_119").ToString();
                }
                else if (dataExamConclusion.StartsWith("Postmenopausal period") || dataExamConclusion.StartsWith("Postmenopause period") || dataExamConclusion.StartsWith("绝经期"))
                {
                    shortFormReport.DataExamConclusion = "5";// App.Current.FindResource("ReportContext_120").ToString();
                }
                
                //ShortFormReport.DataExamConclusion = doc.FormFields[59].Result;
                string dataLeftMammaryGland=doc.FormFields[60].Result;
                if (string.IsNullOrEmpty(dataLeftMammaryGland))
                {
                    shortFormReport.DataLeftMammaryGland = "0";// "";
                }
                else if (dataLeftMammaryGland.StartsWith("Ductal type") || dataLeftMammaryGland.StartsWith("导管型乳腺结构") || dataLeftMammaryGland.StartsWith("导管式结构"))
                {
                    shortFormReport.DataLeftMammaryGland = "5";// App.Current.FindResource("ReportContext_126").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed type with ductal component predominance") || dataLeftMammaryGland.StartsWith("混合型，导管型结构优势") || dataLeftMammaryGland.StartsWith("导管成分优先的"))
                {
                    shortFormReport.DataLeftMammaryGland = "4";// App.Current.FindResource("ReportContext_125").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed type of mammary gland structure") || dataLeftMammaryGland.StartsWith("Mixed type of structure") || dataLeftMammaryGland.StartsWith("混合型乳腺结构") || dataLeftMammaryGland.StartsWith("混合式结构"))
                {
                    shortFormReport.DataLeftMammaryGland = "3";// App.Current.FindResource("ReportContext_124").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed type with amorphous component predominance") || dataLeftMammaryGland.StartsWith("混合型，无定型结构优势") || ( dataLeftMammaryGland.Contains("无") && dataLeftMammaryGland.Contains("混合")))
                {
                    shortFormReport.DataLeftMammaryGland = "2";// App.Current.FindResource("ReportContext_123").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Amorphous type") || dataLeftMammaryGland.StartsWith("无定型乳腺结构") || (dataLeftMammaryGland.Contains("无") && !dataLeftMammaryGland.Contains("混合")))
                {
                    shortFormReport.DataLeftMammaryGland = "1";// App.Current.FindResource("ReportContext_122").ToString();
                }
                //ShortFormReport.DataLeftMammaryGland = doc.FormFields[60].Result;
                string dataLeftAgeRelated = doc.FormFields[61].Result;
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

                string dataRightMammaryGland = doc.FormFields[64].Result;
                if (string.IsNullOrEmpty(dataLeftMammaryGland))
                {
                    shortFormReport.DataRightMammaryGland = "0";// "";
                }
                else if (dataLeftMammaryGland.StartsWith("Ductal type") || dataLeftMammaryGland.StartsWith("导管型乳腺结构") || dataLeftMammaryGland.StartsWith("导管式结构"))
                {
                    shortFormReport.DataRightMammaryGland = "5";// App.Current.FindResource("ReportContext_126").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed type with ductal component predominance") || dataLeftMammaryGland.StartsWith("混合型，导管型结构优势") || dataLeftMammaryGland.StartsWith("导管成分优先的"))
                {
                    shortFormReport.DataRightMammaryGland = "4";// App.Current.FindResource("ReportContext_125").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed type of mammary gland structure") || dataLeftMammaryGland.StartsWith("Mixed type of structure") || dataLeftMammaryGland.StartsWith("混合型乳腺结构") || dataLeftMammaryGland.StartsWith("混合式结构"))
                {
                    shortFormReport.DataRightMammaryGland = "3";// App.Current.FindResource("ReportContext_124").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Mixed type with amorphous component predominance") || dataLeftMammaryGland.StartsWith("混合型，无定型结构优势") || (dataLeftMammaryGland.Contains("无") && dataLeftMammaryGland.Contains("混合")))
                {
                    shortFormReport.DataRightMammaryGland = "2";// App.Current.FindResource("ReportContext_123").ToString();
                }
                else if (dataLeftMammaryGland.StartsWith("Amorphous type") || dataLeftMammaryGland.StartsWith("无定型乳腺结构") || (dataLeftMammaryGland.Contains("无") && !dataLeftMammaryGland.Contains("混合")))
                {
                    shortFormReport.DataRightMammaryGland = "1";// App.Current.FindResource("ReportContext_122").ToString();
                }
                //ShortFormReport.DataLeftMammaryGland = doc.FormFields[60].Result;
                string dataRightAgeRelated = doc.FormFields[65].Result;
                if (string.IsNullOrEmpty(dataRightAgeRelated))
                {
                    shortFormReport.DataRightAgeRelated = "0";// "";
                }
                else if (dataRightAgeRelated.StartsWith("<5"))
                {
                    shortFormReport.DataRightAgeRelated = "1";// App.Current.FindResource("ReportContext_111").ToString();
                }
                else if (dataRightAgeRelated.StartsWith(">95"))
                {
                    shortFormReport.DataRightAgeRelated = "3";// App.Current.FindResource("ReportContext_113").ToString();
                }
                else
                {
                    shortFormReport.DataRightAgeRelated = "2";// App.Current.FindResource("ReportContext_112").ToString();
                }

                //ShortFormReport.DataRightMammaryGland = doc.FormFields[64].Result;
                //ShortFormReport.DataRightAgeRelated = doc.FormFields[65].Result;
                                

            }
            catch(Exception){}
            finally
            {
                Type wordType = wordApp.GetType();
                try
                {
                    if (doc != null)
                    {
                        doc.Close();
                    }
                    if (wordApp != null)
                    {
                        wordApp.Quit();
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        wordType.InvokeMember("Quit", System.Reflection.BindingFlags.InvokeMethod, null, wordApp, null);
                        doc = null;
                        wordApp = null;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                    catch (Exception) { }
                }
            }
            return shortFormReport;
        }


    }
}
