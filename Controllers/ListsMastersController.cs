using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Jobs4Bahrainis.Models;

namespace Jobs4Bahrainis.Controllers
{

    public class ListsMastersController : Controller
    {
        private Jobs4bahrainisEntities db = new Jobs4bahrainisEntities();


       





        // GET: ListsMasters
        public ActionResult Index()
        {
            return View(db.ListsMasters.ToList());
        }

        // GET: ListsMasters/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ListsMaster listsMaster = db.ListsMasters.Find(id);
            if (listsMaster == null)
            {
                return HttpNotFound();
            }
            return View(listsMaster);
        }

        // GET: ListsMasters/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ListsMasters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "lm_Id,lm_Guid,lm_Created,lm_LastUpdated,lm_us_UserId,lm_lt_ListTypeId,lm_lm_ParentId,lm_Value,lm_Ordinal,lm_Ranking,lm_Deleted,lm_ValueEncoded")] ListsMaster listsMaster)
        {
            if (ModelState.IsValid)
            {
                db.ListsMasters.Add(listsMaster);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(listsMaster);
        }

        // GET: ListsMasters/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ListsMaster listsMaster = db.ListsMasters.Find(id);
            if (listsMaster == null)
            {
                return HttpNotFound();
            }
            return View(listsMaster);
        }

        // POST: ListsMasters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "lm_Id,lm_Guid,lm_Created,lm_LastUpdated,lm_us_UserId,lm_lt_ListTypeId,lm_lm_ParentId,lm_Value,lm_Ordinal,lm_Ranking,lm_Deleted,lm_ValueEncoded")] ListsMaster listsMaster)
        {
            if (ModelState.IsValid)
            {
                db.Entry(listsMaster).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(listsMaster);
        }

        // GET: ListsMasters/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ListsMaster listsMaster = db.ListsMasters.Find(id);
            if (listsMaster == null)
            {
                return HttpNotFound();
            }
            return View(listsMaster);
        }

        // POST: ListsMasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ListsMaster listsMaster = db.ListsMasters.Find(id);
            db.ListsMasters.Remove(listsMaster);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
