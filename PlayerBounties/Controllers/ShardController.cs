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
    public class ShardController : Controller
    {
        private PlayerBountyContext db = new PlayerBountyContext();

        //
        // GET: /Shard/

        public ViewResult Index()
        {
            return View(db.Shards.ToList());
        }

        //
        // GET: /Shard/Details/5

        public ViewResult Details(Guid id)
        {
            Shard shard = db.Shards.Find(id);
            return View(shard);
        }

        //
        // GET: /Shard/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Shard/Create

        [HttpPost]
        public ActionResult Create(Shard shard)
        {
            if (ModelState.IsValid)
            {
                shard.Id = Guid.NewGuid();
                db.Shards.Add(shard);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(shard);
        }
        
        //
        // GET: /Shard/Edit/5
 
        public ActionResult Edit(Guid id)
        {
            Shard shard = db.Shards.Find(id);
            return View(shard);
        }

        //
        // POST: /Shard/Edit/5

        [HttpPost]
        public ActionResult Edit(Shard shard)
        {
            if (ModelState.IsValid)
            {
                db.Entry(shard).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(shard);
        }

        //
        // GET: /Shard/Delete/5
 
        public ActionResult Delete(Guid id)
        {
            Shard shard = db.Shards.Find(id);
            return View(shard);
        }

        //
        // POST: /Shard/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {            
            Shard shard = db.Shards.Find(id);
            db.Shards.Remove(shard);
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