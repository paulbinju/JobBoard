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
    
    public partial class CandidateEducation_T
    {
        public int CaEdu_ID { get; set; }
        public Nullable<int> Ca_ID { get; set; }
        public Nullable<int> UniversityID { get; set; }
        public Nullable<int> UniCountryID { get; set; }
        public Nullable<int> DegreeID { get; set; }
        public Nullable<int> SpecializationID { get; set; }
        public Nullable<int> GradeID { get; set; }
        public Nullable<int> FromMonth { get; set; }
        public Nullable<int> FromYear { get; set; }
        public Nullable<int> ToMonth { get; set; }
        public Nullable<int> ToYear { get; set; }
        public Nullable<bool> CurrentlyStudyHere { get; set; }
        public string Activities { get; set; }
        public Nullable<System.DateTime> DOC { get; set; }
    }
}
