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
    
    public partial class NewsArticle
    {
        public int news_id { get; set; }
        public System.Guid news_guid { get; set; }
        public System.DateTime news_Created { get; set; }
        public System.DateTime news_LastUpdated { get; set; }
        public int news_us_UserId { get; set; }
        public int news_us_LastUpdatedUserId { get; set; }
        public string news_Title { get; set; }
        public string news_Summary { get; set; }
        public string news_Detail { get; set; }
        public Nullable<System.DateTime> news_DisplayFrom { get; set; }
        public Nullable<System.DateTime> news_DisplayTo { get; set; }
        public Nullable<byte> news_DisplayOption { get; set; }
        public int news_st_StatusId { get; set; }
        public bool news_HasImage { get; set; }
        public string news_ImageExtension { get; set; }
        public Nullable<System.DateTime> news_Deleted { get; set; }
        public string news_URL { get; set; }
    }
}