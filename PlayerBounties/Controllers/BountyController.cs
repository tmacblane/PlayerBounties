﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
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
		private Character character = new Character();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		// GET: /Bounty/
		public ViewResult Index()
		{
			return View(this.db.Bounties.ToList());
		}

		#region CRUD

		// GET: /Bounty/Create/5
		[Authorize]
		public ActionResult Create(Character character)
		{
			Bounty bounty = new Bounty();
			var loggedInUserId = this.account.GetLoggedInUserId();

			character = this.db.Characters.Where(c => c.Id == character.Id).Include(c => c.Shard).Include(c => c.Faction).Include(c => c.Race).Include(c => c.PlayerClass).Single();

			var characters = this.character.GetAllCharactersOnAShardForAnAccount(loggedInUserId, character.Shard.Name);
			var defaultCharacter = this.character.GetDefaultCharacterForAnAccount(loggedInUserId);
			
			bounty.PlacedOnId = character.Id;
			
			ViewBag.ShardId = new SelectList(this.db.Shards, "Id", "Name", character.ShardId);
			ViewBag.FactionId = new SelectList(this.db.Factions, "Id", "Name", character.FactionId);
			ViewBag.RaceId = new SelectList(this.db.Races, "Id", "Name", character.RaceId);
			ViewBag.PlayerClassId = new SelectList(this.db.PlayerClasses, "Id", "Name", character.PlayerClassId);

			if(defaultCharacter.Single().Shard.Name == bounty.CharacterShard(bounty.PlacedOnId))
			{
				ViewBag.CharacterList = new SelectList(characters, "Id", "Name", defaultCharacter.Single().Id);
			}
			else
			{
				ViewBag.CharacterList = new SelectList(characters, "Id", "Name");
			}

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

				// Checks if a player has a selected a character to place the bounty by
				// if no character is selected, the default character is assigned
				// otherwise, the selected player is used
				if(formCollection["CharacterList"] == string.Empty)
				{
					bounty.PlacedById = character.GetDefaultCharacterForAnAccount(accountId).Single().Id;
				}
				else
				{
					bounty.PlacedById = Guid.Parse(formCollection["CharacterList"]);
				}

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

		// GET: /Bounty/Details/5
		public ActionResult Details(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);
			var loggedInUser = this.account.GetLoggedInUserId();

			if(Request.IsAuthenticated)
			{
				var characters = this.character.GetAllCharactersOnAShardForAnAccount(loggedInUser, bounty.CharacterShard(bounty.PlacedOnId));
				var defaultCharacter = this.character.GetDefaultCharacterForAnAccount(loggedInUser);

				if(defaultCharacter.Count() != 0)
				{
					if(defaultCharacter.Single().Shard.Name == bounty.CharacterShard(bounty.PlacedOnId))
					{
						ViewBag.CharacterList = new SelectList(characters, "Id", "Name", defaultCharacter.Single().Id);
					}
					else
					{
						ViewBag.CharacterList = new SelectList(characters, "Id", "Name");
					}
				}
			}

			return View(bounty);
		}

		[Authorize]
		[HttpPost]
		public ActionResult Details(Guid id, FormCollection formCollection)
		{
			Bounty bounty = this.db.Bounties.Find(id);

			Character character = new Character();

			var accountId = this.account.GetLoggedInUserId();

			// Checks if a player has a selected a character to place the bounty by
			// if no character is selected, the default character is assigned
			// otherwise, the selected player is used
			if(formCollection["CharacterList"] == String.Empty)
			{
				bounty.KilledById = character.GetDefaultCharacterForAnAccount(accountId).Single().Id;
			}
			else
			{
				bounty.KilledById = Guid.Parse(formCollection["CharacterList"]);
			}

			bounty.IsCompletionPending = true;
			bounty.DateCompleted = DateTime.Now;

			this.db.Entry(bounty).State = EntityState.Modified;
			this.db.SaveChanges();

			this.UploadFiles(bounty);

			return RedirectToAction("Dashboard", "Home", null);
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

		#endregion
		
		// GET: /Bounty/PlaceBounty
		[Authorize]
		public ActionResult PlaceBounty()
		{
			Character character = new Character();
			var loggedInUserId = this.account.GetLoggedInUserId();

			var characters = this.character.GetAllCharactersForAnAccount(loggedInUserId);

			var defaultCharacter = this.character.GetDefaultCharacterForAnAccount(loggedInUserId);

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

			if(characters.Count() != 0)
			{
				ViewBag.CharacterList = new SelectList(characters, "Id", "Name", defaultCharacter.Single().Id);
			}
			else
			{
				ViewBag.CharacterList = new SelectList(characters, "Id", "Name");
			}

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
					bounty.KilledById = null;
					bounty.DatePlaced = DateTime.Now;
					bounty.DateCompleted = null;
					bounty.IsPlacementPending = true;
					bounty.IsCompletionPending = null;

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

		public ActionResult PendingPlacement()
		{
			IQueryable<Bounty> bounty = this.db.Bounties.Where(b => b.IsPlacementPending == true);

			return View(bounty);
		}

		public ActionResult ApproveBountyPlacement(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);

			bounty.SetPendingPlacementToFalse(bounty);

			return RedirectToAction("PendingPlacement");
		}

		public ActionResult PendingCompletion()
		{
			IQueryable<Bounty> bounty = this.db.Bounties.Where(b => b.IsCompletionPending == true);

			return View(bounty);
		}

		public ActionResult ApproveBountyCompletion(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);

			bounty.SetPendingCompletionToFalse(bounty);

			return RedirectToAction("PendingCompletion");
		}

		#region Bounty Statistics

        public ActionResult BountyStatistics(string statistic, Guid? id = null)
        {   
            Bounty bounty = new Bounty();

            switch (statistic)
            {
                case "targetsKilled":
                    if (id == Guid.Empty)
                    {
						if(bounty.GetAccountBountiesCompleted(bounty.GetLoggedInUserId()).Count() != 0)
						{
							return PartialView("_BountiesTable", bounty.GetAccountBountiesCompleted(bounty.GetLoggedInUserId()));
						}
						else
						{
							return null;
						}
                    }
                    else
					{
						if(bounty.GetBountiesCompleted(id.Value).Count() != 0)
						{
							return PartialView("_BountiesTable", bounty.GetBountiesCompleted(id.Value));
						}
						else
						{
							return null;
						}
                    }

                case "bountiesPlaced":
					if(id == Guid.Empty)
					{
						if(bounty.GetAccountBountiesPlaced(bounty.GetLoggedInUserId()).Count() != 0)
						{
							return PartialView("_BountiesTable", bounty.GetAccountBountiesPlaced(bounty.GetLoggedInUserId()));
						}
						else
						{
							return null;
						}
                    }
                    else
					{
						if(bounty.GetBountiesPlaced(id.Value).Count() != 0)
						{
							return PartialView("_BountiesTable", bounty.GetBountiesPlaced(id.Value));
						}
						else
						{
							return null;
						}
                    }

                case "bountiesPlacedAgainst":
					if(id == Guid.Empty)
					{
						if(bounty.GetAccountBountiesPlacedOn(bounty.GetLoggedInUserId()).Count() != 0)
						{
							return PartialView("_BountiesTable", bounty.GetAccountBountiesPlacedOn(bounty.GetLoggedInUserId()));
						}
						else
						{
							return null;
						}
                    }
                    else
					{
						if(bounty.GetBountiesPlacedOn(id.Value).Count() != 0)
						{
							return PartialView("_BountiesTable", bounty.GetBountiesPlacedOn(id.Value));
						}
						else
						{
							return null;
						}
                    }
            }

            return PartialView("_BountiesTable", bounty);
		}

		#endregion

		[AcceptVerbs(HttpVerbs.Get)]
		public JsonResult LoadPlayerClassesByFaction(Guid factionId)
		{
			var playerClassList = this.GetPlayerClassesPerFaction(factionId);

			var playerClassData = playerClassList.Select(p => new SelectListItem()
			{
				Value = p.Id.ToString(),
				Text = p.Name,
			});

			return Json(playerClassData, JsonRequestBehavior.AllowGet);
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public JsonResult LoadCharactersByShard(Guid shardId)
		{
			var characterList = this.GetCharactersPerShard(shardId);

			var characterData = characterList.Select(p => new SelectListItem()
			{
				Value = p.Id.ToString(),
				Text = p.Name
			});

			return Json(characterData, JsonRequestBehavior.AllowGet);
		}

		public ActionResult UploadFiles(Bounty bounty)
		{
			KillShotImage killShotImage = new KillShotImage();

			var r = new List<KillShotImage>();

			foreach(string file in Request.Files)
			{
				HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;

				if(hpf.ContentLength == 0)
				{
					continue;
				}

				string fileName = hpf.FileName;
				string filePath = @"Content\Images\";

				killShotImage.Id = Guid.NewGuid();
				killShotImage.FileName = fileName;
				killShotImage.FilePath = filePath;

				this.db.KillShotImages.Add(killShotImage);
				this.db.SaveChanges();

				bounty.KillShotImageId = killShotImage.Id;
				this.db.Entry(bounty).State = EntityState.Modified;
				this.db.SaveChanges();

				string savedFileName = Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory + filePath,
					Path.GetFileName(fileName));

				hpf.SaveAs(savedFileName);
			}

			return View();
		}

		#endregion		

		#region Base class overrides

		protected override void Dispose(bool disposing)
		{
			this.db.Dispose();
			base.Dispose(disposing);
		}

		#endregion

		#region Helper methods

		private IEnumerable<PlayerClass> GetPlayerClassesPerFaction(Guid factionId)
		{
			return this.db.PlayerClasses.Where(p => p.FactionId == factionId);
		}

		private IEnumerable<Character> GetCharactersPerShard(Guid shardId)
		{
			Character character = new Character();

			Guid loggedInUserId = character.GetLoggedInUserId();

			return this.db.Characters.Where(c => c.UserId == loggedInUserId).Where(c => c.ShardId == shardId);
		}

		#endregion
	}
}