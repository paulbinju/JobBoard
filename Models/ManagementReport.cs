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
    
    public partial class ManagementReport
    {
        public int mrep_Id { get; set; }
        public System.Guid mrep_Guid { get; set; }
        public System.DateTime mrep_Created { get; set; }
        public System.DateTime mrep_LastUpdated { get; set; }
        public int mrep_us_UserId { get; set; }
        public string mrep_Name { get; set; }
        public string mrep_SQL { get; set; }
        public int mrep_st_StatusId { get; set; }
        public bool mrep_usesdates { get; set; }
        public bool mrep_usecompanies { get; set; }
        public bool mrep_useemployers { get; set; }
        public bool mrep_useusers { get; set; }
        public bool mrep_uselocations { get; set; }
        public int mrep_mrepg_ManagementReportGroups { get; set; }
        public bool mrep_usebrands { get; set; }
    }
}
