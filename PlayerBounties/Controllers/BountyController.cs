using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Helpers;
using PlayerBounties.Models;
using Postal;

namespace PlayerBounties.Controllers
{
	public class BountyController : Controller
	{
		#region Fields

		private Account account = new Account();
		private Bounty bounty = new Bounty();
		private Character character = new Character();
		private KillShotImage killShotImage = new KillShotImage();
		private PlayerBountyContext db = new PlayerBountyContext();
		private EmailNotificationHelper emailNotificationHelper = new EmailNotificationHelper();

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
			var loggedInUserId = this.account.GetLoggedInUserId();
			var characters = this.character.GetAllCharactersOnAShardForAnAccount(loggedInUserId, this.character.CharacterShard(character.Id));
			var defaultCharacter = this.character.GetDefaultCharacterForAnAccount(loggedInUserId);

			this.bounty.PlacedOnId = character.Id;

			if(defaultCharacter.Single().Shard.Name == this.bounty.CharacterShard(this.bounty.PlacedOnId))
			{
				ViewBag.CharacterList = new SelectList(characters, "Id", "Name", defaultCharacter.Single().Id);
			}
			else
			{
				ViewBag.CharacterList = new SelectList(characters, "Id", "Name");
			}

			return View(this.bounty);
		}

		// POST: /Bounty/Create
		[Authorize]
		[HttpPost]
		public ActionResult Create(Guid id, FormCollection formCollection)
		{
			Character character = this.db.Characters.Find(id);

			var accountId = this.account.GetLoggedInUserId();

			if(formCollection["Amount"] == string.Empty)
			{
				ModelState.AddModelError("Amount", "Amount is required.");
			}
			else if(int.Parse(formCollection["Amount"]) <= 0)
			{
				ModelState.AddModelError("Amount", "Amount must be greater than 0.");
			}

			if(formCollection["CharacterList"] == string.Empty)
			{
				ModelState.AddModelError("CharacterList", "You must select a character to place a bounty with.");
			}

			// Check if character has bounty on them
			if(this.bounty.IsActiveBountyOnCharacter(character.Id) == true)
			{
				ModelState.AddModelError(string.Empty, "A bounty has already been placed on this character");
			}

			if(ModelState.IsValid)
			{
				this.bounty.Id = Guid.NewGuid();

				// Set bounty details
				this.bounty.Amount = int.Parse(formCollection["Amount"]);
				this.bounty.Reason = formCollection["Reason"];
				this.bounty.Message = formCollection["Message"];
				this.bounty.PlacedById = Guid.Parse(formCollection["CharacterList"]);
				this.bounty.PlacedOnId = character.Id;
				this.bounty.DatePlaced = DateTime.Now;
				this.bounty.DateCompleted = null;
				this.bounty.IsPlacementPending = true;

				this.db.Bounties.Add(this.bounty);
				this.db.SaveChanges();

				// Admin alert email notification
				this.emailNotificationHelper.SendBountyNotificationEmail("PendingBountyPlaced-AdminAlert", this.bounty, accountId);

				// Client alert email notification
				this.emailNotificationHelper.SendBountyNotificationEmail("PendingBountyPlaced-ClientAlert", this.bounty, accountId);

				return RedirectToAction("Dashboard", "Home");
			}
			else
			{
				var loggedInUserId = this.account.GetLoggedInUserId();

				character = this.db.Characters.Where(c => c.Id == character.Id).Include(c => c.Shard).Include(c => c.Faction).Include(c => c.Race).Include(c => c.PlayerClass).Single();

				var characters = this.character.GetAllCharactersOnAShardForAnAccount(loggedInUserId, character.Shard.Name);
				var defaultCharacter = this.character.GetDefaultCharacterForAnAccount(loggedInUserId);

				this.bounty.PlacedOnId = character.Id;

				ViewBag.ShardId = new SelectList(this.db.Shards, "Id", "Name", character.ShardId);
				ViewBag.FactionId = new SelectList(this.db.Factions, "Id", "Name", character.FactionId);
				ViewBag.RaceId = new SelectList(this.db.Races, "Id", "Name", character.RaceId);
				ViewBag.PlayerClassId = new SelectList(this.db.PlayerClasses, "Id", "Name", character.PlayerClassId);

				if(defaultCharacter.Single().Shard.Name == this.bounty.CharacterShard(this.bounty.PlacedOnId))
				{
					ViewBag.CharacterList = new SelectList(characters, "Id", "Name", defaultCharacter.Single().Id);
				}
				else
				{
					ViewBag.CharacterList = new SelectList(characters, "Id", "Name");
				}

				return View(this.bounty);
			}
		}

