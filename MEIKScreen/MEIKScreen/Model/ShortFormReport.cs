using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEIKScreen.Model
{
    [Serializable]
    public class ShortFormReport:ICloneable 
    {
        private string dataClientNum;

        public string DataClientNum
        {
            get { return dataClientNum; }
            set { dataClientNum = value; }
        }

        private string dataScreenDate;

        public string DataScreenDate
        {
            get { return dataScreenDate; }
            set { dataScreenDate = value; }
        }
        private string dataScreenLocation;

        public string DataScreenLocation
        {
            get { return dataScreenLocation; }
            set { dataScreenLocation = value; }
        }
        private string dataUserCode;

        public string DataUserCode
        {
            get { return dataUserCode; }
            set { dataUserCode = value; }
        }
        private string dataName;

        public string DataName
        {
            get { return dataName; }
            set { dataName = value; }
        }
        private string dataAge;

        public string DataAge
        {
            get { return dataAge; }
            set { dataAge = value; }
        }

        private string dataHeight;

        public string DataHeight
        {
            get { return dataHeight; }
            set { dataHeight = value; }
        }

        private string dataMobile;

        public string DataMobile
        {
            get { return dataMobile; }
            set { dataMobile = value; }
        }

        private string dataEmail;

        public string DataEmail
        {
            get { return dataEmail; }
            set { dataEmail = value; }
        }


        private string dataLeftFinding;

        public string DataLeftFinding
        {
            get { return dataLeftFinding; }
            set { dataLeftFinding = value; }
        }
        private string dataRightFinding;

        public string DataRightFinding
        {
            get { return dataRightFinding; }
            set { dataRightFinding = value; }
        }

        private string dataLeftSegment;
        public string DataLeftSegment
        {
            get { return dataLeftSegment; }
            set { dataLeftSegment = value; }
        }
        private string dataRightSegment;
        public string DataRightSegment
        {
            get { return dataRightSegment; }
            set { dataRightSegment = value; }
        }

        private string dataLeftLocation;
        public string DataLeftLocation
        {
            get { return dataLeftLocation; }
            set { dataLeftLocation = value; }
        }
        private string dataRightLocation;
        public string DataRightLocation
        {
            get { return dataRightLocation; }
            set { dataRightLocation = value; }
        }
        private string dataLeftLocation2;

        public string DataLeftLocation2
        {
            get { return dataLeftLocation2; }
            set { dataLeftLocation2 = value; }
        }
        private string dataRightLocation2;

        public string DataRightLocation2
        {
            get { return dataRightLocation2; }
            set { dataRightLocation2 = value; }
        }
        private string dataLeftLocation3;

        public string DataLeftLocation3
        {
            get { return dataLeftLocation3; }
            set { dataLeftLocation3 = value; }
        }
        private string dataRightLocation3;

        public string DataRightLocation3
        {
            get { return dataRightLocation3; }
            set { dataRightLocation3 = value; }
        }

        private string dataLeftSize;
        public string DataLeftSize
        {
            get { return dataLeftSize; }
            set { dataLeftSize = value; }
        }
        private string dataRightSize;
        public string DataRightSize
        {
            get { return dataRightSize; }
            set { dataRightSize = value; }
        }        

        private string dataLeftSize2;
        public string DataLeftSize2
        {
            get { return dataLeftSize2; }
            set { dataLeftSize2 = value; }
        }
        private string dataRightSize2;
        public string DataRightSize2
        {
            get { return dataRightSize2; }
            set { dataRightSize2 = value; }
        }        

        private string dataLeftSize3;
        public string DataLeftSize3
        {
            get { return dataLeftSize3; }
            set { dataLeftSize3 = value; }
        }
        private string dataRightSize3;
        public string DataRightSize3
        {
            get { return dataRightSize3; }
            set { dataRightSize3 = value; }
        }        
        
        private string dataRecommendation;

        public string DataRecommendation
        {
            get { return dataRecommendation; }
            set { dataRecommendation = value; }
        }
        private string dataFurtherExam;

        public string DataFurtherExam
        {
            get { return dataFurtherExam; }
            set { dataFurtherExam = value; }
        }
        private string dataConclusion;

        public string DataConclusion
        {
            get { return dataConclusion; }
            set { dataConclusion = value; }
        }
        private string dataConclusion2;

        public string DataConclusion2
        {
            get { return dataConclusion2; }
            set { dataConclusion2 = value; }
        }
        private string dataComments;

        public string DataComments
        {
            get { return dataComments; }
            set { dataComments = value; }
        }
        private string dataSignDate;

        public string DataSignDate
        {
            get { return dataSignDate; }
            set { dataSignDate = value; }
        }

        private byte[] dataSignImg;

        public byte[] DataSignImg
        {
            get { return dataSignImg; }
            set { dataSignImg = value; }
        }
        private string dataMeikTech;

        public string DataMeikTech
        {
            get { return dataMeikTech; }
            set { dataMeikTech = value; }
        }
        private string dataTechLicense;

        public string DataTechLicense
        {
            get { return dataTechLicense; }
            set { dataTechLicense = value; }
        }
        private string dataDoctor;

        public string DataDoctor
        {
            get { return dataDoctor; }
            set { dataDoctor = value; }
        }
        private string dataDoctorLicense;

        public string DataDoctorLicense
        {
            get { return dataDoctorLicense; }
            set { dataDoctorLicense = value; }
        }
        
        private byte[] dataScreenShotImg;

        public byte[] DataScreenShotImg
        {
            get { return dataScreenShotImg; }
            set { dataScreenShotImg = value; }
        }        

        private string dataAddress;

        public string DataAddress
        {
            get { return dataAddress; }
            set { dataAddress = value; }
        }

        private string dataHealthCard;

        public string DataHealthCard
        {
            get { return dataHealthCard; }
            set { dataHealthCard = value; }
        }

        private string dataWeight;

        public string DataWeight
        {
            get { return dataWeight; }
            set { dataWeight = value; }
        }

        private string dataWeightUnit;

        public string DataWeightUnit
        {
            get { return dataWeightUnit; }
            set { dataWeightUnit = value; }
        }

        private string dataMenstrualCycle;

        public string DataMenstrualCycle
        {
            get { return dataMenstrualCycle; }
            set { dataMenstrualCycle = value; }
        }

        private string dataHormones;

        public string DataHormones
        {
            get { return dataHormones; }
            set { dataHormones = value; }
        }

        private string dataSkinAffections;

        public string DataSkinAffections
        {
            get { return dataSkinAffections; }
            set { dataSkinAffections = value; }
        }
        

        private string dataMotherUltra;

        public string DataMotherUltra
        {
            get { return dataMotherUltra; }
            set { dataMotherUltra = value; }
        }

        private string dataLeftBreast;
        public string DataLeftBreast
        {
            get { return dataLeftBreast; }
            set { dataLeftBreast = value; }
        }
        private string dataLeftBreastH;
        public string DataLeftBreastH
        {
            get { return dataLeftBreastH; }
            set { dataLeftBreastH = value; }
        }
        private string dataLeftBreastM;
        public string DataLeftBreastM
        {
            get { return dataLeftBreastM; }
            set { dataLeftBreastM = value; }
        }
        private string dataLeftBreastAP;
        public string DataLeftBreastAP
        {
            get { return dataLeftBreastAP; }
            set { dataLeftBreastAP = value; }
        }

        private string dataRightBreast;
        public string DataRightBreast
        {
            get { return dataRightBreast; }
            set { dataRightBreast = value; }
        }
        private string dataRightBreastH;
        public string DataRightBreastH
        {
            get { return dataRightBreastH; }
            set { dataRightBreastH = value; }
        }
        private string dataRightBreastM;
        public string DataRightBreastM
        {
            get { return dataRightBreastM; }
            set { dataRightBreastM = value; }
        }
        private string dataRightBreastAP;
        public string DataRightBreastAP
        {
            get { return dataRightBreastAP; }
            set { dataRightBreastAP = value; }
        }

        private string dataLeftPalpableMass;

        public string DataLeftPalpableMass
        {
            get { return dataLeftPalpableMass; }
            set { dataLeftPalpableMass = value; }
        }

        private string dataRightPalpableMass;

        public string DataRightPalpableMass
        {
            get { return dataRightPalpableMass; }
            set { dataRightPalpableMass = value; }
        }

        private string dataLeftChangesOfElectricalConductivity;

        public string DataLeftChangesOfElectricalConductivity
        {
            get { return dataLeftChangesOfElectricalConductivity; }
            set { dataLeftChangesOfElectricalConductivity = value; }
        }

        private string dataRightChangesOfElectricalConductivity;

        public string DataRightChangesOfElectricalConductivity
        {
            get { return dataRightChangesOfElectricalConductivity; }
            set { dataRightChangesOfElectricalConductivity = value; }
        }

        private string dataLeftMammaryStruct;

        public string DataLeftMammaryStruct
        {
            get { return dataLeftMammaryStruct; }
            set { dataLeftMammaryStruct = value; }
        }

        private string dataRightMammaryStruct;

        public string DataRightMammaryStruct
        {
            get { return dataRightMammaryStruct; }
            set { dataRightMammaryStruct = value; }
        }

        private string dataLeftLactiferousSinusZone;

        public string DataLeftLactiferousSinusZone
        {
            get { return dataLeftLactiferousSinusZone; }
            set { dataLeftLactiferousSinusZone = value; }
        }

        private string dataRightLactiferousSinusZone;

        public string DataRightLactiferousSinusZone
        {
            get { return dataRightLactiferousSinusZone; }
            set { dataRightLactiferousSinusZone = value; }
        }

        private string dataLeftMammaryContour;

        public string DataLeftMammaryContour
        {
            get { return dataLeftMammaryContour; }
            set { dataLeftMammaryContour = value; }
        }

        private string dataRightMammaryContour;

        public string DataRightMammaryContour
        {
            get { return dataRightMammaryContour; }
            set { dataRightMammaryContour = value; }
        }

        private string dataLeftNumber;

        public string DataLeftNumber
        {
            get { return dataLeftNumber; }
            set { dataLeftNumber = value; }
        }

        private string dataRightNumber;

        public string DataRightNumber
        {
            get { return dataRightNumber; }
            set { dataRightNumber = value; }
        }

        private string dataLeftShape;

        public string DataLeftShape
        {
            get { return dataLeftShape; }
            set { dataLeftShape = value; }
        }

        private string dataRightShape;

        public string DataRightShape
        {
            get { return dataRightShape; }
            set { dataRightShape = value; }
        }

        private string dataLeftShape2;

        public string DataLeftShape2
        {
            get { return dataLeftShape2; }
            set { dataLeftShape2 = value; }
        }

        private string dataRightShape2;

        public string DataRightShape2
        {
            get { return dataRightShape2; }
            set { dataRightShape2 = value; }
        }
        private string dataLeftShape3;

        public string DataLeftShape3
        {
            get { return dataLeftShape3; }
            set { dataLeftShape3 = value; }
        }

        private string dataRightShape3;

        public string DataRightShape3
        {
            get { return dataRightShape3; }
            set { dataRightShape3 = value; }
        }

        private string dataLeftContourAroundFocus;

        public string DataLeftContourAroundFocus
        {
            get { return dataLeftContourAroundFocus; }
            set { dataLeftContourAroundFocus = value; }
        }

        private string dataRightContourAroundFocus;

        public string DataRightContourAroundFocus
        {
            get { return dataRightContourAroundFocus; }
            set { dataRightContourAroundFocus = value; }
        }


        private string dataLeftContourAroundFocus2;

        public string DataLeftContourAroundFocus2
        {
            get { return dataLeftContourAroundFocus2; }
            set { dataLeftContourAroundFocus2 = value; }
        }

        private string dataRightContourAroundFocus2;

        public string DataRightContourAroundFocus2
        {
            get { return dataRightContourAroundFocus2; }
            set { dataRightContourAroundFocus2 = value; }
        }
        private string dataLeftContourAroundFocus3;

        public string DataLeftContourAroundFocus3
        {
            get { return dataLeftContourAroundFocus3; }
            set { dataLeftContourAroundFocus3 = value; }
        }

        private string dataRightContourAroundFocus3;

        public string DataRightContourAroundFocus3
        {
            get { return dataRightContourAroundFocus3; }
            set { dataRightContourAroundFocus3 = value; }
        }


        private string dataLeftInternalElectricalStructure;

        public string DataLeftInternalElectricalStructure
        {
            get { return dataLeftInternalElectricalStructure; }
            set { dataLeftInternalElectricalStructure = value; }
        }

        private string dataRightInternalElectricalStructure;

        public string DataRightInternalElectricalStructure
        {
            get { return dataRightInternalElectricalStructure; }
            set { dataRightInternalElectricalStructure = value; }
        }

        private string dataLeftInternalElectricalStructure2;

        public string DataLeftInternalElectricalStructure2
        {
            get { return dataLeftInternalElectricalStructure2; }
            set { dataLeftInternalElectricalStructure2 = value; }
        }

        private string dataRightInternalElectricalStructure2;

        public string DataRightInternalElectricalStructure2
        {
            get { return dataRightInternalElectricalStructure2; }
            set { dataRightInternalElectricalStructure2 = value; }
        }

        private string dataLeftInternalElectricalStructure3;

        public string DataLeftInternalElectricalStructure3
        {
            get { return dataLeftInternalElectricalStructure3; }
            set { dataLeftInternalElectricalStructure3 = value; }
        }

        private string dataRightInternalElectricalStructure3;

        public string DataRightInternalElectricalStructure3
        {
            get { return dataRightInternalElectricalStructure3; }
            set { dataRightInternalElectricalStructure3 = value; }
        }

        private string dataLeftSurroundingTissues;

        public string DataLeftSurroundingTissues
        {
            get { return dataLeftSurroundingTissues; }
            set { dataLeftSurroundingTissues = value; }
        }

        private string dataRightSurroundingTissues;

        public string DataRightSurroundingTissues
        {
            get { return dataRightSurroundingTissues; }
            set { dataRightSurroundingTissues = value; }
        }

        private string dataLeftSurroundingTissues2;

        public string DataLeftSurroundingTissues2
        {
            get { return dataLeftSurroundingTissues2; }
            set { dataLeftSurroundingTissues2 = value; }
        }

        private string dataRightSurroundingTissues2;

        public string DataRightSurroundingTissues2
        {
            get { return dataRightSurroundingTissues2; }
            set { dataRightSurroundingTissues2 = value; }
        }
        private string dataLeftSurroundingTissues3;

        public string DataLeftSurroundingTissues3
        {
            get { return dataLeftSurroundingTissues3; }
            set { dataLeftSurroundingTissues3 = value; }
        }

        private string dataRightSurroundingTissues3;

        public string DataRightSurroundingTissues3
        {
            get { return dataRightSurroundingTissues3; }
            set { dataRightSurroundingTissues3 = value; }
        }

        private string dataLeftOncomarkerHighlightBenignChanges;

        public string DataLeftOncomarkerHighlightBenignChanges
        {
            get { return dataLeftOncomarkerHighlightBenignChanges; }
            set { dataLeftOncomarkerHighlightBenignChanges = value; }
        }

        private string dataRightOncomarkerHighlightBenignChanges;

        public string DataRightOncomarkerHighlightBenignChanges
        {
            get { return dataRightOncomarkerHighlightBenignChanges; }
            set { dataRightOncomarkerHighlightBenignChanges = value; }
        }

        private string dataLeftOncomarkerHighlightSuspiciousChanges;

        public string DataLeftOncomarkerHighlightSuspiciousChanges
        {
            get { return dataLeftOncomarkerHighlightSuspiciousChanges; }
            set { dataLeftOncomarkerHighlightSuspiciousChanges = value; }
        }

        private string dataRightOncomarkerHighlightSuspiciousChanges;

        public string DataRightOncomarkerHighlightSuspiciousChanges
        {
            get { return dataRightOncomarkerHighlightSuspiciousChanges; }
            set { dataRightOncomarkerHighlightSuspiciousChanges = value; }
        }

        private string dataLeftMeanElectricalConductivity1;

        public string DataLeftMeanElectricalConductivity1
        {
            get { return dataLeftMeanElectricalConductivity1; }
            set { dataLeftMeanElectricalConductivity1 = value; }
        }

        private string dataLeftMeanElectricalConductivity1N1;

        public string DataLeftMeanElectricalConductivity1N1
        {
            get { return dataLeftMeanElectricalConductivity1N1; }
            set { dataLeftMeanElectricalConductivity1N1 = value; }
        }

        private string dataLeftMeanElectricalConductivity1N2;

        public string DataLeftMeanElectricalConductivity1N2
        {
            get { return dataLeftMeanElectricalConductivity1N2; }
            set { dataLeftMeanElectricalConductivity1N2 = value; }
        }

        private string dataRightMeanElectricalConductivity1;

        public string DataRightMeanElectricalConductivity1
        {
            get { return dataRightMeanElectricalConductivity1; }
            set { dataRightMeanElectricalConductivity1 = value; }
        }        

        private string dataLeftMeanElectricalConductivity2;

        public string DataLeftMeanElectricalConductivity2
        {
            get { return dataLeftMeanElectricalConductivity2; }
            set { dataLeftMeanElectricalConductivity2 = value; }
        }        

        private string dataRightMeanElectricalConductivity2;

        public string DataRightMeanElectricalConductivity2
        {
            get { return dataRightMeanElectricalConductivity2; }
            set { dataRightMeanElectricalConductivity2 = value; }
        }        

        private string dataMeanElectricalConductivity3;

        public string DataMeanElectricalConductivity3
        {
            get { return dataMeanElectricalConductivity3; }
            set { dataMeanElectricalConductivity3 = value; }
        }

        private string dataLeftMeanElectricalConductivity3;

        public string DataLeftMeanElectricalConductivity3
        {
            get { return dataLeftMeanElectricalConductivity3; }
            set { dataLeftMeanElectricalConductivity3 = value; }
        }        

        private string dataRightMeanElectricalConductivity3;

        public string DataRightMeanElectricalConductivity3
        {
            get { return dataRightMeanElectricalConductivity3; }
            set { dataRightMeanElectricalConductivity3 = value; }
        }                

        private string dataLeftComparativeElectricalConductivity1;

        public string DataLeftComparativeElectricalConductivity1
        {
            get { return dataLeftComparativeElectricalConductivity1; }
            set { dataLeftComparativeElectricalConductivity1 = value; }
        }

        private string dataRightComparativeElectricalConductivity1;

        public string DataRightComparativeElectricalConductivity1
        {
            get { return dataRightComparativeElectricalConductivity1; }
            set { dataRightComparativeElectricalConductivity1 = value; }
        }        

        private string dataLeftComparativeElectricalConductivity2;

        public string DataLeftComparativeElectricalConductivity2
        {
            get { return dataLeftComparativeElectricalConductivity2; }
            set { dataLeftComparativeElectricalConductivity2 = value; }
        }

        private string dataRightComparativeElectricalConductivity2;

        public string DataRightComparativeElectricalConductivity2
        {
            get { return dataRightComparativeElectricalConductivity2; }
            set { dataRightComparativeElectricalConductivity2 = value; }
        }

        private string dataComparativeElectricalConductivity3;

        public string DataComparativeElectricalConductivity3
        {
            get { return dataComparativeElectricalConductivity3; }
            set { dataComparativeElectricalConductivity3 = value; }
        }

        private string dataLeftComparativeElectricalConductivity3;

        public string DataLeftComparativeElectricalConductivity3
        {
            get { return dataLeftComparativeElectricalConductivity3; }
            set { dataLeftComparativeElectricalConductivity3 = value; }
        }

        private string dataRightComparativeElectricalConductivity3;

        public string DataRightComparativeElectricalConductivity3
        {
            get { return dataRightComparativeElectricalConductivity3; }
            set { dataRightComparativeElectricalConductivity3 = value; }
        }

        private string dataDivergenceBetweenHistogramsReference1;

        public string DataDivergenceBetweenHistogramsReference1
        {
            get { return dataDivergenceBetweenHistogramsReference1; }
            set { dataDivergenceBetweenHistogramsReference1 = value; }
        }

        private string dataLeftDivergenceBetweenHistograms1;

        public string DataLeftDivergenceBetweenHistograms1
        {
            get { return dataLeftDivergenceBetweenHistograms1; }
            set { dataLeftDivergenceBetweenHistograms1 = value; }
        }

        private string dataRightDivergenceBetweenHistograms1;

        public string DataRightDivergenceBetweenHistograms1
        {
            get { return dataRightDivergenceBetweenHistograms1; }
            set { dataRightDivergenceBetweenHistograms1 = value; }
        }

        private string dataDivergenceBetweenHistogramsReference2;

        public string DataDivergenceBetweenHistogramsReference2
        {
            get { return dataDivergenceBetweenHistogramsReference2; }
            set { dataDivergenceBetweenHistogramsReference2 = value; }
        }

        private string dataLeftDivergenceBetweenHistograms2;

        public string DataLeftDivergenceBetweenHistograms2
        {
            get { return dataLeftDivergenceBetweenHistograms2; }
            set { dataLeftDivergenceBetweenHistograms2 = value; }
        }

        private string dataRightDivergenceBetweenHistograms2;

        public string DataRightDivergenceBetweenHistograms2
        {
            get { return dataRightDivergenceBetweenHistograms2; }
            set { dataRightDivergenceBetweenHistograms2 = value; }
        }

        private string dataDivergenceBetweenHistograms3;

        public string DataDivergenceBetweenHistograms3
        {
            get { return dataDivergenceBetweenHistograms3; }
            set { dataDivergenceBetweenHistograms3 = value; }
        }

        private string dataLeftDivergenceBetweenHistograms3;

        public string DataLeftDivergenceBetweenHistograms3
        {
            get { return dataLeftDivergenceBetweenHistograms3; }
            set { dataLeftDivergenceBetweenHistograms3 = value; }
        }

        private string dataRightDivergenceBetweenHistograms3;

        public string DataRightDivergenceBetweenHistograms3
        {
            get { return dataRightDivergenceBetweenHistograms3; }
            set { dataRightDivergenceBetweenHistograms3 = value; }
        }

        private string dataLeftComparisonWithNorm;

        public string DataLeftComparisonWithNorm
        {
            get { return dataLeftComparisonWithNorm; }
            set { dataLeftComparisonWithNorm = value; }
        }

        private string dataRightComparisonWithNorm;

        public string DataRightComparisonWithNorm
        {
            get { return dataRightComparisonWithNorm; }
            set { dataRightComparisonWithNorm = value; }
        }

        private string dataPhaseElectricalConductivityReference;

        public string DataPhaseElectricalConductivityReference
        {
            get { return dataPhaseElectricalConductivityReference; }
            set { dataPhaseElectricalConductivityReference = value; }
        }

        private string dataLeftPhaseElectricalConductivity;

        public string DataLeftPhaseElectricalConductivity
        {
            get { return dataLeftPhaseElectricalConductivity; }
            set { dataLeftPhaseElectricalConductivity = value; }
        }

        private string dataRightPhaseElectricalConductivity;

        public string DataRightPhaseElectricalConductivity
        {
            get { return dataRightPhaseElectricalConductivity; }
            set { dataRightPhaseElectricalConductivity = value; }
        }

        private string dataAgeElectricalConductivityReference;

        public string DataAgeElectricalConductivityReference
        {
            get { return dataAgeElectricalConductivityReference; }
            set { dataAgeElectricalConductivityReference = value; }
        }

        private string dataLeftAgeElectricalConductivity;

        public string DataLeftAgeElectricalConductivity
        {
            get { return dataLeftAgeElectricalConductivity; }
            set { dataLeftAgeElectricalConductivity = value; }
        }

        private string dataRightAgeElectricalConductivity;

        public string DataRightAgeElectricalConductivity
        {
            get { return dataRightAgeElectricalConductivity; }
            set { dataRightAgeElectricalConductivity = value; }
        }

        private string dataAgeValueOfEC;

        public string DataAgeValueOfEC
        {
            get { return dataAgeValueOfEC; }
            set { dataAgeValueOfEC = value; }
        }

        private string dataExamConclusion;

        public string DataExamConclusion
        {
            get { return dataExamConclusion; }
            set { dataExamConclusion = value; }
        }

        private string dataLeftMammaryGland;

        public string DataLeftMammaryGland
        {
            get { return dataLeftMammaryGland; }
            set { dataLeftMammaryGland = value; }
        }

        private string dataLeftAgeRelated;

        public string DataLeftAgeRelated
        {
            get { return dataLeftAgeRelated; }
            set { dataLeftAgeRelated = value; }
        }

        private string dataLeftMeanECOfLesion;

        public string DataLeftMeanECOfLesion
        {
            get { return dataLeftMeanECOfLesion; }
            set { dataLeftMeanECOfLesion = value; }
        }

        private string dataLeftFindings;

        public string DataLeftFindings
        {
            get { return dataLeftFindings; }
            set { dataLeftFindings = value; }
        }

        private string dataLeftMammaryGlandResult;

        public string DataLeftMammaryGlandResult
        {
            get { return dataLeftMammaryGlandResult; }
            set { dataLeftMammaryGlandResult = value; }
        }

        private string dataRightMammaryGland;

        public string DataRightMammaryGland
        {
            get { return dataRightMammaryGland; }
            set { dataRightMammaryGland = value; }
        }

        private string dataRightAgeRelated;

        public string DataRightAgeRelated
        {
            get { return dataRightAgeRelated; }
            set { dataRightAgeRelated = value; }
        }

        private string dataRightMeanECOfLesion;

        public string DataRightMeanECOfLesion
        {
            get { return dataRightMeanECOfLesion; }
            set { dataRightMeanECOfLesion = value; }
        }

        private string dataRightFindings;

        public string DataRightFindings
        {
            get { return dataRightFindings; }
            set { dataRightFindings = value; }
        }

        private string dataRightMammaryGlandResult;

        public string DataRightMammaryGlandResult
        {
            get { return dataRightMammaryGlandResult; }
            set { dataRightMammaryGlandResult = value; }
        }

        private string dataLeftMaxFlag;
        public string DataLeftMaxFlag
        {
            get { return dataLeftMaxFlag; }
            set { dataLeftMaxFlag = value; }
        }
        private string dataRightMaxFlag;
        public string DataRightMaxFlag
        {
            get { return dataRightMaxFlag; }
            set { dataRightMaxFlag = value; }
        }


        private string dataTotalPts;
        public string DataTotalPts
        {
            get { return dataTotalPts; }
            set { dataTotalPts = value; }
        }
        private string dataLeftTotalPts;
        public string DataLeftTotalPts
        {
            get { return dataLeftTotalPts; }
            set { dataLeftTotalPts = value; }
        }
        private string dataRightTotalPts;
        public string DataRightTotalPts
        {
            get { return dataRightTotalPts; }
            set { dataRightTotalPts = value; }
        }


        private string dataBiRadsCategory;
        public string DataBiRadsCategory
        {
            get { return dataBiRadsCategory; }
            set { dataBiRadsCategory = value; }
        }
        private string dataLeftBiRadsCategory;
        public string DataLeftBiRadsCategory
        {
            get { return dataLeftBiRadsCategory; }
            set { dataLeftBiRadsCategory = value; }
        }
        private string dataRightBiRadsCategory;
        public string DataRightBiRadsCategory
        {
            get { return dataRightBiRadsCategory; }
            set { dataRightBiRadsCategory = value; }
        }


        private string dataPoint;
        public string DataPoint
        {
            get { return dataPoint; }
            set { dataPoint = value; }
        }

        /// <summary>
        /// 克隆此对象
        /// </summary>
        /// <returns></returns>
        Object ICloneable.Clone()
        {
            return this.Clone();
        }
        /// <summary>
        /// 克隆此对象
        /// </summary>
        /// <returns></returns>
        public ShortFormReport Clone()
        {
            return (ShortFormReport)this.MemberwiseClone(); 
        }
    }
}
