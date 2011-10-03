using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;
using PlayerBounties.ViewModels;

namespace PlayerBounties.Controllers
{
	public class CharacterController : Controller
	{
		#region Fields

		private Account account = new Account();
		private Avatar avatar = new Avatar();
		private Bounty bounty = new Bounty();
		private Character character = new Character();
		private CharacterAddEditViewModel characterAddEditViewModel = new CharacterAddEditViewModel();
		private Faction faction = new Faction();
		private KillShotImage killShotImage = new KillShotImage();
		private PlayerBountyContext db = new PlayerBountyContext();
		private PlayerClass playerClass = new PlayerClass();
		private Race race = new Race();
		private Shard shard = new Shard();

		#endregion

		#region Type specific methods

		// GET: /Character/
		[Authorize]
		public ViewResult Index()
		{
			var characters = this.character.GetAllCharactersForAnAccount(this.account.GetLoggedInUserId());
			return View(characters.ToList());
		}

		// GET: /Character/Details/5
		public ViewResult Details(Guid id)
		{
			Character character = this.db.Characters.Find(id);

			return View(character);
		}

		// GET: /Character/Create
		[Authorize]
		public ActionResult Create()
		{
			return View(this.characterAddEditViewModel);
		}

		// POST: /Character/Create
		[Authorize]
		[HttpPost]
		public ActionResult Create(CharacterAddEditViewModel characterAddEditViewModel)
		{
			var accountId = this.account.GetLoggedInUserId();

			IQueryable<Character> existingCharacter = this.character.GetCharacterByName(characterAddEditViewModel.Character.Name, characterAddEditViewModel.SelectedShard, characterAddEditViewModel.SelectedFaction);

			if(existingCharacter.Count() != 0 && existingCharacter.Single().UserId != Guid.Empty)
			{
				ModelState.AddModelError("Name", "A character with this information already exists.");
			}

			if(ModelState.IsValid)
			{
				if(existingCharacter.Count() != 0 && existingCharacter.Single().UserId == Guid.Empty)
				{
					existingCharacter.Single().Bio = characterAddEditViewModel.Character.Bio;
					existingCharacter.Single().Motto = characterAddEditViewModel.Character.Motto;
					existingCharacter.Single().PlayerClassId = characterAddEditViewModel.SelectedPlayerClass;
					existingCharacter.Single().RaceId = characterAddEditViewModel.SelectedRace;

					this.Edit(existingCharacter.Single().Id);
				}
				else
				{
					this.character.Id = Guid.NewGuid();
					this.character.UserId = accountId;
					this.character.Name = characterAddEditViewModel.Character.Name;
					this.character.ShardId = characterAddEditViewModel.SelectedShard;
					this.character.FactionId = characterAddEditViewModel.SelectedFaction;

					if(characterAddEditViewModel.SelectedRace != null)
					{
						this.character.RaceId = characterAddEditViewModel.SelectedRace;
					}

					this.character.PlayerClassId = characterAddEditViewModel.SelectedPlayerClass;
					this.character.Motto = characterAddEditViewModel.Character.Motto;
					this.character.Bio = characterAddEditViewModel.Character.Bio;

					if(characterAddEditViewModel.Character.IsPrimary.Equals(true))
					{
						if(this.character.GetDefaultCharacterForAnAccount(accountId).Count() != 0)
						{
							var defaultCharacterId = this.character.GetDefaultCharacterForAnAccount(accountId).Single().Id;

							this.character.SetDefaultCharacterToFalse(defaultCharacterId);
						}
					}
					else if(this.character.GetDefaultCharacterForAnAccount(accountId).Count() == 0)
					{
						this.character.IsPrimary = true;
					}

					this.character.AvatarId = this.avatar.GetAvatarBasedOnClass(characterAddEditViewModel.SelectedPlayerClass).Single().id;

					this.db.Characters.Add(this.character);
					this.db.SaveChanges();
				}

				return RedirectToAction("Dashboard", "Home");
			}
			else
			{
				return View(characterAddEditViewModel);
			}
		}

		[HttpPost]
		public Guid CreateBountyCharacter(Character character)
		{
			if(ModelState.IsValid)
			{
				character.Id = Guid.NewGuid();
				character.UserId = Guid.Empty;
				character.AvatarId = this.avatar.GetAvatarBasedOnClass(character.PlayerClassId).Single().id;

				this.db.Characters.Add(character);
				this.db.SaveChanges();
			}

			return character.Id;
		}

