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
    
    public partial class Report_V
    {
        public string Type { get; set; }
        public string ct_Forename { get; set; }
        public string ct_Surname { get; set; }
        public int ReportID { get; set; }
        public Nullable<int> ContactID { get; set; }
        public Nullable<int> CompanyID { get; set; }
        public Nullable<System.DateTime> ActivityDate { get; set; }
        public Nullable<int> ReportTypeID { get; set; }
    }
}
