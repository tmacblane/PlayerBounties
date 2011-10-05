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
using PlayerBounties.ViewModels;
using Postal;

namespace PlayerBounties.Controllers
{
	public class BountyController : Controller
	{
		#region Fields

		private Account account = new Account();
		private BountiesViewModel bountiesViewModel = new BountiesViewModel();
		private Bounty bounty = new Bounty();
		private BountyCreateViewModel bountyCreateViewModel = new BountyCreateViewModel();
		private Character character = new Character();
		private CharacterAddEditViewModel characterAddEditViewModel = new CharacterAddEditViewModel();
		private EmailNotificationHelper emailNotificationHelper = new EmailNotificationHelper();
		private KillShotImage killShotImage = new KillShotImage();
		private Message message = new Message();
		private PlaceBountyViewModel placeBountyViewModel = new PlaceBountyViewModel();
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
			this.characterAddEditViewModel.Character = this.db.Characters.Find(character.Id);

			var viewModel = new BountyCreateViewModel
			{
				Character = this.characterAddEditViewModel.Character
			};

			return View(viewModel);
		}

		// POST: /Bounty/Create
		[Authorize]
		[HttpPost]
		public ActionResult Create(Guid id, BountyCreateViewModel bountyCreateViewModel)
		{
			Character character = this.db.Characters.Find(id);
			var accountId = this.account.GetLoggedInUserId();
			
			// Check if character has bounty on them
			if(this.bounty.IsActiveBountyOnCharacter(character.Id) == true)
			{
				ModelState.AddModelError(string.Empty, "A bounty has already been placed on this character");
			}

			if(ModelState.IsValid)
			{
				this.bounty.Id = Guid.NewGuid();
				this.bounty.Amount = bountyCreateViewModel.Bounty.Amount;
				this.bounty.Reason = bountyCreateViewModel.Bounty.Reason;
				this.bounty.Message = bountyCreateViewModel.Bounty.Message;
				this.bounty.PlacedById = bountyCreateViewModel.SelectedCharacter;
				this.bounty.PlacedOnId = character.Id;
				this.bounty.DatePlaced = DateTime.Now;
				this.bounty.IsPlacementPending = true;
				this.bounty.DateCompleted = null;
				this.bounty.IsCompletionPending = null;
				this.bounty.KilledById = null;
				this.bounty.KillShotImageId = null;

				this.db.Bounties.Add(this.bounty);
				this.db.SaveChanges();

				// Send email notification
				this.emailNotificationHelper.SendBountyNotificationEmail(this.bounty, "Pending Placement");

				// Add notification message
				this.message.AddBountyNotificationMessage(this.bounty, "Pending Placement");

				return RedirectToAction("Dashboard", "Home");
			}
			else
			{
				this.characterAddEditViewModel.Character = this.db.Characters.Find(character.Id);

				var viewModel = new BountyCreateViewModel
				{
					Character = this.characterAddEditViewModel.Character,
					SelectedCharacter = bountyCreateViewModel.SelectedCharacter
				};

				return View(viewModel);
			}
		}

		// GET: /Bounty/Details/5
		public ActionResult Details(Guid id)
		{
			Bounty bounty = this.db.Bounties.Find(id);

			if(Request.IsAuthenticated)
			{
				string shardName = this.bounty.CharacterShard(bounty.PlacedOnId);

				var viewModel = new BountyDetailModel
				{
					Bounty = bounty,
					Character = this.characterAddEditViewModel.Character,
					SelectedShard = this.db.Shards.Where(s => s.Name == shardName).SingleOrDefault().Id
				};

				return View(viewModel);
			}
			else
			{
				var viewModel = new BountyDetailModel
				{
					Bounty = bounty
				};

				return View(viewModel);
			}
		}

		[Authorize]
		[HttpPost]
		public ActionResult Details(Guid id, BountyDetailModel bountyDetailModel)
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
			
