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
		#region Fields

		private Account account = new Account();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

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
        // GET: /Bounty/PlaceBounty
		public ActionResult PlaceBounty(Character character)
		{
			var sortedShardList = (from shard in db.Shards
								   orderby shard.Name ascending
								   select shard);

			var sortedFactionList = (from faction in db.Factions
									 orderby faction.Name ascending
									 select faction);

			var sortedPlayerClassList = (from playerClass in db.PlayerClasses
										 orderby playerClass.Name ascending
										 select playerClass);

			ViewBag.ShardId = new SelectList(sortedShardList, "Id", "Name");
			ViewBag.FactionId = new SelectList(sortedFactionList, "Id", "Name");
			ViewBag.PlayerClassId = new SelectList(sortedPlayerClassList, "Id", "Name");

			return View();
		}

        //
		// POST: /Bounty/PlaceBounty
        [HttpPost]
		public ActionResult PlaceBounty(Bounty bounty, FormCollection formCollection)
        {
			string characterName = formCollection["characterNameTxt"];
			Guid characterId = Guid.Empty;
			Guid shardId = Guid.Parse(formCollection["ShardId"]);
			Guid factionId = Guid.Parse(formCollection["FactionId"]);
			Guid playerClassId = Guid.Parse(formCollection["PlayerClassId"]);
			var accountId = account.GetLoggedInUserId();

            if (ModelState.IsValid)
            {
                bounty.Id = Guid.NewGuid();

				Character character = new Character();

				if(character.GetCharacter(characterName, shardId, factionId).Count().Equals(0))
				{
					character.Name = characterName;
					character.ShardId = shardId;
					character.FactionId = factionId;
					character.PlayerClassId = playerClassId;

					CharacterController characterController = new CharacterController();
					characterId = characterController.CreateBountyCharacter(character);
				}
				else
				{
					characterId = character.GetCharacter(characterName, shardId, factionId).Single().Id;
				}


				bounty.PlacedById = character.GetDefaultCharacterForAnAccount(accountId).Single().Id;
				bounty.PlacedOnId = characterId;				
				bounty.DatePlaced = DateTime.Now;
				bounty.DateCompleted = null;
				bounty.IsPlacementPending = true;

                db.Bounties.Add(bounty);
                db.SaveChanges();

				return RedirectToAction("Dashboard", "Home");
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