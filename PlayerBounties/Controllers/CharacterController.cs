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
			var sortedShardList = (from shard in db.Shards
								   orderby shard.Name ascending
								   select shard);

			var sortedFactionList = (from faction in db.Factions
								   orderby faction.Name ascending
								   select faction);

			var sortedRaceList = (from race in db.Races
								   orderby race.Name ascending
								   select race);

			var sortedPlayerClassList = (from playerClass in db.PlayerClasses
								   orderby playerClass.Name ascending
								   select playerClass);

			ViewBag.ShardId = new SelectList(sortedShardList, "Id", "Name");
			ViewBag.FactionId = new SelectList(sortedFactionList, "Id", "Name");
			ViewBag.RaceId = new SelectList(sortedRaceList, "Id", "Name");
			ViewBag.PlayerClassId = new SelectList(sortedPlayerClassList, "Id", "Name");
            
			return View();
        }

        //
        // POST: /Character/Create
        [HttpPost]
        public ActionResult Create(Character character)
        {			
			var accountId = account.GetLoggedInUserId();

            if (ModelState.IsValid)
            {
                character.Id = Guid.NewGuid();
				character.UserId = accountId;

				if(character.IsPrimary.Equals(true))
				{
					if(character.GetDefaultCharacterForAnAccount(accountId).Count() != 0)
					{
						var defaultCharacterId = character.GetDefaultCharacterForAnAccount(accountId).Single().Id;

						character.SetDefaultCharacterToFalse(defaultCharacterId);
					}
				}

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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}