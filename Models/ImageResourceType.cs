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
    
    public partial class ImageResourceType
    {
        public int id { get; set; }
        public System.DateTime created { get; set; }
        public int createdby { get; set; }
        public System.DateTime lastupdated { get; set; }
        public int lastupdatedby { get; set; }
        public string name { get; set; }
        public string intro { get; set; }
        public int maxwidth { get; set; }
        public int maxheight { get; set; }
        public int minwidth { get; set; }
        public int minheight { get; set; }
        public bool jpg { get; set; }
        public bool gif { get; set; }
        public bool resize { get; set; }
        public bool uselibrary { get; set; }
        public bool useupload { get; set; }
    }
}
