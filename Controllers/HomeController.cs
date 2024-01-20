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
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
 

namespace Jobs4Bahrainis.Controllers
{
    public class
        HomeController : Controller
    {
        public ActionResult Index()
        {

            if (Session["IsActivated"] != null) // non activated candidate
            {
                Session["Ca_ID"] = null;
                Session["CandidateName"] = null;
            }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                ViewBag.Sectors = db.ListsMasters.Where(x => x.lm_lt_ListTypeId == 4 && x.lm_Deleted == null).OrderBy(x => x.lm_Value).ToList();

                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(@"select top 10 vc_co_CompanyId,vc_Confidential, vc_Id,co_id,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension,vc_views,vc_applications from Vacancies v
            inner join Companies on vc_co_CompanyId=co_Id 
            join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
            where vc_Deleted is null and vc_st_StatusID=1 and vc_ExpiryDate>=getdate() order by vc_lastupdated desc").ToList();


                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;
                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location;
                    ljx.vc_Confidential = lj.vc_Confidential;

                    ljx.vc_Description = StripHTML(lj.vc_Description).Substring(0, 80);

                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;

                    ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


                    //int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    ljx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    ljx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    ljx.postedsince = "" + days + " days ago";
                    //}

                    ljx.postedsince = lj.vc_Created.ToString("dd-MM-yyyy");

                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    ljx.vc_views = lj.vc_views;
                    ljx.vc_applications = lj.vc_applications;

                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToList();


                CandiCount candicount = db.Database.SqlQuery<CandiCount>("select count(*) as CandiCounter from candidates where ca_deleted is null").SingleOrDefault();

                ViewBag.candicount = candicount.CandiCounter;

                CandiCount VisitorCout = db.Database.SqlQuery<CandiCount>("update settings_t set SettingValue = SettingValue + 1 where id = 1;select SettingValue as CandiCounter from settings_T where id=1").SingleOrDefault();

                ViewBag.VisitorCount = VisitorCout.CandiCounter;


            }


            return View();
        }




        public ActionResult JobFair() {

            return View();
        }

        [HttpPost]
        public ActionResult JobFair(FormCollection col, HttpPostedFileBase file)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                JobFair_T jf = new JobFair_T();

                jf.Name = Convert.ToString(col["Name"]);
                jf.Email = Convert.ToString(col["Email"]);
                jf.Mobile = Convert.ToString(col["Mobile"]);
                db.JobFair_T.Add(jf);
                db.SaveChanges();

               

                if (file != null)
                {
                    string filx = file.FileName;
                    string basepath = "~/Documents/JobFair/" + jf.ID + "-" + filx;

                    jf.CV = basepath;
                    db.Entry(jf).Property(x => x.CV).IsModified = true;
                    db.SaveChanges();
                    file.SaveAs(Server.MapPath(basepath));
                }
            }

