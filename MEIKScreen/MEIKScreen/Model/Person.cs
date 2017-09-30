
namespace MEIKScreen.Model
{
    public class Person : ViewModelBase
    {
        private string clientNumber;
        public string ClientNumber
        {
            get { return clientNumber; }
            set { clientNumber = value; OnPropertyChanged("ClientNumber"); }
        }

        private string cid;
        public string Cid
        {
            get { return cid; }
            set { cid = value; }
        }
        private string archiveFolder;
        private string crdFilePath;
        private string iniFilePath;
        /// <summary>
        /// NC: New Client; SR: Screened; DS: Data Sent 
        /// </summary>
        private string status;
        private string statusText;
        private bool poorImages;
        private int id;        
        private string code;
        private string surName="";
        private string givenName;
        private string otherName;

        private string gender;
        private int age;
        private string height;
        private string weight;
        private string address;
        private string address2;
        private string city;
        private string province;
        private string zipCode;
        private string country;
        private string mobile;
        private string email;
        private string birthday;
        private string birthDate;
        private string birthMonth;
        private string birthYear;
        //true: is English, false: isnot English
        private bool reportLanguage=true;
        private bool showTechSignature = true;

        private string regDate;
        private string regMonth;
        private string regYear;
        private string remark;
        private bool free = false;
        //自動分析結果：-1 無結果， 0 正常，1 綠色，2 紅色
        private int result = -1;
        private int leftResult = -1;
        private int rightResult = -1;

        private string techName;
        private string techLicense;
        private string doctorName;
        private string doctorLicense;
        private string screenVenue;

        private string icon = "/Images/id_card.png";
        private string uploaded;
        //private string emailTo;        

        #region Public Members
        public int Id { 
            get { return this.id; }
            set {
                this.id = value;
                OnPropertyChanged("Id");
            }
        }

        public string Icon
        {
            get { return this.icon; }
            set
            {
                this.icon = value;
                OnPropertyChanged("Icon");
            }
        }
       
        public string Code
        {
            get { return this.code; }
            set
            {
                this.code = value;
                OnPropertyChanged("Code");
            }
        }
        public string SurName
        {
            get { return this.surName; }
            set
            {
                this.surName = value;
                OnPropertyChanged("SurName");
            }
        }

        public string GivenName
        {
            get { return this.givenName; }
            set
            {
                this.givenName = value;
                OnPropertyChanged("GivenName");
            }
        }
        public string OtherName
        {
            get { return this.otherName; }
            set
            {
                this.otherName = value;
                OnPropertyChanged("OtherName");
            }
        }

        private string fullName;
        public string FullName
        {
            get { return this.fullName; }
            set
            {
                this.fullName = value;
                OnPropertyChanged("FullName");
            }
        }

        public string Gender
        {
            get { return this.gender; }
            set
            {
                this.gender = value;
                OnPropertyChanged("Gender");
            }
        }
        public int Age
        {
            get { return this.age; }
            set
            {
                this.age = value;
                OnPropertyChanged("Age");
            }
        }
        public string Height
        {
            get { return this.height; }
            set
            {
                this.height = value;
                OnPropertyChanged("Height");
            }
        }
        public string Weight
        {
            get { return this.weight; }
            set
            {
                this.weight = value;
                OnPropertyChanged("Weight");
            }
        }
        public string Mobile
        {
            get { return this.mobile; }
            set
            {
                this.mobile = value;
                OnPropertyChanged("Mobile");
            }
        }
        public string Email
        {
            get { return this.email; }
            set
            {
                this.email = value;
                OnPropertyChanged("Email");
            }
        }
        public string Address
        {
            get { return this.address; }
            set
            {
                this.address = value;
                OnPropertyChanged("Address");
            }
        }

        public string Address2
        {
            get { return this.address2; }
            set
            {
                this.address2 = value;
                OnPropertyChanged("Address2");
            }
        }

        public string City
        {
            get { return this.city; }
            set
            {
                this.city = value;
                OnPropertyChanged("City");
            }
        }

        public string Province
        {
            get { return this.province; }
            set
            {
                this.province = value;
                OnPropertyChanged("Province");
            }
        }

        public string ZipCode
        {
            get { return this.zipCode; }
            set
            {
                this.zipCode = value;
                OnPropertyChanged("ZipCode");
            }
        }

        public string Country
        {
            get { return this.country; }
            set
            {
                this.country = value;
                OnPropertyChanged("Country");
            }
        }

        public string Birthday
        {
            get { return this.birthday; }
            set
            {
                this.birthday = value;
                OnPropertyChanged("Birthday");
            }
        }

        public string BirthDate
        {
            get { return this.birthDate; }
            set
            {
                this.birthDate = value;
                OnPropertyChanged("BirthDate");
            }
        }

        public string BirthMonth
        {
            get { return this.birthMonth; }
            set
            {
                this.birthMonth = value;
                OnPropertyChanged("BirthMonth");
            }
        }

