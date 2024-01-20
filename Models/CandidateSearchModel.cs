using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Jobs4Bahrainis.Models
{
    public class CandidateSearchModel
    {
        public string fname { get; set; }
        public string lname { get; set; }
        public int? age { get; set; }
        public string gender { get; set; }
        public string[] chksector { get; set; }
        public int? qualification { get; set; }
        public string salary { get; set; }
        public string experience { get; set; }
        public string keyword { get; set; }
    }
    public class CandidateSearchResultModel
    {
        public string ca_FullName { get; set; }
        public string ca_EmailAddress { get; set; }
        public string ca_PhoneMobile { get; set; }
        public DateTime? ca_DateOfBirth { get; set; }
        public string ca_Gender { get; set; }
        public int? ca_Age { get; set; }
        public int ca_Id { get; set; }
        public decimal? ca_SalaryFrom { get; set; }
        public decimal? ca_SalaryTo { get; set; }
        public string ca_Profile { get; set; }
        public string EducationLevel { get; set; }
        public string Interest { get; set; }
        public string LogoURL
        {
            get; set;
            //get
            //{
            //    var extns = string.IsNullOrEmpty(co_LogoExtension) ? "jpg" : co_LogoExtension;
            //    return "http://www.jobs4bahrainis.com/Logox/" + co_Guid.ToString().Substring(0, 2) + "/" + co_Guid.ToString().Substring(2, 2) + "/" + co_Guid.ToString().Substring(4, 2) + "/" + co_Guid.ToString() + "." + extns;
            //}
            
        }
        public string CurrentLocation { get; set; }
        public string CurrentJobTitle { get; set; }
        public string Company { get; set; }
        public string Salary { get; set; }
        public string ExpectedSalary { get; set; }
        public DateTime? ca_LastLogin  { get; set; }
        public string TotalExperience { get; set; }
        






    }
    public class CandidateSearchInerestResultModel
    {
        public int ca_id { get; set; }
        public string Interest { get; set; }

    }

    public class JobSearchModel
    {

        public int? qualification { get; set; }
        public string salary { get; set; }
        public string experience { get; set; }
        public string keyword { get; set; }
    }
    public class JobSearchResultModel
    {
        

        public int vc_co_CompanyId { get; set; }
        public int vc_Id { get; set; }
        public bool vc_Confidential { get; set; }

        public string vc_Title { get; set; }
       // public string vc_Description { get; set; }
        public string vc_Location { get; set; }
        public string vc_description { get; set; }
        public string vc_descriptionNoHTML {
            get {return TruncateAtWord(vc_descriptionStrip, 100); }
        }

        public string vc_descriptionStrip
        {
            get { return Regex.Replace(vc_description, "<.*?>", String.Empty);  }
        }

        private string TruncateAtWord(string value, int length)
        {
            if (value == null || value.Length < length || value.IndexOf(" ", length) == -1)
                return value;

            return value.Substring(0, value.IndexOf(" ", length));
        }

        public DateTime vc_Created { get; set; }
        public Guid co_Guid { get; set; }
        public int days_posted { get { return DateTime.Now.Subtract(vc_Created).Days; } }
        public string  postedsince { get { return vc_Created.ToString("dd-MM-yyyy"); } }
        public int vc_Views { get; set; }
        public int vc_Applications { get; set; }
        public string co_LogoExtension { get; set; }
        public string LogoURL
        {
            get;set;
            //get
            //{
            //    var extns = string.IsNullOrEmpty(co_LogoExtension) ? "jpg" : co_LogoExtension;
            //    return "http://www.jobs4bahrainis.com/Logox/" + co_Guid.ToString().Substring(0, 2) + "/" + co_Guid.ToString().Substring(2, 2) + "/" + co_Guid.ToString().Substring(4, 2) + "/" + co_Guid.ToString() + "." + extns;
            //}
        }
        public string co_name { get; set; }

    }
   
}