		// GET: /Bounty/Details/5
		public ActionResult Details(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);

			if(Request.IsAuthenticated)
			{
				var loggedInUser = this.account.GetLoggedInUserId();
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

			Guid loggedInUser = Guid.Empty;
			IQueryable<Character> characters;
			IQueryable<Character> defaultCharacter;

			// Need to check to ensure the bounty is still active
			// if completed
			// ModelState.AddModelError(string.Empty, "The bounty for this target has already been submitted.");
			// else if pending completion
			// ModelState.AddModelError(string.Empty, "The bounty for this target has been submitted for approval.");

			if(formCollection["CharacterList"] == string.Empty)
			{
				ModelState.AddModelError("CharacterList", "You must select a character to place a bounty with.");
			}

			foreach(string file in Request.Files)
			{
				HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;

				if(hpf.FileName == null || hpf.FileName == string.Empty)
				{
					ModelState.AddModelError(string.Empty, "You must first upload a file to complete the bounty.");
				}
			}

			if(ModelState.IsValid)
			{
				if(Request.IsAuthenticated)
				{
					loggedInUser = this.account.GetLoggedInUserId();
					characters = this.character.GetAllCharactersOnAShardForAnAccount(loggedInUser, bounty.CharacterShard(bounty.PlacedOnId));
					defaultCharacter = this.character.GetDefaultCharacterForAnAccount(loggedInUser);

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

				bounty.KilledById = Guid.Parse(formCollection["CharacterList"]);
				bounty.IsCompletionPending = true;
				bounty.DateCompleted = DateTime.Now;

				this.db.Entry(bounty).State = EntityState.Modified;
				this.db.SaveChanges();

				this.UploadFiles(bounty);

				// Admin alert email notification
				this.emailNotificationHelper.SendBountyNotificationEmail("PendingBountyCompletion-AdminAlert", this.bounty, Guid.Empty);

				// Hunter alert email notification
				Guid hunterUserId = this.db.Characters.Find(bounty.KilledById.Value).UserId;

				this.emailNotificationHelper.SendBountyNotificationEmail("PendingBountyCompletion-HunterAlert", this.bounty, hunterUserId);

				// Hunters watching alert email notification that the bounty has been posted for completion
				IQueryable<WatchedBounty> watchedBounties = this.db.WatchedBounties.Where(b => b.BountyId == bounty.Id);

				foreach(WatchedBounty watchedBounty in watchedBounties)
				{
					this.emailNotificationHelper.SendBountyNotificationEmail("PendingBountyCompletion-WatchedAccountAlert", this.bounty, watchedBounty.AccountId);
				}

				return RedirectToAction("Dashboard", "Home", null);
			}

			if(Request.IsAuthenticated)
			{
				loggedInUser = this.account.GetLoggedInUserId();
				characters = this.character.GetAllCharactersOnAShardForAnAccount(loggedInUser, bounty.CharacterShard(bounty.PlacedOnId));
				defaultCharacter = this.character.GetDefaultCharacterForAnAccount(loggedInUser);

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

		public ActionResult Bounties()
		{
			return View(this.db.Bounties.Where(b => b.IsPlacementPending == false).Where(b => b.KilledById == null));
		}

		// GET: /Bounty/PlaceBounty
		[Authorize]
		public ActionResult PlaceBounty()
		{
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
			string characterName = string.Empty;
			Guid characterId = Guid.Empty;
			Guid factionId = Guid.Empty;
			Guid shardId = Guid.Empty;
			Guid playerClassId = Guid.Empty;

			#region Validation errors

			if(formCollection["characterNameTxt"] == string.Empty)
			{
				ModelState.AddModelError("CharacterNameRequired", "Character Name is required.");
			}
			else
			{
				characterName = formCollection["characterNameTxt"];
			}

			if(formCollection["FactionId"] == string.Empty)
			{
				ModelState.AddModelError("FactionRequired", "Faction is required.");
			}
			else
			{
				factionId = Guid.Parse(formCollection["FactionId"]);
			}

			if(formCollection["ShardId"] == string.Empty)
			{
				ModelState.AddModelError("ShardRequired", "Shard is required.");
			}
			else
			{
				shardId = Guid.Parse(formCollection["ShardId"]);
			}

			if(formCollection["PlayerClassId"] == string.Empty || formCollection["PlayerClassId"] == "0")
			{
				ModelState.AddModelError("PlayerClassRequired", "Player Class is required.");
			}
			else
			{
				playerClassId = Guid.Parse(formCollection["PlayerClassId"]);
			}

			if(formCollection["Amount"] == string.Empty)
			{
				ModelState.AddModelError("Amount", "Amount is required.");
			}
			else if(int.Parse(formCollection["Amount"]) <= 0)
			{
				ModelState.AddModelError("Amount", "Amount must be greater than 0.");
			}

			if(formCollection["CharacterList"] == string.Empty)
			{
				ModelState.AddModelError("CharacterList", "You must select a character to place a bounty with.");
			}

			#endregion

			IQueryable<Character> bountyTarget = this.character.GetCharacterByName(characterName, shardId, factionId);

			if(bountyTarget.Count() != 0 && this.bounty.IsActiveBountyOnCharacter(bountyTarget.Single().Id) == true)
			{
				ModelState.AddModelError(string.Empty, "A bounty has already been placed on this character");
			}

			if(ModelState.IsValid)
			{
				var accountId = this.account.GetLoggedInUserId();

				if(bountyTarget.Count().Equals(0))
				{
					this.character.Name = characterName;
					this.character.ShardId = shardId;
					this.character.FactionId = factionId;
					this.character.PlayerClassId = playerClassId;

					CharacterController characterController = new CharacterController();
					characterId = characterController.CreateBountyCharacter(this.character);
				}
				else
				{
					characterId = bountyTarget.Single().Id;
				}

				bounty.Id = Guid.NewGuid();
				bounty.PlacedById = Guid.Parse(formCollection["CharacterList"]);
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
				Character characterResult = this.db.Characters.Find(characterId);
				characterResult.IsBountyTarget = true;

				this.db.Entry(characterResult).State = EntityState.Modified;
				this.db.SaveChanges();

				// Character Achievement
				//var bountyPlacedCount = this.bounty.GetBountiesPlacedOnCount(bounty.PlacedById);

				//List<int> bountyPlacedAchievementList = new List<int>()
				//{	
				//    1, 5, 10, 25, 50, 100, 250, 500
				//};

				//foreach(int value in bountyPlacedAchievementList)
				//{
				//    if(value == bountyPlacedCount)
				//    {
				//        // Check if already has achievement
				//        // Add achievement value
				//    }
				//    else
				//    {
				//        break;
				//    }
				//}

				// Get amount spent for placedById
				//		If amount > achievement
				//			Add achievement

				// Account Achievement

				// Admin alert email notification
				this.emailNotificationHelper.SendBountyNotificationEmail("PendingBountyPlaced-AdminAlert", this.bounty, Guid.Empty);

				// Client alert email notification
				this.emailNotificationHelper.SendBountyNotificationEmail("PendingBountyPlaced-ClientAlert", this.bounty, accountId);

				return RedirectToAction("Dashboard", "Home");
			}

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

			ViewBag.ShardId = new SelectList(sortedShardList, "Id", "Name", shardId);
			ViewBag.FactionId = new SelectList(sortedFactionList, "Id", "Name", factionId);
			ViewBag.PlayerClassId = new SelectList(sortedPlayerClassList, "Id", "Name", playerClassId);

			ViewBag.characterNameTxt = characterName;

			if(characters.Count() != 0)
			{
				ViewBag.CharacterList = new SelectList(characters, "Id", "Name", formCollection["CharacterList"]);
			}
			else
			{
				ViewBag.CharacterList = new SelectList(characters, "Id", "Name");
			}

			return View(bounty);
		}

		public ActionResult PendingPlacement()
		{
			return View(this.bounty.GetPendingPlacementBounties());
		}

		public ActionResult ApproveBountyPlacement(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);
			var accountId = this.account.GetLoggedInUserId();

			bounty.SetPendingPlacementToFalse(bounty);

			// Client alert email notification
			this.emailNotificationHelper.SendBountyNotificationEmail("BountyPlacedApproved-ClientAlert", this.bounty, accountId);

			// Target alert email notification (if they are registered)
			if(this.db.Characters.Find(bounty.PlacedOnId).UserId != Guid.Empty)
			{
				Guid userId = this.db.Characters.Find(bounty.PlacedOnId).UserId;

				this.emailNotificationHelper.SendBountyNotificationEmail("BountyPlacedApproved-ClientAlert", this.bounty, userId);
			}

			return RedirectToAction("PendingPlacement");
		}

		public ActionResult PendingCompletion()
		{
			return View(this.bounty.GetPendingCompletionBounties());
		}

		public ActionResult ApproveBountyCompletion(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);
			var accountId = this.account.GetLoggedInUserId();

			bounty.SetPendingCompletionToFalse(bounty);

			// Client alert email notification
			this.emailNotificationHelper.SendBountyNotificationEmail("BountyCompletionApproved-ClientAlert", this.bounty, accountId);

			// Hunter alert email notification
			Guid hunterUserId = this.db.Characters.Find(bounty.KilledById.Value).UserId;

			this.emailNotificationHelper.SendBountyNotificationEmail("BountyCompletionApproved-HunterAlert", this.bounty, hunterUserId);

			// Target alert email notification (if they are registered)
			if(this.db.Characters.Find(bounty.PlacedOnId).UserId != Guid.Empty)
			{
				Guid userId = this.db.Characters.Find(bounty.PlacedOnId).UserId;

				this.emailNotificationHelper.SendBountyNotificationEmail("BountyCompletionApproved-TargetAlert", this.bounty, userId);
			}

			// Hunters watching alert email notification that the bounty has been posted for completion
			IQueryable<WatchedBounty> watchedBounties = this.db.WatchedBounties.Where(b => b.BountyId == bounty.Id);
			WatchedBountyController watchedBountyController = new WatchedBountyController();

			foreach(WatchedBounty watchedBounty in watchedBounties)
			{
				this.emailNotificationHelper.SendBountyNotificationEmail("BountyCompletionApproved-WatchedAccountAlert", this.bounty, watchedBounty.AccountId);

				// Remove watched bounty record
				watchedBountyController.UnWatch(watchedBounty.BountyId, watchedBounty.AccountId);
			}

			return RedirectToAction("PendingCompletion");
		}

		public ActionResult BountyStatistics(string statistic, Guid? id = null)
		{
			Character characterInfo = null;
			Guid loggedInUserId = Guid.Empty;

			// Get id of user logged in and assign to variable to be used in cases
			if(Request.IsAuthenticated)
			{
				loggedInUserId = this.bounty.GetLoggedInUserId();
			}

			if(id != Guid.Empty || id != null)
			{
				characterInfo = this.db.Characters.Find(id);
			}

			switch(statistic)
			{
				case "targetsKilled":

					// Guid.Empty searches for account bounty statistics
					if(id == Guid.Empty && Request.IsAuthenticated)
					{
						// assign variable to results of GetAccountBountiesCompleted
						var accountBountiesCompleted = this.bounty.GetAccountBountiesCompleted(loggedInUserId);

						// if count is not 0 return results
						if(accountBountiesCompleted.Count() != 0)
						{
							IEnumerable<Bounty> targetsKilled = accountBountiesCompleted;
							@ViewBag.Title = "Targets Killed";
							return View("_BountiesTable", targetsKilled);
						}
						else
						{
							return null;
						}
					}
					else
					{
						var characterBountiesCompleted = this.bounty.GetBountiesCompleted(id.Value);

						if(characterBountiesCompleted.Count() != 0)
						{
							IEnumerable<Bounty> targetsKilled = characterBountiesCompleted;
							@ViewBag.Title = "Targets Killed By - " + characterInfo.Name;
							return View("_BountiesTable", targetsKilled);
						}
						else
						{
							return null;
						}
					}

				case "bountiesPlaced":
					if(id == Guid.Empty && Request.IsAuthenticated)
					{
						var accountBountiesPlaced = this.bounty.GetAccountBountiesPlaced(loggedInUserId);

						if(accountBountiesPlaced.Count() != 0)
						{
							@ViewBag.Title = "Bounties Placed";
							return View("_BountiesTable", accountBountiesPlaced);
						}
						else
						{
							return null;
						}
					}
					else
					{
						var characterBountiesPlaced = this.bounty.GetBountiesPlaced(id.Value);

						if(characterBountiesPlaced.Count() != 0)
						{
							@ViewBag.Title = "Bounties Placed By - " + characterInfo.Name;
							return View("_BountiesTable", characterBountiesPlaced);
						}
						else
						{
							return null;
						}
					}

				case "pendingBountiesPlaced":
					if(id == Guid.Empty && Request.IsAuthenticated)
					{
						var accountPendingBountiesPlaced = this.bounty.GetAccountPendingBountiesPlaced(loggedInUserId);

						if(accountPendingBountiesPlaced.Count() != 0)
						{
							@ViewBag.Title = "Pending Bounties Placed";
							return View("_BountiesTable", accountPendingBountiesPlaced);
						}
						else
						{
							return null;
						}
					}
					else
					{
						var characterPendingBountiesPlaced = this.bounty.GetPendingBountiesPlaced(id.Value);

						if(characterPendingBountiesPlaced.Count() != 0)
						{
							@ViewBag.Title = "Pending Bounties Placed By - " + characterInfo.Name;
							return View("_BountiesTable", characterPendingBountiesPlaced);
						}
						else
						{
							return null;
						}
					}

				case "bountiesPlacedAgainst":
					if(id == Guid.Empty && Request.IsAuthenticated)
					{
						var accountBountiesAgainst = this.bounty.GetAccountBountiesPlacedOn(loggedInUserId);

						if(accountBountiesAgainst.Count() != 0)
						{
							@ViewBag.Title = "Bounties Placed Against";
							return View("_BountiesTable", accountBountiesAgainst);
						}
						else
						{
							return null;
						}
					}
					else
					{
						var characterBountiesAgainst = this.bounty.GetBountiesPlacedOn(id.Value);

						if(characterBountiesAgainst.Count() != 0)
						{
							@ViewBag.Title = "Bounties Placed On - " + characterInfo.Name;
							return View("_BountiesTable", characterBountiesAgainst);
						}
						else
						{
							return null;
						}
					}

				case "activeBounties":
					if(id == Guid.Empty && Request.IsAuthenticated)
					{
						var accountActiveBounties = this.bounty.GetAccountActiveBounties(loggedInUserId);

						if(accountActiveBounties.Count() != 0)
						{
							@ViewBag.Title = "Active Bounties";
							return View("_BountiesTable", accountActiveBounties);
						}
						else
						{
							return null;
						}
					}
					else
					{
						var characterActiveBounties = this.bounty.GetActiveBountiesPlaced(id.Value);

						if(characterActiveBounties.Count() != 0)
						{
							@ViewBag.Title = "Active Bounties Placed By - " + characterInfo.Name;
							return View("_BountiesTable", characterActiveBounties);
						}
						else
						{
							return null;
						}
					}

				case "bountiesSignedUpFor":
					if(id == Guid.Empty && Request.IsAuthenticated)
					{
						List<Bounty> accountBountiesSignedUpFor = this.bounty.GetAccountBountiesSignedUpFor(loggedInUserId);

						if(accountBountiesSignedUpFor.Count() != 0)
						{
							@ViewBag.Title = "Bounties Signed Up For";
							return View("_BountiesTable", accountBountiesSignedUpFor);
						}
						else
						{
							return null;
						}
					}
					else
					{
						return null;
					}

			}

			return View("_BountiesTable", this.bounty);
		}

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
			var r = new List<KillShotImage>();

			foreach(string file in Request.Files)
			{
				HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
				string convertedFileName = DateTime.Now.ToString("mmddyyyyhhmmss");

				if(hpf.ContentLength == 0)
				{
					continue;
				}

				string fileName = hpf.FileName;
				string filePath = @"Content\Images\";
				string convertedFilePath = @"Content\Images\KillShots";
				string thumbnailFilePath = @"Content\Images\Thumbnails\";

				this.killShotImage.Id = Guid.NewGuid();
				this.killShotImage.FileName = string.Concat(convertedFileName, ".jpg");
				this.killShotImage.FilePath = convertedFilePath;
				this.killShotImage.ThumbnailFileName = string.Concat(convertedFileName, "_thumbnail.jpg");
				this.killShotImage.FilePath = thumbnailFilePath;

				this.db.KillShotImages.Add(this.killShotImage);
				this.db.SaveChanges();

				bounty.KillShotImageId = this.killShotImage.Id;
				this.db.Entry(bounty).State = EntityState.Modified;
				this.db.SaveChanges();

				string savedFileName = Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory + filePath,
					Path.GetFileName(fileName));

				hpf.SaveAs(savedFileName);

				// Create renamed original image
				string savedConvertedFileName = Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory + convertedFilePath,
					Path.GetFileName(convertedFileName));

