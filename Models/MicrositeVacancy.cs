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
    
    public partial class MicrositeVacancy
    {
        public int MicrositeID { get; set; }
        public int VacancyID { get; set; }
    
        public virtual Microsite Microsite { get; set; }
    }
}