        public string BirthYear
        {
            get { return this.birthYear; }
            set
            {
                this.birthYear = value;
                OnPropertyChanged("BirthYear");
            }
        }
        public string RegDate
        {
            get { return this.regDate; }
            set
            {
                this.regDate = value;
                OnPropertyChanged("RegDate");
            }
        }
        public string RegMonth
        {
            get { return this.regMonth; }
            set
            {
                this.regMonth = value;
                OnPropertyChanged("RegMonth");
            }
        }
        public string RegYear
        {
            get { return this.regYear; }
            set
            {
                this.regYear = value;
                OnPropertyChanged("RegYear");
            }
        }

        public bool ReportLanguage
        {
            get { return this.reportLanguage; }
            set
            {
                this.reportLanguage = value;
                OnPropertyChanged("ReportLanguage");
            }
        }

        public bool ShowTechSignature
        {
            get { return this.showTechSignature; }
            set
            {
                this.showTechSignature = value;
            }
        }

        public string Uploaded
        {
            get { return uploaded; }
            set { uploaded = value; OnPropertyChanged("Uploaded"); }
        }

        public string IniFilePath
        {
            get { return iniFilePath; }
            set { iniFilePath = value; }
        }

        public string Status
        {
            get { return status; }
            set { status = value; OnPropertyChanged("Status"); }
        }

        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; OnPropertyChanged("StatusText"); }
        }

        public bool PoorImages
        {
            get { return poorImages; }
            set { poorImages = value; }
        }

        public bool Free
        {
            get { return free; }
            set { free = value; OnPropertyChanged("Free"); }
        }

        public int Result
        {
            get { return result; }
            set { result = value; }
        }

        public int LeftResult
        {
            get { return leftResult; }
            set { leftResult = value; }
        }
        public int RightResult
        {
            get { return rightResult; }
            set { rightResult = value; }
        }

        public string ArchiveFolder
        {
            get { return archiveFolder; }
            set { archiveFolder = value; }
        }


        public string TechName
        {
            get { return techName; }
            set { techName = value; }
        }

        public string TechLicense
        {
            get { return techLicense; }
            set { techLicense = value; }
        }


        public string DoctorName
        {
            get { return doctorName; }
            set { doctorName = value; }
        }

        public string DoctorLicense
        {
            get { return doctorLicense; }
            set { doctorLicense = value; }
        }


        public string ScreenVenue
        {
            get { return screenVenue; }
            set { screenVenue = value; OnPropertyChanged("ScreenVenue"); }
        }

        public string CrdFilePath
        {
            get { return crdFilePath; }
            set { crdFilePath = value; }
        }

        public string Remark
        {
            get { return this.remark; }
            set
            {
                this.remark = value;
                OnPropertyChanged("Remark");
            }
        }

        private string dateLastMenstruation;
        public string DateLastMenstruation
        {
            get { return dateLastMenstruation; }
            set { dateLastMenstruation = value; OnPropertyChanged("DateLastMenstruation"); }
        }       

        private bool mensesStatus;

        public bool MensesStatus
        {
            get { return mensesStatus; }
            set { mensesStatus = value; OnPropertyChanged("MensesStatus"); }
        }

        private bool menstrualCycleDisorder;

        public bool MenstrualCycleDisorder
        {
            get { return menstrualCycleDisorder; }
            set { menstrualCycleDisorder = value; OnPropertyChanged("MenstrualCycleDisorder"); }
        }
        private bool postmenopause;

        public bool Postmenopause
        {
            get { return postmenopause; }
            set { postmenopause = value; OnPropertyChanged("Postmenopause"); }
        }

        private string postmenopauseYear;
        public string PostmenopauseYear
        {
            get { return postmenopauseYear; }
            set { postmenopauseYear = value; OnPropertyChanged("PostmenopauseYear"); }
        }

        private bool hormonalContraceptives;

        public bool HormonalContraceptives
        {
            get { return hormonalContraceptives; }
            set { hormonalContraceptives = value; OnPropertyChanged("HormonalContraceptives"); }
        }
        private string menstrualCycleDisorderDesc;

        public string MenstrualCycleDisorderDesc
        {
            get { return menstrualCycleDisorderDesc; }
            set { menstrualCycleDisorderDesc = value; OnPropertyChanged("MenstrualCycleDisorderDesc"); }
        }
        private string postmenopauseDesc;

        public string PostmenopauseDesc
        {
            get { return postmenopauseDesc; }
            set { postmenopauseDesc = value; OnPropertyChanged("PostmenopauseDesc"); }
        }
        private string hormonalContraceptivesBrandName;

        public string HormonalContraceptivesBrandName
        {
            get { return hormonalContraceptivesBrandName; }
            set { hormonalContraceptivesBrandName = value; OnPropertyChanged("HormonalContraceptivesBrandName"); }
        }
        private string hormonalContraceptivesPeriod;

        public string HormonalContraceptivesPeriod
        {
            get { return hormonalContraceptivesPeriod; }
            set { hormonalContraceptivesPeriod = value; OnPropertyChanged("HormonalContraceptivesPeriod"); }
        }


        private bool somaticStatus;

        public bool SomaticStatus
        {
            get { return somaticStatus; }
            set { somaticStatus = value; OnPropertyChanged("SomaticStatus"); }
        }

        private bool adiposity;

        public bool Adiposity
        {
            get { return adiposity; }
            set { adiposity = value; OnPropertyChanged("Adiposity"); }
        }
        private bool essentialHypertension;

        public bool EssentialHypertension
        {
            get { return essentialHypertension; }
            set { essentialHypertension = value; OnPropertyChanged("EssentialHypertension"); }
        }
        private bool diabetes;

        public bool Diabetes
        {
            get { return diabetes; }
            set { diabetes = value; OnPropertyChanged("Diabetes"); }
        }
        private bool thyroidGlandDiseases;

        public bool ThyroidGlandDiseases
        {
            get { return thyroidGlandDiseases; }
            set { thyroidGlandDiseases = value; OnPropertyChanged("ThyroidGlandDiseases"); }
        }
        private bool somaticOther;

        public bool SomaticOther
        {
            get { return somaticOther; }
            set { somaticOther = value; OnPropertyChanged("SomaticOther"); }
        }
        private string essentialHypertensionDesc;

        public string EssentialHypertensionDesc
        {
            get { return essentialHypertensionDesc; }
            set { essentialHypertensionDesc = value; OnPropertyChanged("EssentialHypertensionDesc"); }
        }
        private string diabetesDesc;

        public string DiabetesDesc
        {
            get { return diabetesDesc; }
            set { diabetesDesc = value; OnPropertyChanged("DiabetesDesc"); }
        }
        private string thyroidGlandDiseasesDesc;

        public string ThyroidGlandDiseasesDesc
        {
            get { return thyroidGlandDiseasesDesc; }
            set { thyroidGlandDiseasesDesc = value; OnPropertyChanged("ThyroidGlandDiseasesDesc"); }
        }
        private string somaticOtherDesc;

        public string SomaticOtherDesc
        {
            get { return somaticOtherDesc; }
            set { somaticOtherDesc = value; OnPropertyChanged("SomaticOtherDesc"); }
        }



        private bool gynecologicStatus;

        public bool GynecologicStatus
        {
            get { return gynecologicStatus; }
            set { gynecologicStatus = value; OnPropertyChanged("GynecologicStatus"); }
        }
        private bool infertility;

        public bool Infertility
        {
            get { return infertility; }
            set { infertility = value; OnPropertyChanged("Infertility"); }
        }
        private bool ovaryDiseases;

        public bool OvaryDiseases
        {
            get { return ovaryDiseases; }
            set { ovaryDiseases = value; OnPropertyChanged("OvaryDiseases"); }
        }
        private bool ovaryCyst;

        public bool OvaryCyst
        {
            get { return ovaryCyst; }
            set { ovaryCyst = value; OnPropertyChanged("OvaryCyst"); }
        }
        private bool ovaryCancer;

        public bool OvaryCancer
        {
            get { return ovaryCancer; }
            set { ovaryCancer = value; OnPropertyChanged("OvaryCancer"); }
        }
        private bool ovaryEndometriosis;

        public bool OvaryEndometriosis
        {
            get { return ovaryEndometriosis; }
            set { ovaryEndometriosis = value; OnPropertyChanged("OvaryEndometriosis"); }
        }
        private bool ovaryOther;

        public bool OvaryOther
        {
            get { return ovaryOther; }
            set { ovaryOther = value; OnPropertyChanged("OvaryOther"); }
        }
        private bool uterusDiseases;

        public bool UterusDiseases
        {
            get { return uterusDiseases; }
            set { uterusDiseases = value; OnPropertyChanged("UterusDiseases"); }
        }
        private bool uterusMyoma;

        public bool UterusMyoma
        {
            get { return uterusMyoma; }
            set { uterusMyoma = value; OnPropertyChanged("UterusMyoma"); }
        }
        private bool uterusCancer;

        public bool UterusCancer
        {
            get { return uterusCancer; }
            set { uterusCancer = value; OnPropertyChanged("UterusCancer"); }
        }
        private bool uterusEndometriosis;

        public bool UterusEndometriosis
        {
            get { return uterusEndometriosis; }
            set { uterusEndometriosis = value; OnPropertyChanged("UterusEndometriosis"); }
        }
        private bool uterusOther;

        public bool UterusOther
        {
            get { return uterusOther; }
            set { uterusOther = value; OnPropertyChanged("UterusOther"); }
        }
        private bool gynecologicOther;

        public bool GynecologicOther
        {
            get { return gynecologicOther; }
            set { gynecologicOther = value; OnPropertyChanged("GynecologicOther"); }
        }

        private string infertilityDesc;

        public string InfertilityDesc
        {
            get { return infertilityDesc; }
            set { infertilityDesc = value; OnPropertyChanged("InfertilityDesc"); }
        }
        private string ovaryOtherDesc;

        public string OvaryOtherDesc
        {
            get { return ovaryOtherDesc; }
            set { ovaryOtherDesc = value; OnPropertyChanged("OvaryOtherDesc"); }
        }
        private string uterusOtherDesc;

        public string UterusOtherDesc
        {
            get { return uterusOtherDesc; }
            set { uterusOtherDesc = value; OnPropertyChanged("UterusOtherDesc"); }
        }
        private string gynecologicOtherDesc;

        public string GynecologicOtherDesc
        {
            get { return gynecologicOtherDesc; }
            set { gynecologicOtherDesc = value; OnPropertyChanged("GynecologicOtherDesc"); }
        }


        private bool obstetricStatus;

        public bool ObstetricStatus
        {
            get { return obstetricStatus; }
            set { obstetricStatus = value; OnPropertyChanged("ObstetricStatus"); }
        }
        private string obstetricDesc;
        public string ObstetricDesc
        {
            get { return obstetricDesc; }
            set { obstetricDesc = value; OnPropertyChanged("ObstetricDesc"); }
        }

        private bool abortions;

        public bool Abortions
        {
            get { return abortions; }
            set { abortions = value; OnPropertyChanged("Abortions"); }
        }
        private bool births;

        public bool Births
        {
            get { return births; }
            set { births = value; OnPropertyChanged("Births"); }
        }
        private string abortionsNumber;

        public string AbortionsNumber
        {
            get { return abortionsNumber; }
            set { abortionsNumber = value; OnPropertyChanged("AbortionsNumber"); }
        }
        private string birthsNumber;

        public string BirthsNumber
        {
            get { return birthsNumber; }
            set { birthsNumber = value; OnPropertyChanged("BirthsNumber"); }
        }


        private bool lactationTill1Month;

        public bool LactationTill1Month
        {
            get { return this.lactationTill1Month; }
            set { this.lactationTill1Month = value; OnPropertyChanged("LactationTill1Month"); }
        }
        private bool lactationTill1Year;

        public bool LactationTill1Year
        {
            get { return this.lactationTill1Year; }
            set { this.lactationTill1Year = value; OnPropertyChanged("LactationTill1Year"); }
        }
        private bool lactationOver1Year;

        public bool LactationOver1Year
        {
            get { return this.lactationOver1Year; }
            set { this.lactationOver1Year = value; OnPropertyChanged("LactationOver1Year"); }
        }



        private bool diseasesStatus;

        public bool DiseasesStatus
        {
            get { return diseasesStatus; }
            set { diseasesStatus = value; OnPropertyChanged("DiseasesStatus"); }
        }
        private bool trauma;

        public bool Trauma
        {
            get { return trauma; }
            set { trauma = value; OnPropertyChanged("Trauma"); }
        }
        private bool mastitis;

        public bool Mastitis
        {
            get { return mastitis; }
            set { mastitis = value; OnPropertyChanged("Mastitis"); }
        }
        private bool fibrousCysticMastopathy;

        public bool FibrousCysticMastopathy
        {
            get { return fibrousCysticMastopathy; }
            set { fibrousCysticMastopathy = value; OnPropertyChanged("FibrousCysticMastopathy"); }
        }
        private bool cysts;

        public bool Cysts
        {
            get { return cysts; }
            set { cysts = value; OnPropertyChanged("Cysts"); }
        }
        private bool cancer;

        public bool Cancer
        {
            get { return cancer; }
            set { cancer = value; OnPropertyChanged("Cancer"); }
        }
        private bool diseasesOther;

        public bool DiseasesOther
        {
            get { return diseasesOther; }
            set { diseasesOther = value; OnPropertyChanged("DiseasesOther"); }
        }
        private string traumaDesc;

        public string TraumaDesc
        {
            get { return traumaDesc; }
            set { traumaDesc = value; OnPropertyChanged("TraumaDesc"); }
        }
        private string mastitisDesc;

        public string MastitisDesc
        {
            get { return mastitisDesc; }
            set { mastitisDesc = value; OnPropertyChanged("MastitisDesc"); }
        }
        private string fibrousCysticMastopathyDesc;

        public string FibrousCysticMastopathyDesc
        {
            get { return fibrousCysticMastopathyDesc; }
            set { fibrousCysticMastopathyDesc = value; OnPropertyChanged("FibrousCysticMastopathyDesc"); }
        }
        private string cystsDesc;

        public string CystsDesc
        {
            get { return cystsDesc; }
            set { cystsDesc = value; OnPropertyChanged("CystsDesc"); }
        }
        private string cancerDesc;

        public string CancerDesc
        {
            get { return cancerDesc; }
            set { cancerDesc = value; OnPropertyChanged("CancerDesc"); }
        }
        private string diseasesOtherDesc;

        public string DiseasesOtherDesc
        {
            get { return diseasesOtherDesc; }
            set { diseasesOtherDesc = value; OnPropertyChanged("DiseasesOtherDesc"); }
        }

        private bool palpationNormal;

        public bool PalpationNormal
        {
            get { return palpationNormal; }
            set { palpationNormal = value; OnPropertyChanged("PalpationNormal"); }
        }
        private bool palpationStatus;

        public bool PalpationStatus
        {
            get { return palpationStatus; }
            set { palpationStatus = value; OnPropertyChanged("PalpationStatus"); }
        }
        private bool palpationDiffuse;

        public bool PalpationDiffuse
        {
            get { return palpationDiffuse; }
            set { palpationDiffuse = value; OnPropertyChanged("PalpationDiffuse"); }
        }
        private bool palpationFocal;

        public bool PalpationFocal
        {
            get { return palpationFocal; }
            set { palpationFocal = value; OnPropertyChanged("PalpationFocal"); }
        }
        private bool ultrasoundDiffuse;

        private bool ultrasoundStatus;

        public bool UltrasoundStatus
        {
            get { return ultrasoundStatus; }
            set { ultrasoundStatus = value; OnPropertyChanged("UltrasoundStatus"); }
        }
        private bool ultrasoundNormal;
        public bool UltrasoundNormal
        {
            get { return ultrasoundNormal; }
            set { ultrasoundNormal = value; OnPropertyChanged("UltrasoundNormal"); }
        }

        public bool UltrasoundDiffuse
        {
            get { return ultrasoundDiffuse; }
            set { ultrasoundDiffuse = value; OnPropertyChanged("UltrasoundDiffuse"); }
        }
        private bool ultrasoundFocal;

        public bool UltrasoundFocal
        {
            get { return ultrasoundFocal; }
            set { ultrasoundFocal = value; OnPropertyChanged("ultrasoundFocal"); }
        }

        private bool mammographyNormal;
        public bool MammographyNormal
        {
            get { return mammographyNormal; }
            set { mammographyNormal = value; OnPropertyChanged("MammographyNormal"); }
        }
        private bool mammographyStatus;

        public bool MammographyStatus
        {
            get { return mammographyStatus; }
            set { mammographyStatus = value; OnPropertyChanged("MammographyStatus"); }
        }
        private bool mammographyDiffuse;

        public bool MammographyDiffuse
        {
            get { return mammographyDiffuse; }
            set { mammographyDiffuse = value; OnPropertyChanged("MammographyDiffuse"); }
        }
        private bool mammographyFocal;

        public bool MammographyFocal
        {
            get { return mammographyFocal; }
            set { mammographyFocal = value; OnPropertyChanged("MammographyFocal"); }
        }

        private bool biopsyNormal;
        public bool BiopsyNormal
        {
            get { return biopsyNormal; }
            set { biopsyNormal = value; OnPropertyChanged("BiopsyNormal"); }
        }

        private bool biopsyStatus;
        public bool BiopsyStatus
        {
            get { return biopsyStatus; }
            set { biopsyStatus = value; OnPropertyChanged("BiopsyStatus"); }
        }
        private bool biopsyDiffuse;

        public bool BiopsyDiffuse
        {
            get { return biopsyDiffuse; }
            set { biopsyDiffuse = value; OnPropertyChanged("BiopsyDiffuse"); }
        }
        private bool biopsyFocal;

        public bool BiopsyFocal
        {
            get { return biopsyFocal; }
            set { biopsyFocal = value; OnPropertyChanged("BiopsyFocal"); }
        }
        private bool biopsyCancer;
        
        public bool BiopsyCancer
        {
            get { return biopsyCancer; }
            set { biopsyCancer = value; OnPropertyChanged("BiopsyCancer"); }
        }
        private bool biopsyProliferation;

        public bool BiopsyProliferation
        {
            get { return biopsyProliferation; }
            set { biopsyProliferation = value; OnPropertyChanged("BiopsyProliferation"); }
        }
        private bool biopsyDysplasia;

        public bool BiopsyDysplasia
        {
            get { return biopsyDysplasia; }
            set { biopsyDysplasia = value; OnPropertyChanged("BiopsyDysplasia"); }
        }
        private bool biopsyIntraductalPapilloma;

        public bool BiopsyIntraductalPapilloma
        {
            get { return biopsyIntraductalPapilloma; }
            set { biopsyIntraductalPapilloma = value; OnPropertyChanged("BiopsyIntraductalPapilloma"); }
        }
        private bool biopsyFibroadenoma;

        public bool BiopsyFibroadenoma
        {
            get { return biopsyFibroadenoma; }
            set { biopsyFibroadenoma = value; OnPropertyChanged("BiopsyFibroadenoma"); }
        }
        private bool biopsyOther;

        public bool BiopsyOther
        {
            get { return biopsyOther; }
            set { biopsyOther = value; OnPropertyChanged("BiopsyOther"); }
        }
        private string palpationDesc;

        public string PalpationDesc
        {
            get { return palpationDesc; }
            set { palpationDesc = value; }
        }
        private string ultrasoundnDesc;

        public string UltrasoundnDesc
        {
            get { return ultrasoundnDesc; }
            set { ultrasoundnDesc = value; }
        }
        private string mammographyDesc;

        public string MammographyDesc
        {
            get { return mammographyDesc; }
            set { mammographyDesc = value;  }
        }
        private string biopsyOtherDesc;

        public string BiopsyOtherDesc
        {
            get { return biopsyOtherDesc; }
            set { biopsyOtherDesc = value;  }
        }

        private string examinationsOtherDesc;
        public string ExaminationsOtherDesc
        {
            get { return examinationsOtherDesc; }
            set { examinationsOtherDesc = value; OnPropertyChanged("ExaminationsOtherDesc"); }
        }

        

        #endregion


        #region Family

        private bool familyBreastCancer1;
        public bool FamilyBreastCancer1
        {
            get { return familyBreastCancer1; }
            set { familyBreastCancer1 = value; OnPropertyChanged("FamilyBreastCancer1"); }
        }

        private bool familyBreastCancer2;
        public bool FamilyBreastCancer2
        {
            get { return familyBreastCancer2; }
            set { familyBreastCancer2 = value; OnPropertyChanged("FamilyBreastCancer2"); }
        }

        private bool familyBreastCancer3;
        public bool FamilyBreastCancer3
        {
            get { return familyBreastCancer3; }
            set { familyBreastCancer3 = value; OnPropertyChanged("FamilyBreastCancer3"); }
        }
        private string breastCancerDesc;
        public string BreastCancerDesc
        {
            get { return breastCancerDesc; }
            set { breastCancerDesc = value; OnPropertyChanged("BreastCancerDesc"); }
        }

        private bool familyUterineCancer1;
        public bool FamilyUterineCancer1
        {
            get { return familyUterineCancer1; }
            set { familyUterineCancer1 = value; OnPropertyChanged("FamilyUterineCancer1"); }
        }

        private bool familyUterineCancer2;
        public bool FamilyUterineCancer2
        {
            get { return familyUterineCancer2; }
            set { familyUterineCancer2 = value; OnPropertyChanged("FamilyUterineCancer2"); }
        }

        private bool familyUterineCancer3;
        public bool FamilyUterineCancer3
        {
            get { return familyUterineCancer3; }
            set { familyUterineCancer3 = value; OnPropertyChanged("FamilyUterineCancer3"); }
        }
        private string uterineCancerDesc;
        public string UterineCancerDesc
        {
            get { return uterineCancerDesc; }
            set { uterineCancerDesc = value; OnPropertyChanged("UterineCancerDesc"); }
        }

        private bool familyCervicalCancer1;
        public bool FamilyCervicalCancer1
        {
            get { return familyCervicalCancer1; }
            set { familyCervicalCancer1 = value; OnPropertyChanged("FamilyCervicalCancer1"); }
        }

        private bool familyCervicalCancer2;
        public bool FamilyCervicalCancer2
        {
            get { return familyCervicalCancer2; }
            set { familyCervicalCancer2 = value; OnPropertyChanged("FamilyCervicalCancer2"); }
        }

        private bool familyCervicalCancer3;
        public bool FamilyCervicalCancer3
        {
            get { return familyCervicalCancer3; }
            set { familyCervicalCancer3 = value; OnPropertyChanged("FamilyCervicalCancer3"); }
        }
        private string cervicalCancerDesc;
        public string CervicalCancerDesc
        {
            get { return cervicalCancerDesc; }
            set { cervicalCancerDesc = value; OnPropertyChanged("CervicalCancerDesc"); }
        }

        private bool familyOvarianCancer1;
        public bool FamilyOvarianCancer1
        {
            get { return familyOvarianCancer1; }
            set { familyOvarianCancer1 = value; OnPropertyChanged("FamilyOvarianCancer1"); }
        }

        private bool familyOvarianCancer2;
        public bool FamilyOvarianCancer2
        {
            get { return familyOvarianCancer2; }
            set { familyOvarianCancer2 = value; OnPropertyChanged("FamilyOvarianCancer2"); }
        }

        private bool familyOvarianCancer3;
        public bool FamilyOvarianCancer3
        {
            get { return familyOvarianCancer3; }
            set { familyOvarianCancer3 = value; OnPropertyChanged("FamilyOvarianCancer3"); }
        }
        private string ovarianCancerDesc;
        public string OvarianCancerDesc
        {
            get { return ovarianCancerDesc; }
            set { ovarianCancerDesc = value; OnPropertyChanged("OvarianCancerDesc"); }
        }

        private string familyCancerDesc;
        public string FamilyCancerDesc
        {
            get { return familyCancerDesc; }
            set { familyCancerDesc = value; OnPropertyChanged("FamilyCancerDesc"); }
        }

        #endregion


        #region Complaints

        private bool palpableLumps;
        public bool PalpableLumps
        {
            get { return palpableLumps; }
            set { palpableLumps = value; OnPropertyChanged("PalpableLumps"); }
        }

        private int leftPosition=0;
        public int LeftPosition
        {
            get { return leftPosition; }
            set { leftPosition = value; OnPropertyChanged("LeftPosition"); }
        }

        private int rightPosition=0;
        public int RightPosition
        {
            get { return rightPosition; }
            set { rightPosition = value; OnPropertyChanged("RightPosition"); }
        }

        private int degree = 0;
        public int Degree
        {
            get { return degree; }
            set { degree = value; OnPropertyChanged("Degree"); }
        }


        private bool pain;
        public bool Pain
        {
            get { return pain; }
            set { pain = value; OnPropertyChanged("Pain"); }
        }
        private bool colostrum;

        public bool Colostrum
        {
            get { return colostrum; }
            set { colostrum = value; OnPropertyChanged("Colostrum"); }
        }
        private bool serousDischarge;

        public bool SerousDischarge
        {
            get { return serousDischarge; }
            set { serousDischarge = value; OnPropertyChanged("SerousDischarge"); }
        }
        private bool bloodDischarge;

        public bool BloodDischarge
        {
            get { return bloodDischarge; }
            set { bloodDischarge = value; OnPropertyChanged("BloodDischarge"); }
        }
        private bool other;

        public bool Other
        {
            get { return other; }
            set { other = value; OnPropertyChanged("Other"); }
        }

        private bool pregnancy;

        public bool Pregnancy
        {
            get { return pregnancy; }
            set { pregnancy = value; OnPropertyChanged("Pregnancy"); }
        }
        private bool lactation;

        public bool Lactation
        {
            get { return lactation; }
            set { lactation = value; OnPropertyChanged("Lactation"); }
        }
        private string lactationTerm;

        public string LactationTerm
        {
            get { return lactationTerm; }
            set { lactationTerm = value; OnPropertyChanged("LactationTerm"); }
        }
        
        private string pregnancyTerm;

        public string PregnancyTerm
        {
            get { return pregnancyTerm; }
            set { pregnancyTerm = value; OnPropertyChanged("PregnancyTerm"); }
        }

        private string otherDesc;

        public string OtherDesc
        {
            get { return otherDesc; }
            set { otherDesc = value; OnPropertyChanged("OtherDesc"); }
        }

        private bool breastImplants;
        public bool BreastImplants
        {
            get { return breastImplants; }
            set { breastImplants = value; OnPropertyChanged("BreastImplants"); }
        }

        private bool breastImplantsRight;
        public bool BreastImplantsRight
        {
            get { return breastImplantsRight; }
            set { breastImplantsRight = value; OnPropertyChanged("BreastImplantsRight"); }
        }

        private bool breastImplantsLeft;
        public bool BreastImplantsLeft
        {
            get { return breastImplantsLeft; }
            set { breastImplantsLeft = value; OnPropertyChanged("BreastImplantsLeft"); }
        }

        private string breastImplantsRightYear;
        public string BreastImplantsRightYear
        {
            get { return breastImplantsRightYear; }
            set { breastImplantsRightYear = value; OnPropertyChanged("BreastImplantsRightYear"); }
        }

        private string breastImplantsLeftYear;
        public string BreastImplantsLeftYear
        {
            get { return breastImplantsLeftYear; }
            set { breastImplantsLeftYear = value; OnPropertyChanged("BreastImplantsLeftYear"); }
        }

        private bool materialsGel;
        public bool MaterialsGel
        {
            get { return materialsGel; }
            set { materialsGel = value; OnPropertyChanged("MaterialsGel"); }
        }

        private bool materialsFat;
        public bool MaterialsFat
        {
            get { return materialsFat; }
            set { materialsFat = value; OnPropertyChanged("MaterialsFat"); }
        }

        private bool materialsOthers;
        public bool MaterialsOthers
        {
            get { return materialsOthers; }
            set { materialsOthers = value; OnPropertyChanged("MaterialsOthers"); }
        }

        #endregion

        #region Examination

        private bool palpation;
        public bool Palpation
        {
            get { return palpation; }
            set { palpation = value; OnPropertyChanged("Palpation"); }
        }       

        private bool ultrasound;
        public bool Ultrasound
        {
            get { return ultrasound; }
            set { ultrasound = value; OnPropertyChanged("Ultrasound"); }
        }

        private bool mammography;
        public bool Mammography
        {
            get { return mammography; }
            set { mammography = value; OnPropertyChanged("Mammography"); }
        }

        private bool biopsy;
        public bool Biopsy
        {
            get { return biopsy; }
            set { biopsy = value; OnPropertyChanged("Biopsy"); }
        }


        private string palationYear;
        public string PalationYear
        {
            get { return palationYear; }
            set { palationYear = value; OnPropertyChanged("PalationYear"); }
        }

        private string ultrasoundYear;
        public string UltrasoundYear
        {
            get { return ultrasoundYear; }
            set { ultrasoundYear = value; OnPropertyChanged("UltrasoundYear"); }
        }

        private string mammographyYear;
        public string MammographyYear
        {
            get { return mammographyYear; }
            set { mammographyYear = value; OnPropertyChanged("MammographyYear"); }
        }

        private string biopsyYear;
        public string BiopsyYear
        {
            get { return biopsyYear; }
            set { biopsyYear = value; OnPropertyChanged("BiopsyYear"); }
        }

        private bool meikScreening;
        public bool MeikScreening
        {
            get { return meikScreening; }
            set { meikScreening = value; OnPropertyChanged("MeikScreening"); }
        }
        private string meikScreenYear;
        public string MeikScreenYear
        {
            get { return meikScreenYear; }
            set { meikScreenYear = value; OnPropertyChanged("MeikScreenYear"); }
        }
        private string meikPoint;
        public string MeikPoint
        {
            get { return meikPoint; }
            set { meikPoint = value; OnPropertyChanged("MeikPoint"); }
        }

        private string meikScreenDesc;
        public string MeikScreenDesc
        {
            get { return meikScreenDesc; }
            set { meikScreenDesc = value; OnPropertyChanged("MeikScreenDesc"); }
        }



        private bool redSwollen;
        public bool RedSwollen
        {
            get { return redSwollen; }
            set { redSwollen = value; OnPropertyChanged("RedSwollen"); }
        }
        private string redSwollenDesc;
        public string RedSwollenDesc
        {
            get { return redSwollenDesc; }
            set { redSwollenDesc = value; OnPropertyChanged("RedSwollenDesc"); }
        }

        private bool palpable;
        public bool Palpable
        {
            get { return palpable; }
            set { palpable = value; OnPropertyChanged("Palpable"); }
        }
        private string palpableDesc;
        public string PalpableDesc
        {
            get { return palpableDesc; }
            set { palpableDesc = value; OnPropertyChanged("PalpableDesc"); }
        }

        private bool serous;
        public bool Serous
        {
            get { return serous; }
            set { serous = value; OnPropertyChanged("Serous"); }
        }
        private string seriousDesc;
        public string SeriousDesc
        {
            get { return seriousDesc; }
            set { seriousDesc = value; OnPropertyChanged("SeriousDesc"); }
        }

        private bool wounds;
        public bool Wounds
        {
            get { return wounds; }
            set { wounds = value; OnPropertyChanged("Wounds"); }
        }
        private string woundsDesc;
        public string WoundsDesc
        {
            get { return woundsDesc; }
            set { woundsDesc = value; OnPropertyChanged("WoundsDesc"); }
        }

        private bool scars;
        public bool Scars
        {
            get { return scars; }
            set { scars = value; OnPropertyChanged("Scars"); }
        }
        private string scarsDesc;
        public string ScarsDesc
        {
            get { return scarsDesc; }
            set { scarsDesc = value; OnPropertyChanged("ScarsDesc"); }
        }

        private int redSwollenLeft;
        public int RedSwollenLeft
        {
            get { return redSwollenLeft; }
            set { redSwollenLeft = value; OnPropertyChanged("RedSwollenLeft"); }
        }

        private int redSwollenRight;
        public int RedSwollenRight
        {
            get { return redSwollenRight; }
            set { redSwollenRight = value; OnPropertyChanged("RedSwollenRight"); }
        }

        private int palpableLeft;
        public int PalpableLeft
        {
            get { return palpableLeft; }
            set { palpableLeft = value; OnPropertyChanged("PalpableLeft"); }
        }

        private int palpableRight;
        public int PalpableRight
        {
            get { return palpableRight; }
            set { palpableRight = value; OnPropertyChanged("PalpableRight"); }
        }

        private int serousLeft;
        public int SerousLeft
        {
            get { return serousLeft; }
            set { serousLeft = value; OnPropertyChanged("SerousLeft"); }
        }

        private int serousRight;
        public int SerousRight
        {
            get { return serousRight; }
            set { serousRight = value; OnPropertyChanged("SerousRight"); }
        }        

        private int woundsLeft;
        public int WoundsLeft
        {
            get { return woundsLeft; }
            set { woundsLeft = value; OnPropertyChanged("WoundsLeft"); }
        }

        private int woundsRight;
        public int WoundsRight
        {
            get { return woundsRight; }
            set { woundsRight = value; OnPropertyChanged("WoundsRight"); }
        }

        private int scarsLeft;
        public int ScarsLeft
        {
            get { return scarsLeft; }
            set { scarsLeft = value; OnPropertyChanged("ScarsLeft"); }
        }

        private int scarsRight;
        public int ScarsRight
        {
            get { return scarsRight; }
            set { scarsRight = value; OnPropertyChanged("ScarsRight"); }
        }
        #endregion
    }
}
