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
    
    public partial class JobSearchSector
    {
        public int jss_Id { get; set; }
        public System.Guid jss_GUID { get; set; }
        public System.DateTime jss_Created { get; set; }
        public int jss_js_JobSearchId { get; set; }
        public int jss_lm_SectorId { get; set; }
        public Nullable<System.DateTime> jss_Deleted { get; set; }
    }
}
