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
    
    public partial class PackageType
    {
        public int pt_Id { get; set; }
        public System.Guid pt_GUID { get; set; }
        public System.DateTime pt_Created { get; set; }
        public System.DateTime pt_LastUpdated { get; set; }
        public Nullable<System.DateTime> pt_Deleted { get; set; }
        public string pt_Name { get; set; }
        public int pt_Grouping { get; set; }
        public int pt_DisplayOrder { get; set; }
        public string pt_Description { get; set; }
        public string pt_DetaiLlink { get; set; }
        public int pt_Jobs { get; set; }
        public bool pt_FeaturedJob { get; set; }
        public bool pt_JobLogo { get; set; }
        public bool pt_FeatJobSectorHome { get; set; }
        public bool pt_FeatJobBoardHome { get; set; }
        public bool pt_ProfilePage { get; set; }
        public bool pt_FeatEmpSectorHome { get; set; }
        public bool pt_FeatEmpBoardHome { get; set; }
        public bool pt_Microsite { get; set; }
        public bool pt_CvSearch { get; set; }
        public bool pt_EmpBanner { get; set; }
        public bool pt_EmpMpuBanner { get; set; }
        public bool pt_NotifyByEmail { get; set; }
        public int pt_DurationDays { get; set; }
    }
}