			foreach(string file in Request.Files)
			{
				HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;

				if(hpf.FileName == null || hpf.FileName == string.Empty)
				{
					ModelState.AddModelError(string.Empty, "You must first upload a file to complete the bounty.");
				}
			}

			ModelState["KillShotImageId"].Errors.Clear();

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

				bounty.KilledById = bountyDetailModel.SelectedCharacter;
				bounty.IsCompletionPending = true;
				bounty.DateCompleted = DateTime.Now;

				this.db.Entry(bounty).State = EntityState.Modified;
				this.db.SaveChanges();

				this.UploadFiles(bounty);

				// Send email notification
				this.emailNotificationHelper.SendBountyNotificationEmail(bounty, "Pending Completion");

				// Add notification message
				this.message.AddBountyNotificationMessage(bounty, "Pending Completion");

				return RedirectToAction("Dashboard", "Home", null);
			}
			else
			{				
				string shardName = this.bounty.CharacterShard(bounty.PlacedOnId);

				var viewModel = new BountyDetailModel
				{
					Bounty = bounty,
					Character = this.characterAddEditViewModel.Character,
					SelectedShard = this.db.Shards.Where(s => s.Name == shardName).SingleOrDefault().Id,
					SelectedCharacter = bountyDetailModel.SelectedCharacter
				};

				return View("Details", viewModel);
			}
		}

		// GET: /Bounty/Edit/5
		[Authorize]
		public ActionResult Edit(Guid id)
		{
			if(this.bounty.IsBountyOwner(this.account.GetLoggedInUserId(), id))
			{
				BountyCreateViewModel bountyCreateViewModel = new BountyCreateViewModel();

				bountyCreateViewModel.Bounty = this.db.Bounties.Find(id);

				this.characterAddEditViewModel.Character = this.db.Characters.Find(bountyCreateViewModel.Bounty.PlacedOnId);

				var viewModel = new BountyCreateViewModel
				{
					Bounty = bountyCreateViewModel.Bounty,
					Character = this.characterAddEditViewModel.Character,
					SelectedCharacter = bountyCreateViewModel.Bounty.PlacedById,
				};

				return View("Edit", viewModel);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Bounty/Edit/5
		[HttpPost]
		public ActionResult Edit(BountyCreateViewModel bountyCreateViewModel)
		{
			var loggedInUserId = this.bounty.GetLoggedInUserId();
			bountyCreateViewModel.Bounty = this.db.Bounties.Find(bountyCreateViewModel.Bounty.Id);
			bountyCreateViewModel.Character = this.db.Characters.Find(bountyCreateViewModel.Bounty.PlacedOnId);

			ModelState["Character.Name"].Errors.Clear();

			if(this.bounty.IsBountyOwner(loggedInUserId, bountyCreateViewModel.Bounty.Id) && this.bounty.GetStatus(bountyCreateViewModel.Bounty.Id) == "Placement Pending")
			{
				if(ModelState.IsValid)
				{
					this.bounty = bountyCreateViewModel.Bounty;
					this.db.Entry(this.bounty).State = EntityState.Modified;

					this.bounty.PlacedOnId = bountyCreateViewModel.Bounty.PlacedOnId;
					this.bounty.Amount = bountyCreateViewModel.Bounty.Amount;
					this.bounty.Reason = bountyCreateViewModel.Bounty.Reason;
					this.bounty.Message = bountyCreateViewModel.Bounty.Message;
					this.bounty.PlacedById = bountyCreateViewModel.SelectedCharacter;

					this.db.SaveChanges();
					return RedirectToAction("Details", new { id = bountyCreateViewModel.Bounty.Id });
				}
				else
				{
					var viewModel = new BountyCreateViewModel
					{
						Bounty = bountyCreateViewModel.Bounty,
						Character = bountyCreateViewModel.Character,
						SelectedCharacter = bountyCreateViewModel.Bounty.PlacedById
					};

					return View("Edit", viewModel);
				}
			}
			else
			{
				return RedirectToAction("Details", new { id = bountyCreateViewModel.Bounty.Id });
			}
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
			var viewModel = new BountiesViewModel
			{
				Bounty = this.bounty
			};

			return View(viewModel);
		}

		// GET: /Bounty/PlaceBounty
		[Authorize]
		public ActionResult PlaceBounty()
		{
			return View(this.placeBountyViewModel);
		}

		// POST: /Bounty/PlaceBounty
		[Authorize]
		[HttpPost]
		public ActionResult PlaceBounty(PlaceBountyViewModel placeBountyViewModel)
		{
			string characterName = string.Empty;
			Guid characterId = Guid.Empty;
			Guid factionId = Guid.Empty;
			Guid shardId = Guid.Empty;
			Guid playerClassId = Guid.Empty;
			
			IQueryable<Character> bountyTarget = this.character.GetCharacterByName(placeBountyViewModel.Character.Name, placeBountyViewModel.SelectedShard, placeBountyViewModel.SelectedFaction);

			if(bountyTarget.Count() != 0 && this.bounty.IsActiveBountyOnCharacter(bountyTarget.Single().Id) == true)
			{
				ModelState.AddModelError(string.Empty, "A bounty has already been placed on this character");
			}

			if(ModelState.IsValid)
			{
				var accountId = this.account.GetLoggedInUserId();

				if(bountyTarget.Count().Equals(0))
				{
					this.character.Name = placeBountyViewModel.Character.Name;
					this.character.ShardId = placeBountyViewModel.SelectedShard;
					this.character.FactionId = placeBountyViewModel.SelectedFaction;
					this.character.PlayerClassId = placeBountyViewModel.SelectedPlayerClass;

					CharacterController characterController = new CharacterController();
					characterId = characterController.CreateBountyCharacter(this.character);
				}
				else
				{
					characterId = bountyTarget.Single().Id;
				}

				this.bounty.Id = Guid.NewGuid();
				this.bounty.Amount = placeBountyViewModel.Bounty.Amount;
				this.bounty.Reason = placeBountyViewModel.Bounty.Reason;
				this.bounty.Message = placeBountyViewModel.Bounty.Message;
				this.bounty.PlacedById = placeBountyViewModel.SelectedCharacter;
				this.bounty.PlacedOnId = characterId;
				this.bounty.DatePlaced = DateTime.Now;
				this.bounty.IsPlacementPending = true;
				this.bounty.DateCompleted = null;
				this.bounty.IsCompletionPending = null;
				this.bounty.KilledById = null;
				this.bounty.KillShotImageId = null;

				// Create bounty record
				this.db.Bounties.Add(this.bounty);
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

				// Send email notification
				this.emailNotificationHelper.SendBountyNotificationEmail(this.bounty, "Pending Placement");

				// Add notification message
				this.message.AddBountyNotificationMessage(this.bounty, "Pending Placement");

				return RedirectToAction("Dashboard", "Home");
			}

			return View(placeBountyViewModel);
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

			// Send email notification
			this.emailNotificationHelper.SendBountyNotificationEmail(bounty, "Placement Approved");

			// Add notification message
			this.message.AddBountyNotificationMessage(bounty, "Placement Approved");

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

			this.bounty.SetPendingCompletionToFalse(bounty);

			// Add notification message
			this.message.AddBountyNotificationMessage(bounty, "Completion Approved");

			// Send email notification
			this.emailNotificationHelper.SendBountyNotificationEmail(bounty, "Completion Approved");
			
			IQueryable<WatchedBounty> watchedBounties = this.db.WatchedBounties.Where(b => b.BountyId == bounty.Id);
			WatchedBountyController watchedBountyController = new WatchedBountyController();
						
			// Remove watched bounty record
			foreach(WatchedBounty watchedBounty in watchedBounties)
			{
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