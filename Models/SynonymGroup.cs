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
    
    public partial class SynonymGroup
    {
        public int syng_Id { get; set; }
        public System.Guid syng_Guid { get; set; }
        public System.DateTime syng_Created { get; set; }
        public string syng_name { get; set; }
        public int syng_br_BrandId { get; set; }
        public int syng_us_UserId { get; set; }
        public Nullable<System.DateTime> syng_Deleted { get; set; }
    }
}
