using MEIKScreen.Common;
using MEIKScreen.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace MEIKScreen
{
    
    public partial class SearchClient : Window
    {        
        private ObservableCollection<Person> patientCollection;
        public SearchClient()
        {
            InitializeComponent();
            this.txtClientNo.Focus();                        
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtClientNo.Text) && string.IsNullOrEmpty(this.txtLastName.Text) && string.IsNullOrEmpty(this.txtFirstName.Text)
                && string.IsNullOrEmpty(this.txtMiddleName.Text) && string.IsNullOrEmpty(this.txtMobile.Text) && string.IsNullOrEmpty(this.txtEmail.Text)
                && string.IsNullOrEmpty(this.txtBirthDate.Text) && string.IsNullOrEmpty(this.txtBirthMonth.Text) && string.IsNullOrEmpty(this.txtBirthYear.Text))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_87").ToString());
                this.txtClientNo.Focus();
            }            
            else
            {                
                try
                {                    
                    NameValueCollection nvlist = new NameValueCollection();
                    nvlist.Add("clientId", this.txtClientNo.Text);
                    nvlist.Add("lastName", this.txtLastName.Text);
                    nvlist.Add("firstName", this.txtFirstName.Text);
                    nvlist.Add("otherName", this.txtMiddleName.Text);
                    if (!string.IsNullOrEmpty(this.txtBirthDate.Text) && !string.IsNullOrEmpty(this.txtBirthMonth.Text) && !string.IsNullOrEmpty(this.txtBirthYear.Text))
                    {
                        nvlist.Add("birthday", this.txtBirthYear.Text + "/" + this.txtBirthMonth.Text.PadLeft(2, '0') + "/" + this.txtBirthDate.Text.PadLeft(2, '0'));
                    }
                    nvlist.Add("mobile", this.txtMobile.Text);
                    nvlist.Add("email", this.txtEmail.Text);
                    string resStr = HttpWebTools.Post(App.reportSettingModel.CloudPath + "/api/getClientInfo", nvlist, App.reportSettingModel.CloudToken);
                    var jsonObj = JObject.Parse(resStr);
                    bool status = (bool)jsonObj["status"];
                    List<Person> personList = new List<Person>();
                    if (status)
                    {                        
                        var personArr = jsonObj["data"] as JArray;
                        foreach (JObject personItem in personArr)
                        {
                            Person person = new Person();
                            //获取用户已登记的帐户ID
                            person.Cid = (string)personItem["cid"];
                            //获取UserInfo表的ID值
                            person.ClientNumber = (string)personItem["infoId"];
                            person.SurName = (string)personItem["lastName"];
                            person.GivenName = (string)personItem["firstName"];
                            person.OtherName = (string)personItem["otherName"];
                            person.FullName = person.SurName;
                            if (!string.IsNullOrEmpty(person.GivenName))
                            {
                                person.FullName = person.FullName + "," + person.GivenName;
                            }
                            if (!string.IsNullOrEmpty(person.OtherName))
                            {
                                person.FullName = person.FullName + " " + person.OtherName;
                            }

                            person.Birthday = (string)personItem["birthday"];
                            if (!string.IsNullOrEmpty(person.Birthday))
                            {
                                string[] strArr = person.Birthday.Split('/');
                                if (strArr.Count() == 3)
                                {
                                    person.BirthYear = strArr[0];
                                    person.BirthMonth = strArr[1];
                                    person.BirthDate = strArr[2];
                                    
                                }
                            }

                            person.Mobile = (string)personItem["mobile"];
                            person.Email = (string)personItem["email"];
                            person.Address = (string)personItem["address"];
                            person.Height = (string)personItem["height"];
                            person.Weight = (string)personItem["weight"];
                            person.Address = (string)personItem["address"];
                            person.FamilyBreastCancer1 = Convert.ToBoolean(((string)personItem["familyBreastCancer1"]));
                            person.FamilyBreastCancer2 = Convert.ToBoolean(((string)personItem["familyBreastCancer2"]));
                            person.FamilyBreastCancer3 = Convert.ToBoolean(((string)personItem["familyBreastCancer3"]));
                            person.FamilyUterineCancer1 = Convert.ToBoolean(((string)personItem["familyUterineCancer1"]));
                            person.FamilyUterineCancer2 = Convert.ToBoolean(((string)personItem["familyUterineCancer2"]));
                            person.FamilyUterineCancer3 = Convert.ToBoolean(((string)personItem["familyUterineCancer3"]));
                            person.FamilyCervicalCancer1 = Convert.ToBoolean(((string)personItem["familyCervicalCancer1"]));
                            person.FamilyCervicalCancer2 = Convert.ToBoolean(((string)personItem["familyCervicalCancer2"]));
                            person.FamilyCervicalCancer3 = Convert.ToBoolean(((string)personItem["familyCervicalCancer3"]));
                            person.FamilyOvarianCancer1 = Convert.ToBoolean(((string)personItem["familyOvarianCancer1"]));
                            person.FamilyOvarianCancer2 = Convert.ToBoolean(((string)personItem["familyOvarianCancer2"]));
                            person.FamilyOvarianCancer3 = Convert.ToBoolean(((string)personItem["familyOvarianCancer3"]));

                            person.PalpableLumps = Convert.ToBoolean(((string)personItem["complaintsPalpableLumps"]));
                            person.Pain = Convert.ToBoolean(((string)personItem["complaintsPain"]));
                            person.Degree = Convert.ToInt32((string)personItem["complaintsDegree"]);
                            person.Colostrum = Convert.ToBoolean(((string)personItem["complaintsColostrum"]));
                            person.SerousDischarge = Convert.ToBoolean(((string)personItem["complaintsSerous"]));
                            person.BloodDischarge = Convert.ToBoolean(((string)personItem["complaintsBlood"]));
                            person.LeftPosition = Convert.ToInt32((string)personItem["complaintsLumpsLeftPosition"]);
                            person.RightPosition = Convert.ToInt32((string)personItem["complaintsLumpsRightPosition"]);
                            person.Pregnancy = Convert.ToBoolean(((string)personItem["complaintsPregnancy"]));
                            person.PregnancyTerm = (string)personItem["complaintsPregnancyTerm"];

                            person.Lactation = Convert.ToBoolean(((string)personItem["complaintsLactation"]));
                            person.LactationTerm = (string)personItem["complaintsLactationTerm"];
                            person.BreastImplants = Convert.ToBoolean(((string)personItem["complaintsBreastImplants"]));
                            person.BreastImplantsLeft = Convert.ToBoolean(((string)personItem["complaintsBreastImplantsLeft"]));
                            person.BreastImplantsRight = Convert.ToBoolean(((string)personItem["complaintsBreastImplantsRight"]));
                            person.BreastImplantsLeftYear = (string)personItem["complaintsBreastImplantsLeftYear"];
                            person.BreastImplantsRightYear = (string)personItem["complaintsBreastImplantsRightYear"];
                            person.MaterialsGel = Convert.ToBoolean(((string)personItem["complaintsBreastImplantsGel"]));
                            person.MaterialsFat = Convert.ToBoolean(((string)personItem["complaintsBreastImplantsFat"]));
                            person.MaterialsOthers = Convert.ToBoolean(((string)personItem["complaintsBreastImplantsOthers"]));
                            person.DateLastMenstruation = (string)personItem["mensesLastMenstruation"];

                            person.MenstrualCycleDisorder = Convert.ToBoolean(((string)personItem["mensesCycleDisorder"]));
                            person.Postmenopause = Convert.ToBoolean(((string)personItem["mensesPostmenopause"]));
                            person.PostmenopauseYear = (string)personItem["mensesPostmenopauseYear"];
                            person.HormonalContraceptives = Convert.ToBoolean(((string)personItem["mensesHormonalContraceptives"]));
                            person.HormonalContraceptivesBrandName = (string)personItem["mensesHormonalContraceptivesName"];
                            person.HormonalContraceptivesPeriod = (string)personItem["mensesHormonalContraceptivesPeriod"];
                            person.Infertility = Convert.ToBoolean(((string)personItem["obstetricInfertility"]));
                            person.Abortions = Convert.ToBoolean(((string)personItem["obstetricAbortions"]));
                            person.AbortionsNumber = (string)personItem["obstetricAbortionsTimes"];
                            person.Births = Convert.ToBoolean(((string)personItem["obstetricBirths"]));
                            person.BirthsNumber = (string)personItem["obstetricBirthsTimes"];
                            person.LactationTill1Month = Convert.ToBoolean(((string)personItem["obstetricLactationUnderMonth"]));
                            person.LactationTill1Year = Convert.ToBoolean(((string)personItem["obstetricLactationUnderYear"]));
                            person.LactationOver1Year = Convert.ToBoolean(((string)personItem["obstetricLactationOverYear"]));
                            person.Trauma = Convert.ToBoolean(((string)personItem["anamnesisBreastDiseasesTrauma"]));
                            person.Mastitis = Convert.ToBoolean(((string)personItem["anamnesisBreastDiseasesMastitis"]));
                            person.FibrousCysticMastopathy = Convert.ToBoolean(((string)personItem["anamnesisBreastDiseasesFibrous"]));
                            person.Cysts = Convert.ToBoolean(((string)personItem["anamnesisBreastDiseasesCysts"]));
                            person.Cancer = Convert.ToBoolean(((string)personItem["anamnesisBreastDiseasesCancer"]));
                            person.OvaryDiseases = Convert.ToBoolean(((string)personItem["anamnesisOvaryDiseasesInflammation"]));
                            person.OvaryCyst = Convert.ToBoolean(((string)personItem["anamnesisOvaryDiseasesCyst"]));
                            person.OvaryCancer = Convert.ToBoolean(((string)personItem["anamnesisOvaryDiseasesCancer"]));
                            person.OvaryEndometriosis = Convert.ToBoolean(((string)personItem["anamnesisOvaryDiseasesEndometriosis"]));
                            person.UterusDiseases = Convert.ToBoolean(((string)personItem["anamnesisUterusDiseasesInflammation"]));
                            person.UterusMyoma = Convert.ToBoolean(((string)personItem["anamnesisUterusDiseasesMyoma"]));
                            person.UterusCancer = Convert.ToBoolean(((string)personItem["anamnesisUterusDiseasesCancer"]));
                            person.UterusEndometriosis = Convert.ToBoolean(((string)personItem["anamnesisUterusDiseasesEndometriosis"]));
                            person.Adiposity = Convert.ToBoolean(((string)personItem["anamnesisSomaticDiseasesAdiposity"]));
                            person.EssentialHypertension = Convert.ToBoolean(((string)personItem["anamnesisSomaticDiseasesHypertension"]));
                            person.Diabetes = Convert.ToBoolean(((string)personItem["anamnesisSomaticDiseasesDiabetes"]));
                            person.ThyroidGlandDiseases = Convert.ToBoolean(((string)personItem["anamnesisSomaticDiseasesThyroid"]));
                            person.Palpation = Convert.ToBoolean(((string)personItem["examinationsPalpation"]));
                            person.PalationYear = (string)personItem["examinationsPalpationYear"];
                            person.PalpationNormal = Convert.ToBoolean(((string)personItem["examinationsPalpationNormal"]));
                            person.PalpationStatus = Convert.ToBoolean(((string)personItem["examinationsPalpationAbnormal"]));
                            person.PalpationDiffuse = Convert.ToBoolean(((string)personItem["examinationsPalpationDiffuse"]));
                            person.PalpationFocal = Convert.ToBoolean(((string)personItem["examinationsPalpationFocal"]));
                            person.Ultrasound = Convert.ToBoolean(((string)personItem["examinationsUltrasound"]));
                            person.UltrasoundYear = (string)personItem["examinationsUltrasoundYear"];
                            person.UltrasoundNormal = Convert.ToBoolean(((string)personItem["examinationsUltrasoundNormal"]));
                            person.UltrasoundStatus = Convert.ToBoolean(((string)personItem["examinationsUltrasoundAbnormal"]));
                            person.UltrasoundDiffuse = Convert.ToBoolean(((string)personItem["examinationsUltrasoundDiffuse"]));
                            person.UltrasoundFocal = Convert.ToBoolean(((string)personItem["examinationsUltrasoundFocal"]));
                            person.Mammography = Convert.ToBoolean(((string)personItem["examinationsMammography"]));
                            person.MammographyYear = (string)personItem["examinationsMammographyYear"];
                            person.MammographyNormal = Convert.ToBoolean(((string)personItem["examinationsMammographyNormal"]));
                            person.MammographyStatus = Convert.ToBoolean(((string)personItem["examinationsMammographyAbnormal"]));
                            person.MammographyDiffuse = Convert.ToBoolean(((string)personItem["examinationsMammographyDiffuse"]));
                            person.MammographyFocal = Convert.ToBoolean(((string)personItem["examinationsMammographyFocal"]));
                            person.Biopsy = Convert.ToBoolean(((string)personItem["examinationsBiopsy"]));
                            person.BiopsyYear = (string)personItem["examinationsBiopsyYear"];
                            person.BiopsyNormal = Convert.ToBoolean(((string)personItem["examinationsBiopsyNormal"]));
                            person.BiopsyStatus = Convert.ToBoolean(((string)personItem["examinationsBiopsyAbnormal"]));
                            person.BiopsyCancer = Convert.ToBoolean(((string)personItem["examinationsBiopsyCancer"]));
                            person.BiopsyProliferation = Convert.ToBoolean(((string)personItem["examinationsBiopsyProliferation"]));
                            person.BiopsyDysplasia = Convert.ToBoolean(((string)personItem["examinationsBiopsyDysplasia"]));
                            person.BiopsyIntraductalPapilloma = Convert.ToBoolean(((string)personItem["examinationsBiopsyPapilloma"]));
                            person.BiopsyFibroadenoma = Convert.ToBoolean(((string)personItem["examinationsBiopsyFibroadenoma"]));
                            personList.Add(person);

                        }
                        
                    }
                    patientCollection = new ObservableCollection<Person>(personList);
                }
                catch(Exception ex) {
                    MessageBox.Show(this, ex.Message);
                }
                finally
                {
                    patientGrid.ItemsSource = patientCollection;
                }
            }
        }


        private void DG_Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (patientGrid.SelectedItem == null)
            {
                return;
            }
            string patientFolder = null;
            string code = getLastCode();
            try{
                var person = patientGrid.SelectedItem as Person;

                //string dayFolder = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("MM_yyyy") + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("dd");
                string monthFolder = App.reportSettingModel.DataBaseFolder + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy_MM");
                if (!Directory.Exists(monthFolder))
                {
                    Directory.CreateDirectory(monthFolder);
                }
                            
                patientFolder = monthFolder + System.IO.Path.DirectorySeparatorChar + code + "-" + person.SurName;
                if (!string.IsNullOrEmpty(person.GivenName))
                {
                    patientFolder = patientFolder + "," + person.GivenName;
                }
                if (!string.IsNullOrEmpty(person.OtherName))
                {
                    patientFolder = patientFolder + " " + person.OtherName;
                }
                if (!Directory.Exists(patientFolder))
                {
                    Directory.CreateDirectory(patientFolder);                    
                    //創建用戶信息文件
                    string patientFile = patientFolder + System.IO.Path.DirectorySeparatorChar + code + ".ini";
                    if (!File.Exists(patientFile))
                    {
                        var fs = File.Create(patientFile);
                        fs.Close();
                    }
                    person.IniFilePath=patientFile;
                    OperateIniFile.WriteIniData("Personal data", "clientnumber", person.ClientNumber, patientFile);
                    OperateIniFile.WriteIniData("Personal data", "surname", person.SurName, patientFile);
                    OperateIniFile.WriteIniData("Personal data", "given name", person.GivenName, patientFile);
                    OperateIniFile.WriteIniData("Personal data", "other name", person.OtherName, patientFile);
                    OperateIniFile.WriteIniData("Personal data", "birth date", person.BirthDate, patientFile);
                    OperateIniFile.WriteIniData("Personal data", "birth month", person.BirthMonth, patientFile);
                    OperateIniFile.WriteIniData("Personal data", "birth year", person.BirthYear, patientFile);

                    OperateIniFile.WriteIniData("Report", "Technician Name Required", App.reportSettingModel.ShowTechSignature.ToString(), patientFile);                    
                    if (!string.IsNullOrEmpty(App.reportSettingModel.CloudUser))
                    {
                        OperateIniFile.WriteIniData("Report", "Technician Name", App.reportSettingModel.CloudUser, patientFile);                        
                    }
                    OperateIniFile.WriteIniData("Report", "Screen Venue", App.reportSettingModel.ScreenVenue, patientFile);

                    OperateIniFile.WriteIniData("Personal data", "address", person.Address, patientFile); 
                    OperateIniFile.WriteIniData("Personal data", "height", person.Height, patientFile);                
                    OperateIniFile.WriteIniData("Personal data", "weight", person.Weight, patientFile);                
                    OperateIniFile.WriteIniData("Personal data", "mobile", person.Mobile, patientFile);                
                    OperateIniFile.WriteIniData("Personal data", "email", person.Email, patientFile);                                             
                    
                    //Family History                
                    OperateIniFile.WriteIniData("Family History", "FamilyBreastCancer1", person.FamilyBreastCancer1 ? "1" : "0", patientFile);                
                    OperateIniFile.WriteIniData("Family History", "FamilyBreastCancer2", person.FamilyBreastCancer2 ? "1" : "0", patientFile);                
                    OperateIniFile.WriteIniData("Family History", "FamilyBreastCancer3", person.FamilyBreastCancer3 ? "1" : "0", patientFile);               
               
                    OperateIniFile.WriteIniData("Family History", "FamilyUterineCancer1", person.FamilyUterineCancer1 ? "1" : "0", patientFile);               
                    OperateIniFile.WriteIniData("Family History", "FamilyUterineCancer2",  person.FamilyUterineCancer2 ? "1" : "0", patientFile);               
                    OperateIniFile.WriteIniData("Family History", "FamilyUterineCancer3", person.FamilyUterineCancer3 ? "1" : "0", patientFile);
                               
                    OperateIniFile.WriteIniData("Family History", "FamilyCervicalCancer1", person.FamilyCervicalCancer1? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Family History", "FamilyCervicalCancer2", person.FamilyCervicalCancer2? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Family History", "FamilyCervicalCancer3", person.FamilyCervicalCancer3? "1" : "0", patientFile);
                
                    OperateIniFile.WriteIniData("Family History", "FamilyOvarianCancer1", person.FamilyOvarianCancer1? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Family History", "FamilyOvarianCancer2", person.FamilyOvarianCancer2? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Family History", "FamilyOvarianCancer3", person.FamilyOvarianCancer3? "1" : "0", patientFile);                
                
                    //Complaints                
                    OperateIniFile.WriteIniData("Complaints", "palpable lumps", person.PalpableLumps? "1" : "0", patientFile);                    
                    if(person.PalpableLumps){
                        OperateIniFile.WriteIniData("Complaints", "left position", person.LeftPosition.ToString(), patientFile);
                        OperateIniFile.WriteIniData("Complaints", "right position", person.RightPosition.ToString(), patientFile);
                    }  
                    OperateIniFile.WriteIniData("Complaints", "pain", person.Pain ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "degree", person.Degree.ToString(), patientFile);

                    OperateIniFile.WriteIniData("Complaints", "colostrum", person.Colostrum? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "serous discharge",  person.SerousDischarge ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "blood discharge",  person.BloodDischarge? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "other", person.Other? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "pregnancy", person.Pregnancy? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "lactation", person.Lactation ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "lactation term", person.LactationTerm, patientFile);
                    OperateIniFile.WriteIniData("Complaints", "pregnancy term", person.PregnancyTerm, patientFile);

                    OperateIniFile.WriteIniData("Complaints", "BreastImplants", person.BreastImplants? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "BreastImplantsLeft", person.BreastImplantsLeft? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "BreastImplantsRight", person.BreastImplantsRight ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "MaterialsGel", person.MaterialsGel ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "MaterialsFat", person.MaterialsFat? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Complaints", "MaterialsOthers", person.MaterialsOthers? "1" : "0", patientFile);

                    OperateIniFile.WriteIniData("Complaints", "BreastImplantsLeftYear", person.BreastImplantsLeftYear, patientFile);
                    OperateIniFile.WriteIniData("Complaints", "BreastImplantsRightYear", person.BreastImplantsRightYear, patientFile);

                    //Anamnesis
                    OperateIniFile.WriteIniData("Menses", "DateLastMenstruation", person.DateLastMenstruation, patientFile);
                    OperateIniFile.WriteIniData("Menses", "menstrual cycle disorder", person.MenstrualCycleDisorder ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Menses", "postmenopause", person.Postmenopause? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Menses", "postmenopause year",person.PostmenopauseYear, patientFile);
                    OperateIniFile.WriteIniData("Menses", "hormonal contraceptives", person.HormonalContraceptives? "1" : "0", patientFile);                

                    OperateIniFile.WriteIniData("Menses", "hormonal contraceptives brand name", person.HormonalContraceptivesBrandName, patientFile);
                    OperateIniFile.WriteIniData("Menses", "hormonal contraceptives period", person.HormonalContraceptivesPeriod, patientFile);

                    OperateIniFile.WriteIniData("Somatic", "adiposity", person.Adiposity? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Somatic", "essential hypertension", person.EssentialHypertension? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Somatic", "diabetes", person.Diabetes? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Somatic", "thyroid gland diseases", person.ThyroidGlandDiseases? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Somatic", "other", person.SomaticOther ? "1" : "0", patientFile);
                

                    OperateIniFile.WriteIniData("Gynecologic", "infertility", person.Infertility? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Gynecologic", "ovary diseases", person.OvaryDiseases ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Gynecologic", "ovary cyst", person.OvaryCyst? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Gynecologic", "ovary cancer",  person.OvaryCancer? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Gynecologic", "ovary endometriosis", person.OvaryEndometriosis? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Gynecologic", "ovary other", person.OvaryOther? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Gynecologic", "uterus diseases", person.UterusDiseases? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Gynecologic", "uterus myoma", person.UterusMyoma? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Gynecologic", "uterus cancer", person.UterusCancer? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Gynecologic", "uterus endometriosis", person.UterusEndometriosis? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Gynecologic", "uterus other",  person.UterusOther? "1" : "0", patientFile);
                                
                    OperateIniFile.WriteIniData("Obstetric", "abortions", person.Abortions ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Obstetric", "births", person.Births? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Obstetric", "abortions number", person.AbortionsNumber, patientFile);
                    OperateIniFile.WriteIniData("Obstetric", "births number", person.BirthsNumber, patientFile);
                
                    OperateIniFile.WriteIniData("Lactation", "lactation till 1 month", person.LactationTill1Month ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Lactation", "lactation till 1 year", person.LactationTill1Year? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Lactation", "lactation over 1 year", person.LactationOver1Year ? "1" : "0", patientFile);


                    OperateIniFile.WriteIniData("Diseases", "trauma", person.Trauma ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Diseases", "mastitis", person.Mastitis? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Diseases", "fibrous- cystic mastopathy", person.FibrousCysticMastopathy ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Diseases", "cysts", person.Cysts? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Diseases", "cancer", person.Cancer ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Diseases", "other", person.DiseasesOther ? "1" : "0", patientFile);
                

                    OperateIniFile.WriteIniData("Palpation", "palpation", person.Palpation? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Palpation", "palpation year",  person.PalationYear , patientFile);
                    OperateIniFile.WriteIniData("Palpation", "diffuse", person.PalpationDiffuse ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Palpation", "focal", person.PalpationFocal ? "1" : "0", patientFile);
                     OperateIniFile.WriteIniData("Palpation", "palpation normal", person.PalpationNormal? "1" : "0", person.IniFilePath);
                    OperateIniFile.WriteIniData("Palpation", "palpation status", person.PalpationStatus ? "1" : "0", person.IniFilePath);              

                    OperateIniFile.WriteIniData("Ultrasound", "ultrasound", person.Ultrasound ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Ultrasound", "ultrasound year", person.UltrasoundYear, patientFile);
                    OperateIniFile.WriteIniData("Ultrasound", "diffuse", person.UltrasoundDiffuse? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Ultrasound", "focal",  person.UltrasoundFocal? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Ultrasound", "ultrasound normal", person.UltrasoundNormal ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Ultrasound", "ultrasound status", person.UltrasoundStatus ? "1" : "0", patientFile);
                

                    OperateIniFile.WriteIniData("Mammography", "mammography", person.Mammography ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Mammography", "mammography year", person.MammographyYear, patientFile);
                    OperateIniFile.WriteIniData("Mammography", "diffuse", person.MammographyDiffuse? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Mammography", "focal", person.MammographyFocal? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Mammography", "mammography normal", person.MammographyNormal? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Mammography", "mammography status", person.MammographyStatus ? "1" : "0", patientFile);

                    OperateIniFile.WriteIniData("Biopsy", "biopsy", person.Biopsy? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "biopsy year", person.BiopsyYear, patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "diffuse", person.BiopsyDiffuse? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "focal", person.BiopsyFocal? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "cancer", person.BiopsyCancer? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "proliferation", person.BiopsyProliferation ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "dysplasia", person.BiopsyDysplasia ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "intraductal papilloma", person.BiopsyIntraductalPapilloma? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "fibroadenoma", person.BiopsyFibroadenoma? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "other", person.BiopsyOther ? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "biopsy normal", person.BiopsyNormal? "1" : "0", patientFile);
                    OperateIniFile.WriteIniData("Biopsy", "biopsy status", person.BiopsyStatus ? "1" : "0", patientFile);
                
                }
                else
                {
                    MessageBox.Show(this, string.Format(App.Current.FindResource("Message_24").ToString(), patientFolder));
                }

                UserList userlistWin = this.Owner as UserList;
                userlistWin.loadArchiveFolder(patientFolder);
                try
                {
                    Clipboard.SetText(code);
                }
                catch (Exception) { }
            }
            catch (Exception ex)
            {
                FileHelper.SetFolderPower(patientFolder, "Everyone", "FullControl");
                FileHelper.SetFolderPower(patientFolder, "Users", "FullControl");
                MessageBox.Show(this, App.Current.FindResource("Message_23").ToString());
                MessageBox.Show(this, string.Format(App.Current.FindResource("Message_25").ToString(), patientFolder, ", Error: " + ex.Message));
            }
            finally
            {
                OperateIniFile.WriteIniData("Data", "Last Code", code, System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
            }
        }

        //private string MatchFolder(string folderName,string code)
        //{            
        //    //遍历指定文件夹下所有文件
        //    DirectoryInfo theFolder = new DirectoryInfo(folderName);
        //    try
        //    {
        //        DirectoryInfo[] folderList = theFolder.GetDirectories();
        //        //遍历文件
        //        foreach (DirectoryInfo NextDir in folderList)
        //        {
        //            if (NextDir.Name.StartsWith(code))
        //            {
        //                return NextDir.FullName;
        //            }
        //        }
        //        return null;
        //    }
        //    catch (Exception) {
        //        return null;
        //    }

            
        //} 

        private string getLastCode()
        {
            string lastCode = OperateIniFile.ReadIniData("Data", "Last Code", "", System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
            string dateStr = DateTime.Now.ToString("yyMMdd");
            int num = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                try
                {
                    num = Convert.ToInt32(lastCode.Substring(lastCode.Length - 2, 2));
                }
                catch (Exception)
                {
                    num = 50;
                }
                num = lastCode.StartsWith(dateStr) ? (num + 1) : 1;
            }
            lastCode=dateStr + App.reportSettingModel.DeviceNo + num.ToString("00");
            return lastCode;
        }

        private void Year_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) <= DateTime.Now.Year)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_59").ToString());
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

        private void Month_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) <= DateTime.Now.Year)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_57").ToString());
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

        private void Day_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex re = new Regex("[0-9]+");
                if (re.IsMatch(e.Text))
                {
                    var textBox = (TextBox)sender;
                    if (!string.IsNullOrEmpty(textBox.Text))
                    {
                        if (Convert.ToInt32(textBox.Text + e.Text) <= DateTime.Now.Year)
                        {
                            e.Handled = false;
                        }
                        else
                        {
                            MessageBox.Show(this, App.Current.FindResource("Message_58").ToString());
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

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            this.txtClientNo.Text = "";
            this.txtLastName.Text = "";
            this.txtFirstName.Text = "";
            this.txtMiddleName.Text = "";
            this.txtBirthDate.Text = "";
            this.txtBirthMonth.Text = "";
            this.txtBirthYear.Text = "";
            this.txtMobile.Text = "";
            this.txtMobile.Text = "";
            this.txtClientNo.Focus();
            if (patientCollection != null)
            {
                patientCollection.Clear();
            }
        }

        
    }

     
}
