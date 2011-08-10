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
    public class BountyController : Controller
    {
        private PlayerBountyContext db = new PlayerBountyContext();

		public ActionResult Search(Character character)
		{
			ViewBag.ShardId = new SelectList(db.Shards, "Id", "Name");
			ViewBag.FactionId = new SelectList(db.Factions, "Id", "Name");
			ViewBag.RaceId = new SelectList(db.Races, "Id", "Name");
			ViewBag.PlayerClassId = new SelectList(db.PlayerClasses, "Id", "Name");
			return View();
		}

        //
        // GET: /Bounty/
        public ViewResult Index()
        {
            return View(db.Bounties.ToList());
        }

        //
        // GET: /Bounty/Details/5

        public ViewResult Details(Guid id)
        {
            Bounty bounty = db.Bounties.Find(id);
            return View(bounty);
        }

        //
        // GET: /Bounty/Create

        public ActionResult Create(Character character)
        {
			Bounty bounty = new Bounty();
			bounty.PlacedOnId = character.Id;
			return View(bounty);
        } 

        //
        // POST: /Bounty/Create

        [HttpPost]
		public ActionResult Create(Bounty bounty)
        {
			// string characterName = formCollection["characterShardName"];

            if (ModelState.IsValid)
            {
                bounty.Id = Guid.NewGuid();

				Character character = new Character();
							
				// search for character by name, shard, allegiance
				// if character does not exist
				// create character
				// return placedOnId as newly created characterId

				// bounty.PlacedById
				// bounty.PlacedOnId
				
				bounty.DatePlaced = DateTime.Now;
				bounty.DateCompleted = DateTime.MinValue;
				bounty.IsPlacementPending = true;
                db.Bounties.Add(bounty);
                db.SaveChanges();

                return RedirectToAction("Index");  
            }

            return View(bounty);
        }
        
        //
        // GET: /Bounty/Edit/5
 
        public ActionResult Edit(Guid id)
        {
            Bounty bounty = db.Bounties.Find(id);
            return View(bounty);
        }

        //
        // POST: /Bounty/Edit/5

        [HttpPost]
        public ActionResult Edit(Bounty bounty)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bounty).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(bounty);
        }

        //
        // GET: /Bounty/Delete/5
 
        public ActionResult Delete(Guid id)
        {
            Bounty bounty = db.Bounties.Find(id);
            return View(bounty);
        }

        //
        // POST: /Bounty/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {            
            Bounty bounty = db.Bounties.Find(id);
            db.Bounties.Remove(bounty);
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