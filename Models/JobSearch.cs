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
    
    public partial class JobSearch
    {
        public int js_Id { get; set; }
        public System.Guid js_GUID { get; set; }
        public System.DateTime js_Created { get; set; }
        public System.DateTime js_LastUpdated { get; set; }
        public Nullable<System.DateTime> js_Deleted { get; set; }
        public int js_ca_CandidateId { get; set; }
        public string js_SearchName { get; set; }
        public string js_Keywords { get; set; }
        public decimal js_SalaryFrom { get; set; }
        public decimal js_SalaryTo { get; set; }
        public string js_JobReference { get; set; }
        public int js_PostedWithinDays { get; set; }
        public int js_RecruiterType { get; set; }
        public bool js_JbeEnabled { get; set; }
        public int js_JbeFrequency { get; set; }
        public System.DateTime js_LastRan { get; set; }
        public int js_VacanciesInList { get; set; }
        public int js_KeywordsScopeID { get; set; }
    }
}