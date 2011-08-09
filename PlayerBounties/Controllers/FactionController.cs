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
    public class FactionController : Controller
    {
        private PlayerBountyContext db = new PlayerBountyContext();

        //
        // GET: /Faction/

        public ViewResult Index()
        {
            return View(db.Factions.ToList());
        }

        //
        // GET: /Faction/Details/5

        public ViewResult Details(Guid id)
        {
            Faction faction = db.Factions.Find(id);
            return View(faction);
        }

        //
        // GET: /Faction/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Faction/Create

        [HttpPost]
        public ActionResult Create(Faction faction)
        {
            if (ModelState.IsValid)
            {
                faction.Id = Guid.NewGuid();
                db.Factions.Add(faction);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(faction);
        }
        
        //
        // GET: /Faction/Edit/5
 
        public ActionResult Edit(Guid id)
        {
            Faction faction = db.Factions.Find(id);
            return View(faction);
        }

        //
        // POST: /Faction/Edit/5

        [HttpPost]
        public ActionResult Edit(Faction faction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(faction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(faction);
        }

        //
        // GET: /Faction/Delete/5
 
        public ActionResult Delete(Guid id)
        {
            Faction faction = db.Factions.Find(id);
            return View(faction);
        }

        //
        // POST: /Faction/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {            
            Faction faction = db.Factions.Find(id);
            db.Factions.Remove(faction);
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