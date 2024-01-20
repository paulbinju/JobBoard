using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs4Bahrainis.Models
{
    public class CSPackage
    {

        public int PackageID { get; set; }
        public string PackageName { get; set; }
        public int AmountPerYear { get; set; }
    }

    public class CareerServices
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Article { get; set; }
        public DateTime DOC { get; set; }
    }

    public class CompanyVideos
    {
        public int CoVideo_ID { get; set; }
        public string VideoURL { get; set; }
    }


    public class PhotoGallery
    {
        public int PGID { get; set; }
        public Guid PG_GUID { get; set; }
        public int Co_ID { get; set; }
        public string PhotoExtension { get; set; }
        public string ImagePath { get; set; }
    }

    public class Testimonials
    {
        public int CTID { get; set; }
        public int Co_ID { get; set; }
        public Guid CT_GUID { get; set; }
        public string CTDetails { get; set; }
        public string CTPicExtension { get; set; }

        
    
    }
    public class Medias
    {
        public int CMID { get; set; }
        public int Co_ID { get; set; }
        public Guid CM_GUID { get; set; }
        public string CMTitle { get; set; }
        public string CMPicExtension { get; set; }



    }
 
    public class jobapplicants
    {
        public int ca_id { get; set; }
        public int app_id { get; set; }
        public int app_st_ShortlistStatusId { get; set; }
        public string ca_PhotoExtension { get; set; }
        public DateTime app_created { get; set; }
        public DateTime ca_LastLogin { get; set; }
        public Guid ca_GUID { get; set; }
        public string ca_FirstName { get; set; }
        public string ca_Surname { get; set; }
        public string CurrentCountry { get; set; }
        public string educationlevel { get; set; }
        public string ca_Profile { get; set; }
        public bool ca_hasphoto { get; set; }
        public string ca_photoextension { get; set; }
        public string ca_Gender { get; set; }
        public DateTime ca_DateOfBirth { get; set; }
        public string ca_maritalstatus { get; set; }
        public string functiontitle { get; set; }
        public string CurrentJobTitle { get; set; }
        public string TotalRelevantExperience { get; set; }

    }

    public class reccontacts
    {
        public int ct_Id { get; set; }
        public int ct_co_CompanyId { get; set; }
        public Guid co_Guid { get; set; }
        public string ct_Forename { get; set; }
        public string ct_Surname { get; set; }
        public string ct_EmailAddress { get; set; }
        public string ct_Password { get; set; }
        public string ct_Telephone { get; set; }
        public int co_PositioninCompanyID { get; set; }
        public string lm_Value { get; set; }
        public bool ct_Superuser { get; set; }
        public DateTime? ct_Deleted { get; set; }
         
        public bool? ct_PostJob { get; set; }
        public bool? ct_SearchCV { get; set; }
    }

    public class LatestJobs
    {
        public int vc_id { get; set; }
        public string vc_Title { get; set; }
        public string vc_Location { get; set; }
        public string co_Name { get; set; }
        public string co_NameURL { get; set; }
        public string vc_Description { get; set; }
        public Guid co_Guid { get; set; }
        public DateTime vc_Created { get; set; }
        public string LogoURL { get; set; }
        public string jobURL { get; set; }
        public string postedsince { get; set; }
        public string co_LogoExtension { get; set; }
        public int? lm_Id { get; set; }
        public string lm_value { get; set; }
        public int vc_co_CompanyId { get; set; }
        public int vc_views { get; set; }
        public int vc_applications { get; set; }
        public bool vc_Confidential { get; set; }

    }


    public class ManageVacancies
    {
        public int vc_id { get; set; }
        public DateTime vc_Created { get; set; }
        public DateTime? vc_Deleted { get; set; }
        public DateTime? vc_ExpiryDate { get; set; }
        public int vc_st_StatusID { get; set; }
        public string vc_Reference { get; set; }
        public string vc_Title { get; set; }
        public string ct_Forename { get; set; }
        public string ct_Surname { get; set; }
        public int vc_Views { get; set; }
        public int applicants { get; set; }
    }

    public class forgotpass
    {
        public int id { get; set; }
        public string emailid { get; set; }
        public string passwordx { get; set; }
        public Guid guidx { get; set; }
    }


    public class jobsector
    {
        public int SectorID { get; set; }
        public string Sector { get; set; }
        public int Total { get; set; }
        public string SectorURL { get; set; }
    }
   
    public class JobSeekers
    {
        public int ca_id { get; set; }
        public Guid ca_GUID { get; set; }
        public string ca_FirstName { get; set; }
        public string ca_Surname { get; set; }
        public string ca_EmailAddress { get; set; }
        public string ca_PhoneMobile { get; set; }
        public DateTime? ca_DateOfBirth { get; set; }
        public string ca_Gender { get; set; }
        public int Age { get; set; }
        public string ProfilePic { get; set; }
        public string ca_photoextension { get; set; }
        public string lm_value { get; set; }
        public decimal ca_SalaryFrom { get; set; }
        public string ca_Profile { get; set; }
        public string ca_MaritalStatus { get; set; }

        public bool ca_HasPhoto { get; set; }
        


    }

    public class CandidateNew
    {
        public int Ca_ID { get; set; }
        public Guid ca_GUID { get; set; }
        public int? ca_lm_countryid { get; set; }
        public int? Ca_CurrentCountryID { get; set; }
        public int? ca_lm_EducationLevel { get; set; }
        public int? ca_UniversityID { get; set; }
        public int? Ca_SectorIndustryID { get; set; }
        public int? ca_FunctionTitleID { get; set; }
        public int? ca_CurrentJobTitleID { get; set; }
        public int? ca_TotalRelavantExperience { get; set; }

        public string ca_FirstName { get; set; }
        public string ca_Surname { get; set; }
        public string Nationality { get; set; }
        public string ca_PhoneMobile { get; set; }
        public string ca_EmailAddress { get; set; }
        public string ca_Password { get; set; }
        public DateTime? ca_DateOfBirth { get; set; }
        public DateTime? ca_lastupdated { get; set; }
        public DateTime? ca_created { get; set; }
        public string ca_Gender { get; set; }
        public string ca_MaritalStatus { get; set; }
        public string CurrentLocation { get; set; }
        public string HighestEducation { get; set; }
        public string University { get; set; }
        public string FunctionTitle { get; set; }
        public string JobTitle { get; set; }
        public string TotalExperience { get; set; }
        public string ca_Profile { get; set; }
        public bool ca_HasPhoto { get; set; }
        public string ca_photoextension { get; set; }
        public int ca_CompletedRegTabs { get; set; }
        public DateTime? ca_emailactivatedon { get; set; }
        public bool? Ca_EmailActivated { get; set; }
        public bool? ca_IsFresher { get; set; }
        


    }

    public class EmployersTemp {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCR { get; set; }
        public string POBox { get; set; }
        public string ContactTime { get; set; }
        public DateTime DOC { get; set; }
    }


    public class CandidateSkills
    {

        public int CaSkillID { get; set; }
        public int Ca_ID { get; set; }
        public string SkillLevel { get; set; }
        public string Skills { get; set; }
        public int SkillID { get; set; }
        public int SkillLevelID { get; set; }
    }

    public class CandidateApplications {
        public int vc_Id { get; set; }
        public int app_id { get; set; }
        public DateTime app_created { get; set; }
        public string vc_Title { get; set; }
        public string lm_Value { get; set; }
    }


    public class JobApplicant
    {
        public string ca_FirstName { get; set; }
        public string ca_Surname { get; set; }
        public string vc_Title { get; set; }
        public DateTime app_LastUpdated { get; set; }
        public string vc_ApplicationEmail { get; set; }
    }



    public class CandiateNotes
    {
        public DateTime notedate { get; set; }
        public string ct_Forename { get; set; }
        public string ct_Surname { get; set; }
        public string notedetails { get; set; }
    }
    public class CandidateLanguages
    {

        public int Caln_ID { get; set; }
        public int Ca_ID { get; set; }
        public string Language { get; set; }
        public string Proficiency { get; set; }
        public int ProficiencyID { get; set; }
        public int LanguageID { get; set; }

    }



    public class Nationality
    {

        public int lm_Id { get; set; }
        public string lm_Value { get; set; }

    }
    public class ListMaster
    {
        public int lm_Id { get; set; }
        public int lm_lt_ListTypeId { get; set; }
        public int lm_Ordinal { get; set; }
        public string lm_Value { get; set; }
        public string SectorURL { get; set; }

    }

    public class CandiCount
    {
        public int CandiCounter { get; set; }
    }

    public class AllCompanies {
        public int co_id { get; set; }
        public string co_Name { get; set; }
        public Guid co_Guid { get; set; }
        public string co_postaladdress { get; set; }
        public string co_telephone { get; set; }
        public DateTime co_created { get; set; }

        public int ct_id { get; set; }
        public string ct_forename { get; set; }
        public string ct_surname { get; set; }
        public string ct_emailaddress { get; set; }
        public string ct_password { get; set; }
        public string ct_telephone { get; set; }
        public string packagename { get; set; }
        public DateTime? startdate { get; set; }
        public DateTime? enddate { get; set; }
        public DateTime ct_lastlogin { get; set; }

        public string co_LogoExtension { get; set; }
        public string LogoURL { get; set; }

    }



    public class BOVacancies {
        public int ID { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public DateTime LastUpdated { get; set; }
    }


    public class BOJobFair
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string CV { get; set; }
        public DateTime DOC { get; set; }

    }



    public class CompanyContacts
    {
        public int ct_id { get; set; }
        public string name { get; set; }
        public string usertype { get; set; }
    }

    public class Reports {
        public int ContactID { get; set; }
        public string Type { get; set; }
        public int Total { get; set; }

    }

    public class Reports2
    {
        public string name { get; set; }
        public string type { get; set; }
        public int total { get; set; }

        public int credits { get; set; }
        public DateTime lastlogin { get; set; }
    }


    public class CompanyNames
    {
        public string Company { get; set; }
    }
    public class JobTitles
    {
        public string JobTitle { get; set; }
    }

    public class IdealCareeMove
    {
        public int CaICM_ID { get; set; }
        public int Ca_ID { get; set; }
        public int JobTitleID { get; set; }
        public int FunctionID { get; set; }
        public int IndustryID { get; set; }
        public int ExperienceID { get; set; }
        public int ExpectedSalaryID { get; set; }
        public string JobTitle { get; set; }
        public string JobFunction { get; set; }
        public string Industry { get; set; }
        public string ExpectedSalary { get; set; }
        public string Experience { get; set; }
    }


    public class CandidateEducationNew
    {
        public int CaEdu_ID { get; set; }
        public int Ca_ID { get; set; }
        public int UniversityID { get; set; }
        public int UniCountryID { get; set; }
        public int DegreeID { get; set; }
        public int SpecializationID { get; set; }
        public int GradeID { get; set; }
        public string University { get; set; }
        public string UniCountry { get; set; }
        public string Degree { get; set; }
        public string Specialization { get; set; }
        public string Grade { get; set; }
        public int FromMonth { get; set; }
        public int FromYear { get; set; }
        public int ToMonth { get; set; }
        public int ToYear { get; set; }
        public bool CurrentlyStudyHere { get; set; }
        public string Activities { get; set; }

         

    }

    public class CandidateWorkHistory
    {
        public int CaWorkHistory_ID { get; set; }
        public int Ca_ID { get; set; }
        public int JobTitleID { get; set; }
        public int JobLevelID { get; set; }
        public int ReportingToID { get; set; }
        public int IndustryID { get; set; }
        public int FunctionID { get; set; }
        public int JobLocationID { get; set; }
        public int SalaryID { get; set; }
        public string JobTitle { get; set; }
        public string JobLevel { get; set; }
        public string Company { get; set; }
        public string ReportingTo { get; set; }
        public string Industry { get; set; }
        public string JobFunction { get; set; }
        public int FromMonth { get; set; }
        public int FromYear { get; set; }
        public int ToMonth { get; set; }
        public int ToYear { get; set; }
        public bool CurrentlyWorkingHere { get; set; }
        public string JobLocation { get; set; }
        public string Salary { get; set; }
        public string Description { get; set; }
        



    }

    public class CandidateCertifications
    {
        public int CaCertification_ID { get; set; }
        public int Ca_ID { get; set; }
        public int CertificationID { get; set; }
        public int AuthorityID { get; set; }
        public string Certification { get; set; }
        public string Authority { get; set; }
        public int FromMonth { get; set; }
        public int FromYear { get; set; }
        public int ToMonth { get; set; }
        public int ToYear { get; set; }
        public bool DoNotExpire { get; set; }
    }

    public class CandidateDocuments
    {
        public Guid cd_Guid { get; set; }
        public int cd_CaEdu_ID { get; set; }
        public String cd_OriginalName { get; set; }
        public string cd_FileExtension { get; set; }
    }


    public class CSCredits
    {
        public int CreditTypeID { get; set; }
        public int CreditsPerMonth { get; set; }
    }

    public class CSCreditUsage
    {
        public int CompanyID { get; set; }
        
    }

    public class CSSubscription
    {
        public int SubscriptionID { get; set; }
        public string PackageName { get; set; }
        public string ct_Forename { get; set; }
        public string ct_Surname { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime PurchaseDate { get; set; }


    }

    public class CCredits
    {
        public int CreditTypeID { get; set; }
        public string CreditType { get; set; }
        public int VoucherMonth { get; set; }
        public int VoucherYear { get; set; }
        public int CreditBalance { get; set; }
    }

    public class JobDetails
    {
        public int vc_Id { get; set; }
        public Guid vc_GUID { get; set; }
        public DateTime vc_Created { get; set; }
        public DateTime? vc_LastUpdated { get; set; }
        public DateTime? vc_Deleted { get; set; }
        public int vc_br_SourceBrandId { get; set; }
        public int vc_co_CompanyId { get; set; }
        public int vc_ct_ContactId { get; set; }
        public string vc_Reference { get; set; }
        public string vc_Title { get; set; }
        public int vc_lm_JobTypeId { get; set; }
        public string vc_Description { get; set; }
        public string vc_ApplicationEmail { get; set; }
        public string vc_ApplicationEmail2 { get; set; }
        public DateTime? vc_ClosingDate { get; set; }
        public DateTime? vc_ExpiryDate { get; set; }
        public int vc_Views { get; set; }
        public int vc_Applications { get; set; }
        public int vc_st_StatusID { get; set; }
        public bool vc_Disabled { get; set; }
        public int vc_IndustryID { get; set; }
        public int vc_FunctionID { get; set; }
        public int vc_ExperienceID { get; set; }
        public int vc_QualificationID { get; set; }
        public int vc_AgegroupID { get; set; }
        public int vc_SalaryRangeID { get; set; }
        public int vc_JobLocationID { get; set; }
        public string vc_JobRequirements { get; set; }
        public string vc_CompanyDetails { get; set; }

        public string co_Name { get; set; }
        public string co_NameURL { get; set; }
        public Guid co_Guid { get; set; }
        public string LogoURL { get; set; }
        public string co_LogoExtension { get; set; }
        public string postedsince { get; set; }
        public string JobType { get; set; }
        public string Industry { get; set; }
        public string Functions { get; set; }
        public string Experience { get; set; }
        public string Qualification { get; set; }
        public string AgeGroup { get; set; }
        public string SalaryRange { get; set; }
        public string JobLocation { get; set; }
        public bool vc_Confidential { get; set; }

    }


    public class UserActvReport
    {
        public string ct_Forename { get; set; }
        public string ct_Surname { get; set; }
        public string vc_Title { get; set; }
        public string ca_FirstName { get; set; }
        public string ca_Surname { get; set; }
        public DateTime? ct_LastLogin { get; set; }
        public DateTime? UsedOn { get; set; }
        public int? CreditTypeID { get; set; }
        public int? CandidateID { get; set; }
        public int?  VacancyID { get; set; }
       

 
    }

 


}