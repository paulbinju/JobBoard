//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Jobs4Bahrainis.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class VacanciesBkp
    {
        public int vc_Id { get; set; }
        public System.Guid vc_GUID { get; set; }
        public System.DateTime vc_Created { get; set; }
        public System.DateTime vc_LastUpdated { get; set; }
        public Nullable<System.DateTime> vc_Deleted { get; set; }
        public int vc_br_SourceBrandId { get; set; }
        public int vc_co_CompanyId { get; set; }
        public int vc_ct_ContactId { get; set; }
        public string vc_Reference { get; set; }
        public string vc_Title { get; set; }
        public int vc_lm_JobTypeId { get; set; }
        public string vc_Location { get; set; }
        public string vc_Experience { get; set; }
        public string vc_Qualification { get; set; }
        public string vc_AgeGroup { get; set; }
        public decimal vc_SalaryFrom { get; set; }
        public decimal vc_SalaryTo { get; set; }
        public string vc_Bonus { get; set; }
        public string vc_Commission { get; set; }
        public string vc_Benefits { get; set; }
        public string vc_Description { get; set; }
        public string vc_ApplicationEmail { get; set; }
        public string vc_ApplicationEmail2 { get; set; }
        public Nullable<System.DateTime> vc_ClosingDate { get; set; }
        public Nullable<System.DateTime> vc_ExpiryDate { get; set; }
        public Nullable<System.DateTime> vc_FeaturedUntil { get; set; }
        public Nullable<System.DateTime> vc_FeaturedSectorUntil { get; set; }
        public Nullable<System.DateTime> vc_FeaturedHomeUntil { get; set; }
        public int vc_BoostCount { get; set; }
        public Nullable<System.DateTime> vc_LastBoost { get; set; }
        public string vc_JobG8Id { get; set; }
        public string vc_JobG8Advertiser { get; set; }
        public int vc_Views { get; set; }
        public int vc_Applications { get; set; }
        public int vc_st_StatusID { get; set; }
        public string vc_KillerXML { get; set; }
        public string vc_ApplicationURL { get; set; }
        public string vc_Duration { get; set; }
        public Nullable<int> vc_MinimumQualificationLevelID { get; set; }
        public Nullable<bool> vc_Disabled { get; set; }
        public Nullable<int> vc_IndustryID { get; set; }
        public Nullable<int> vc_FunctionID { get; set; }
        public Nullable<int> vc_ExperienceID { get; set; }
        public Nullable<int> vc_QualificationID { get; set; }
        public Nullable<int> vc_AgegroupID { get; set; }
        public Nullable<int> vc_SalaryRangeID { get; set; }
        public Nullable<int> vc_JobLocationID { get; set; }
        public string vc_JobRequirements { get; set; }
        public string vc_CompanyDetails { get; set; }
    }
}
