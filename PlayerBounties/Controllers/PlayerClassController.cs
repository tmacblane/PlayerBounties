using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PlayerBounties.Models;

namespace PlayerBounties.Controllers
{ 
    public class PlayerClassController : Controller
    {
        private PlayerBountyContext db = new PlayerBountyContext();

        //
        // GET: /PlayerClass/

        public ViewResult Index()
        {
            return View(db.PlayerClasses.ToList());
        }

        //
        // GET: /PlayerClass/Details/5

        public ViewResult Details(Guid id)
        {
            PlayerClass playerclass = db.PlayerClasses.Find(id);
            return View(playerclass);
        }

        //
        // GET: /PlayerClass/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /PlayerClass/Create

        [HttpPost]
        public ActionResult Create(PlayerClass playerclass)
        {
            if (ModelState.IsValid)
            {
                playerclass.Id = Guid.NewGuid();
                db.PlayerClasses.Add(playerclass);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(playerclass);
        }
        
        //
        // GET: /PlayerClass/Edit/5
 
        public ActionResult Edit(Guid id)
        {
            PlayerClass playerclass = db.PlayerClasses.Find(id);
            return View(playerclass);
        }

        //
        // POST: /PlayerClass/Edit/5

        [HttpPost]
        public ActionResult Edit(PlayerClass playerclass)
        {
            if (ModelState.IsValid)
            {
                db.Entry(playerclass).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(playerclass);
        }

        //
        // GET: /PlayerClass/Delete/5
 
        public ActionResult Delete(Guid id)
        {
            PlayerClass playerclass = db.PlayerClasses.Find(id);
            return View(playerclass);
        }

        //
        // POST: /PlayerClass/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {            
            PlayerClass playerclass = db.PlayerClasses.Find(id);
            db.PlayerClasses.Remove(playerclass);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}