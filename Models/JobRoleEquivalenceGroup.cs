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
    
    public partial class JobRoleEquivalenceGroup
    {
        public int jreg_Id { get; set; }
        public System.Guid jreg_Guid { get; set; }
        public System.DateTime jreg_Created { get; set; }
        public int jreg_us_UserId { get; set; }
        public int jreg_br_BrandId { get; set; }
        public string jreg_Name { get; set; }
        public Nullable<System.DateTime> jreg_Deleted { get; set; }
    }
}