				Bitmap bitmap = new Bitmap(savedFileName);

				savedConvertedFileName = string.Concat(savedConvertedFileName, ".jpg");

				this.SaveJpegThumbnail(savedConvertedFileName, bitmap, 100);

				// Create thumbnail file and save
				string savedThumbnailFileName = Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory + thumbnailFilePath,
					Path.GetFileName(convertedFileName));

				savedThumbnailFileName = string.Concat(savedThumbnailFileName, "_thumbnail.jpg");

				Image thumbnailKillShotImage = ResizeImage(bitmap, new Size(170, 170));

				this.SaveJpegThumbnail(savedThumbnailFileName, (Bitmap)thumbnailKillShotImage, 100);
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
			Guid loggedInUserId = this.character.GetLoggedInUserId();

			return this.db.Characters.Where(c => c.UserId == loggedInUserId).Where(c => c.ShardId == shardId);
		}

		private void SaveJpegThumbnail(string path, Bitmap img, long quality)
		{
			// Encoder parameter for image quality
			EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);

			// Jpeg image codec
			ImageCodecInfo jpegCodec = this.GetEncoderInfo("image/jpeg");

			if(jpegCodec == null)
			{
				return;
			}

			EncoderParameters encoderParams = new EncoderParameters(1);
			encoderParams.Param[0] = qualityParam;

			MemoryStream memoryStream = new MemoryStream();
			FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);

			img.Save(memoryStream, jpegCodec, encoderParams);
			byte[] matrix = memoryStream.ToArray();
			fileStream.Write(matrix, 0, matrix.Length);

			memoryStream.Close();
			fileStream.Close();
		}

		private ImageCodecInfo GetEncoderInfo(string mimeType)
		{
			// Get image codecs for all image formats
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

			// Find the correct image codec
			for(int i = 0; i < codecs.Length; i++)
			{
				if(codecs[i].MimeType == mimeType)
				{
					return codecs[i];
				}
			}

			return null;
		}

		private static Image ResizeImage(Image imageToResize, Size size)
		{
			int sourceWidth = imageToResize.Width;
			int sourceHeight = imageToResize.Height;

			float percent = 0;
			float percentW = 0;
			float percentH = 0;

			percentW = (float)size.Width / (float)sourceWidth;
			percentH = (float)size.Height / (float)sourceHeight;

			if(percentH < percentW)
			{
				percent = percentH;
			}
			else
			{
				percent = percentW;
			}

			int destWidth = (int)(sourceWidth * percent);
			int destHeight = (int)(sourceHeight * percent);

			Bitmap bitmap = new Bitmap(destWidth, destHeight);
			Graphics graphics = Graphics.FromImage((Image)bitmap);

			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;

			graphics.DrawImage(imageToResize, 0, 0, destWidth, destHeight);
			graphics.Dispose();

			return (Image)bitmap;
		}

		#endregion
	}
}