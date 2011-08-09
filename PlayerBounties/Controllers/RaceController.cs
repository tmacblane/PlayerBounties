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
    public class RaceController : Controller
    {
        private PlayerBountyContext db = new PlayerBountyContext();

        //
        // GET: /Race/

        public ViewResult Index()
        {
            return View(db.Races.ToList());
        }

        //
        // GET: /Race/Details/5

        public ViewResult Details(Guid id)
        {
            Race race = db.Races.Find(id);
            return View(race);
        }

        //
        // GET: /Race/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Race/Create

        [HttpPost]
        public ActionResult Create(Race race)
        {
            if (ModelState.IsValid)
            {
                race.Id = Guid.NewGuid();
                db.Races.Add(race);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(race);
        }
        
        //
        // GET: /Race/Edit/5
 
        public ActionResult Edit(Guid id)
        {
            Race race = db.Races.Find(id);
            return View(race);
        }

        //
        // POST: /Race/Edit/5

        [HttpPost]
        public ActionResult Edit(Race race)
        {
            if (ModelState.IsValid)
            {
                db.Entry(race).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(race);
        }

        //
        // GET: /Race/Delete/5
 
        public ActionResult Delete(Guid id)
        {
            Race race = db.Races.Find(id);
            return View(race);
        }

        //
        // POST: /Race/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {            
            Race race = db.Races.Find(id);
            db.Races.Remove(race);
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