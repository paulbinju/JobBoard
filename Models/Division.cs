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
    
    public partial class Division
    {
        public int div_Id { get; set; }
        public System.Guid div_Guid { get; set; }
        public System.DateTime div_Created { get; set; }
        public System.DateTime div_LastUpdated { get; set; }
        public Nullable<System.DateTime> div_Deleted { get; set; }
        public string div_Name { get; set; }
        public bool div_HasLogo { get; set; }
        public string div_LogoExtension { get; set; }
        public int div_br_BrandId { get; set; }
        public int div_IntegrationKey { get; set; }
    }
}