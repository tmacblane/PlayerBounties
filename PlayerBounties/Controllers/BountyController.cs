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

		// POST: /Character/Create
		[Authorize]
		[HttpPost]
		public ActionResult Create(Guid id, FormCollection formCollection)
		{
			Character character = this.db.Characters.Find(id);

			var accountId = this.account.GetLoggedInUserId();

			if(ModelState.IsValid)
			{
				this.bounty.Id = Guid.NewGuid();

				// Set bounty details
				this.bounty.Amount = int.Parse(formCollection["Amount"]);
				this.bounty.Reason = formCollection["Reason"];
				this.bounty.Message = formCollection["Message"];

				// Checks if a player has a selected a character to place the bounty by
				// if no character is selected, the default character is assigned
				// otherwise, the selected player is used
				if(formCollection["CharacterList"] == string.Empty)
				{
					this.bounty.PlacedById = character.GetDefaultCharacterForAnAccount(accountId).Single().Id;
				}
				else
				{
					this.bounty.PlacedById = Guid.Parse(formCollection["CharacterList"]);
				}

				this.bounty.PlacedOnId = character.Id;
				this.bounty.DatePlaced = DateTime.Now;
				this.bounty.DateCompleted = null;
				this.bounty.IsPlacementPending = true;

				this.db.Bounties.Add(this.bounty);
				this.db.SaveChanges();

				// Admin alert email notification
				dynamic pendingBountyAdminAlertEmail = new Email("PendingBountyPlaced-AdminAlert");

				pendingBountyAdminAlertEmail.ClientName = character.CharacterName(this.bounty.PlacedById);
				pendingBountyAdminAlertEmail.TargetName = character.CharacterName(this.bounty.PlacedOnId);
				pendingBountyAdminAlertEmail.Amount = this.bounty.Amount;

				pendingBountyAdminAlertEmail.Send();

				// Client alert email notification
				dynamic pendingBountyClientAlertEmail = new Email("PendingBountyPlaced-ClientAlert");

				pendingBountyClientAlertEmail.UserEmailAddress = this.db.Accounts.Find(accountId).EmailAddress;
				pendingBountyClientAlertEmail.ClientName = character.CharacterName(this.bounty.PlacedById);
				pendingBountyClientAlertEmail.TargetName = character.CharacterName(this.bounty.PlacedOnId);
				pendingBountyClientAlertEmail.Amount = this.bounty.Amount;

				pendingBountyClientAlertEmail.Send();

				return RedirectToAction("Dashboard", "Home");
			}

			return View(this.bounty);
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

			if(ModelState.IsValid)
			{
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
				
				var accountId = this.account.GetLoggedInUserId();

				// Checks if a player has a selected a character to place the bounty by
				// if no character is selected, the default character is assigned
				// otherwise, the selected player is used
				if(formCollection["CharacterList"] == string.Empty)
				{
					bounty.KilledById = this.character.GetDefaultCharacterForAnAccount(accountId).Single().Id;
				}
				else
				{
					bounty.KilledById = Guid.Parse(formCollection["CharacterList"]);
				}

				foreach(string file in Request.Files)
				{
					HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;

					if(hpf.FileName == null || hpf.FileName == string.Empty)
					{
						ModelState.AddModelError(string.Empty, "You must first upload a file to complete the bounty");

						return View(bounty);
					}
				}

				bounty.IsCompletionPending = true;
				bounty.DateCompleted = DateTime.Now;

				this.db.Entry(bounty).State = EntityState.Modified;
				this.db.SaveChanges();

				this.UploadFiles(bounty);

				// Admin alert email notification
				dynamic pendingBountyCompletionAdminNotification = new Email("PendingBountyCompletion-AdminAlert");

				pendingBountyCompletionAdminNotification.ClientName = this.character.CharacterName(bounty.PlacedById);
				pendingBountyCompletionAdminNotification.TargetName = this.character.CharacterName(bounty.PlacedOnId);
				pendingBountyCompletionAdminNotification.HunterName = this.character.CharacterName(bounty.KilledById.Value);
				pendingBountyCompletionAdminNotification.Amount = bounty.Amount;

				pendingBountyCompletionAdminNotification.Send();

				// Hunter alert email notification
				dynamic pendingBountyCompletionHunterNotification = new Email("PendingBountyCompletion-HunterAlert");

				Guid hunterUserId = this.db.Characters.Find(bounty.KilledById.Value).UserId;

				pendingBountyCompletionHunterNotification.UserEmailAddress = this.db.Accounts.Find(hunterUserId).EmailAddress;
				pendingBountyCompletionHunterNotification.ClientName = this.character.CharacterName(bounty.PlacedById);
				pendingBountyCompletionHunterNotification.TargetName = this.character.CharacterName(bounty.PlacedOnId);
				pendingBountyCompletionHunterNotification.HunterName = this.character.CharacterName(bounty.KilledById.Value);
				pendingBountyCompletionHunterNotification.Amount = bounty.Amount;

				pendingBountyCompletionHunterNotification.Send();

				return RedirectToAction("Dashboard", "Home", null);
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
			string characterName = formCollection["characterNameTxt"];
			Guid characterId = Guid.Empty;
			Guid shardId = Guid.Parse(formCollection["ShardId"]);
			Guid factionId = Guid.Parse(formCollection["FactionId"]);
			Guid playerClassId = Guid.Parse(formCollection["PlayerClassId"]);
			var accountId = this.account.GetLoggedInUserId();

			if(ModelState.IsValid)
			{
				bounty.Id = Guid.NewGuid();

				if(this.character.GetCharacterByName(characterName, shardId, factionId).Count().Equals(0))
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
					characterId = this.character.GetCharacterByName(characterName, shardId, factionId).Single().Id;
				}

				Character characterResult = this.db.Characters.Find(characterId);

				if(characterResult.IsBountyTarget == false)
				{
					// Set bounty details
					bounty.PlacedById = characterResult.GetDefaultCharacterForAnAccount(accountId).Single().Id;
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
					characterResult.IsBountyTarget = true;
					this.db.Entry(characterResult).State = EntityState.Modified;
					this.db.SaveChanges();

					// Admin alert email notification
					dynamic pendingBountyAdminAlertEmail = new Email("PendingBountyPlaced-AdminAlert");

					pendingBountyAdminAlertEmail.ClientName = characterResult.CharacterName(bounty.PlacedById);
					pendingBountyAdminAlertEmail.TargetName = characterResult.CharacterName(bounty.PlacedOnId);
					pendingBountyAdminAlertEmail.Amount = bounty.Amount;

					pendingBountyAdminAlertEmail.Send();

					// Client alert email notification
					dynamic pendingBountyClientAlertEmail = new Email("PendingBountyPlaced-ClientAlert");

					pendingBountyClientAlertEmail.UserEmailAddress = this.db.Accounts.Find(accountId).EmailAddress;
					pendingBountyClientAlertEmail.ClientName = characterResult.CharacterName(bounty.PlacedById);
					pendingBountyClientAlertEmail.TargetName = characterResult.CharacterName(bounty.PlacedOnId);
					pendingBountyClientAlertEmail.Amount = bounty.Amount;

					pendingBountyClientAlertEmail.Send();

					return RedirectToAction("Dashboard", "Home");
				}
				else
				{
					// alert that there is a bounty on this target
					return RedirectToAction("PlaceBounty", "Bounty", new { character = characterResult });
				}
			}
			else
			{
				return RedirectToAction("PlaceBounty", "Bounty", new { this.character });
			}
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
			dynamic bountyPlacementApprovedClientNotification = new Email("BountyPlacedApproved-ClientAlert");

			bountyPlacementApprovedClientNotification.UserEmailAddress = this.db.Accounts.Find(accountId).EmailAddress;
			bountyPlacementApprovedClientNotification.ClientName = this.character.CharacterName(bounty.PlacedById);
			bountyPlacementApprovedClientNotification.TargetName = this.character.CharacterName(bounty.PlacedOnId);
			bountyPlacementApprovedClientNotification.Amount = bounty.Amount;

			bountyPlacementApprovedClientNotification.Send();

			// Target alert email notification (if they are registered)
			if(this.db.Characters.Find(bounty.PlacedOnId).UserId != Guid.Empty)
			{
				Guid userId = this.db.Characters.Find(bounty.PlacedOnId).UserId;

				dynamic bountyPlacementApprovedTargetNotification = new Email("BountyPlacedApproved-TargetAlert");

				bountyPlacementApprovedTargetNotification.UserEmailAddress = this.db.Accounts.Find(userId).EmailAddress;
				bountyPlacementApprovedTargetNotification.ClientName = this.character.CharacterName(bounty.PlacedById);
				bountyPlacementApprovedTargetNotification.TargetName = this.character.CharacterName(bounty.PlacedOnId);
				bountyPlacementApprovedTargetNotification.Amount = bounty.Amount;
				bountyPlacementApprovedTargetNotification.Reason = bounty.Reason;

				bountyPlacementApprovedTargetNotification.Send();
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
			dynamic bountyCompletionApprovedClientNotification = new Email("BountyCompletionApproved-ClientAlert");

			bountyCompletionApprovedClientNotification.UserEmailAddress = this.db.Accounts.Find(accountId).EmailAddress;
			bountyCompletionApprovedClientNotification.ClientName = this.character.CharacterName(bounty.PlacedById);
			bountyCompletionApprovedClientNotification.TargetName = this.character.CharacterName(bounty.PlacedOnId);
			bountyCompletionApprovedClientNotification.HunterName = this.character.CharacterName(bounty.KilledById.Value);
			bountyCompletionApprovedClientNotification.Amount = bounty.Amount;

			bountyCompletionApprovedClientNotification.Send();

			// Hunter alert email notification
			dynamic bountyCompletionApprovedHunterNotification = new Email("BountyCompletionApproved-HunterAlert");

			Guid hunterUserId = this.db.Characters.Find(bounty.KilledById.Value).UserId;

			bountyCompletionApprovedHunterNotification.UserEmailAddress = this.db.Accounts.Find(hunterUserId).EmailAddress;
			bountyCompletionApprovedHunterNotification.ClientName = this.character.CharacterName(bounty.PlacedById);
			bountyCompletionApprovedHunterNotification.TargetName = this.character.CharacterName(bounty.PlacedOnId);
			bountyCompletionApprovedHunterNotification.HunterName = this.character.CharacterName(bounty.KilledById.Value);
			bountyCompletionApprovedHunterNotification.Amount = bounty.Amount;

			bountyCompletionApprovedHunterNotification.Send();

			// Target alert email notification (if they are registered)
			if(this.db.Characters.Find(bounty.PlacedOnId).UserId != Guid.Empty)
			{
				Guid userId = this.db.Characters.Find(bounty.PlacedOnId).UserId;

				dynamic bountyPlacementApprovedTargetNotification = new Email("BountyCompletionApproved-TargetAlert");

				bountyPlacementApprovedTargetNotification.UserEmailAddress = this.db.Accounts.Find(userId).EmailAddress;
				bountyPlacementApprovedTargetNotification.ClientName = this.character.CharacterName(bounty.PlacedById);
				bountyPlacementApprovedTargetNotification.TargetName = this.character.CharacterName(bounty.PlacedOnId);
				bountyPlacementApprovedTargetNotification.HunterName = this.character.CharacterName(bounty.KilledById.Value);
				bountyPlacementApprovedTargetNotification.Amount = bounty.Amount;
				bountyPlacementApprovedTargetNotification.Reason = bounty.Reason;

				bountyPlacementApprovedTargetNotification.Send();
			}

			return RedirectToAction("PendingCompletion");
		}
		
		public ActionResult BountyStatistics(string statistic, Guid? id = null)
		{
			// Get id of user logged in and assign to variable to be used in cases
			var loggedInUserId = this.bounty.GetLoggedInUserId();
			Character characterInfo = null;

			if(id != Guid.Empty || id != null)
			{
				characterInfo = this.db.Characters.Find(id);
			}

			switch(statistic)
			{
				case "targetsKilled":
					
					// Guid.Empty searches for account bounty statistics
					if(id == Guid.Empty)
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
					if(id == Guid.Empty)
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

				case "bountiesPlacedAgainst":
					if(id == Guid.Empty)
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

				Image thumbnailKillShotImage = ResizeImage(bitmap, new Size(125, 125));

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