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

namespace Jobs4Bahrainis.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
           
            using(Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
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
                    try {
                        ljx.co_Name = lj.co_Name.Substring(0, 18) + "...";
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


        public ActionResult Login(string loggedin) {
            if (loggedin == "invalid")
            {
                ViewBag.LoggedIn = "Invalid Email ID / Password ! try again.";
            }
            return View();
        }



        [HttpPost]
        public ActionResult Login(FormCollection col)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                string email = Convert.ToString(col["emailid"]);
                string password = Convert.ToString(col["password"]);
                var EmpLoggedIn = db.Contacts.Where(x => x.ct_EmailAddress == email && x.ct_Password == password && x.ct_Deleted == null).ToList();
                if (EmpLoggedIn.Count > 0)
                {
                    Session["RecruiterID"] = Convert.ToInt32(EmpLoggedIn[0].ct_Id);
                    Session["RecruiterName"] = Convert.ToString(EmpLoggedIn[0].ct_Forename);

                    return RedirectToAction("../Recruiter");
                }
                else
                {
                    var JobSeekerLoggedIn = db.Candidates.Where(x => x.ca_EmailAddress == email && x.ca_Password == password && x.ca_Deleted == null).ToList();

                    if (JobSeekerLoggedIn.Count > 0)
                    {
                        Session["CandidateID"] = Convert.ToInt32(JobSeekerLoggedIn[0].ca_Id);
                        Session["CandidateName"] = Convert.ToString(JobSeekerLoggedIn[0].ca_FirstName);


                        return RedirectToAction("../JobSeekerProfile");
                    }
                    else
                    {
                        return RedirectToAction("Login/invalid");

                    }
                }
            }
        }

        public ActionResult RecruiterRegistration()
        {

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                ViewBag.Sectors = db.Database.SqlQuery<jobsector>("select lm_Id as SectorID,lm_Value as Sector from ListsMaster where lm_lt_ListTypeId=4 and lm_us_UserId=25 and lm_Deleted is null order by lm_Ordinal").ToList();

            }
            return View();
        }

 

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
                sb.AppendFormat("insert into Companies(co_Name,co_br_HomeBrandId,co_PostalAddress,co_Telephone,co_ClientType,co_st_StatusID,co_NameEncoded) values('{0}',2,'{1}','{2}',30,22,'{3}')", companyname, address, telephone, companynameencoded);
                db.Database.ExecuteSqlCommand(sb.ToString());

                int companyid = db.Database.SqlQuery<int>("select max(co_id) as companyid from Companies").SingleOrDefault();

                


                string FirstName = Convert.ToString(col["FirstName"]);
                string LastName = Convert.ToString(col["LastName"]);
                string EmailID = Convert.ToString(col["EmailID"]);
                string Password = Convert.ToString(col["Password"]);

                sb = new StringBuilder();
                sb.AppendFormat("insert into Contacts(ct_Forename,ct_Surname,ct_EmailAddress,ct_Telephone,ct_Password,ct_Superuser,ct_st_StatusId,ct_co_CompanyId) values('{0}','{1}','{2}','{3}','{4}',1,20,{5})", FirstName, LastName, EmailID, Password, telephone, companyid);
                db.Database.ExecuteSqlCommand(sb.ToString());


                Contact ct = db.Contacts.OrderByDescending(x => x.ct_Id).First();
                Session["RecruiterID"] = Convert.ToInt32(ct.ct_Id);
                Session["RecruiterName"] = Convert.ToString(ct.ct_Forename);
                Session["CompanyID"] = companyid;
            }

            return RedirectToAction("RecruiterRegistration");
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

            return RedirectToAction("PaymentSuccess");
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




        public ActionResult Recruiter()
        {
            if (Session["RecruiterID"] == null) { return RedirectToAction("Login"); }

            int recruiterid = Convert.ToInt32(Session["RecruiterID"]);
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var recruiter = db.Contacts.Where(x => x.ct_Id == recruiterid).ToList();
                int companyid = recruiter[0].ct_co_CompanyId;

                var company = db.Companies.Where(x => x.co_Id == companyid).ToList();

                string extns = company[0].co_LogoExtension == "" ? "jpg" : company[0].co_LogoExtension;
                ViewBag.Logo = "http://www.jobs4bahrainis.com/Logox/" + company[0].co_Guid.ToString().Substring(0, 2) + "/" + company[0].co_Guid.ToString().Substring(2, 2) + "/" + company[0].co_Guid.ToString().Substring(4, 2) + "/" + company[0].co_Guid.ToString() + ".png";
                ViewBag.Company = company[0].co_Name;
                ViewBag.Name = recruiter[0].ct_Forename;
                ViewBag.Email = recruiter[0].ct_EmailAddress;
                ViewBag.Phone = recruiter[0].ct_Telephone;



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

                ViewBag.lstjobsectorcount = db.Database.SqlQuery<jobsector>(jobSectorCount).ToList();

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

                ViewBag.LatestJobs = LatestJobs2.ToList();



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
                    jobskr.Age = DateTime.Now.Year - js.ca_DateOfBirth.Year;
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

        public ActionResult JobSeeker()
        {

            if (Session["CandidateID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["CandidateID"]);

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

                    ljx.postedsince = "" + days;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToList();



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

                ViewBag.lstjobsectorcount = db.Database.SqlQuery<jobsector>(jobSectorCount).ToList();

            }
            

            return View();
        }


        public ActionResult JobSeekerProfile()
        {

            if (Session["CandidateID"] == null) { return RedirectToAction("Login"); }

            int candidateid = Convert.ToInt32(Session["CandidateID"]);
            ViewBag.Candidateid = candidateid;

            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                var candidate = db.Database.SqlQuery<JobSeekers>("select * from [dbo].[Candidates] c join [ListsMaster] lm on c.ca_lm_EducationLevel = lm.lm_Id where ca_id=" + candidateid).ToList();
                           

                ViewBag.Profilepic = "http://www.jobs4bahrainis.com/documents/photos/" + candidate[0].ca_GUID.ToString().Substring(0, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(2, 2) + "/" + candidate[0].ca_GUID.ToString().Substring(4, 2) + "/" + candidate[0].ca_GUID.ToString() + "." + candidate[0].ca_photoextension;

                 
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

                ViewBag.CVpath = "http://www.jobs4bahrainis.com/cvdx/" + candidatedocument[0].cd_Guid.ToString().Substring(0, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(2, 2) + "/" + candidatedocument[0].cd_Guid.ToString().Substring(4, 2) + "/" + candidatedocument[0].cd_Guid.ToString() + "." + candidatedocument[0].cd_FileExtension;



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

                ViewBag.Interests = intrst.Remove(intrst.Length - 2);

                 

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

                ViewBag.lstjobsectorcount = db.Database.SqlQuery<jobsector>(jobSectorCount).ToList();
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





        public ActionResult DeleteCandidateSkill(int caskillid)
        {
            using (Jobs4bahrainisEntities db = new Jobs4bahrainisEntities())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("delete from [CandidateSkills] where Ca_ID={0} and caskillid={1}", Convert.ToInt32(Session["CandidateID"]), caskillid);
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
        public ActionResult Search(FormCollection col)
        {
            string keywords = Convert.ToString(col["keywords"]);
            string sector = Convert.ToString(col["sector"]);

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(@"select top 100 vc_Id,co_id,vc_Title,lm_Id,lm_value,vc_Location,co_name,vc_description,co_Guid,vc_Created,co_HasLogo,co_LogoExtension from Vacancies v
            inner join Companies c on v.vc_co_CompanyId = c.co_Id 
            inner join VacancySectors vs on vs.vs_vc_VacancyId = v.vc_Id
            inner join ListsMaster lm on lm.lm_Id = vs.vs_lm_SectorId
            where vc_Deleted is null and co_br_HomeBrandId=2 and vc_Title like '%{0}%' and lm_Id={1} order by vc_id desc", keywords, sector);

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

                ViewBag.LatestJobs = LatestJobs2.ToList();

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
                ViewBag.lstjobsectorcount = db.Database.SqlQuery<jobsector>(jobSectorCount).ToList();
            }

            return View();
        }






        public ActionResult JobDescription(int jobid, string jobtitle)
        {
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
                    ljx.lm_value = lj.lm_value;
                    LatestJobs2.Add(ljx);

                }

                ViewBag.LatestJobs = LatestJobs2.ToList();




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
                ViewBag.lstjobsectorcount = db.Database.SqlQuery<jobsector>(jobSectorCount).ToList();
            }
            return View();
        }

        public ActionResult PostAJob()
        {
            return View();
        }


        public ActionResult ContactUs()
        {
            return View();
        }


        public ActionResult MediaCenter()
        {
            return View();
        }

       

       

        public ActionResult Registration()
        {
            return View();
        }










        public string urlcleaner(string originalstr)
        {
            string cleanstr = string.Empty;
            if (originalstr != null)
            {
                cleanstr = originalstr.Replace(" ", "-").Replace(".", "").Replace("?", "").Replace("&", "").Replace("'", "").Replace("!", "").Replace(":", "").Replace("%", "").Replace(";", "").Replace("*", "").Replace("\"", "");
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