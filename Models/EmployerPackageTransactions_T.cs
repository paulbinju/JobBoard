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
    
    public partial class EmployerPackageTransactions_T
    {
        public int EmpPkgTransID { get; set; }
        public Nullable<int> PkgID { get; set; }
        public bool Jobs { get; set; }
        public Nullable<bool> Usage { get; set; }
        public Nullable<int> CompanyID { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<System.DateTime> DOE { get; set; }
    }
}