		// GET: /Character/Edit/5 
		[Authorize]
		public ActionResult Edit(Guid id)
		{
			if(this.character.IsCharacterOwner(this.account.GetLoggedInUserId(), id))
			{
				CharacterAddEditViewModel characterAddEditViewModel = new CharacterAddEditViewModel();

				characterAddEditViewModel.Character = this.db.Characters.Find(id);

				var viewModel = new CharacterAddEditViewModel
				{
					Character = characterAddEditViewModel.Character,
					SelectedFaction = characterAddEditViewModel.Character.FactionId,
					SelectedPlayerClass = characterAddEditViewModel.Character.PlayerClassId,
					SelectedRace = characterAddEditViewModel.Character.RaceId,
					SelectedShard = characterAddEditViewModel.Character.ShardId
				};

				return View("Edit", viewModel);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Character/Edit/5
		[Authorize]
		[HttpPost]
		public ActionResult Edit(CharacterAddEditViewModel characterAddEditViewModel)
		{
			Character character = this.db.Characters.Find(characterAddEditViewModel.Character.Id);
			var accountId = this.account.GetLoggedInUserId();

			this.db.Entry(character).State = EntityState.Modified;

			if(this.character.IsCharacterOwner(accountId, character.Id) || this.character.UserId == Guid.Empty)
			{
				IQueryable<Character> existingCharacter = this.character.GetCharacterByName(characterAddEditViewModel.Character.Name, characterAddEditViewModel.SelectedShard, characterAddEditViewModel.SelectedFaction);

				if(existingCharacter.Count() != 0 && existingCharacter.Single().UserId != Guid.Empty && existingCharacter.Single().Id != characterAddEditViewModel.Character.Id)
				{
					ModelState.AddModelError("Name", "A character with this information already exists.");
				}

				if(ModelState.IsValid)
				{
					if(characterAddEditViewModel.Character.UserId == Guid.Empty)
					{
						character.UserId = accountId;
					}

					character.Name = characterAddEditViewModel.Character.Name;
					character.ShardId = characterAddEditViewModel.SelectedShard;
					character.FactionId = characterAddEditViewModel.SelectedFaction;

					if(characterAddEditViewModel.SelectedRace != null)
					{
						character.RaceId = characterAddEditViewModel.SelectedRace;
					}

					character.PlayerClassId = characterAddEditViewModel.SelectedPlayerClass;

					if(this.character.GetDefaultCharacterForAnAccount(accountId).Count() == 0)
					{
						character.IsPrimary = true;
					}

					character.AvatarId = this.avatar.GetAvatarBasedOnClass(characterAddEditViewModel.SelectedPlayerClass).Single().id;

					this.db.SaveChanges();
					return RedirectToAction("Dashboard", "Home");
				}
				else
				{
					var viewModel = new CharacterAddEditViewModel
					{
						Character = characterAddEditViewModel.Character,
						SelectedFaction = characterAddEditViewModel.Character.FactionId,
						SelectedPlayerClass = characterAddEditViewModel.Character.PlayerClassId,
						SelectedRace = characterAddEditViewModel.Character.RaceId,
						SelectedShard = characterAddEditViewModel.Character.ShardId
					};

					return View("Edit", viewModel);
				}
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public JsonResult LoadPlayerClassesByFaction(Guid factionId)
		{
			var playerClassList = this.GetPlayerClassesPerFaction(factionId);

			var playerClassData = playerClassList.Select(p => new SelectListItem()
			{
				Value = p.Id.ToString(),
				Text = p.Name
			});

			return Json(playerClassData, JsonRequestBehavior.AllowGet);
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public JsonResult LoadRacesByPlayerClass(Guid playerClassId)
		{
			var playerClassRaceList = this.GetRacesPerPlayerClass(playerClassId);

			var playerClassRaceData = playerClassRaceList.Select(r => new SelectListItem()
			{
				Value = r.RaceId.ToString(),
				Text = this.race.GetRaceName(r.RaceId)
			});

			return Json(playerClassRaceData.OrderBy(r => r.Text), JsonRequestBehavior.AllowGet);
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public JsonResult LoadCharactersByShard(Guid shardId)
		{
			var loggedInUserId = this.account.GetLoggedInUserId();
			var characterList = this.GetCharactersPerShard(loggedInUserId, shardId);

			var characterData = characterList.Select(c => new SelectListItem()
			{
				Value = c.Id.ToString(),
				Text = c.Name
			});

			return Json(characterData, JsonRequestBehavior.AllowGet);
		}

		public ActionResult KillShotImages(Guid characterId, string imageType)
		{
			List<KillShotImage> killShotImages = new List<KillShotImage>();
			List<Guid> killImageIds = new List<Guid>();

			killImageIds = this.bounty.GetAllKillShotImageIdsByCharacter(characterId, imageType);

			foreach(Guid killImageId in killImageIds)
			{
				killShotImages.Add(new KillShotImage
				{
					Id = this.db.KillShotImages.Find(killImageId).Id,
					FileName = this.db.KillShotImages.Find(killImageId).FileName,
					FilePath = this.db.KillShotImages.Find(killImageId).FilePath,
					ThumbnailFileName = this.db.KillShotImages.Find(killImageId).ThumbnailFileName,
					ThumbnailFilePath = this.db.KillShotImages.Find(killImageId).ThumbnailFilePath
				});
			}

			return PartialView("_KillShotImageSlider", killShotImages);
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
			return this.db.PlayerClasses.Where(p => p.FactionId == factionId).OrderBy(p => p.Name);
		}

		private IEnumerable<Character> GetCharactersPerShard(Guid accountId, Guid shardId)
		{
			return this.db.Characters.Where(c => c.UserId == accountId).Where(c => c.Shard.Id == shardId).Include(c => c.Shard).Include(c => c.Faction).Include(c => c.Race).Include(c => c.PlayerClass).OrderBy(c => c.Name);
		}

		private IEnumerable<PlayerClassRace> GetRacesPerPlayerClass(Guid playerClassId)
		{
			return this.db.PlayerClassRaces.Where(p => p.PlayerClassId == playerClassId).OrderBy(p => p.RaceId).ToList();
		}

		#endregion
	}
}