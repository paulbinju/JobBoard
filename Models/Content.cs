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
    
    public partial class Content
    {
        public int cont_Id { get; set; }
        public System.Guid cont_Guid { get; set; }
        public System.DateTime cont_Created { get; set; }
        public System.DateTime cont_LastUpdated { get; set; }
        public int cont_us_userID { get; set; }
        public Nullable<int> cont_ParentId { get; set; }
        public string cont_Name { get; set; }
        public bool cont_System { get; set; }
        public int cont_br_BrandId { get; set; }
        public int cont_CommonId { get; set; }
        public string cont_Title { get; set; }
        public string cont_Url { get; set; }
        public string cont_Body { get; set; }
        public string cont_MetaTitle { get; set; }
        public string cont_MetaKeywords { get; set; }
        public string cont_MetaDescription { get; set; }
        public Nullable<System.DateTime> cont_Deleted { get; set; }
        public bool cont_Sitemap { get; set; }
        public int cont_Navmenu { get; set; }
    }
}
