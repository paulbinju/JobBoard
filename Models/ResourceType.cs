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
    
    public partial class ResourceType
    {
        public int id { get; set; }
        public System.DateTime created { get; set; }
        public int createdby { get; set; }
        public System.DateTime lastupdated { get; set; }
        public int lastupdatedby { get; set; }
        public bool deleted { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string filename { get; set; }
        public int contentid { get; set; }
        public int nocontentid { get; set; }
        public Nullable<int> detailcontentid { get; set; }
        public string attributesconfigxml { get; set; }
        public string rewrittenlisting { get; set; }
        public string rewrittendetail { get; set; }
        public bool usebody { get; set; }
        public bool usesummary { get; set; }
        public int itemsperpage { get; set; }
        public int logoid { get; set; }
    }
}