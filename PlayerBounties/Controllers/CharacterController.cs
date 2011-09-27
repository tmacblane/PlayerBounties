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
		private Bounty bounty = new Bounty();
		private KillShotImage killShotImage = new KillShotImage();
		private Character character = new Character();
		private Faction faction = new Faction();
		private PlayerClass playerClass = new PlayerClass();
		private Race race = new Race();
		private Shard shard = new Shard();
		private PlayerBountyContext db = new PlayerBountyContext();

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
			ViewBag.ShardId = new SelectList(this.shard.GetShardsList(), "Id", "Name");
			ViewBag.FactionId = new SelectList(this.faction.GetFactionsList(), "Id", "Name");
			ViewBag.RaceId = new SelectList(this.race.GetRacesList(), "Id", "Name");
			ViewBag.PlayerClassId = new SelectList(this.playerClass.GetPlayerClassesList(), "Id", "Name");

			return View();
		}

		// POST: /Character/Create
		[Authorize]
		[HttpPost]
		public ActionResult Create(Character character)
		{
			var accountId = this.account.GetLoggedInUserId();

			IQueryable<Character> existingCharacter = character.GetCharacterByName(character.Name, character.ShardId, character.FactionId);

			if(existingCharacter.Count() != 0 && existingCharacter.Single().UserId != Guid.Empty)
			{
				ModelState.AddModelError("Name", "A character with this information already exists.");
			}

			if(ModelState.IsValid)
			{
				if(existingCharacter.Count() != 0 && existingCharacter.Single().UserId == Guid.Empty)
				{
					existingCharacter.Single().Bio = character.Bio;
					existingCharacter.Single().Motto = character.Motto;
					existingCharacter.Single().PlayerClassId = character.PlayerClassId;
					existingCharacter.Single().RaceId = character.RaceId;

					this.Edit(existingCharacter.Single());
				}
				else
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
					else if(character.GetDefaultCharacterForAnAccount(accountId).Count() == 0)
					{
						character.IsPrimary = true;
					}

					// set character avatar based on class
					Avatar avatar = new Avatar();
					character.AvatarId = avatar.GetAvatarBasedOnClass(character.PlayerClassId).Single().id;

					this.db.Characters.Add(character);
					this.db.SaveChanges();
				}

				return RedirectToAction("Dashboard", "Home");
			}
			else
			{
				ViewBag.ShardId = new SelectList(this.shard.GetShardsList(), "Id", "Name", character.ShardId);
				ViewBag.FactionId = new SelectList(this.faction.GetFactionsList(), "Id", "Name", character.FactionId);
				ViewBag.RaceId = new SelectList(this.race.GetRacesList(), "Id", "Name", character.RaceId);
				ViewBag.PlayerClassId = new SelectList(this.playerClass.GetPlayerClassesList(), "Id", "Name", character.PlayerClassId);

				return View(character);
			}
		}

		[HttpPost]
		public Guid CreateBountyCharacter(Character character)
		{
			if(ModelState.IsValid)
			{
				character.Id = Guid.NewGuid();
				character.UserId = Guid.Empty;

				// set character avatar based on class
				Avatar avatar = new Avatar();
				character.AvatarId = avatar.GetAvatarBasedOnClass(character.PlayerClassId).Single().id;

				this.db.Characters.Add(character);
				this.db.SaveChanges();
			}

			ViewBag.ShardId = new SelectList(this.shard.GetShardsList(), "Id", "Name", character.ShardId);
			ViewBag.FactionId = new SelectList(this.faction.GetFactionsList(), "Id", "Name", character.FactionId);
			ViewBag.RaceId = new SelectList(this.race.GetRacesList(), "Id", "Name", character.RaceId);
			ViewBag.PlayerClassId = new SelectList(this.playerClass.GetPlayerClassesList(), "Id", "Name", character.PlayerClassId);

			return character.Id;
		}

		// GET: /Character/Edit/5 
		[Authorize]
		public ActionResult Edit(Guid id)
		{
			if(this.character.IsCharacterOwner(this.account.GetLoggedInUserId(), id))
			{
				Character character = this.db.Characters.Find(id);

				ViewBag.ShardId = new SelectList(this.shard.GetShardsList(), "Id", "Name", character.ShardId);
				ViewBag.FactionId = new SelectList(this.faction.GetFactionsList(), "Id", "Name", character.FactionId);
				ViewBag.RaceId = new SelectList(this.race.GetRacesList(), "Id", "Name", character.RaceId);
				ViewBag.PlayerClassId = new SelectList(this.playerClass.GetPlayerClassesList(), "Id", "Name", character.PlayerClassId);

				return View(character);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Character/Edit/5
		[Authorize]
		[HttpPost]
		public ActionResult Edit(Character character)
		{
			var accountId = this.account.GetLoggedInUserId();

			this.db.Entry(character).State = EntityState.Modified;

			if(this.character.IsCharacterOwner(accountId, character.Id) || this.character.UserId == Guid.Empty)
			{
				if(ModelState.IsValid)
				{
					if(character.UserId == Guid.Empty)
					{
						character.UserId = accountId;
					}

					if(character.GetDefaultCharacterForAnAccount(accountId).Count() == 0)
					{
						character.IsPrimary = true;
					}

					// set character avatar based on class
					Avatar avatar = new Avatar();
					character.AvatarId = avatar.GetAvatarBasedOnClass(character.PlayerClassId).Single().id;

					this.db.SaveChanges();
					return RedirectToAction("Dashboard", "Home");
				}

				ViewBag.ShardId = new SelectList(this.shard.GetShardsList(), "Id", "Name", character.ShardId);
				ViewBag.FactionId = new SelectList(this.faction.GetFactionsList(), "Id", "Name", character.FactionId);
				ViewBag.RaceId = new SelectList(this.race.GetRacesList(), "Id", "Name", character.RaceId);
				ViewBag.PlayerClassId = new SelectList(this.playerClass.GetPlayerClassesList(), "Id", "Name", character.PlayerClassId);

				return View(character);
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
			return this.db.PlayerClasses.Where(p => p.FactionId == factionId);
		}

		#endregion
	}
}