            ViewBag.Regd = "YES";
            return View();
        }




        public void CandidateActivationMail(int Ca_ID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                JobSeekers candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + Ca_ID).SingleOrDefault();
                string path = Server.MapPath("~/Templates/CandidateActivation.html");
                string bodycontent = System.IO.File.ReadAllText(path);
                bodycontent = bodycontent.Replace("##activationmaillink##", "http://www.jobs4bahrainis.com/ActivateCandidate/" + candidate.ca_GUID + "/WelcometoJ4B");
                common cmn = new common();

                cmn.SendMail(candidate.ca_EmailAddress, "Account Activation - J4B Website", bodycontent);
            }

        }




        public ActionResult ResendCandidateActivationMail(int Ca_ID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                JobSeekers candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + Ca_ID).SingleOrDefault();
                string path = Server.MapPath("~/Templates/CandidateActivation.html");
                string bodycontent = System.IO.File.ReadAllText(path);
                bodycontent = bodycontent.Replace("##activationmaillink##", "http://www.jobs4bahrainis.com/ActivateCandidate/" + candidate.ca_GUID + "/WelcometoJ4B");
                common cmn = new common();

                cmn.SendMail(candidate.ca_EmailAddress, "Account Activation - J4B Website", bodycontent);
                cmn.SendMail("binju@northstar.bh", "Account Activation - J4B Website", bodycontent);
            }

            return View();
        }







        public ActionResult NonCompletedRegistrations()
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<CandidateNew> ca = db.Database.SqlQuery<CandidateNew>(@"select ca_id,ca_guid,ca_created,ca_lastupdated,ca_firstname,ca_surname,ca_emailaddress,ca_password,ca_active,ca_completedregtabs,ca_emailactivated,ca_lm_CountryId,
ca_emailactivatedon  from candidates where ca_completedregtabs<6 and ca_active=0 and ca_lastupdated>'2018-10-12 00:00:00.200' and ca_lastupdated<'2018-10-21 00:00:00.200'").ToList();

                common cmn = new common();
                string path = Server.MapPath("~/Templates/Completeyourprofile.html");

                string emaillinklist = "";


                foreach (var c in ca)
                {
                    string bodycontent = System.IO.File.ReadAllText(path);
                    bodycontent = bodycontent.Replace("##continuationmaillink##", "http://www.jobs4bahrainis.com/CanRegContinue/" + c.ca_GUID + "/WelcometoJ4B");
                    cmn.SendMail(c.ca_EmailAddress, "A few more steps to complete your profile - J4B Website", bodycontent);
                    emaillinklist += "<br><a href='http://www.jobs4bahrainis.com/CanRegContinue/" + c.ca_GUID + "/WelcometoJ4B'>" + c.ca_GUID + "</a>";
                }

                ViewBag.emaillinklist = emaillinklist;
            }

            return View();
        }

        public ActionResult CanRegContinue(Guid Ca_Guid, string welcome)
        {

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                CandidateNew cn = db.Database.SqlQuery<CandidateNew>("select *  from candidates where Ca_Guid='" + Ca_Guid + "'").SingleOrDefault();

                if (cn != null)
                {
                    Session["Ca_ID"] = cn.Ca_ID;
                    Session["ReginProgress"] = "YES";
                    return RedirectToAction("../CanRegProfileSummary");

                }
                else
                {
                    return RedirectToAction("../Login");
                }


            }
        }




        public ActionResult ActivateEmployer(Guid co_Guid, string welcome)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update Companies set co_activated=1 where  co_Guid='" + co_Guid + "'");
                reccontacts contact = db.Database.SqlQuery<reccontacts>("select ct_co_CompanyId,ct_EmailAddress from contacts where ct_co_companyid in(select co_id from companies where co_guid='" + co_Guid + "') and ct_superuser=1").SingleOrDefault();

                string path = Server.MapPath("~/Templates/EmailVerifiedRecruiterThanks.html");
                string bodycontent = System.IO.File.ReadAllText(path);

                common cmn = new common();
                cmn.SendMail(contact.ct_EmailAddress, "Jobs4Bahrainis.com", bodycontent);

                
                path = Server.MapPath("~/Templates/EmployerVerifiedEmail.html");
                bodycontent = System.IO.File.ReadAllText(path);

                bodycontent = bodycontent.Replace("##linktoemployerprofile##", "http://www.jobs4bahrainis.com/Employer/" + contact.ct_co_CompanyId + "/J4B");

                cmn.SendMail("Bayden.tierney@jobs4bahrainis.com", "Employer Registered and Email Verified  - J4B Website", bodycontent);
                cmn.SendMail("binju.paul@tradearabia.net", "Employer Registered and Email Verified  - J4B Website", bodycontent);
                cmn.SendMail("mohamed.Ghazwan@jobs4bahrainis.com", "Employer Registered and Email Verified  - J4B Website", bodycontent);




            }
            return View();
        }



        public ActionResult ActivateCandidate(Guid Ca_Guid, string welcome)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                CandidateNew cn = db.Database.SqlQuery<CandidateNew>("select *  from candidates where Ca_Guid='" + Ca_Guid + "'").SingleOrDefault();

                if (cn.Ca_EmailActivated != true)
                {

                    db.Database.ExecuteSqlCommand("Update Candidates set Ca_EmailActivated=1,Ca_EmailActivatedOn=getdate() where Ca_Guid='" + Ca_Guid + "'");


                    string path = Server.MapPath("~/Templates/CandiateVerifiedEmail.html");
                    string bodycontent = System.IO.File.ReadAllText(path);
                    bodycontent = bodycontent.Replace("##linktocandidateprofile##", "http://www.jobs4bahrainis.com/BackOfficeJobSeekerProfile/" + Ca_Guid);
                    common cmn = new common();

                    cmn.SendMail("Bayden.tierney@gulfconnexions.com", "Candidate Registered and Verified  - J4B Website", bodycontent);
                    cmn.SendMail("binju.paul@tradearabia.net", "Candidate Registered and Verified  - J4B Website", bodycontent);
                    cmn.SendMail("mohamed.Ghazwan@jobs4bahrainis.com", "Candidate Registered and Verified  - J4B Website", bodycontent);



                    bodycontent = "";
                    path = Server.MapPath("~/Templates/J4BThankYouEmail.html");
                    bodycontent = System.IO.File.ReadAllText(path);

                    cmn.SendMail(cn.ca_EmailAddress, "Jobs4Bahrainis.com", bodycontent);
                }
                else
                {
                    return RedirectToAction("../Login");
                }

            }

            return View();

        }



        public ActionResult menulogin()
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

            return PartialView();
        }


        public ActionResult Login(string loggedin, string returnurl)
        {
            Session["RecruiterID"] = null;
            Session["RecruiterName"] = null;
            Session["CompanyID"] = null;
            Session["Recruiter1"] = null;
            Session["Recruiter2"] = null;

            Session["Ca_ID"] = null;
            Session["CandidateName"] = null;

            if (loggedin == "invalid")
            {
                ViewBag.LoggedIn = "Invalid Email ID / Password / Non-Actived Account ! try again.";
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

                    int companyid = Convert.ToInt32(EmpLoggedIn[0].ct_co_CompanyId);
                    var company = db.Companies.Where(x => x.co_Id == companyid && x.co_activated == true).ToList();
                      

                    if (company.Count == 0)
                    {
                        return RedirectToAction("Login/invalid");
                    }


                    Session["RecruiterID"] = Convert.ToInt32(EmpLoggedIn[0].ct_Id);
                    Session["RecruiterName"] = Convert.ToString(EmpLoggedIn[0].ct_Forename);
                    Session["CompanyID"] = Convert.ToInt32(EmpLoggedIn[0].ct_co_CompanyId);

                    dbOperations dbo = new dbOperations();
                    dbo.ReportAdd(Convert.ToInt32(EmpLoggedIn[0].ct_co_CompanyId), Convert.ToInt32(EmpLoggedIn[0].ct_Id), 1);

                    string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                    Session["Logo"] = "Documents/logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;


                    db.Database.ExecuteSqlCommand("Update Contacts set ct_lastlogin=getdate() where ct_id=" + Session["RecruiterID"]);

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
                    var JobSeekerLoggedIn = db.Candidates.Where(x => x.ca_EmailAddress == email && x.ca_Password == password && x.ca_Deleted == null && x.Ca_EmailActivated == true).ToList();

                    if (JobSeekerLoggedIn.Count > 0)
                    {
                        db.Database.ExecuteSqlCommand("update candidates set ca_lastlogin=getdate() where ca_id=" + JobSeekerLoggedIn[0].ca_Id);

                        //if (JobSeekerLoggedIn[0].ca_Active == false)
                        //{
                        //    Session["Ca_ID"] = JobSeekerLoggedIn[0].ca_Id;
                        //    Session["IsActivated"] = "NO";

                        //    return RedirectToAction("../CanRegProfileSummary");

                        //}

                        Session["Ca_ID"] = Convert.ToInt32(JobSeekerLoggedIn[0].ca_Id);
                        Session["CandidateName"] = Convert.ToString(JobSeekerLoggedIn[0].ca_FirstName) + " " + Convert.ToString(JobSeekerLoggedIn[0].ca_Surname);


                        if (JobSeekerLoggedIn[0].ca_HasPhoto == true)
                        {
                            Session["Profilepic"] = "~/documents/photos/" + JobSeekerLoggedIn[0].ca_GUID.ToString().Substring(0, 2) + "/" + JobSeekerLoggedIn[0].ca_GUID.ToString().Substring(2, 2) + "/" + JobSeekerLoggedIn[0].ca_GUID.ToString().Substring(4, 2) + "/" + JobSeekerLoggedIn[0].ca_GUID.ToString() + "." + JobSeekerLoggedIn[0].ca_PhotoExtension;

                        }



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

        public ActionResult NonActivated()
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

        [HttpPost]
        public ActionResult NewsletterSubscription(FormCollection col)
        {
            String emailid = Convert.ToString(col["address"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                try
                {
                    db.Database.ExecuteSqlCommand("insert into NewsletterSubscribers_T (EmailID) values('" + emailid + "')");
                }
                catch
                {

                }
            }

            return View();
        }




        public ActionResult CanRegProfileSummary(string tabupdate)
        {
            if (Session["Ca_ID"] == null) { return RedirectToAction("RegisterJobseekerLogin"); }




            ViewBag.curtabno = 1;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                int tabno = db.Database.SqlQuery<int>("select ca_CompletedRegTabs from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();
                ViewBag.tabno = tabno;

                if (tabupdate != "tabupdate" && tabno != 0)
                {
                    return RedirectToAction("../" + CurTabFinder(tabno));
                }

                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderByDescending(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();

               





                var candidate = db.Database.SqlQuery<CandidateNew>(@"select ca_GUID,ca_PhotoExtension,ca_IsFresher, ca_HasPhoto,c.ca_lm_countryid,ca_lm_EducationLevel,ca_UniversityID,Ca_SectorIndustryID,ca_FunctionTitleID,ca_CurrentJobTitleID,ca_TotalRelavantExperience,c.Ca_CurrentCountryID, c.ca_FirstName,c.ca_Surname,lmcona.lm_Value as Nationality, ca_PhoneMobile,ca_EmailAddress,ca_Password, c.ca_DateOfBirth, c.ca_Gender, c.ca_MaritalStatus,
lmcurcon.lm_Value as CurrentLocation,lmedu.lm_Value as HighestEducation,lmuni.lm_Value as University, lmSecInd.lm_Value as SectorIndustry,
lmfun.lm_Value as FunctionTitle, lmjt.lm_Value as JobTitle, lmexp.lm_Value as TotalExperience, c.ca_Profile
from Candidates c
join ListsMaster lmcurcon on lmcurcon.lm_Id = c.ca_CurrentCountryID
join ListsMaster lmedu on lmedu.lm_Id = c.ca_lm_EducationLevel
join ListsMaster lmuni on lmuni.lm_Id = c.ca_UniversityID
join ListsMaster lmSecInd on lmSecInd.lm_Id = c.Ca_SectorIndustryID
join ListsMaster lmfun on lmfun.lm_Id = c.ca_FunctionTitleID
join ListsMaster lmjt on lmjt.lm_Id = c.ca_CurrentJobTitleID
join ListsMaster lmexp on lmexp.lm_Id = c.ca_TotalRelavantExperience
join ListsMaster lmcona on lmcona.lm_Id = c.ca_lm_CountryId
where c.ca_id=" + Convert.ToInt32(Session["Ca_ID"]) + "").ToList();



                if (candidate.Count != 0)
                {

                    ViewBag.MaritalStatus = candidate[0].ca_MaritalStatus;
                    ViewBag.DOB = Convert.ToDateTime(candidate[0].ca_DateOfBirth).ToString("dd/MM/yyyy");
                    ViewBag.ca_lm_countryid = candidate[0].ca_lm_countryid;
                    ViewBag.ca_Gender = candidate[0].ca_Gender;
                    ViewBag.ca_MaritalStatus = candidate[0].ca_MaritalStatus;
                    ViewBag.Ca_CurrentCountryID = candidate[0].Ca_CurrentCountryID;
                    ViewBag.ca_lm_EducationLevel = candidate[0].ca_lm_EducationLevel;
                    ViewBag.ca_UniversityID = candidate[0].ca_UniversityID;
                    ViewBag.Ca_SectorIndustryID = candidate[0].Ca_SectorIndustryID;
                    ViewBag.ca_FunctionTitleID = candidate[0].ca_FunctionTitleID;
                    ViewBag.ca_CurrentJobTitleID = candidate[0].ca_CurrentJobTitleID;
                    ViewBag.ca_TotalRelavantExperience = candidate[0].ca_TotalRelavantExperience;
                    ViewBag.ca_Profile = candidate[0].ca_Profile;
                    ViewBag.ca_IsFresher = candidate[0].ca_IsFresher;


                    if (candidate[0].ca_HasPhoto == true)
                    {
                        ViewBag.uploadphoto = "YES";
                    }

                    var candidatedoc = db.Database.SqlQuery<CandidateDocuments>("select top 1 cd_OriginalName,cd_FileExtension from candidatedocuments where cd_ca_CandidateId=" + Convert.ToInt32(Session["Ca_ID"]) + " and cd_Type= 'CV' order by cd_id desc").ToList();
                    if (candidatedoc.Count != 0)
                    {
                        ViewBag.candidatecv = candidatedoc[0].cd_OriginalName + "." + candidatedoc[0].cd_FileExtension;
                    }
                }



            }




            return View();
        }

        public ActionResult CanRegEducation(string tabupdate, string addmore)
        {
            if (Session["Ca_ID"] == null) { return RedirectToAction("RegisterJobseekerLogin"); }



            ViewBag.tabupdate = tabupdate;
            ViewBag.addmore = addmore;


            ViewBag.curtabno = 2;
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                int tabno = db.Database.SqlQuery<int>("select ca_CompletedRegTabs from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();
                ViewBag.tabno = tabno;

                if (tabupdate != "tabupdate" && tabno != 1)
                {
                    return RedirectToAction("../" + CurTabFinder(tabno));
                }



                ViewBag.Education = db.Database.SqlQuery<CandidateEducationNew>(@"select ce.CaEdu_ID,ce.Ca_ID,UniversityID,UniCountryID,DegreeID,SpecializationID,GradeID,lmu.lm_Value as University,lmcon.lm_Value as UniCountry, lmDeg.lm_Value as Degree, lmSpez.lm_Value as Specialization, lmgr.lm_Value as Grade, ce.FromMonth,ce.FromYear, ce.ToMonth,ce.ToYear, ce.CurrentlyStudyHere,ce.Activities  from [CandidateEducation_T] ce 
                join ListsMaster lmu on lmu.lm_Id= ce.UniversityID
                join ListsMaster lmCon on lmCon.lm_Id= ce.UniCountryID
                join ListsMaster lmDeg on lmDeg.lm_Id= ce.DegreeID
                join ListsMaster lmSpez on lmSpez.lm_Id= ce.SpecializationID
                join ListsMaster lmGr on lmGr.lm_Id= ce.GradeID
                where ca_id=" + Convert.ToInt32(Session["Ca_ID"]) + " order by fromyear desc, frommonth desc").ToList();

                var candidatemediadocs = db.Database.SqlQuery<CandidateDocuments>("select cd_CaEdu_ID,cd_OriginalName,cd_FileExtension from candidatedocuments where cd_ca_CandidateId=" + Convert.ToInt32(Session["Ca_ID"]) + " and cd_Type= 'MEDIA' and cd_CaEdu_ID is not null").ToList();
                ViewBag.candidatemediadocs = candidatemediadocs.ToList();

                if (addmore != "YES")
                {
                    var ce = db.Database.SqlQuery<CandidateEducation_T>("select top 1 * from CandidateEducation_T where ca_id=" + Convert.ToInt32(Session["Ca_ID"]) + " order by CaEdu_ID desc").ToList();
                    if (ce.Count != 0)
                    {
                        ViewBag.CaEdu_ID = ce[0].CaEdu_ID;
                        ViewBag.UniversityID = ce[0].UniversityID;
                        ViewBag.UniCountryID = ce[0].UniCountryID;
                        ViewBag.DegreeID = ce[0].DegreeID;
                        ViewBag.SpecializationID = ce[0].SpecializationID;
                        ViewBag.GradeID = ce[0].GradeID;
                        ViewBag.FromMonth = ce[0].FromMonth;
                        ViewBag.FromYear = ce[0].FromYear;
                        ViewBag.ToMonth = ce[0].ToMonth;
                        ViewBag.ToYear = ce[0].ToYear;
                        ViewBag.CurrentlyStudyHere = ce[0].CurrentlyStudyHere;
                        ViewBag.Activities = ce[0].Activities;

                        var candidatedoc = db.Database.SqlQuery<CandidateDocuments>("select top 1 cd_OriginalName,cd_FileExtension from candidatedocuments where cd_ca_CandidateId=" + Convert.ToInt32(Session["Ca_ID"]) + " and cd_Type= 'MEDIA'  and cd_CaEdu_ID=" + ce[0].CaEdu_ID + "  order by cd_id desc").ToList();
                        if (candidatedoc.Count != 0)
                        {
                            ViewBag.candidatemedia = candidatedoc[0].cd_OriginalName + "." + candidatedoc[0].cd_FileExtension;
                        }
                    }

                }


                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderByDescending(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();





            }


            return View();
        }

        public ActionResult CanRegWorkHistory(string tabupdate, string addmore)
        {
           // if (Session["Ca_ID"] == null) { return RedirectToAction("RegisterJobseekerLogin"); }

            ViewBag.tabupdate = tabupdate;
            ViewBag.addmore = addmore;

            ViewBag.curtabno = 3;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                bool ca_IsFresher = db.Database.SqlQuery<bool>("select ca_IsFresher from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();

                if (ca_IsFresher) {

                    return RedirectToAction("CanRegSuccess");
                }

                //int tabno = db.Database.SqlQuery<int>("select ca_CompletedRegTabs from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();

                

                ViewBag.tabno = 3;

                //if (tabupdate != "tabupdate" && tabno != 2)
                //{
                //    return RedirectToAction("../" + CurTabFinder(tabno));
                //}


                ViewBag.Experience2 = db.Database.SqlQuery<CandidateWorkHistory>(@"select cwh.CaWorkHistory_ID,cwh.Ca_ID,JobTitleID,cwh.JobLevelID,cwh.ReportingToID,cwh.IndustryID,cwh.FunctionID,cwh.JobLocationID,cwh.SalaryID,lmjt.lm_Value as JobTitle, lmjl.lm_Value as JobLevel, cwh.Company, lmrt.lm_Value as ReportingTo, lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction, 
                cwh.FromMonth,cwh.FromYear,cwh.ToMonth,cwh.ToYear,cwh.CurrentlyWorkingHere,lmjlo.lm_Value as JobLocation, lmsal.lm_Value as Salary,cwh.[Description] from [CandidateWorkHistory_T] cwh 
                join ListsMaster lmjt on lmjt.lm_Id = cwh.JobTitleID
                join ListsMaster lmjl on lmjl.lm_Id = cwh.JobLevelID
                join ListsMaster lmrt on lmrt.lm_Id = cwh.ReportingToID
                join ListsMaster lmind on lmind.lm_Id = cwh.IndustryID
                join ListsMaster lmfn on lmfn.lm_Id = cwh.FunctionID
                join ListsMaster lmjlo on lmjlo.lm_Id = cwh.JobLocationID
                join ListsMaster lmsal on lmsal.lm_Id = cwh.SalaryID
                where ca_id=" + Convert.ToInt32(Session["Ca_ID"]) + " order by fromyear desc, frommonth desc").ToList();




                if (addmore != "YES")
                {
                    var cw = db.Database.SqlQuery<CandidateWorkHistory_T>("select top 1 * from Candidateworkhistory_T where ca_id=" + Convert.ToInt32(Session["Ca_ID"]) + " order by CaWorkHistory_ID desc").ToList();
                    if (cw.Count != 0)
                    {
                        ViewBag.CaWorkHistory_ID = cw[0].CaWorkHistory_ID;
                        ViewBag.JobTitleID = cw[0].JobTitleID;
                        ViewBag.Company = cw[0].Company;
                        ViewBag.JobLevelID = cw[0].JobLevelID;
                        ViewBag.ReportingToID = cw[0].ReportingToID;
                        ViewBag.IndustryID = cw[0].IndustryID;
                        ViewBag.FunctionID = cw[0].FunctionID;
                        ViewBag.FromMonth = cw[0].FromMonth;
                        ViewBag.FromYear = cw[0].FromYear;
                        ViewBag.ToMonth = cw[0].ToMonth;
                        ViewBag.ToYear = cw[0].ToYear;
                        ViewBag.CurrentlyWorkingHere = cw[0].CurrentlyWorkingHere;
                        ViewBag.JobLocationID = cw[0].JobLocationID;
                        ViewBag.SalaryID = cw[0].SalaryID;
                        ViewBag.Description = cw[0].Description;

                    }

                }

                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderByDescending(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();


            }


            return View();
        }

        public ActionResult CanRegITSkills(string tabupdate)
        {
            if (Session["Ca_ID"] == null) { return RedirectToAction("RegisterJobseekerLogin"); }



            ViewBag.curtabno = 4;
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                int tabno = db.Database.SqlQuery<int>("select ca_CompletedRegTabs from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();
                ViewBag.tabno = tabno;

                if (tabupdate != "tabupdate" && tabno != 3)
                {
                    return RedirectToAction("../" + CurTabFinder(tabno));
                }
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,33,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderByDescending(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.OtherSkill = ddLists.Where(x => x.lm_lt_ListTypeId == 33).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();


            }


            return View();
        }

        public ActionResult CanRegAccomplishments(string tabupdate, string addmore)
        {
            if (Session["Ca_ID"] == null) { return RedirectToAction("RegisterJobseekerLogin"); }



            ViewBag.curtabno = 5;
            ViewBag.tabupdate = tabupdate;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                int tabno = db.Database.SqlQuery<int>("select ca_CompletedRegTabs from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();
                ViewBag.tabno = tabno;

                if (tabupdate != "tabupdate" && tabno != 4)
                {
                    return RedirectToAction("../" + CurTabFinder(tabno));
                }


                ViewBag.CandidateCertifications = db.Database.SqlQuery<CandidateCertifications>(@"select cc.CaCertification_ID,CertificationID,Authority,cc.Ca_ID,lmcer.lm_Value as Certification, cc.FromMonth,cc.FromYear,cc.ToMonth,cc.ToYear,cc.DoNotExpire from [CandidateCertification_T] cc
                join ListsMaster lmcer on lmcer.lm_Id = cc.CertificationID
                where ca_id=" + Convert.ToInt32(Session["Ca_ID"]) + " order by fromyear desc, frommonth desc").ToList();


                if (addmore != "YES")
                {

                    var cw = db.Database.SqlQuery<CandidateCertification_T>("select top 1 * from [CandidateCertification_T] where ca_id=" + Convert.ToInt32(Session["Ca_ID"]) + " order by CaCertification_ID desc").ToList();
                    if (cw.Count != 0)
                    {
                        ViewBag.CertificationID = cw[0].CertificationID;
                        ViewBag.Authority = cw[0].Authority;
                        ViewBag.FromMonth = cw[0].FromMonth;
                        ViewBag.FromYear = cw[0].FromYear;
                        ViewBag.ToMonth = cw[0].ToMonth;
                        ViewBag.ToYear = cw[0].ToYear;
                        ViewBag.DoNotExpire = cw[0].DoNotExpire;
                    }
                }

                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderByDescending(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();


            }


            return View();
        }

        public ActionResult CanRegIdealCareerMove(string tabupdate)
        {
            if (Session["Ca_ID"] == null) { return RedirectToAction("RegisterJobseekerLogin"); }


            ViewBag.curtabno = 6;
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                int tabno = db.Database.SqlQuery<int>("select ca_CompletedRegTabs from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();
                ViewBag.tabno = tabno;

                if (tabupdate != "tabupdate" && tabno != 5)
                {
                    return RedirectToAction("../" + CurTabFinder(tabno));
                }

                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderByDescending(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();


            }


            return View();
        }


        public string CurTabFinder(int tabno)
        {
            string tabname = "";

            switch (tabno)
            {
                case 0:
                    {
                        tabname = "CanRegProfileSummary";
                        break;
                    }

                case 1:
                    {
                        tabname = "CanRegEducation";
                        break;
                    }

                case 2:
                    {
                        tabname = "CanRegWorkHistory";
                        break;
                    }
                case 3:
                    {
                        tabname = "CanRegITSkills";
                        break;
                    }
                case 4:
                    {
                        tabname = "CanRegAccomplishments";
                        break;
                    }
                case 5:
                    {
                        tabname = "CanRegIdealCareerMove";
                        break;
                    }
                case 6:
                    {
                        tabname = "NonActivated";
                        break;
                    }
            }

            return tabname;
        }



        public ActionResult JobseekerRegistration()
        {
            if (Session["Ca_ID"] == null) { return RedirectToAction("RegisterJobseekerLogin"); }



            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderByDescending(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();


            }
            return View();
        }








        public ActionResult EmployerComingSoon()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EmployerComingSoon(FormCollection col)
        {
            string contacttime = "";
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string FirstName = Convert.ToString(col["FirstName"]);
                string LastName = Convert.ToString(col["LastName"]);
                string Email = Convert.ToString(col["Email"]);
                string Mobile = Convert.ToString(col["Mobile"]);
                string CompanyName = Convert.ToString(col["CompanyName"]);
                string CompanyCR = Convert.ToString(col["CompanyCR"]);
                string POBox = Convert.ToString(col["POBox"]);

                

                if (col["8"] != null) {
                    contacttime += "8am-10am";
                }
                if (col["10"] != null)
                {
                    contacttime += " 10am-12pm";
                }
                if (col["12"] != null)
                {
                    contacttime += " 12pm-2pm";
                }
                if (col["2"] != null)
                {
                    contacttime += " 2pm-4pm";
                }
                if (col["4"] != null)
                {
                    contacttime += " 4pm-6pm";
                }
                  


                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("INSERT INTO [dbo].[EmployerComingSoon_T]  ([FirstName],[LastName],[Email],[Phone],[CompanyName],CompanyCR,POBox,ContactTime) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')", FirstName, LastName, Email, Mobile, CompanyName, CompanyCR, POBox, contacttime);
                db.Database.ExecuteSqlCommand(sb.ToString());
            }

            //send mail
            string bodycontent;
            bodycontent = "<font face='Arial' size='2'>";
            bodycontent += "<b>An Employer submitted their details as a contact request on Jobs4Bahrainis Website!</b>";
            bodycontent += "<br><br>";
            bodycontent += "<p>First Name: " + Convert.ToString(col["FirstName"]) + "</p>";
            bodycontent += "<p>Last Name: " + Convert.ToString(col["LastName"]) + "</p>";
            bodycontent += "<p>Phone: " + Convert.ToString(col["Mobile"]) + "</p>";
            bodycontent += "<p>Email: " + Convert.ToString(col["Email"]) + "</p>";
            bodycontent += "<p>Company Name: " + Convert.ToString(col["CompanyName"]) + "</p>";
            bodycontent += "<p>Company CR: " + Convert.ToString(col["CompanyCR"]) + "</p>";
            bodycontent += "<p>PO Box: " + Convert.ToString(col["POBox"]) + "</p>";
            bodycontent += "<p>Contact Time: " + contacttime + "</p>";


            bodycontent += "</font>";

            common cmn = new common();
            cmn.SendMail("Bayden.tierney@gulfconnexions.com", "Jobs4Bahrainis.com Employer", bodycontent);
            cmn.SendMail("binju.paul@tradearabia.net", "Jobs4Bahrainis.com Employer Contact Request", bodycontent);
            cmn.SendMail("mohamed.Ghazwan@jobs4bahrainis.com", "Jobs4Bahrainis.com Employer Contact Request", bodycontent);

            return RedirectToAction("EmployerCSThanks");
        }
        public ActionResult EmployerCSThanks()
        {
            return View();
        }

        public ActionResult RegisterEmployerLogin(string e)
        {

            if (e == null)
            {
                ViewBag.message = "";
            }
            else {
                ViewBag.message = "Email ID is already in use, try forgot password or use another Email";
            }

            
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
                try
                {

                    decimal newcompanyid = db.Database.SqlQuery<decimal>("insert into Companies(co_Name) values('" + Email + "');select @@identity;").SingleOrDefault();
                    sb.AppendFormat("insert into [contacts] (ct_EmailAddress,ct_Password,ct_co_companyid) values('{0}','{1}',{2});select @@identity;", Email, Password, newcompanyid);
                    newcontactid = db.Database.SqlQuery<decimal>(sb.ToString()).SingleOrDefault();

                    Session["CompanyID"] = newcompanyid;
                    Session["newcontactid"] = newcontactid;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("UNIQUE KEY constraint"))
                    {
                        return RedirectToAction("../RegisterEmployerLogin/EmailAlreadyInUse");
                    }
                    else
                    {
                        return RedirectToAction("../RegisterEmployerLogin");
                    }
                }


            }
            return RedirectToAction("../RecruiterRegistration");
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
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (31,25,4) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.CompanySize = ddLists.Where(x => x.lm_lt_ListTypeId == 31).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Position = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();



            }
            return View();
        }


        [ValidateInput(false)]
        [HttpPost]
        public ActionResult RecruiterContact(FormCollection col, HttpPostedFileBase uploadlogo)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string companyname = Convert.ToString(col["CompanyName"]).Replace("'", "&#039;");
                string companynameencoded = companyname.Replace(" ", "");
                string telephone = Convert.ToString(col["Phone"]);
                string mobile = Convert.ToString(col["Mobile"]);
                string address = StripHTML(Convert.ToString(col["Address"]));
                string profile = StripHTML(Convert.ToString(col["Profile"])).Replace("'", "&#039;");
                int co_PositioninCompanyID = Convert.ToInt32(col["co_PositioninCompanyID"]);
                int co_CompanySizeID = Convert.ToInt32(col["co_CompanySizeID"]);
                string co_CRNo = Convert.ToString(col["co_CRNo"]);



                int companyid = Convert.ToInt32(Session["CompanyID"]);

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("update Companies set co_haslogo=1, co_Name='{0}',co_br_HomeBrandId=2,co_PostalAddress='{1}',co_Telephone='{2}',co_ClientType=30,co_st_StatusID=25,co_NameEncoded='{3}', co_CompanySizeID={4}, co_Profile='{5}', co_CRNo={7} where co_id={6};select @@identity as companyid", companyname, address, telephone, companynameencoded, co_CompanySizeID, profile, companyid, co_CRNo);
                try
                {
                    db.Database.ExecuteSqlCommand(sb.ToString());
                }
                catch // duplicate company name
                {
                    return RedirectToAction("RecruiterRegistration");
                }


                string FirstName = Convert.ToString(col["FirstName"]);
                string LastName = Convert.ToString(col["LastName"]);

                int newcontactid = Convert.ToInt32(Convert.ToString(col["newcontactid"]).Trim());

                sb = new StringBuilder();
                sb.AppendFormat("update Contacts set ct_Forename='{0}',ct_Surname='{1}',ct_Telephone='{2}',ct_Superuser=1,ct_st_StatusId=20,ct_co_CompanyId={3},co_PositioninCompanyID={5},ct_postjob=1,ct_SearchCV=1 where ct_id={4}", FirstName, LastName, mobile, companyid, newcontactid, co_PositioninCompanyID);

                db.Database.ExecuteSqlCommand(sb.ToString());


                Contact ct = db.Contacts.Where(x => x.ct_Id == newcontactid).SingleOrDefault();
                
                Session["RecruiterName"] = Convert.ToString(ct.ct_Forename);
                Session["CompanyID"] = companyid;


                if (uploadlogo != null)
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

                    string extension = Path.GetExtension(uploadlogo.FileName);

                    uploadlogo.SaveAs(folderpath + "/" + coguid + "-original" + extension);


                    dbOperations dbo = new dbOperations();
                    dbo.CompanyLogoExt(Convert.ToInt32(Session["CompanyID"]), extension);


                    if (extension == ".jpg" || extension == ".jpeg")
                    {

                        resizeImage(folderpath + "/" + coguid + "-original" + extension, 300);
                        resizeImage(folderpath + "/" + coguid + "-original" + extension, 70);
                    }
                    else
                    {
                        uploadlogo.SaveAs(folderpath + "/" + coguid + extension);
                    }


                }



               

            }

            return RedirectToAction("RecruiterBA");

        }



        public ActionResult RecruiterBA()
        {
            if (Session["CompanyID"] == null) {
                return RedirectToAction("../RegisterEmployerLogin");
            }

            dbOperations dbo = new dbOperations();

            ViewBag.Sectors = dbo.getlist(4).ToList();

            List<ListMaster> firstsector = ViewBag.Sectors;
            ViewBag.FirstSectorID = firstsector.Min(x => x.lm_Id);
            ViewBag.SectorCount = firstsector.Count();
            
               
      
            return View();
        }

        public ActionResult EmployerRegSuccess()
        {
            if (Session["CompanyID"] == null)
            {
                return RedirectToAction("../RegisterEmployerLogin");
            }

            int Co_ID = Convert.ToInt32(Session["CompanyID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                reccontacts recruiter = db.Database.SqlQuery<reccontacts>("select co_Guid,ct_EmailAddress from companies c join contacts ct on c.co_id= ct.ct_co_companyid where ct.ct_superuser=1 and  c.co_id=" + Co_ID).SingleOrDefault();
                string path = Server.MapPath("~/Templates/EmployerActivation.html");
                string bodycontent = System.IO.File.ReadAllText(path);
                bodycontent = bodycontent.Replace("##activationmaillink##", "http://www.jobs4bahrainis.com/ActivateEmployer/" + recruiter.co_Guid + "/WelcometoJ4B");
                common cmn = new common();

                cmn.SendMail(recruiter.ct_EmailAddress, "Account Activation - J4B Website", bodycontent);
            }

            return View();
        }







        public ActionResult RegisterJobseekerLogin()
        {
            dbOperations dbo = new dbOperations();
            ViewBag.Country = dbo.getlist(13);

            return View();

        }

        [HttpPost]
        public ActionResult RegisterJobseekerLogin(FormCollection col)
        {

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string firstname = Convert.ToString(col["firstname"]);
                string surname = Convert.ToString(col["surname"]);
                string Email = Convert.ToString(col["EmailID"]);
                string Password = Convert.ToString(col["Password"]);
                string Phone = Convert.ToString(col["Phone"]);
                Int32 countryid = Convert.ToInt32(col["countryid"]);
                string ca_IsFresher = Convert.ToString(col["ca_IsFresher"]);

                int fresher = 1;


                if (ca_IsFresher == "yes")
                {
                    fresher = 1;
                }
                else
                {
                    fresher = 0;
                }


                decimal Ca_ID = 0;





                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(" insert into [Candidates] ([ca_FirstName],[ca_Surname],[ca_EmailAddress],[ca_Password],[ca_PhoneMobile],[ca_lm_CountryId], [ca_TotalRelavantExperience],[ca_IsFresher],[ca_Created]) values('{0}', '{1}', '{2}', '{3}', '{4}',{5},0,{6},getdate()); select @@identity;", firstname, surname, Email, Password, Phone, countryid, fresher);

                Ca_ID = db.Database.SqlQuery<decimal>(sb.ToString()).SingleOrDefault();

                Session["Ca_ID"] = Ca_ID;
                Session["ReginProgress"] = "YES";




                CandidateNew cn = db.Database.SqlQuery<CandidateNew>("select * from Candidates where ca_id=" + Ca_ID).SingleOrDefault();


                //send mail



                //common cmn = new common();
                //string path = Server.MapPath("~/Templates/Completeyourprofile.html");
                //string bodycontent = System.IO.File.ReadAllText(path);
                //bodycontent = bodycontent.Replace("##continuationmaillink##", "http://www.jobs4bahrainis.com/CanRegContinue/" + cn.ca_GUID + "/WelcometoJ4B");
                //cmn.SendMail(cn.ca_EmailAddress, "A few more steps to complete your profile - J4B Website", bodycontent);

            }
            return RedirectToAction("../CanRegProfileSummary");
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



        [ValidateInput(false)]
        [HttpPost]
        public ActionResult RegisterCandidate(FormCollection col, HttpPostedFileBase uploadcpr)
        {
            decimal Ca_ID = Convert.ToDecimal(Session["Ca_ID"]);


            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                string[] dob = Convert.ToString(col["ca_DateOfBirth"]).Split('/');



                DateTime dateofbirth = Convert.ToDateTime(dob[2] + "/" + dob[1] + "/" + dob[0]);
                string gender = Convert.ToString(col["ca_Gender"]);
                string ca_MaritalStatus = Convert.ToString(col["ca_MaritalStatus"]);
                Int32 ca_lm_CountryId = Convert.ToInt32(col["ca_lm_CountryId"]);
                string ca_lm_EducationLevel = Convert.ToString(col["ca_lm_EducationLevel"]);
                string ca_University = Convert.ToString(col["proUniversity"]);
                string Ca_SectorIndustryID = Convert.ToString(col["Ca_SectorIndustryID"]);
                string ca_FunctionTitleID = Convert.ToString(col["ca_FunctionTitleID"]);
                string ca_CurrentJobTitleID = Convert.ToString(col["ca_CurrentJobTitleID"]);
                decimal ca_TotalRelavantExperience = Convert.ToDecimal(col["ca_TotalRelavantExperience"]);
                string ca_Profile = StripHTML(Convert.ToString(col["ca_Profile"])).Replace("'", "&#039;");




                StringBuilder sb = new StringBuilder();

                sb.AppendFormat(@"UPDATE [dbo].[Candidates] SET ca_CompletedRegTabs=2, [ca_lastupdated]=getdate() ,[ca_br_OriginatingBrandId] = 1  ,[ca_CurrentCountryId] = {0} 
                ,[ca_lm_EducationLevel] = {1} ,[ca_ProfileSearchable] = 1 ,[ca_CVSearchable] = 1 ,[ca_DetailsVisible] = 1,[ca_st_StatusID] = 25,[ca_Gender] = '{2}'
                ,[ca_DateOfBirth] = '{3}',[ca_MaritalStatus] = '{4}',[ca_FunctionTitleID] = {5},[ca_CurrentJobTitleID] = '{6}'
                ,[ca_TotalRelavantExperience] = {7},[ca_UniversityID] = '{8}',ca_Profile='{10}',Ca_SectorIndustryID={11},ca_lm_countryid=523 where ca_Id ={9}", ca_lm_CountryId, ca_lm_EducationLevel, gender, dateofbirth, ca_MaritalStatus
                , ca_FunctionTitleID, ca_CurrentJobTitleID, ca_TotalRelavantExperience, ca_University, Ca_ID, ca_Profile, Ca_SectorIndustryID);

                string candisql = sb.ToString();

                db.Database.ExecuteSqlCommand(sb.ToString());


                if (uploadcpr != null)
                {
                    // save ID
                    string extension = Path.GetExtension(uploadcpr.FileName);
                    extension = extension.TrimStart('.');
                    string fileName = Path.GetFileNameWithoutExtension(uploadcpr.FileName);

                    StringBuilder sbcd = new StringBuilder();
                    sbcd.AppendFormat("insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType,cd_Type) values('{0}',{1},{2},'{3}','{4}','{5}','{6}'); select @@identity;", DateTime.Now, Ca_ID, 1, fileName, extension, "", "ID");

                    decimal Cd_ID = 0;

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

                    dbOperations dbo = new dbOperations();
                    dbo.jsCareerMove(Convert.ToInt32(Session["Ca_ID"]), Convert.ToInt32(col["JobTitleID"]), Convert.ToInt32(col["FunctionID"]), Convert.ToInt32(col["IndustryID"]), Convert.ToInt32(col["ExperienceID"]), Convert.ToInt32(col["ExpectedSalaryID"]));
                    CandidateActivationMail(Convert.ToInt32(Session["Ca_ID"]));
                }





                //db.Database.ExecuteSqlCommand("Update Candidates set ca_CompletedRegTabs=1 where Ca_ID=" + Ca_ID + " and ca_CompletedRegTabs=0");
            }

            return new JsonpResult
            {

                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult CanRegSuccess()
        {

            if (Session["Ca_ID"] != null)
            {
                int Ca_ID = Convert.ToInt32(Session["Ca_ID"]);

                CandidateActivationMail(Convert.ToInt32(Session["Ca_ID"]));

                Session["Ca_ID"] = null;
            }



            return View();
        }


        [HttpPost]
        public ActionResult UploadProfilePicRegistration(HttpPostedFileBase uploadphoto)
        {
            decimal Ca_ID = Convert.ToDecimal(Session["Ca_ID"]);
            if (uploadphoto != null)
            {

                SqlCon mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "SELECT ca_GUID from Candidates where ca_Id=@ca_Id";
                mycon.sqlCmd_.Parameters.AddWithValue("@ca_Id", Ca_ID);
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

                FileInfo fi = new FileInfo(uploadphoto.FileName);
                string extension = fi.Extension.ToLower();

                uploadphoto.SaveAs(folderpath + "/" + ca_GUID + extension);
                dbOperations dbo = new dbOperations();
                dbo.CandidatePhotoExt(Ca_ID, extension.Replace(".", ""));
            }

            return new JsonpResult
            {

                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };

        }

        [HttpPost]
        public ActionResult UploadCVregistration(HttpPostedFileBase uploadcv)
        {

            decimal Ca_ID = Convert.ToDecimal(Session["Ca_ID"]);

            if (uploadcv != null)
            {

                using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
                {

                    // save CV
                    string extension = Path.GetExtension(uploadcv.FileName);
                    extension = extension.TrimStart('.');
                    string fileName = Path.GetFileNameWithoutExtension(uploadcv.FileName);

                    StringBuilder sbcd = new StringBuilder();
                    sbcd.AppendFormat("delete from CandidateDocuments where cd_ca_CandidateID={1} and cd_type='CV';insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType,cd_Type) values('{0}',{1},{2},'{3}','{4}','{5}','{6}'); select @@identity;", DateTime.Now, Ca_ID, 1, fileName, extension, "", "CV");

                    decimal Cd_ID = db.Database.SqlQuery<decimal>(sbcd.ToString()).SingleOrDefault();



                    SqlCon mycon = new SqlCon();
                    mycon.sqlCmd_.CommandText = "SELECT cd_Guid from CandidateDocuments where Cd_ID=@Cd_ID";
                    mycon.sqlCmd_.Parameters.AddWithValue("@Cd_ID", Cd_ID);
                    mycon.sqlConOpen();
                    string cd_Guid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
                    cd_Guid = cd_Guid.ToUpper();
                    mycon.sqlConClose();

                    string folderpath = Server.MapPath("~/Documents/cvs/" + cd_Guid.Substring(0, 2) + "/" + cd_Guid.Substring(2, 2) + "/" + cd_Guid.Substring(4, 2));
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


        [ValidateInput(false)]
        [HttpPost]
        public ActionResult RecruiterUpdate(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string companyname = Convert.ToString(col["CompanyName"]);
                string companynameencoded = companyname.Replace(" ", "");
                string telephone = Convert.ToString(col["Phone"]);
                string address = Convert.ToString(col["Address"]);


                int co_PositioninCompanyID = Convert.ToInt32(col["co_PositioninCompanyID"]);
                int co_CompanySizeID = Convert.ToInt32(col["co_CompanySizeID"]);
                int companyid = Convert.ToInt32(Session["CompanyID"]);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("update Companies set co_haslogo=1, co_Name='{0}',co_br_HomeBrandId=2,co_PostalAddress='{1}',co_Telephone='{2}',co_ClientType=30,co_st_StatusID=25,co_NameEncoded='{3}', co_CompanySizeID={4} where co_id={5};select @@identity as companyid", companyname, address, telephone, companynameencoded, co_CompanySizeID, companyid);
                db.Database.ExecuteSqlCommand(sb.ToString());
            }
            return RedirectToAction("Recruiter");
        }




        //[ValidateInput(false)]
        //[HttpPost]
        //public ActionResult RecruiterProfileUpdate(FormCollection col, HttpPostedFileBase profileimage1, HttpPostedFileBase profileimage2, HttpPostedFileBase profileimage3, HttpPostedFileBase profileimage4, HttpPostedFileBase profileimage5)
        //{
        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {
        //        string extension1 = "", extension2 = "", extension3 = "", extension4 = "", extension5 = "";
        //        int companyid = Convert.ToInt32(Session["CompanyID"]);

        //        string title1 = Convert.ToString(col["title1"]);
        //        string title2 = Convert.ToString(col["title2"]);
        //        string title3 = Convert.ToString(col["title3"]);
        //        string title4 = Convert.ToString(col["title4"]);
        //        string title5 = Convert.ToString(col["title5"]);

        //        string CompanyGUID = Convert.ToString(col["CompanyGUID"]);

        //        string para1 = StripHTMLExcept(Convert.ToString(col["para1"]));
        //        string para2 = StripHTMLExcept(Convert.ToString(col["para2"]));
        //        string para3 = StripHTMLExcept(Convert.ToString(col["para3"]));
        //        string para4 = StripHTMLExcept(Convert.ToString(col["para4"]));
        //        string para5 = StripHTMLExcept(Convert.ToString(col["para5"]));


        //        string folderpath = Server.MapPath("~/Documents/ProfilePics/" + CompanyGUID.Substring(0, 2) + "/" + CompanyGUID.Substring(2, 2) + "/" + CompanyGUID.Substring(4, 2));
        //        DirectoryInfo dir = new DirectoryInfo(folderpath);
        //        if (!dir.Exists)
        //        {
        //            dir.Create();
        //        }

        //        StringBuilder sb = new StringBuilder();

        //        string qry = "UPDATE Companies SET co_ProfileTitle1='{0}',co_ProfileTitle2='{1}',co_ProfileTitle3='{2}',co_ProfileTitle4='{12}',co_ProfileTitle5='{13}',co_Profile='{3}',co_Profile2='{4}',co_Profile3='{5}',co_Profile4='{14}',co_Profile5='{15}' ";



        //        if (profileimage1 != null)
        //        {
        //            extension1 = Path.GetExtension(profileimage1.FileName);
        //            profileimage1.SaveAs(folderpath + "/profileimage1" + extension1);
        //            qry += ",co_ProfilePicExtn1='{7}'";

        //        }

        //        if (profileimage2 != null)
        //        {
        //            extension2 = Path.GetExtension(profileimage2.FileName);
        //            profileimage2.SaveAs(folderpath + "/profileimage2" + extension2);
        //            qry += ",co_ProfilePicExtn2='{8}'";

        //        }
        //        if (profileimage3 != null)
        //        {
        //            extension3 = Path.GetExtension(profileimage3.FileName);
        //            profileimage3.SaveAs(folderpath + "/profileimage3" + extension3);
        //            qry += ",co_ProfilePicExtn3='{9}'";

        //        }

        //        if (profileimage4 != null)
        //        {
        //            extension4 = Path.GetExtension(profileimage4.FileName);
        //            profileimage4.SaveAs(folderpath + "/profileimage4" + extension4);
        //            qry += ",co_ProfilePicExtn4='{10}'";

        //        }

        //        if (profileimage5 != null)
        //        {
        //            extension5 = Path.GetExtension(profileimage5.FileName);
        //            profileimage5.SaveAs(folderpath + "/profileimage5" + extension5);
        //            qry += ",co_ProfilePicExtn5='{11}'";

        //        }


        //        sb.AppendFormat(qry + " WHERE co_id={6} ", title1, title2, title3, para1, para2, para3, companyid, extension1, extension2, extension3, extension4, extension5, title4, title5, para4, para5);


        //        db.Database.ExecuteSqlCommand(sb.ToString());


        //    }
        //    return RedirectToAction("Recruiter");
        //}

        public ActionResult PublishTestimonials(int yesno)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update companies set co_TestimonialsPublished=" + yesno + " where co_id=" + Convert.ToInt32(Session["CompanyID"]));
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult PublishProfile(int yesno)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update companies set co_ProfilePublished=" + yesno + " where co_id=" + Convert.ToInt32(Session["CompanyID"]));
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult PublishVideos(int yesno)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update companies set co_VideoPublished=" + yesno + " where co_id=" + Convert.ToInt32(Session["CompanyID"]));
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }



        public ActionResult PublishGallery(int yesno)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update companies set co_GalleryPublished=" + yesno + " where co_id=" + Convert.ToInt32(Session["CompanyID"]));
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult PublishMedia(int yesno)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update companies set co_MediaPublished=" + yesno + " where co_id=" + Convert.ToInt32(Session["CompanyID"]));
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult Updateappstatus(int app_id, int appstatusid)
        {
            if (Session["CompanyID"] != null)
            {
                using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
                {
                    db.Database.ExecuteSqlCommand("Update applications set app_st_ShortlistStatusId= " + appstatusid + " where app_id=" + app_id);
                }
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult ProfileParaDelete(int profileno)
        {


            string sql = "Update Companies set ##title##=null,##profile##=null,##extension##=null where co_id=" + Convert.ToInt32(Session["CompanyID"]);

            switch (profileno)
            {
                case 1:
                    {
                        sql = sql.Replace("##title##", "co_ProfileTitle1").Replace("##profile##", "co_Profile").Replace("##extension##", "co_ProfilePicExtn1");
                        break;
                    }
                case 2:
                    {
                        sql = sql.Replace("##title##", "co_ProfileTitle2").Replace("##profile##", "co_Profile2").Replace("##extension##", "co_ProfilePicExtn2");
                        break;
                    }
                case 3:
                    {
                        sql = sql.Replace("##title##", "co_ProfileTitle3").Replace("##profile##", "co_Profile3").Replace("##extension##", "co_ProfilePicExtn3");
                        break;
                    }
                case 4:
                    {
                        sql = sql.Replace("##title##", "co_ProfileTitle4").Replace("##profile##", "co_Profile4").Replace("##extension##", "co_ProfilePicExtn4");
                        break;
                    }
                case 5:
                    {
                        sql = sql.Replace("##title##", "co_ProfileTitle5").Replace("##profile##", "co_Profile5").Replace("##extension##", "co_ProfilePicExtn5");
                        break;
                    }
            }
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand(sql);
            }


            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }




        [ValidateInput(false)]
        [HttpPost]
        public ActionResult RecruiterProfileUpdate(FormCollection col, HttpPostedFileBase profileimage)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string extension = "";
                int companyid = Convert.ToInt32(Session["CompanyID"]);
                string CompanyGUID = Convert.ToString(col["CompanyGUID"]);

                string title1 = Convert.ToString(col["title"]);
                string para1 = StripHTMLExcept(Convert.ToString(col["para"])).Replace("'", "&#039;");

                string folderpath = Server.MapPath("~/Documents/ProfilePics/" + CompanyGUID.Substring(0, 2) + "/" + CompanyGUID.Substring(2, 2) + "/" + CompanyGUID.Substring(4, 2));

                DirectoryInfo dir = new DirectoryInfo(folderpath);
                if (!dir.Exists)
                {
                    dir.Create();
                }

                StringBuilder sb = new StringBuilder();
                string qry = "UPDATE Companies SET ##title##='{0}',##profile##='{1}' ";
                if (profileimage != null)
                {
                    extension = Path.GetExtension(profileimage.FileName);
                    profileimage.SaveAs(folderpath + "/profileimage" + Convert.ToInt32(col["profileno"]) + extension);
                    qry += ",##extension##='{2}'";
                }
                else
                {
                    extension = "";
                }

                if (Convert.ToInt32(col["profileno"]) == 1)
                {
                    qry = qry.Replace("##title##", "co_ProfileTitle1").Replace("##profile##", "co_Profile").Replace("##extension##", "co_ProfilePicExtn1");
                }
                else if (Convert.ToInt32(col["profileno"]) == 2)
                {
                    qry = qry.Replace("##title##", "co_ProfileTitle2").Replace("##profile##", "co_Profile2").Replace("##extension##", "co_ProfilePicExtn2");
                }
                else if (Convert.ToInt32(col["profileno"]) == 3)
                {
                    qry = qry.Replace("##title##", "co_ProfileTitle3").Replace("##profile##", "co_Profile3").Replace("##extension##", "co_ProfilePicExtn3");
                }
                else if (Convert.ToInt32(col["profileno"]) == 4)
                {
                    qry = qry.Replace("##title##", "co_ProfileTitle4").Replace("##profile##", "co_Profile4").Replace("##extension##", "co_ProfilePicExtn4");
                }
                else if (Convert.ToInt32(col["profileno"]) == 5)
                {
                    qry = qry.Replace("##title##", "co_ProfileTitle5").Replace("##profile##", "co_Profile5").Replace("##extension##", "co_ProfilePicExtn5");
                }



                sb.AppendFormat(qry + " WHERE co_id={3} ", title1, para1, extension, companyid);
                db.Database.ExecuteSqlCommand(sb.ToString());
            }
            return RedirectToAction("Recruiter");
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


        public ActionResult getAuthority()
        {

            dbOperations dbo = new dbOperations();
            List<ListMaster> companies = dbo.getlist(30);

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

            dbOperations dbo = new dbOperations();
            dbo.jsCertification(Convert.ToInt32(Session["Ca_ID"]), Convert.ToInt32(col["CertificationID"]), Convert.ToString(col["Authority"]), Convert.ToInt32(col["FromMonth"]), Convert.ToInt32(col["FromYear"]), Convert.ToInt32(col["ToMonth"]), Convert.ToInt32(col["ToYear"]), Convert.ToBoolean(col["DoNotExpire"]));


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
            dbOperations dbo = new dbOperations();
            dbo.jsHonours(Convert.ToInt32(col["Ca_Id"]), Convert.ToString(col["Title"]), Convert.ToString(col["Associatedwith"]), Convert.ToString(col["Issuer"]), Convert.ToString(col["Issuedon"]), Convert.ToString(col["Description"]));

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        [HttpPost]
        public ActionResult JobSeekerITSkillUpdate(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                dbOperations dbo = new dbOperations();
                dbo.jsSkillUpdate(Convert.ToInt32(col["CaSkill_ID"]), Convert.ToString(col["SkillID"]), Convert.ToString(col["SkillLevelID"]));
            }

            return RedirectToAction("JobSeekerProfile");
        }
        [HttpPost]
        public ActionResult JobSeekerOSkillUpdate(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                dbOperations dbo = new dbOperations();
                dbo.jsOSkillUpdate(Convert.ToInt32(col["CaOSkill_ID"]), Convert.ToString(col["OSkillID"]), Convert.ToString(col["OSkillLevelID"]));
            }

            return RedirectToAction("JobSeekerProfile");
        }

        [HttpPost]
        public ActionResult JobSeekerLanguageUpdate(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                dbOperations dbo = new dbOperations();
                dbo.jsLanguageUpdate(Convert.ToInt32(col["CaLanguage_Id"]), Convert.ToString(col["LanguageID"]), Convert.ToString(col["ProficiencyID"]));
            }

            return RedirectToAction("JobSeekerProfile");
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
        public ActionResult JobSeekerWorkHistoryProfile(FormCollection col)
        {

            dbOperations dbo = new dbOperations();

            dbo.jsWorkHistory(Convert.ToInt32(Session["Ca_ID"]), Convert.ToInt32(col["JobTitleID"]), Convert.ToString(col["Company"]), Convert.ToInt32(col["JobLevelID"]), Convert.ToInt32(col["ReportingToID"]), Convert.ToInt32(col["IndustryID"]), Convert.ToInt32(col["FunctionID"]), Convert.ToInt32(col["FromMonth"]), Convert.ToInt32(col["FromYear"]), Convert.ToInt32(col["ToMonth"]), Convert.ToInt32(col["ToYear"]), Convert.ToBoolean(col["CurrentlyWorkingHere"]), Convert.ToInt32(col["JobLocationID"]), Convert.ToInt32(col["SalaryID"]), Convert.ToString(col["Description"]));


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

            dbOperations dbo = new dbOperations();
            if (Convert.ToString(col["CaWorkHistory_ID"]) != "") // updating
            {
                dbo.jsWorkHistoryUpdate(Convert.ToInt32(col["CaWorkHistory_ID"]), Convert.ToInt32(col["JobTitleID"]), Convert.ToString(col["Company"]), Convert.ToInt32(col["JobLevelID"]), Convert.ToInt32(col["ReportingToID"]), Convert.ToInt32(col["IndustryID"]), Convert.ToInt32(col["FunctionID"]), Convert.ToInt32(col["FromMonth"]), Convert.ToInt32(col["FromYear"]), Convert.ToInt32(col["ToMonth"]), Convert.ToInt32(col["ToYear"]), Convert.ToBoolean(col["CurrentlyWorkingHere"]), Convert.ToInt32(col["JobLocationID"]), Convert.ToInt32(col["SalaryID"]), Convert.ToString(col["Description"]));

            }
            else
            {
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
        public ActionResult JobSeekerCertificationsUpdate(FormCollection col)
        {

            string[] tomonth = Convert.ToString(col["ToMonth"]).Split(',');
            string[] toyear = Convert.ToString(col["ToYear"]).Split(',');


            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                dbOperations dbo = new dbOperations();

                dbo.jsCertificationUpdate(Convert.ToInt32(col["CaCertification_ID"]), Convert.ToInt32(col["CertificationID"]), Convert.ToString(col["Authority"]), Convert.ToInt32(col["FromMonth"]), Convert.ToInt32(col["FromYear"]), Convert.ToInt32(tomonth[0]), Convert.ToInt32(toyear[0]), Convert.ToBoolean(col["DoNotExpire"]));
            }

            return RedirectToAction("JobSeekerProfile");
        }



        [ValidateInput(false)]
        [HttpPost]
        public ActionResult JobSeekerWorkHistoryUpdate(FormCollection col)
        {

            string[] tomonth = Convert.ToString(col["wh_tomonth"]).Split(',');
            string[] toyear = Convert.ToString(col["wh_toyear"]).Split(',');


            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                dbOperations dbo = new dbOperations();

                dbo.jsWorkHistoryUpdate(Convert.ToInt32(col["CaWorkHistory_ID"]), Convert.ToInt32(col["wh_jobtitle"]), Convert.ToString(col["wh_company"]), Convert.ToInt32(col["wh_level"]), Convert.ToInt32(col["wh_reportingto"]), Convert.ToInt32(col["wh_industry"]), Convert.ToInt32(col["wh_function"]), Convert.ToInt32(col["wh_frommonth"]), Convert.ToInt32(col["wh_fromyear"]), Convert.ToInt32(tomonth[0]), Convert.ToInt32(toyear[0]), Convert.ToBoolean(col["wh_currentlywork"]), Convert.ToInt32(col["wh_location"]), Convert.ToInt32(col["wh_salary"]), StripHTMLExcept(Convert.ToString(col["Description"])));
            }

            return RedirectToAction("JobSeekerProfile");
        }



        [ValidateInput(false)]
        [HttpPost]
        public ActionResult JobSeekerEducationProfile(FormCollection col, HttpPostedFileBase uploadmedia)
        {
            int Ca_ID = Convert.ToInt32(Session["Ca_ID"]);

            int cdid = 0;
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {


                dbOperations dbo = new dbOperations();


                cdid = dbo.jsEducation(Ca_ID, Convert.ToString(col["eduSchool"]), Convert.ToString(col["eduSchoolcountry"]), Convert.ToString(col["eduDegree"]), Convert.ToString(col["eduField"]), Convert.ToString(col["eduGrade"]), Convert.ToString(col["edufrommonth"]), Convert.ToString(col["edufromyear"]), Convert.ToString(col["edutomonth"]), Convert.ToString(col["edutoyear"]), Convert.ToBoolean(col["educurrentlystudy"]), Convert.ToString(col["eduActivities"]));

                if (uploadmedia != null)
                {
                    // save Media
                    string extension = Path.GetExtension(uploadmedia.FileName);
                    extension = extension.TrimStart('.');
                    string fileName = Path.GetFileNameWithoutExtension(uploadmedia.FileName);

                    StringBuilder sbcd = new StringBuilder();
                    sbcd.AppendFormat("insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType,cd_Type,cd_caedu_id) values('{0}',{1},{2},'{3}','{4}','{5}','{6}',{7}); select @@identity;", DateTime.Now, Ca_ID, 1, fileName, extension, "", "MEDIA", cdid);

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




        [ValidateInput(false)]
        [HttpPost]
        public ActionResult JobSeekerEducation(FormCollection col, HttpPostedFileBase uploadmedia)
        {
            int Ca_ID = Convert.ToInt32(Session["Ca_ID"]);

            int cdid = 0;
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {


                dbOperations dbo = new dbOperations();

                if (Convert.ToString(col["CaEdu_ID"]) != "") // updating
                {
                    dbo.jsEducationUpdate(Convert.ToInt32(col["CaEdu_ID"]), Convert.ToString(col["eduSchool"]), Convert.ToString(col["eduSchoolcountry"]), Convert.ToString(col["eduDegree"]), Convert.ToString(col["eduField"]), Convert.ToString(col["eduGrade"]), Convert.ToString(col["edufrommonth"]), Convert.ToString(col["edufromyear"]), Convert.ToString(col["edutomonth"]), Convert.ToString(col["edutoyear"]), Convert.ToBoolean(col["educurrentlystudy"]), Convert.ToString(col["eduActivities"]));
                    cdid = Convert.ToInt32(col["CaEdu_ID"]);
                }
                else
                {
                    cdid = dbo.jsEducation(Ca_ID, Convert.ToString(col["eduSchool"]), Convert.ToString(col["eduSchoolcountry"]), Convert.ToString(col["eduDegree"]), Convert.ToString(col["eduField"]), Convert.ToString(col["eduGrade"]), Convert.ToString(col["edufrommonth"]), Convert.ToString(col["edufromyear"]), Convert.ToString(col["edutomonth"]), Convert.ToString(col["edutoyear"]), Convert.ToBoolean(col["educurrentlystudy"]), Convert.ToString(col["eduActivities"]));
                }
                if (uploadmedia != null)
                {
                    // save Media
                    string extension = Path.GetExtension(uploadmedia.FileName);
                    extension = extension.TrimStart('.');
                    string fileName = Path.GetFileNameWithoutExtension(uploadmedia.FileName);

                    StringBuilder sbcd = new StringBuilder();
                    sbcd.AppendFormat("insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType,cd_Type,cd_caedu_id) values('{0}',{1},{2},'{3}','{4}','{5}','{6}',{7}); select @@identity;", DateTime.Now, Ca_ID, 1, fileName, extension, "", "MEDIA", cdid);

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



        [ValidateInput(false)]
        [HttpPost]
        public ActionResult CandidateEducationUpdate(FormCollection col, HttpPostedFileBase uploadmedia)
        {
            int Ca_ID = Convert.ToInt32(Session["Ca_ID"]);


            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                string[] tomonth = Convert.ToString(col["edutomonth"]).Split(',');
                string[] toyear = Convert.ToString(col["edutoyear"]).Split(',');

                dbOperations dbo = new dbOperations();
                dbo.jsEducationUpdate(Convert.ToInt32(col["CaEdu_ID"]), Convert.ToString(col["eduSchool"]), Convert.ToString(col["eduSchoolcountry"]), Convert.ToString(col["eduDegree"]), Convert.ToString(col["eduField"]), Convert.ToString(col["eduGrade"]), Convert.ToString(col["edufrommonth"]), Convert.ToString(col["edufromyear"]), tomonth[0], toyear[0], Convert.ToBoolean(col["educurrentlystudy"]), Convert.ToString(col["eduActivities"]));

                if (uploadmedia != null)
                {
                    // save Media
                    string extension = Path.GetExtension(uploadmedia.FileName);
                    extension = extension.TrimStart('.');
                    string fileName = Path.GetFileNameWithoutExtension(uploadmedia.FileName);

                    StringBuilder sbcd = new StringBuilder();
                    sbcd.AppendFormat("insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType,cd_Type) values('{0}',{1},{2},'{3}','{4}','{5}','{6}'); select @@identity;", DateTime.Now, Ca_ID, 1, fileName, extension, "", "MEDIA");

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

            return RedirectToAction("JobSeekerProfile");
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


        public ActionResult NoOtherSkillAdd()
        {
            dbOperations dbo = new dbOperations();
            dbo.jsNOOSkills(Convert.ToInt32(Session["Ca_ID"]));
            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }



        [HttpPost]
        public ActionResult JobseekerOSkills(FormCollection col)
        {

            dbOperations dbo = new dbOperations();

            dbo.jsOSkills(Convert.ToInt32(Session["Ca_ID"]), Convert.ToInt32(col["OSkillID"]), Convert.ToInt32(col["OSkillLevelID"]));


            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult getActivationStatus(decimal Ca_ID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<CandidateNew> cask = db.Database.SqlQuery<CandidateNew>("select ca_id from candidates where ca_active=1 and ca_id=" + Ca_ID).ToList();
                return new JsonpResult
                {
                    ContentEncoding = Encoding.UTF8,
                    Data = cask,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                };
            }
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

        public ActionResult getJobSeekerOSkills(decimal Ca_ID)
        {

            dbOperations dbo = new dbOperations();
            List<CandidateSkills> cask = dbo.getCandidateOSkills(Ca_ID);



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
        public ActionResult JobseekerCareerMove(FormCollection col, HttpPostedFileBase uploadcpr)
        {
            int Ca_ID = Convert.ToInt32(Session["Ca_ID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                if (uploadcpr != null)
                {
                    // save ID
                    string extension = Path.GetExtension(uploadcpr.FileName);
                    extension = extension.TrimStart('.');
                    string fileName = Path.GetFileNameWithoutExtension(uploadcpr.FileName);

                    StringBuilder sbcd = new StringBuilder();
                    sbcd.AppendFormat("insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType,cd_Type) values('{0}',{1},{2},'{3}','{4}','{5}','{6}'); select @@identity;", DateTime.Now, Ca_ID, 1, fileName, extension, "", "ID");

                    decimal Cd_ID = 0;

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

                    dbOperations dbo = new dbOperations();
                    dbo.jsCareerMove(Convert.ToInt32(Session["Ca_ID"]), Convert.ToInt32(col["JobTitleID"]), Convert.ToInt32(col["FunctionID"]), Convert.ToInt32(col["IndustryID"]), Convert.ToInt32(col["ExperienceID"]), Convert.ToInt32(col["ExpectedSalaryID"]));
                    CandidateActivationMail(Convert.ToInt32(Session["Ca_ID"]));
                }
                else
                {
                    return RedirectToAction("~/CanRegIdealCareerMove/Incomplete");
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
        public ActionResult JobseekerCareerMoveUpdate(FormCollection col)
        {
            int Ca_ID = Convert.ToInt32(Session["Ca_ID"]);
            dbOperations dbo = new dbOperations();
            dbo.jsCareerMove(Convert.ToInt32(Session["Ca_ID"]), Convert.ToInt32(col["JobTitleID"]), Convert.ToInt32(col["FunctionID"]), Convert.ToInt32(col["IndustryID"]), Convert.ToInt32(col["ExperienceID"]), Convert.ToInt32(col["ExpectedSalaryID"]));

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }




        [HttpPost]
        public ActionResult RecruiterSectors2(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Delete from [CompanySectors] where cs_co_CompanyId=" + Convert.ToInt32(Session["CompanyID"]));
                string sectorsvv = Convert.ToString(col["checkeditems"]);
                string[] eachsector = sectorsvv.Split(',');

                dbOperations dbo = new dbOperations();
                int count = 0;

                foreach (var sec in eachsector)
                {
                    if (count < 3)
                    {
                        dbo.CompanySectors(Convert.ToInt32(Session["CompanyID"]), Convert.ToInt32(sec));
                        count++;
                    }
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
        public ActionResult RecruiterPhotoGallery(HttpPostedFileBase file)
        {

            SqlCon mycon = new SqlCon();

            int CompanyID = Convert.ToInt32(Session["CompanyID"]);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("insert into [CompanyPhotoGallery_T] (Co_ID) values({0});select @@identity as pgid", CompanyID);

            mycon.sqlConOpen();
            mycon.sqlCmd_.CommandText = sb.ToString();
            int pgid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());


            mycon.sqlConClose();



            if (file != null)
            {
                // save CV
                string extension = Path.GetExtension(file.FileName);
                extension = extension.TrimStart('.');
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);

                StringBuilder sbcd = new StringBuilder();

                mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "SELECT pg_Guid from CompanyPhotoGallery_T where pgid=@pgid;update CompanyPhotoGallery_T set PhotoExtension=@extn  where pgid=@pgid";
                mycon.sqlCmd_.Parameters.AddWithValue("@pgid", pgid);
                mycon.sqlCmd_.Parameters.AddWithValue("@extn", extension);

                mycon.sqlConOpen();
                string cd_Guid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
                cd_Guid = cd_Guid.ToUpper();
                mycon.sqlConClose();

                string folderpath = Server.MapPath("~/Documents/PhotoGallery/" + cd_Guid.Substring(0, 2) + "/" + cd_Guid.Substring(2, 2) + "/" + cd_Guid.Substring(4, 2));
                DirectoryInfo dir = new DirectoryInfo(folderpath);
                if (!dir.Exists)
                {
                    dir.Create();
                }


                file.SaveAs(folderpath + "/" + cd_Guid + "." + extension);
            }


            return RedirectToAction("Recruiter");
        }


        [ValidateInput(false)]
        [HttpPost]
        public ActionResult RecruiterTestimonial(HttpPostedFileBase file, FormCollection col)
        {

            SqlCon mycon = new SqlCon();

            int CompanyID = Convert.ToInt32(Session["CompanyID"]);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("insert into [CompanyTestimonials_T] (Co_ID,CTDetails) values({0},'{1}');select @@identity as ctid", CompanyID, Convert.ToString(col["Testimonial"]));

            mycon.sqlConOpen();
            mycon.sqlCmd_.CommandText = sb.ToString();
            int ctid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());


            mycon.sqlConClose();



            if (file != null)
            {
                // save CV
                string extension = Path.GetExtension(file.FileName);
                extension = extension.TrimStart('.');
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);

                StringBuilder sbcd = new StringBuilder();

                mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "SELECT CT_GUID from CompanyTestimonials_T where ctid=@ctid;update CompanyTestimonials_T set [CTPicExtension]=@extn  where ctid=@ctid";
                mycon.sqlCmd_.Parameters.AddWithValue("@ctid", ctid);
                mycon.sqlCmd_.Parameters.AddWithValue("@extn", extension);

                mycon.sqlConOpen();
                string CT_GUID = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
                CT_GUID = CT_GUID.ToUpper();
                mycon.sqlConClose();

                string folderpath = Server.MapPath("~/Documents/Testimonials/" + CT_GUID.Substring(0, 2) + "/" + CT_GUID.Substring(2, 2) + "/" + CT_GUID.Substring(4, 2));
                DirectoryInfo dir = new DirectoryInfo(folderpath);
                if (!dir.Exists)
                {
                    dir.Create();
                }


                file.SaveAs(folderpath + "/" + CT_GUID + "." + extension);
            }


            return RedirectToAction("Recruiter");
        }



        [ValidateInput(false)]
        [HttpPost]
        public ActionResult RecruiterMedias(HttpPostedFileBase mediafile, FormCollection col)
        {

            SqlCon mycon = new SqlCon();

            int CompanyID = Convert.ToInt32(Session["CompanyID"]);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("insert into [CompanyMedia_T] (Co_ID,CMTitle) values({0},'{1}');select @@identity as cmid", CompanyID, Convert.ToString(col["Title"]));

            mycon.sqlConOpen();
            mycon.sqlCmd_.CommandText = sb.ToString();
            int cmid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());


            mycon.sqlConClose();



            if (mediafile != null)
            {
                // save CV
                string extension = Path.GetExtension(mediafile.FileName);
                extension = extension.TrimStart('.');
                string fileName = Path.GetFileNameWithoutExtension(mediafile.FileName);

                StringBuilder sbcd = new StringBuilder();

                mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "SELECT CM_GUID from CompanyMedia_T where cmid=@cmid;update CompanyMedia_T set [CMPicExtension]=@extn  where cmid=@cmid";
                mycon.sqlCmd_.Parameters.AddWithValue("@cmid", cmid);
                mycon.sqlCmd_.Parameters.AddWithValue("@extn", extension);

                mycon.sqlConOpen();
                string CM_GUID = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
                CM_GUID = CM_GUID.ToUpper();
                mycon.sqlConClose();

                string folderpath = Server.MapPath("~/Documents/Medias/" + CM_GUID.Substring(0, 2) + "/" + CM_GUID.Substring(2, 2) + "/" + CM_GUID.Substring(4, 2));
                DirectoryInfo dir = new DirectoryInfo(folderpath);
                if (!dir.Exists)
                {
                    dir.Create();
                }


                mediafile.SaveAs(folderpath + "/" + CM_GUID + "." + extension);
            }


            return RedirectToAction("Recruiter");
        }




        [HttpPost]
        public ActionResult RecruiterHeader(FormCollection col, HttpPostedFileBase file)
        {

            SqlCon mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "SELECT co_guid from Companies where co_id=@co_id";
            mycon.sqlCmd_.Parameters.AddWithValue("@co_id", Convert.ToInt32(Session["CompanyID"]));
            mycon.sqlConOpen();
            string coguid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
            coguid = coguid.ToUpper();
            mycon.sqlConClose();

            string folderpath = Server.MapPath("~/Documents/Header/" + coguid.Substring(0, 2) + "/" + coguid.Substring(2, 2) + "/" + coguid.Substring(4, 2));
            DirectoryInfo dir = new DirectoryInfo(folderpath);
            if (!dir.Exists)
            {
                dir.Create();
            }

            string extension = Path.GetExtension(file.FileName);

            dbOperations dbo = new dbOperations();
            dbo.CompanyHeaderExt(Convert.ToInt32(Session["CompanyID"]), extension);

            file.SaveAs(folderpath + "/" + coguid + extension);


            return RedirectToAction("Recruiter");
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

        public ActionResult PaymentSuccess()
        {


            return View();
        }

        public ActionResult PaymentFailed()
        {


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
                bmp.Save(docid.Replace("-original", ""), ImageFormat.Jpeg);
                bmp.Dispose();
                image.Dispose();

            }
            else if (thumbWidth == 70)
            {
                bmp.Save(docid.Replace("-original", "-small"), ImageFormat.Jpeg);
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

        public ActionResult UploadCV(HttpPostedFileBase uploadcv)
        {

            if (uploadcv != null)
            {
                int Ca_ID = Convert.ToInt32(Session["Ca_ID"]);
                decimal Cd_ID = 0;
                // save CV
                string extension = Path.GetExtension(uploadcv.FileName);
                extension = extension.TrimStart('.');
                string fileName = Path.GetFileNameWithoutExtension(uploadcv.FileName);

                StringBuilder sbcd = new StringBuilder();
                sbcd.AppendFormat("delete from CandidateDocuments where cd_ca_Candidateid={1} and cd_Type='CV';insert into [CandidateDocuments] (cd_Created,cd_ca_CandidateId,cd_doct_DocumentTypeId,cd_OriginalName,cd_FileExtension,cd_MimeType,cd_Type) values('{0}',{1},{2},'{3}','{4}','{5}','{6}'); select @@identity;", DateTime.Now, Ca_ID, 1, fileName, extension, "", "CV");
                using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
                {
                    Cd_ID = db.Database.SqlQuery<decimal>(sbcd.ToString()).SingleOrDefault();
                }


                SqlCon mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "SELECT cd_Guid from CandidateDocuments where Cd_ID=@Cd_ID";
                mycon.sqlCmd_.Parameters.AddWithValue("@Cd_ID", Cd_ID);
                mycon.sqlConOpen();
                string cd_Guid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
                cd_Guid = cd_Guid.ToUpper();
                mycon.sqlConClose();

                string folderpath = Server.MapPath("~/Documents/cvs/" + cd_Guid.Substring(0, 2) + "/" + cd_Guid.Substring(2, 2) + "/" + cd_Guid.Substring(4, 2));
                DirectoryInfo dir = new DirectoryInfo(folderpath);
                if (!dir.Exists)
                {
                    dir.Create();
                }


                uploadcv.SaveAs(folderpath + "/" + cd_Guid + "." + extension);
            }
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

                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4  and lm_Deleted is null order by lm_Ordinal").ToList();
                dbOperations dbo = new dbOperations();
                ViewBag.FunctionalTitles = dbo.getlist(17);
            }



            return View();
        }

        public ActionResult JobSearch2()
        {

            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["Ca_ID"]);
            ViewBag.Candidateid = candidateid;

            //if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            //int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30,32) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Sectors1 = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Agegroup = ddLists.Where(x => x.lm_lt_ListTypeId == 32).OrderBy(x => x.lm_Value).ToList();

                //var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                //int companyid = recruiter[0].ct_co_CompanyId;

                //var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                //string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                //ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                //ViewBag.Company = company[0].co_Name;
                //ViewBag.Name = recruiter[0].ct_Forename;
                //ViewBag.Email = recruiter[0].ct_EmailAddress;
                //ViewBag.Phone = recruiter[0].ct_Telephone;

                //ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4  and lm_Deleted is null order by lm_Ordinal").ToList();

                dbOperations dbo = new dbOperations();
                ViewBag.FunctionalTitles = dbo.getlist(17);
                //List<JobSeekers> jobseekers = db.Database.SqlQuery<JobSeekers>("select ca_Id,ca_GUID, ca_FirstName,ca_Surname,ca_EmailAddress,ca_PhoneMobile,ca_DateOfBirth,ca_Gender,ca_photoextension from candidates where ca_id in (select top 20 app_ca_CandidateId from Applications where app_vc_VacancyId in (select vc_id from Vacancies where vc_co_CompanyId=" + companyid + ") order by app_id desc)").ToList();

                List<JobSeekers> jos = new List<JobSeekers>();

                JobSeekers jobskr;

                //foreach (var js in jobseekers)
                //{
                //    jobskr = new JobSeekers();

                //    jobskr.ca_FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(js.ca_FirstName.ToLower());
                //    jobskr.ca_Surname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(js.ca_Surname.ToLower());
                //    jobskr.ca_EmailAddress = js.ca_EmailAddress.ToLower();
                //    jobskr.ca_PhoneMobile = js.ca_PhoneMobile.Replace("00973", "").Replace("+973", "").Replace(" ", "");
                //    jobskr.ca_DateOfBirth = js.ca_DateOfBirth;
                //    jobskr.Age = DateTime.Now.Year - js.ca_DateOfBirth.Value.Year;
                //    jobskr.ca_Gender = js.ca_Gender == "M" ? "Male" : "Female";
                //    jobskr.ca_photoextension = js.ca_photoextension;
                //    jobskr.ca_GUID = js.ca_GUID;
                //    string extn = js.ca_photoextension == "" ? "jpg" : js.ca_photoextension;
                //    jobskr.ProfilePic = "http://www.jobs4bahrainis.com/documents/photos/" + js.ca_GUID.ToString().Substring(0, 2) + "/" + js.ca_GUID.ToString().Substring(2, 2) + "/" + js.ca_GUID.ToString().Substring(4, 2) + "/" + js.ca_GUID.ToString() + "." + extn;
                //    jos.Add(jobskr);
                //}
                ViewBag.jobseekers = jos.ToList();

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

                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4  and lm_Deleted is null order by lm_Ordinal").ToList();
                //dbOperations dbo = new dbOperations();
                ViewBag.FunctionalTitles = dbo.getlist(17);
            }



            return View();
        }
        public ActionResult JobSearch3()
        {

            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["Ca_ID"]);
            ViewBag.Candidateid = candidateid;

            //if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            //int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30,32,6) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                //ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Sectors1 = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Agegroup = ddLists.Where(x => x.lm_lt_ListTypeId == 32).OrderBy(x => x.lm_Value).ToList();
                ViewBag.JobType = ddLists.Where(x => x.lm_lt_ListTypeId == 6).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();

                //var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                //int companyid = recruiter[0].ct_co_CompanyId;

                //var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                //string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                //ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                //ViewBag.Company = company[0].co_Name;
                //ViewBag.Name = recruiter[0].ct_Forename;
                //ViewBag.Email = recruiter[0].ct_EmailAddress;
                //ViewBag.Phone = recruiter[0].ct_Telephone;

                //ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4  and lm_Deleted is null order by lm_Ordinal").ToList();

                dbOperations dbo = new dbOperations();
                ViewBag.FunctionalTitles = dbo.getlist(17);
                //List<JobSeekers> jobseekers = db.Database.SqlQuery<JobSeekers>("select ca_Id,ca_GUID, ca_FirstName,ca_Surname,ca_EmailAddress,ca_PhoneMobile,ca_DateOfBirth,ca_Gender,ca_photoextension from candidates where ca_id in (select top 20 app_ca_CandidateId from Applications where app_vc_VacancyId in (select vc_id from Vacancies where vc_co_CompanyId=" + companyid + ") order by app_id desc)").ToList();

                List<JobSeekers> jos = new List<JobSeekers>();

                JobSeekers jobskr;

                //foreach (var js in jobseekers)
                //{
                //    jobskr = new JobSeekers();

                //    jobskr.ca_FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(js.ca_FirstName.ToLower());
                //    jobskr.ca_Surname = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(js.ca_Surname.ToLower());
                //    jobskr.ca_EmailAddress = js.ca_EmailAddress.ToLower();
                //    jobskr.ca_PhoneMobile = js.ca_PhoneMobile.Replace("00973", "").Replace("+973", "").Replace(" ", "");
                //    jobskr.ca_DateOfBirth = js.ca_DateOfBirth;
                //    jobskr.Age = DateTime.Now.Year - js.ca_DateOfBirth.Value.Year;
                //    jobskr.ca_Gender = js.ca_Gender == "M" ? "Male" : "Female";
                //    jobskr.ca_photoextension = js.ca_photoextension;
                //    jobskr.ca_GUID = js.ca_GUID;
                //    string extn = js.ca_photoextension == "" ? "jpg" : js.ca_photoextension;
                //    jobskr.ProfilePic = "http://www.jobs4bahrainis.com/documents/photos/" + js.ca_GUID.ToString().Substring(0, 2) + "/" + js.ca_GUID.ToString().Substring(2, 2) + "/" + js.ca_GUID.ToString().Substring(4, 2) + "/" + js.ca_GUID.ToString() + "." + extn;
                //    jos.Add(jobskr);
                //}
                ViewBag.jobseekers = jos.ToList();

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
                //ViewBag.Experience = db.Database.SqlQuery<CandidateExperience>("SELECT * from [dbo].[CandidateExperience]  where ca_id=" + candidateid).ToList();
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

                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4  and lm_Deleted is null order by lm_Ordinal").ToList();
                //dbOperations dbo = new dbOperations();
                ViewBag.FunctionalTitles = dbo.getlist(17);
            }



            return View();
        }

        public ActionResult JobSearchResult(string keyword, string[] chksector, string salary, int? qualification, string experience, int? page)
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
            }
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
                ViewBag.totalSearchResult = totalSearchCount;

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



            return View(candidateResultFilter);
        }

        //11Dec2018
        //public ActionResult JobSearchResult(string keyword, string[] chksector, string salary, int? qualification, string experience, int? page)
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
        //    }
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
        //    List<JobSearchResultModel> jobSearchResult = null;
        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {

        //        SqlParameter qualififcationParam = AddParameter("p_qaulification", qualification, typeof(Int32));
        //        SqlParameter salaryFromParam = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
        //        SqlParameter salaryToParam = AddParameter("p_salaryto", salaryTo, typeof(decimal));
        //        SqlParameter expFromParam = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
        //        SqlParameter expToParam = AddParameter("p_experienceTo", expTo, typeof(Int32));
        //        SqlParameter keywordParam = AddParameter("p_keyword", keyword, typeof(String));
        //        SqlParameter searchTypeParam = AddParameter("p_searchType", 1, typeof(Int32));
        //        SqlParameter pageSizeParam = AddParameter("p_pageSize", pageSize, typeof(Int32));
        //        SqlParameter pageIndexParam = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

        //        object[] parameters = new object[] { qualififcationParam
        //                                            , salaryFromParam, salaryToParam, expFromParam,expToParam
        //                                            ,keywordParam,searchTypeParam,pageSizeParam,pageIndexParam};

        //        string sqlQry = @"proc_JobSearch @p_qaulification,@p_salaryfrom,@p_salaryto,@p_experienceFrom,@p_experienceTo,@p_keyword,@p_searchType,
        //                          @p_pageSize,@p_pageIndex";

        //        jobSearchResult = db.Database.SqlQuery<JobSearchResultModel>(sqlQry, parameters).ToList();

        //        SqlParameter qualififcationParam2 = AddParameter("p_qaulification", qualification, typeof(Int32));
        //        SqlParameter salaryFromParam2 = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
        //        SqlParameter salaryToParam2 = AddParameter("p_salaryto", salaryTo, typeof(decimal));
        //        SqlParameter expFromParam2 = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
        //        SqlParameter expToParam2 = AddParameter("p_experienceTo", expTo, typeof(Int32));
        //        SqlParameter keywordParam2 = AddParameter("p_keyword", keyword, typeof(String));
        //        SqlParameter searchTypeParam2 = AddParameter("p_searchType", 2, typeof(Int32));
        //        SqlParameter pageSizeParam2 = AddParameter("p_pageSize", pageSize, typeof(Int32));
        //        SqlParameter pageIndexParam2 = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

        //        parameters = new object[] { qualififcationParam2
        //                                            , salaryFromParam2, salaryToParam2, expFromParam2,expToParam2
        //                                            ,keywordParam2,searchTypeParam2,pageSizeParam2,pageIndexParam2};
        //        totalSearchCount = db.Database.SqlQuery<int>(sqlQry, parameters).FirstOrDefault();
        //        ViewBag.totalSearchResult = totalSearchCount;

        //    }
        //    // this is just to test with mock data
        //    //for (int i = 0; i < 100; i++)
        //    //{
        //    //    jobSearchResult.Add(jobSearchResult[0]);
        //    //}
        //    //totalSearchCount = jobSearchResult.Count;
        //    //var pageNumber = page ?? 1; // if no page was specified in the querystring, default to the first page (1)
        //    //var candidateResultFilter = jobSearchResult.ToPagedList(pageNumber, 10); // will only contain 25 products max because of the pageSize


        //    var candidateResultFilter = new StaticPagedList<JobSearchResultModel>(jobSearchResult, pageIndex, pageSize, totalSearchCount);



        //    return View(candidateResultFilter);
        //}

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
            //if (Session["RecruiterID"] == null) { return RedirectToAction("../Login/-/Packages"); }


            return View();
        }


        [HttpPost]
        public ActionResult PaymentMode(FormCollection col)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("../Login/-/Packages"); }

            ViewBag.PackageID = Convert.ToInt32(col["PackageID"]);
            Session["PackageID"] = Convert.ToInt32(col["PackageID"]);

            return View();
        }

        [HttpPost]
        public ActionResult PaymentModeSelected(FormCollection col)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("../Login/-/Packages"); }

            if (Convert.ToInt32(col["paymentmode"]) == 1)
            {
                return RedirectToAction("../CreditCardPayment");
            }
            else
            {
                dbOperations dbo = new dbOperations();


                int invoiceno = dbo.OrderCreate(Convert.ToInt32(Session["CompanyID"]), Convert.ToInt32(Session["RecruiterID"]), "INV", Convert.ToInt32(Session["PackageID"]));
                InvoiceGenerator(invoiceno);
            }
            return View();
        }



        public ActionResult CreditCardPayment()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("../Login/-/Packages"); }


            if (Session["PackageID"] != null)
            {


                dbOperations dbo = new dbOperations();
                int invoiceno = dbo.OrderCreate(Convert.ToInt32(Session["CompanyID"]), Convert.ToInt32(Session["RecruiterID"]), "CC", Convert.ToInt32(Session["PackageID"]));

                ViewBag.invoiceno = invoiceno;
                Session["invoiceno"] = invoiceno;

                InvoiceGenerator(invoiceno);
                String response = null;




                Merchant merchant = new Merchant();

                StringBuilder url = new StringBuilder();
                merchant.GatewayUrl = merchant.GatewayHost.ToString();

                Connection connection = new Connection(merchant);


                StringBuilder data = new StringBuilder();
                data.Append("apiOperation=CREATE_SESSION&merchant=" + merchant.MerchantId);
                data.Append("&apiUsername=merchant.E05783950");
                data.Append("&apiPassword=da6541fe5542f13e443a8ca998ac0c21");

                response = connection.SendTransaction(data.ToString());




                NameValueCollection respValues = new NameValueCollection();
                if (response != null && response.Length > 0)
                {
                    String[] responses = response.Split('&');
                    foreach (String responseField in responses)
                    {
                        String[] field = responseField.Split('=');
                        respValues.Add(field[0], HttpUtility.UrlDecode(field[1]));
                    }
                }
                ViewBag.sessionid = respValues["session.id"];
            }
            else
            {
                RedirectToAction("../Login/-/Packages");
            }


            return View();
        }

        public void InvoiceGenerator(int invoiceno)
        {

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<CSPackage> pkg = db.Database.SqlQuery<CSPackage>("select * from [dbo].[CSPackage_T] where packageid=" + Convert.ToInt32(Session["PackageID"])).ToList();

                string packagetext = "";
                int amountpermonth = 0, vat = 0, grandtotal = 0, amountperyear = 0;

                foreach (var p in pkg)
                {
                    amountpermonth = p.AmountPerYear / 12;
                    amountperyear = p.AmountPerYear;
                    vat = p.AmountPerYear * 5 / 100;
                    //grandtotal = p.AmountPerYear + vat;
                    grandtotal = p.AmountPerYear;
                    packagetext = "" + p.PackageName + " Package for One year (BHD " + amountpermonth + " x 12 months)";
                }

                common cmn = new common();



                string path = Server.MapPath("~/Templates/Invoice.html");
                string bodycontent = System.IO.File.ReadAllText(path);
                bodycontent = bodycontent.Replace("##INVOICENUMBER##", invoiceno.ToString()).Replace("##INVOICEDATE##", DateTime.Now.ToString("dd-MMMM-yyyy"));
                bodycontent = bodycontent.Replace("##PACKAGETEXT##", packagetext).Replace("##ANNUALAMOUNT##", amountperyear.ToString());
                //.Replace("##VATAMOUNT##", vat.ToString());
                bodycontent = bodycontent.Replace("##GRANDTOTAL##", grandtotal.ToString()).Replace("##AMOUNTINWORDS##", cmn.ConvertNumbertoWords(grandtotal));



                int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
                int companyid = Convert.ToInt32(Session["CompanyID"]);



                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();


                bodycontent = bodycontent.Replace("##COMPANYNAME##", company[0].co_Name).Replace("##CONTACTPERSON##", recruiter[0].ct_Forename + " " + recruiter[0].ct_Surname).Replace("##ADDRESS##", company[0].co_PostalAddress).Replace("##PHONENUMBER##", company[0].co_Telephone).Replace("##EMAILADDRESS##", recruiter[0].ct_EmailAddress);


                cmn.SendMail(recruiter[0].ct_EmailAddress, "Jobs4Bahrainis.com - Invoice", bodycontent);
                cmn.SendMail("Bayden.tierney@jobs4bahrainis.com", "Jobs4Bahrainis.com - Invoice", bodycontent);
                cmn.SendMail("binju.paul@tradearabia.net", "Jobs4Bahrainis.com - Invoice", bodycontent);
                cmn.SendMail("mohamed.Ghazwan@jobs4bahrainis.com", "Jobs4Bahrainis.com - Invoice", bodycontent);




                ViewBag.HtmlInvoice = bodycontent;

            }

        }

        public ActionResult SubscriptionThanks()
        {


            return View();
        }



        public ActionResult SuccessfullPayment(FormCollection col)
        {

            if (Convert.ToString(Session["invoiceno"]) == "") { return RedirectToAction("../Login/-/Packages"); }

            dbOperations dbo = new dbOperations();
            dbo.CS_AssignSubscriptiontoCompany(Convert.ToInt32(Session["CompanyID"]), Convert.ToInt32(Session["RecruiterID"]), Convert.ToInt32(Session["PackageID"]));

            ReceiptGenerator(Convert.ToInt32(Session["invoiceno"]));



            return RedirectToAction("../PaymentSuccess");
        }



        public void ReceiptGenerator(int invoiceno)
        {

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<CSPackage> pkg = db.Database.SqlQuery<CSPackage>("select * from [dbo].[CSPackage_T] where packageid=" + Convert.ToInt32(Session["PackageID"]) + ";update Order_T set paid=1 where orderno=" + invoiceno).ToList();

                string packagetext = "";
                int amountpermonth = 0, vat = 0, grandtotal = 0, amountperyear = 0;

                foreach (var p in pkg)
                {
                    amountpermonth = p.AmountPerYear / 12;
                    amountperyear = p.AmountPerYear;
                    vat = p.AmountPerYear * 5 / 100;
                    grandtotal = p.AmountPerYear + vat;
                    packagetext = "" + p.PackageName + " Package for One year (BHD " + amountpermonth + " x 12 months)";
                }

                common cmn = new common();



                string path = Server.MapPath("~/Templates/Receipt.html");
                string bodycontent = System.IO.File.ReadAllText(path);
                bodycontent = bodycontent.Replace("##INVOICENUMBER##", invoiceno.ToString()).Replace("##INVOICEDATE##", DateTime.Now.ToString("dd-MMMM-yyyy"));
                bodycontent = bodycontent.Replace("##PACKAGETEXT##", packagetext).Replace("##ANNUALAMOUNT##", amountperyear.ToString());
                //.Replace("##VATAMOUNT##", vat.ToString());
                bodycontent = bodycontent.Replace("##GRANDTOTAL##", grandtotal.ToString()).Replace("##AMOUNTINWORDS##", cmn.ConvertNumbertoWords(grandtotal));



                int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
                int companyid = Convert.ToInt32(Session["CompanyID"]);



                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();


                bodycontent = bodycontent.Replace("##COMPANYNAME##", company[0].co_Name).Replace("##CONTACTPERSON##", recruiter[0].ct_Forename + " " + recruiter[0].ct_Surname).Replace("##ADDRESS##", company[0].co_PostalAddress).Replace("##PHONENUMBER##", company[0].co_Telephone).Replace("##EMAILADDRESS##", recruiter[0].ct_EmailAddress);


                cmn.SendMail(recruiter[0].ct_EmailAddress, "Jobs4Bahrainis.com - Successful Payment", bodycontent);
                cmn.SendMail("Bayden.tierney@jobs4bahrainis.com", "Jobs4Bahrainis.com - Successful Payment", bodycontent);
                cmn.SendMail("binju.paul@tradearabia.net", "Jobs4Bahrainis.com - Successful Payment", bodycontent);
                cmn.SendMail("mohamed.Ghazwan@jobs4bahrainis.com", "Jobs4Bahrainis.com - Successful Payment", bodycontent);


                ViewBag.HtmlInvoice = bodycontent;


                path = Server.MapPath("~/Templates/J4BProfileactivatedRecruiter.html");
                bodycontent = System.IO.File.ReadAllText(path);
                cmn.SendMail(recruiter[0].ct_EmailAddress, "Jobs4Bahrainis.com", bodycontent);


                Session["invoiceno"] = "";
            }

        }



        [HttpPost]
        public ActionResult CompanyManagement(FormCollection col, HttpPostedFileBase uploadcv, HttpPostedFileBase uploadcv2, HttpPostedFileBase uploadcv3, HttpPostedFileBase uploadcv4)
        {

            string Name = Convert.ToString(col["Name"]);
            string Position = Convert.ToString(col["Position"]);
            int CompanyID = Convert.ToInt32(Session["CompanyID"]);

            string Name2 = Convert.ToString(col["Name2"]);
            string Position2 = Convert.ToString(col["Position2"]);


            string Name3 = Convert.ToString(col["Name3"]);
            string Position3 = Convert.ToString(col["Position3"]);


            string Name4 = Convert.ToString(col["Name4"]);
            string Position4 = Convert.ToString(col["Position4"]);



            int companymanagementid = AddCompanyManagement(CompanyID);


            SqlCon mycon = new SqlCon();
            StringBuilder sb = new StringBuilder();
            mycon.sqlConOpen();
            sb.AppendFormat("update [CompanyManagement_T] set Name=@Name,Position=@Position,Name2=@Name2,Position2=@Position2,Name3=@Name3,Position3=@Position3,Name4=@Name4,Position4=@Position4  where companymanagementid=@companymanagementid");
            mycon.sqlCmd_.Parameters.AddWithValue("@companymanagementid", companymanagementid);
            mycon.sqlCmd_.Parameters.AddWithValue("@Name", Name);
            mycon.sqlCmd_.Parameters.AddWithValue("@Name2", Name2);
            mycon.sqlCmd_.Parameters.AddWithValue("@Name3", Name3);
            mycon.sqlCmd_.Parameters.AddWithValue("@Name4", Name4);
            mycon.sqlCmd_.Parameters.AddWithValue("@Position", Position);
            mycon.sqlCmd_.Parameters.AddWithValue("@Position2", Position2);
            mycon.sqlCmd_.Parameters.AddWithValue("@Position3", Position3);
            mycon.sqlCmd_.Parameters.AddWithValue("@Position4", Position4);

            mycon.sqlCmd_.CommandText = sb.ToString();
            Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
            mycon.sqlConClose();


            UpdateCompanyManagement(1, companymanagementid, uploadcv);

            if (Name2 != "")
            {
                UpdateCompanyManagement(2, companymanagementid, uploadcv2);
            }
            if (Name3 != "")
            {
                UpdateCompanyManagement(3, companymanagementid, uploadcv3);
            }
            if (Name4 != "")
            {
                UpdateCompanyManagement(4, companymanagementid, uploadcv4);
            }





            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult getCompanyContacts(int CompanyID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<CompanyContacts> comcontacts = db.Database.SqlQuery<CompanyContacts>("select ct_id,ct_Forename + ' ' + ct_Surname as name, usertype = case ct_superuser when 1 then 'Admin' when 0 then 'User' end from Contacts where ct_co_CompanyId =" + CompanyID).ToList();

                return new JsonpResult
                {
                    ContentEncoding = Encoding.UTF8,
                    Data = comcontacts,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                };
            }
        }

        public int AddCompanyManagement(int CompanyID)
        {
            SqlCon mycon = new SqlCon();
            StringBuilder sb = new StringBuilder();
            mycon.sqlConOpen();
            sb.AppendFormat("select companymanagementid from [CompanyManagement_T] where Co_ID={0};", CompanyID);
            mycon.sqlCmd_.CommandText = sb.ToString();
            int companymanagementid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
            mycon.sqlConClose();

            if (companymanagementid == 0)
            {
                mycon = new SqlCon();
                mycon.sqlConOpen();
                sb.AppendFormat("Insert into [CompanyManagement_T] (Co_ID) values({0});select @@identity as companymanagementid", CompanyID);
                mycon.sqlCmd_.CommandText = sb.ToString();
                companymanagementid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
                mycon.sqlConClose();
            }
            return companymanagementid;
        }


        public void UpdateCompanyManagement(int itemno, int companymanagementid, HttpPostedFileBase uploadcv)
        {

            SqlCon mycon = new SqlCon();


            if (uploadcv != null)
            {
                // save CV
                string extension = Path.GetExtension(uploadcv.FileName);
                extension = extension.TrimStart('.');
                string fileName = Path.GetFileNameWithoutExtension(uploadcv.FileName);

                StringBuilder sbcd = new StringBuilder();

                mycon = new SqlCon();

                if (itemno == 1)
                {
                    mycon.sqlCmd_.CommandText = "update CompanyManagement_T set extn=@extn  where companymanagementid=@companymanagementid;select cm_guid from CompanyManagement_T  where companymanagementid=@companymanagementid";
                }
                else if (itemno == 2)
                {
                    mycon.sqlCmd_.CommandText = "update CompanyManagement_T set extn2=@extn  where companymanagementid=@companymanagementid;select cm_guid from CompanyManagement_T  where companymanagementid=@companymanagementid";
                }
                else if (itemno == 3)
                {
                    mycon.sqlCmd_.CommandText = "update CompanyManagement_T set extn3=@extn  where companymanagementid=@companymanagementid;select cm_guid from CompanyManagement_T  where companymanagementid=@companymanagementid";
                }
                else if (itemno == 4)
                {
                    mycon.sqlCmd_.CommandText = "update CompanyManagement_T set extn4=@extn  where companymanagementid=@companymanagementid;select cm_guid from CompanyManagement_T  where companymanagementid=@companymanagementid";
                }

                mycon.sqlCmd_.Parameters.AddWithValue("@companymanagementid", companymanagementid);
                mycon.sqlCmd_.Parameters.AddWithValue("@extn", extension.Trim());

                mycon.sqlConOpen();
                string cd_Guid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
                cd_Guid = cd_Guid.ToUpper();
                mycon.sqlConClose();

                string folderpath = Server.MapPath("~/Documents/CoManagement/" + cd_Guid.Substring(0, 2) + "/" + cd_Guid.Substring(2, 2) + "/" + cd_Guid.Substring(4, 2));
                DirectoryInfo dir = new DirectoryInfo(folderpath);
                if (!dir.Exists)
                {
                    dir.Create();
                }


                uploadcv.SaveAs(folderpath + "/topmanagement" + itemno + "." + extension);
            }
        }


        [HttpPost]
        public ActionResult CompanyVideo(FormCollection col)
        {

            dbOperations dbo = new dbOperations();

            dbo.CompanyVideo(Convert.ToInt32(Session["CompanyID"]), Convert.ToString(col["Video"]));

            return RedirectToAction("Recruiter", "Home");

        }

        [HttpPost]
        public ActionResult companycontact(FormCollection col)
        {
            SqlConnection.ClearAllPools();
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                Contact ct = new Contact();
                ct.ct_EmailAddress = Convert.ToString(col["Email"]);
                ct.ct_Password = Convert.ToString(col["Password"]);
                ct.ct_Telephone = Convert.ToString(col["Phone"]);
                ct.ct_co_CompanyId = Convert.ToInt32(col["CompanyID"]);
                ct.ct_Forename = Convert.ToString(col["FirstName"]);
                ct.ct_Surname = Convert.ToString(col["LastName"]);
                ct.co_PositioninCompanyID = Convert.ToInt32(col["co_PositioninCompanyID"]);
                ct.ct_Superuser = Convert.ToBoolean(col["ct_Superuser"]);
                ct.ct_postjob = Convert.ToBoolean(col["ct_PostJob"]);
                ct.ct_SearchCV = Convert.ToBoolean(col["ct_SearchCV"]);
                ct.ct_LastUpdated = DateTime.Now;
                ct.ct_LastLogin = DateTime.Now;
                ct.ct_Created = DateTime.Now;
                ct.ct_GUID = Guid.NewGuid();
                ct.ct_ApplicationEmail = "";

                ct.ct_MarketingOptIn = false;
                ct.ct_st_StatusId = 20;
                ct.ct_LastUpdated = DateTime.Now;
                ct.ct_JobTitle = "";


                db.Contacts.Add(ct);
                db.SaveChanges();

            }

            return RedirectToAction("Recruiter", "Home");
        }


        [HttpPost]
        public ActionResult CompanyContactEdit(FormCollection col)
        {
            SqlConnection.ClearAllPools();
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                bool postjob = false, searchcv = false;

                if (col["ct_PostJob"] != null)
                {
                    postjob = true;
                }
                else
                {
                    postjob = false;
                }

                if (col["ct_SearchCV"] != null)
                {
                    searchcv = true;
                }
                else
                {
                    searchcv = false;
                }

                string sql = "";
                sql = "Update Contacts set ct_EmailAddress='" + Convert.ToString(col["Email"]) + "',ct_Password='" + Convert.ToString(col["Password"]) + "',ct_Telephone='" + Convert.ToString(col["Phone"]) + "',ct_Forename='" + Convert.ToString(col["FirstName"]) + "',ct_Surname='" + Convert.ToString(col["LastName"]) + "',co_PositioninCompanyID=" + Convert.ToInt32(col["co_PositioninCompanyID"]) + ", ct_Deleted=null,ct_Superuser='" + Convert.ToBoolean(col["ct_Superuser"]) + "',ct_PostJob='" + col["ct_PostJob"] + "',ct_SearchCV='" + col["ct_SearchCV"] + "' where ct_id=" + col["ContactID"];
                db.Database.ExecuteSqlCommand(sql);
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }



        public ActionResult CompanyContactDelete(int contactid)
        {
            SqlConnection.ClearAllPools();
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update Contacts set ct_deleted=getdate() where ct_id=" + contactid + " and ct_co_CompanyId=" + Convert.ToInt32(Session["CompanyID"]));
            }
            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult Employer(int companyid, string companyname)
        {



            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_co_CompanyId == companyid).ToList();



                ViewBag.Contacts = db.Database.SqlQuery<reccontacts>("select ct_Forename,ct_Surname,ct_EmailAddress,ct_Telephone,ct_Superuser,co_PositioninCompanyID,lm_Value  from Contacts ct join ListsMaster lm on lm.lm_Id = ct.co_PositioninCompanyID where ct_co_companyid=" + companyid).ToList();
                //db.Contacts.Where(x => x.ct_co_CompanyId == companyid).ToList();


                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();
                string extns = "";

                if (company[0].co_HasLogo == true)
                {
                    extns = company[0].co_LogoExtension.Trim() == "" ? ".jpg" : company[0].co_LogoExtension;
                    //"http://www.jobs4bahrainis.com/Logox/"
                    ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;
                }

                if (company[0].co_HeaderExtension != null)
                {
                    extns = company[0].co_HeaderExtension.Trim();

                    ViewBag.HeaderPic = "Documents/Header/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + extns;
                }


                ViewBag.Company = company[0].co_Name;
                ViewBag.CompanyID = company[0].co_Id;

                ViewBag.CompanyPhone = company[0].co_Telephone;
                ViewBag.PostalAddress = company[0].co_PostalAddress;
                ViewBag.LastUpdated = company[0].co_LastUpdated;

                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.SurName = recruiter[0].ct_Surname;

                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;
                ViewBag.CompanyProfile = company[0].co_Profile;
                ViewBag.Video = company[0].co_Video;
                ViewBag.CompanySizeID = company[0].co_CompanySizeID;
                ViewBag.PositionID = recruiter[0].co_PositioninCompanyID;

                ViewBag.co_VideoPublished = company[0].co_VideoPublished;
                ViewBag.co_GalleryPublished = company[0].co_GalleryPublished;
                ViewBag.co_TestimonialsPublished = company[0].co_TestimonialsPublished;
                ViewBag.co_MediaPublished = company[0].co_MediaPublished;
                ViewBag.co_ProfilePublished = company[0].co_ProfilePublished;

                ViewBag.CompanyProfile = company[0].co_Profile;
                ViewBag.CompanyProfile2 = company[0].co_Profile2;
                ViewBag.CompanyProfile3 = company[0].co_Profile3;
                ViewBag.CompanyProfile4 = company[0].co_Profile4;
                ViewBag.CompanyProfile5 = company[0].co_Profile5;


                ViewBag.ProfileTitle1 = company[0].co_ProfileTitle1;
                ViewBag.ProfileTitle2 = company[0].co_ProfileTitle2;
                ViewBag.ProfileTitle3 = company[0].co_ProfileTitle3;
                ViewBag.ProfileTitle4 = company[0].co_ProfileTitle4;
                ViewBag.ProfileTitle5 = company[0].co_ProfileTitle5;


                ViewBag.co_ProfilePicExtn1 = company[0].co_ProfilePicExtn1;
                ViewBag.co_ProfilePicExtn2 = company[0].co_ProfilePicExtn2;
                ViewBag.co_ProfilePicExtn3 = company[0].co_ProfilePicExtn3;
                ViewBag.co_ProfilePicExtn4 = company[0].co_ProfilePicExtn4;
                ViewBag.co_ProfilePicExtn5 = company[0].co_ProfilePicExtn5;


                ViewBag.co_ProfilePic1 = "Documents/ProfilePics/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/profileimage1" + trimPicExtensions(company[0].co_ProfilePicExtn1);
                ViewBag.co_ProfilePic2 = "Documents/ProfilePics/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/profileimage2" + trimPicExtensions(company[0].co_ProfilePicExtn2);
                ViewBag.co_ProfilePic3 = "Documents/ProfilePics/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/profileimage3" + trimPicExtensions(company[0].co_ProfilePicExtn3);
                ViewBag.co_ProfilePic4 = "Documents/ProfilePics/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/profileimage4" + trimPicExtensions(company[0].co_ProfilePicExtn4);
                ViewBag.co_ProfilePic5 = "Documents/ProfilePics/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/profileimage5" + trimPicExtensions(company[0].co_ProfilePicExtn5);


                ViewBag.ContactID = Convert.ToDecimal(Session["RecruiterID"]);


                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (31,25,4) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.CompanySize = ddLists.Where(x => x.lm_lt_ListTypeId == 31).OrderBy(x => x.lm_Id).ToList();

                foreach (var cs in ViewBag.CompanySize)
                {
                    if (cs.lm_Id == ViewBag.CompanySizeID)
                    {
                        ViewBag.ThisCompanySize = cs.lm_Value;
                    }
                }

                ViewBag.Position = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();

                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();

                ViewBag.CompanyBA = db.Database.SqlQuery<ListMaster>("Select * from ListsMaster where lm_Id in (select cs_lm_sectorid from CompanySectors where cs_co_companyid=" + companyid + ")").ToList();





                var subs = db.Database.SqlQuery<CSSubscription>(@"select top 1 s.SubscriptionID,p.PackageName,c.ct_Forename,c.ct_Surname, s.StartDate,s.EndDate,s.DOC as PurchaseDate from [dbo].[CSSubscription_T] s  join Contacts c on c.ct_Id = s.ContactID join CSPackage_T p on p.PackageID = s.PackageID where companyid=" + Convert.ToInt32(companyid) + " order by s.SubscriptionID desc").ToList();

                ViewBag.Subscription = subs;

                if (subs.Count != 0)
                {


                    ViewBag.CreditsAvailable = db.Database.SqlQuery<CCredits>(@"select ct.CreditType, v.VoucherMonth,v.VoucherYear,v.CreditBalance from [dbo].[CSVouchers_T] v join CSCreditType_T ct on v.CreditTypeID = ct.CreditTypeID where SubscriptionID = (select top 1 SubscriptionID from CSSubscription_T where CompanyID = " + Convert.ToInt32(companyid) + "  order by subscriptionid desc) order by VoucherYear, VoucherMonth").ToList();

                    List<CCredits> actualmonthlycredit = db.Database.SqlQuery<CCredits>("select CreditTypeID,CreditsPermonth as CreditBalance from  [dbo].[CSCredits_T] where packageid in ( select top 1 packageid from [dbo].[CSSubscription_T] where companyid=" + Convert.ToInt32(companyid) + " and enddate>getdate() order by startdate desc)").ToList();

                    List<CCredits> thismonthcredit = db.Database.SqlQuery<CCredits>(@"select v.credittypeid,v.CreditBalance from [dbo].[CSVouchers_T] v  where SubscriptionID = (select top 1 SubscriptionID from CSSubscription_T where CompanyID = " + Convert.ToInt32(companyid) + "  order by subscriptionid desc)  and v.vouchermonth = month(getdate()) and v.voucheryear = year(getdate())").ToList();

                    float totalmonthjobcredit = actualmonthlycredit.Where(x => x.CreditTypeID == 1).Select(x => x.CreditBalance).SingleOrDefault();
                    float totalmonthcvcredit = actualmonthlycredit.Where(x => x.CreditTypeID == 2).Select(x => x.CreditBalance).SingleOrDefault();

                    ViewBag.totalmonthjobcredit = totalmonthjobcredit;
                    ViewBag.totalmonthcvcredit = totalmonthcvcredit;

                    float jobcredit = thismonthcredit.Where(x => x.CreditTypeID == 1).Select(x => x.CreditBalance).SingleOrDefault();
                    float cvcredit = thismonthcredit.Where(x => x.CreditTypeID == 2).Select(x => x.CreditBalance).SingleOrDefault();
                    ViewBag.thismonthjobcredit = jobcredit;
                    ViewBag.thismonthcvcredit = cvcredit;


                    ViewBag.JobCredit = ((jobcredit / totalmonthjobcredit) * 180) + "deg";
                    ViewBag.CVCredit = ((cvcredit / totalmonthcvcredit) * 180) + "deg";
                    common cmn = new common();

                    ViewBag.ThisMonthname = cmn.GetMonthName(DateTime.Now.Month);

                    ViewBag.TotalCVViewed = db.Database.SqlQuery<int>("select count(CreditUsageID) as cvviewed from CSCreditUsage_T where CompanyID=" + Convert.ToInt32(companyid) + " and credittypeid=2").SingleOrDefault();
                    ViewBag.TotalJobViews = db.Database.SqlQuery<int>("select  ISNULL(SUM(vc_Views), 0) as totalviews from vacancies where vc_co_CompanyId=" + Convert.ToInt32(companyid)).SingleOrDefault();


                }



                //                ViewBag.CreditUsage = @"select c.ct_Forename,c.ct_Surname,cu.UsedOn,ct.CreditType,v.vc_Title,ca.ca_FirstName,ca.ca_Surname from [dbo].[CSCreditUsage_T] cu
                //join Contacts c on cu.ContactID= c.ct_Id
                //join CSCreditType_T ct on ct.CreditTypeID = cu.CreditTypeID
                //join Vacancies v on cu.VacancyID = v.vc_Id
                //join Candidates ca on cu.CandidateID = ca.ca_Id
                //where cu.VoucherID in (select VoucherID from CSVouchers_T where SubscriptionID in (select SubscriptionID from CSSubscription_T where CompanyID=4336))";



                StringBuilder sbcm = new StringBuilder();
                sbcm.AppendFormat(@"select top 4 * from [dbo].[CompanyManagement_T] where co_Id={0}", companyid);
                List<CompanyManagement_T> cmtlst = db.Database.SqlQuery<CompanyManagement_T>(sbcm.ToString()).ToList();


                List<CompanyManagement_T> CompanyManagement = new List<CompanyManagement_T>();

                CompanyManagement_T CompanyMgt;

                foreach (var cm in cmtlst)
                {
                    CompanyMgt = new CompanyManagement_T();
                    CompanyMgt.CompanyManagementID = cm.CompanyManagementID;
                    CompanyMgt.Name = cm.Name;
                    CompanyMgt.Position = cm.Position;
                    CompanyMgt.Name2 = cm.Name2;
                    CompanyMgt.Position2 = cm.Position2;
                    CompanyMgt.Name3 = cm.Name3;
                    CompanyMgt.Position3 = cm.Position3;
                    CompanyMgt.Name4 = cm.Name4;
                    CompanyMgt.Position4 = cm.Position4;

                    if (cm.Extn != null && cm.Extn != "")
                    {
                        CompanyMgt.Extn = "Documents/CoManagement/" + cm.CM_GUID.ToString().Substring(0, 2) + "/" + cm.CM_GUID.ToString().Substring(2, 2) + "/" + cm.CM_GUID.ToString().Substring(4, 2) + "/topmanagement1." + cm.Extn.Trim() + "?" + DateTime.Now; ;
                    }
                    else
                    {
                        CompanyMgt.Extn = "dummypath";
                    }

                    if (cm.Extn2 != null && cm.Extn2 != "")
                    {
                        CompanyMgt.Extn2 = "Documents/CoManagement/" + cm.CM_GUID.ToString().Substring(0, 2) + "/" + cm.CM_GUID.ToString().Substring(2, 2) + "/" + cm.CM_GUID.ToString().Substring(4, 2) + "/topmanagement2." + cm.Extn2.Trim() + "?" + DateTime.Now; ;
                    }
                    else
                    {
                        CompanyMgt.Extn2 = "dummypath";
                    }

                    if (cm.Extn3 != null && cm.Extn3 != "")
                    {
                        CompanyMgt.Extn3 = "Documents/CoManagement/" + cm.CM_GUID.ToString().Substring(0, 2) + "/" + cm.CM_GUID.ToString().Substring(2, 2) + "/" + cm.CM_GUID.ToString().Substring(4, 2) + "/topmanagement3." + cm.Extn3.Trim() + "?" + DateTime.Now; ;
                    }
                    else
                    {
                        CompanyMgt.Extn3 = "dummypath";
                    }

                    if (cm.Extn4 != null && cm.Extn4 != "")
                    {
                        CompanyMgt.Extn4 = "Documents/CoManagement/" + cm.CM_GUID.ToString().Substring(0, 2) + "/" + cm.CM_GUID.ToString().Substring(2, 2) + "/" + cm.CM_GUID.ToString().Substring(4, 2) + "/topmanagement4." + cm.Extn4.Trim() + "?" + DateTime.Now; ;
                    }
                    else
                    {
                        CompanyMgt.Extn4 = "dummypath";
                    }



                    CompanyManagement.Add(CompanyMgt);

                }

                ViewBag.CompanyManagement = CompanyManagement;




                StringBuilder sbpg = new StringBuilder();
                sbpg.AppendFormat(@"select * from [dbo].[CompanyPhotoGallery_T] where co_Id={0}", companyid);
                List<PhotoGallery> pglst = db.Database.SqlQuery<PhotoGallery>(sbpg.ToString()).ToList();


                List<PhotoGallery> photogallery = new List<PhotoGallery>();

                PhotoGallery pgallery;

                foreach (var pg in pglst)
                {
                    pgallery = new PhotoGallery();
                    pgallery.PGID = pg.PGID;
                    pgallery.ImagePath = "Documents/PhotoGallery/" + pg.PG_GUID.ToString().Substring(0, 2) + "/" + pg.PG_GUID.ToString().Substring(2, 2) + "/" + pg.PG_GUID.ToString().Substring(4, 2) + "/" + pg.PG_GUID.ToString() + "." + pg.PhotoExtension;
                    photogallery.Add(pgallery);
                }

                ViewBag.photogallery = photogallery;



                StringBuilder sbct = new StringBuilder();
                sbct.AppendFormat(@"select * from [dbo].[CompanyTestimonials_T] where co_Id={0}", companyid);
                List<Testimonials> lstCT = db.Database.SqlQuery<Testimonials>(sbct.ToString()).ToList();

                List<Testimonials> testimonials = new List<Testimonials>();

                Testimonials testi;

                foreach (var ct in lstCT)
                {
                    testi = new Testimonials();
                    testi.CTID = ct.CTID;
                    testi.CT_GUID = ct.CT_GUID;
                    testi.CTDetails = ct.CTDetails;
                    testi.CTPicExtension = "Documents/Testimonials/" + ct.CT_GUID.ToString().Substring(0, 2) + "/" + ct.CT_GUID.ToString().Substring(2, 2) + "/" + ct.CT_GUID.ToString().Substring(4, 2) + "/" + ct.CT_GUID.ToString() + "." + ct.CTPicExtension;
                    testimonials.Add(testi);
                }

                ViewBag.testimonials = testimonials;



                StringBuilder sbmedia = new StringBuilder();
                sbmedia.AppendFormat(@"select * from [dbo].[CompanyMedia_T] where co_Id={0}", companyid);
                List<Medias> lstmedia = db.Database.SqlQuery<Medias>(sbmedia.ToString()).ToList();

                List<Medias> medias = new List<Medias>();

                Medias mda;

                foreach (var media in lstmedia)
                {
                    mda = new Medias();
                    mda.CMID = media.CMID;
                    mda.CM_GUID = media.CM_GUID;
                    mda.CMTitle = media.CMTitle;
                    mda.CMPicExtension = "Documents/Medias/" + media.CM_GUID.ToString().Substring(0, 2) + "/" + media.CM_GUID.ToString().Substring(2, 2) + "/" + media.CM_GUID.ToString().Substring(4, 2) + "/" + media.CM_GUID.ToString() + "." + media.CMPicExtension;
                    medias.Add(mda);
                }

                ViewBag.medias = medias;




                List<CompanyVideos> videourls = db.Database.SqlQuery<CompanyVideos>("select top 3 CoVideo_ID,VideoURL from CompanyVideo_T where co_id=" + Convert.ToString(companyid)).ToList();

                List<CompanyVideos> videoids = new List<CompanyVideos>();

                CompanyVideos vdoid;
                foreach (var v in videourls)
                {
                    vdoid = new CompanyVideos();
                    string[] fullurl = v.VideoURL.Split('=');
                    vdoid.VideoURL = fullurl[1];
                    vdoid.CoVideo_ID = v.CoVideo_ID;
                    videoids.Add(vdoid);
                }

                ViewBag.videourls = videoids;





                dbOperations dbo = new dbOperations();

                ViewBag.Sectors = dbo.getlist(4).ToList();

                ViewBag.BAexisting = db.Database.SqlQuery<int>("select cs_lm_SectorId as bsid from [dbo].[CompanySectors] where cs_co_CompanyId=" + companyid).ToList();







                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(@" select vc_Id,co_id,vc_Confidential,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
inner join Companies on vc_co_CompanyId=co_Id 
join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
where vc_co_CompanyId=" + companyid + " and vc_deleted is null and vc_st_StatusID=1 order by vc_id desc").ToList();

                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location;
                    ljx.vc_Description = StripHTML(lj.vc_Description).Substring(0, 80);

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

                    ljx.LogoURL = "Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + extns;


                    //int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    ljx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    ljx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    ljx.postedsince = "" + days + " days ago";
                    //}
                    ljx.postedsince = lj.vc_Created.ToString("dd-MM-yyyy");

                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToList();




            }





            return View();
        }



        #region reports here
        public ActionResult Reports()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.SurName = recruiter[0].ct_Surname;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;


                //ViewBag.Reports = db.Database.SqlQuery<Reports>("select ContactID , rt.Type, count(ReportID) as Total from Report_T r   join ReportType_T rt on r.ReportTypeID= rt.ReportTypeID where companyid=" + Convert.ToString(Session["CompanyID"]) + " group by r.ContactID ,  rt.Type  order by ContactID").ToList();
                //ViewBag.comcontacts = db.Database.SqlQuery<CompanyContacts>("select ct_id,ct_Forename + ' ' + ct_Surname as name, usertype = case ct_superuser when 1 then 'Admin' when 0 then 'User' end from Contacts where ct_co_CompanyId =" + Convert.ToString(Session["CompanyID"]) + " and ct_id in (select ContactID from Report_T where companyid=" + Convert.ToString(Session["CompanyID"]) + ")").ToList();



                List<Reports2> _report = db.Database.SqlQuery<Reports2>("select ct_forename + ' ' + ct_surname as name,type,count(reportid) as total from [dbo].[Report_V] where companyid=" + Convert.ToString(Session["CompanyID"]) + " group by  ct_forename,ct_surname,type order by ct_forename ,type desc").ToList();
                List<Reports2> _report2 = db.Database.SqlQuery<Reports2>("select ct_forename + ' ' + ct_surname as name, max(activitydate) as lastlogin from [dbo].[Report_V] where companyid=" + Convert.ToString(Session["CompanyID"]) + " and reporttypeid=1 group by  ct_forename,ct_surname").ToList();
                List<Reports2> _report3 = db.Database.SqlQuery<Reports2>("select ct_forename + ' ' + ct_surname as name,  count(reportid) as credits from [Report_V] where companyid=" + Convert.ToString(Session["CompanyID"]) + " and reporttypeid<>1 group by  ct_forename,ct_surname").ToList();


 
                foreach (var r in _report.OrderBy(x => x.name))
                {
                    foreach (var r2 in _report2)
                    {
                        if (r2.name == r.name)
                        {
                            r.lastlogin = r2.lastlogin;
                        }
                    }
                    foreach (var r3 in _report3)
                    {
                        if (r3.name == r.name)
                        {
                            r.credits = r3.credits;
                        }
                    }
                }
                ViewBag._report = _report;
            }



            return View();
        }
        #endregion




        public ActionResult Recruiter()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();

                int companyid = recruiter[0].ct_co_CompanyId;

                ViewBag.PreviewURL = "Employer/" + companyid + "/Preview";

                ViewBag.Contacts = db.Database.SqlQuery<reccontacts>("select ct_Id,ct_Forename,ct_Surname,ct_EmailAddress,ct_Password,ct_Telephone,ct_Superuser,ct_Deleted,co_PositioninCompanyID,lm_Value,ct_PostJob,ct_SearchCV  from Contacts ct join ListsMaster lm on lm.lm_Id = ct.co_PositioninCompanyID where ct_deleted is null and ct_co_companyid=" + companyid).ToList();
                //db.Contacts.Where(x => x.ct_co_CompanyId == companyid).ToList();


                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();
                string extns = "";

                if (company[0].co_HasLogo == true)
                {
                    extns = company[0].co_LogoExtension.Trim() == "" ? ".jpg" : company[0].co_LogoExtension;
                    //"http://www.jobs4bahrainis.com/Logox/"
                    ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;
                }

                if (company[0].co_HeaderExtension != null)
                {
                    extns = company[0].co_HeaderExtension.Trim();

                    ViewBag.HeaderPic = "Documents/Header/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + extns;
                }


                ViewBag.Company = company[0].co_Name;
                ViewBag.CompanyID = company[0].co_Id;
                ViewBag.CompanyGUID = company[0].co_Guid;

                ViewBag.CompanyPhone = company[0].co_Telephone;
                ViewBag.PostalAddress = company[0].co_PostalAddress;
                ViewBag.LastUpdated = company[0].co_LastUpdated;

                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.SurName = recruiter[0].ct_Surname;

                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;




                ViewBag.Video = company[0].co_Video;
                ViewBag.CompanySizeID = company[0].co_CompanySizeID;
                ViewBag.PositionID = recruiter[0].co_PositioninCompanyID;
                ViewBag.ct_Superuser = recruiter[0].ct_Superuser;
                ViewBag.ct_Id = recruiter[0].ct_Id;


                ViewBag.co_VideoPublished = company[0].co_VideoPublished;
                ViewBag.co_GalleryPublished = company[0].co_GalleryPublished;
                ViewBag.co_TestimonialsPublished = company[0].co_TestimonialsPublished;
                ViewBag.co_MediaPublished = company[0].co_MediaPublished;
                ViewBag.co_ProfilePublished = company[0].co_ProfilePublished;



                ViewBag.CompanyProfile = company[0].co_Profile;
                ViewBag.CompanyProfile2 = company[0].co_Profile2;
                ViewBag.CompanyProfile3 = company[0].co_Profile3;
                ViewBag.CompanyProfile4 = company[0].co_Profile4;
                ViewBag.CompanyProfile5 = company[0].co_Profile5;


                ViewBag.ProfileTitle1 = company[0].co_ProfileTitle1;
                ViewBag.ProfileTitle2 = company[0].co_ProfileTitle2;
                ViewBag.ProfileTitle3 = company[0].co_ProfileTitle3;
                ViewBag.ProfileTitle4 = company[0].co_ProfileTitle4;
                ViewBag.ProfileTitle5 = company[0].co_ProfileTitle5;


                ViewBag.co_ProfilePicExtn1 = company[0].co_ProfilePicExtn1;
                ViewBag.co_ProfilePicExtn2 = company[0].co_ProfilePicExtn2;
                ViewBag.co_ProfilePicExtn3 = company[0].co_ProfilePicExtn3;
                ViewBag.co_ProfilePicExtn4 = company[0].co_ProfilePicExtn4;
                ViewBag.co_ProfilePicExtn5 = company[0].co_ProfilePicExtn5;


                ViewBag.co_ProfilePic1 = "Documents/ProfilePics/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/profileimage1" + trimPicExtensions(company[0].co_ProfilePicExtn1);
                ViewBag.co_ProfilePic2 = "Documents/ProfilePics/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/profileimage2" + trimPicExtensions(company[0].co_ProfilePicExtn2);
                ViewBag.co_ProfilePic3 = "Documents/ProfilePics/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/profileimage3" + trimPicExtensions(company[0].co_ProfilePicExtn3);
                ViewBag.co_ProfilePic4 = "Documents/ProfilePics/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/profileimage4" + trimPicExtensions(company[0].co_ProfilePicExtn4);
                ViewBag.co_ProfilePic5 = "Documents/ProfilePics/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/profileimage5" + trimPicExtensions(company[0].co_ProfilePicExtn5);




                ViewBag.ContactID = Convert.ToDecimal(Session["RecruiterID"]);


                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (31,25,4) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.CompanySize = ddLists.Where(x => x.lm_lt_ListTypeId == 31).OrderBy(x => x.lm_Id).ToList();

                foreach (var cs in ViewBag.CompanySize)
                {
                    if (cs.lm_Id == ViewBag.CompanySizeID)
                    {
                        ViewBag.ThisCompanySize = cs.lm_Value;
                    }
                }


                ViewBag.Position = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();

                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();

                ViewBag.CompanyBA = db.Database.SqlQuery<ListMaster>("Select * from ListsMaster where lm_Id in (select cs_lm_sectorid from CompanySectors where cs_co_companyid=" + Session["CompanyID"] + ")").ToList();





                var subs = db.Database.SqlQuery<CSSubscription>(@"select top 1 s.SubscriptionID,p.PackageName,c.ct_Forename,c.ct_Surname, s.StartDate,s.EndDate,s.DOC as PurchaseDate from [dbo].[CSSubscription_T] s  join Contacts c on c.ct_Id = s.ContactID join CSPackage_T p on p.PackageID = s.PackageID where companyid=" + Convert.ToInt32(companyid) + " order by s.SubscriptionID desc").ToList();

                ViewBag.Subscription = subs;

                if (subs.Count != 0)
                {


                    ViewBag.CreditsAvailable = db.Database.SqlQuery<CCredits>(@"select ct.CreditType, v.VoucherMonth,v.VoucherYear,v.CreditBalance from [dbo].[CSVouchers_T] v join CSCreditType_T ct on v.CreditTypeID = ct.CreditTypeID where SubscriptionID = (select top 1 SubscriptionID from CSSubscription_T where CompanyID = " + Convert.ToInt32(companyid) + "  order by subscriptionid desc) order by VoucherYear, VoucherMonth").ToList();

                    List<CCredits> actualmonthlycredit = db.Database.SqlQuery<CCredits>("select CreditTypeID,CreditsPermonth as CreditBalance from  [dbo].[CSCredits_T] where packageid in ( select top 1 packageid from [dbo].[CSSubscription_T] where companyid=" + Convert.ToInt32(companyid) + " and enddate>getdate() order by startdate desc)").ToList();

                    List<CCredits> thismonthcredit = db.Database.SqlQuery<CCredits>(@"select v.credittypeid,v.CreditBalance from [dbo].[CSVouchers_T] v  where SubscriptionID = (select top 1 SubscriptionID from CSSubscription_T where CompanyID = " + Convert.ToInt32(companyid) + "  order by subscriptionid desc)  and v.vouchermonth = month(getdate()) and v.voucheryear = year(getdate())").ToList();

                    float totalmonthjobcredit = actualmonthlycredit.Where(x => x.CreditTypeID == 1).Select(x => x.CreditBalance).SingleOrDefault();
                    float totalmonthcvcredit = actualmonthlycredit.Where(x => x.CreditTypeID == 2).Select(x => x.CreditBalance).SingleOrDefault();

                    ViewBag.totalmonthjobcredit = totalmonthjobcredit;
                    ViewBag.totalmonthcvcredit = totalmonthcvcredit;

                    float jobcredit = thismonthcredit.Where(x => x.CreditTypeID == 1).Select(x => x.CreditBalance).SingleOrDefault();
                    float cvcredit = thismonthcredit.Where(x => x.CreditTypeID == 2).Select(x => x.CreditBalance).SingleOrDefault();
                    ViewBag.thismonthjobcredit = jobcredit;
                    ViewBag.thismonthcvcredit = cvcredit;


                    ViewBag.JobCredit = ((jobcredit / totalmonthjobcredit) * 180) + "deg";
                    ViewBag.CVCredit = ((cvcredit / totalmonthcvcredit) * 180) + "deg";
                    common cmn = new common();

                    ViewBag.ThisMonthname = cmn.GetMonthName(DateTime.Now.Month);

                    ViewBag.TotalCVViewed = db.Database.SqlQuery<int>("select count(CreditUsageID) as cvviewed from CSCreditUsage_T where CompanyID=" + Convert.ToInt32(companyid) + " and credittypeid=2").SingleOrDefault();
                    ViewBag.TotalJobViews = db.Database.SqlQuery<int>("select  ISNULL(SUM(vc_Views), 0) as totalviews from vacancies where vc_co_CompanyId=" + Convert.ToInt32(companyid)).SingleOrDefault();
                    ViewBag.TotalFollowers = db.Database.SqlQuery<int>("select count(FollowerID) as totalfollowers from CompanyFollowers_T where CompanyID =" + Convert.ToInt32(companyid)).SingleOrDefault();

                }



                //                ViewBag.CreditUsage = @"select c.ct_Forename,c.ct_Surname,cu.UsedOn,ct.CreditType,v.vc_Title,ca.ca_FirstName,ca.ca_Surname from [dbo].[CSCreditUsage_T] cu
                //join Contacts c on cu.ContactID= c.ct_Id
                //join CSCreditType_T ct on ct.CreditTypeID = cu.CreditTypeID
                //join Vacancies v on cu.VacancyID = v.vc_Id
                //join Candidates ca on cu.CandidateID = ca.ca_Id
                //where cu.VoucherID in (select VoucherID from CSVouchers_T where SubscriptionID in (select SubscriptionID from CSSubscription_T where CompanyID=4336))";



                StringBuilder sbcm = new StringBuilder();
                sbcm.AppendFormat(@"select top 4 * from [dbo].[CompanyManagement_T] where co_Id={0}", Convert.ToString(Session["CompanyID"]));
                List<CompanyManagement_T> cmtlst = db.Database.SqlQuery<CompanyManagement_T>(sbcm.ToString()).ToList();


                List<CompanyManagement_T> CompanyManagement = new List<CompanyManagement_T>();

                CompanyManagement_T CompanyMgt;

                foreach (var cm in cmtlst)
                {
                    CompanyMgt = new CompanyManagement_T();
                    CompanyMgt.CompanyManagementID = cm.CompanyManagementID;
                    CompanyMgt.Name = cm.Name;
                    CompanyMgt.Position = cm.Position;
                    CompanyMgt.Name2 = cm.Name2;
                    CompanyMgt.Position2 = cm.Position2;
                    CompanyMgt.Name3 = cm.Name3;
                    CompanyMgt.Position3 = cm.Position3;
                    CompanyMgt.Name4 = cm.Name4;
                    CompanyMgt.Position4 = cm.Position4;

                    if (cm.Extn != null && cm.Extn != "")
                    {
                        CompanyMgt.Extn = "Documents/CoManagement/" + cm.CM_GUID.ToString().Substring(0, 2) + "/" + cm.CM_GUID.ToString().Substring(2, 2) + "/" + cm.CM_GUID.ToString().Substring(4, 2) + "/topmanagement1." + cm.Extn.Trim() + "?" + DateTime.Now; ;
                    }
                    else
                    {
                        CompanyMgt.Extn = "dummypath";
                    }

                    if (cm.Extn2 != null && cm.Extn2 != "")
                    {
                        CompanyMgt.Extn2 = "Documents/CoManagement/" + cm.CM_GUID.ToString().Substring(0, 2) + "/" + cm.CM_GUID.ToString().Substring(2, 2) + "/" + cm.CM_GUID.ToString().Substring(4, 2) + "/topmanagement2." + cm.Extn2.Trim() + "?" + DateTime.Now; ;
                    }
                    else
                    {
                        CompanyMgt.Extn2 = "dummypath";
                    }

                    if (cm.Extn3 != null && cm.Extn3 != "")
                    {
                        CompanyMgt.Extn3 = "Documents/CoManagement/" + cm.CM_GUID.ToString().Substring(0, 2) + "/" + cm.CM_GUID.ToString().Substring(2, 2) + "/" + cm.CM_GUID.ToString().Substring(4, 2) + "/topmanagement3." + cm.Extn3.Trim() + "?" + DateTime.Now; ;
                    }
                    else
                    {
                        CompanyMgt.Extn3 = "dummypath";
                    }

                    if (cm.Extn4 != null && cm.Extn4 != "")
                    {
                        CompanyMgt.Extn4 = "Documents/CoManagement/" + cm.CM_GUID.ToString().Substring(0, 2) + "/" + cm.CM_GUID.ToString().Substring(2, 2) + "/" + cm.CM_GUID.ToString().Substring(4, 2) + "/topmanagement4." + cm.Extn4.Trim() + "?" + DateTime.Now; ;
                    }
                    else
                    {
                        CompanyMgt.Extn4 = "dummypath";
                    }



                    CompanyManagement.Add(CompanyMgt);

                }

                ViewBag.CompanyManagement = CompanyManagement;




                StringBuilder sbpg = new StringBuilder();
                sbpg.AppendFormat(@"select * from [dbo].[CompanyPhotoGallery_T] where co_Id={0}", Convert.ToString(Session["CompanyID"]));
                List<PhotoGallery> pglst = db.Database.SqlQuery<PhotoGallery>(sbpg.ToString()).ToList();


                List<PhotoGallery> photogallery = new List<PhotoGallery>();

                PhotoGallery pgallery;

                foreach (var pg in pglst)
                {
                    pgallery = new PhotoGallery();
                    pgallery.PGID = pg.PGID;
                    pgallery.ImagePath = "Documents/PhotoGallery/" + pg.PG_GUID.ToString().Substring(0, 2) + "/" + pg.PG_GUID.ToString().Substring(2, 2) + "/" + pg.PG_GUID.ToString().Substring(4, 2) + "/" + pg.PG_GUID.ToString() + "." + pg.PhotoExtension;
                    photogallery.Add(pgallery);
                }

                ViewBag.photogallery = photogallery;



                StringBuilder sbct = new StringBuilder();
                sbct.AppendFormat(@"select * from [dbo].[CompanyTestimonials_T] where co_Id={0}", Convert.ToString(Session["CompanyID"]));
                List<Testimonials> lstCT = db.Database.SqlQuery<Testimonials>(sbct.ToString()).ToList();

                List<Testimonials> testimonials = new List<Testimonials>();

                Testimonials testi;

                foreach (var ct in lstCT)
                {
                    testi = new Testimonials();
                    testi.CTID = ct.CTID;
                    testi.CT_GUID = ct.CT_GUID;
                    testi.CTDetails = ct.CTDetails;
                    testi.CTPicExtension = "Documents/Testimonials/" + ct.CT_GUID.ToString().Substring(0, 2) + "/" + ct.CT_GUID.ToString().Substring(2, 2) + "/" + ct.CT_GUID.ToString().Substring(4, 2) + "/" + ct.CT_GUID.ToString() + "." + ct.CTPicExtension;
                    testimonials.Add(testi);
                }

                ViewBag.testimonials = testimonials;



                StringBuilder sbmedia = new StringBuilder();
                sbmedia.AppendFormat(@"select * from [dbo].[CompanyMedia_T] where co_Id={0}", Convert.ToString(Session["CompanyID"]));
                List<Medias> lstmedia = db.Database.SqlQuery<Medias>(sbmedia.ToString()).ToList();

                List<Medias> medias = new List<Medias>();

                Medias mda;

                foreach (var media in lstmedia)
                {
                    mda = new Medias();
                    mda.CMID = media.CMID;
                    mda.CM_GUID = media.CM_GUID;
                    mda.CMTitle = media.CMTitle;
                    mda.CMPicExtension = "Documents/Medias/" + media.CM_GUID.ToString().Substring(0, 2) + "/" + media.CM_GUID.ToString().Substring(2, 2) + "/" + media.CM_GUID.ToString().Substring(4, 2) + "/" + media.CM_GUID.ToString() + "." + media.CMPicExtension;
                    medias.Add(mda);
                }

                ViewBag.medias = medias;






                List<CompanyVideos> videourls = db.Database.SqlQuery<CompanyVideos>("select top 3 CoVideo_ID,VideoURL from CompanyVideo_T where co_id=" + Convert.ToString(Session["CompanyID"])).ToList();

                List<CompanyVideos> videoids = new List<CompanyVideos>();

                CompanyVideos vdoid;
                foreach (var v in videourls)
                {
                    vdoid = new CompanyVideos();
                    if (v.VideoURL.Contains("="))
                    {
                        string[] fullurl = v.VideoURL.Split('=');
                        vdoid.VideoURL = fullurl[1];
                        vdoid.CoVideo_ID = v.CoVideo_ID;
                        videoids.Add(vdoid);
                    }
                    else if (v.VideoURL.Contains(".be/"))
                    {

                        string vx = v.VideoURL.Replace("https://","");
                        string[] fullurl = v.VideoURL.Split('/');
                        vdoid.VideoURL = fullurl[1];
                        vdoid.CoVideo_ID = v.CoVideo_ID;
                        videoids.Add(vdoid);
                    }
                }

                ViewBag.videourls = videoids;





                dbOperations dbo = new dbOperations();

                ViewBag.Sectors = dbo.getlist(4).ToList();

                List<ListMaster> firstsector = ViewBag.Sectors;
                ViewBag.FirstSectorID = firstsector.Min(x => x.lm_Id);
                ViewBag.SectorCount = firstsector.Count();

                ViewBag.BAexisting = db.Database.SqlQuery<int>("select cs_lm_SectorId as bsid from [dbo].[CompanySectors] where cs_co_CompanyId=" + companyid).ToList();




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
                select count(app_id) as total from Applications where app_vc_VacancyId in (select vc_id from Vacancies where vc_co_CompanyId={0})", companyid);

                ViewBag.Statistics = db.Database.SqlQuery<int>(stb.ToString()).ToList();


                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(@" select top 2 vc_Id,co_id,vc_Confidential,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension,vc_views,vc_applications from Vacancies v
inner join Companies on vc_co_CompanyId=co_Id 
join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
where vc_co_CompanyId=" + companyid + " and vc_Deleted is null and vc_st_StatusID=1 order by vc_id desc").ToList();

                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location;
                    ljx.vc_Description = StripHTML(lj.vc_Description).Substring(0, 80);

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

                    ljx.LogoURL = "Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + extns;


                    //int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    ljx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    ljx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    ljx.postedsince = "" + days + " days ago";
                    //}

                    ljx.postedsince = lj.vc_Created.ToString("dd-MM-yyyy");


                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToList();




            }

            return View();
        }

        public ActionResult SearchDashboard()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }


            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);

           

      

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                if (recruiter[0].ct_SearchCV == false)
                {

                    return RedirectToAction("NoPermission");
                }
                int companyid = recruiter[0].ct_co_CompanyId;

                ViewBag.Contacts = db.Database.SqlQuery<reccontacts>("select ct_Forename,ct_Surname,ct_EmailAddress,ct_Telephone,ct_Superuser,co_PositioninCompanyID,lm_Value  from Contacts ct join ListsMaster lm on lm.lm_Id = ct.co_PositioninCompanyID where ct_co_companyid=" + companyid).ToList();
                //db.Contacts.Where(x => x.ct_co_CompanyId == companyid).ToList();

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                dbOperations dbo = new dbOperations();

                if (!dbo.CS_CreditCheck(Convert.ToInt32(company[0].co_Id), 2))
                {
                    return RedirectToAction("NoValidSubscription");
                }

                string extns = "";

                if (company[0].co_HasLogo == true)
                {
                    extns = company[0].co_LogoExtension.Trim() == "" ? ".jpg" : company[0].co_LogoExtension;
                    //"http://www.jobs4bahrainis.com/Logox/"
                    ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;
                }



                ViewBag.Company = company[0].co_Name;
                ViewBag.CompanyID = company[0].co_Id;

                ViewBag.CompanyPhone = company[0].co_Telephone;
                ViewBag.PostalAddress = company[0].co_PostalAddress;
                ViewBag.LastUpdated = company[0].co_LastUpdated;

                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.SurName = recruiter[0].ct_Surname;

                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;
                ViewBag.CompanyProfile = company[0].co_Profile;
                ViewBag.Video = company[0].co_Video;
                ViewBag.CompanySizeID = company[0].co_CompanySizeID;
                ViewBag.PositionID = recruiter[0].co_PositioninCompanyID;

                ViewBag.ContactID = Convert.ToDecimal(Session["RecruiterID"]);


                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (31,25,4,28) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.CompanySize = ddLists.Where(x => x.lm_lt_ListTypeId == 31).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Position = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();

                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.CompanyBA = db.Database.SqlQuery<ListMaster>("Select * from ListsMaster where lm_Id in (select cs_lm_sectorid from CompanySectors where cs_co_companyid=" + Session["CompanyID"] + ")").ToList();





                var subs = db.Database.SqlQuery<CSSubscription>(@"select top 1 s.SubscriptionID,p.PackageName,c.ct_Forename,c.ct_Surname, s.StartDate,s.EndDate,s.DOC as PurchaseDate from [dbo].[CSSubscription_T] s  join Contacts c on c.ct_Id = s.ContactID join CSPackage_T p on p.PackageID = s.PackageID where companyid=" + Convert.ToInt32(companyid) + " order by s.SubscriptionID desc").ToList();

                ViewBag.Subscription = subs;

                if (subs.Count != 0)
                {


                    ViewBag.CreditsAvailable = db.Database.SqlQuery<CCredits>(@"select ct.CreditType, v.VoucherMonth,v.VoucherYear,v.CreditBalance from [dbo].[CSVouchers_T] v join CSCreditType_T ct on v.CreditTypeID = ct.CreditTypeID where SubscriptionID = (select top 1 SubscriptionID from CSSubscription_T where CompanyID = " + Convert.ToInt32(companyid) + "  order by subscriptionid desc) order by VoucherYear, VoucherMonth").ToList();

                    List<CCredits> actualmonthlycredit = db.Database.SqlQuery<CCredits>("select CreditTypeID,CreditsPermonth as CreditBalance from  [dbo].[CSCredits_T] where packageid in ( select top 1 packageid from [dbo].[CSSubscription_T] where companyid=" + Convert.ToInt32(companyid) + " and enddate>getdate() order by startdate desc)").ToList();

                    List<CCredits> thismonthcredit = db.Database.SqlQuery<CCredits>(@"select v.credittypeid,v.CreditBalance from [dbo].[CSVouchers_T] v  where SubscriptionID = (select top 1 SubscriptionID from CSSubscription_T where CompanyID = " + Convert.ToInt32(companyid) + "  order by subscriptionid desc)  and v.vouchermonth = month(getdate()) and v.voucheryear = year(getdate())").ToList();

                    float totalmonthjobcredit = actualmonthlycredit.Where(x => x.CreditTypeID == 1).Select(x => x.CreditBalance).SingleOrDefault();
                    float totalmonthcvcredit = actualmonthlycredit.Where(x => x.CreditTypeID == 2).Select(x => x.CreditBalance).SingleOrDefault();

                    ViewBag.totalmonthjobcredit = totalmonthjobcredit;
                    ViewBag.totalmonthcvcredit = totalmonthcvcredit;

                    float jobcredit = thismonthcredit.Where(x => x.CreditTypeID == 1).Select(x => x.CreditBalance).SingleOrDefault();
                    float cvcredit = thismonthcredit.Where(x => x.CreditTypeID == 2).Select(x => x.CreditBalance).SingleOrDefault();
                    ViewBag.thismonthjobcredit = jobcredit;
                    ViewBag.thismonthcvcredit = cvcredit;


                    ViewBag.JobCredit = ((jobcredit / totalmonthjobcredit) * 180) + "deg";
                    ViewBag.CVCredit = ((cvcredit / totalmonthcvcredit) * 180) + "deg";
                    common cmn = new common();

                    ViewBag.ThisMonthname = cmn.GetMonthName(DateTime.Now.Month);

                    ViewBag.TotalCVViewed = db.Database.SqlQuery<int>("select count(CreditUsageID) as cvviewed from CSCreditUsage_T where CompanyID=" + Convert.ToInt32(companyid) + " and credittypeid=2").SingleOrDefault();
                    ViewBag.TotalJobViews = db.Database.SqlQuery<int>("select  ISNULL(SUM(vc_Views), 0) as totalviews from vacancies where vc_co_CompanyId=" + Convert.ToInt32(companyid)).SingleOrDefault();


                }



                //                ViewBag.CreditUsage = @"select c.ct_Forename,c.ct_Surname,cu.UsedOn,ct.CreditType,v.vc_Title,ca.ca_FirstName,ca.ca_Surname from [dbo].[CSCreditUsage_T] cu
                //join Contacts c on cu.ContactID= c.ct_Id
                //join CSCreditType_T ct on ct.CreditTypeID = cu.CreditTypeID
                //join Vacancies v on cu.VacancyID = v.vc_Id
                //join Candidates ca on cu.CandidateID = ca.ca_Id
                //where cu.VoucherID in (select VoucherID from CSVouchers_T where SubscriptionID in (select SubscriptionID from CSSubscription_T where CompanyID=4336))";



                StringBuilder sbcm = new StringBuilder();
                sbcm.AppendFormat(@"select * from [dbo].[CompanyManagement_T] where co_Id={0}", Convert.ToString(Session["CompanyID"]));
                List<CompanyManagement_T> cmtlst = db.Database.SqlQuery<CompanyManagement_T>(sbcm.ToString()).ToList();


                List<CompanyManagement_T> CompanyManagement = new List<CompanyManagement_T>();

                CompanyManagement_T CompanyMgt;

                foreach (var cm in cmtlst)
                {
                    CompanyMgt = new CompanyManagement_T();
                    CompanyMgt.CompanyManagementID = cm.CompanyManagementID;
                    CompanyMgt.Name = cm.Name;
                    CompanyMgt.Position = cm.Position;
                    CompanyMgt.Extn = "Documents/CoManagement/" + cm.CM_GUID.ToString().Substring(0, 2) + "/" + cm.CM_GUID.ToString().Substring(2, 2) + "/" + cm.CM_GUID.ToString().Substring(4, 2) + "/" + cm.CM_GUID.ToString() + "." + cm.Extn;
                    CompanyManagement.Add(CompanyMgt);

                }

                ViewBag.CompanyManagement = CompanyManagement;


               

                ViewBag.Sectors = dbo.getlist(4).ToList();

                ViewBag.BAexisting = db.Database.SqlQuery<int>("select cs_lm_SectorId as bsid from [dbo].[CompanySectors] where cs_co_CompanyId=" + companyid).ToList();




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
                select count(app_id) as total from Applications where app_vc_VacancyId in (select vc_id from Vacancies where vc_co_CompanyId={0})", companyid);

                ViewBag.Statistics = db.Database.SqlQuery<int>(stb.ToString()).ToList();

            }

            return View();
        }


        public ActionResult MyCompanies() {

            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["Ca_ID"]);

            ViewBag.Sectors = listmastervalues(4);

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                int tabno = db.Database.SqlQuery<int>("select ca_CompletedRegTabs from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();
                if (tabno < 5)
                {
                    return RedirectToAction("../" + CurTabFinder(tabno));
                }

                var candidate = db.Candidates.Where(x => x.ca_Id == candidateid).ToList();

                ViewBag.Profilepic = "Documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_PhotoExtension;

                ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
                ViewBag.Phone = candidate[0].ca_PhoneMobile;
                ViewBag.Email = candidate[0].ca_EmailAddress;


                var Companies = db.Database.SqlQuery<AllCompanies>("select cf.CompanyID as co_id,co.co_Guid,co.co_LogoExtension, co.co_Name,co.co_PostalAddress from CompanyFollowers_T cf join Companies co on cf.companyid=co.co_id where candidateid=" + candidateid).ToList();


                List<AllCompanies> allco = new List<AllCompanies>();

                AllCompanies _allc;

                string extns = "";

               


                foreach (var co in Companies) {
                    _allc = new AllCompanies();
                    _allc.co_id = co.co_id;
                    _allc.co_Name = co.co_Name;
                    _allc.co_postaladdress = co.co_postaladdress;
                    extns = co.co_LogoExtension.Trim() == "" ? "jpg" : co.co_LogoExtension;
                    _allc.LogoURL = "~/Documents/Logos/" + co.co_Guid.ToString().Substring(0, 2) + "/" + co.co_Guid.ToString().Substring(2, 2) + "/" + co.co_Guid.ToString().Substring(4, 2) + "/" + co.co_Guid.ToString() + "-original" + extns;
                    allco.Add(_allc);
                }

                ViewBag.Companies = allco.ToList();

            }

                return View();

        }




        public ActionResult JobSeeker()
        {

            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["Ca_ID"]);

            ViewBag.Sectors = listmastervalues(4);

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                int tabno = db.Database.SqlQuery<int>("select ca_CompletedRegTabs from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();
                if (tabno < 5)
                {
                    return RedirectToAction("../" + CurTabFinder(tabno));
                }

                var candidate = db.Candidates.Where(x => x.ca_Id == candidateid).ToList();

                ViewBag.Profilepic = "Documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_PhotoExtension;

                ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
                ViewBag.Phone = candidate[0].ca_PhoneMobile;
                ViewBag.Email = candidate[0].ca_EmailAddress;



                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(@"select vc_co_CompanyId, vc_Id,co_id,vc_Confidential,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies on vc_co_CompanyId=co_Id 
            join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
            where vc_id in (select distinct app_vc_VacancyId from applications where app_ca_CandidateId=" + candidateid + ") and vc_Deleted is null and vc_st_StatusID=1 order by vc_Title").ToList();

                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;
                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");
                    ljx.vc_Confidential = lj.vc_Confidential;



                    ljx.vc_Description = TruncateAtWord(StripHTML(lj.vc_Description), 100);

                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;

                    ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;



                    //int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    ljx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    ljx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    ljx.postedsince = "" + days + " days ago";
                    //}

                    ljx.postedsince = lj.vc_Created.ToString("dd-MM-yyyy");

                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2;






                List<LatestJobs> RecomendedJobs = db.Database.SqlQuery<LatestJobs>(@"select top 10 vc_co_CompanyId,vc_Confidential, vc_Id,co_id,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies on vc_co_CompanyId=co_Id 
            join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
            where vc_FunctionID in (select ca_FunctionTitleID from Candidates where ca_Id=" + candidateid + ") and vc_Deleted is null and vc_st_StatusID=1 order by vc_Title").ToList();

                List<LatestJobs> RecJobs = new List<LatestJobs>();
                LatestJobs Rjx;


                foreach (var rj in RecomendedJobs)
                {
                    Rjx = new LatestJobs();
                    Rjx.vc_co_CompanyId = rj.vc_co_CompanyId;
                    Rjx.vc_id = rj.vc_id;
                    Rjx.vc_Title = rj.vc_Title;
                    Rjx.vc_Location = rj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");

                    Rjx.vc_Confidential = rj.vc_Confidential;



                    Rjx.vc_Description = TruncateAtWord(StripHTML(rj.vc_Description), 100);

                    Rjx.vc_Created = rj.vc_Created;
                    try
                    {
                        Rjx.co_Name = rj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        Rjx.co_Name = rj.co_Name;
                    }
                    Rjx.co_NameURL = urlcleaner(rj.co_Name);
                    string extns = rj.co_LogoExtension.Trim() == "" ? "jpg" : rj.co_LogoExtension;

                    Rjx.LogoURL = "~/Documents/Logos/" + rj.co_Guid.ToString().Substring(0, 2) + "/" + rj.co_Guid.ToString().Substring(2, 2) + "/" + rj.co_Guid.ToString().Substring(4, 2) + "/" + rj.co_Guid.ToString() + "-original" + extns;


                    //int days = Convert.ToInt32((DateTime.Now - rj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    Rjx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    Rjx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    Rjx.postedsince = "" + days + " days ago";
                    //}

                    Rjx.postedsince = rj.vc_Created.ToString("dd-MM-yyyy");

                    Rjx.jobURL = urlcleaner(rj.vc_Title);
                    RecJobs.Add(Rjx);

                }

                ViewBag.RecJobs = RecJobs;




                List<LatestJobs> SvdJobs = db.Database.SqlQuery<LatestJobs>(@"select top 10 vc_co_CompanyId,vc_Confidential, vc_Id,co_id,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies on vc_co_CompanyId=co_Id 
            join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
            where vc_id in (select vc_id from SavedJob_T where ca_Id=" + candidateid + ") and vc_Deleted is null and vc_st_StatusID=1 order by vc_Title").ToList();

                List<LatestJobs> SavedJobs = new List<LatestJobs>();
                LatestJobs sjx;


                foreach (var sj in SvdJobs)
                {
                    sjx = new LatestJobs();
                    sjx.vc_co_CompanyId = sj.vc_co_CompanyId;
                    sjx.vc_id = sj.vc_id;
                    sjx.vc_Title = sj.vc_Title;
                    sjx.vc_Location = sj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");

                    sjx.vc_Confidential = sj.vc_Confidential;



                    sjx.vc_Description = TruncateAtWord(StripHTML(sj.vc_Description), 100);

                    sjx.vc_Created = sj.vc_Created;
                    try
                    {
                        sjx.co_Name = sj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        sjx.co_Name = sj.co_Name;
                    }
                    sjx.co_NameURL = urlcleaner(sj.co_Name);
                    string extns = sj.co_LogoExtension.Trim() == "" ? "jpg" : sj.co_LogoExtension;

                    sjx.LogoURL = "~/Documents/Logos/" + sj.co_Guid.ToString().Substring(0, 2) + "/" + sj.co_Guid.ToString().Substring(2, 2) + "/" + sj.co_Guid.ToString().Substring(4, 2) + "/" + sj.co_Guid.ToString() + "-original" + extns;


                    //int days = Convert.ToInt32((DateTime.Now - sj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    sjx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    sjx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    sjx.postedsince = "" + days + " days ago";
                    //}
                    sjx.postedsince = sj.vc_Created.ToString("dd-MM-yyyy");

                    sjx.jobURL = urlcleaner(sj.vc_Title);
                    SavedJobs.Add(sjx);

                }

                ViewBag.SavedJobs = SavedJobs;




            }


            return View();
        }

        public ActionResult QuickSearch()
        {

            //if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            //int candidateid = Convert.ToInt32(Session["Ca_ID"]);

            ViewBag.Sectors = listmastervalues(4);

            //using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            //{
            //    int tabno = db.Database.SqlQuery<int>("select ca_CompletedRegTabs from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();
            //    if (tabno < 5)
            //    {
            //        return RedirectToAction("../" + CurTabFinder(tabno));
            //    }

            //    var candidate = db.Candidates.Where(x => x.ca_Id == candidateid).ToList();

            //    ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_PhotoExtension;

            //    ViewBag.Name = candidate[0].ca_FirstName + " " + candidate[0].ca_Surname;
            //    ViewBag.Phone = candidate[0].ca_PhoneMobile;
            //    ViewBag.Email = candidate[0].ca_EmailAddress;



            //    List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(@"select vc_co_CompanyId, vc_Id,co_id,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            //inner join Companies on vc_co_CompanyId=co_Id 
            //join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
            //where vc_id in (select distinct app_vc_VacancyId from applications where app_ca_CandidateId=" + candidateid + ") and vc_Deleted is null order by vc_Title").ToList();

            //    List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
            //    LatestJobs ljx;


            //    foreach (var lj in LatestJobs1)
            //    {
            //        ljx = new LatestJobs();
            //        ljx.vc_co_CompanyId = lj.vc_co_CompanyId;
            //        ljx.vc_id = lj.vc_id;
            //        ljx.vc_Title = lj.vc_Title;
            //        ljx.vc_Location = lj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");




            //        ljx.vc_Description = TruncateAtWord(StripHTML(lj.vc_Description), 100);

            //        ljx.vc_Created = lj.vc_Created;
            //        try
            //        {
            //            ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
            //        }
            //        catch
            //        {
            //            ljx.co_Name = lj.co_Name;
            //        }
            //        ljx.co_NameURL = urlcleaner(lj.co_Name);
            //        string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;

            //        ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


            //        int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

            //        if (days == 0)
            //        {
            //            ljx.postedsince = "Today";
            //        }
            //        else if (days == 1)
            //        {
            //            ljx.postedsince = "" + days + " day ago";
            //        }
            //        else if (days > 1)
            //        {
            //            ljx.postedsince = "" + days + " days ago";
            //        }

            //        ljx.jobURL = urlcleaner(lj.vc_Title);
            //        LatestJobs2.Add(ljx);

            //    }

            //    ViewBag.LatestJobs = LatestJobs2;






            //    List<LatestJobs> RecomendedJobs = db.Database.SqlQuery<LatestJobs>(@"select top 10 vc_co_CompanyId, vc_Id,co_id,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            //inner join Companies on vc_co_CompanyId=co_Id 
            //join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
            //where vc_FunctionID in (select ca_FunctionTitleID from Candidates where ca_Id=" + candidateid + ") and vc_Deleted is null order by vc_Title").ToList();

            //    List<LatestJobs> RecJobs = new List<LatestJobs>();
            //    LatestJobs Rjx;


            //    foreach (var rj in RecomendedJobs)
            //    {
            //        Rjx = new LatestJobs();
            //        Rjx.vc_co_CompanyId = rj.vc_co_CompanyId;
            //        Rjx.vc_id = rj.vc_id;
            //        Rjx.vc_Title = rj.vc_Title;
            //        Rjx.vc_Location = rj.vc_Location.Replace(", Bahrain", "").Replace("Kingdom of Bahrain", "Manama").Replace("Bahrain", "Manama");




            //        Rjx.vc_Description = TruncateAtWord(StripHTML(rj.vc_Description), 100);

            //        Rjx.vc_Created = rj.vc_Created;
            //        try
            //        {
            //            Rjx.co_Name = rj.co_Name.Substring(0, 18) + "...";
            //        }
            //        catch
            //        {
            //            Rjx.co_Name = rj.co_Name;
            //        }
            //        Rjx.co_NameURL = urlcleaner(rj.co_Name);
            //        string extns = rj.co_LogoExtension.Trim() == "" ? "jpg" : rj.co_LogoExtension;

            //        Rjx.LogoURL = "~/Documents/Logos/" + rj.co_Guid.ToString().Substring(0, 2) + "/" + rj.co_Guid.ToString().Substring(2, 2) + "/" + rj.co_Guid.ToString().Substring(4, 2) + "/" + rj.co_Guid.ToString() + "-original" + extns;


            //        int days = Convert.ToInt32((DateTime.Now - rj.vc_Created).TotalDays);

            //        if (days == 0)
            //        {
            //            Rjx.postedsince = "Today";
            //        }
            //        else if (days == 1)
            //        {
            //            Rjx.postedsince = "" + days + " day ago";
            //        }
            //        else if (days > 1)
            //        {
            //            Rjx.postedsince = "" + days + " days ago";
            //        }

            //        Rjx.jobURL = urlcleaner(rj.vc_Title);
            //        RecJobs.Add(Rjx);

            //    }

            //    ViewBag.RecJobs = RecJobs;






            //}


            return View();
        }

        public ActionResult CCareerServicesDetails(int articleid)
        {
            if (Session["Ca_ID"] == null && Session["RecruiterID"] == null) { return RedirectToAction("../Login/-/CCareerServicesDetails-" + articleid); }

            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("select * from CareerServices_T where id={0}", articleid);
                List<CareerServices> cs = db.Database.SqlQuery<CareerServices>(sb.ToString()).ToList();
                ViewBag.CareerService = cs;
            }

            return View();
        }


        public ActionResult CandidateCareerServices()
        {
            if (Session["Ca_ID"] == null) { return RedirectToAction("Login"); }

            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }

            return View();
        }


        public ActionResult CVViewer(string ca_GUID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<CandidateDocument> candidatedocument = db.Database.SqlQuery<CandidateDocument>("select top 1 * from CandidateDocuments where cd_Type='CV' and cd_ca_CandidateID=(select top 1 ca_id from candidates where ca_GUID='" + ca_GUID + "') order by cd_id desc").ToList();

                if (candidatedocument.Count > 0)
                {
                    ViewBag.CVpath = "Documents/cvs/" + candidatedocument[0].cd_Guid.ToString().Substring(0, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(2, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(4, 2) + "/" + candidatedocument[0].cd_Guid.ToString() + "." + candidatedocument[0].cd_FileExtension;
                }
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
                int tabno = db.Database.SqlQuery<int>("select ca_CompletedRegTabs from candidates where ca_id=" + Convert.ToInt32(Session["Ca_ID"])).SingleOrDefault();
                if (tabno < 1)
                {
                    return RedirectToAction("../" + CurTabFinder(tabno));
                }

                ViewBag.tabno = tabno;

                // if all fields are not filled it will redirect back to login

                var candidate = db.Database.SqlQuery<CandidateNew>(@"select ca_GUID,ca_PhotoExtension, ca_HasPhoto,c.ca_lm_countryid,ca_lm_EducationLevel,ca_UniversityID,Ca_SectorIndustryID,ca_FunctionTitleID,ca_CurrentJobTitleID,ca_TotalRelavantExperience,c.Ca_CurrentCountryID, c.ca_FirstName,c.ca_Surname,lmcona.lm_Value as Nationality, ca_PhoneMobile,ca_EmailAddress,ca_Password, c.ca_DateOfBirth, c.ca_Gender, c.ca_MaritalStatus,
lmcurcon.lm_Value as CurrentLocation,lmedu.lm_Value as HighestEducation,lmuni.lm_Value as University, 
lmfun.lm_Value as FunctionTitle, lmjt.lm_Value as JobTitle, lmexp.lm_Value as TotalExperience, c.ca_Profile
from Candidates c
join ListsMaster lmcurcon on lmcurcon.lm_Id = c.ca_CurrentCountryID
join ListsMaster lmedu on lmedu.lm_Id = c.ca_lm_EducationLevel
join ListsMaster lmuni on lmuni.lm_Id = c.ca_UniversityID
join ListsMaster lmfun on lmfun.lm_Id = c.ca_FunctionTitleID
left join ListsMaster lmjt on lmjt.lm_Id = c.ca_CurrentJobTitleID
join ListsMaster lmexp on lmexp.lm_Id = c.ca_TotalRelavantExperience
join ListsMaster lmcona on lmcona.lm_Id = c.ca_lm_CountryId
where c.ca_id=" + candidateid + " and ca_active=1").ToList();


                if (candidate.Count == 0)
                {
                    return RedirectToAction("../Login");
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
                ViewBag.MaritalStatus = candidate[0].ca_MaritalStatus;
                ViewBag.Ca_GUID = candidate[0].ca_GUID.ToString();

                ViewBag.DOB = Convert.ToDateTime(candidate[0].ca_DateOfBirth).ToString("dd/MM/yyyy");

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
                ViewBag.Ca_SectorIndustryID = candidate[0].Ca_SectorIndustryID;
                ViewBag.ca_CurrentJobTitleID = candidate[0].ca_CurrentJobTitleID;
                ViewBag.ca_TotalRelavantExperience = candidate[0].ca_TotalRelavantExperience;




                ViewBag.University2 = candidate[0].University;
                ViewBag.FunctionTitle = candidate[0].FunctionTitle;
                ViewBag.CurrentJobTitle = candidate[0].JobTitle;
                ViewBag.TotalExperience = candidate[0].TotalExperience;
                ViewBag.ca_Profile = candidate[0].ca_Profile;




                ViewBag.CandiateProfile = candidate.ToList();


                List<CandidateDocument> candidatedocument = db.Database.SqlQuery<CandidateDocument>("select top 1 * from CandidateDocuments where cd_Type='CV' and cd_ca_CandidateID=" + candidateid + " order by cd_id desc").ToList();

                if (candidatedocument.Count > 0)
                {
                    ViewBag.CVpath = "Documents/cvs/" + candidatedocument[0].cd_Guid.ToString().Substring(0, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(2, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(4, 2) + "/" + candidatedocument[0].cd_Guid.ToString() + "." + candidatedocument[0].cd_FileExtension;
                }

                ViewBag.Education = db.Database.SqlQuery<CandidateEducationNew>(@"select ce.CaEdu_ID,ce.Ca_ID,UniversityID,UniCountryID,DegreeID,SpecializationID,GradeID,lmu.lm_Value as University,lmcon.lm_Value as UniCountry, lmDeg.lm_Value as Degree, lmSpez.lm_Value as Specialization, lmgr.lm_Value as Grade, ce.FromMonth,ce.FromYear, ce.ToMonth,ce.ToYear, ce.CurrentlyStudyHere,ce.Activities  from [CandidateEducation_T] ce 
                join ListsMaster lmu on lmu.lm_Id= ce.UniversityID
                join ListsMaster lmCon on lmCon.lm_Id= ce.UniCountryID
                join ListsMaster lmDeg on lmDeg.lm_Id= ce.DegreeID
                join ListsMaster lmSpez on lmSpez.lm_Id= ce.SpecializationID
                join ListsMaster lmGr on lmGr.lm_Id= ce.GradeID
                where ca_id=" + candidateid + " order by fromyear desc, frommonth desc").ToList();

                var candidatemediadocs = db.Database.SqlQuery<CandidateDocuments>("select cd_CaEdu_ID,cd_OriginalName,cd_FileExtension from candidatedocuments where cd_ca_CandidateId=" + Convert.ToInt32(Session["Ca_ID"]) + " and cd_Type= 'MEDIA' and cd_CaEdu_ID is not null").ToList();
                ViewBag.candidatemediadocs = candidatemediadocs.ToList();







                ViewBag.Experience2 = db.Database.SqlQuery<CandidateWorkHistory>(@"select cwh.CaWorkHistory_ID,cwh.Ca_ID,JobTitleID,cwh.JobLevelID,cwh.ReportingToID,cwh.IndustryID,cwh.FunctionID,cwh.JobLocationID,cwh.SalaryID,lmjt.lm_Value as JobTitle, lmjl.lm_Value as JobLevel, cwh.Company, lmrt.lm_Value as ReportingTo, lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction, 
                cwh.FromMonth,cwh.FromYear,cwh.ToMonth,cwh.ToYear,cwh.CurrentlyWorkingHere,lmjlo.lm_Value as JobLocation, lmsal.lm_Value as Salary,cwh.[Description] from [CandidateWorkHistory_T] cwh 
                join ListsMaster lmjt on lmjt.lm_Id = cwh.JobTitleID
                join ListsMaster lmjl on lmjl.lm_Id = cwh.JobLevelID
                join ListsMaster lmrt on lmrt.lm_Id = cwh.ReportingToID
                join ListsMaster lmind on lmind.lm_Id = cwh.IndustryID
                join ListsMaster lmfn on lmfn.lm_Id = cwh.FunctionID
                join ListsMaster lmjlo on lmjlo.lm_Id = cwh.JobLocationID
                join ListsMaster lmsal on lmsal.lm_Id = cwh.SalaryID
                where ca_id=" + candidateid + " order by fromyear desc, frommonth desc").ToList();

                ViewBag.CandidateCertifications = db.Database.SqlQuery<CandidateCertifications>(@"select cc.CaCertification_ID,CertificationID,Authority,cc.Ca_ID,lmcer.lm_Value as Certification, cc.FromMonth,cc.FromYear,cc.ToMonth,cc.ToYear,cc.DoNotExpire from [CandidateCertification_T] cc
                join ListsMaster lmcer on lmcer.lm_Id = cc.CertificationID
                where ca_id=" + Convert.ToInt32(Session["Ca_ID"]) + " order by fromyear desc, frommonth desc").ToList();


                ViewBag.IdealCareerMove = db.Database.SqlQuery<IdealCareeMove>(@"select icm.CaICM_ID,icm.Ca_ID,icm.JobTitleID,icm.FunctionID,icm.IndustryID,icm.ExperienceID,icm.ExpectedSalaryID, lmjt.lm_Value as JobTitle,lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction,lmexp.lm_Value as Experience, lmsal.lm_Value as ExpectedSalary from [CandidateIdealCareerMove_T] icm
                join ListsMaster lmjt on lmjt.lm_Id = icm.JobTitleID
                join ListsMaster lmfn on lmfn.lm_Id = icm.FunctionID
                join ListsMaster lmind on lmind.lm_Id = icm.IndustryID
                join ListsMaster lmexp on lmexp.lm_Id = icm.ExperienceID
                join ListsMaster lmsal on lmsal.lm_Id = icm.ExpectedSalaryID
                where ca_id=" + candidateid).ToList();



                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,33,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderByDescending(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.OSkill = ddLists.Where(x => x.lm_lt_ListTypeId == 33).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();


            }

            dbOperations dbo = new dbOperations();
            List<CandidateSkills> cask = dbo.getCandidateSkills(candidateid);
            ViewBag.CandidateSkill = cask;

            List<CandidateSkills> ocask = dbo.getCandidateOSkills(candidateid);
            ViewBag.CandidateOSkill = ocask;

            List<CandidateLanguages> canlan = dbo.getCandidateLanguages(candidateid);
            ViewBag.CandidateLanguage = canlan;


            return View();
        }


        public ActionResult BackOfficeLogin()
        {



            return View();
        }


        [HttpPost]
        public ActionResult BackOfficeLogin(FormCollection col)
        {
            string emailid = Convert.ToString(col["emailid"]);
            string password = Convert.ToString(col["password"]);

            if (emailid == "admin@jobs4bahrainis.com" && password == "j4b2018")
            {
                Session["BackOfficeLogin"] = "YES";
                return RedirectToAction("BackOfficeEmployersAssignPackage");
            }
            else
            {
                ViewBag.Message = "Invalid User ID / Password";
            }

            return View();
        }


        public ActionResult BackOfficeNonActivatedCandiates()
        {

            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<CandidateNew> candidates = db.Database.SqlQuery<CandidateNew>(@"select ca_id,ca_guid,ca_PhoneMobile,ca_created,ca_lastupdated,ca_firstname,ca_surname,ca_emailaddress,ca_active,ca_completedregtabs 
                 from candidates where ca_completedregtabs>=3 and ca_active=0 and ca_deleted is null   order by ca_lastupdated desc").ToList();
                ViewBag.candidates = candidates;
            }

            return View();
        }


        public ActionResult BackOfficeActivatedCandiates()
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<CandidateNew> candidates = db.Database.SqlQuery<CandidateNew>(@"select ca_id,ca_guid,ca_PhoneMobile,ca_created,ca_lastupdated,ca_firstname,ca_surname,ca_emailaddress,ca_active,ca_completedregtabs from candidates where ca_completedregtabs>=3 and ca_active=1  and ca_deleted is null   order by ca_lastupdated desc").ToList();
                ViewBag.candidates = candidates;
            }

            return View();
        }

        public ActionResult BackOfficeRegistrationProgressCandiates()
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<CandidateNew> candidates = db.Database.SqlQuery<CandidateNew>(@"select ca_id,ca_guid,ca_PhoneMobile,ca_created,ca_lastupdated,ca_firstname,ca_surname,ca_emailaddress,ca_active,ca_completedregtabs  from candidates where ca_completedregtabs<3 and ca_completedregtabs>0 and ca_active=0  and ca_deleted is null and ca_created>'2018-12-31 00:00:00.780'  order by ca_completedregtabs desc").ToList();
                ViewBag.candidates = candidates;
            }

            return View();
        }

        public ActionResult BackOfficeEmployersTemp()
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<EmployersTemp> EmployersTemp_ = db.Database.SqlQuery<EmployersTemp>(@"select  * from EmployerComingSoon_T  order by id desc").ToList();
                ViewBag.EmployersTemp = EmployersTemp_;
            }

            return View();
        }


        public ActionResult BackOfficeEmployersAssignPackage()
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                //   ViewBag.Employers = db.Database.SqlQuery<AllCompanies>("select co_id,co_Name from companies where co_Deleted is null and co_activated = 1 and co_id not in (select companyid from CSSubscription_T where enddate > getdate()) order by co_name").ToList();
                ViewBag.Employers = db.Database.SqlQuery<AllCompanies>("select co_id,co_Name from companies where co_Deleted is null and co_activated = 1 order by co_name").ToList();
                ViewBag.Employersall = db.Database.SqlQuery<AllCompanies>(@"select co_created,co_id,ct_id,co_name,ct_forename,ct_surname,ct_emailaddress,ct_telephone  from companies c 
                left join contacts ct on ct.ct_co_companyid = c.co_id
                where co_activated = 1 and ct_superuser = 1  and co_id not in (select companyid from CSSubscription_T where enddate > getdate())").ToList();


            }
            return View();
        }




        public ActionResult BackOfficeVacancies()
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                ViewBag.BOVacancies = db.Database.SqlQuery<BOVacancies>(@"select vc_id as ID,c.co_Name as Company,vc_Title as Title,vc_lastupdated as LastUpdated from vacancies v
  join companies c on v.vc_co_companyid = c.co_id
  where vc_deleted is null and vc_ExpiryDate <= getdate()
  order by vc_id desc").ToList();


            }
            return View();
        }


        public ActionResult BackOfficeJobFair()
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                ViewBag.BOJobFair = db.Database.SqlQuery<BOJobFair>(@"select * from [dbo].[JobFair_T] order by id desc").ToList();


            }
            return View();
        }



        [HttpPost]
        public ActionResult BackOfficeEmployersAssignPackage2(FormCollection col)
        {
            dbOperations dbo = new dbOperations();



            dbo.CS_AssignSubscriptiontoCompany(Convert.ToInt32(col["CompanyID"]), Convert.ToInt32(col["ContactID"]), Convert.ToInt32(col["PackageID"]));


            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                reccontacts recruiter = db.Database.SqlQuery<reccontacts>("select co_Guid,ct_EmailAddress from companies c join contacts ct on c.co_id= ct.ct_co_companyid where ct.ct_superuser=1 and  c.co_id=" + Convert.ToInt32(col["CompanyID"]) + " and ct.ct_id=" + Convert.ToInt32(col["ContactID"])).SingleOrDefault();


                common cmn = new common();

                string path = Server.MapPath("~/Templates/J4BProfileactivatedRecruiter.html");
                string bodycontent = System.IO.File.ReadAllText(path);
                cmn.SendMail(recruiter.ct_EmailAddress, "Profile Successfully Activated  - J4B Website", bodycontent);
            }
            return RedirectToAction("BackOfficeEmployersActivated");

        }

        public ActionResult BackOfficeEmployersAssignedPackage()
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }


            return View();
        }

        //public ActionResult BackOfficeEmployersActivate()
        //{
        //    if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }


        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {
        //        ViewBag.Employers = db.Database.SqlQuery<AllCompanies>("select co_id,co_Name from companies where co_Deleted is null order by co_name").ToList();
        //    }


        //    return View();
        //}

        //public ActionResult BackOfficeEmployersActivate2(FormCollection col)
        //{
        //    if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }


        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {
        //        db.Database.ExecuteSqlCommand("Update Companies set co_activated=1 where co_id=" + Convert.ToString(col["CompanyID"]));
        //    }


        //    return View();
        //}


        public ActionResult BackOfficeEmployersActivated()
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                ViewBag.Employers = db.Database.SqlQuery<AllCompanies>(@"select co_created,co_id,ct_id,co_name,ct_forename,ct_surname,ct_emailaddress,ct_password,ct_telephone,packagename,startdate,enddate,ct_lastlogin from companies c 
                 left join contacts ct on ct.ct_co_companyid = c.co_id
                 left join CSSubscription_T s on s.companyid = c.co_id
                 left join CSPackage_T p on s.packageid = p.packageid
                 where co_activated = 1 and ct_superuser = 1 and s.packageid is not null  and s.EndDate>getdate()").ToList();
            }
            return View();
        }

        public ActionResult BackOfficeEmployersEmailNonActivated()
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                ViewBag.Employers = db.Database.SqlQuery<AllCompanies>(@"select co_created,co_id,ct_id,co_name,ct_forename,ct_surname,ct_emailaddress,ct_password,ct_telephone,ct_lastlogin  from companies c 
                 left join contacts ct on ct.ct_co_companyid = c.co_id
                where co_activated=0  and ct_id is not null").ToList();
            }
            return View();
        }


        public ActionResult BackOfficeJobSeekerProfile(string guid)
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }


            ViewBag.guidz = guid;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {



                int candidateid = db.Database.SqlQuery<int>("select ca_id from candidates where ca_guid='" + guid + "'").SingleOrDefault();
                ViewBag.Candidateid = candidateid;




                var candidate = db.Database.SqlQuery<CandidateNew>(@"select ca_created,ca_GUID,ca_PhotoExtension, ca_HasPhoto,c.ca_lm_countryid,ca_lm_EducationLevel,ca_UniversityID,ca_FunctionTitleID,ca_CurrentJobTitleID,ca_TotalRelavantExperience,c.Ca_CurrentCountryID, c.ca_FirstName,c.ca_Surname,lmcona.lm_Value as Nationality, ca_PhoneMobile,ca_EmailAddress,ca_Password, c.ca_DateOfBirth, c.ca_Gender, c.ca_MaritalStatus,
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
                where c.ca_id=" + candidateid + " ").ToList();


                if (candidate.Count == 0)
                {
                    return RedirectToAction("../BackOfficeLogin");
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
                ViewBag.MaritalStatus = candidate[0].ca_MaritalStatus;
                ViewBag.Ca_GUID = candidate[0].ca_GUID.ToString();

                ViewBag.DOB = Convert.ToDateTime(candidate[0].ca_DateOfBirth).ToString("dd/MM/yyyy");
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
                ViewBag.ca_lastupdated = Convert.ToDateTime(candidate[0].ca_created).ToString("dd/MM/yyyy");




                ViewBag.University2 = candidate[0].University;
                ViewBag.FunctionTitle = candidate[0].FunctionTitle;
                ViewBag.CurrentJobTitle = candidate[0].JobTitle;
                ViewBag.TotalExperience = candidate[0].TotalExperience;
                ViewBag.ca_Profile = candidate[0].ca_Profile;




                ViewBag.CandiateProfile = candidate.ToList();


                // List<CandidateDocument> candidatedocument = db.Database.SqlQuery<CandidateDocument>("select top 2 * from CandidateDocuments where cd_Type in('CV','ID') and cd_ca_CandidateID=(select top 1 ca_id from candidates where ca_ID='" + candidateid + "') order by cd_id desc").ToList();

                List<CandidateDocument> candidatedocument = db.Database.SqlQuery<CandidateDocument>("select  top 1 * from CandidateDocuments where  cd_ca_CandidateID=" + candidateid + " and cd_type='CV'  union all select  top 1 * from CandidateDocuments where cd_ca_CandidateID=" + candidateid + " and cd_type='ID'").ToList();

                foreach (var cd in candidatedocument)
                {

                    if (cd.cd_Type.Trim() == "CV")
                    {
                        ViewBag.CVpath = "Documents/cvs/" + cd.cd_Guid.ToString().Substring(0, 2) + "/" + cd.cd_Guid.ToString().Substring(2, 2) + "/" + cd.cd_Guid.ToString().Substring(4, 2) + "/" + cd.cd_Guid.ToString() + "." + cd.cd_FileExtension;
                    }
                    else if (cd.cd_Type.Trim() == "ID")
                    {
                        ViewBag.IDpath = "Documents/Identity/" + cd.cd_Guid.ToString().Substring(0, 2) + "/" + cd.cd_Guid.ToString().Substring(2, 2) + "/" + cd.cd_Guid.ToString().Substring(4, 2) + "/" + cd.cd_Guid.ToString() + "." + cd.cd_FileExtension;
                    }

                }







                ViewBag.Education = db.Database.SqlQuery<CandidateEducationNew>(@"select ce.CaEdu_ID,ce.Ca_ID,UniversityID,UniCountryID,DegreeID,SpecializationID,GradeID,lmu.lm_Value as University,lmcon.lm_Value as UniCountry, lmDeg.lm_Value as Degree, lmSpez.lm_Value as Specialization, lmgr.lm_Value as Grade, ce.FromMonth,ce.FromYear, ce.ToMonth,ce.ToYear, ce.CurrentlyStudyHere,ce.Activities  from [CandidateEducation_T] ce 
                join ListsMaster lmu on lmu.lm_Id= ce.UniversityID
                join ListsMaster lmCon on lmCon.lm_Id= ce.UniCountryID
                join ListsMaster lmDeg on lmDeg.lm_Id= ce.DegreeID
                join ListsMaster lmSpez on lmSpez.lm_Id= ce.SpecializationID
                join ListsMaster lmGr on lmGr.lm_Id= ce.GradeID
                where ca_id=" + candidateid + " order by fromyear desc, frommonth desc").ToList();

                var candidatemediadocs = db.Database.SqlQuery<CandidateDocuments>("select cd_CaEdu_ID,cd_OriginalName,cd_FileExtension from candidatedocuments where cd_ca_CandidateId=" + Convert.ToInt32(Session["Ca_ID"]) + " and cd_Type= 'MEDIA' and cd_CaEdu_ID is not null").ToList();
                ViewBag.candidatemediadocs = candidatemediadocs.ToList();




                ViewBag.Experience2 = db.Database.SqlQuery<CandidateWorkHistory>(@"select cwh.CaWorkHistory_ID,cwh.Ca_ID,JobTitleID,cwh.JobLevelID,cwh.ReportingToID,cwh.IndustryID,cwh.FunctionID,cwh.JobLocationID,cwh.SalaryID,lmjt.lm_Value as JobTitle, lmjl.lm_Value as JobLevel, cwh.Company, lmrt.lm_Value as ReportingTo, lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction, 
                cwh.FromMonth,cwh.FromYear,cwh.ToMonth,cwh.ToYear,cwh.CurrentlyWorkingHere,lmjlo.lm_Value as JobLocation, lmsal.lm_Value as Salary,cwh.[Description] from [CandidateWorkHistory_T] cwh 
                join ListsMaster lmjt on lmjt.lm_Id = cwh.JobTitleID
                join ListsMaster lmjl on lmjl.lm_Id = cwh.JobLevelID
                join ListsMaster lmrt on lmrt.lm_Id = cwh.ReportingToID
                join ListsMaster lmind on lmind.lm_Id = cwh.IndustryID
                join ListsMaster lmfn on lmfn.lm_Id = cwh.FunctionID
                join ListsMaster lmjlo on lmjlo.lm_Id = cwh.JobLocationID
                join ListsMaster lmsal on lmsal.lm_Id = cwh.SalaryID
                where ca_id=" + candidateid + " order by fromyear desc, frommonth desc").ToList();


                ViewBag.CandidateCertifications = db.Database.SqlQuery<CandidateCertifications>(@"select cc.CaCertification_ID,CertificationID,Authority,cc.Ca_ID,lmcer.lm_Value as Certification, cc.FromMonth,cc.FromYear,cc.ToMonth,cc.ToYear,cc.DoNotExpire from [CandidateCertification_T] cc
                join ListsMaster lmcer on lmcer.lm_Id = cc.CertificationID
                where ca_id=" + candidateid + " order by fromyear desc, frommonth desc").ToList();


                ViewBag.IdealCareerMove = db.Database.SqlQuery<IdealCareeMove>(@"select icm.CaICM_ID,icm.Ca_ID,icm.JobTitleID,icm.FunctionID,icm.IndustryID,icm.ExperienceID,icm.ExpectedSalaryID, lmjt.lm_Value as JobTitle,lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction,lmexp.lm_Value as Experience, lmsal.lm_Value as ExpectedSalary from [CandidateIdealCareerMove_T] icm
                join ListsMaster lmjt on lmjt.lm_Id = icm.JobTitleID
                join ListsMaster lmfn on lmfn.lm_Id = icm.FunctionID
                join ListsMaster lmind on lmind.lm_Id = icm.IndustryID
                join ListsMaster lmexp on lmexp.lm_Id = icm.ExperienceID
                join ListsMaster lmsal on lmsal.lm_Id = icm.ExpectedSalaryID
                where ca_id=" + candidateid).ToList();



                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,33,24,26,28,29,30) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.FunctionalTitles = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderByDescending(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.OSkill = ddLists.Where(x => x.lm_lt_ListTypeId == 33).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();


            }



            return View();
        }

 
        public ActionResult BOEmpActivationResend(Guid co_Guid) {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }

            int Co_ID = Convert.ToInt32(Session["CompanyID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                reccontacts recruiter = db.Database.SqlQuery<reccontacts>("select co_Guid,ct_EmailAddress from companies c join contacts ct on c.co_id= ct.ct_co_companyid where c.co_Guid='" + co_Guid + "'").SingleOrDefault();
                string path = Server.MapPath("~/Templates/EmployerActivation.html");
                string bodycontent = System.IO.File.ReadAllText(path);
                bodycontent = bodycontent.Replace("##activationmaillink##", "http://www.jobs4bahrainis.com/ActivateEmployer/" + recruiter.co_Guid + "/WelcometoJ4B");
                common cmn = new common();

                cmn.SendMail(recruiter.ct_EmailAddress, "Account Activation - J4B Website", bodycontent);
            }

            return View();


        }



        public ActionResult BOEmailActivateCompany(int companyid)
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update COMPANIES set co_activated=1 where co_id=" + companyid + "");
            }
            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult BOCompanyDelete(int companyid)
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Delete from contacts where ct_co_companyid=" + companyid + ";Delete from COMPANIES where co_id=" + companyid + "");
            }
            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult BOCandiateDelete(int candidateid)
        {
            if (Session["BackOfficeLogin"] == null) { return RedirectToAction("BackOfficeLogin"); }
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Delete from candidates where ca_id=" + candidateid);
            }
            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        



        public ActionResult CandidateActivate(Guid guid)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update Candidates set ca_active=1 where ca_guid='" + guid + "'");

                Candidate candidate = db.Database.SqlQuery<Candidate>("select * from candidates where ca_guid='" + guid + "'").SingleOrDefault();




                string path = Server.MapPath("~/Templates/J4BProfileactivated.html");
                string bodycontent = System.IO.File.ReadAllText(path);
                common cmn = new common();
                cmn.SendMail(candidate.ca_EmailAddress, "Account Activated - J4B Website", bodycontent);
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }





        public ActionResult CandidateReject(Guid guid)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("Update Candidates set ca_active=0, ca_deleted=getdate() where ca_guid='" + guid + "'");
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
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


        public ActionResult DeleteCompanyManagement(int CompanyManagementID, int itemno)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();

                if (itemno == 1)
                {
                    sb.AppendFormat("update [CompanyManagement_T] set Name='',Position='',Extn='' where CompanyManagementID={0} and Co_Id={1}", CompanyManagementID, Convert.ToString(Session["CompanyID"]));
                }
                else if (itemno == 2)
                {
                    sb.AppendFormat("update [CompanyManagement_T] set Name2='',Position2='',Extn2='' where CompanyManagementID={0} and Co_Id={1}", CompanyManagementID, Convert.ToString(Session["CompanyID"]));
                }
                else if (itemno == 3)
                {
                    sb.AppendFormat("update [CompanyManagement_T] set Name3='',Position3='',Extn3='' where CompanyManagementID={0} and Co_Id={1}", CompanyManagementID, Convert.ToString(Session["CompanyID"]));
                }
                else if (itemno == 4)
                {
                    sb.AppendFormat("update [CompanyManagement_T] set Name4='',Position4='',Extn4='' where CompanyManagementID={0} and Co_Id={1}", CompanyManagementID, Convert.ToString(Session["CompanyID"]));
                }

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

        public ActionResult DeletePhotoGallery(int PGID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Delete from [CompanyPhotoGallery_T] where pgid={0} and Co_ID={1}", PGID, Convert.ToString(Session["CompanyID"]));
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



        public ActionResult DeleteTestimonials(int CTID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Delete from [CompanyTestimonials_T] where CTID={0} and Co_ID={1}", CTID, Convert.ToString(Session["CompanyID"]));
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


        public ActionResult DeleteMedia(int CMID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Delete from [CompanyMedia_T] where CMID={0} and Co_ID={1}", CMID, Convert.ToString(Session["CompanyID"]));
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




        public ActionResult DeleteVideo(int CoVideo_ID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Delete from [CompanyVideo_T] where CoVideo_ID={0} and Co_ID={1}", CoVideo_ID, Convert.ToString(Session["CompanyID"]));
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
                sb.AppendFormat("select count(ca_id) as count from [CandidateLanguage_T] where Ca_ID={0}", Convert.ToInt32(Session["Ca_ID"]), Caln_ID);
                int ca_id = db.Database.SqlQuery<int>(sb.ToString()).SingleOrDefault();

                if (ca_id > 1)
                {
                    sb = new StringBuilder();
                    sb.AppendFormat("Delete from [CandidateLanguage_T] where Ca_ID={0} and CaLanguage_ID={1}", Convert.ToInt32(Session["Ca_ID"]), Caln_ID);
                    db.Database.ExecuteSqlCommand(sb.ToString());
                }
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


        public ActionResult DeleteCandidateOSkill(int OCaSkillID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from [CandidateOtherSkill_T] where Ca_ID={0} and caoskill_id={1}", Convert.ToInt32(Session["Ca_ID"]), OCaSkillID);
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
        public ActionResult Search(FormCollection col, int? page = 1)
        {
            string keywords = Convert.ToString(col["keywords"]);
            string sector = Convert.ToString(col["sector"]);

            ViewBag.Sectors = listmastervalues(4);

            if (keywords == "")
            {
                ViewBag.keywords = "0";
            }
            else
            {
                ViewBag.keywords = keywords;
            }
            ViewBag.sector = sector;

            StringBuilder sb = new StringBuilder();

            string sqlqry = @"select  vc_Id,co_id,vc_Confidential,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension,vc_co_CompanyId,vc_views,vc_applications from Vacancies v
            inner join Companies on vc_co_CompanyId = co_Id
            join listsmaster lm on lm.lm_Id = v.vc_JobLocationID
            where vc_Deleted is null and vc_st_StatusID = 1  and vc_ExpiryDate>=getdate() ";

            if (keywords != "")
            {
                sqlqry += " and vc_Title like '%{0}%' ";
            }

            if (sector != "0")
            {
                sqlqry += " and vc_industryid={1}";
            }

        

            sqlqry += " order by vc_id desc";

            sb.AppendFormat(sqlqry, keywords, sector);

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location;
                    ljx.vc_Description = TruncateAtWord(StripHTML(lj.vc_Description), 100);
                    ljx.vc_Created = lj.vc_Created;
                    ljx.vc_Confidential = lj.vc_Confidential;

                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;
                    ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


                    //int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    ljx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    ljx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    ljx.postedsince = "" + days + " days ago";
                    //}

                    ljx.postedsince = lj.vc_Created.ToString("dd-MM-yyyy");

                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    ljx.vc_views = lj.vc_views;
                    ljx.vc_applications = lj.vc_applications;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToPagedList(page ?? 1, 10);



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
            ViewBag.Sectors = listmastervalues(4);
            ViewBag.keywords = keywords;
            ViewBag.sector = sector;

            StringBuilder sb = new StringBuilder();

            string sqlqry = @"select  vc_Id,co_id,vc_Confidential,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension,vc_co_CompanyId,vc_views,vc_applications from Vacancies v
            inner join Companies on vc_co_CompanyId = co_Id
            join listsmaster lm on lm.lm_Id = v.vc_JobLocationID
            where vc_Deleted is null and vc_st_StatusID = 1  and vc_ExpiryDate>=getdate()";

            if (keywords != "0")
            {
                sqlqry += " and vc_Title like '%{0}%' ";
            }

            if (sector != "0")
            {
                sqlqry += " and vc_industryid={1}";
            }



            sqlqry += " order by vc_id desc";


            
            sb.AppendFormat(sqlqry, keywords, sector);


            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location;
                    ljx.vc_Description = TruncateAtWord(StripHTML(lj.vc_Description), 100);
                    ljx.vc_Created = lj.vc_Created;
                    ljx.vc_Confidential = lj.vc_Confidential;

                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;
                    ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


                    //int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    ljx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    ljx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    ljx.postedsince = "" + days + " days ago";
                    //}

                    ljx.postedsince = lj.vc_Created.ToString("dd-MM-yyyy");

                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    ljx.vc_views = lj.vc_views;
                    ljx.vc_applications = lj.vc_applications;
                    LatestJobs2.Add(ljx);

                }


                ViewBag.LatestJobs = LatestJobs2.ToPagedList(page ?? 1, 10);



            }

            return View();
        }


        public ActionResult SectorJobs(int sectorid, string sector, int? page = 1)
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


            sb.AppendFormat(@"select  vc_Id,co_id,vc_Confidential,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension,vc_co_CompanyId,vc_views,vc_applications from Vacancies v
inner join Companies on vc_co_CompanyId=co_Id 
join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
where vc_Deleted is null and vc_st_StatusID=1 and vc_industryid={0} and vc_ExpiryDate>=getdate() and vc_ExpiryDate>=getdate() order by vc_Title", sectorid);


            ViewBag.Sectors = listmastervalues(4);

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location;
                    ljx.vc_Description = TruncateAtWord(StripHTML(lj.vc_Description), 100);
                    ljx.vc_Created = lj.vc_Created;
                    ljx.vc_Confidential = lj.vc_Confidential;

                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;
                    ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


                    //int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    ljx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    ljx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    ljx.postedsince = "" + days + " days ago";
                    //}
                    ljx.postedsince = lj.vc_Created.ToString("dd-MM-yyyy");
                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    ljx.vc_views = lj.vc_views;
                    ljx.vc_applications = lj.vc_applications;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToPagedList(page ?? 1, 10);


            }

            return View();
        }


        public ActionResult CompanyJobs(int companyid, int? page = 1)
        {

            if (Session["Ca_ID"] == null && Session["RecruiterID"] == null)
            {
                if (page > 1)
                {
                    return RedirectToAction("../Login/-/CompanyJobs-" + companyid +"-" + page);



                }
            }



            ViewBag.companyid = companyid;
            StringBuilder sb = new StringBuilder();


            sb.AppendFormat(@"select  vc_Id,co_id,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension,vc_co_CompanyId,vc_views,vc_applications from Vacancies v
inner join Companies on vc_co_CompanyId=co_Id 
join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
where vc_Deleted is null and vc_st_StatusID=1 and vc_co_CompanyId={0} and vc_ExpiryDate>=getdate() order by vc_Title", companyid);


            ViewBag.Sectors = listmastervalues(4);

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();

                    ljx.vc_id = lj.vc_id;
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location;
                    ljx.vc_Description = TruncateAtWord(StripHTML(lj.vc_Description), 100);
                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;
                    ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


                    //int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    ljx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    ljx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    ljx.postedsince = "" + days + " days ago";
                    //}
                    ljx.postedsince = lj.vc_Created.ToString("dd-MM-yyyy");
                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    ljx.vc_views = lj.vc_views;
                    ljx.vc_applications = lj.vc_applications;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToPagedList(page ?? 1, 10);


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


            ViewBag.Sectors = listmastervalues(4);




            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(@"select  vc_Id,co_id,vc_Confidential,vc_Title,lm.lm_Value as vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension,vc_co_CompanyId,vc_views,vc_applications from Vacancies v
            inner join Companies on vc_co_CompanyId=co_Id 
            join listsmaster lm on lm.lm_Id= v.vc_JobLocationID
            where vc_Deleted is null and vc_st_StatusID=1 and vc_ExpiryDate>=getdate() order by vc_Id desc");

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {




                List<LatestJobs> LatestJobs1 = db.Database.SqlQuery<LatestJobs>(sb.ToString()).ToList();
                List<LatestJobs> LatestJobs2 = new List<LatestJobs>();
                LatestJobs ljx;


                foreach (var lj in LatestJobs1)
                {
                    ljx = new LatestJobs();
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;
                    ljx.vc_id = lj.vc_id;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Location = lj.vc_Location;

                    ljx.vc_Confidential = lj.vc_Confidential;


                    ljx.vc_Description = TruncateAtWord(StripHTML(lj.vc_Description), 100);

                    ljx.vc_Created = lj.vc_Created;
                    try
                    {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
                    }
                    catch
                    {
                        ljx.co_Name = lj.co_Name;
                    }
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;

                    ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


                    //int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    ljx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    ljx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    ljx.postedsince = "" + days + " days ago";
                    //}


                    ljx.postedsince = lj.vc_Created.ToString("dd-MM-yyyy");
                    ljx.vc_views = lj.vc_views;
                    ljx.vc_applications = lj.vc_applications;
                    ljx.jobURL = urlcleaner(lj.vc_Title);
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToPagedList(page ?? 1, 10);


            }

            return View();
        }

        public ActionResult JobSearchDetail(int vc_id = 0)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {



                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"select top 100 vc_Id,co_id,vc_Confidential,vc_Title,lm_Id,lm_value,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
                inner join Companies c on v.vc_co_CompanyId = c.co_Id 
                left outer join VacancySectors vs on vs.vs_vc_VacancyId = v.vc_Id
                left outer join ListsMaster lm on lm.lm_Id = vs.vs_lm_SectorId
                where vc_Deleted is null and vc_st_StatusID=1 and vc_id ={0} ", vc_id);

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







            }
            return View();
        }

        public ActionResult JobDescription(int jobid, string jobtitle)
        {
            if (Session["Ca_ID"] == null && Session["RecruiterID"] == null) { return RedirectToAction("../Login/-/JobDescription-" + jobid); }

            if (Convert.ToString(Session["ReginProgress"]) == "YES")
            {
                return RedirectToAction("Login");
            }

            ViewBag.VacancyID = jobid;

            ViewBag.Sectors = listmastervalues(4);

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {



                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"update Vacancies set vc_views= vc_views+1 where vc_id={0};select v.vc_id, v.vc_views,v.vc_Confidential,v.vc_applications,vc_created,v.vc_co_CompanyId,co.co_Name,co.co_guid,co.co_LogoExtension,vc_title,ct.lm_Value as JobType,v.vc_Description as vc_Description,ind.lm_Value as Industry, fn.lm_Value as Functions,
ex.lm_Value as Experience, ql.lm_Value as Qualification, ag.lm_Value as AgeGroup, sal.lm_Value as SalaryRange, jl.lm_Value as JobLocation, v.vc_JobRequirements,v.vc_CompanyDetails
from Vacancies v 
join listsmaster ind on v.vc_IndustryID= ind.lm_Id
join listsmaster fn on v.vc_FunctionID= fn.lm_Id
join ListsMaster ex on v.vc_ExperienceID= ex.lm_Id
join ListsMaster ql on v.vc_QualificationID= ql.lm_Id
join ListsMaster ag on v.vc_AgegroupID= ag.lm_Id
join ListsMaster sal on v.vc_SalaryRangeID= sal.lm_Id
join ListsMaster jl on v.vc_JobLocationID = jl.lm_Id
join ListsMaster ct on v.vc_lm_JobTypeId = ct.lm_Id
join companies co on co.co_id= v.vc_co_companyid
where vc_Deleted is null and vc_st_StatusID=1 and vc_id ={0} order by vc_id desc ", jobid);

                List<JobDetails> JobDetails = db.Database.SqlQuery<JobDetails>(sb.ToString()).ToList();
                List<JobDetails> JobDetails2 = new List<JobDetails>();
                JobDetails ljx;


                foreach (var lj in JobDetails)
                {
                    ljx = new JobDetails();

                    ljx.vc_Id = lj.vc_Id;
                    ljx.vc_Created = lj.vc_Created;
                    ljx.co_Name = lj.co_Name;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Description = lj.vc_Description.Replace(System.Environment.NewLine, "<br>");
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    ljx.vc_Created = lj.vc_Created;
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;
                    ViewBag.CompanyID = lj.vc_co_CompanyId; 
                    ljx.vc_Confidential = lj.vc_Confidential;
                    string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;
                    ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


                    //int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    //if (days == 0)
                    //{
                    //    ljx.postedsince = "Today";
                    //}
                    //else if (days == 1)
                    //{
                    //    ljx.postedsince = "" + days + " day ago";
                    //}
                    //else if (days > 1)
                    //{
                    //    ljx.postedsince = "" + days + " days ago";
                    //}

                    ljx.postedsince = lj.vc_Created.ToString("dd-MM-yyyy");

                    ljx.JobType = lj.JobType;


                    ljx.Industry = lj.Industry;
                    ljx.Functions = lj.Functions;
                    ljx.Experience = lj.Experience;
                    ljx.Qualification = lj.Qualification;
                    ljx.AgeGroup = lj.AgeGroup;
                    ljx.SalaryRange = lj.SalaryRange;
                    ljx.JobLocation = lj.JobLocation;
                    ljx.vc_CompanyDetails = lj.vc_CompanyDetails;
                    ljx.vc_JobRequirements = lj.vc_JobRequirements;
                    ljx.vc_Views = lj.vc_Views;
                    ljx.vc_Applications = lj.vc_Applications;
                    JobDetails2.Add(ljx);

                }

                ViewBag.JobDetails = JobDetails2.ToList();

            }
            return View();
        }

        public ActionResult JobDescEmployer(int jobid, string jobtitle)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("../Login/-/JobDescEmployer-" + jobid); }

            ViewBag.VacancyID = jobid;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {


                int recruiterid = Convert.ToInt32(Session["RecruiterID"]);

                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns2 = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "../../Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns2;




                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.SurName = recruiter[0].ct_Surname;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;


                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"select v.vc_id,vc_created,v.vc_co_CompanyId,co.co_Name,co.co_guid,co.co_LogoExtension,vc_title,ct.lm_Value as JobType,v.vc_Description as vc_Description,ind.lm_Value as Industry, fn.lm_Value as Functions,
                ex.lm_Value as Experience, ql.lm_Value as Qualification, ag.lm_Value as AgeGroup, sal.lm_Value as SalaryRange, jl.lm_Value as JobLocation, v.vc_JobRequirements,v.vc_CompanyDetails
                from Vacancies v 
                join listsmaster ind on v.vc_IndustryID= ind.lm_Id
                join listsmaster fn on v.vc_FunctionID= fn.lm_Id
                join ListsMaster ex on v.vc_ExperienceID= ex.lm_Id
                join ListsMaster ql on v.vc_QualificationID= ql.lm_Id
                join ListsMaster ag on v.vc_AgegroupID= ag.lm_Id
                join ListsMaster sal on v.vc_SalaryRangeID= sal.lm_Id
                join ListsMaster jl on v.vc_JobLocationID = jl.lm_Id
                join ListsMaster ct on v.vc_lm_JobTypeId = ct.lm_Id
                join companies co on co.co_id= v.vc_co_companyid
                where vc_id={0} order by vc_id desc", jobid);

                List<JobDetails> JobDetails = db.Database.SqlQuery<JobDetails>(sb.ToString()).ToList();
                List<JobDetails> JobDetails2 = new List<JobDetails>();
                JobDetails ljx;


                foreach (var lj in JobDetails)
                {
                    ljx = new JobDetails();

                    ljx.vc_Id = lj.vc_Id;
                    ljx.vc_Created = lj.vc_Created;
                    ljx.co_Name = lj.co_Name;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Description = lj.vc_Description.Replace(System.Environment.NewLine, "<br>");
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    ljx.vc_Created = lj.vc_Created;
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;

                    string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;
                    ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days == 0)
                    {
                        ljx.postedsince = "Today";
                    }
                    else if (days == 1)
                    {
                        ljx.postedsince = "" + days + " day ago";
                    }
                    else if (days > 1)
                    {
                        ljx.postedsince = "" + days + " days ago";
                    }



                    ljx.JobType = lj.JobType;
                    ljx.Industry = lj.Industry;
                    ljx.Functions = lj.Functions;
                    ljx.Experience = lj.Experience;
                    ljx.Qualification = lj.Qualification;
                    ljx.AgeGroup = lj.AgeGroup;
                    ljx.SalaryRange = lj.SalaryRange;
                    ljx.JobLocation = lj.JobLocation;
                    ljx.vc_CompanyDetails = lj.vc_CompanyDetails;
                    ljx.vc_JobRequirements = lj.vc_JobRequirements;
                    JobDetails2.Add(ljx);

                }

                ViewBag.JobDetails = JobDetails2.ToList();


                List<jobapplicants> jbapplicants = db.Database.SqlQuery<jobapplicants>(@"select ca_id,app.app_id,app.app_st_ShortlistStatusId,ca.ca_PhotoExtension,app_created,ca_LastLogin,ca_GUID, ca_FirstName,ca_Surname,cc.lm_Value as CurrentCountry,el.lm_Value as educationlevel,ca_Profile,ca_hasphoto,ca_photoextension,ca_Gender,ca_DateOfBirth,
                         ca_maritalstatus,ft.lm_Value as functiontitle,cjt.lm_Value as CurrentJobTitle, tre.lm_Value as TotalRelevantExperience  from candidates ca
                         join listsmaster cc on cc.lm_Id=ca.ca_CurrentCountryID
                         join ListsMaster el on el.lm_id=ca.ca_lm_EducationLevel
                         join ListsMaster ft on ft.lm_id=ca.ca_functiontitleid
                         join listsmaster cjt on cjt.lm_id=ca.ca_currentjobtitleid
                         join listsmaster tre on tre.lm_id=ca.ca_TotalRelavantExperience
					     right join Applications app on ca.ca_Id = app.app_ca_CandidateId
                         where ca_Active=1 and app_st_ShortlistStatusId=0 and app.app_vc_VacancyId=" + jobid).ToList();



                ViewBag.jbapplicants = jbapplicants;


            }

            dbOperations dbo = new dbOperations();
            ViewBag.ListTypeIDs = dbo.getlist(35);


            return View();
        }




        public ActionResult NoValidSubscription()
        {



            return View();
        }

        public ActionResult NoPermission()
        {

            return View();
        }


        public ActionResult PostAJob()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }
            dbOperations dbo = new dbOperations();

            if (!dbo.CS_CreditCheck(Convert.ToInt32(Session["CompanyID"]), 1))
            {
                return RedirectToAction("NoValidSubscription");
            }



            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();

                if (recruiter[0].ct_postjob == false)
                {

                    return RedirectToAction("NoPermission");
                }

                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                ViewBag.co_JobCompanyDesc = company[0].co_JobCompanyDesc;

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                //http://www.jobs4bahrainis.com/Logox/
                ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;


                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (32,6,5,12,28,13,17,4) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Functions = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobType = ddLists.Where(x => x.lm_lt_ListTypeId == 6).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Agegroup = ddLists.Where(x => x.lm_lt_ListTypeId == 32).OrderBy(x => x.lm_Value).ToList();





            }
            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult PostedJob(FormCollection col)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            addjob(col, 1);

            dbOperations dbo = new dbOperations();
            dbo.ReportAdd(Convert.ToInt32(Session["CompanyID"]), Convert.ToInt32(Session["RecruiterID"]), 3);

            return RedirectToAction("../ManageVacancies");
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult PostedJobDraft(FormCollection col)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            addjob(col, 0);

            return RedirectToAction("../ManageVacancies");
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult PostedJobPreview(FormCollection col)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int vacancyid = addjob(col, 0);

            return RedirectToAction("../JobEdit/" + vacancyid + "/preview");
        }



        public int addjob(FormCollection col, int jobstatusid)
        {


            common cmn = new common();
            dbOperations dbo = new dbOperations();


            //creating job
            SqlCon mycon = new SqlCon();

            string sql = @"INSERT INTO Vacancies(vc_Created, vc_LastUpdated, vc_br_SourceBrandId, vc_co_CompanyId, vc_ct_ContactId, vc_Reference, vc_Title, vc_lm_JobTypeId, vc_JobLocationID, vc_IndustryID, vc_FunctionID, vc_ExperienceID, vc_QualificationID, vc_AgeGroupID, vc_SalaryRangeID, vc_JobRequirements, vc_CompanyDetails, vc_Description, vc_ApplicationEmail, vc_ApplicationEmail2, vc_ClosingDate, vc_ExpiryDate, vc_st_StatusID, vc_Disabled,vc_Confidential) VALUES " +
                        " (@vc_Created,@vc_LastUpdated,@vc_br_SourceBrandId,@vc_co_CompanyId,@vc_ct_ContactId,@vc_Reference,@vc_Title,@vc_lm_JobTypeId,@vc_JobLocationID,@vc_IndustryID,@vc_FunctionID,@vc_ExperienceID,@vc_QualificationID,@vc_AgeGroupID,@vc_SalaryRangeID,@vc_JobRequirements,@vc_CompanyDetails,@vc_Description,@vc_ApplicationEmail,@vc_ApplicationEmail2,@vc_ClosingDate,@vc_ExpiryDate,@vc_st_StatusID,@vc_Disabled,@vc_Confidential); select @@identity";

            mycon.sqlCmd_.CommandText = sql;
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Created", DateTime.Now);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_LastUpdated", DateTime.Now);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_br_SourceBrandId", 2);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_co_CompanyId", Session["CompanyID"]);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ct_ContactId", Session["RecruiterID"]);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Reference", Convert.ToString(col["referencecode"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Title", Convert.ToString(col["vc_Title"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_lm_JobTypeId", Convert.ToString(col["contracttype"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_JobLocationID", Convert.ToString(col["vc_JobLocationID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_IndustryID", Convert.ToString(col["vc_IndustryID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_FunctionID", Convert.ToString(col["vc_FunctionID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ExperienceID", Convert.ToString(col["vc_ExperienceID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_QualificationID", Convert.ToString(col["vc_QualificationID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_AgeGroupID", Convert.ToString(col["vc_AgeGroupID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_SalaryRangeID", Convert.ToString(col["vc_SalaryRangeID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_JobRequirements", StripHTMLExcept(Convert.ToString(col["vc_JobRequirements"])).Replace("'", "&#039;"));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_CompanyDetails", StripHTMLExcept(Convert.ToString(col["vc_CompanyDetails"])).Replace("'", "&#039;"));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Description", StripHTMLExcept(Convert.ToString(col["description"])).Replace("'", "&#039;"));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ApplicationEmail", Convert.ToString(col["applicationemailid"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ApplicationEmail2", Convert.ToString(col["applicationemailid2"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ClosingDate", DateTime.Now.AddMonths(1));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ExpiryDate", DateTime.Now.AddMonths(1));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Confidential", Convert.ToBoolean(col["vc_Confidential"]));

            if (jobstatusid == 0)
            {
                mycon.sqlCmd_.Parameters.AddWithValue("@vc_st_StatusID", 6);
            }
            else
            {
                mycon.sqlCmd_.Parameters.AddWithValue("@vc_st_StatusID", 1);
            }

            if (col["reserved"] != "")
            {
                mycon.sqlCmd_.Parameters.AddWithValue("@vc_Disabled", Convert.ToBoolean(col["reserved"]));
            }
            else {
                mycon.sqlCmd_.Parameters.AddWithValue("@vc_Disabled", false);
            }

            mycon.sqlConOpen();
            int VacancyID = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
            mycon.sqlConClose();

            mycon = new SqlCon();
            mycon.sqlConOpen();
            sql = "Update companies set co_JobCompanyDesc='" + StripHTMLExcept(Convert.ToString(col["vc_CompanyDetails"])).Replace("'", "&#039;") + "' where co_id=" + Convert.ToInt32(Session["CompanyID"]);
            mycon.sqlCmd_.CommandText = sql;
            mycon.sqlCmd().ExecuteNonQuery();
            mycon.sqlConClose();

            return VacancyID;

        }


        public ActionResult JobEdit(int jobid, string preview)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            if (preview == "preview")
            {
                ViewBag.preview = preview;
            }


            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                int recruiterid = Convert.ToInt32(Session["RecruiterID"]);

                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns2 = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "../Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns2;




                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.SurName = recruiter[0].ct_Surname;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;


                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"select * from Vacancies v where vc_id ={0} order by vc_id desc ", jobid);

                JobDetails JobDetailstmp = db.Database.SqlQuery<JobDetails>(sb.ToString()).SingleOrDefault();

                if (JobDetailstmp.vc_FunctionID == 0 || JobDetailstmp.vc_IndustryID == 0 || JobDetailstmp.vc_ExperienceID == 0 || JobDetailstmp.vc_QualificationID == 0 || JobDetailstmp.vc_AgegroupID == 0 || JobDetailstmp.vc_SalaryRangeID == 0 || JobDetailstmp.vc_JobLocationID == 0)
                {
                }
                else // has linked values
                {

                sb = new StringBuilder();
                sb.AppendFormat(@"select v.vc_id,vc_created,v.vc_Confidential,v.vc_co_CompanyId,co.co_Name,co.co_guid,co.co_LogoExtension,vc_title,ct.lm_Value as JobType,v.vc_Description as vc_Description,ind.lm_Value as Industry, fn.lm_Value as Functions,
                ex.lm_Value as Experience, ql.lm_Value as Qualification,ct.lm_Value as JobType, ag.lm_Value as AgeGroup, sal.lm_Value as SalaryRange, jl.lm_Value as JobLocation, v.vc_JobRequirements,v.vc_CompanyDetails,
                v.vc_IndustryID,v.vc_FunctionID,v.vc_ExperienceID,v.vc_QualificationID,v.vc_AgegroupID,v.vc_SalaryRangeID,v.vc_JobLocationID,v.vc_lm_JobTypeId,v.vc_Reference,v.vc_ApplicationEmail,vc_Disabled,vc_st_StatusID
                from Vacancies v 
                join listsmaster ind on v.vc_IndustryID= ind.lm_Id
                join listsmaster fn on v.vc_FunctionID= fn.lm_Id
                join ListsMaster ex on v.vc_ExperienceID= ex.lm_Id
                join ListsMaster ql on v.vc_QualificationID= ql.lm_Id
                join ListsMaster ag on v.vc_AgegroupID= ag.lm_Id
                join ListsMaster sal on v.vc_SalaryRangeID= sal.lm_Id
                join ListsMaster jl on v.vc_JobLocationID = jl.lm_Id
                join ListsMaster ct on v.vc_lm_JobTypeId = ct.lm_Id
                join companies co on co.co_id= v.vc_co_companyid
                where vc_id={0} order by vc_id desc", jobid);

                }



          

                List<JobDetails> JobDetails = db.Database.SqlQuery<JobDetails>(sb.ToString()).ToList();
                List<JobDetails> JobDetails2 = new List<JobDetails>();
                JobDetails ljx;


                foreach (var lj in JobDetails)
                {
                    ljx = new JobDetails();

                    ljx.vc_Id = lj.vc_Id;
                    ljx.vc_Created = lj.vc_Created;
                    ljx.co_Name = lj.co_Name;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Description = lj.vc_Description.Replace(System.Environment.NewLine, "<br>");
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    ljx.vc_Created = lj.vc_Created;
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;
                    ljx.vc_Confidential = lj.vc_Confidential;

                    //string extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;
                    //ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days == 0)
                    {
                        ljx.postedsince = "Today";
                    }
                    else if (days == 1)
                    {
                        ljx.postedsince = "" + days + " day ago";
                    }
                    else if (days > 1)
                    {
                        ljx.postedsince = "" + days + " days ago";
                    }



                    ljx.JobType = lj.JobType;
                    ljx.vc_lm_JobTypeId = lj.vc_lm_JobTypeId;
                    ljx.JobType = lj.JobType;
                    ljx.Industry = lj.Industry;
                    ljx.vc_IndustryID = lj.vc_IndustryID;
                    ljx.Functions = lj.Functions;
                    ljx.vc_FunctionID = lj.vc_FunctionID;
                    ljx.Experience = lj.Experience;
                    ljx.vc_ExperienceID = lj.vc_ExperienceID;
                    ljx.Qualification = lj.Qualification;
                    ljx.vc_QualificationID = lj.vc_QualificationID;
                    ljx.AgeGroup = lj.AgeGroup;
                    ljx.vc_AgegroupID = lj.vc_AgegroupID;
                    ljx.SalaryRange = lj.SalaryRange;
                    ljx.vc_SalaryRangeID = lj.vc_SalaryRangeID;
                    ljx.JobLocation = lj.JobLocation;
                    ljx.vc_JobLocationID = lj.vc_JobLocationID;
                    ljx.vc_CompanyDetails = lj.vc_CompanyDetails;
                    ljx.vc_JobRequirements = lj.vc_JobRequirements;
                    ljx.vc_Reference = lj.vc_Reference;
                    ljx.vc_ApplicationEmail = lj.vc_ApplicationEmail;
                    ljx.vc_Disabled = lj.vc_Disabled;
                    ljx.vc_st_StatusID = lj.vc_st_StatusID;
                    JobDetails2.Add(ljx);

                }

                ViewBag.JobDetails = JobDetails2.ToList();




                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (32,6,5,12,28,13,17,4) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Functions = ddLists.Where(x => x.lm_lt_ListTypeId == 17).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobType = ddLists.Where(x => x.lm_lt_ListTypeId == 6).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Agegroup = ddLists.Where(x => x.lm_lt_ListTypeId == 32).OrderBy(x => x.lm_Value).ToList();

                ViewBag.JobID = jobid;
            }
            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult UpdateJob(FormCollection col)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            vacancyupdate(col, 1);

            return RedirectToAction("../ManageVacancies");
        }



        [ValidateInput(false)]
        [HttpPost]
        public ActionResult UpdateJobDraft(FormCollection col)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            vacancyupdate(col, 6);

            return RedirectToAction("../ManageVacancies");
        }



        [ValidateInput(false)]
        [HttpPost]
        public ActionResult UpdateJobPreview(FormCollection col)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            vacancyupdate(col, Convert.ToInt32(col["vc_st_StatusID"]));

            return RedirectToAction("../JobEdit/" + Convert.ToInt32(col["vc_id"]) + "/preview");
        }





        public void vacancyupdate(FormCollection col, int statusid)
        {


            common cmn = new common();
            dbOperations dbo = new dbOperations();
            SqlCon mycon = new SqlCon();
            string sql = @"UPDATE Vacancies SET vc_Created=@vc_Created, vc_LastUpdated=@vc_LastUpdated, vc_br_SourceBrandId=@vc_br_SourceBrandId, vc_co_CompanyId=@vc_co_CompanyId, vc_ct_ContactId=@vc_ct_ContactId, vc_Reference=@vc_Reference, vc_Title=@vc_Title, vc_lm_JobTypeId=@vc_lm_JobTypeId, vc_JobLocationID=@vc_JobLocationID, vc_IndustryID=@vc_IndustryID, vc_FunctionID=@vc_FunctionID, vc_ExperienceID=@vc_ExperienceID, vc_QualificationID=@vc_QualificationID, vc_AgeGroupID=@vc_AgeGroupID, vc_SalaryRangeID=@vc_SalaryRangeID, vc_JobRequirements=@vc_JobRequirements, vc_CompanyDetails=@vc_CompanyDetails, vc_Description=@vc_Description, vc_ApplicationEmail=@vc_ApplicationEmail, vc_ApplicationEmail2=@vc_ApplicationEmail2, vc_ClosingDate=@vc_ClosingDate, vc_ExpiryDate=@vc_ExpiryDate,vc_Deleted=null, vc_st_StatusID=@vc_st_StatusID, vc_Disabled=@vc_Disabled,vc_Confidential=@vc_Confidential where vc_id=@vc_id";
            mycon.sqlCmd_.CommandText = sql;
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_id", Convert.ToInt32(col["vc_id"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Created", DateTime.Now);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_LastUpdated", DateTime.Now);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_br_SourceBrandId", 2);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_co_CompanyId", Session["CompanyID"]);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ct_ContactId", Session["RecruiterID"]);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Reference", Convert.ToString(col["referencecode"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Title", Convert.ToString(col["vc_Title"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_lm_JobTypeId", Convert.ToString(col["contracttype"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_JobLocationID", Convert.ToString(col["vc_JobLocationID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_IndustryID", Convert.ToString(col["vc_IndustryID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_FunctionID", Convert.ToString(col["vc_FunctionID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ExperienceID", Convert.ToString(col["vc_ExperienceID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_QualificationID", Convert.ToString(col["vc_QualificationID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_AgeGroupID", Convert.ToString(col["vc_AgeGroupID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_SalaryRangeID", Convert.ToString(col["vc_SalaryRangeID"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_JobRequirements", StripHTMLExcept(Convert.ToString(col["vc_JobRequirements"])).Replace("'", "&#039;"));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_CompanyDetails", StripHTMLExcept(Convert.ToString(col["vc_CompanyDetails"])).Replace("'", "&#039;"));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Description", StripHTMLExcept(Convert.ToString(col["description"])).Replace("'", "&#039;"));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ApplicationEmail", Convert.ToString(col["applicationemailid"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ApplicationEmail2", Convert.ToString(col["applicationemailid2"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ClosingDate", DateTime.Now.AddMonths(1));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_ExpiryDate", DateTime.Now.AddMonths(1));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_st_StatusID", statusid);
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Disabled", Convert.ToBoolean(col["reserved"]));
            mycon.sqlCmd_.Parameters.AddWithValue("@vc_Confidential", Convert.ToBoolean(col["vc_Confidential"]));

            mycon.sqlConOpen();
            mycon.sqlCmd().ExecuteNonQuery();
            mycon.sqlConClose();

            mycon = new SqlCon();
            mycon.sqlConOpen();
            sql = "Update companies set co_JobCompanyDesc='" + StripHTMLExcept(Convert.ToString(col["vc_CompanyDetails"])).Replace("'", "&#039;") + "' where co_id=" + Convert.ToInt32(Session["CompanyID"]);
            mycon.sqlCmd_.CommandText = sql;
            mycon.sqlCmd().ExecuteNonQuery();

            mycon.sqlConClose();


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
                ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.SurName = recruiter[0].ct_Surname;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;


                List<ManageVacancies> lstManageVacancies = db.Database.SqlQuery<ManageVacancies>(@"select top 100 v.vc_id,vc_Created,vc_Deleted,vc_ExpiryDate,vc_st_StatusID,vc_Reference,vc_Title,c.ct_Forename,c.ct_Surname,vc_Views, count(a.app_id) as applicants from Vacancies v
inner join contacts c on c.ct_Id=vc_ct_ContactId
left join Applications a on a.app_vc_VacancyId=v.vc_Id 
where vc_co_CompanyId=" + companyid + " group by  v.vc_id,vc_Created,vc_Deleted,vc_ExpiryDate,vc_st_StatusID,vc_Reference,vc_Title,c.ct_Forename,c.ct_Surname,vc_Views order by vc_Created desc").ToList();


                ViewBag.lstManageVacancies = lstManageVacancies;



                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@" select v.vc_id,vc_created,v.vc_co_CompanyId,co.co_Name,co.co_guid,co.co_LogoExtension,vc_title,ct.lm_Value as JobType,v.vc_Description as vc_Description,ind.lm_Value as Industry, fn.lm_Value as Functions,
                ex.lm_Value as Experience, ql.lm_Value as Qualification,ct.lm_Value as JobType, ag.lm_Value as AgeGroup, sal.lm_Value as SalaryRange, jl.lm_Value as JobLocation, v.vc_JobRequirements,v.vc_CompanyDetails,
                v.vc_IndustryID,v.vc_FunctionID,v.vc_ExperienceID,v.vc_QualificationID,v.vc_AgegroupID,v.vc_SalaryRangeID,v.vc_JobLocationID,v.vc_lm_JobTypeId,v.vc_Reference,v.vc_ApplicationEmail,vc_Disabled,vc_st_StatusID
                from Vacancies v 
                join listsmaster ind on v.vc_IndustryID= ind.lm_Id
                join listsmaster fn on v.vc_FunctionID= fn.lm_Id
                join ListsMaster ex on v.vc_ExperienceID= ex.lm_Id
                join ListsMaster ql on v.vc_QualificationID= ql.lm_Id
                join ListsMaster ag on v.vc_AgegroupID= ag.lm_Id
                join ListsMaster sal on v.vc_SalaryRangeID= sal.lm_Id
                join ListsMaster jl on v.vc_JobLocationID = jl.lm_Id
                join ListsMaster ct on v.vc_lm_JobTypeId = ct.lm_Id
                join companies co on co.co_id= v.vc_co_companyid
                where co.co_id={0} order by vc_Created desc", companyid);

                List<JobDetails> JobDetails = db.Database.SqlQuery<JobDetails>(sb.ToString()).ToList();
                List<JobDetails> JobDetails2 = new List<JobDetails>();
                JobDetails ljx;


                foreach (var lj in JobDetails)
                {
                    ljx = new JobDetails();

                    ljx.vc_Id = lj.vc_Id;
                    ljx.vc_Created = lj.vc_Created;
                    ljx.co_Name = lj.co_Name;
                    ljx.vc_Title = lj.vc_Title;
                    ljx.vc_Description = lj.vc_Description.Replace(System.Environment.NewLine, "<br>");
                    ljx.co_NameURL = urlcleaner(lj.co_Name);
                    ljx.vc_Created = lj.vc_Created;
                    ljx.vc_co_CompanyId = lj.vc_co_CompanyId;

                    extns = lj.co_LogoExtension.Trim() == "" ? "jpg" : lj.co_LogoExtension;
                    ljx.LogoURL = "~/Documents/Logos/" + lj.co_Guid.ToString().Substring(0, 2) + "/" + lj.co_Guid.ToString().Substring(2, 2) + "/" + lj.co_Guid.ToString().Substring(4, 2) + "/" + lj.co_Guid.ToString() + "-original" + extns;


                    int days = Convert.ToInt32((DateTime.Now - lj.vc_Created).TotalDays);

                    if (days == 0)
                    {
                        ljx.postedsince = "Today";
                    }
                    else if (days == 1)
                    {
                        ljx.postedsince = "" + days + " day ago";
                    }
                    else if (days > 1)
                    {
                        ljx.postedsince = "" + days + " days ago";
                    }



                    ljx.JobType = lj.JobType;
                    ljx.vc_lm_JobTypeId = lj.vc_lm_JobTypeId;
                    ljx.JobType = lj.JobType;
                    ljx.Industry = lj.Industry;
                    ljx.vc_IndustryID = lj.vc_IndustryID;
                    ljx.Functions = lj.Functions;
                    ljx.vc_FunctionID = lj.vc_FunctionID;
                    ljx.Experience = lj.Experience;
                    ljx.vc_ExperienceID = lj.vc_ExperienceID;
                    ljx.Qualification = lj.Qualification;
                    ljx.vc_QualificationID = lj.vc_QualificationID;
                    ljx.AgeGroup = lj.AgeGroup;
                    ljx.vc_AgegroupID = lj.vc_AgegroupID;
                    ljx.SalaryRange = lj.SalaryRange;
                    ljx.vc_SalaryRangeID = lj.vc_SalaryRangeID;
                    ljx.JobLocation = lj.JobLocation;
                    ljx.vc_JobLocationID = lj.vc_JobLocationID;
                    ljx.vc_CompanyDetails = lj.vc_CompanyDetails;
                    ljx.vc_JobRequirements = lj.vc_JobRequirements;
                    ljx.vc_ExpiryDate = lj.vc_ExpiryDate;
                    ljx.vc_Reference = lj.vc_Reference;
                    ljx.vc_ApplicationEmail = lj.vc_ApplicationEmail;
                    ljx.vc_Disabled = lj.vc_Disabled;
                    ljx.vc_st_StatusID = lj.vc_st_StatusID;
                    JobDetails2.Add(ljx);

                }

                ViewBag.JobDetails = JobDetails2.ToList();





            }
            return View();
        }

        [HttpPost]
        public ActionResult DeleteVacancy(FormCollection col)
        {

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("UPDATE vacancies set vc_deleted=getdate() where vc_id=" + col["vc_id"]);
            }


            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }



        public ActionResult PublishVacancyy(int jobid)
        {

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string sql = "UPDATE vacancies set vc_st_StatusID=1, vc_deleted=null,vc_Created=getdate(),vc_LastUpdated=getdate(), vc_ClosingDate=DATEADD(month, 1, getdate()),vc_ExpiryDate=DATEADD(month, 1, getdate()) where vc_id=" + jobid + " and vc_co_CompanyId=" + Convert.ToInt32(Session["CompanyID"]);
                db.Database.ExecuteSqlCommand(sql);
            }

            dbOperations dbo = new dbOperations();
            dbo.CS_CreditUsage(Convert.ToInt32(Session["CompanyID"]), Convert.ToInt32(Session["RecruiterID"]), 1, jobid, null);



            //send mail
            string bodycontent;
            bodycontent = "<font face='Arial' size='2'>";
            bodycontent += "<p>Job has been successfully posted on Jobs4Bahrainis Website by <b>" + Session["ContactPerson"] + "<b></p>";
            bodycontent += "</font>";


            //  cmn.SendMail(Convert.ToString(col["applicationemailid"]), "Bayden.tierney@jobs4bahrainis.com", "Job Posted on J4B Website", bodycontent);




            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
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

                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_Deleted is null order by lm_Ordinal").ToList();

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
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30,32,11,6,34) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Sectors1 = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Agegroup = ddLists.Where(x => x.lm_lt_ListTypeId == 32).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.notice_period = ddLists.Where(x => x.lm_lt_ListTypeId == 11).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.employment_type = ddLists.Where(x => x.lm_lt_ListTypeId == 6).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.last_login = ddLists.Where(x => x.lm_lt_ListTypeId == 34).OrderBy(x => x.lm_Ordinal).ToList();

                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();

                if (recruiter[0].ct_SearchCV == false)
                {
                    return RedirectToAction("NoPermission");
                }

                int companyid = recruiter[0].ct_co_CompanyId;

                dbOperations dbo = new dbOperations();

                if (!dbo.CS_CreditCheck(Convert.ToInt32(companyid), 2))
                {
                    return RedirectToAction("NoValidSubscription");
                }

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;
                if (company[0].co_HasLogo == true)
                {
                    extns = company[0].co_LogoExtension.Trim() == "" ? ".jpg" : company[0].co_LogoExtension;
                    //"http://www.jobs4bahrainis.com/Logox/"
                    ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;
                }
                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;

                //ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4  and lm_Deleted is null order by lm_Ordinal").ToList();

                
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

        public ActionResult CandidateSearchRefine()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30,32,11,6,34) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Sectors1 = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Agegroup = ddLists.Where(x => x.lm_lt_ListTypeId == 32).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.notice_period = ddLists.Where(x => x.lm_lt_ListTypeId == 11).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.employment_type = ddLists.Where(x => x.lm_lt_ListTypeId == 6).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.last_login = ddLists.Where(x => x.lm_lt_ListTypeId == 34).OrderBy(x => x.lm_Ordinal).ToList();

                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;
                if (company[0].co_HasLogo == true)
                {
                    extns = company[0].co_LogoExtension.Trim() == "" ? ".jpg" : company[0].co_LogoExtension;
                    //"http://www.jobs4bahrainis.com/Logox/"
                    ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;
                }
                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;

                //ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4  and lm_Deleted is null order by lm_Ordinal").ToList();

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

        public ActionResult CandidateSearch3()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (4,12,17,13,18,22,19,20,21,25,5,27,23,24,26,28,29,30,32) and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Country = ddLists.Where(x => x.lm_lt_ListTypeId == 13).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.EducationLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 12).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Sectors1 = ddLists.Where(x => x.lm_lt_ListTypeId == 4).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Agegroup = ddLists.Where(x => x.lm_lt_ListTypeId == 32).OrderBy(x => x.lm_Value).ToList();


                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;
                if (company[0].co_HasLogo == true)
                {
                    extns = company[0].co_LogoExtension.Trim() == "" ? ".jpg" : company[0].co_LogoExtension;
                    //"http://www.jobs4bahrainis.com/Logox/"
                    ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;
                }
                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;

                //ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();
                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4  and lm_Deleted is null order by lm_Ordinal").ToList();

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
                    //whereCand += whereUsed ? "and ca_Age = @p_age " : @"where ca_Age = @p_age ";
                    whereCand += whereUsed ? "and dbo.fun_GetAge(ca_DateOfBirth) = @p_age " : @"where dbo.fun_GetAge(ca_DateOfBirth) = @p_age ";
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
                sqlQry = string.Format(@"select CA.*,CE.TotalExp,LM.EducationLevel,LM2.CurrentLocation,LM3.CurrentJobTitle ,CH.CurrentEmployer
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
                                    ca_CurrentCountryID,
                                    ca_CurrentJobTitleID,
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
			                select lm_Id,lm_Value as CurrentLocation 
			                from ListsMaster 
			                where lm_lt_ListTypeId = 13
			                ) LM2
			                ON CA.ca_CurrentCountryID = LM2.lm_Id
                            left outer join
			                (
			                select lm_Id,lm_Value as CurrentJobTitle 
			                from ListsMaster 
			                where lm_lt_ListTypeId = 22
			                ) LM3
			                ON CA.ca_CurrentJobTitleID = LM3.lm_Id
                            left outer join
			                (
			          
                            SELECT TOP 1 *, Company as CurrentEmployer 
                              FROM CandidateWorkHistory_T
                            order by CaWorkHistory_ID desc
			                 ) CH
			                ON CA.Ca_ID = CH.Ca_ID 
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
        //public ActionResult CandidateSearchResult2(string keyword, string fname, string lname, int? age, int? vc_AgeGroupID, string gender, string[] chksector, int? qualification, string salary, string experience, int? ca_lm_CountryId, int? acclanguage, string employment_status, string notice_period, int? wh_jobtitle, int? ca_TotalRelavantExperience, int? wh_level, int? eduSchool, int? ca_lm_EducationLevel, int? eduDegree, int? eduField, int? skill_add, int? ic_currentjobtitle, int? icm_industry, int? icm_salary, string employment_type, string last_login, int? page)
        //{
        //    if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

        //    int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
        //    using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
        //    {
        //        var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
        //        //  int companyid = recruiter[0].ct_co_CompanyId;

        //        // var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

        //        //   string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
        //        //  ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

        //        // ViewBag.Company = company[0].co_Name;
        //        ViewBag.Name = recruiter[0].ct_Forename;
        //        ViewBag.Email = recruiter[0].ct_EmailAddress;
        //        ViewBag.Phone = recruiter[0].ct_Telephone;
        //    }
        //    DateTime last_login_date = DateTime.Now;


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
        //        last_login = string.IsNullOrEmpty(last_login) ? null : last_login;
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
        //            whereCand += whereUsed ? "and ca_Surname = ltrim(rtrim(@p_surname)) " : @"where ca_Surname = ltrim(rtrim(@p_surname)) ";
        //            whereUsed = true;
        //        }

        //        if (!string.IsNullOrEmpty(gender))
        //        {
        //            whereCand += whereUsed ? "and ca_Gender = ltrim(rtrim(@p_Gender)) " : @"where ca_Gender = ltrim(rtrim(@p_Gender)) ";
        //            whereUsed = true;
        //        }

        //        if (age != null)
        //        {
        //            //whereCand += whereUsed ? "and ca_Gender = @p_age " : @"where ca_Gender = @p_age ";
        //            //whereCand += whereUsed ? "and ca_Age = @p_age " : @"where ca_Age = @p_age ";
        //            whereCand += whereUsed ? "and dbo.fun_GetAge(ca_DateOfBirth) = @p_age " : @"where dbo.fun_GetAge(ca_DateOfBirth) = @p_age ";
        //            whereUsed = true;
        //        }

        //        if (qualification != null)
        //        {
        //            whereCand += whereUsed ? "and ca_lm_EducationLevel = @p_qaulification " : @"where ca_lm_EducationLevel = @p_qaulification ";
        //            whereUsed = true;
        //        }

        //        if (salaryFrom != null && salaryTo != null)
        //        {
        //            if (salaryFrom != -1)
        //            {
        //                whereCand += whereUsed ? "and ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)) " : @"where ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)) ";
        //            }
        //            else
        //            {
        //                whereCand += whereUsed ? "and ca_SalaryFrom >= @p_salaryto " : @"where ca_SalaryFrom >= @p_salaryto ";

        //            }
        //            whereUsed = true;
        //        }

        //        if (!string.IsNullOrEmpty(keyword))
        //        {
        //            //whereCand += whereUsed ? "and ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%') " : @"where ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%') ";

        //            //whereUsed = true;
        //        }
        //        if (!string.IsNullOrEmpty(keyword))
        //        {
        //            whereCand += whereUsed ? "or ca_FirstName = ltrim(rtrim(@p_keyword)) " : @"where ca_FirstName = ltrim(rtrim(@p_keyword)) and  ca_Active>0 ";

        //            whereUsed = true;
        //        }
        //        if (!string.IsNullOrEmpty(last_login))
        //        {
        //            if (last_login == "Today")
        //            {
        //                last_login_date = DateTime.Now.Date;
        //            }
        //            else if (last_login == "1 Week")
        //            {

        //            }
        //            else if (last_login == "2 Weeks")
        //            {

        //            }
        //            else if (last_login == "1 Month")
        //            {

        //            }
        //            else if (last_login == "3 Months")
        //            {

        //            }
        //            else if (last_login == "6 Months")
        //            {

        //            }
        //            else if (last_login == "1 Year")
        //            {

        //            }
        //            else if (last_login == "2 Years")
        //            {

        //            }
        //            whereCand += whereUsed ? "and ca_LastLogin = @p_LastLogin" : @"where ca_LastLogin = @p_LastLogin ";

        //            whereUsed = true;
        //        }
        //        // ca_lm_CountryId where
        //        if (ca_lm_CountryId != null)
        //        {
        //            whereCand += whereUsed ? "and ca_CurrentCountryID = @p_CurrentCountryID " : @"where ca_CurrentCountryID = @p_CurrentCountryID ";
        //            whereUsed = true;
        //        }
        //        if (wh_jobtitle != null)
        //        {
        //            whereCand += whereUsed ? "and ca_CurrentJobTitleID = @p_CurrentJobTitleID " : @"where ca_CurrentJobTitleID = @p_CurrentJobTitleID ";
        //            whereUsed = true;
        //        }
        //        if (ca_TotalRelavantExperience != null)
        //        {
        //            whereCand += whereUsed ? "and ca_TotalRelavantExperience = @p_TotalRelavantExperience " : @"where ca_TotalRelavantExperience = @p_TotalRelavantExperience ";
        //            whereUsed = true;
        //        }
        //        if (wh_level != null)
        //        {

        //            whereCand += whereUsed ? "and ca_Id in (select Ca_ID from CandidateWorkHistory_T where c.ca_Id = Ca_ID and CurrentlyWorkingHere=1 and  JobLevelID =@p_JobLevelID) " : @"where ca_Id in (select Ca_ID from CandidateWorkHistory_T where c.ca_Id = Ca_ID and CurrentlyWorkingHere=1 and  JobLevelID =@p_JobLevelID) ";

        //            whereUsed = true;
        //        }


        //        if (qualification != null)
        //        {
        //            whereCand += whereUsed ? "and ca_lm_EducationLevel = @p_qaulification " : @"where ca_lm_EducationLevel = @p_qaulification ";
        //            whereUsed = true;
        //        }
        //        if (expFrom != null && expTo != null)
        //        {
        //            if (expFrom != -1)
        //            {
        //                whereExp += whereExpUsed ? "and  (CE.TotalExp between  @p_experienceFrom and @p_experienceTo) " : @"where (CE.TotalExp between  @p_experienceFrom and @p_experienceTo) ";
        //            }
        //            else
        //            {
        //                whereExp += whereExpUsed ? "and CE.TotalExp >= @p_experienceTo " : @"where CE.TotalExp >= @p_experienceTo ";

        //            }
        //            whereExpUsed = true;
        //        }

        //        if (!string.IsNullOrEmpty(sectors))
        //        {
        //            whereCand += whereUsed ? string.Format("and ca_id in ( select ci_ca_CandidateId from CandidateInterests where ci_lm_LinkId in ({0})) ", sectors) : string.Format(@"where ca_id in ( select ci_ca_CandidateId from CandidateInterests where ci_lm_LinkId in ({0})) ", sectors);
        //            //whereCand += whereUsed ? string.Format("and ca_id in ( select Ca_ID from CandidateExperience where industry in ({0})) ", sectors) : string.Format(@"where ca_id in ( select Ca_ID from CandidateExperience where industry in  ({0})) ", sectors);
        //            whereUsed = true;
        //        }
        //        bool activeOnly = true;
        //        //display only active profiles 
        //        if (activeOnly == true)
        //        {
        //            whereCand += whereUsed ? "and  ca_Active>0 " : @"where  ca_Active>0 ";
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
        //        SqlParameter LastLoginParam = AddParameter("p_LastLogin", last_login_date, typeof(DateTime));
        //        //new para
        //        //CurrentCountryIDParam para
        //        SqlParameter CurrentCountryIDParam = AddParameter("p_CurrentCountryID", ca_lm_CountryId, typeof(Int32));
        //        SqlParameter CurrentJobTitleIDParam = AddParameter("p_CurrentJobTitleID", wh_jobtitle, typeof(Int32));
        //        SqlParameter TotalRelavantExperienceParam = AddParameter("p_TotalRelavantExperience ", ca_TotalRelavantExperience, typeof(Int32));
        //        SqlParameter JobLevelIDParam = AddParameter("p_JobLevelID ", wh_level, typeof(Int32));

        //        SqlParameter searchTypeParam = AddParameter("p_searchType", 1, typeof(Int32));
        //        SqlParameter pageSizeParam = AddParameter("p_pageSize", pageSize, typeof(Int32));
        //        SqlParameter pageIndexParam = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

        //        object[] parameters = new object[] { fNameParam, lNameParam, ageParam, genderParam, qualififcationParam
        //                                            , salaryFromParam, salaryToParam, expFromParam,expToParam
        //                                            ,keywordParam,
        //                                            LastLoginParam,CurrentCountryIDParam,CurrentJobTitleIDParam,TotalRelavantExperienceParam,JobLevelIDParam,
        //                                        searchTypeParam,pageSizeParam,pageIndexParam,sectorParam};
        //        string sqlQry = @"proc_CandidateSearch @p_firstname,@p_surname,@p_age,@p_Gender,@p_qaulification
        //                          ,@p_salaryfrom,@p_salaryto,@p_experienceFrom,@p_experienceTo,@p_keyword,
        //                            @p_LastLogin,@p_CurrentCountryID,p_CurrentJobTitleID,p_TotalRelavantExperience,p_JobLevelID,
        //                            @p_searchType,
        //                          @p_pageSize,@p_pageIndex,@p_sector";
        //        sqlQry = string.Format(@"select CA.*,CE.TotalExp,LM.EducationLevel,LM2.CurrentLocation,LM3.CurrentJobTitle ,CH.CurrentEmployer,CH.Salary,CM.ExpectedSalary,LM4.TotalExperience
        //           from
        //           (
        //           select  ca_FirstName + ' ' + ca_Surname ca_FullName ,
        //             ca_EmailAddress,
        //             ca_PhoneMobile,
        //             case ca_Gender
        //             when 'M' then 'Male'
        //             when 'F' then 'Female'
        //             else '' end as ca_Gender,
        //             ca_DateOfBirth,
        //             ca_lm_EducationLevel,
        //                            ca_CurrentCountryID,
        //                            ca_CurrentJobTitleID,
        //             ca_SalaryFrom,
        //             ca_SalaryTo,
        //             ca_Id,
        //             dbo.fun_GetAge(ca_DateOfBirth) ca_Age,
        //             ca_Profile,
        //             dbo.fun_GetProfilePic(ca_GUID,ca_PhotoExtension) LogoURL,
        //                            ca_Active,
        //                            ca_LastLogin,
        //                               ca_TotalRelavantExperience



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
        //                    left outer join
        //           (
        //           select lm_Id,lm_Value as CurrentLocation 
        //           from ListsMaster 
        //           where lm_lt_ListTypeId = 13
        //           ) LM2
        //           ON CA.ca_CurrentCountryID = LM2.lm_Id
        //                    left outer join
        //           (
        //           select lm_Id,lm_Value as CurrentJobTitle 
        //           from ListsMaster 
        //           where lm_lt_ListTypeId = 22
        //           ) LM3
        //           ON CA.ca_CurrentJobTitleID = LM3.lm_Id
        //                 left outer join
        //           (
        //           select lm_Id,REPLACE(lm_Value,'years','') as TotalExperience 
        //           from ListsMaster 
        //           where lm_lt_ListTypeId = 28
        //           ) LM4
        //           ON CA.ca_TotalRelavantExperience = LM4.lm_Id
        //                    left outer join
        //           (

        //                   select cwh.CaWorkHistory_ID,cwh.Ca_ID,lmjt.lm_Value as JobTitle, lmjl.lm_Value as JobLevel, cwh.Company as CurrentEmployer , lmrt.lm_Value as ReportingTo, lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction, 
        //        cwh.FromMonth,cwh.FromYear,cwh.ToMonth,cwh.ToYear,cwh.CurrentlyWorkingHere,lmjlo.lm_Value as JobLocation, lmsal.lm_Value as Salary,cwh.[Description] from [CandidateWorkHistory_T] cwh 
        //        join ListsMaster lmjt on lmjt.lm_Id = cwh.JobTitleID
        //        join ListsMaster lmjl on lmjl.lm_Id = cwh.JobLevelID
        //        join ListsMaster lmrt on lmrt.lm_Id = cwh.ReportingToID
        //        join ListsMaster lmind on lmind.lm_Id = cwh.IndustryID
        //        join ListsMaster lmfn on lmfn.lm_Id = cwh.FunctionID
        //        join ListsMaster lmjlo on lmjlo.lm_Id = cwh.JobLocationID
        //        join ListsMaster lmsal on lmsal.lm_Id = cwh.SalaryID
        //       where cwh.CurrentlyWorkingHere = 1
        //            ) CH
        //           ON CA.Ca_ID = CH.Ca_ID 
        //         left outer join
        //           (

        //                   select icm.CaICM_ID,icm.Ca_ID, lmjt.lm_Value as JobTitle,lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction,lmexp.lm_Value as Experience, lmsal.lm_Value as ExpectedSalary from [CandidateIdealCareerMove_T] icm
        //        join ListsMaster lmjt on lmjt.lm_Id = icm.JobTitleID
        //        join ListsMaster lmfn on lmfn.lm_Id = icm.FunctionID
        //        join ListsMaster lmind on lmind.lm_Id = icm.IndustryID
        //        join ListsMaster lmexp on lmexp.lm_Id = icm.ExperienceID
        //        join ListsMaster lmsal on lmsal.lm_Id = icm.ExpectedSalaryID

        //            ) CM
        //           ON CA.Ca_ID = CM.Ca_ID 
        //           left outer join
        //           (
        //           select Ca_ID, cast(isnull(sum(round(datediff(dd,fromdate,todate)/365.25,2)),0) as decimal(5,2))  TotalExp 
        //           from CandidateExperience
        //           group by Ca_ID 
        //           ) CE
        //           ON CA.Ca_ID = CE.Ca_ID
        //           {1} 
        //           order by Ca_ID
        //           OFFSET @p_pageSize  * (@p_pageIndex -1) rows fetch next @p_pageSize ROWS ONLY", whereCand, whereExp);
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

        //        //country id 
        //        SqlParameter CurrentCountryIDParam2 = AddParameter("p_CurrentCountryID", ca_lm_CountryId, typeof(Int32));
        //        SqlParameter CurrentJobTitleIDParam2 = AddParameter("p_CurrentJobTitleID", wh_jobtitle, typeof(Int32));
        //        SqlParameter TotalRelavantExperienceParam2 = AddParameter("p_TotalRelavantExperience ", ca_TotalRelavantExperience, typeof(Int32));
        //        SqlParameter JobLevelIDParam2 = AddParameter("p_JobLevelID ", wh_level, typeof(Int32));

        //        parameters = new object[] { fNameParam2, lNameParam2, ageParam2, genderParam2, qualififcationParam2
        //                                            , salaryFromParam2, salaryToParam2, expFromParam2,expToParam2
        //                                            ,keywordParam2,
        //                                            CurrentCountryIDParam2,CurrentJobTitleIDParam2,TotalRelavantExperienceParam2,JobLevelIDParam2,
        //                                            searchTypeParam2,pageSizeParam2,pageIndexParam2,sectorParam2};
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

        public ActionResult CandidateSearchResult2(string keyword, string gender, int? vc_AgeGroupID, int? ca_lm_CountryId, int?[] acclanguage, string notice_period, string employment_status,
           int? wh_jobtitle, string wh_company, int? ca_TotalRelavantExperience, int? wh_level, string[] chksector, string[] chksector2,

            int? eduSchool, int? ca_lm_EducationLevel, int? eduDegree, int? eduField,
            int?[] skill_add,
            int? ic_currentjobtitle, int? icm_industry, int? icm_salary, string employment_type, string last_login,

             int? page,

            string fname, string lname, int? age, int? qualification, string salary, string experience)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);

            dbOperations dbo = new dbOperations();
            dbo.ReportAdd(Convert.ToInt32(Session["CompanyID"]), Convert.ToInt32(Session["RecruiterID"]), 2);

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
            DateTime last_login_date = DateTime.Now;


            StringBuilder sb = new StringBuilder();

            string CanAgeGroup = null;
            if (vc_AgeGroupID == 3249)
            {
                CanAgeGroup = "0:20";
            }
            else if (vc_AgeGroupID == 3250)
            {
                CanAgeGroup = "21:25";
            }
            else if (vc_AgeGroupID == 3251)
            {
                CanAgeGroup = "26:30";
            }
            else if (vc_AgeGroupID == 3252)
            {
                CanAgeGroup = "31:35";
            }
            else if (vc_AgeGroupID == 3253)
            {
                CanAgeGroup = "36:40";
            }
            else if (vc_AgeGroupID == 3254)
            {
                CanAgeGroup = "41:45";
            }
            else if (vc_AgeGroupID == 3255)
            {
                CanAgeGroup = "46:50";
            }
            else if (vc_AgeGroupID == 3256)
            {
                CanAgeGroup = "51:55";
            }
            else if (vc_AgeGroupID == 3257)
            {
                CanAgeGroup = "56:60";
            }
            else if (vc_AgeGroupID == 3258)
            {
                CanAgeGroup = "61:100";
            }

            //if (vc_AgeGroupID == 5804)
            //{
            //    CanAgeGroup = "0:20";
            //}
            //else if (vc_AgeGroupID == 5805)
            //{
            //    CanAgeGroup = "21:25";
            //}
            //else if (vc_AgeGroupID == 5806)
            //{
            //    CanAgeGroup = "26:30";
            //}
            //else if (vc_AgeGroupID == 5807)
            //{
            //    CanAgeGroup = "31:35";
            //}
            //else if (vc_AgeGroupID == 5808)
            //{
            //    CanAgeGroup = "36:40";
            //}
            //else if (vc_AgeGroupID == 5809)
            //{
            //    CanAgeGroup = "41:45";
            //}
            //else if (vc_AgeGroupID == 5810)
            //{
            //    CanAgeGroup = "46:50";
            //}
            //else if (vc_AgeGroupID == 5811)
            //{
            //    CanAgeGroup = "51:55";
            //}
            //else if (vc_AgeGroupID == 5812)
            //{
            //    CanAgeGroup = "56:60";
            //}
            //else if (vc_AgeGroupID == 5813)
            //{
            //    CanAgeGroup = "61:100";
            //}


            int? ageFrom = null, ageTo = null;
            var spltAge = CanAgeGroup != null ? CanAgeGroup.Split(':') : null;
            if (spltAge != null && spltAge.Count() == 2)
            {
                ageFrom = int.Parse(spltAge[0]);
                ageTo = int.Parse(spltAge[1]);
            }

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
            string sectors2 = null;
            if (chksector2 != null)
            {
                foreach (var d in chksector2)
                {
                    sectors2 += d + ",";
                }
                sectors2 = sectors2.Substring(0, sectors2.Length - 1);
            }
            string skillsSet = null;
            if (skill_add != null)
            {
                foreach (var d in skill_add)
                {
                    skillsSet += d + ",";
                }
                skillsSet = skillsSet.Substring(0, skillsSet.Length - 1);
            }
            string langSet = null;
            if (acclanguage != null)
            {
                foreach (var d in acclanguage)
                {
                    langSet += d + ",";
                }
                langSet = langSet.Substring(0, langSet.Length - 1);
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
                last_login = string.IsNullOrEmpty(last_login) ? null : last_login;
                sectors = string.IsNullOrEmpty(sectors) ? null : sectors;
                sectors2 = string.IsNullOrEmpty(sectors2) ? null : sectors2;
                sb.Append(@"");
                bool whereUsed = false;
                bool whereExpUsed = false;
                string whereCand = "";
                string whereExp = "";



                if (!string.IsNullOrEmpty(keyword))
                {

                    //whereCand = @"where ca_id  
                    //                              IN (
                    //      SELECT [ca_id] 
                    //       FROM   candidates 
                    //       WHERE  ca_currentjobtitleid = 
                    //              (SELECT [lm_id] 
                    //               FROM   [listsmaster] 
                    //               WHERE  ( lm_value LIKE 
                    //                        '%' + @p_keyword + '%' 
                    //                       )
                    //               )                
                    //                                   ) ";
                    whereCand = @"where ca_id  
                                                  IN (
                          SELECT [ca_id] 
                           FROM   candidates 
                           WHERE  ca_currentjobtitleid = 
                                  (SELECT [lm_id] 
                                   FROM   [listsmaster] 
                                   WHERE  ( lm_value = 
                                            @p_keyword and lm_lt_ListTypeId=22
                                           )
                                   )                
                                                       ) ";


                    whereUsed = true;
                }
                if (!string.IsNullOrEmpty(gender))
                {
                    whereCand += whereUsed ? "and ca_Gender = ltrim(rtrim(@p_Gender)) " : @"where ca_Gender = ltrim(rtrim(@p_Gender)) ";
                    whereUsed = true;
                }

                //if (age != null)
                //{
                //    //whereCand += whereUsed ? "and ca_Gender = @p_age " : @"where ca_Gender = @p_age ";
                //    //whereCand += whereUsed ? "and ca_Age = @p_age " : @"where ca_Age = @p_age ";
                //    whereCand += whereUsed ? "and dbo.fun_GetAge(ca_DateOfBirth) = @p_age " : @"where dbo.fun_GetAge(ca_DateOfBirth) = @p_age ";
                //    whereUsed = true;
                //}
                if (ageFrom != null && ageTo != null)
                {
                    whereCand += whereUsed ? "and (dbo.fun_GetAge(ca_DateOfBirth) between @p_ageFrom and  @p_ageTo) " : @"where (dbo.fun_GetAge(ca_DateOfBirth) between @p_ageFrom and  @p_ageTo) ";
                    //if (ageFrom != -1)
                    //{
                    //    whereCand += whereUsed ? "and ((dbo.fun_GetAge(ca_DateOfBirth) >= @p_ageFrom and dbo.fun_GetAge(ca_DateOfBirth) <= @p_ageTo) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)) " : @"where ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)) ";
                    //}
                    //else
                    //{
                    //    whereCand += whereUsed ? "and ca_SalaryFrom >= @p_salaryto " : @"where ca_SalaryFrom >= @p_salaryto ";

                    //}
                    whereUsed = true;
                }

                // ca_lm_CountryId where
                if (ca_lm_CountryId != null)
                {
                    whereCand += whereUsed ? "and ca_CurrentCountryID = @p_CurrentCountryID " : @"where ca_CurrentCountryID = @p_CurrentCountryID ";
                    whereUsed = true;
                }
                if (!string.IsNullOrEmpty(langSet))
                {
                    whereCand += whereUsed ? string.Format("and ca_id in ( select ca_id from CandidateLanguage_T where LanguageID in ({0})) ", langSet) : string.Format(@"where ca_id in ( select ca_id from CandidateLanguage_T where LanguageID in ({0})) ", langSet);
                    //whereCand += whereUsed ? string.Format("and ca_id in ( select Ca_ID from CandidateExperience where industry in ({0})) ", sectors) : string.Format(@"where ca_id in ( select Ca_ID from CandidateExperience where industry in  ({0})) ", sectors);
                    whereUsed = true;
                }
                if (!string.IsNullOrEmpty(employment_status))
                {
                    whereCand += whereUsed ? "and ca_IsFresher = @p_ca_IsFresher " : @"where ca_IsFresher = @p_ca_IsFresher ";
                    whereUsed = true;
                }
                if (wh_jobtitle != null)
                {
                    whereCand += whereUsed ? "and ca_CurrentJobTitleID = @p_CurrentJobTitleID " : @"where ca_CurrentJobTitleID = @p_CurrentJobTitleID ";
                    whereUsed = true;
                }
                if (!string.IsNullOrEmpty(wh_company))
                {
                    whereCand += whereUsed ? "and ca_id IN (SELECT ca_id FROM CandidateWorkHistory_T WHERE  Company  LIKE '%' +  @p_Company + '%'  ) " : @"where ca_id IN (SELECT ca_id FROM CandidateWorkHistory_T WHERE  Company  LIKE '%' +  @p_Company + '%'  ) ";
                    whereUsed = true;
                }
                if (wh_level != null)
                {
                    whereCand += whereUsed ? "and ca_id IN (SELECT ca_id FROM CandidateWorkHistory_T WHERE  JobLevelID =@p_JobLevelID and CurrentlyWorkingHere=1  ) " : @"where ca_id IN (SELECT ca_id FROM CandidateWorkHistory_T WHERE  JobLevelID  =@p_JobLevelID and CurrentlyWorkingHere=1) ";
                    whereUsed = true;
                }

                if (!string.IsNullOrEmpty(sectors))
                {
                    whereCand += whereUsed ? string.Format("and ca_id in ( select ca_id from CandidateWorkHistory_T where IndustryID in ({0})) ", sectors) : string.Format(@"where ca_id in ( select ca_id from CandidateWorkHistory_T where IndustryID in ({0})) ", sectors);
                    //whereCand += whereUsed ? string.Format("and ca_id in ( select Ca_ID from CandidateExperience where industry in ({0})) ", sectors) : string.Format(@"where ca_id in ( select Ca_ID from CandidateExperience where industry in  ({0})) ", sectors);
                    whereUsed = true;
                }
                if (!string.IsNullOrEmpty(sectors2))
                {
                    whereCand += whereUsed ? string.Format("and ca_id in ( select ca_id from CandidateWorkHistory_T where FunctionID in ({0})) ", sectors2) : string.Format(@"where ca_id in ( select ca_id from CandidateWorkHistory_T where FunctionID in ({0})) ", sectors2);
                    //whereCand += whereUsed ? string.Format("and ca_id in ( select Ca_ID from CandidateExperience where industry in ({0})) ", sectors) : string.Format(@"where ca_id in ( select Ca_ID from CandidateExperience where industry in  ({0})) ", sectors);
                    whereUsed = true;
                }
                if (eduSchool != null)
                {
                    whereCand += whereUsed ? "and ca_id IN (SELECT ca_id FROM CandidateEducation_T WHERE  UniversityID =@p_UniversityID ) " : @"where ca_id IN (SELECT ca_id FROM CandidateEducation_T WHERE  UniversityID =@p_UniversityID) ";
                    whereUsed = true;
                }
                if (ca_lm_EducationLevel != null)
                {
                    whereCand += whereUsed ? "and ca_lm_EducationLevel = @p_ca_lm_EducationLevel " : @"where ca_lm_EducationLevel = @p_ca_lm_EducationLevel ";
                    whereUsed = true;
                }
                if (eduDegree != null)
                {
                    whereCand += whereUsed ? "and ca_id IN (SELECT ca_id FROM CandidateEducation_T WHERE  DegreeID =@p_DegreeID ) " : @"where ca_id IN (SELECT ca_id FROM CandidateEducation_T WHERE  DegreeID =@p_DegreeID) ";
                    whereUsed = true;
                }
                if (eduField != null)
                {
                    whereCand += whereUsed ? "and ca_id IN (SELECT ca_id FROM CandidateEducation_T WHERE  SpecializationID =@p_SpecializationID ) " : @"where ca_id IN (SELECT ca_id FROM CandidateEducation_T WHERE  SpecializationID =@p_SpecializationID) ";
                    whereUsed = true;
                }


                if (!string.IsNullOrEmpty(skillsSet))
                {
                    whereCand += whereUsed ? string.Format("and ca_id in ( select ca_id from CandidateSkill_T where SkillID in ({0})) ", skillsSet) : string.Format(@"where ca_id in ( select ca_id from CandidateSkill_T where SkillID in ({0})) ", skillsSet);
                    //whereCand += whereUsed ? string.Format("and ca_id in ( select Ca_ID from CandidateExperience where industry in ({0})) ", sectors) : string.Format(@"where ca_id in ( select Ca_ID from CandidateExperience where industry in  ({0})) ", sectors);
                    whereUsed = true;
                }

                if (ic_currentjobtitle != null)
                {
                    whereCand += whereUsed ? "and ca_id IN (SELECT ca_id FROM CandidateIdealCareerMove_T WHERE  JobTitleID =@p_JobTitleID ) " : @"where ca_id IN (SELECT ca_id FROM CandidateIdealCareerMove_T WHERE  JobTitleID =@p_JobTitleID) ";
                    whereUsed = true;
                }
                if (icm_industry != null)
                {
                    whereCand += whereUsed ? "and ca_id IN (SELECT ca_id FROM CandidateIdealCareerMove_T WHERE  IndustryID =@p_IndustryID ) " : @"where ca_id IN (SELECT ca_id FROM CandidateIdealCareerMove_T WHERE  IndustryID =@p_IndustryID ) ";
                    whereUsed = true;
                }
                if (icm_salary != null)
                {
                    whereCand += whereUsed ? "and ca_id IN (SELECT ca_id FROM CandidateIdealCareerMove_T WHERE  ExpectedSalaryID =@p_ExpectedSalaryID ) " : @"where ca_id IN (SELECT ca_id FROM CandidateIdealCareerMove_T WHERE  ExpectedSalaryID =@p_ExpectedSalaryID) ";
                    whereUsed = true;
                }
                //if (ageFrom != null && ageTo != null)
                //{
                //    if (ageFrom != -1)
                //    {
                //        whereExp += whereExpUsed ? "and  (CE.TotalExp between  @p_experienceFrom and @p_experienceTo) " : @"where (CE.TotalExp between  @p_experienceFrom and @p_experienceTo) ";
                //    }
                //    else
                //    {
                //        whereExp += whereExpUsed ? "and CE.TotalExp >= @p_experienceTo " : @"where CE.TotalExp >= @p_experienceTo ";

                //    }
                //    whereExpUsed = true;
                //}

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
                    //whereCand += whereUsed ? "and ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%') " : @"where ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%') ";

                    //whereUsed = true;
                }
                //if (!string.IsNullOrEmpty(keyword))
                //{
                //    whereCand += whereUsed ? "or ca_FirstName = ltrim(rtrim(@p_keyword)) " : @"where ca_FirstName = ltrim(rtrim(@p_keyword)) and  ca_Active>0 ";

                //    whereUsed = true;
                //}
                if (!string.IsNullOrEmpty(last_login))
                {
                    if (last_login == "Today")
                    {
                        last_login_date = DateTime.Now.Date;
                    }
                    else if (last_login == "1 Week")
                    {

                    }
                    else if (last_login == "2 Weeks")
                    {

                    }
                    else if (last_login == "1 Month")
                    {

                    }
                    else if (last_login == "3 Months")
                    {

                    }
                    else if (last_login == "6 Months")
                    {

                    }
                    else if (last_login == "1 Year")
                    {

                    }
                    else if (last_login == "2 Years")
                    {

                    }
                    whereCand += whereUsed ? "and ca_LastLogin = @p_LastLogin" : @"where ca_LastLogin = @p_LastLogin ";

                    whereUsed = true;
                }


                if (ca_TotalRelavantExperience != null)
                {
                    whereCand += whereUsed ? "and ca_TotalRelavantExperience = @p_TotalRelavantExperience " : @"where ca_TotalRelavantExperience = @p_TotalRelavantExperience ";
                    whereUsed = true;
                }
                //if (wh_level != null)
                //{

                //    whereCand += whereUsed ? "and ca_Id in (select Ca_ID from CandidateWorkHistory_T where c.ca_Id = Ca_ID and CurrentlyWorkingHere=1 and  JobLevelID =@p_JobLevelID) " : @"where ca_Id in (select Ca_ID from CandidateWorkHistory_T where c.ca_Id = Ca_ID and CurrentlyWorkingHere=1 and  JobLevelID =@p_JobLevelID) ";

                //    whereUsed = true;
                //}


                if (qualification != null)
                {
                    whereCand += whereUsed ? "and ca_lm_EducationLevel = @p_qaulification " : @"where ca_lm_EducationLevel = @p_qaulification ";
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

                //if (!string.IsNullOrEmpty(sectors))
                //{
                //    whereCand += whereUsed ? string.Format("and ca_id in ( select ci_ca_CandidateId from CandidateInterests where ci_lm_LinkId in ({0})) ", sectors) : string.Format(@"where ca_id in ( select ci_ca_CandidateId from CandidateInterests where ci_lm_LinkId in ({0})) ", sectors);
                //    //whereCand += whereUsed ? string.Format("and ca_id in ( select Ca_ID from CandidateExperience where industry in ({0})) ", sectors) : string.Format(@"where ca_id in ( select Ca_ID from CandidateExperience where industry in  ({0})) ", sectors);
                //    whereUsed = true;
                //}

                bool activeOnly = true;
                //display only active profiles 
                if (activeOnly == true)
                {
                    whereCand += whereUsed ? "and  ca_Active>0 " : @"where  ca_Active>0 ";
                    whereUsed = true;
                }
                SqlParameter keywordParam = AddParameter("p_keyword", keyword, typeof(String));
                SqlParameter genderParam = AddParameter("p_Gender", gender, typeof(string));
                SqlParameter ageParam = AddParameter("p_age", age, typeof(Int32));
                SqlParameter ageFromParam = AddParameter("p_ageFrom", ageFrom, typeof(Int32));
                SqlParameter ageToParam = AddParameter("p_ageTo", ageTo, typeof(Int32));

                SqlParameter CurrentCountryIDParam = AddParameter("p_CurrentCountryID", ca_lm_CountryId, typeof(Int32));
                SqlParameter langSetParam = AddParameter("p_langSet", langSet, typeof(String));
                SqlParameter IsFresherParam = AddParameter("p_ca_IsFresher", employment_status, typeof(String));

                SqlParameter CurrentJobTitleIDParam = AddParameter("p_CurrentJobTitleID", wh_jobtitle, typeof(Int32));
                SqlParameter companyParam = AddParameter("p_Company", wh_company, typeof(String));
                SqlParameter JobLevelIDParam = AddParameter("p_JobLevelID", wh_level, typeof(Int32));
                SqlParameter sectorParam = AddParameter("p_sector", sectors, typeof(String));
                SqlParameter sector2Param = AddParameter("p_sector2", sectors2, typeof(String));
                SqlParameter UniversityIDParam = AddParameter("p_UniversityID", eduSchool, typeof(Int32));
                SqlParameter ca_lm_EducationLevelParam = AddParameter("p_ca_lm_EducationLevel", ca_lm_EducationLevel, typeof(Int32));
                SqlParameter DegreeIDParam = AddParameter("p_DegreeID", eduDegree, typeof(Int32));
                SqlParameter SpecializationIDParam = AddParameter("p_SpecializationID ", eduField, typeof(Int32));
                SqlParameter skillsSetParam = AddParameter("p_skillsSet", skillsSet, typeof(String));


                SqlParameter JobTitleIDParam = AddParameter("p_JobTitleID ", ic_currentjobtitle, typeof(Int32));
                SqlParameter IndustryIDParam = AddParameter("p_IndustryID ", icm_industry, typeof(Int32));
                SqlParameter ExpectedSalaryIDParam = AddParameter("p_ExpectedSalaryID ", icm_salary, typeof(Int32));


                SqlParameter fNameParam = AddParameter("p_firstname", fname, typeof(string));
                SqlParameter lNameParam = AddParameter("p_surname", lname, typeof(string));


                SqlParameter qualififcationParam = AddParameter("p_qaulification", qualification, typeof(Int32));
                SqlParameter salaryFromParam = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
                SqlParameter salaryToParam = AddParameter("p_salaryto", salaryTo, typeof(decimal));
                SqlParameter expFromParam = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
                SqlParameter expToParam = AddParameter("p_experienceTo", expTo, typeof(Int32));


                SqlParameter LastLoginParam = AddParameter("p_LastLogin", last_login_date, typeof(DateTime));
                //new para
                //CurrentCountryIDParam para


                SqlParameter TotalRelavantExperienceParam = AddParameter("p_TotalRelavantExperience ", ca_TotalRelavantExperience, typeof(Int32));
                //SqlParameter JobLevelIDParam = AddParameter("p_JobLevelID ", wh_level, typeof(Int32));

                SqlParameter searchTypeParam = AddParameter("p_searchType", 1, typeof(Int32));
                SqlParameter pageSizeParam = AddParameter("p_pageSize", pageSize, typeof(Int32));
                SqlParameter pageIndexParam = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

                object[] parameters = new object[] { fNameParam, lNameParam,   qualififcationParam
                                                    , salaryFromParam, salaryToParam, expFromParam,expToParam
                                                    ,keywordParam,genderParam,ageFromParam,ageToParam, ageParam,CurrentCountryIDParam,langSetParam,IsFresherParam,
                                                    CurrentJobTitleIDParam,companyParam,JobLevelIDParam,
                                                    sectorParam,sector2Param,
                                                    UniversityIDParam,ca_lm_EducationLevelParam,
                                                    DegreeIDParam,SpecializationIDParam,
                                                    skillsSetParam,
                                                    JobTitleIDParam,IndustryIDParam,ExpectedSalaryIDParam,
                                                    LastLoginParam,TotalRelavantExperienceParam,
                                                searchTypeParam,pageSizeParam,pageIndexParam,};
                string sqlQry = @"proc_CandidateSearch @p_firstname,@p_surname,@p_qaulification
                                  ,@p_salaryfrom,@p_salaryto,@p_experienceFrom,@p_experienceTo,
                                    @p_keyword,@p_Gender,@p_ageFrom,@p_ageTo,@p_age,p_CurrentCountryID,@p_langSet,@p_ca_IsFresher,
                                    @p_CurrentJobTitleID,@p_Company,@p_JobLevelID,
                                       @p_sector,@p_sector2,
                                    @p_UniversityID,@p_ca_lm_EducationLevel,
                                    @p_DegreeID,@p_SpecializationID,
                                        @p_skillsSet,
                                     @p_JobTitleID,@p_IndustryID,@p_ExpectedSalaryID,

                                    @p_LastLogin,p_TotalRelavantExperience,
                                    @p_searchType,
                                  @p_pageSize,@p_pageIndex";
                sqlQry = string.Format(@"select CA.*,CE.TotalExp,LM.EducationLevel,LM2.CurrentLocation,LM3.CurrentJobTitle ,CH.Company,CH.lm_value As Salary,CM.ExpectedSalary,LM4.TotalExperience
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
                                    ca_CurrentCountryID,
                                    ca_CurrentJobTitleID,
                     ca_SalaryFrom,
                     ca_SalaryTo,
                     ca_Id,
                     dbo.fun_GetAge(ca_DateOfBirth) ca_Age,
                     ca_Profile,
                     dbo.fun_GetProfilePic(ca_GUID,ca_PhotoExtension) LogoURL,
                                    ca_Active,
                                    ca_LastLogin,
                                       ca_TotalRelavantExperience



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
                   select lm_Id,lm_Value as CurrentLocation 
                   from ListsMaster 
                   where lm_lt_ListTypeId = 13
                   ) LM2
                   ON CA.ca_CurrentCountryID = LM2.lm_Id
                            left outer join
                   (
                   select lm_Id,lm_Value as CurrentJobTitle 
                   from ListsMaster 
                   where lm_lt_ListTypeId = 22
                   ) LM3
                   ON CA.ca_CurrentJobTitleID = LM3.lm_Id
                         left outer join
                   (
                   select lm_Id,REPLACE(lm_Value,'years','') as TotalExperience 
                   from ListsMaster 
                   where lm_lt_ListTypeId = 28
                   ) LM4
                   ON CA.ca_TotalRelavantExperience = LM4.lm_Id
                            left outer join
                   (
                         SELECT candidateworkhistory_t.*, 
                               listsmaster.lm_value 
                        FROM   candidateworkhistory_t 
                               JOIN listsmaster 
                                 ON listsmaster.lm_id = candidateworkhistory_t.salaryid 
                        WHERE  candidateworkhistory_t.currentlyworkinghere = 1
                    ) CH
                   ON CA.Ca_ID = CH.Ca_ID 
                 left outer join
                   (

                           select icm.CaICM_ID,icm.Ca_ID, lmjt.lm_Value as JobTitle,lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction,lmexp.lm_Value as Experience, lmsal.lm_Value as ExpectedSalary from [CandidateIdealCareerMove_T] icm
                join ListsMaster lmjt on lmjt.lm_Id = icm.JobTitleID
                join ListsMaster lmfn on lmfn.lm_Id = icm.FunctionID
                join ListsMaster lmind on lmind.lm_Id = icm.IndustryID
                join ListsMaster lmexp on lmexp.lm_Id = icm.ExperienceID
                join ListsMaster lmsal on lmsal.lm_Id = icm.ExpectedSalaryID

                    ) CM
                   ON CA.Ca_ID = CM.Ca_ID 
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
                SqlParameter keywordParam2 = AddParameter("p_keyword", keyword, typeof(String));
                SqlParameter genderParam2 = AddParameter("p_Gender", gender, typeof(string));
                SqlParameter ageParam2 = AddParameter("p_age", age, typeof(Int32));
                SqlParameter ageFromParam2 = AddParameter("p_ageFrom", ageFrom, typeof(Int32));
                SqlParameter ageToParam2 = AddParameter("p_ageTo", ageTo, typeof(Int32));

                SqlParameter CurrentCountryIDParam2 = AddParameter("p_CurrentCountryID", ca_lm_CountryId, typeof(Int32));
                SqlParameter langSetParam2 = AddParameter("p_langSet", langSet, typeof(String));
                SqlParameter IsFresherParam2 = AddParameter("p_ca_IsFresher", employment_status, typeof(String));
                SqlParameter CurrentJobTitleIDParam2 = AddParameter("p_CurrentJobTitleID", wh_jobtitle, typeof(Int32));
                SqlParameter companyParam2 = AddParameter("p_Company", wh_company, typeof(String));
                SqlParameter JobLevelIDParam2 = AddParameter("p_JobLevelID", wh_level, typeof(Int32));
                SqlParameter sectorParam2 = AddParameter("p_sector", sectors, typeof(String));
                SqlParameter sector2Param2 = AddParameter("p_sector2", sectors2, typeof(String));


                SqlParameter UniversityIDParam2 = AddParameter("p_UniversityID", eduSchool, typeof(Int32));
                SqlParameter ca_lm_EducationLevelParam2 = AddParameter("p_ca_lm_EducationLevel", ca_lm_EducationLevel, typeof(Int32));
                SqlParameter DegreeIDParam2 = AddParameter("p_DegreeID", eduDegree, typeof(Int32));
                SqlParameter SpecializationIDParam2 = AddParameter("p_SpecializationID ", eduField, typeof(Int32));
                SqlParameter skillsSetParam2 = AddParameter("p_skillsSet", skillsSet, typeof(String));

                SqlParameter JobTitleIDParam2 = AddParameter("p_JobTitleID ", ic_currentjobtitle, typeof(Int32));
                SqlParameter IndustryIDParam2 = AddParameter("p_IndustryID ", icm_industry, typeof(Int32));
                SqlParameter ExpectedSalaryIDParam2 = AddParameter("p_ExpectedSalaryID ", icm_salary, typeof(Int32));



                SqlParameter fNameParam2 = AddParameter("p_firstname", fname, typeof(string));
                SqlParameter lNameParam2 = AddParameter("p_surname", lname, typeof(string));


                SqlParameter qualififcationParam2 = AddParameter("p_qaulification", qualification, typeof(Int32));
                SqlParameter salaryFromParam2 = AddParameter("p_salaryfrom", salaryFrom, typeof(decimal));
                SqlParameter salaryToParam2 = AddParameter("p_salaryto", salaryTo, typeof(decimal));
                SqlParameter expFromParam2 = AddParameter("p_experienceFrom", expFrom, typeof(Int32));
                SqlParameter expToParam2 = AddParameter("p_experienceTo", expTo, typeof(Int32));

                SqlParameter searchTypeParam2 = AddParameter("p_searchType", 2, typeof(Int32));
                SqlParameter pageSizeParam2 = AddParameter("p_pageSize", pageSize, typeof(Int32));
                SqlParameter pageIndexParam2 = AddParameter("p_pageIndex", pageIndex, typeof(Int32));

                //country id 


                SqlParameter TotalRelavantExperienceParam2 = AddParameter("p_TotalRelavantExperience ", ca_TotalRelavantExperience, typeof(Int32));
                //SqlParameter JobLevelIDParam2 = AddParameter("p_JobLevelID ", wh_level, typeof(Int32));


                parameters = new object[] { fNameParam2, lNameParam2,   qualififcationParam2
                                                    , salaryFromParam2, salaryToParam2, expFromParam2,expToParam2
                                                    ,keywordParam2,genderParam2,ageFromParam2,ageToParam2, ageParam2,CurrentCountryIDParam2,langSetParam2,IsFresherParam2,

                                                    CurrentJobTitleIDParam2,companyParam2,JobLevelIDParam2,
                                                    sectorParam2,sector2Param2,
                                                    UniversityIDParam2,ca_lm_EducationLevelParam2,
                                                    DegreeIDParam2,SpecializationIDParam2,
                                                    skillsSetParam2,
                                                    JobTitleIDParam2,IndustryIDParam2,ExpectedSalaryIDParam2,
                                                    TotalRelavantExperienceParam2,
                                                    searchTypeParam2,pageSizeParam2,pageIndexParam2};
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
                ViewBag.totalSearchResult = totalSearchCount;

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

        //public ActionResult CandidateSearchResult2(string keyword, int? vc_AgeGroupID, string gender, string location, 
        //    string language, string notice_period, string employment_status, string[] chksector, int? qualification,
        //    string icm_salary, string wh_jobtitle, string wh_company, string wh_level, string eduSchool, string eduDegree, 
        //    string eduField, string ca_TotalRelavantExperience, string ic_currentjobtitle, string icm_industry, string employment_type, int? page)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    decimal? salaryFrom = null, salaryTo = null;
        //    var spltSalary = icm_salary != null ? icm_salary.Split(':') : null;
        //    if (spltSalary != null && spltSalary.Count() == 2)
        //    {
        //        salaryFrom = decimal.Parse(spltSalary[0]);
        //        salaryTo = decimal.Parse(spltSalary[1]);
        //    }

        //    decimal? expFrom = null, expTo = null;
        //    var spltExp = ca_TotalRelavantExperience != null ? ca_TotalRelavantExperience.Split(':') : null;
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

        //        //fname = string.IsNullOrEmpty(fname) ? null : fname;
        //        //lname = string.IsNullOrEmpty(lname) ? null : lname;
        //        gender = string.IsNullOrEmpty(gender) ? null : gender;
        //        keyword = string.IsNullOrEmpty(keyword) ? null : keyword;
        //        sectors = string.IsNullOrEmpty(sectors) ? null : sectors;
        //        sb.Append(@"");
        //        bool whereUsed = false;
        //        bool whereExpUsed = false;
        //        string whereCand = "";
        //        string whereExp = "";
        //        //if (!string.IsNullOrEmpty(fname))
        //        //{
        //        //    whereCand = @"where ca_FirstName = ltrim(rtrim(@p_firstname)) ";
        //        //    whereUsed = true;
        //        //}

        //        //if (!string.IsNullOrEmpty(lname))
        //        //{
        //        //    whereCand += whereUsed ? "and ca_Surname = ltrim(rtrim(@p_surname)) " : @"where ca_Surname = ltrim(rtrim(@p_surname)) ";
        //        //    whereUsed = true;
        //        //}

        //        if (!string.IsNullOrEmpty(gender))
        //        {
        //            whereCand += whereUsed ? "and ca_Gender = ltrim(rtrim(@p_Gender)) " : @"where ca_Gender = ltrim(rtrim(@p_Gender)) ";
        //            whereUsed = true;
        //        }

        //        if (vc_AgeGroupID != null)
        //        {
        //            whereCand += whereUsed ? "and ca_Gender = @p_age " : @"where ca_Gender = @p_age ";
        //            whereUsed = true;
        //        }

        //        if (qualification != null)
        //        {
        //            whereCand += whereUsed ? "and ca_lm_EducationLevel = @p_qaulification " : @"where ca_lm_EducationLevel = @p_qaulification ";
        //            whereUsed = true;
        //        }

        //        if (salaryFrom != null && salaryTo != null)
        //        {
        //            if (salaryFrom != -1)
        //            {
        //                whereCand += whereUsed ? "and ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)) " : @"where ((ca_SalaryFrom >= @p_salaryfrom and ca_SalaryFrom <= @p_salaryto) and (ca_SalaryTo >= @p_salaryfrom and ca_SalaryTo <= @p_salaryto)) ";
        //            }
        //            else
        //            {
        //                whereCand += whereUsed ? "and ca_SalaryFrom >= @p_salaryto " : @"where ca_SalaryFrom >= @p_salaryto ";

        //            }
        //            whereUsed = true;
        //        }

        //        if (!string.IsNullOrEmpty(keyword))
        //        {
        //            whereCand += whereUsed ? "and ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%') " : @"where ca_Id in (select ca_Id from CandidateExperience where c.ca_Id = Ca_ID and  JobTitle like '%' + @p_keyword + '%') ";
        //            whereUsed = true;
        //        }

        //        if (expFrom != null && expTo != null)
        //        {
        //            if (expFrom != -1)
        //            {
        //                whereExp += whereExpUsed ? "and  (CE.TotalExp between  @p_experienceFrom and @p_experienceTo) " : @"where (CE.TotalExp between  @p_experienceFrom and @p_experienceTo) ";
        //            }
        //            else
        //            {
        //                whereExp += whereExpUsed ? "and CE.TotalExp >= @p_experienceTo " : @"where CE.TotalExp >= @p_experienceTo ";

        //            }
        //            whereExpUsed = true;
        //        }

        //        if (!string.IsNullOrEmpty(sectors))
        //        {
        //            whereCand += whereUsed ? string.Format("and ca_id in ( select ci_ca_CandidateId from CandidateInterests where ci_lm_LinkId in ({0})) ", sectors) : string.Format(@"where ca_id in ( select ci_ca_CandidateId from CandidateInterests where ci_lm_LinkId in ({0})) ", sectors);
        //            whereUsed = true;
        //        }

        //        //SqlParameter fNameParam = AddParameter("p_firstname", fname, typeof(string));
        //        //SqlParameter lNameParam = AddParameter("p_surname", lname, typeof(string));
        //        SqlParameter ageParam = AddParameter("p_age", vc_AgeGroupID, typeof(Int32));
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

        //        object[] parameters = new object[] {ageParam, genderParam, qualififcationParam
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
        //           OFFSET @p_pageSize  * (@p_pageIndex -1) rows fetch next @p_pageSize ROWS ONLY", whereCand, whereExp);
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
        //        SqlParameter ageParam2 = AddParameter("p_age", vc_AgeGroupID, typeof(Int32));
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


        //        parameters = new object[] { ageParam2, genderParam2, qualififcationParam2
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

        public ActionResult CandidateSearchDetail2(int ca_id = 0, int vid = 0)
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);



            ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
            ViewBag.Logo = Convert.ToString(Session["Logo"]);
            ViewBag.vid = vid;



            int candidateid = ca_id;
            Session["Ca_ID_current"] = candidateid;
            ViewBag.Candidateid = candidateid;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;
                //var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                var candidate = db.Database.SqlQuery<CandidateNew>(@"select ca_GUID,ca_PhotoExtension, ca_HasPhoto,c.ca_lm_countryid,ca_lm_EducationLevel,ca_UniversityID,ca_FunctionTitleID,ca_CurrentJobTitleID,ca_TotalRelavantExperience,c.Ca_CurrentCountryID, c.ca_FirstName,c.ca_Surname,lmcona.lm_Value as Nationality, ca_PhoneMobile,ca_EmailAddress,ca_Password, c.ca_DateOfBirth, c.ca_Gender, c.ca_MaritalStatus,
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
where c.ca_id=" + candidateid + " and ca_active=1").ToList();
                if (candidate.Count == 0)
                {
                    //candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                    candidate = db.Database.SqlQuery<CandidateNew>("select * from [dbo].[Candidates] where ca_id=" + candidateid).ToList();
                }

                if (candidate[0].ca_HasPhoto == true)
                {
                    ViewBag.Profilepic = "Documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

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

                ViewBag.MaritalStatus = candidate[0].ca_MaritalStatus;

                //ViewBag.DOB = Convert.ToDateTime(candidate[0].ca_DateOfBirth).ToString("yyyy/MM/dd");
                ViewBag.DOB = Convert.ToDateTime(candidate[0].ca_DateOfBirth).ToString("dd/MM/yyyy");
                //var selectedGender = candidate[0].ca_Gender;
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
                ViewBag.Education1 = db.Database.SqlQuery<CandidateEducationNew>(@"select ce.CaEdu_ID,ce.Ca_ID,lmu.lm_Value as University,lmcon.lm_Value as UniCountry, lmDeg.lm_Value as Degree, lmSpez.lm_Value as Specialization, lmgr.lm_Value as Grade, ce.FromMonth,ce.FromYear, ce.ToMonth,ce.ToYear, ce.CurrentlyStudyHere,ce.Activities  from [CandidateEducation_T] ce 
                join ListsMaster lmu on lmu.lm_Id= ce.UniversityID
                join ListsMaster lmCon on lmCon.lm_Id= ce.UniCountryID
                join ListsMaster lmDeg on lmDeg.lm_Id= ce.DegreeID
                join ListsMaster lmSpez on lmSpez.lm_Id= ce.SpecializationID
                join ListsMaster lmGr on lmGr.lm_Id= ce.GradeID
                where ca_id=" + candidateid + " order by fromyear desc, frommonth desc").ToList();

                ViewBag.Usage = db.Database.SqlQuery<CSCreditUsage>(@"select * from [CSCreditUsage_T]
  
                where CandidateID=" + candidateid + "and CompanyID=" + companyid).ToList();

                if (ViewBag.Usage.Count > 0)
                {
                    ViewBag.Redeemed = true;
                }
                else
                {
                    ViewBag.Redeemed = false;
                }
                ViewBag.Experience2 = db.Database.SqlQuery<CandidateWorkHistory>(@"select cwh.CaWorkHistory_ID,cwh.Ca_ID,lmjt.lm_Value as JobTitle, lmjl.lm_Value as JobLevel, cwh.Company, lmrt.lm_Value as ReportingTo, lmind.lm_Value as Industry, lmfn.lm_Value as JobFunction, 
                cwh.FromMonth,cwh.FromYear,cwh.ToMonth,cwh.ToYear,cwh.CurrentlyWorkingHere,lmjlo.lm_Value as JobLocation, lmsal.lm_Value as Salary,cwh.[Description] from [CandidateWorkHistory_T] cwh 
                join ListsMaster lmjt on lmjt.lm_Id = cwh.JobTitleID
                join ListsMaster lmjl on lmjl.lm_Id = cwh.JobLevelID
                join ListsMaster lmrt on lmrt.lm_Id = cwh.ReportingToID
                join ListsMaster lmind on lmind.lm_Id = cwh.IndustryID
                join ListsMaster lmfn on lmfn.lm_Id = cwh.FunctionID
                join ListsMaster lmjlo on lmjlo.lm_Id = cwh.JobLocationID
                join ListsMaster lmsal on lmsal.lm_Id = cwh.SalaryID
                where ca_id=" + candidateid + " order by fromyear desc, frommonth desc").ToList();

                ViewBag.CandidateCertifications = db.Database.SqlQuery<CandidateCertifications>(@"select cc.CaCertification_ID,CertificationID,Authority,cc.Ca_ID,lmcer.lm_Value as Certification, cc.FromMonth,cc.FromYear,cc.ToMonth,cc.ToYear,cc.DoNotExpire from [CandidateCertification_T] cc
                join ListsMaster lmcer on lmcer.lm_Id = cc.CertificationID
                where ca_id=" + candidateid + " order by fromyear desc, frommonth desc").ToList();

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
                ViewBag.University = ddLists.Where(x => x.lm_lt_ListTypeId == 18).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobTitle = ddLists.Where(x => x.lm_lt_ListTypeId == 22).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Degree = ddLists.Where(x => x.lm_lt_ListTypeId == 19).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Specialization = ddLists.Where(x => x.lm_lt_ListTypeId == 20).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Grade = ddLists.Where(x => x.lm_lt_ListTypeId == 21).OrderByDescending(x => x.lm_Value).ToList();
                ViewBag.Reportingto = ddLists.Where(x => x.lm_lt_ListTypeId == 25).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Salary = ddLists.Where(x => x.lm_lt_ListTypeId == 5).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.JobLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 27).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Skill = ddLists.Where(x => x.lm_lt_ListTypeId == 23).OrderBy(x => x.lm_Value).ToList();
                ViewBag.SkillLevel = ddLists.Where(x => x.lm_lt_ListTypeId == 24).OrderBy(x => x.lm_Id).ToList();
                ViewBag.Languages = ddLists.Where(x => x.lm_lt_ListTypeId == 26).OrderBy(x => x.lm_Value).ToList();
                ViewBag.Experience = ddLists.Where(x => x.lm_lt_ListTypeId == 28).OrderBy(x => x.lm_Ordinal).ToList();
                ViewBag.Certification = ddLists.Where(x => x.lm_lt_ListTypeId == 29).OrderBy(x => x.lm_Value).ToList();


                List<CandidateApplications> applicantalready = db.Database.SqlQuery<CandidateApplications>(@"select  v.vc_Id,app_id,app.app_created,v.vc_Title,l.lm_Value from applications app 
join vacancies v on v.vc_Id = app.app_vc_VacancyId
join Companies c on c.co_Id = v.vc_co_CompanyId
join listsmaster l on l.lm_Id= app.app_st_ShortlistStatusId
where app_ca_CandidateId = " + candidateid + "  and co_id=" + Convert.ToInt32(Session["CompanyID"])).ToList();


                ViewBag.applicantalready = applicantalready;



                List<CandiateNotes> canotes = db.Database.SqlQuery<CandiateNotes>(@"select notedate, c.ct_Forename,c.ct_Surname,notedetails from Note_T n join contacts c on c.ct_Id = n.ct_id  where n.ca_id=" + candidateid + "and co_id=" + companyid).ToList();
                ViewBag.canotes = canotes;
            }

            dbOperations dbo = new dbOperations();
            List<CandidateSkills> cask = dbo.getCandidateSkills(candidateid);
            ViewBag.CandidateSkill = cask;

            List<CandidateSkills> ocask = dbo.getCandidateOSkills(candidateid);
            ViewBag.CandidateOSkill = ocask;

            List<CandidateLanguages> canlan = dbo.getCandidateLanguages(candidateid);
            ViewBag.CandidateLanguage = canlan;


            return View();
        }


        public ActionResult CareerServices()
        {

            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }

            return View();
        }


        public ActionResult AboutUs()
        {
            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }
            return View();
        }

        public ActionResult CVChecklist()
        {
            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }


            return View();
        }
        public ActionResult CVTips()
        {
            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }
            return View();
        }

        public ActionResult CVWriting()
        {
            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }
            return View();
        }





        public ActionResult TermsandConditions()
        {
            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }




            return View();
        }
        


        public ActionResult PrivacyPolicy()
        {
            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }




            return View();
        }
        public ActionResult TermsandConditionsUsers()
        {
            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }




            return View();
        }


        public ActionResult TermsandConditionsJobSeeker()
        {
            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }




            return View();
        }



        public ActionResult ContactUs()
        {
            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }




            return View();
        }



        [HttpPost]
        public ActionResult ContactUs(FormCollection col)
        {
            //send mail
            string bodycontent;
            bodycontent = "<font face='Arial' size='2'>";
            bodycontent += "<b>Jobs4Bahrainis Website Feedback!</b>";
            bodycontent += "<br><br>";
            bodycontent += "<p>Name: " + Convert.ToString(col["Name"]) + "</p>";
            bodycontent += "<p>Email: " + Convert.ToString(col["Email"]) + "</p>";
            bodycontent += "<p>Company: " + Convert.ToString(col["Company"]) + "</p>";
            bodycontent += "<p>Phone: " + Convert.ToString(col["Phone"]) + "</p>";
            bodycontent += "<p>Comments: " + Convert.ToString(col["Comments"]) + "</p>";
            bodycontent += "</font>";

            common cmn = new common();
            cmn.SendMail("admin@jobs4bahrainis.com", "Jobs4Bahrainis.com Contact Us", bodycontent);
            cmn.SendMail("mohamed.Ghazwan@jobs4bahrainis.com", "Jobs4Bahrainis.com Contact Us", bodycontent);

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult PasswordReset(string guidlink)
        {
            ViewBag.guidlink = guidlink;
            return View();
        }

        [HttpPost]
        public ActionResult PasswordReset2(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string sql;

                string guidx = Convert.ToString(col["guidlink"]);
                Guid guidlink = new Guid(Convert.ToString(col["guidlink"]));

                if (Convert.ToString(col["typez"]) == "Candidate")
                {
                    sql = "update candidates set ca_password='" + Convert.ToString(col["password"]) + "' where ca_guid='" + guidlink + "'";
                }
                else
                {
                    sql = "update contacts set ct_password='" + Convert.ToString(col["password"]) + "' where ca_guid='" + guidlink + "'";
                }

                db.Database.ExecuteSqlCommand(sql);
            }

            //guidlink

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }



        public ActionResult ForgotPassword()
        {
            return View();
        }



        public ActionResult ForgotPasswordChecker(FormCollection col)
        {

            string ispresent = "NO";

            string sqlquery = "";

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                if (Convert.ToString(col["typez"]) == "Employer")
                {
                    sqlquery = "select top 1 ct_id as id,ct_EmailAddress as emailid,ct_Password as passwordx, ct_guid as guidx from Contacts where ct_EmailAddress = '" + Convert.ToString(col["email"]) + "' order by ct_id desc";
                }
                else
                {
                    sqlquery = "select top 1 ca_Id as id,ca_EmailAddress as emailid,ca_Password as passwordx, ca_guid as guidx from Candidates where ca_EmailAddress = '" + Convert.ToString(col["email"]) + "' order by ca_id desc";
                }

                forgotpass fp = db.Database.SqlQuery<forgotpass>(sqlquery).SingleOrDefault();

                //send mail
                string bodycontent;
                bodycontent = "<img src='http://www.jobs4bahrainis.com/images/jobs4bahrainislogo.png'><br><br><font face='Arial' size='2'>";
                bodycontent += "<b>Forgot password - Jobs4Bahrainis.com</b>";
                bodycontent += "<p>Please find below the link to reset your password.</p>";
                bodycontent += "<p><table><tr><td style='height:40px;background-color:red;color:white;padding:0 30px;text-decoration:none'><a style='text-decoration:none;color:white' href='http://www.jobs4bahrainis.com/PasswordReset/" + fp.guidx + "'>CLICK HERE</a></td></tr></table></p>";
                bodycontent += "<p><br><br>Best Regards,<br>Jobs4Bahrainis</p>";

                bodycontent += "</font><br>";

                common cmn = new common();
                cmn.SendMail(fp.emailid, "Forgot password - Jobs4Bahrainis.com", bodycontent);


                if (fp.id != 0)
                {
                    ispresent = "YES";
                }
                else
                {
                    ispresent = "NO";
                }

                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = ispresent,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult EmployerCareerServices()
        {

            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }

            return View();
        }


        public ActionResult ECareerServicesDetails(int articleid)
        {


            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
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

            if (Session["Ca_ID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["CandidateName"]);
                ViewBag.Profilepic = Convert.ToString(Session["Profilepic"]);
            }
            else if (Session["RecruiterID"] != null)
            {
                ViewBag.Name = Convert.ToString(Session["RecruiterName"]);
                ViewBag.Logo = Convert.ToString(Session["Logo"]);
            }
            return View();
        }






        public static string TruncateAtWord(string value, int length)
        {
            if (value == null || value.Length < length || value.IndexOf(" ", length) == -1)
                return value;

            return value.Substring(0, value.IndexOf(" ", length));
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
                string ca_Profile = StripHTML(Convert.ToString(col["ca_Profile"]));

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




        [HttpPost]
        public ActionResult CandidateNote(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Insert into Note_T(ca_id,ct_id,co_id,NoteDate,NoteDetails) values({0},{1},{2},getdate(),'{3}')", Convert.ToInt32(col["ca_id"]), Convert.ToInt32(Session["RecruiterID"]), Convert.ToInt32(Session["CompanyID"]), Convert.ToString(col["notes"]));
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


        public ActionResult SaveJob(int VacancyID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("INSERT into SavedJob_T(vc_id,ca_id,saveddate) values(" + VacancyID + "," + Convert.ToInt32(Session["Ca_ID"]) + ",getdate())");
                db.Database.Connection.Close();
            }
            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult FollowCompany(int CompanyID)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                db.Database.ExecuteSqlCommand("INSERT into CompanyFollowers_T(CompanyID,CandidateID,DOC) values(" + CompanyID + "," + Convert.ToInt32(Session["Ca_ID"]) + ",getdate())");
                db.Database.Connection.Close();
            }
            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = "OK",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }



        public ActionResult AlreadyApplied(int VacancyID)
        {
            string ispresent = "NO";
            int alreadyapplied = 0;
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                alreadyapplied = db.Database.SqlQuery<int>("select top 1 app_id from Applications where app_vc_VacancyId=" + VacancyID + " and app_ca_CandidateId=" + Convert.ToInt32(Session["Ca_ID"]) + "").SingleOrDefault();

                if (alreadyapplied != 0)
                {

                    ispresent = "YES";
                }
                else
                {
                    ispresent = "NO";
                }

                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = ispresent,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult JobApplication(int VacancyID)
        {
            string ispresent = "NO";

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("update vacancies set vc_applications= vc_applications+1 where vc_id={0};insert into Applications(app_vc_VacancyId, app_ca_CandidateId, app_Created, app_LastUpdated) values({0},{1},getdate(),getdate());select @@identity", VacancyID, Convert.ToInt32(Session["Ca_ID"]));
                decimal candi = 0;
                candi = db.Database.SqlQuery<decimal>(sb.ToString()).SingleOrDefault();
                if (candi != 0)
                {
                    ispresent = "YES";
                }
                else
                {
                    ispresent = "NO";
                }


                JobApplicant jba = db.Database.SqlQuery<JobApplicant>("select c.ca_FirstName,c.ca_Surname,v.vc_Title, a.app_LastUpdated,v.vc_ApplicationEmail from Applications a join Vacancies v on a.app_vc_VacancyId= vc_Id join Candidates c on a.app_ca_CandidateId= c.ca_Id where a.app_vc_VacancyId=" + VacancyID + " and a.app_ca_CandidateId=" + Convert.ToInt32(Session["Ca_ID"]) + "").SingleOrDefault();

                string bodycontent = "<p style='font-family:arial;font-size:14px;'>";
                bodycontent += "<br>Dear Employer,<br><br>You have received a new application for the Vacancy:<b>" + jba.vc_Title + "</b>";
                bodycontent += "<br><br>Candidate Name: <b>" + jba.ca_FirstName + " " + jba.ca_Surname + "</b>";
                bodycontent += "<br><br>Application Date: <b>" + jba.app_LastUpdated.ToString("dd-MMM-yyyy") + "</b>";
                bodycontent += "<br><br>To view more details about this candidate please log on to the website  http://www.jobs4bahrainis.com </p>";


                string path = Server.MapPath("~/Templates/ApplicationReceived.html");
                string bodycontenthtml = System.IO.File.ReadAllText(path);
                bodycontenthtml = bodycontenthtml.Replace("##messages##", bodycontent);


                common cmn = new common();

                cmn.SendMail(jba.vc_ApplicationEmail, "Job Application - J4B Website", bodycontenthtml);

                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = ispresent,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult EmailChecker(string EmailID)
        {
            string ispresent = "NO";
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("select top 1 ca_id from Candidates where ca_deleted is null and ca_EmailAddress='{0}'", EmailID);
                int candi = 0;
                candi = db.Database.SqlQuery<int>(sb.ToString()).SingleOrDefault();
                if (candi != 0)
                {
                    ispresent = "YES";
                }
                else
                {
                    ispresent = "NO";
                }

                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = ispresent,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        public ActionResult EmailCheckerEMP(string EmailID)
        {
            string ispresent = "NO";
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("select top 1 ct_id from contacts where ct_deleted is null and ct_EmailAddress='{0}'", EmailID);
                int candi = 0;
                candi = db.Database.SqlQuery<int>(sb.ToString()).SingleOrDefault();
                if (candi != 0)
                {
                    ispresent = "YES";
                }
                else
                {
                    ispresent = "NO";
                }

                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = ispresent,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public ActionResult CompanyChecker(string companyname)
        {
            string ispresent = "NO";
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("select top 1 co_Id from companies where co_deleted is null and co_Name='{0}'", companyname);
                int candi = 0;
                candi = db.Database.SqlQuery<int>(sb.ToString()).SingleOrDefault();
                if (candi != 0)
                {
                    ispresent = "YES";
                }
                else
                {
                    ispresent = "NO";
                }

                db.Database.Connection.Close();
            }

            return new JsonpResult
            {
                ContentEncoding = Encoding.UTF8,
                Data = ispresent,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }


        [HttpPost]
        public ActionResult RedeemCredit(FormCollection col)
        {
            bool isRedeemed = false;
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {

                if (Session["RecruiterID"] != null)
                {
                    int Ca_ID = Convert.ToInt32(Session["Ca_ID_current"]);
                    int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
                    var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                    int companyid = recruiter[0].ct_co_CompanyId;

                    dbOperations dbo = new dbOperations();
                    isRedeemed = dbo.CS_CreditUsage(companyid, recruiterid, 2, 0, Ca_ID);

                    dbo.ReportAdd(companyid, recruiterid, 4);

                }
                //    string Ca_ID = Convert.ToString(col["Ca_ID"]);
                //    string DegreeLevel = Convert.ToString(col["DegreeLevel"]);
                //    string MajorSubject = Convert.ToString(col["MajorSubject"]);
                //    string Institute = Convert.ToString(col["Institute"]);
                //    string Grade = Convert.ToString(col["Grade"]);
                //    string State = Convert.ToString(col["State"]);
                //    string Country = Convert.ToString(col["c"]);



                //    StringBuilder sb = new StringBuilder();
                //    sb.AppendFormat("insert into [CandidateEducation] (Ca_ID,DegreeLevel,MajorSubject,Institute,Grade,State,Country) values({0},'{1}','{2}','{3}','{4}','{5}','{6}')", Convert.ToInt32(Ca_ID), Convert.ToString(DegreeLevel), Convert.ToString(MajorSubject), Convert.ToString(Institute), Convert.ToString(Grade), Convert.ToString(State), Convert.ToString(Country));
                //    db.Database.ExecuteSqlCommand(sb.ToString());
                //    db.Database.Connection.Close();
            }

            //return isRedeemed;
            if (isRedeemed)
            {
                return new JsonpResult
                {
                    ContentEncoding = Encoding.UTF8,
                    Data = "OK",
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                };
            }
            else
            {
                return new JsonpResult
                {

                };
                //return new JsonpResult
                //{
                //    ContentEncoding = Encoding.UTF8,
                //    Data = "OK",
                //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                //};

            }

        }


        public ActionResult VacancySectorz()
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_id in (select distinct vc_industryid from Vacancies where vc_Deleted is null and vc_st_StatusID=1 and vc_ExpiryDate>=getdate())").ToList();

                List<ListMaster> lm = new List<ListMaster>();
                ListMaster _lm;
                foreach (var s in ddLists)
                {
                    _lm = new ListMaster();
                    _lm.lm_Value = s.lm_Value;
                    _lm.lm_Id = s.lm_Id;
                    _lm.SectorURL = urlcleaner(s.lm_Value);
                    lm.Add(_lm);
                }


                ViewBag.VacancySectorlist = lm;
            }
            return PartialView();
        }


        public ActionResult JobApplicantsUpdated(int jobid, int typeid)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<jobapplicants> jbapplicants = db.Database.SqlQuery<jobapplicants>(@"select ca_id,app.app_id,app.app_st_ShortlistStatusId,ca.ca_PhotoExtension,app_created,ca_LastLogin,ca_GUID, ca_FirstName,ca_Surname,cc.lm_Value as CurrentCountry,el.lm_Value as educationlevel,ca_Profile,ca_hasphoto,ca_photoextension,ca_Gender,ca_DateOfBirth,
                     ca_maritalstatus,ft.lm_Value as functiontitle,cjt.lm_Value as CurrentJobTitle, tre.lm_Value as TotalRelevantExperience  from candidates ca
                     join listsmaster cc on cc.lm_Id=ca.ca_CurrentCountryID
                     join ListsMaster el on el.lm_id=ca.ca_lm_EducationLevel
                     join ListsMaster ft on ft.lm_id=ca.ca_functiontitleid
                     join listsmaster cjt on cjt.lm_id=ca.ca_currentjobtitleid
                     join listsmaster tre on tre.lm_id=ca.ca_TotalRelavantExperience
					 right join Applications app on ca.ca_Id = app.app_ca_CandidateId
                     where ca_Active=1 and app.app_vc_VacancyId=" + jobid + " and app_st_ShortlistStatusId=" + typeid).ToList();


                ViewBag.jbapplicants = jbapplicants;
            }

            dbOperations dbo = new dbOperations();
            ViewBag.ListTypeIDs = dbo.getlist(35);

            return PartialView();
        }





        public List<ListMaster> listmastervalues(int listtypeid)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                List<ListMaster> ddLists = db.Database.SqlQuery<ListMaster>("select * from ListsMaster where lm_lt_ListTypeId in (32,6,5,12,28,13,17,4) and lm_Deleted is null order by lm_Ordinal").ToList();
                return ddLists.Where(x => x.lm_lt_ListTypeId == listtypeid).ToList();
            }

        }


        public ActionResult UserActivityReport()
        {

            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                List<UserActvReport> lstall = db.Database.SqlQuery<UserActvReport>(@"select ct_Forename, ct_Surname, vc.vc_Title,ca.ca_FirstName,ca_Surname, ct_LastLogin,cu.UsedOn,cu.CreditTypeID,cu.CandidateID, cu.VacancyID from contacts ct 
                inner join[CSCreditUsage_T] cu on ct.ct_Id = cu.ContactID 
                left join Candidates ca on ca.ca_id=cu.CandidateID
                left join Vacancies vc on vc.vc_id=cu.VacancyID
                where ct_co_CompanyId=" + companyid).ToList();

                ViewBag.Contacts = lstall.ToList();

                ViewBag.Summary = db.Database.SqlQuery<UserActvReport>("select  ct_Forename, ct_Surname, ct_LastLogin from contacts  where ct_co_CompanyId =" + companyid).ToList();

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.SurName = recruiter[0].ct_Surname;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;
                ViewBag.LastUpdated = company[0].co_LastUpdated;


            }

            return View();
        }

        public ActionResult RecruitmentActivityReport()
        {



            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension.Trim() == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "Documents/Logos/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + "-original" + extns;

                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.SurName = recruiter[0].ct_Surname;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;
                ViewBag.LastUpdated = company[0].co_LastUpdated;


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
                select count(app_id) as total from Applications where app_vc_VacancyId in (select vc_id from Vacancies where vc_co_CompanyId={0})", companyid);

                ViewBag.Statistics = db.Database.SqlQuery<int>(stb.ToString()).ToList();


                ViewBag.Subscription = db.Database.SqlQuery<CSSubscription>(@"select top 1 s.SubscriptionID,p.PackageName,c.ct_Forename,c.ct_Surname, s.StartDate,s.EndDate,s.DOC as PurchaseDate from [dbo].[CSSubscription_T] s  join Contacts c on c.ct_Id = s.ContactID join CSPackage_T p on p.PackageID = s.PackageID where companyid=" + Convert.ToInt32(companyid) + " order by s.SubscriptionID desc").ToList();
            }



            return View();
        }

        public string trimPicExtensions(string extension)
        {

            string cleanstring = "";
            if (extension != null)
            {
                extension = extension.Trim();
                cleanstring = extension + "?" + Convert.ToString(DateTime.Now).Replace(" ", "").Replace(":", "").Replace("/", "");
            }
            else
            {
                cleanstring = "j4b";
            }

            return cleanstring;
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public string StripHTMLExcept(string sText)
        {

            sText = Regex.Replace(sText, "style=(\"|')[^(\"|')]*(\"|')", "");
            sText = sText.Replace(Environment.NewLine, "<br />");

            // font div br tags are only removed

            return Regex.Replace(sText, "</?(font|div|br|\t|\n|\r)[^>]*>", String.Empty, RegexOptions.IgnoreCase);
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