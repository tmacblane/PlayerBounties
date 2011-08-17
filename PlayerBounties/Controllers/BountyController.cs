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

		#region Type specific methods

		// GET: /Bounty/
		public ViewResult Index()
		{
			return View(this.db.Bounties.ToList());
		}

		// GET: /Bounty/Details/5
		public ViewResult Details(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);
			return View(bounty);
		}

		// GET: /Bounty/Create/5
		[Authorize]
		public ActionResult Create(Character character)
		{
			Bounty bounty = new Bounty();
			character = this.db.Characters.Find(character.Id);

			bounty.PlacedOnId = character.Id;

			ViewBag.ShardId = new SelectList(this.db.Shards, "Id", "Name", character.ShardId);
			ViewBag.FactionId = new SelectList(this.db.Factions, "Id", "Name", character.FactionId);
			ViewBag.RaceId = new SelectList(this.db.Races, "Id", "Name", character.RaceId);
			ViewBag.PlayerClassId = new SelectList(this.db.PlayerClasses, "Id", "Name", character.PlayerClassId);

			return View(bounty);
		}

		// POST: /Character/Create
		[Authorize]
		[HttpPost]
		public ActionResult Create(Guid id, FormCollection formCollection)
		{
			Bounty bounty = new Bounty();
			Character character = this.db.Characters.Find(id);

			var accountId = this.account.GetLoggedInUserId();

			if(ModelState.IsValid)
			{
				bounty.Id = Guid.NewGuid();

				// Set bounty details
				bounty.Amount = int.Parse(formCollection["Amount"]);
				bounty.Reason = formCollection["Reason"];
				bounty.Message = formCollection["Message"];
				bounty.PlacedById = character.GetDefaultCharacterForAnAccount(accountId).Single().Id;
				bounty.PlacedOnId = character.Id;
				bounty.DatePlaced = DateTime.Now;
				bounty.DateCompleted = null;
				bounty.IsPlacementPending = true;

				this.db.Bounties.Add(bounty);
				this.db.SaveChanges();

				return RedirectToAction("Dashboard", "Home");
			}


			return View(bounty);
		}

		// GET: /Bounty/PlaceBounty
		[Authorize]
		public ActionResult PlaceBounty()
		{
			var sortedShardList = from shard in this.db.Shards
								   orderby shard.Name ascending
								   select shard;

			var sortedFactionList = from faction in this.db.Factions
									 orderby faction.Name ascending
									 select faction;

			var sortedPlayerClassList = from playerClass in this.db.PlayerClasses
										 orderby playerClass.Name ascending
										 select playerClass;

			ViewBag.ShardId = new SelectList(sortedShardList, "Id", "Name");
			ViewBag.FactionId = new SelectList(sortedFactionList, "Id", "Name");
			ViewBag.PlayerClassId = new SelectList(sortedPlayerClassList, "Id", "Name");

			return View();
		}

		// POST: /Bounty/PlaceBounty
		[Authorize]
		[HttpPost]
		public ActionResult PlaceBounty(Bounty bounty, FormCollection formCollection)
		{
			Character character = new Character();
			string characterName = formCollection["characterNameTxt"];
			Guid characterId = Guid.Empty;
			Guid shardId = Guid.Parse(formCollection["ShardId"]);
			Guid factionId = Guid.Parse(formCollection["FactionId"]);
			Guid playerClassId = Guid.Parse(formCollection["PlayerClassId"]);
			var accountId = this.account.GetLoggedInUserId();

			if(ModelState.IsValid)
			{
				bounty.Id = Guid.NewGuid();

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

				character = this.db.Characters.Find(characterId);

				if(character.IsBountyTarget == false)
				{
					// Set bounty details
					bounty.PlacedById = character.GetDefaultCharacterForAnAccount(accountId).Single().Id;
					bounty.PlacedOnId = characterId;
					bounty.DatePlaced = DateTime.Now;
					bounty.DateCompleted = null;
					bounty.IsPlacementPending = true;

					// Create bounty record
					this.db.Bounties.Add(bounty);
					this.db.SaveChanges();

					// Set character is bounty target to true and update the record
					character.IsBountyTarget = true;
					this.db.Entry(character).State = EntityState.Modified;
					this.db.SaveChanges();

					return RedirectToAction("Dashboard", "Home");
				}
				else
				{
					// alert that there is a bounty on this target
					return RedirectToAction("PlaceBounty", "Bounty", new { character });
				}
			}
			else
			{
				return RedirectToAction("PlaceBounty", "Bounty", new { character });
			}
		}

		// GET: /Bounty/Edit/5 
		public ActionResult Edit(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);
			return View(bounty);
		}

		// POST: /Bounty/Edit/5
		[HttpPost]
		public ActionResult Edit(Bounty bounty)
		{
			if(ModelState.IsValid)
			{
				this.db.Entry(bounty).State = EntityState.Modified;
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(bounty);
		}

		// GET: /Bounty/Delete/5
		public ActionResult Delete(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);
			return View(bounty);
		}

		// POST: /Bounty/Delete/5
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);
			this.db.Bounties.Remove(bounty);
			this.db.SaveChanges();
			return RedirectToAction("Index");
		}

		public ActionResult PendingPlacement()
		{
			IQueryable<Bounty> bounty = this.db.Bounties.Where(b => b.IsPlacementPending == true);

			return View(bounty);
		}

		public ActionResult ApproveBountyPlacement(Guid id)
		{
			Bounty bounty = db.Bounties.Find(id);

			bounty.SetPendingPlacementToFalse(bounty);

			return RedirectToAction("PendingPlacement");
		}

		public ActionResult ApproveBountyCompletion(Guid id)
		{
			Bounty bounty = db.Bounties.Find(id);

			bounty.SetPendingCompletionToFalse(bounty);

			return RedirectToAction("PendingPlacement");
		}

		#endregion

		#region Base class overrides

		protected override void Dispose(bool disposing)
		{
			this.db.Dispose();
			base.Dispose(disposing);
		}

		#endregion
	}
}