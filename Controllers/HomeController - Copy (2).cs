using Jobs4Bahrainis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Globalization;
using System.Text;
using System.Drawing;
using System.IO;
using System.Data.SqlClient;
using PagedList;
using PagedList.Mvc;

namespace Jobs4Bahrainis.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                ViewBag.Sectors = db.ListsMasters.Where(x => x.lm_lt_ListTypeId == 4 && x.lm_Deleted == null).OrderBy(x => x.lm_Value).ToList();

                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(@"select top 12 vc_Id,co_id,vc_Title,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies 
inner join Companies on vc_co_CompanyId=co_Id where vc_Id in
 (select top 50 max(vc_Id) as vc_id from Vacancies inner join Companies on vc_co_CompanyId=co_Id
  where vc_Deleted is null  and vc_br_SourceBrandId=2     group by co_id order by vc_id desc)  and vc_id not in (38193,38201,38221,38195,38188)  order by vc_id desc
 ").ToList();

                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = "Manama";
                    //lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");
                    try
                    {
                        ljx.vc_Description = fparamaker(lj.vc_Description, 9);
                    }
                    catch
                    {
                        ljx.vc_Description = lj.vc_Description;
                    }
                    ljx.vc_Created = lj.vc_Created;
                    try {
                        ljx.co_Name = lj.co_Name.Substring(0, 15) + "...";
                    }
                    catch {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.LogoURL = "http://www.jobs4bahrainis.com/Logox/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + (lj.co_LogoExtension != "" ? lj.co_LogoExtension : ".jpg");


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days > 200)
                    {
                        days = days - 200;
                    }
                    else if (days > 40)
                    {
                        days = days - 40;
                    }
                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    ljx.postedsince = "" + days;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToList();




            }


            return View();
        }

        public ActionResult menulogin() {

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                if (Session["Ca_ID"] != null)
                {
                    int candidateid = Convert.ToInt32(Session["Ca_ID"]);


                    var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                    if (candidate.Count == 0)
                    {
                        candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                    }

                    if (candidate[0].ca_HasPhoto == true)
                    {
                        ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

                    }

                    ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
                }
                else if (Session["RecruiterID"] != null)
                {
                    int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
                    var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                    int companyid = recruiter[0].ct_co_CompanyId;

                    var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                    string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                    ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                    ViewBag.Name = recruiter[0].ct_Forename;
                }

            }

            return PartialView();
        }


        public ActionResult Login(string loggedin, string returnurl) {
            Session["RecruiterID"] = null;
            Session["RecruiterName"] = null;
            Session["CompanyID"] = null;
            Session["Recruiter1"] = null;
            Session["Recruiter2"] = null;

            Session["Ca_ID"] = null;
            Session["CandidateName"] = null;

            if (loggedin == "invalid")
            {
                ViewBag.LoggedIn = "Invalid Email ID / Password ! try again.";
            }

            ViewBag.returnurl = returnurl;

            return View();
        }



        [HttpPost]
        public ActionResult Login(FormCollection col)
        {
            Session["RecruiterID"] = null;
            Session["RecruiterName"] = null;
            Session["CompanyID"] = null;
            Session["Recruiter1"] = null;
            Session["Recruiter2"] = null;

            Session["Ca_ID"] = null;
            Session["CandidateName"] = null;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string email = Convert.ToString(col["emailid"]);
                string password = Convert.ToString(col["password"]);
                var EmpLoggedIn = db.Contacts.Where(x => x.ct_EmailAddress == email && x.ct_Password == password && x.ct_Deleted == null).ToList();
                if (EmpLoggedIn.Count > 0)
                {
                    Session["RecruiterID"] = Convert.ToInt32(EmpLoggedIn[0].ct_Id);
                    Session["RecruiterName"] = Convert.ToString(EmpLoggedIn[0].ct_Forename);
                    Session["CompanyID"] = Convert.ToInt32(EmpLoggedIn[0].ct_co_CompanyId);

                    if (Convert.ToString(col["returnurl"]) == "")
                    {
                        return RedirectToAction("../Recruiter");
                    }
                    else
                    {
                        return RedirectToAction("../" + Convert.ToString(col["returnurl"]).Replace('-', '/'));
                    }
                }
                else
                {
                    var JobSeekerLoggedIn = db.Candidates.Where(x => x.ca_EmailAddress == email && x.ca_Password == password && x.ca_Deleted == null).ToList();

                    if (JobSeekerLoggedIn.Count > 0)
                    {

                        if (JobSeekerLoggedIn[0].ca_Active == false)
                        {
                            Session["Ca_ID"] = JobSeekerLoggedIn[0].ca_Id;

                            return RedirectToAction("../Registration");
                           
                        }

                        Session["Ca_ID"] = Convert.ToInt32(JobSeekerLoggedIn[0].ca_Id);
                        Session["CandidateName"] = Convert.ToString(JobSeekerLoggedIn[0].ca_FirstName);




                        if (Convert.ToString(col["returnurl"]) == "")
                        {
                            return RedirectToAction("../JobSeekerProfile");
                        }
                        else
                        {
                            return RedirectToAction("../" + Convert.ToString(col["returnurl"]).Replace('-', '/'));
                        }
                    }
                    else
                    {
                        if (Convert.ToString(col["returnurl"]) == "")
                        {
                            return RedirectToAction("Login/invalid");
                        }
                        else
                        {
                            return RedirectToAction("../" + Convert.ToString(col["returnurl"]).Replace('-', '/'));
                        }


                    }
                }
            }
        }

        public ActionResult Signout()
        {

            Session["RecruiterID"] = null;
            Session["RecruiterName"] = null;
            Session["CompanyID"] = null;
            Session["Recruiter1"] = null;
            Session["Recruiter2"] = null;

            Session["Ca_ID"] = null;
            Session["CandidateName"] = null;


            return View();
        }

        public ActionResult JobseekerRegistration() {
              if (Session["Ca_ID"] == null) { return RedirectToAction("RegisterJobseekerLogin"); }

            

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x=>x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Value).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Authority = ddLists.Where(x => x.lm_lt_ListTypeId == 30).OrderBy(x => x.lm_Value).ToList();

            }
            return View();
        }





        public ActionResult RecruiterRegistration()
        {

            if (Session["newcontactid"] == null)
            {
                return RedirectToAction("../RegisterEmployerLogin");
            }
            else
            {
                ViewBag.newcontactid = Convert.ToDecimal(Session["newcontactid"]);
            }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();

            }
            return View();
        }

        public ActionResult RegisterEmployerLogin()
        {

            return View();

        }

        [HttpPost]
        public ActionResult RegisterEmployerLogin(FormCollection col)
        {
            decimal newcontactid = 0;
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string Email = Convert.ToString(col["EmailID"]);
                string Password = Convert.ToString(col["Password"]);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("insert into [contacts] (ct_EmailAddress,ct_Password) values('{0}','{1}');select @@identity;", Email, Password);
                newcontactid = db.Database.SqlQuery<decimal>(sb.ToString()).SingleOrDefault();

                Session["newcontactid"] = newcontactid;

            }
            return RedirectToAction("../RecruiterRegistration");
        }




        public ActionResult RegisterJobseekerLogin() {
            dbOperations dbo = new dbOperations();
            ViewBag.Country = dbo.getlist(13);

            return View();

        }

        [HttpPost]
        public ActionResult RegisterJobseekerLogin(FormCollection col, HttpPostedFileBase uploadcpr)
        {
           
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string firstname = Convert.ToString(col["firstname"]);
                string surname = Convert.ToString(col["surname"]);
                string Email = Convert.ToString(col["EmailID"]);
                string Password = Convert.ToString(col["Password"]);
                string Phone = Convert.ToString(col["Phone"]);
                Int32 countryid = Convert.ToInt32(col["countryid"]);

                decimal Ca_ID = 0;
                decimal Cd_ID = 0;


                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(" insert into [Candidates] ([ca_FirstName],[ca_Surname],[ca_EmailAddress],[ca_Password],[ca_PhoneMobile],[ca_lm_CountryId], [ca_TotalRelavantExperience]) values('{0}', '{1}', '{2}', '{3}', '{4}',{5},0); select @@identity;", firstname, surname, Email, Password, Phone, countryid);

                Ca_ID = db.Database.SqlQuery<decimal>(sb.ToString()).SingleOrDefault();

                Session["Ca_ID"] = Ca_ID;

                // save ID
                string extension = Path.GetExtension(uploadcpr.FileName);
                extension = extension.TrimStart('.');
                string fileName = Path.GetFileNameWithoutExtension(uploadcpr.FileName);

                StringBuilder sbcd = new StringBuilder();
                sbcd.AppendFormat("insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType,cd_Type) values('{0}',{1},{2},'{3}','{4}','{5}','{6}'); select @@identity;", DateTime.Now, Ca_ID, 1, fileName, extension, "","ID");

                Cd_ID = db.Database.SqlQuery<decimal>(sbcd.ToString()).SingleOrDefault();


                
                SqlCon mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "SELECT cd_Guid from CandidateDocuments where Cd_ID=@Cd_ID";
                mycon.sqlCmd_.Parameters.AddWithValue("@Cd_ID", Cd_ID);
                mycon.sqlConOpen();
                string cd_Guid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
                cd_Guid = cd_Guid.ToUpper();
                mycon.sqlConClose();

                string folderpath = Server.MapPath("~/Documents/Identity/" + cd_Guid.Substring(0, 2) + "/" + cd_Guid.Substring(2, 2) + "/" + cd_Guid.Substring(4, 2));
                DirectoryInfo dir = new DirectoryInfo(folderpath);
                if (!dir.Exists)
                {
                    dir.Create();
                }


                uploadcpr.SaveAs(folderpath + "/" + cd_Guid + "." + extension);



            }
            return RedirectToAction("../Registration");
        }

        [HttpPost]
        public ActionResult UpdateCandidateAccount(FormCollection col)
        {
            
            string firstname = Convert.ToString(col["firstname"]);
            string surname = Convert.ToString(col["surname"]);
            int countryid = Convert.ToInt32(col["countryid"]);
            string phone = Convert.ToString(col["phone"]);
            string email = Convert.ToString(col["EmailID"]);
            string password = Convert.ToString(col["password"]);

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(@"UPDATE [dbo].[Candidates] SET [ca_LastUpdated] =getdate(),[ca_FirstName] = '{0}' 
                ,[ca_surname] = '{1}' ,[ca_lm_CountryID] = {2} ,[ca_phonemobile] = '{3}' ,[ca_emailaddress] = '{4}',[ca_password] = '{5}' where ca_Id ={6}",
                firstname, surname, countryid, phone, email, password, Convert.ToInt32(Session["Ca_ID"]));

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand(sb.ToString());
            }
            return new JsonpResult
            {

                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }




        [HttpPost]
        public ActionResult RegisterCandidate(FormCollection col, HttpPostedFileBase uploadcv)
        {
            decimal Ca_ID = Convert.ToDecimal(Session["Ca_ID"]);
           

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
               
                DateTime dateofbirth = Convert.ToDateTime(col["ca_DateOfBirth"]);
                string gender = Convert.ToString(col["ca_Gender"]);
                string ca_MaritalStatus = Convert.ToString(col["ca_MaritalStatus"]);
                Int32 ca_lm_CountryId = Convert.ToInt32(col["ca_lm_CountryId"]);
                string ca_lm_EducationLevel = Convert.ToString(col["ca_lm_EducationLevel"]);
                string ca_University = Convert.ToString(col["proUniversity"]);
                string ca_FunctionTitleID = Convert.ToString(col["ca_FunctionTitleID"]);
                string ca_CurrentJobTitle = Convert.ToString(col["ca_CurrentJobTitle"]);
                decimal ca_TotalRelavantExperience = Convert.ToDecimal(col["ca_TotalRelavantExperience"]);
                string ca_Profile = Convert.ToString(col["ca_Profile"]);

                
                

                StringBuilder sb = new StringBuilder();

                sb.AppendFormat(@"UPDATE [dbo].[Candidates] SET [ca_Created] =getdate() ,[ca_br_OriginatingBrandId] = 1  ,[ca_CurrentCountryId] = {0} 
                ,[ca_lm_EducationLevel] = {1} ,[ca_ProfileSearchable] = 1 ,[ca_CVSearchable] = 1 ,[ca_DetailsVisible] = 1,[ca_st_StatusID] = 25,[ca_Gender] = '{2}'
                ,[ca_DateOfBirth] = '{3}',[ca_MaritalStatus] = '{4}',[ca_FunctionTitleID] = {5},[ca_CurrentJobTitleID] = '{6}'
                ,[ca_TotalRelavantExperience] = {7},[ca_UniversityID] = '{8}',ca_Profile='{10}' where ca_Id ={9}", ca_lm_CountryId, ca_lm_EducationLevel, gender, dateofbirth, ca_MaritalStatus
                , ca_FunctionTitleID, ca_CurrentJobTitle, ca_TotalRelavantExperience, ca_University, Ca_ID, ca_Profile);

                string candisql = sb.ToString();

                db.Database.ExecuteSqlCommand(sb.ToString());



                if (uploadcv != null)
                {
                    // save CV
                    string extension = Path.GetExtension(uploadcv.FileName);
                    extension = extension.TrimStart('.');
                    string fileName = Path.GetFileNameWithoutExtension(uploadcv.FileName);

                    StringBuilder sbcd = new StringBuilder();
                    sbcd.AppendFormat("insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType,cd_Type) values('{0}',{1},{2},'{3}','{4}','{5}','{6}'); select @@identity;", DateTime.Now, Ca_ID, 1, fileName, extension, "", "CV");

                    decimal Cd_ID = db.Database.SqlQuery<decimal>(sbcd.ToString()).SingleOrDefault();



                    SqlCon mycon = new SqlCon();
                    mycon.sqlCmd_.CommandText = "SELECT cd_Guid from CandidateDocuments where Cd_ID=@Cd_ID";
                    mycon.sqlCmd_.Parameters.AddWithValue("@Cd_ID", Cd_ID);
                    mycon.sqlConOpen();
                    string cd_Guid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
                    cd_Guid = cd_Guid.ToUpper();
                    mycon.sqlConClose();

                    string folderpath = Server.MapPath("~/Documents/CV/" + cd_Guid.Substring(0, 2) + "/" + cd_Guid.Substring(2, 2) + "/" + cd_Guid.Substring(4, 2));
                    DirectoryInfo dir = new DirectoryInfo(folderpath);
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }


                    uploadcv.SaveAs(folderpath + "/" + cd_Guid + "." + extension);
                }
            }

            return new JsonpResult
            {

                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        /*
"insert into [Candidates] (ca_FirstName,ca_Surname,ca_DateOfBirth,ca_Gender,ca_PhoneMobile,ca_lm_CountryId,ca_EmailAddress,ca_Password,ca_CVSearchable,ca_lm_EducationLevel,ca_MaritalStatus,ca_FunctionTitleID,ca_University,ca_UniCountry,ca_ExpectedSalary, ca_TotalRelavantExperience,ca_Profile) " +
        "values('{0}','{1}','{2}','{3}','{4}',{5},'{6}','{7}',0,{8},'{9}',{10},'{11}',{12},{13},{14},'{15}');select @@identity;",
        FirstName, LastName, dateofbirth, gender, PhoneNumber, 523, Email, Password, ca_lm_EducationLevel, ca_MaritalStatus, ca_FunctionTitleID, ca_University, ca_UniCountry, ca_ExpectedSalary, ca_TotalRelavantExperience, ca_Profile);
        */
        // db.Database.ExecuteSqlCommand(sb.ToString());
        //int Ca_ID = db.Database.SqlQuery<int>("select max(ca_Id) from Candidates").SingleOrDefault();

        //string extension = Path.GetExtension(file.FileName);
        //extension = extension.TrimStart('.');
        //string fileName = Path.GetFileNameWithoutExtension(file.FileName);

        //StringBuilder sbcd = new StringBuilder();
        //sbcd.AppendFormat("insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType) values('{0}',{1},{2},'{3}','{4}','{5}')", DateTime.Now, Ca_ID, 1, fileName, extension, "");

        //db.Database.ExecuteSqlCommand(sbcd.ToString());


        //Session["Ca_ID"] = Ca_ID;

        //SqlCon mycon = new SqlCon();
        //mycon.sqlCmd_.CommandText = "SELECT cd_Guid from CandidateDocuments where cd_ca_CandidateId=@ca_Id";
        //mycon.sqlCmd_.Parameters.AddWithValue("@ca_Id", Convert.ToInt32(Session["Ca_ID"]));
        //mycon.sqlConOpen();
        //string cd_Guid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
        //cd_Guid = cd_Guid.ToUpper();
        //mycon.sqlConClose();

        //string folderpath = Server.MapPath("~/Documents/cv/" + cd_Guid.Substring(0, 2) + "/" + cd_Guid.Substring(2, 2) + "/" + cd_Guid.Substring(4, 2));
        //DirectoryInfo dir = new DirectoryInfo(folderpath);
        //if (!dir.Exists)
        //{
        //    dir.Create();
        //}


        //file.SaveAs(folderpath + "/" + cd_Guid + "." + extension);


        //dbOperations dbo = new dbOperations();
        //dbo.CandidatePhotoExt(Convert.ToInt32(Session["Ca_ID"]), extension);




        [HttpPost]
        public ActionResult RecruiterContact(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string companyname = Convert.ToString(col["CompanyName"]);
                string companynameencoded = companyname.Replace(" ", "");
                string telephone = Convert.ToString(col["Phone"]);
                string address = Convert.ToString(col["Address"]);

          

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("insert into Companies(co_Name,co_br_HomeBrandId,co_PostalAddress,co_Telephone,co_ClientType,co_st_StatusID,co_NameEncoded) values('{0}',2,'{1}','{2}',30,22,'{3}');select @@identity as companyid", companyname, address, telephone, companynameencoded);

                decimal companyid = db.Database.SqlQuery<decimal>(sb.ToString()).SingleOrDefault();

                


                string FirstName = Convert.ToString(col["FirstName"]);
                string LastName = Convert.ToString(col["LastName"]);
                decimal newcontactid = Convert.ToDecimal(col["newcontactid"]);

                sb = new StringBuilder();
                sb.AppendFormat("update Contacts set ct_Forename='{0}',ct_Surname='{1}',ct_Telephone='{2}',ct_Superuser=1,ct_st_StatusId=20,ct_co_CompanyId={3} where ct_id={4}", FirstName, LastName, telephone, companyid, newcontactid);
                db.Database.ExecuteSqlCommand(sb.ToString());


                Contact ct = db.Contacts.Where(x => x.ct_Id == newcontactid).SingleOrDefault();
                Session["RecruiterID"] = Convert.ToInt32(ct.ct_Id);
                Session["RecruiterName"] = Convert.ToString(ct.ct_Forename);
                Session["CompanyID"] = companyid;

                Session["Recruiter1"] = "YES";
            }

            return RedirectToAction("RecruiterRegistration");
        }


        
        public ActionResult getABCcompanies(string companyname)
        {

            dbOperations dbo = new dbOperations();
            List<CompanyNames> companies = dbo.abcCompanies(companyname);

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = companies.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }
        public ActionResult getWorkExperinceJobTitle(string jobtitle)
        {

            dbOperations dbo = new dbOperations();
            List<JobTitles> jobtitles = dbo.jobTitles(jobtitle);

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = jobtitles.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        [HttpPost]
        public ActionResult JobSeekerCertification(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                dbOperations dbo = new dbOperations();
                dbo.jsCertification(Convert.ToInt32(Session["Ca_ID"]), Convert.ToInt32(col["CertificationID"]), Convert.ToInt32(col["AuthorityID"]), Convert.ToInt32(col["FromMonth"]), Convert.ToInt32(col["FromYear"]), Convert.ToInt32(col["ToMonth"]), Convert.ToInt32(col["ToYear"]), Convert.ToBoolean(col["DoNotExpire"]));

            }
            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        [HttpPost]
        public ActionResult JobSeekerHonours(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                dbOperations dbo = new dbOperations();
                dbo.jsHonours(Convert.ToInt32(col["Ca_Id"]), Convert.ToString(col["Title"]), Convert.ToString(col["Associatedwith"]), Convert.ToString(col["Issuer"]), Convert.ToString(col["Issuedon"]), Convert.ToString(col["Description"]));
            }
            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        [HttpPost]
        public ActionResult JobSeekerLanguage(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                dbOperations dbo = new dbOperations();
                dbo.jsLanguage(Convert.ToInt32(Session["Ca_Id"]), Convert.ToString(col["LanguageID"]), Convert.ToString(col["ProficiencyID"]));

            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        [HttpPost]
        public ActionResult JobSeekerWorkHistory(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                dbOperations dbo = new dbOperations();
                dbo.jsWorkHistory(Convert.ToInt32(Session["Ca_ID"]), Convert.ToInt32(col["JobTitleID"]), Convert.ToString(col["Company"]), Convert.ToInt32(col["JobLevelID"]), Convert.ToInt32(col["ReportingToID"]), Convert.ToInt32(col["IndustryID"]), Convert.ToInt32(col["FunctionID"]), Convert.ToInt32(col["FromMonth"]), Convert.ToInt32(col["FromYear"]), Convert.ToInt32(col["ToMonth"]), Convert.ToInt32(col["ToYear"]), Convert.ToBoolean(col["CurrentlyWorkingHere"]), Convert.ToInt32(col["JobLocationID"]), Convert.ToInt32(col["SalaryID"]), Convert.ToString(col["Description"]));

            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        [HttpPost]
        public ActionResult JobSeekerEducation(FormCollection col, HttpPostedFileBase uploadmedia)
        {
            int Ca_ID = Convert.ToInt32(Session["Ca_ID"]);
            

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                dbOperations dbo = new dbOperations();
                dbo.jsEducation(Ca_ID, Convert.ToString(col["eduSchool"]),  Convert.ToString(col["eduSchoolcountry"]),  Convert.ToString(col["eduDegree"]),  Convert.ToString(col["eduField"]),  Convert.ToString(col["eduGrade"]),  Convert.ToString(col["edufrommonth"]),  Convert.ToString(col["edufromyear"]),  Convert.ToString(col["edutomonth"]),  Convert.ToString(col["edutoyear"]), Convert.ToBoolean(col["educurrentlystudy"]),  Convert.ToString(col["eduActivities"]));


                if (uploadmedia != null)
                {

                

                               // save Media
                    string extension = Path.GetExtension(uploadmedia.FileName);
                    extension = extension.TrimStart('.');
                    string fileName = Path.GetFileNameWithoutExtension(uploadmedia.FileName);

                    StringBuilder sbcd = new StringBuilder();
                    sbcd.AppendFormat("insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType,cd_Type) values('{0}',{1},{2},'{3}','{4}','{5}','{6}'); select @@identity;", DateTime.Now, Ca_ID, 1, fileName, extension, "","MEDIA");

                    decimal Cd_ID = db.Database.SqlQuery<decimal>(sbcd.ToString()).SingleOrDefault();



                    SqlCon mycon = new SqlCon();
                    mycon.sqlCmd_.CommandText = "SELECT cd_Guid from CandidateDocuments where Cd_ID=@Cd_ID";
                    mycon.sqlCmd_.Parameters.AddWithValue("@Cd_ID", Cd_ID);
                    mycon.sqlConOpen();
                    string cd_Guid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
                    cd_Guid = cd_Guid.ToUpper();
                    mycon.sqlConClose();

                    string folderpath = Server.MapPath("~/Documents/Media/" + cd_Guid.Substring(0, 2) + "/" + cd_Guid.Substring(2, 2) + "/" + cd_Guid.Substring(4, 2));
                    DirectoryInfo dir = new DirectoryInfo(folderpath);
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }


                    uploadmedia.SaveAs(folderpath + "/" + cd_Guid + "." + extension);
                }
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }




        [HttpPost]
        public ActionResult JobseekerSkills(FormCollection col)
        {
            
                dbOperations dbo = new dbOperations();
                dbo.jsSkills(Convert.ToInt32(Session["Ca_ID"]), Convert.ToInt32(col["SkillID"]), Convert.ToInt32(col["SkillLevelID"]));

            

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult getJobSeekerSkills(decimal Ca_ID)
        {

            dbOperations dbo = new dbOperations();
            List<CandidateSkills> cask = dbo.getCandidateSkills(Ca_ID);



            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = cask.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }



        public ActionResult getJobSeekerLanguages(decimal Ca_ID)
        {

            dbOperations dbo = new dbOperations();
            List<CandidateLanguages> cask = dbo.getCandidateLanguages(Ca_ID);



            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = cask.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        [HttpPost]
        public ActionResult JobseekerCareerMove(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                dbOperations dbo = new dbOperations();
                dbo.jsCareerMove(Convert.ToInt32(Session["Ca_ID"]), Convert.ToInt32(col["JobTitleID"]), Convert.ToInt32(col["FunctionID"]), Convert.ToInt32(col["IndustryID"]), Convert.ToInt32(col["ExperienceID"]), Convert.ToInt32(col["ExpectedSalaryID"]));
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }




        public ActionResult CandidateActivate()
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update Candidates set ca_active=1 where ca_id=" + Session["Ca_ID"]);
            }

                return RedirectToAction("../Login");
        }

        [HttpPost]
        public ActionResult RecruiterSectors2(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                string sectorsvv = Convert.ToString(col["checkeditems"]);
                string[] eachsector = sectorsvv.Split(',');

                dbOperations dbo = new dbOperations();

                foreach (var sec in eachsector)
                {
                    dbo.CompanySectors(Convert.ToInt32(Session["CompanyID"]), Convert.ToInt32(sec));
                }

                
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        [HttpPost]
        public ActionResult RecruiterLogo(FormCollection col, HttpPostedFileBase file)
        {

            SqlCon mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "SELECT co_guid from Companies where co_id=@co_id";
            mycon.sqlCmd_.Parameters.AddWithValue("@co_id", Convert.ToInt32(Session["CompanyID"]));
            mycon.sqlConOpen();
            string coguid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
            coguid = coguid.ToUpper();
            mycon.sqlConClose();

            string folderpath = Server.MapPath("~/Documents/Logos/" + coguid.Substring(0, 2) + "/" + coguid.Substring(2, 2) + "/" + coguid.Substring(4, 2));
            DirectoryInfo dir = new DirectoryInfo(folderpath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            string extension = Path.GetExtension(file.FileName);

            file.SaveAs(folderpath + "/" + coguid + "-original" + extension);


            dbOperations dbo = new dbOperations();
            dbo.CompanyLogoExt(Convert.ToInt32(Session["CompanyID"]), extension);


            if (extension == ".jpg" || extension == ".jpeg")
            {
              
                resizeImage(folderpath + "/" + coguid + "-original" + extension, 300);
                resizeImage(folderpath + "/" + coguid + "-original" + extension, 70);
            }
            else
            {
                file.SaveAs(folderpath + "/" + coguid + extension);
            }

            return RedirectToAction("Recruiter");
        }

        public ActionResult PaymentSuccess() {


            return View();
        }

        private void resizeImage(string docid, int thumbWidth)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile((docid));

            int srcWidth = image.Width;
            int srcHeight = image.Height;
            double tHeight = Convert.ToDouble(srcWidth) / Convert.ToDouble(srcHeight);
            int thumbHeight = Convert.ToInt32(thumbWidth / tHeight);
            Bitmap bmp = new Bitmap(image, thumbWidth, thumbHeight);

            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bmp);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            System.Drawing.Rectangle rectDestination = new System.Drawing.Rectangle(0, 0, thumbWidth, thumbHeight);
            gr.DrawImage(image, rectDestination, 0, 0, srcWidth, srcHeight, GraphicsUnit.Pixel);
            if (thumbWidth == 300)
            {
                bmp.Save(docid.Replace("-original", ""));
                bmp.Dispose();
                image.Dispose();

            }
            else if (thumbWidth == 70)
            {
                bmp.Save(docid.Replace("-original", "-small"));
                bmp.Dispose();
                image.Dispose();

            }


        }

        [HttpPost]
        public ActionResult CandidatePhoto(FormCollection col, HttpPostedFileBase file)
        {

            SqlCon mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "SELECT ca_GUID from Candidates where ca_Id=@ca_Id";
            mycon.sqlCmd_.Parameters.AddWithValue("@ca_Id", Convert.ToInt32(Session["Ca_ID"]));
            mycon.sqlConOpen();
            string ca_GUID = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
            ca_GUID = ca_GUID.ToUpper();
            mycon.sqlConClose();

            string folderpath = Server.MapPath("~/Documents/photos/" + ca_GUID.Substring(0, 2) + "/" + ca_GUID.Substring(2, 2) + "/" + ca_GUID.Substring(4, 2));
            DirectoryInfo dir = new DirectoryInfo(folderpath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            string extension = Path.GetExtension(file.FileName);
            extension = extension.TrimStart('.');
            file.SaveAs(folderpath + "/" + ca_GUID + "." + extension);


            dbOperations dbo = new dbOperations();
            dbo.CandidatePhotoExt(Convert.ToInt32(Session["Ca_ID"]), extension);


            //if (extension == "jpg" || extension == "jpeg")
            //{

            //    resizeImage(folderpath + "/" + ca_GUID +  extension, 300);
            //    resizeImage(folderpath + "/" + ca_GUID +  extension, 70);
            //}

            return RedirectToAction("JobSeekerProfile");
        }

        //public ActionResult JobSearch()
        //{

        //    if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

        //    int candidateid = Convert.ToInt32(Session["Ca_ID"]);
        //    ViewBag.Candidateid = candidateid;

        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {

        //        var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
        //        if (candidate.Count == 0)
        //        {
        //            candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
        //        }

        //        if (candidate[0].ca_HasPhoto == true)
        //        {
        //            ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

        //        }

        //        ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
        //        ViewBag.fName = candidate[0].ca_FirstName;
        //        ViewBag.lName = candidate[0].ca_Surname;
        //        ViewBag.Phone = candidate[0].ca_PhoneMobile;
        //        ViewBag.Email = candidate[0].ca_EmailAddress;
        //        ViewBag.DOB = Convert.ToDateTime(candidate[0].ca_DateOfBirth).ToString("dd-MM-yyyy");
        //        var selectedGender = candidate[0].ca_Gender;
        //        ViewBag.Genders = new SelectList(new List<SelectListItem> { new SelectListItem { Text = "Male", Value = "M" }, new SelectListItem { Text = "Female", Value = "F" }, }, "Value", "Text", selectedGender);





        //        ViewBag.CandiateProfile = candidate.ToList();

        //        var candidatedocument = db.CandidateDocuments.Where(x => x.cd_ca_CandidateId == candidateid).ToList();
        //        if (candidatedocument.Count > 0)
        //        {

        //            ViewBag.CVpath = "http://www.jobs4bahrainis.com/cvdx/" + candidatedocument[0].cd_Guid.ToString().Substring(0, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(2, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(4, 2) + "/" + candidatedocument[0].cd_Guid.ToString() + "." + candidatedocument[0].cd_FileExtension;
        //        }


        //        ViewBag.Education = db.Database.SqlQuery<CandidateEducation>("SELECT * from [dbo].[CandidateEducation]  where ca_id=" + candidateid).ToList();
        //        ViewBag.Experience = db.Database.SqlQuery<CandidateExperience>("SELECT * from [dbo].[CandidateExperience]  where ca_id=" + candidateid).ToList();
        //        ViewBag.Skills = db.Database.SqlQuery<CandidateSkill>("SELECT * from [dbo].[CandidateSkills]  where ca_id=" + candidateid).ToList();
        //        ViewBag.Skill_level = new SelectList(new List<SelectListItem> { new SelectListItem { Text = "Beginner", Value = "Beginner" }, new SelectListItem { Text = "Intermediate", Value = "Intermediate" }, new SelectListItem { Text = "Expert", Value = "Expert" }, new SelectListItem { Text = "Advanced", Value = "Advanced" } }, "Value", "Text");

        //        var Interests = db.Database.SqlQuery<ListsMaster>("select * from listsmaster where lm_id in(select ci_lm_linkid from CandidateInterests where ci_ca_candidateid=" + candidateid + ") and lm_lt_listtypeid=4").ToList();

        //        string intrst = "";
        //        foreach (var i in Interests)
        //        {
        //            intrst += i.lm_Value + " , ";
        //        }
        //        if (Interests.Count > 0)
        //        {
        //            ViewBag.Interests = intrst.Remove(intrst.Length - 2);
        //        }

        //        ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();

        //    }



        //    return View();
        //}

        public ActionResult JobSearch()
        {

            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["Ca_ID"]);
            ViewBag.Candidateid = candidateid;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                if (candidate.Count == 0)
                {
                    candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                }

                if (candidate[0].ca_HasPhoto == true)
                {
                    ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

                }

                ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
                ViewBag.fName = candidate[0].ca_FirstName;
                ViewBag.lName = candidate[0].ca_Surname;
                ViewBag.Phone = candidate[0].ca_PhoneMobile;
                ViewBag.Email = candidate[0].ca_EmailAddress;
                ViewBag.DOB = Convert.ToDateTime(candidate[0].ca_DateOfBirth).ToString("dd-MM-yyyy");
                var selectedGender = candidate[0].ca_Gender;
                ViewBag.Genders = new SelectList(new List<SelectListItem> { new SelectListItem { Text = "Male", Value = "M" }, new SelectListItem { Text = "Female", Value = "F" }, }, "Value", "Text", selectedGender);





                ViewBag.CandiateProfile = candidate.ToList();

                var candidatedocument = db.CandidateDocuments.Where(x => x.cd_ca_CandidateId == candidateid).ToList();
                if (candidatedocument.Count > 0)
                {

                    ViewBag.CVpath = "http://www.jobs4bahrainis.com/cvdx/" + candidatedocument[0].cd_Guid.ToString().Substring(0, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(2, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(4, 2) + "/" + candidatedocument[0].cd_Guid.ToString() + "." + candidatedocument[0].cd_FileExtension;
                }


                ViewBag.Education = db.Database.SqlQuery<CandidateEducation>("SELECT * from [dbo].[CandidateEducation]  where ca_id=" + candidateid).ToList();
                ViewBag.Experience = db.Database.SqlQuery<CandidateExperience>("SELECT * from [dbo].[CandidateExperience]  where ca_id=" + candidateid).ToList();
                ViewBag.Skills = db.Database.SqlQuery<CandidateSkill>("SELECT * from [dbo].[CandidateSkills]  where ca_id=" + candidateid).ToList();
                ViewBag.Skill_level = new SelectList(new List<SelectListItem> { new SelectListItem { Text = "Beginner", Value = "Beginner" }, new SelectListItem { Text = "Intermediate", Value = "Intermediate" }, new SelectListItem { Text = "Expert", Value = "Expert" }, new SelectListItem { Text = "Advanced", Value = "Advanced" } }, "Value", "Text");

                var Interests = db.Database.SqlQuery<ListsMaster>("select * from listsmaster where lm_id in(select ci_lm_linkid from CandidateInterests where ci_ca_candidateid=" + candidateid + ") and lm_lt_listtypeid=4").ToList();

                string intrst = "";
                foreach (var i in Interests)
                {
                    intrst += i.lm_Value + " , ";
                }
                if (Interests.Count > 0)
                {
                    ViewBag.Interests = intrst.Remove(intrst.Length - 2);
                }

                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();

            }



            return View();
        }
        public ActionResult JobSearchResult(string keyword, string[] chksector, string salary, int? qualification, string experience, int? page)
        {
            decimal? salaryFrom = null, salaryTo = null;
            var spltSalary = salary != null ? salary.Split(':') : null;
            if (spltSalary != null && spltSalary.Count() == 2)
            {
                salaryFrom = decimal.Parse(spltSalary[0]);
                salaryTo = decimal.Parse(spltSalary[1]);
            }

            decimal? expFrom = null, expTo = null;
            var spltExp = experience != null ? experience.Split(':') : null;
            if (spltExp != null && spltExp.Count() == 2)
            {
                expFrom = int.Parse(spltExp[0]);
                expTo = int.Parse(spltExp[1]);
            }

            int totalSearchCount = 0;
            var pageIndex = (page ?? 1); //set pageIndex to 1 when null
            var pageSize = 10; // max record display per page
            List<JobSearchResultModel> jobSearchResult = null;
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                SqlParameter qualififcationParam = AddParameter("p_qaulification", qualification, typeof(Int32));
                SqlParameter salaryFromParam = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
                SqlParameter salaryToParam = AddParameter("p_salaryto", salaryTo, typeof(decimal));
                SqlParameter expFromParam = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
                SqlParameter expToParam = AddParameter("p_experienceTo", expTo, typeof(Int32));
                SqlParameter keywordParam = AddParameter("p_keyword", keyword, typeof(String));
                SqlParameter searchTypeParam = AddParameter("p_searchType", 1, typeof(Int32));
                SqlParameter pageSizeParam = AddParameter("p_pageSize", pageSize, typeof(Int32));
                SqlParameter pageIndexParam = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

                object[] parameters = new object[] { qualififcationParam
                                                    , salaryFromParam, salaryToParam, expFromParam,expToParam
                                                    ,keywordParam,searchTypeParam,pageSizeParam,pageIndexParam};

                string sqlQry = @"proc_JobSearch @p_qaulification,@p_salaryfrom,@p_salaryto,@p_experienceFrom,@p_experienceTo,@p_keyword,@p_searchType,
                                  @p_pageSize,@p_pageIndex";

                jobSearchResult = db.Database.SqlQuery<JobSearchResultModel>(sqlQry, parameters).ToList();

                SqlParameter qualififcationParam2 = AddParameter("p_qaulification", qualification, typeof(Int32));
                SqlParameter salaryFromParam2 = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
                SqlParameter salaryToParam2 = AddParameter("p_salaryto", salaryTo, typeof(decimal));
                SqlParameter expFromParam2 = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
                SqlParameter expToParam2 = AddParameter("p_experienceTo", expTo, typeof(Int32));
                SqlParameter keywordParam2 = AddParameter("p_keyword", keyword, typeof(String));
                SqlParameter searchTypeParam2 = AddParameter("p_searchType", 2, typeof(Int32));
                SqlParameter pageSizeParam2 = AddParameter("p_pageSize", pageSize, typeof(Int32));
                SqlParameter pageIndexParam2 = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

                parameters = new object[] { qualififcationParam2
                                                    , salaryFromParam2, salaryToParam2, expFromParam2,expToParam2
                                                    ,keywordParam2,searchTypeParam2,pageSizeParam2,pageIndexParam2};
                totalSearchCount = db.Database.SqlQuery<int>(sqlQry, parameters).FirstOrDefault();

            }
            // this is just to test with mock data
            //for (int i = 0; i < 100; i++)
            //{
            //    jobSearchResult.Add(jobSearchResult[0]);
            //}
            //totalSearchCount = jobSearchResult.Count;
            //var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
            //var candidateResultFilter = jobSearchResult.ToPagedList(pageNumber, 10); // will only contain 25 products max because of the pageSize


            var candidateResultFilter = new StaticPagedList<JobSearchResultModel>(jobSearchResult, pageIndex, pageSize, totalSearchCount);

            ViewBag.lstjobsectorcount = getJobsectorscount();

            return View(candidateResultFilter);
        }

        //[HttpPost]
        //public ActionResult JobSearchResult(JobSearchModel model)
        //{
        //    decimal? salaryFrom = null, salaryTo = null;
        //    var spltSalary = model.salary != null ? model.salary.Split(':') : null;
        //    if (spltSalary != null && spltSalary.Count() == 2)
        //    {
        //        salaryFrom = decimal.Parse(spltSalary[0]);
        //        salaryTo = decimal.Parse(spltSalary[1]);
        //    }

        //    decimal? expFrom = null, expTo = null;
        //    var spltExp = model.experience != null ? model.experience.Split(':') : null;
        //    if (spltExp != null && spltExp.Count() == 2)
        //    {
        //        expFrom = int.Parse(spltExp[0]);
        //        expTo = int.Parse(spltExp[1]);
        //    }
        //    List<JobSearchResultModel> jobSearchResult = null;
        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {

        //        SqlParameter qualififcationParam = AddParameter("p_qaulification", model.qualification, typeof(Int32));
        //        SqlParameter salaryFromParam = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
        //        SqlParameter salaryToParam = AddParameter("p_salaryto", salaryTo, typeof(decimal));
        //        SqlParameter expFromParam = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
        //        SqlParameter expToParam = AddParameter("p_experienceTo", expTo, typeof(Int32));
        //        SqlParameter keywordParam = AddParameter("p_keyword", model.keyword, typeof(String));


        //        object[] parameters = new object[] { qualififcationParam
        //                                            , salaryFromParam, salaryToParam, expFromParam,expToParam
        //                                            ,keywordParam};
        //        string sqlQry = "proc_JobSearch @p_qaulification,@p_salaryfrom,@p_salaryto,@p_experienceFrom,@p_experienceTo,@p_keyword";
        //        jobSearchResult = db.Database.SqlQuery<JobSearchResultModel>(sqlQry, parameters).ToList();

        //        //int cnt = 0;
        //        //foreach (var d in candidateResult)
        //        //{
        //        //    SqlParameter ca_id_Param = AddParameter("ca_id", d.ca_Id, typeof(Int32));
        //        //    parameters = new object[] { ca_id_Param };
        //        //    sqlQry = "proc_CandidateSearchInterest @ca_id";
        //        //    var ca_interest = db.Database.SqlQuery<CandidateSearchInerestResultModel>(sqlQry, parameters).FirstOrDefault();
        //        //    d.Interest = ca_interest.Interest;
        //        //    candidateResult[cnt].Interest = d.Interest;
        //        //    cnt++;
        //        //}
        //    }
        //    return View(jobSearchResult);
        //}


        public ActionResult Packages()
        {

            return View();
        }




        public ActionResult Recruiter()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? ".jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;
                ViewBag.Company = company[0].co_Name;
                ViewBag.CompanyPhone = company[0].co_Telephone;
                ViewBag.PostalAddress = company[0].co_PostalAddress;
                ViewBag.LastUpdated = company[0].co_LastUpdated;

                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;
                ViewBag.CompanyProfile = company[0].co_Profile;


     

                ViewBag.lstjobsectorcount = getJobsectorscount();

                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(@"select  vc_Id,co_id,vc_Title,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo from Vacancies 
inner join Companies on vc_co_CompanyId=co_Id where vc_co_CompanyId=" + companyid + " and vc_Deleted is null order by vc_id desc").ToList();

                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");
                    try
                    {
                        ljx.vc_Description = fparamaker(lj.vc_Description, 9);
                    }
                    catch
                    {
                        ljx.vc_Description = lj.vc_Description;
                    }
                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    extns = lj.co_LogoExtension == "" ? "jpg" : lj.co_LogoExtension;

                    ljx.LogoURL = "http://www.jobs4bahrainis.com/Logox/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "." + extns;


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days > 200)
                    {
                        days = days - 200;
                    }
                    else if (days > 40)
                    {
                        days = days - 40;
                    }

                    ljx.postedsince = "" + days;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.Take(20).ToList();



                List<JobSeekers> jobseekers = db.Database.SqlQuery<JobSeekers>("select ca_Id,ca_GUID, ca_FirstName,ca_Surname,ca_EmailAddress,ca_PhoneMobile,ca_DateOfBirth,ca_Gender,ca_photoextension from candidates where ca_id in (select top 20 app_ca_CandidateId from Applications where app_vc_VacancyId in (select vc_id from Vacancies where vc_co_CompanyId=" + companyid + ") order by app_id desc)").ToList();

                List<JobSeekers> jos = new List<JobSeekers>();

                JobSeekers jobskr;

                foreach (var js in jobseekers)
                {
                    jobskr = new JobSeekers();

                    jobskr.ca_FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(js.ca_FirstName.ToLower());
                    jobskr.ca_Surname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(js.ca_Surname.ToLower());
                    jobskr.ca_EmailAddress = js.ca_EmailAddress.ToLower();
                    jobskr.ca_PhoneMobile = js.ca_PhoneMobile.Replace("00973", "").Replace("+973", "").Replace(" ", "");
                    jobskr.ca_DateOfBirth = js.ca_DateOfBirth;
                    jobskr.Age = DateTime.Now.Year - js.ca_DateOfBirth.Value.Year;
                    jobskr.ca_Gender = js.ca_Gender == "M" ? "Male" : "Female";
                    jobskr.ca_photoextension = js.ca_photoextension;
                    jobskr.ca_GUID = js.ca_GUID;
                    string extn = js.ca_photoextension == "" ? "jpg" : js.ca_photoextension;
                    jobskr.ProfilePic = "http://www.jobs4bahrainis.com/documents/photos/" + js.ca_GUID.ToString().Substring(0, 2) + "/" + js.ca_GUID.ToString().Substring(2, 2) + "/" + js.ca_GUID.ToString().Substring(4, 2) + "/" + js.ca_GUID.ToString() + "." + extn;
                    jos.Add(jobskr);
                }
                ViewBag.jobseekers = jos.ToList();

                StringBuilder stb = new StringBuilder();

                stb.AppendFormat(@"select count(vc_id) as total from vacancies where  vc_co_companyid={0}
union all
select count(vc_id) as total from vacancies where  vc_co_companyid={0} and vc_ExpiryDate>=GETDATE()
union all
select count(vc_id) as total from vacancies where  vc_co_companyid={0} and vc_ExpiryDate<GETDATE()
union all
select count(vc_id) as total from vacancies where  vc_co_companyid={0} and vc_Deleted is null
union all
select count(vc_id) as total from vacancies where  vc_co_companyid={0} and vc_Deleted is not null
union all
select count(app_id) as total from Applications where app_vc_VacancyId in (select vc_id from Vacancies where vc_co_CompanyId={0})
", companyid);

                ViewBag.Statistics = db.Database.SqlQuery<int>(stb.ToString()).ToList();

            }

            return View();
        }

        public ActionResult JobSeeker()
        {

            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["Ca_ID"]);

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var candidate = db.Candidates.Where(x => x.ca_Id == candidateid).ToList();

                ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_PhotoExtension;

                ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
                ViewBag.Phone = candidate[0].ca_PhoneMobile;
                ViewBag.Email = candidate[0].ca_EmailAddress;



                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(@"select top 12 vc_Id,co_id,vc_Title,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo from Vacancies 
inner join Companies on vc_co_CompanyId=co_Id where vc_Id in
 (select top 50 max(vc_Id) as vc_id from Vacancies inner join Companies on vc_co_CompanyId=co_Id
  where vc_Deleted is null  and vc_br_SourceBrandId=2     group by co_id order by vc_id desc)  and vc_id not in (38193,38201,38221,38195,38188)  order by vc_id desc
 ").ToList();

                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");
                    try
                    {
                        ljx.vc_Description = fparamaker(lj.vc_Description, 9);
                    }
                    catch
                    {
                        ljx.vc_Description = lj.vc_Description;
                    }
                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.LogoURL = "http://www.jobs4bahrainis.com/Logox/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + ".jpg";


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days > 200)
                    {
                        days = days - 200;
                    }
                    else if (days > 40)
                    {
                        days = days - 40;
                    }
                    ljx.jobURL = urlcleaner(lj.vc_Title);

                    ljx.postedsince = "" + days;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToList();



                ViewBag.lstjobsectorcount = getJobsectorscount();

            }
            

            return View();
        }


        public ActionResult JobSeekerProfile()
        {

            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["Ca_ID"]);
            ViewBag.Candidateid = candidateid;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                

                var candidate = db.Database.SqlQuery<CandidateNew>(@"select c.ca_lm_countryid,ca_lm_EducationLevel,ca_UniversityID,ca_FunctionTitleID,ca_CurrentJobTitleID,ca_TotalRelavantExperience,c.Ca_CurrentCountryID, c.ca_FirstName,c.ca_Surname,lmcona.lm_Value as Nationality, ca_PhoneMobile,ca_EmailAddress,ca_Password, c.ca_DateOfBirth, c.ca_Gender, c.ca_MaritalStatus,
lmcurcon.lm_Value as CurrentLocation,lmedu.lm_Value as HighestEducation,lmuni.lm_Value as University, 
lmfun.lm_Value as FunctionTitle, lmjt.lm_Value as JobTitle, lmexp.lm_Value as TotalExperience, c.ca_Profile
from Candidates c
join ListsMaster lmcurcon on lmcurcon.lm_Id = c.ca_CurrentCountryID
join ListsMaster lmedu on lmedu.lm_Id = c.ca_lm_EducationLevel
join ListsMaster lmuni on lmuni.lm_Id = c.ca_UniversityID
join ListsMaster lmfun on lmfun.lm_Id = c.ca_FunctionTitleID
join ListsMaster lmjt on lmjt.lm_Id = c.ca_CurrentJobTitleID
join ListsMaster lmexp on lmexp.lm_Id = c.ca_TotalRelavantExperience
join ListsMaster lmcona on lmcona.lm_Id = c.ca_lm_CountryId
where c.ca_id=" + candidateid).ToList();
         

                if (candidate[0].ca_HasPhoto == true)
                {
                    ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;
                   
                }
                 
                ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
                ViewBag.fName = candidate[0].ca_FirstName;
                ViewBag.lName = candidate[0].ca_Surname;
                ViewBag.Phone = candidate[0].ca_PhoneMobile;
                ViewBag.Email = candidate[0].ca_EmailAddress;
                ViewBag.MaritalStatus = candidate[0].ca_MaritalStatus;

                ViewBag.DOB = Convert.ToDateTime(candidate[0].ca_DateOfBirth).ToString("yyyy/MM/dd");
                var selectedGender = candidate[0].ca_Gender;
                ViewBag.Gender = candidate[0].ca_Gender;
                ViewBag.Nationality = candidate[0].Nationality;
                ViewBag.ca_lm_countryid = candidate[0].ca_lm_countryid;
                ViewBag.ca_Gender = candidate[0].ca_Gender;
                ViewBag.ca_MaritalStatus = candidate[0].ca_MaritalStatus;
                ViewBag.CurrentLocation = candidate[0].CurrentLocation;
                ViewBag.Ca_CurrentCountryID = candidate[0].Ca_CurrentCountryID;
                ViewBag.HighestEducation = candidate[0].HighestEducation;
                ViewBag.ca_lm_EducationLevel = candidate[0].ca_lm_EducationLevel;
                ViewBag.ca_UniversityID = candidate[0].ca_UniversityID;
                ViewBag.ca_FunctionTitleID = candidate[0].ca_FunctionTitleID;
                ViewBag.ca_CurrentJobTitleID = candidate[0].ca_CurrentJobTitleID;
                ViewBag.ca_TotalRelavantExperience = candidate[0].ca_TotalRelavantExperience;









                ViewBag.University2 = candidate[0].University;
                ViewBag.FunctionTitle = candidate[0].FunctionTitle;
                ViewBag.CurrentJobTitle = candidate[0].JobTitle;
                ViewBag.TotalExperience = candidate[0].TotalExperience;
                ViewBag.ca_Profile = candidate[0].ca_Profile;


                //ViewBag.Genders = new SelectList(new List<SelectListItem> { new SelectListItem { Text = "Male", Value = "M" }, new SelectListItem { Text = "Female", Value = "F" }, }, "Value", "Text", selectedGender);





                ViewBag.CandiateProfile = candidate.ToList();

                var candidatedocument = db.CandidateDocuments.Where(x => x.cd_ca_CandidateId == candidateid).ToList();
                if (candidatedocument.Count > 0)
                {

                    ViewBag.CVpath = "http://www.jobs4bahrainis.com/cvdx/" + candidatedocument[0].cd_Guid.ToString().Substring(0, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(2, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(4, 2) + "/" + candidatedocument[0].cd_Guid.ToString() + "." + candidatedocument[0].cd_FileExtension;
                }


                ViewBag.Education = db.Database.SqlQuery<CandidateEducationNew>(@"select ce.CaEdu_ID,ce.Ca_ID,lmu.lm_Value as University,lmcon.lm_Value as UniCountry, lmDeg.lm_Value as Degree, lmSpez.lm_Value as Specialization, lmgr.lm_Value as Grade, ce.FromMonth,ce.FromYear, ce.ToMonth,ce.ToYear, ce.CurrentlyStudyHere,ce.Activities  from [CandidateEducation_T] ce 
                join ListsMaster lmu on lmu.lm_Id= ce.UniversityID
                join ListsMaster lmCon on lmCon.lm_Id= ce.UniCountryID
                join ListsMaster lmDeg on lmDeg.lm_Id= ce.DegreeID
                join ListsMaster lmSpez on lmSpez.lm_Id= ce.SpecializationID
                join ListsMaster lmGr on lmGr.lm_Id= ce.GradeID
                where ca_id=" + candidateid).ToList();


                ViewBag.Experience2 = db.Database.SqlQuery<CandidateWorkHistory>(@"select cwh.CaWorkHistory_ID,cwh.Ca_ID,lmjt.lm_Value as JobTitle, lmjl.lm_Value as JobLevel, cwh.Company, lmrt.lm_Value as ReportingTo, lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction, 
                cwh.FromMonth,cwh.FromYear,cwh.ToMonth,cwh.ToYear,cwh.CurrentlyWorkingHere,lmjlo.lm_Value as JobLocation, lmsal.lm_Value as Salary,cwh.[Description] from [CandidateWorkHistory_T] cwh 
                join ListsMaster lmjt on lmjt.lm_Id = cwh.JobTitleID
                join ListsMaster lmjl on lmjl.lm_Id = cwh.JobLevelID
                join ListsMaster lmrt on lmrt.lm_Id = cwh.ReportingToID
                join ListsMaster lmind on lmind.lm_Id = cwh.IndustryID
                join ListsMaster lmfn on lmfn.lm_Id = cwh.FunctionID
                join ListsMaster lmjlo on lmjlo.lm_Id = cwh.JobLocationID
                join ListsMaster lmsal on lmsal.lm_Id = cwh.SalaryID
                where ca_id=" + candidateid).ToList();


                ViewBag.CandidateCertifications = db.Database.SqlQuery<CandidateCertifications>(@"select cc.CaCertification_ID,cc.Ca_ID,lmcer.lm_Value as Certification, lmau.lm_Value as Authority, cc.FromMonth,cc.FromYear,cc.ToMonth,cc.ToYear,cc.DoNotExpire from [CandidateCertification_T] cc
                join ListsMaster lmcer on lmcer.lm_Id = cc.CertificationID
                join ListsMaster lmau on lmau.lm_Id = cc.AuthorityID
                where ca_id=" + candidateid).ToList();
    

                ViewBag.IdealCareerMove = db.Database.SqlQuery<IdealCareeMove>(@"select icm.CaICM_ID,icm.Ca_ID, lmjt.lm_Value as JobTitle,lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction,lmexp.lm_Value as Experience, lmsal.lm_Value as ExpectedSalary from [CandidateIdealCareerMove_T] icm
                join ListsMaster lmjt on lmjt.lm_Id = icm.JobTitleID
                join ListsMaster lmfn on lmfn.lm_Id = icm.FunctionID
                join ListsMaster lmind on lmind.lm_Id = icm.IndustryID
                join ListsMaster lmexp on lmexp.lm_Id = icm.ExperienceID
                join ListsMaster lmsal on lmsal.lm_Id = icm.ExpectedSalaryID
                where ca_id=" + candidateid).ToList();

                 
           
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Value).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Authority = ddLists.Where(x => x.lm_lt_ListTypeId == 30).OrderBy(x => x.lm_Value).ToList();

            }



            return View();
        }



 

        [HttpPost]
        public ActionResult AddCandidateSkill2(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                string ca_id = Convert.ToString(col["Ca_ID"]);
                string skills = Convert.ToString(col["Skills"]);
                string skilllevel = Convert.ToString(col["SkillLevel"]);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("insert into [CandidateSkills] (Ca_ID,Skills,SkillLevel) values({0},'{1}','{2}')", Convert.ToInt32(ca_id), Convert.ToString(skills), Convert.ToString(skilllevel));
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }




        public ActionResult DeleteCandidateLanguage(int Caln_ID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Delete from [CandidateLanguage_T] where Ca_ID={0} and CaLanguage_ID={1}", Convert.ToInt32(Session["Ca_ID"]), Caln_ID);
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }



        public ActionResult DeleteCandidateSkill(int CaSkillID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from [CandidateSkill_T] where Ca_ID={0} and caskill_id={1}", Convert.ToInt32(Session["Ca_ID"]), CaSkillID);
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

 

        public ActionResult DeleteEDU(int CaEduID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from [CandidateEducation_T] where Ca_ID={0} and CaEdu_ID={1}", Convert.ToInt32(Session["Ca_ID"]), CaEduID);
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult DeleteWH(int caWHid)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from [CandidateWorkHistory_T] where Ca_ID={0} and CaWorkHistory_ID={1}", Convert.ToInt32(Session["Ca_ID"]), caWHid);
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }
        public ActionResult DeleteCER(int caCERid)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from [CandidateCertification_T] where Ca_ID={0} and CaCertification_ID={1}", Convert.ToInt32(Session["Ca_ID"]), caCERid);
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult DeleteICM(int caICMid)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from [CandidateIdealCareerMove_T] where Ca_ID={0} and caICM_id={1}", Convert.ToInt32(Session["Ca_ID"]), caICMid);
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        [HttpPost]
        public ActionResult Search(FormCollection col, int? page=1)
        {
            string keywords = Convert.ToString(col["keywords"]);
            string sector = Convert.ToString(col["sector"]);

            ViewBag.keywords = keywords;
            ViewBag.sector = sector;

            StringBuilder sb = new StringBuilder();

            if (sector == "0")
            {
                sb.AppendFormat(@"select top 100 vc_Id,co_id,vc_Title,lm_Id,lm_value,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies c on v.vc_co_CompanyId = c.co_Id 
            inner join VacancySectors vs on vs.vs_vc_VacancyId = v.vc_Id
            inner join ListsMaster lm on lm.lm_Id = vs.vs_lm_SectorId
            where vc_Deleted is null and co_br_HomeBrandId=2 and vc_Title like '%{0}%' order by vc_id desc", keywords);
            }
            else
            {
                sb.AppendFormat(@"select top 100 vc_Id,co_id,vc_Title,lm_Id,lm_value,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies c on v.vc_co_CompanyId = c.co_Id 
            inner join VacancySectors vs on vs.vs_vc_VacancyId = v.vc_Id
            inner join ListsMaster lm on lm.lm_Id = vs.vs_lm_SectorId
            where vc_Deleted is null and co_br_HomeBrandId=2 and vc_Title like '%{0}%' and lm_Id={1} order by vc_id desc", keywords, sector);
            }

         

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");
                    try
                    {
                        ljx.vc_Description = fparamaker(lj.vc_Description, 9);
                    }
                    catch
                    {
                        ljx.vc_Description = lj.vc_Description;
                    }
                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.LogoURL = "http://www.jobs4bahrainis.com/Logox/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + ".jpg";


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days > 200)
                    {
                        days = days - 200;
                    }
                    else if (days > 40)
                    {
                        days = days - 40;
                    }

                    ljx.postedsince = "" + days;
                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToPagedList(page ?? 1, 10);

        
                ViewBag.lstjobsectorcount = getJobsectorscount();
            }

            return View();
        }


        public ActionResult Search2(string keywords, string sector, int? page = 1)
        {

            if (Session["Ca_ID"] == null && Session["RecruiterID"] == null)
            {
                if (page > 1)
                {
                    return RedirectToAction("../Login/-/Search2-" + keywords + "-0-" + page);

                }
            }

            ViewBag.keywords = keywords;
            ViewBag.sector = sector;

            StringBuilder sb = new StringBuilder();

            if (sector == "0")
            {
                sb.AppendFormat(@"select top 100 vc_Id,co_id,vc_Title,lm_Id,lm_value,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies c on v.vc_co_CompanyId = c.co_Id 
            inner join VacancySectors vs on vs.vs_vc_VacancyId = v.vc_Id
            inner join ListsMaster lm on lm.lm_Id = vs.vs_lm_SectorId
            where vc_Deleted is null and co_br_HomeBrandId=2 and vc_Title like '%{0}%' order by vc_id desc", keywords);
            }
            else
            {
                sb.AppendFormat(@"select top 100 vc_Id,co_id,vc_Title,lm_Id,lm_value,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies c on v.vc_co_CompanyId = c.co_Id 
            inner join VacancySectors vs on vs.vs_vc_VacancyId = v.vc_Id
            inner join ListsMaster lm on lm.lm_Id = vs.vs_lm_SectorId
            where vc_Deleted is null and co_br_HomeBrandId=2 and vc_Title like '%{0}%' and lm_Id={1} order by vc_id desc", keywords, sector);
            }



            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");
                    try
                    {
                        ljx.vc_Description = fparamaker(lj.vc_Description, 9);
                    }
                    catch
                    {
                        ljx.vc_Description = lj.vc_Description;
                    }
                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.LogoURL = "http://www.jobs4bahrainis.com/Logox/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + ".jpg";


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days > 200)
                    {
                        days = days - 200;
                    }
                    else if (days > 40)
                    {
                        days = days - 40;
                    }

                    ljx.postedsince = "" + days;
                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToPagedList(page ?? 1, 10);


                ViewBag.lstjobsectorcount = getJobsectorscount();
            }

            return View();
        }


        public ActionResult SectorJobs(int sectorid, string sector, int? page=1)
        {

            if (Session["Ca_ID"] == null && Session["RecruiterID"] == null)
            {
                if (page > 1)
                {
                    return RedirectToAction("../Login/-/SectorJobs-" + sectorid + "-0-" + page);

                   

                }
            }

            

            ViewBag.SectorID = sectorid;
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(@"select top 100 vc_Id,co_id,vc_Title,lm_Id,lm_value,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies c on v.vc_co_CompanyId = c.co_Id 
            inner join VacancySectors vs on vs.vs_vc_VacancyId = v.vc_Id
            inner join ListsMaster lm on lm.lm_Id = vs.vs_lm_SectorId
            where vc_Deleted is null and co_br_HomeBrandId=2 and lm.lm_ID={0} order by vc_id desc", sectorid);

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");
                    try
                    {
                        ljx.vc_Description = fparamaker(lj.vc_Description, 9);
                    }
                    catch
                    {
                        ljx.vc_Description = lj.vc_Description;
                    }
                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.LogoURL = "http://www.jobs4bahrainis.com/Logox/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + ".jpg";


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days > 200)
                    {
                        days = days - 200;
                    }
                    else if (days > 40)
                    {
                        days = days - 40;
                    }

                    ljx.postedsince = "" + days;
                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToPagedList(page ?? 1, 10);

                ViewBag.lstjobsectorcount = getJobsectorscount();
            }

            return View();
        }




        public ActionResult LatestJobs(int? page = 1)
        {
            if (Session["Ca_ID"] == null && Session["RecruiterID"] == null)
            {
                if (page > 1)
                {
                    return RedirectToAction("../Login/0/LatestJobs-" + page);
                }
            }

                StringBuilder sb = new StringBuilder();

            sb.AppendFormat(@"select top 100 vc_Id,co_id,vc_Title,lm_Id,lm_value,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies c on v.vc_co_CompanyId = c.co_Id 
            inner join VacancySectors vs on vs.vs_vc_VacancyId = v.vc_Id
            inner join ListsMaster lm on lm.lm_Id = vs.vs_lm_SectorId
            where vc_Deleted is null and co_br_HomeBrandId=2 order by vc_id desc");

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");
                    try
                    {
                        ljx.vc_Description = fparamaker(lj.vc_Description, 9);
                    }
                    catch
                    {
                        ljx.vc_Description = lj.vc_Description;
                    }
                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.LogoURL = "http://www.jobs4bahrainis.com/Logox/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + ".jpg";


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days > 200)
                    {
                        days = days - 200;
                    }
                    else if (days > 40)
                    {
                        days = days - 40;
                    }

                    ljx.postedsince = "" + days;
                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToPagedList(page ?? 1, 10);

                ViewBag.lstjobsectorcount = getJobsectorscount();
            }

            return View();
        }

        public ActionResult JobSearchDetail(int vc_id = 0)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {



                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"select top 100 vc_Id,co_id,vc_Title,lm_Id,lm_value,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
                inner join Companies c on v.vc_co_CompanyId = c.co_Id 
                left outer join VacancySectors vs on vs.vs_vc_VacancyId = v.vc_Id
                left outer join ListsMaster lm on lm.lm_Id = vs.vs_lm_SectorId
                where vc_Deleted is null and vc_id ={0} ", vc_id);

                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");

                    ljx.vc_Description = lj.vc_Description.Replace(System.Environment.NewLine, "<br>");

                    ljx.vc_Created = lj.vc_Created;

                    ljx.co_Name = lj.co_Name;
                    ljx.LogoURL = "http://www.jobs4bahrainis.com/Logox/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + ".jpg";


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days > 200)
                    {
                        days = days - 200;
                    }
                    else if (days > 40)
                    {
                        days = days - 40;
                    }

                    ljx.postedsince = "" + days;
                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    ljx.lm_value = lj.lm_value;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToList();





                ViewBag.lstjobsectorcount = getJobsectorscount();

            }
            return View();
        }

        public ActionResult JobDescription(int jobid, string jobtitle)
        {
            if (Session["Ca_ID"] == null && Session["RecruiterID"] == null) { return RedirectToAction("../Login/-/JobDescription-" + jobid); }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {



                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"select top 100 vc_Id,co_id,vc_Title,lm_Id,lm_value,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies c on v.vc_co_CompanyId = c.co_Id 
            inner join VacancySectors vs on vs.vs_vc_VacancyId = v.vc_Id
            inner join ListsMaster lm on lm.lm_Id = vs.vs_lm_SectorId
            where vc_Deleted is null and vc_id ={0} ", jobid);

                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");

                    ljx.vc_Description = lj.vc_Description.Replace(System.Environment.NewLine, "<br>");

                    ljx.vc_Created = lj.vc_Created;

                    ljx.co_Name = lj.co_Name;
                    ljx.LogoURL = "http://www.jobs4bahrainis.com/Logox/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + ".jpg";


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days > 200)
                    {
                        days = days - 200;
                    }
                    else if (days > 40)
                    {
                        days = days - 40;
                    }

                    ljx.postedsince = "" + days;
                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    ljx.lm_Id = lj.lm_Id;
                    ljx.lm_value = lj.lm_value;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToList();





                ViewBag.lstjobsectorcount = getJobsectorscount();

            }
            return View();
        }



        public List<jobsector> getJobsectorscount() {



            string jobSectorCount = @"select lm_id as SectorID,lm_Value as Sector,COUNT(vc_id) as Total from listsmaster 
                        right join vacancysectors on vs_lm_sectorid=lm_id
                        join vacancies on vc_id=vs_vc_vacancyid
                        where lm_lt_listtypeid=4 and lm_deleted is null and vc_deleted is null and vc_br_sourcebrandid=2 
                        and Vacancies.vc_ExpiryDate > GETDATE() and vc_closingdate>getdate() and vc_st_statusid=1
                        group by lm_value,lm_id
                        union all
                        select lm_id,lm_value as SectorName, 0 as total from listsmaster  where lm_id not in(
                        select distinct lm_id from listsmaster 
                        right join vacancysectors on vs_lm_sectorid=lm_id
                        join vacancies on vc_id=vs_vc_vacancyid
                        where lm_lt_listtypeid=4 and lm_deleted is null and vc_deleted is null and vc_br_sourcebrandid=2 
                        and Vacancies.vc_ExpiryDate > GETDATE() and vc_closingdate>getdate() and vc_st_statusid=1
                        )and lm_lt_listtypeid=4 and lm_deleted is null
                        order by lm_value";


            
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<jobsector> lstjobsectorcount = db.Database.SqlQuery<jobsector>(jobSectorCount).ToList();
                jobsector js;

                List<jobsector> lstjs = new List<jobsector>();

                foreach (var jsc in lstjobsectorcount)
                {
                    js = new jobsector();
                    js.SectorID = jsc.SectorID;
                    js.Sector = jsc.Sector;
                    js.SectorURL = urlcleaner(jsc.Sector);
                    lstjs.Add(js);
                }



                return lstjs;

            }



        }







        public ActionResult PostAJob()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }
            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;

                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Value").ToList();
            }
            return View();
        }

        [HttpPost]
        public ActionResult PostedJob(FormCollection col)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }


            common cmn = new common();
            dbOperations dbo = new dbOperations();




            //creating job
            SqlCon mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "INSERT INTO Vacancies (vc_Created,vc_LastUpdated,vc_br_SourceBrandId,vc_co_CompanyId,vc_ct_ContactId,vc_Reference,vc_Title,vc_lm_JobTypeId,vc_Location,vc_Experience,vc_Qualification,vc_AgeGroup,vc_SalaryFrom,vc_SalaryTo,vc_Commission,vc_Benefits,vc_Description,vc_ApplicationEmail,vc_ApplicationEmail2,vc_ClosingDate,vc_ExpiryDate,vc_st_StatusID,vc_Disabled) VALUES " +
                        " (@vc_Created,@vc_LastUpdated,@vc_br_SourceBrandId,@vc_co_CompanyId,@vc_ct_ContactId,@vc_Reference,@vc_Title,@vc_lm_JobTypeId,@vc_Location,@vc_Experience,@vc_Qualification,@vc_AgeGroup,@vc_SalaryFrom,@vc_SalaryTo,@vc_Commission,@vc_Benefits,@vc_Description,@vc_ApplicationEmail,@vc_ApplicationEmail2,@vc_ClosingDate,@vc_ExpiryDate,@vc_st_StatusID,@vc_Disabled); select @@identity";
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Created", DateTime.Now);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_LastUpdated", DateTime.Now);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_br_SourceBrandId", 2);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_co_CompanyId", Session["CompanyID"]);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ct_ContactId", Session["RecruiterID"]);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Reference", Convert.ToString(col["referencecode"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Title", Convert.ToString(col["jobtitle"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_lm_JobTypeId", Convert.ToString(col["contracttype"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Location", Convert.ToString(col["location"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Experience", Convert.ToString(col["experience"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Qualification", Convert.ToString(col["qualification"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_AgeGroup", Convert.ToString(col["agegroup"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_SalaryFrom", Convert.ToString(col["salaryfrom"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_SalaryTo", Convert.ToString(col["salaryto"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Commission", Convert.ToString(col["commission"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Benefits", Convert.ToString(col["benefits"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Description", Convert.ToString(col["description"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ApplicationEmail", Convert.ToString(col["applicationemailid"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ApplicationEmail2", Convert.ToString(col["applicationemailid2"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ClosingDate", DateTime.Now.AddMonths(1));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ExpiryDate", DateTime.Now.AddMonths(1));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_st_StatusID", 1);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Disabled", Convert.ToBoolean(col["reserved"]));

            mycon.sqlConOpen();
            int VacancyID = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
            mycon.sqlConClose();

            //creating Vacancy sectors

            string sect = Convert.ToString(col["chksector"]);
            string[] sector = sect.Split(',');

            foreach (var s in sector)
            {

                dbo.VacancySectors(VacancyID, Convert.ToInt32(s));

            }

            //send mail
            string bodycontent;
            bodycontent = "<font face='Arial' size='2'>";
            bodycontent += "<p>Job has been successfully posted on Jobs4Bahrainis Website by <b>" + Session["ContactPerson"] + "<b></p>";
            bodycontent += "<h2>" + Convert.ToString(col["jobtitle"]) + "</h2>";
            bodycontent += "<p>Expires on: " + DateTime.Now.AddMonths(1) + "</p>";
            bodycontent += "<p>Ref No.: " + Convert.ToString(col["referencecode"]) + "</p>";
            bodycontent += "<p>Location: " + Convert.ToString(col["location"]) + "</p>";
            bodycontent += "</font>";


          //  cmn.SendMail(Convert.ToString(col["applicationemailid"]), "admin@jobs4bahrainis.com", "Job Posted on J4B Website", bodycontent);

            return View();
        }


        public ActionResult ManageVacancies()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;


                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(@"select  vc_Id,co_id,vc_Title,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo from Vacancies 
inner join Companies on vc_co_CompanyId=co_Id where vc_co_CompanyId=" + companyid + " and vc_Deleted is null order by vc_id desc").ToList();

                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");
                    try
                    {
                        ljx.vc_Description = fparamaker(lj.vc_Description, 20);
                    }
                    catch
                    {
                        ljx.vc_Description = lj.vc_Description;
                    }
                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    extns = lj.co_LogoExtension == "" ? "jpg" : lj.co_LogoExtension;

                    ljx.LogoURL = "http://www.jobs4bahrainis.com/Logox/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "." + extns;


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days > 200)
                    {
                        days = days - 200;
                    }
                    else if (days > 40)
                    {
                        days = days - 40;
                    }

                    ljx.postedsince = "" + days;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToList();


            }
            return View();
        }


        //public ActionResult CandidateSearch()
        //{
        //    if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

        //    int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {
        //        var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
        //        int companyid = recruiter[0].ct_co_CompanyId;

        //        var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

        //        string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
        //        ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

        //        ViewBag.Company = company[0].co_Name;
        //        ViewBag.Name = recruiter[0].ct_Forename;
        //        ViewBag.Email = recruiter[0].ct_EmailAddress;
        //        ViewBag.Phone = recruiter[0].ct_Telephone;

        //        ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();

        //    }
 
        //    return View();
        //}

        public ActionResult CandidateSearch()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;

                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();
              
                dbOperations dbo = new dbOperations();
                ViewBag.FunctionalTitles = dbo.getlist(17);
                List<JobSeekers> jobseekers = db.Database.SqlQuery<JobSeekers>("select ca_Id,ca_GUID, ca_FirstName,ca_Surname,ca_EmailAddress,ca_PhoneMobile,ca_DateOfBirth,ca_Gender,ca_photoextension from candidates where ca_id in (select top 20 app_ca_CandidateId from Applications where app_vc_VacancyId in (select vc_id from Vacancies where vc_co_CompanyId=" + companyid + ") order by app_id desc)").ToList();

                List<JobSeekers> jos = new List<JobSeekers>();

                JobSeekers jobskr;

                foreach (var js in jobseekers)
                {
                    jobskr = new JobSeekers();

                    jobskr.ca_FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(js.ca_FirstName.ToLower());
                    jobskr.ca_Surname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(js.ca_Surname.ToLower());
                    jobskr.ca_EmailAddress = js.ca_EmailAddress.ToLower();
                    jobskr.ca_PhoneMobile = js.ca_PhoneMobile.Replace("00973", "").Replace("+973", "").Replace(" ", "");
                    jobskr.ca_DateOfBirth = js.ca_DateOfBirth;
                    jobskr.Age = DateTime.Now.Year - js.ca_DateOfBirth.Value.Year;
                    jobskr.ca_Gender = js.ca_Gender == "M" ? "Male" : "Female";
                    jobskr.ca_photoextension = js.ca_photoextension;
                    jobskr.ca_GUID = js.ca_GUID;
                    string extn = js.ca_photoextension == "" ? "jpg" : js.ca_photoextension;
                    jobskr.ProfilePic = "http://www.jobs4bahrainis.com/documents/photos/" + js.ca_GUID.ToString().Substring(0, 2) + "/" + js.ca_GUID.ToString().Substring(2, 2) + "/" + js.ca_GUID.ToString().Substring(4, 2) + "/" + js.ca_GUID.ToString() + "." + extn;
                    jos.Add(jobskr);
                }
                ViewBag.jobseekers = jos.ToList();
            }


            return View();
        }

        public ActionResult CandidateSearch2()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;

                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();

                dbOperations dbo = new dbOperations();
                ViewBag.FunctionalTitles = dbo.getlist(17);
                List<JobSeekers> jobseekers = db.Database.SqlQuery<JobSeekers>("select ca_Id,ca_GUID, ca_FirstName,ca_Surname,ca_EmailAddress,ca_PhoneMobile,ca_DateOfBirth,ca_Gender,ca_photoextension from candidates where ca_id in (select top 20 app_ca_CandidateId from Applications where app_vc_VacancyId in (select vc_id from Vacancies where vc_co_CompanyId=" + companyid + ") order by app_id desc)").ToList();

                List<JobSeekers> jos = new List<JobSeekers>();

                JobSeekers jobskr;

                foreach (var js in jobseekers)
                {
                    jobskr = new JobSeekers();

                    jobskr.ca_FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(js.ca_FirstName.ToLower());
                    jobskr.ca_Surname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(js.ca_Surname.ToLower());
                    jobskr.ca_EmailAddress = js.ca_EmailAddress.ToLower();
                    jobskr.ca_PhoneMobile = js.ca_PhoneMobile.Replace("00973", "").Replace("+973", "").Replace(" ", "");
                    jobskr.ca_DateOfBirth = js.ca_DateOfBirth;
                    jobskr.Age = DateTime.Now.Year - js.ca_DateOfBirth.Value.Year;
                    jobskr.ca_Gender = js.ca_Gender == "M" ? "Male" : "Female";
                    jobskr.ca_photoextension = js.ca_photoextension;
                    jobskr.ca_GUID = js.ca_GUID;
                    string extn = js.ca_photoextension == "" ? "jpg" : js.ca_photoextension;
                    jobskr.ProfilePic = "http://www.jobs4bahrainis.com/documents/photos/" + js.ca_GUID.ToString().Substring(0, 2) + "/" + js.ca_GUID.ToString().Substring(2, 2) + "/" + js.ca_GUID.ToString().Substring(4, 2) + "/" + js.ca_GUID.ToString() + "." + extn;
                    jos.Add(jobskr);
                }
                ViewBag.jobseekers = jos.ToList();
            }


            return View();
        }

        //[HttpPost]
        //public ActionResult CandidateSearchResult(CandidateSearchModel model)
        //{
        //    decimal? salaryFrom = null, salaryTo = null;
        //    var spltSalary = model.salary != null ? model.salary.Split(':') : null;
        //    if (spltSalary != null && spltSalary.Count() == 2)
        //    {
        //        salaryFrom = decimal.Parse(spltSalary[0]);
        //        salaryTo = decimal.Parse(spltSalary[1]);
        //    }

        //    decimal? expFrom = null, expTo = null;
        //    var spltExp = model.experience != null ? model.experience.Split(':') : null;
        //    if (spltExp != null && spltExp.Count() == 2)
        //    {
        //        expFrom = int.Parse(spltExp[0]);
        //        expTo = int.Parse(spltExp[1]);
        //    }
        //    List<CandidateSearchResultModel> candidateResult = null;
        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {

        //        SqlParameter fNameParam = AddParameter("p_firstname", model.fname, typeof(string));
        //        SqlParameter lNameParam = AddParameter("p_surname", model.lname, typeof(string));
        //        SqlParameter ageParam = AddParameter("p_age", model.age, typeof(Int32));
        //        SqlParameter genderParam = AddParameter("p_Gender", model.gender, typeof(string));
        //        SqlParameter qualififcationParam = AddParameter("p_qaulification", model.qualification, typeof(Int32));
        //        SqlParameter salaryFromParam = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
        //        SqlParameter salaryToParam = AddParameter("p_salaryto", salaryTo, typeof(decimal));
        //        SqlParameter expFromParam = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
        //        SqlParameter expToParam = AddParameter("p_experienceTo", expTo, typeof(Int32));
        //        SqlParameter keywordParam = AddParameter("p_keyword", model.keyword, typeof(String));


        //        object[] parameters = new object[] { fNameParam, lNameParam, ageParam, genderParam, qualififcationParam
        //                                            , salaryFromParam, salaryToParam, expFromParam,expToParam
        //                                            ,keywordParam};
        //        string sqlQry = "proc_CandidateSearch @p_firstname,@p_surname,@p_age,@p_Gender,@p_qaulification,@p_salaryfrom,@p_salaryto,@p_experienceFrom,@p_experienceTo,@p_keyword";
        //        candidateResult = db.Database.SqlQuery<CandidateSearchResultModel>(sqlQry, parameters).ToList();

        //        var candidateResultView = new List<CandidateSearchResultModel>();
        //        int cnt = 0;
        //        foreach (var d in candidateResult)
        //        {
        //            SqlParameter ca_id_Param = AddParameter("ca_id", d.ca_Id, typeof(Int32));
        //            parameters = new object[] { ca_id_Param };
        //            sqlQry = "proc_CandidateSearchInterest @ca_id";
        //            var ca_interest = db.Database.SqlQuery<CandidateSearchInerestResultModel>(sqlQry, parameters).FirstOrDefault();
        //            d.Interest = ca_interest.Interest;
        //            candidateResult[cnt].Interest = d.Interest;
        //            cnt++;
        //        }
        //    }
        //    return View(candidateResult);
        //}

        //public ActionResult CandidateSearchResult(string keyword, string fname, string lname, int? age, string gender, string[] chksector, int? qualification, string salary, string experience, int? page)
        //{
        //    decimal? salaryFrom = null, salaryTo = null;
        //    var spltSalary = salary != null ? salary.Split(':') : null;
        //    if (spltSalary != null && spltSalary.Count() == 2)
        //    {
        //        salaryFrom = decimal.Parse(spltSalary[0]);
        //        salaryTo = decimal.Parse(spltSalary[1]);
        //    }

        //    decimal? expFrom = null, expTo = null;
        //    var spltExp = experience != null ? experience.Split(':') : null;
        //    if (spltExp != null && spltExp.Count() == 2)
        //    {
        //        expFrom = int.Parse(spltExp[0]);
        //        expTo = int.Parse(spltExp[1]);
        //    }
        //    int totalSearchCount = 0;
        //    var pageIndex = (page ?? 1); //set pageIndex to 1 when null
        //    var pageSize = 10; // max record display per page
        //    List<CandidateSearchResultModel> candidateResult = null;
        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {

        //        SqlParameter fNameParam = AddParameter("p_firstname", fname, typeof(string));
        //        SqlParameter lNameParam = AddParameter("p_surname", lname, typeof(string));
        //        SqlParameter ageParam = AddParameter("p_age", age, typeof(Int32));
        //        SqlParameter genderParam = AddParameter("p_Gender", gender, typeof(string));
        //        SqlParameter qualififcationParam = AddParameter("p_qaulification", qualification, typeof(Int32));
        //        SqlParameter salaryFromParam = AddParameter("p_salaryfrom", salaryFrom, tgetABCcompaniesypeof(decimal));
        //        SqlParameter salaryToParam = AddParameter("p_salaryto", salaryTo, typeof(decimal));
        //        SqlParameter expFromParam = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
        //        SqlParameter expToParam = AddParameter("p_experienceTo", expTo, typeof(Int32));
        //        SqlParameter keywordParam = AddParameter("p_keyword", keyword, typeof(String));
        //        SqlParameter searchTypeParam = AddParameter("p_searchType", 1, typeof(Int32));
        //        SqlParameter pageSizeParam = AddParameter("p_pageSize", pageSize, typeof(Int32));
        //        SqlParameter pageIndexParam = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

        //        object[] parameters = new object[] { fNameParam, lNameParam, ageParam, genderParam, qualififcationParam
        //                                            , salaryFromParam, salaryToParam, expFromParam,expToParam
        //                                            ,keywordParam,searchTypeParam,pageSizeParam,pageIndexParam};
        //        string sqlQry = @"proc_CandidateSearch @p_firstname,@p_surname,@p_age,@p_Gender,@p_qaulification
        //                          ,@p_salaryfrom,@p_salaryto,@p_experienceFrom,@p_experienceTo,@p_keyword,@p_searchType,
        //                          @p_pageSize,@p_pageIndex";

        //        candidateResult = db.Database.SqlQuery<CandidateSearchResultModel>(sqlQry, parameters).ToList();

        //        // getting record count for the search filter
        //        SqlParameter fNameParam2 = AddParameter("p_firstname", fname, typeof(string));
        //        SqlParameter lNameParam2 = AddParameter("p_surname", lname, typeof(string));
        //        SqlParameter ageParam2 = AddParameter("p_age", age, typeof(Int32));
        //        SqlParameter genderParam2 = AddParameter("p_Gender", gender, typeof(string));
        //        SqlParameter qualififcationParam2 = AddParameter("p_qaulification", qualification, typeof(Int32));
        //        SqlParameter salaryFromParam2 = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
        //        SqlParameter salaryToParam2 = AddParameter("p_salaryto", salaryTo, typeof(decimal));
        //        SqlParameter expFromParam2 = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
        //        SqlParameter expToParam2 = AddParameter("p_experienceTo", expTo, typeof(Int32));
        //        SqlParameter keywordParam2 = AddParameter("p_keyword", keyword, typeof(String));
        //        SqlParameter searchTypeParam2 = AddParameter("p_searchType", 2, typeof(Int32));
        //        SqlParameter pageSizeParam2 = AddParameter("p_pageSize", pageSize, typeof(Int32));
        //        SqlParameter pageIndexParam2 = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

        //        parameters = new object[] { fNameParam2, lNameParam2, ageParam2, genderParam2, qualififcationParam2
        //                                            , salaryFromParam2, salaryToParam2, expFromParam2,expToParam2
        //                                            ,keywordParam2,searchTypeParam2,pageSizeParam2,pageIndexParam2};
        //        totalSearchCount = db.Database.SqlQuery<int>(sqlQry, parameters).FirstOrDefault();
        //        //var candidateResultView = new List<CandidateSearchResultModel>();
        //        int cnt = 0;
        //        foreach (var d in candidateResult)
        //        {
        //            SqlParameter ca_id_Param = AddParameter("ca_id", d.ca_Id, typeof(Int32));
        //            parameters = new object[] { ca_id_Param };
        //            sqlQry = "proc_CandidateSearchInterest @ca_id";
        //            var ca_interest = db.Database.SqlQuery<CandidateSearchInerestResultModel>(sqlQry, parameters).FirstOrDefault();
        //            d.Interest = ca_interest.Interest;
        //            candidateResult[cnt].Interest = d.Interest;
        //            cnt++;
        //        }


        //    }
        //    // this is just to test with mock data
        //    //for (int i = 0; i < 100; i++)
        //    //{
        //    //    candidateResult.Add(candidateResult[0]);
        //    //}


        //    var candidateResultFilter = new StaticPagedList<CandidateSearchResultModel>(candidateResult, pageIndex, pageSize, totalSearchCount);


        //    return View(candidateResultFilter);


        //}

        //public ActionResult CandidateSearchResult(string keyword, string fname, string lname, int? age, string gender, string[] chksector, int? qualification, string salary, string experience, int? page)
        //{
        //    decimal? salaryFrom = null, salaryTo = null;
        //    var spltSalary = salary != null ? salary.Split(':') : null;
        //    if (spltSalary != null && spltSalary.Count() == 2)
        //    {
        //        salaryFrom = decimal.Parse(spltSalary[0]);
        //        salaryTo = decimal.Parse(spltSalary[1]);
        //    }

        //    decimal? expFrom = null, expTo = null;
        //    var spltExp = experience != null ? experience.Split(':') : null;
        //    if (spltExp != null && spltExp.Count() == 2)
        //    {
        //        expFrom = int.Parse(spltExp[0]);
        //        expTo = int.Parse(spltExp[1]);
        //    }
        //    string sectors = null;
        //    if (chksector != null)
        //    {
        //        foreach (var d in chksector)
        //        {
        //            sectors += d + ",";
        //        }
        //        sectors = sectors.Substring(0, sectors.Length - 1);
        //    }
        //    int totalSearchCount = 0;
        //    var pageIndex = (page ?? 1); //set pageIndex to 1 when null
        //    var pageSize = 10; // max record display per page
        //    List<CandidateSearchResultModel> candidateResult = null;
        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {
        //        fname = string.IsNullOrEmpty(fname) ? null : fname;
        //        lname = string.IsNullOrEmpty(lname) ? null : lname;
        //        gender = string.IsNullOrEmpty(gender) ? null : gender;
        //        keyword = string.IsNullOrEmpty(keyword) ? null : keyword;

        //        SqlParameter fNameParam = AddParameter("p_firstname", fname, typeof(string));
        //        SqlParameter lNameParam = AddParameter("p_surname", lname, typeof(string));
        //        SqlParameter ageParam = AddParameter("p_age", age, typeof(Int32));
        //        SqlParameter genderParam = AddParameter("p_Gender", gender, typeof(string));
        //        SqlParameter qualififcationParam = AddParameter("p_qaulification", qualification, typeof(Int32));
        //        SqlParameter salaryFromParam = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
        //        SqlParameter salaryToParam = AddParameter("p_salaryto", salaryTo, typeof(decimal));
        //        SqlParameter expFromParam = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
        //        SqlParameter expToParam = AddParameter("p_experienceTo", expTo, typeof(Int32));
        //        SqlParameter keywordParam = AddParameter("p_keyword", keyword, typeof(String));
        //        SqlParameter sectorParam = AddParameter("p_sector", sectors, typeof(String));
        //        SqlParameter searchTypeParam = AddParameter("p_searchType", 1, typeof(Int32));
        //        SqlParameter pageSizeParam = AddParameter("p_pageSize", pageSize, typeof(Int32));
        //        SqlParameter pageIndexParam = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

        //        object[] parameters = new object[] { fNameParam, lNameParam, ageParam, genderParam, qualififcationParam
        //                                            , salaryFromParam, salaryToParam, expFromParam,expToParam
        //                                            ,keywordParam,searchTypeParam,pageSizeParam,pageIndexParam,sectorParam};
        //        string sqlQry = @"proc_CandidateSearch @p_firstname,@p_surname,@p_age,@p_Gender,@p_qaulification
        //                          ,@p_salaryfrom,@p_salaryto,@p_experienceFrom,@p_experienceTo,@p_keyword,@p_searchType,
        //                          @p_pageSize,@p_pageIndex,@p_sector";

        //        candidateResult = db.Database.SqlQuery<CandidateSearchResultModel>(sqlQry, parameters).ToList();

        //        // getting record count for the search filter
        //        SqlParameter fNameParam2 = AddParameter("p_firstname", fname, typeof(string));
        //        SqlParameter lNameParam2 = AddParameter("p_surname", lname, typeof(string));
        //        SqlParameter ageParam2 = AddParameter("p_age", age, typeof(Int32));
        //        SqlParameter genderParam2 = AddParameter("p_Gender", gender, typeof(string));
        //        SqlParameter qualififcationParam2 = AddParameter("p_qaulification", qualification, typeof(Int32));
        //        SqlParameter salaryFromParam2 = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
        //        SqlParameter salaryToParam2 = AddParameter("p_salaryto", salaryTo, typeof(decimal));
        //        SqlParameter expFromParam2 = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
        //        SqlParameter expToParam2 = AddParameter("p_experienceTo", expTo, typeof(Int32));
        //        SqlParameter keywordParam2 = AddParameter("p_keyword", keyword, typeof(String));
        //        SqlParameter searchTypeParam2 = AddParameter("p_searchType", 2, typeof(Int32));
        //        SqlParameter pageSizeParam2 = AddParameter("p_pageSize", pageSize, typeof(Int32));
        //        SqlParameter pageIndexParam2 = AddParameter("p_pageIndex", pageIndex, typeof(Int32));
        //        SqlParameter sectorParam2 = AddParameter("p_sector", sectors, typeof(String));


        //        parameters = new object[] { fNameParam2, lNameParam2, ageParam2, genderParam2, qualififcationParam2
        //                                            , salaryFromParam2, salaryToParam2, expFromParam2,expToParam2
        //                                            ,keywordParam2,searchTypeParam2,pageSizeParam2,pageIndexParam2,sectorParam2};
        //        totalSearchCount = db.Database.SqlQuery<int>(sqlQry, parameters).FirstOrDefault();
        //        //var candidateResultView = new List<CandidateSearchResultModel>();
        //        int cnt = 0;
        //        foreach (var d in candidateResult)
        //        {
        //            SqlParameter ca_id_Param = AddParameter("ca_id", d.ca_Id, typeof(Int32));
        //            parameters = new object[] { ca_id_Param };
        //            sqlQry = "proc_CandidateSearchInterest @ca_id";
        //            var ca_interest = db.Database.SqlQuery<CandidateSearchInerestResultModel>(sqlQry, parameters).FirstOrDefault();
        //            d.Interest = ca_interest.Interest;
        //            candidateResult[cnt].Interest = d.Interest;
        //            cnt++;
        //        }


        //    }
        //    // this is just to test with mock data
        //    //for (int i = 0; i < 100; i++)
        //    //{
        //    //    candidateResult.Add(candidateResult[0]);
        //    //}


        //    var candidateResultFilter = new StaticPagedList<CandidateSearchResultModel>(candidateResult, pageIndex, pageSize, totalSearchCount);


        //    return View(candidateResultFilter);


        //}

        //     public ActionResult CandidateSearchResult(string keyword,string fname, string lname,int? age ,string gender,string[] chksector,int? qualification,string salary,string experience,int? page)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    decimal? salaryFrom = null, salaryTo = null;
        //    var spltSalary = salary != null ? salary.Split(':') : null;
        //    if (spltSalary != null && spltSalary.Count() == 2)
        //    {
        //        salaryFrom = decimal.Parse(spltSalary[0]);
        //        salaryTo = decimal.Parse(spltSalary[1]);
        //    }

        //    decimal? expFrom = null, expTo = null;
        //    var spltExp = experience != null ? experience.Split(':') : null;
        //    if (spltExp != null && spltExp.Count() == 2)
        //    {
        //        expFrom = int.Parse(spltExp[0]);
        //        expTo = int.Parse(spltExp[1]);
        //    }
        //    string sectors = null;
        //    if(chksector != null)
        //    {
        //        foreach(var d in chksector)
        //        {
        //            sectors += d + ",";
        //        }
        //        sectors = sectors.Substring(0, sectors.Length - 1);
        //    }
        //    int totalSearchCount = 0;
        //    var pageIndex = (page ?? 1); //set pageIndex to 1 when null
        //    var pageSize = 10; // max record display per page
        //    List<CandidateSearchResultModel> candidateResult = null;
        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {
        //        fname = string.IsNullOrEmpty(fname) ? null : fname;
        //        lname = string.IsNullOrEmpty(lname) ? null : lname;
        //        gender = string.IsNullOrEmpty(gender) ? null : gender;
        //        keyword = string.IsNullOrEmpty(keyword) ? null : keyword;
        //        sectors = string.IsNullOrEmpty(sectors) ? null : sectors;
        //        sb.Append(@"");
        //        bool whereUsed = false;
        //        bool whereExpUsed = false;
        //        string whereCand = "";
        //        string whereExp = "";
        //        if (!string.IsNullOrEmpty(fname))
        //        {
        //            whereCand = @"where ca_FirstName = ltrim(rtrim(@p_firstname)) ";
        //            whereUsed = true;
        //        }

        //        if (!string.IsNullOrEmpty(lname))
        //        {
        //            whereCand = whereUsed ? "and ca_Surname = ltrim(rtrim(@p_surname)) " : @"where ca_Surname = ltrim(rtrim(@p_surname)) ";
        //            whereUsed = true;
        //        }

        //        if (!string.IsNullOrEmpty(gender))
        //        {
        //            whereCand = whereUsed ? "and ca_Gender = ltrim(rtrim(@p_Gender)) " : @"where ca_Gender = ltrim(rtrim(@p_Gender)) ";
        //            whereUsed = true;
        //        }

        //        if (age != null) { 
        //            whereCand = whereUsed ? "and ca_Gender = @p_age " : @"where ca_Gender = @p_age ";
        //            whereUsed = true;
        //        }

        //        if (qualification != null)
        //        {
        //            whereCand = whereUsed ? "and ca_lm_EducationLevel = @p_qaulification " : @"where ca_lm_EducationLevel = @p_qaulification ";
        //            whereUsed = true;
        //        }

        //        if (salaryFrom != null && salaryTo != null)
        //        {
        //            if (salaryFrom != -1)
        //            {
        //                whereCand = whereUsed ? "and ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)) " : @"where ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)) ";
        //            }
        //            else
        //            {
        //                whereCand = whereUsed ? "and ca_SalaryFrom >= @p_salaryto " : @"where ca_SalaryFrom >= @p_salaryto ";

        //            }
        //            whereUsed = true;
        //        }

        //        if (!string.IsNullOrEmpty(keyword))
        //        {
        //            whereCand = whereUsed ? "and ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%') " : @"where ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%') ";
        //            whereUsed = true;
        //        }

        //        if (expFrom != null && expTo != null)
        //        {
        //            if (expFrom != -1)
        //            {
        //                whereExp = whereExpUsed ? "and  (CE.TotalExp between  @p_experienceFrom and @p_experienceTo) " : @"where (CE.TotalExp between  @p_experienceFrom and @p_experienceTo) ";
        //            }
        //            else
        //            {
        //                whereExp = whereExpUsed ? "and CE.TotalExp >= @p_experienceTo " : @"where CE.TotalExp >= @p_experienceTo ";

        //            }
        //            whereExpUsed = true;
        //        }

        //        if (!string.IsNullOrEmpty(sectors))
        //        {
        //            whereCand = whereUsed ? "and ca_id in ( select ci_ca_CandidateId from CandidateInterests where ci_lm_LinkId in (16,36,26)) " : @"where ca_id in ( select ci_ca_CandidateId from CandidateInterests where ci_lm_LinkId in (16,36,26)) ";
        //            whereUsed = true;
        //        }

        //        SqlParameter fNameParam = AddParameter("p_firstname", fname, typeof(string));
        //        SqlParameter lNameParam = AddParameter("p_surname", lname, typeof(string));
        //        SqlParameter ageParam = AddParameter("p_age", age, typeof(Int32));
        //        SqlParameter genderParam = AddParameter("p_Gender", gender, typeof(string));
        //        SqlParameter qualififcationParam = AddParameter("p_qaulification", qualification, typeof(Int32));
        //        SqlParameter salaryFromParam = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
        //        SqlParameter salaryToParam = AddParameter("p_salaryto", salaryTo, typeof(decimal));
        //        SqlParameter expFromParam = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
        //        SqlParameter expToParam = AddParameter("p_experienceTo", expTo, typeof(Int32));
        //        SqlParameter keywordParam = AddParameter("p_keyword", keyword, typeof(String));
        //        SqlParameter sectorParam = AddParameter("p_sector", sectors, typeof(String));
        //        SqlParameter searchTypeParam = AddParameter("p_searchType", 1, typeof(Int32));
        //        SqlParameter pageSizeParam = AddParameter("p_pageSize", pageSize, typeof(Int32));
        //        SqlParameter pageIndexParam = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

        //        object[] parameters = new object[] { fNameParam, lNameParam, ageParam, genderParam, qualififcationParam
        //                                            , salaryFromParam, salaryToParam, expFromParam,expToParam
        //                                            ,keywordParam,searchTypeParam,pageSizeParam,pageIndexParam,sectorParam};
        //        string sqlQry = @"proc_CandidateSearch @p_firstname,@p_surname,@p_age,@p_Gender,@p_qaulification
        //                          ,@p_salaryfrom,@p_salaryto,@p_experienceFrom,@p_experienceTo,@p_keyword,@p_searchType,
        //                          @p_pageSize,@p_pageIndex,@p_sector";
        //        sqlQry = string.Format(@"select CA.*,CE.TotalExp,LM.EducationLevel
        //           from
        //           (
        //           select  ca_Surname + ' ' + ca_FirstName ca_FullName ,
        //             ca_EmailAddress,
        //             ca_PhoneMobile,
        //             case ca_Gender
        //             when 'M' then 'Male'
        //             when 'F' then 'Female'
        //             else '' end as ca_Gender,
        //             ca_DateOfBirth,
        //             ca_lm_EducationLevel,
        //             ca_SalaryFrom,
        //             ca_SalaryTo,
        //             ca_Id,
        //             dbo.fun_GetAge(ca_DateOfBirth) ca_Age,
        //             ca_Profile,
        //             dbo.fun_GetProfilePic(ca_GUID,ca_PhotoExtension) LogoURL

        //           from candidates c
        //           {0}
        //           ) CA
        //           left outer join
        //           (
        //           select lm_Id,lm_Value as EducationLevel 
        //           from ListsMaster 
        //           where lm_lt_ListTypeId = 12
        //           ) LM
        //           ON CA.ca_lm_EducationLevel = LM.lm_Id
        //           left outer join
        //           (
        //           select Ca_ID, cast(isnull(sum(round(datediff(dd,fromdate,todate)/365.25,2)),0) as decimal(5,2))  TotalExp 
        //           from CandidateExperience
        //           group by Ca_ID 
        //           ) CE
        //           ON CA.Ca_ID = CE.Ca_ID
        //           {1}
        //           order by Ca_ID
        //           OFFSET @p_pageSize  * (@p_pageIndex -1) rows fetch next @p_pageSize ROWS ONLY",whereCand,whereExp);
        //        /*
        //          sqlQry = @"select CA.*,CE.TotalExp,LM.EducationLevel
        //                      from
        //                      (
        //                      select  ca_Surname + ' ' + ca_FirstName ca_FullName ,
        //                              ca_EmailAddress,
        //                              ca_PhoneMobile,
        //                              case ca_Gender
        //                              when 'M' then 'Male'
        //                              when 'F' then 'Female'
        //                              else '' end as ca_Gender,
        //                              ca_DateOfBirth,
        //                              ca_lm_EducationLevel,
        //                              ca_SalaryFrom,
        //                              ca_SalaryTo,
        //                              ca_Id,
        //                              dbo.fun_GetAge(ca_DateOfBirth) ca_Age,
        //                              ca_Profile,
        //                              dbo.fun_GetProfilePic(ca_GUID,ca_PhotoExtension) LogoURL

        //                      from candidates c
        //                      where (@p_firstname is null or ca_FirstName = ltrim(rtrim(@p_firstname)))
        //                        and (@p_surname is null or ca_Surname = ltrim(rtrim(@p_surname)))
        //                        and (@p_Gender is null or ca_Gender = @p_Gender)
        //                        and (@p_age is null or dbo.fun_GetAge(ca_DateOfBirth) = @p_age)
        //                        and (@p_qaulification is null or ca_lm_EducationLevel = @p_qaulification)
        //                        and ((@p_salaryfrom is null and @p_salaryto is null) or ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)))
        //                        and (@p_keyword is null) or ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%')
        //                      ) CA
        //                      left outer join
        //                      (
        //                      select lm_Id,lm_Value as EducationLevel 
        //                      from ListsMaster 
        //                      where lm_lt_ListTypeId = 12
        //                      ) LM
        //                      ON CA.ca_lm_EducationLevel = LM.lm_Id
        //                      left outer join
        //                      (
        //                      select Ca_ID, cast(isnull(sum(round(datediff(dd,fromdate,todate)/365.25,2)),0) as decimal(5,2))  TotalExp 
        //                      from CandidateExperience
        //                      group by Ca_ID 
        //                      ) CE
        //                      ON CA.Ca_ID = CE.Ca_ID
        //                      where ((@p_experienceFrom is null and @p_experienceTo is null) or (CE.TotalExp between  @p_experienceFrom and @p_experienceTo))
        //                      order by Ca_ID
        //                      OFFSET @p_pageSize  * (@p_pageIndex -1) rows fetch next @p_pageSize ROWS ONLY";
        //          */

        //        candidateResult = db.Database.SqlQuery<CandidateSearchResultModel>(sqlQry, parameters).ToList();

        //        // getting record count for the search filter
        //        SqlParameter fNameParam2 = AddParameter("p_firstname", fname, typeof(string));
        //        SqlParameter lNameParam2 = AddParameter("p_surname", lname, typeof(string));
        //        SqlParameter ageParam2 = AddParameter("p_age", age, typeof(Int32));
        //        SqlParameter genderParam2 = AddParameter("p_Gender", gender, typeof(string));
        //        SqlParameter qualififcationParam2 = AddParameter("p_qaulification", qualification, typeof(Int32));
        //        SqlParameter salaryFromParam2 = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
        //        SqlParameter salaryToParam2 = AddParameter("p_salaryto", salaryTo, typeof(decimal));
        //        SqlParameter expFromParam2 = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
        //        SqlParameter expToParam2 = AddParameter("p_experienceTo", expTo, typeof(Int32));
        //        SqlParameter keywordParam2 = AddParameter("p_keyword", keyword, typeof(String));
        //        SqlParameter searchTypeParam2 = AddParameter("p_searchType", 2, typeof(Int32));
        //        SqlParameter pageSizeParam2 = AddParameter("p_pageSize", pageSize, typeof(Int32));
        //        SqlParameter pageIndexParam2 = AddParameter("p_pageIndex", pageIndex, typeof(Int32));
        //        SqlParameter sectorParam2 = AddParameter("p_sector", sectors, typeof(String));


        //        parameters = new object[] { fNameParam2, lNameParam2, ageParam2, genderParam2, qualififcationParam2
        //                                            , salaryFromParam2, salaryToParam2, expFromParam2,expToParam2
        //                                            ,keywordParam2,searchTypeParam2,pageSizeParam2,pageIndexParam2,sectorParam2};
        //        sqlQry = string.Format(@"select count(1) RecordCount
        //           from
        //           (
        //           select ca_Id,
        //                    ca_lm_EducationLevel
        //           from candidates c
        //           {0}
        //           ) CA
        //           left outer join
        //           (
        //           select lm_Id,lm_Value as EducationLevel 
        //           from ListsMaster 
        //           where lm_lt_ListTypeId = 12
        //           ) LM
        //           ON CA.ca_lm_EducationLevel = LM.lm_Id
        //           left outer join
        //           (
        //           select Ca_ID, cast(isnull(sum(round(datediff(dd,fromdate,todate)/365.25,2)),0) as decimal(5,2))  TotalExp 
        //           from CandidateExperience
        //           group by Ca_ID 
        //           ) CE
        //           ON CA.Ca_ID = CE.Ca_ID
        //           {1}
        //           ", whereCand, whereExp);
        //        totalSearchCount = db.Database.SqlQuery<int>(sqlQry, parameters).FirstOrDefault();
        //        //var candidateResultView = new List<CandidateSearchResultModel>();
        //        int cnt = 0;
        //        foreach (var d in candidateResult)
        //        {
        //            SqlParameter ca_id_Param = AddParameter("ca_id", d.ca_Id, typeof(Int32));
        //            parameters = new object[] { ca_id_Param };
        //            sqlQry = "proc_CandidateSearchInterest @ca_id";
        //            var ca_interest = db.Database.SqlQuery<CandidateSearchInerestResultModel>(sqlQry, parameters).FirstOrDefault();
        //            d.Interest = ca_interest.Interest;
        //            candidateResult[cnt].Interest = d.Interest;
        //            cnt++;
        //        }


        //    }
        //    // this is just to test with mock data
        //    //for (int i = 0; i < 100; i++)
        //    //{
        //    //    candidateResult.Add(candidateResult[0]);
        //    //}


        //    var candidateResultFilter = new StaticPagedList<CandidateSearchResultModel>(candidateResult, pageIndex, pageSize, totalSearchCount);


        //    return View(candidateResultFilter);


        //}

        public ActionResult CandidateSearchResult(string keyword, string fname, string lname, int? age, string gender, string[] chksector, int? qualification, string salary, string experience, int? page)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
              //  int companyid = recruiter[0].ct_co_CompanyId;

               // var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

             //   string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
              //  ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

               // ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;
            }
                StringBuilder sb = new StringBuilder();
            decimal? salaryFrom = null, salaryTo = null;
            var spltSalary = salary != null ? salary.Split(':') : null;
            if (spltSalary != null && spltSalary.Count() == 2)
            {
                salaryFrom = decimal.Parse(spltSalary[0]);
                salaryTo = decimal.Parse(spltSalary[1]);
            }

            decimal? expFrom = null, expTo = null;
            var spltExp = experience != null ? experience.Split(':') : null;
            if (spltExp != null && spltExp.Count() == 2)
            {
                expFrom = int.Parse(spltExp[0]);
                expTo = int.Parse(spltExp[1]);
            }
            string sectors = null;
            if (chksector != null)
            {
                foreach (var d in chksector)
                {
                    sectors += d + ",";
                }
                sectors = sectors.Substring(0, sectors.Length - 1);
            }
            int totalSearchCount = 0;
            var pageIndex = (page ?? 1); //set pageIndex to 1 when null
            var pageSize = 10; // max record display per page
            List<CandidateSearchResultModel> candidateResult = null;
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                fname = string.IsNullOrEmpty(fname) ? null : fname;
                lname = string.IsNullOrEmpty(lname) ? null : lname;
                gender = string.IsNullOrEmpty(gender) ? null : gender;
                keyword = string.IsNullOrEmpty(keyword) ? null : keyword;
                sectors = string.IsNullOrEmpty(sectors) ? null : sectors;
                sb.Append(@"");
                bool whereUsed = false;
                bool whereExpUsed = false;
                string whereCand = "";
                string whereExp = "";
                if (!string.IsNullOrEmpty(fname))
                {
                    whereCand = @"where ca_FirstName = ltrim(rtrim(@p_firstname)) ";
                    whereUsed = true;
                }

                if (!string.IsNullOrEmpty(lname))
                {
                    whereCand += whereUsed ? "and ca_Surname = ltrim(rtrim(@p_surname)) " : @"where ca_Surname = ltrim(rtrim(@p_surname)) ";
                    whereUsed = true;
                }

                if (!string.IsNullOrEmpty(gender))
                {
                    whereCand += whereUsed ? "and ca_Gender = ltrim(rtrim(@p_Gender)) " : @"where ca_Gender = ltrim(rtrim(@p_Gender)) ";
                    whereUsed = true;
                }

                if (age != null)
                {
                    //whereCand += whereUsed ? "and ca_Gender = @p_age " : @"where ca_Gender = @p_age ";
                    whereCand += whereUsed ? "and ca_Age = @p_age " : @"where ca_Age = @p_age ";
                    whereUsed = true;
                }

                if (qualification != null)
                {
                    whereCand += whereUsed ? "and ca_lm_EducationLevel = @p_qaulification " : @"where ca_lm_EducationLevel = @p_qaulification ";
                    whereUsed = true;
                }

                if (salaryFrom != null && salaryTo != null)
                {
                    if (salaryFrom != -1)
                    {
                        whereCand += whereUsed ? "and ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)) " : @"where ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)) ";
                    }
                    else
                    {
                        whereCand += whereUsed ? "and ca_SalaryFrom >= @p_salaryto " : @"where ca_SalaryFrom >= @p_salaryto ";

                    }
                    whereUsed = true;
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    whereCand += whereUsed ? "and ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%') " : @"where ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%') ";
                    whereUsed = true;
                }

                if (expFrom != null && expTo != null)
                {
                    if (expFrom != -1)
                    {
                        whereExp += whereExpUsed ? "and  (CE.TotalExp between  @p_experienceFrom and @p_experienceTo) " : @"where (CE.TotalExp between  @p_experienceFrom and @p_experienceTo) ";
                    }
                    else
                    {
                        whereExp += whereExpUsed ? "and CE.TotalExp >= @p_experienceTo " : @"where CE.TotalExp >= @p_experienceTo ";

                    }
                    whereExpUsed = true;
                }

                if (!string.IsNullOrEmpty(sectors))
                {
                      whereCand += whereUsed ? string.Format("and ca_id in ( select ci_ca_CandidateId from CandidateInterests where ci_lm_LinkId in ({0})) ", sectors) : string.Format(@"where ca_id in ( select ci_ca_CandidateId from CandidateInterests where ci_lm_LinkId in ({0})) ", sectors);
                    //whereCand += whereUsed ? string.Format("and ca_id in ( select Ca_ID from CandidateExperience where industry in ({0})) ", sectors) : string.Format(@"where ca_id in ( select Ca_ID from CandidateExperience where industry in  ({0})) ", sectors);
                    whereUsed = true;
                }

                SqlParameter fNameParam = AddParameter("p_firstname", fname, typeof(string));
                SqlParameter lNameParam = AddParameter("p_surname", lname, typeof(string));
                SqlParameter ageParam = AddParameter("p_age", age, typeof(Int32));
                SqlParameter genderParam = AddParameter("p_Gender", gender, typeof(string));
                SqlParameter qualififcationParam = AddParameter("p_qaulification", qualification, typeof(Int32));
                SqlParameter salaryFromParam = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
                SqlParameter salaryToParam = AddParameter("p_salaryto", salaryTo, typeof(decimal));
                SqlParameter expFromParam = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
                SqlParameter expToParam = AddParameter("p_experienceTo", expTo, typeof(Int32));
                SqlParameter keywordParam = AddParameter("p_keyword", keyword, typeof(String));
                SqlParameter sectorParam = AddParameter("p_sector", sectors, typeof(String));
                SqlParameter searchTypeParam = AddParameter("p_searchType", 1, typeof(Int32));
                SqlParameter pageSizeParam = AddParameter("p_pageSize", pageSize, typeof(Int32));
                SqlParameter pageIndexParam = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

                object[] parameters = new object[] { fNameParam, lNameParam, ageParam, genderParam, qualififcationParam
                                                    , salaryFromParam, salaryToParam, expFromParam,expToParam
                                                    ,keywordParam,searchTypeParam,pageSizeParam,pageIndexParam,sectorParam};
                string sqlQry = @"proc_CandidateSearch @p_firstname,@p_surname,@p_age,@p_Gender,@p_qaulification
                                  ,@p_salaryfrom,@p_salaryto,@p_experienceFrom,@p_experienceTo,@p_keyword,@p_searchType,
                                  @p_pageSize,@p_pageIndex,@p_sector";
                sqlQry = string.Format(@"select CA.*,CE.TotalExp,LM.EducationLevel
			                from
			                (
			                select  ca_FirstName + ' ' + ca_Surname ca_FullName ,
					                ca_EmailAddress,
					                ca_PhoneMobile,
					                case ca_Gender
					                when 'M' then 'Male'
					                when 'F' then 'Female'
					                else '' end as ca_Gender,
					                ca_DateOfBirth,
					                ca_lm_EducationLevel,
					                ca_SalaryFrom,
					                ca_SalaryTo,
					                ca_Id,
					                dbo.fun_GetAge(ca_DateOfBirth) ca_Age,
					                ca_Profile,
					                dbo.fun_GetProfilePic(ca_GUID,ca_PhotoExtension) LogoURL
			
			                from candidates c
			                {0}
			                ) CA
			                left outer join
			                (
			                select lm_Id,lm_Value as EducationLevel 
			                from ListsMaster 
			                where lm_lt_ListTypeId = 12
			                ) LM
			                ON CA.ca_lm_EducationLevel = LM.lm_Id
			                left outer join
			                (
			                select Ca_ID, cast(isnull(sum(round(datediff(dd,fromdate,todate)/365.25,2)),0) as decimal(5,2))  TotalExp 
			                from CandidateExperience
			                group by Ca_ID 
			                ) CE
			                ON CA.Ca_ID = CE.Ca_ID
			                {1}
			                order by Ca_ID
			                OFFSET @p_pageSize  * (@p_pageIndex -1) rows fetch next @p_pageSize ROWS ONLY", whereCand, whereExp);
                /*
                  sqlQry = @"select CA.*,CE.TotalExp,LM.EducationLevel
                              from
                              (
                              select  ca_Surname + ' ' + ca_FirstName ca_FullName ,
                                      ca_EmailAddress,
                                      ca_PhoneMobile,
                                      case ca_Gender
                                      when 'M' then 'Male'
                                      when 'F' then 'Female'
                                      else '' end as ca_Gender,
                                      ca_DateOfBirth,
                                      ca_lm_EducationLevel,
                                      ca_SalaryFrom,
                                      ca_SalaryTo,
                                      ca_Id,
                                      dbo.fun_GetAge(ca_DateOfBirth) ca_Age,
                                      ca_Profile,
                                      dbo.fun_GetProfilePic(ca_GUID,ca_PhotoExtension) LogoURL

                              from candidates c
                              where (@p_firstname is null or ca_FirstName = ltrim(rtrim(@p_firstname)))
                                and (@p_surname is null or ca_Surname = ltrim(rtrim(@p_surname)))
                                and (@p_Gender is null or ca_Gender = @p_Gender)
                                and (@p_age is null or dbo.fun_GetAge(ca_DateOfBirth) = @p_age)
                                and (@p_qaulification is null or ca_lm_EducationLevel = @p_qaulification)
                                and ((@p_salaryfrom is null and @p_salaryto is null) or ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)))
                                and (@p_keyword is null) or ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%')
                              ) CA
                              left outer join
                              (
                              select lm_Id,lm_Value as EducationLevel 
                              from ListsMaster 
                              where lm_lt_ListTypeId = 12
                              ) LM
                              ON CA.ca_lm_EducationLevel = LM.lm_Id
                              left outer join
                              (
                              select Ca_ID, cast(isnull(sum(round(datediff(dd,fromdate,todate)/365.25,2)),0) as decimal(5,2))  TotalExp 
                              from CandidateExperience
                              group by Ca_ID 
                              ) CE
                              ON CA.Ca_ID = CE.Ca_ID
                              where ((@p_experienceFrom is null and @p_experienceTo is null) or (CE.TotalExp between  @p_experienceFrom and @p_experienceTo))
                              order by Ca_ID
                              OFFSET @p_pageSize  * (@p_pageIndex -1) rows fetch next @p_pageSize ROWS ONLY";
                  */

                candidateResult = db.Database.SqlQuery<CandidateSearchResultModel>(sqlQry, parameters).ToList();

                // getting record count for the search filter
                SqlParameter fNameParam2 = AddParameter("p_firstname", fname, typeof(string));
                SqlParameter lNameParam2 = AddParameter("p_surname", lname, typeof(string));
                SqlParameter ageParam2 = AddParameter("p_age", age, typeof(Int32));
                SqlParameter genderParam2 = AddParameter("p_Gender", gender, typeof(string));
                SqlParameter qualififcationParam2 = AddParameter("p_qaulification", qualification, typeof(Int32));
                SqlParameter salaryFromParam2 = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
                SqlParameter salaryToParam2 = AddParameter("p_salaryto", salaryTo, typeof(decimal));
                SqlParameter expFromParam2 = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
                SqlParameter expToParam2 = AddParameter("p_experienceTo", expTo, typeof(Int32));
                SqlParameter keywordParam2 = AddParameter("p_keyword", keyword, typeof(String));
                SqlParameter searchTypeParam2 = AddParameter("p_searchType", 2, typeof(Int32));
                SqlParameter pageSizeParam2 = AddParameter("p_pageSize", pageSize, typeof(Int32));
                SqlParameter pageIndexParam2 = AddParameter("p_pageIndex", pageIndex, typeof(Int32));
                SqlParameter sectorParam2 = AddParameter("p_sector", sectors, typeof(String));


                parameters = new object[] { fNameParam2, lNameParam2, ageParam2, genderParam2, qualififcationParam2
                                                    , salaryFromParam2, salaryToParam2, expFromParam2,expToParam2
                                                    ,keywordParam2,searchTypeParam2,pageSizeParam2,pageIndexParam2,sectorParam2};
                sqlQry = string.Format(@"select count(1) RecordCount
			                from
			                (
			                select ca_Id,
                            ca_lm_EducationLevel
			                from candidates c
			                {0}
			                ) CA
			                left outer join
			                (
			                select lm_Id,lm_Value as EducationLevel 
			                from ListsMaster 
			                where lm_lt_ListTypeId = 12
			                ) LM
			                ON CA.ca_lm_EducationLevel = LM.lm_Id
			                left outer join
			                (
			                select Ca_ID, cast(isnull(sum(round(datediff(dd,fromdate,todate)/365.25,2)),0) as decimal(5,2))  TotalExp 
			                from CandidateExperience
			                group by Ca_ID 
			                ) CE
			                ON CA.Ca_ID = CE.Ca_ID
			                {1}
			                ", whereCand, whereExp);
                totalSearchCount = db.Database.SqlQuery<int>(sqlQry, parameters).FirstOrDefault();
                //var candidateResultView = new List<CandidateSearchResultModel>();
                int cnt = 0;
                foreach (var d in candidateResult)
                {
                    SqlParameter ca_id_Param = AddParameter("ca_id", d.ca_Id, typeof(Int32));
                    parameters = new object[] { ca_id_Param };
                    sqlQry = "proc_CandidateSearchInterest @ca_id";
                    var ca_interest = db.Database.SqlQuery<CandidateSearchInerestResultModel>(sqlQry, parameters).FirstOrDefault();
                    d.Interest = ca_interest.Interest;
                    candidateResult[cnt].Interest = d.Interest;
                    cnt++;
                }


            }
            // this is just to test with mock data
            //for (int i = 0; i < 100; i++)
            //{
            //    candidateResult.Add(candidateResult[0]);
            //}


            var candidateResultFilter = new StaticPagedList<CandidateSearchResultModel>(candidateResult, pageIndex, pageSize, totalSearchCount);


            return View(candidateResultFilter);


        }


        public SqlParameter AddParameter(string parameterName, object value, Type type)
        {
            SqlParameter param;
            if (type == typeof(string))
            {
                param = new SqlParameter(parameterName, DbType.String);
                if (value == null)
                {
                    param.Value = DBNull.Value;
                }
                else
                {
                    param.Value = value;
                }
                return param;
            }
            else if (type == typeof(DateTime))
            {
                param = new SqlParameter(parameterName, DbType.DateTime);
                if (value == null)
                {
                    param.Value = DBNull.Value;
                }
                else
                {
                    param.Value = value;
                }
                return param;
            }
            else if (type == typeof(Int32))
            {
                param = new SqlParameter(parameterName, DbType.Int32);
                if (value == null)
                {
                    param.Value = DBNull.Value;
                }
                else
                {
                    param.Value = value;
                }
                return param;
            }
            else if (type == typeof(decimal))
            {
                param = new SqlParameter(parameterName, DbType.Decimal);
                if (value == null)
                {
                    param.Value = DBNull.Value;
                }
                else
                {
                    param.Value = value;
                }
                return param;
            }
            else
            {
                return new SqlParameter(parameterName, value);

            }

        }
        public ActionResult CandidateSearchDetail(int ca_id = 0)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
         

            int candidateid = ca_id; //  Convert.ToInt32(Session["Ca_ID"]);
            ViewBag.Candidateid = candidateid;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                if (candidate.Count == 0)
                {
                    candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                }

                if (candidate[0].ca_HasPhoto == true)
                {
                    ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

                }
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.CName = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
                ViewBag.fName = candidate[0].ca_FirstName;
                ViewBag.lName = candidate[0].ca_Surname;
                ViewBag.Phone = candidate[0].ca_PhoneMobile;
                ViewBag.Email = candidate[0].ca_EmailAddress;
                ViewBag.DOB = Convert.ToDateTime(candidate[0].ca_DateOfBirth).ToString("dd-MM-yyyy");
                var selectedGender = candidate[0].ca_Gender;
                ViewBag.Genders = new SelectList(new List<SelectListItem> { new SelectListItem { Text = "Male", Value = "M" }, new SelectListItem { Text = "Female", Value = "F" }, }, "Value", "Text", selectedGender);





                ViewBag.CandiateProfile = candidate.ToList();

                var candidatedocument = db.CandidateDocuments.Where(x => x.cd_ca_CandidateId == candidateid).ToList();
                if (candidatedocument.Count > 0)
                {

                    ViewBag.CVpath = "http://www.jobs4bahrainis.com/cvdx/" + candidatedocument[0].cd_Guid.ToString().Substring(0, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(2, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(4, 2) + "/" + candidatedocument[0].cd_Guid.ToString() + "." + candidatedocument[0].cd_FileExtension;
                }


                ViewBag.Education = db.Database.SqlQuery<CandidateEducation>("SELECT * from [dbo].[CandidateEducation]  where ca_id=" + candidateid).ToList();
                ViewBag.Experience = db.Database.SqlQuery<CandidateExperience>("SELECT * from [dbo].[CandidateExperience]  where ca_id=" + candidateid).ToList();
                ViewBag.Skills = db.Database.SqlQuery<CandidateSkill>("SELECT * from [dbo].[CandidateSkills]  where ca_id=" + candidateid).ToList();
                ViewBag.Skill_level = new SelectList(new List<SelectListItem> { new SelectListItem { Text = "Beginner", Value = "Beginner" }, new SelectListItem { Text = "Intermediate", Value = "Intermediate" }, new SelectListItem { Text = "Expert", Value = "Expert" }, new SelectListItem { Text = "Advanced", Value = "Advanced" } }, "Value", "Text");

                var Interests = db.Database.SqlQuery<ListsMaster>("select * from listsmaster where lm_id in(select ci_lm_linkid from CandidateInterests where ci_ca_candidateid=" + candidateid + ") and lm_lt_listtypeid=4").ToList();

                string intrst = "";
                foreach (var i in Interests)
                {
                    intrst += i.lm_Value + " , ";
                }
                if (Interests.Count > 0)
                {
                    ViewBag.Interests = intrst.Remove(intrst.Length - 2);
                }


            }



            return View();
        }

        public ActionResult AboutUs()
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                if (Session["Ca_ID"] != null)
                {
                    int candidateid = Convert.ToInt32(Session["Ca_ID"]);


                    var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                    if (candidate.Count == 0)
                    {
                        candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                    }

                    if (candidate[0].ca_HasPhoto == true)
                    {
                        ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

                    }

                    ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
                }
                else if (Session["RecruiterID"] != null)
                {
                    int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
                    var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                    int companyid = recruiter[0].ct_co_CompanyId;

                    var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                    string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                    ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                    ViewBag.Name = recruiter[0].ct_Forename;
                }
            }

            return View();
        }

        public ActionResult CVChecklist()
        {
            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["Ca_ID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                if (candidate.Count == 0)
                {
                    candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                }

                if (candidate[0].ca_HasPhoto == true)
                {
                    ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

                }

                ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;

            }
                return View();
        }
        public ActionResult CVTips()
        {
            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["Ca_ID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                if (candidate.Count == 0)
                {
                    candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                }

                if (candidate[0].ca_HasPhoto == true)
                {
                    ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

                }

                ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;

            }
            return View();
        }

        public ActionResult CVWriting()
        {
            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["Ca_ID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                if (candidate.Count == 0)
                {
                    candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                }

                if (candidate[0].ca_HasPhoto == true)
                {
                    ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

                }

                ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;

            }
            return View();
        }
        


        public ActionResult ContactUs()
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                if (Session["Ca_ID"] != null)
                {
                    int candidateid = Convert.ToInt32(Session["Ca_ID"]);


                    var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                    if (candidate.Count == 0)
                    {
                        candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                    }

                    if (candidate[0].ca_HasPhoto == true)
                    {
                        ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

                    }

                    ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
                }
                else if (Session["RecruiterID"] != null)
                {
                    int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
                    var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                    int companyid = recruiter[0].ct_co_CompanyId;

                    var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                    string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                    ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                    ViewBag.Name = recruiter[0].ct_Forename;
                }
            }


            return View();
        }

        public ActionResult RecruiterContact()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;


            }
            return View();
        }
        public ActionResult RecruiterAlert()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;


            }
            return View();
        }




        public ActionResult MediaCenter()
        {

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                if (Session["Ca_ID"] != null)
                {
                    int candidateid = Convert.ToInt32(Session["Ca_ID"]);


                    var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                    if (candidate.Count == 0)
                    {
                        candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                    }

                    if (candidate[0].ca_HasPhoto == true)
                    {
                        ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

                    }

                    ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
                }
                else if (Session["RecruiterID"] != null)
                {
                    int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
                    var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                    int companyid = recruiter[0].ct_co_CompanyId;

                    var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                    string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                    ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                    ViewBag.Name = recruiter[0].ct_Forename;
                }
            }

            return View();
        }

       

       

        public ActionResult Registration()
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                
                List<Nationality> list = db.Database.SqlQuery<Nationality>("select lm_Id,lm_Value from ListsMaster where lm_lt_ListTypeId=13 and lm_Deleted is null").ToList();
                ViewBag.nationality = new SelectList(list, "lm_Id", "lm_Value");

                ViewBag.Genders = new SelectList(new List<SelectListItem> { new SelectListItem { Text = "Male", Value = "M" }, new SelectListItem { Text = "Female", Value = "F" }, }, "Value", "Text");
                ViewBag.Skill_level = new SelectList(new List<SelectListItem> { new SelectListItem { Text = "Beginner", Value = "Beginner" }, new SelectListItem { Text = "Intermediate", Value = "Intermediate" }, new SelectListItem { Text = "Expert", Value = "Expert" }, new SelectListItem { Text = "Advanced", Value = "Advanced" } }, "Value", "Text");

            }

           
            return View();
        }










        public string urlcleaner(string originalstr)
        {
            string cleanstr = string.Empty;
            if (originalstr != null)
            {
                cleanstr = originalstr.Replace(" ", "-").Replace(".", "").Replace("?", "").Replace("&", "").Replace("'", "").Replace("!", "").Replace(":", "").Replace("%", "").Replace(";", "").Replace("*", "").Replace("\"", "").Replace("/", "");
            }

            return cleanstr;
        }

        public string fparamaker(string fpara, int noofwords)
        {
            string[] fparax = fpara.Split(' ');

            string newfpara = "";

            for (int x = 0; x <= noofwords; x++)
            {
                newfpara = newfpara + " " + fparax[x];
            }


            return newfpara.Trim();
        }



        //[HttpPost]
        //public ActionResult JobSeekerProfile(CandidateSkills model)
        //{


        //    Jobs4bahrainisEntities db = new Jobs4bahrainisEntities();


        //    CandidateSkill ca_skill = new CandidateSkill();
        //    ca_skill.Skills = model.Skills;
        //    ca_skill.SkillLevel = model.SkillLevel;

        //    db.CandidateSkills.Add(ca_skill);
        //    db.SaveChanges();
        //    return View();



        //}

        //[HttpPost]
        //public ActionResult UpdateProfile(FormCollection col, string Command)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return PartialView("_EditEmployee", emp);
        //    }
        //    else
        //    {
        //        Employee empObj = new Employee();
        //        empObj.ID = emp.Id;
        //        empObj.Emp_ID = emp.Emp_ID;
        //        empObj.Name = emp.Name;
        //        empObj.Dept = emp.Dept;
        //        empObj.City = emp.City;
        //        empObj.State = emp.State;
        //        empObj.Country = emp.Country;
        //        empObj.Mobile = emp.Mobile;

        //        bool IsSuccess = mobjModel.UpdateEmployee(empObj);
        //        if (IsSuccess)
        //        {
        //            TempData["OperStatus"] = "Employee updated succeessfully";
        //            ModelState.Clear();
        //            return RedirectToAction("SearchEmployee", "ManageEmployee");
        //        }
        //    }

        //    return PartialView("_EditEmployee");
        //}

        [HttpPost]
        public ActionResult AddCandidateWorkHistory(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string Ca_ID = Convert.ToString(col["Ca_ID"]);
                string JobTitle = Convert.ToString(col["JobTitle"]);
                string Company = Convert.ToString(col["Company"]);
                string TotalExp = Convert.ToString(col["Experience"]);
                string City = Convert.ToString(col["City"]);
                string Country = Convert.ToString(col["Country"]);



                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("insert into [CandidateExperience] (Ca_ID,JobTitle,Company,TotalExp,City,Country) values({0},'{1}','{2}','{3}','{4}','{5}')", Convert.ToInt32(Ca_ID), Convert.ToString(JobTitle), Convert.ToString(Company), Convert.ToString(TotalExp), Convert.ToString(City), Convert.ToString(Country));
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        [HttpPost]
        public ActionResult UpdateCandidateProfileSummary(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string Ca_ID = Convert.ToString(col["Ca_ID"]);
                string ca_FirstName = Convert.ToString(col["ca_FirstName"]);
                string ca_Surname = Convert.ToString(col["ca_Surname"]);
                string ca_DateOfBirth = Convert.ToString(col["ca_DateOfBirth"]);
                string ca_Gender = Convert.ToString(col["ca_Gender"]);
                string ca_PhoneMobile = Convert.ToString(col["ca_PhoneMobile"]);
                string ca_EmailAddress = Convert.ToString(col["ca_EmailAddress"]);
                //string ca_lm_EducationLevel = Convert.ToString(col["ca_lm_EducationLevel"]);
                string ca_lm_EducationLevel = "773";
                string ca_SalaryFrom = Convert.ToString(col["ca_SalaryFrom"]);
                string ca_Profile = Convert.ToString(col["ca_Profile"]);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("update [Candidates] SET ca_FirstName='{1}',ca_Surname='{2}',ca_DateOfBirth='{3}',ca_Gender='{4}',ca_PhoneMobile='{5}',ca_EmailAddress='{6}',ca_lm_EducationLevel='{7}',ca_SalaryFrom='{8}',ca_Profile='{9}' WHERE Ca_ID= {0}",
                    Convert.ToInt32(Ca_ID), Convert.ToString(ca_FirstName), Convert.ToString(ca_Surname), Convert.ToDateTime(ca_DateOfBirth), Convert.ToString(ca_Gender), Convert.ToString(ca_PhoneMobile), Convert.ToString(ca_EmailAddress), Convert.ToInt32(ca_lm_EducationLevel), Convert.ToString(ca_SalaryFrom), Convert.ToString(ca_Profile));
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }
        [HttpPost]
        public ActionResult AddCandidateDegree(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string Ca_ID = Convert.ToString(col["Ca_ID"]);
                string DegreeLevel = Convert.ToString(col["DegreeLevel"]);
                string MajorSubject = Convert.ToString(col["MajorSubject"]);
                string Institute = Convert.ToString(col["Institute"]);
                string Grade = Convert.ToString(col["Grade"]);
                string State = Convert.ToString(col["State"]);
                string Country = Convert.ToString(col["c"]);



                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("insert into [CandidateEducation] (Ca_ID,DegreeLevel,MajorSubject,Institute,Grade,State,Country) values({0},'{1}','{2}','{3}','{4}','{5}','{6}')", Convert.ToInt32(Ca_ID), Convert.ToString(DegreeLevel), Convert.ToString(MajorSubject), Convert.ToString(Institute), Convert.ToString(Grade), Convert.ToString(State), Convert.ToString(Country));
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult DeleteCandidateExperince(int caExperince)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from [CandidateExperience] where Ca_ID={0} and CaExpID={1}", Convert.ToInt32(Session["Ca_ID"]), caExperince);
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult DeleteCandidateDegree(int CaEdu_ID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from [CandidateEducation] where Ca_ID={0} and CaEdu_ID={1}", Convert.ToInt32(Session["Ca_ID"]), CaEdu_ID);
                db.Database.ExecuteSqlCommand(sb.ToString());
                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }
    }

    public class JsonpResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;
            string jsoncallback = (context.RouteData.Values["jsoncallback"] as string) ?? request["jsoncallback"];
            if (!string.IsNullOrEmpty(jsoncallback))
            {
                if (string.IsNullOrEmpty(base.ContentType))
                {
                    base.ContentType = "application/x-javascript";
                }
                response.Write(string.Format("{0}((", jsoncallback));
            }
            base.ExecuteResult(context);
            if (!string.IsNullOrEmpty(jsoncallback))
            {
                response.Write("))");
            }
        }
    }
}