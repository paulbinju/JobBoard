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
    
    public partial class VacancySector
    {
        public int vs_Id { get; set; }
        public System.Guid vs_GUID { get; set; }
        public System.DateTime vs_Created { get; set; }
        public Nullable<System.DateTime> vs_Deleted { get; set; }
        public int vs_vc_VacancyId { get; set; }
        public int vs_lm_SectorId { get; set; }
    }
}
