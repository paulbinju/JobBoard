using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Jobs4Bahrainis.Models;
using System.Text;

namespace Jobs4Bahrainis.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            Jobs4bahrainisEntities db = new Jobs4bahrainisEntities(); 

            List<status_T> list = db.status_T.ToList();
            ViewBag.Status = new SelectList(list, "ID", "Status");

            return View();
        }


        [HttpPost]
        public ActionResult Index(EmployeeViewModel model)
        {
            try
            {

                Jobs4bahrainisEntities db = new Jobs4bahrainisEntities();
                List<status_T> list = db.status_T.ToList();
                ViewBag.DepartmentList = new SelectList(list, "ID", "Status");

                status_T emp = new status_T();
                emp.ID = model.ID;
                emp.Status = model.Status;
                

                db.status_T.Add(emp);
                db.SaveChanges();

                
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
    }
}