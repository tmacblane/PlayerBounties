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
    public class CharacterController : Controller
	{
		#region Fields

		private Account account = new Account();
		private Character character = new Character();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		//
        // GET: /Character/

        public ViewResult Index()
        {
			var characters = character.GetAllCharactersForAnAccount(account.GetLoggedInUserId());
			return View(characters.ToList());
        }

        //
        // GET: /Character/Details/5

        public ViewResult Details(Guid id)
        {
            Character character = db.Characters.Find(id);
            return View(character);
        }

        //
        // GET: /Character/Create

        public ActionResult Create()
        {
            ViewBag.ShardId = new SelectList(db.Shards, "Id", "Name");
            ViewBag.FactionId = new SelectList(db.Factions, "Id", "Name");
            ViewBag.RaceId = new SelectList(db.Races, "Id", "Name");
            ViewBag.PlayerClassId = new SelectList(db.PlayerClasses, "Id", "Name");
            return View();
        } 

        //
        // POST: /Character/Create
        [HttpPost]
        public ActionResult Create(Character character)
        {
            if (ModelState.IsValid)
            {
                character.Id = Guid.NewGuid();
				character.UserId = account.GetLoggedInUserId();
                db.Characters.Add(character);
                db.SaveChanges();

				return RedirectToAction("Dashboard", "Home", new
				{
					accountId = character.UserId
				});
            }

            ViewBag.ShardId = new SelectList(db.Shards, "Id", "Name", character.ShardId);
            ViewBag.FactionId = new SelectList(db.Factions, "Id", "Name", character.FactionId);
            ViewBag.RaceId = new SelectList(db.Races, "Id", "Name", character.RaceId);
            ViewBag.PlayerClassId = new SelectList(db.PlayerClasses, "Id", "Name", character.PlayerClassId);
            return View(character);
        }

		[HttpPost]
		public Guid CreateBountyCharacter(Character character)
		{
			if(ModelState.IsValid)
			{
				character.Id = Guid.NewGuid();
				character.UserId = Guid.Empty;
				db.Characters.Add(character);
				db.SaveChanges();
			}

			ViewBag.ShardId = new SelectList(db.Shards, "Id", "Name", character.ShardId);
			ViewBag.FactionId = new SelectList(db.Factions, "Id", "Name", character.FactionId);
			ViewBag.RaceId = new SelectList(db.Races, "Id", "Name", character.RaceId);
			ViewBag.PlayerClassId = new SelectList(db.PlayerClasses, "Id", "Name", character.PlayerClassId);

			return character.Id;
		}
        
        //
        // GET: /Character/Edit/5 
        public ActionResult Edit(Guid id)
        {
            Character character = db.Characters.Find(id);
            ViewBag.ShardId = new SelectList(db.Shards, "Id", "Name", character.ShardId);
            ViewBag.FactionId = new SelectList(db.Factions, "Id", "Name", character.FactionId);
            ViewBag.RaceId = new SelectList(db.Races, "Id", "Name", character.RaceId);
            ViewBag.PlayerClassId = new SelectList(db.PlayerClasses, "Id", "Name", character.PlayerClassId);
            return View(character);
        }

        //
        // POST: /Character/Edit/5

        [HttpPost]
        public ActionResult Edit(Character character)
        {
            if (ModelState.IsValid)
            {
				character.UserId = account.GetLoggedInUserId();
                db.Entry(character).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("MyAccount", "Account"); 
            }
            ViewBag.ShardId = new SelectList(db.Shards, "Id", "Name", character.ShardId);
            ViewBag.FactionId = new SelectList(db.Factions, "Id", "Name", character.FactionId);
            ViewBag.RaceId = new SelectList(db.Races, "Id", "Name", character.RaceId);
            ViewBag.PlayerClassId = new SelectList(db.PlayerClasses, "Id", "Name", character.PlayerClassId);
            return View(character);
        }

        //
        // GET: /Character/Delete/5
 
        public ActionResult Delete(Guid id)
        {
            Character character = db.Characters.Find(id);
            return View(character);
        }

        //
        // POST: /Character/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {            
            Character character = db.Characters.Find(id);
            db.Characters.Remove(character);
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