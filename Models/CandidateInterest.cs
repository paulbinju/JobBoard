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
    
    public partial class CandidateInterest
    {
        public int ci_Id { get; set; }
        public System.Guid ci_GUID { get; set; }
        public System.DateTime ci_Created { get; set; }
        public int ci_ca_CandidateId { get; set; }
        public int ci_lm_LinkId { get; set; }
        public int ci_cit_InterestTypeId { get; set; }
        public Nullable<System.DateTime> ci_Deleted { get; set; }
    }
}
