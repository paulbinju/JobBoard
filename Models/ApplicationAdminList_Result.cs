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
    
    public partial class ApplicationAdminList_Result
    {
        public int app_Id { get; set; }
        public System.DateTime app_Created { get; set; }
        public int app_vc_VacancyId { get; set; }
        public int app_ca_CandidateId { get; set; }
        public string ca_Firstname { get; set; }
        public string ca_Surname { get; set; }
        public Nullable<int> ca_MediatorKey { get; set; }
        public string co_Name { get; set; }
        public string ct_Forename { get; set; }
        public string ct_Surname { get; set; }
        public string vc_Title { get; set; }
        public string vc_Reference { get; set; }
        public string st_Name { get; set; }
        public string Ranking { get; set; }
    }
}
