﻿using System;
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
			var sortedShardList = from shard in this.db.Shards
								   orderby shard.Name ascending
								   select shard;

			var sortedFactionList = from faction in this.db.Factions
									 orderby faction.Name ascending
									 select faction;

			var sortedRaceList = from race in this.db.Races
								  orderby race.Name ascending
								  select race;

			var sortedPlayerClassList = from playerClass in this.db.PlayerClasses
										 orderby playerClass.Name ascending
										 select playerClass;

			ViewBag.ShardId = new SelectList(sortedShardList, "Id", "Name");
			ViewBag.FactionId = new SelectList(sortedFactionList, "Id", "Name");
			ViewBag.RaceId = new SelectList(sortedRaceList, "Id", "Name");
			ViewBag.PlayerClassId = new SelectList(sortedPlayerClassList, "Id", "Name");

			return View();
		}

		// POST: /Character/Create
		[Authorize]
		[HttpPost]
		public ActionResult Create(Character character)
		{
			var accountId = this.account.GetLoggedInUserId();

			if(ModelState.IsValid)
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

				this.db.Characters.Add(character);
				this.db.SaveChanges();

				return RedirectToAction("Dashboard", "Home");
			}

			ViewBag.ShardId = new SelectList(this.db.Shards, "Id", "Name", character.ShardId);
			ViewBag.FactionId = new SelectList(this.db.Factions, "Id", "Name", character.FactionId);
			ViewBag.RaceId = new SelectList(this.db.Races, "Id", "Name", character.RaceId);
			ViewBag.PlayerClassId = new SelectList(this.db.PlayerClasses, "Id", "Name", character.PlayerClassId);
			return View(character);
		}

		[HttpPost]
		public Guid CreateBountyCharacter(Character character)
		{
			if(ModelState.IsValid)
			{
				character.Id = Guid.NewGuid();
				character.UserId = Guid.Empty;
				this.db.Characters.Add(character);
				this.db.SaveChanges();
			}

			ViewBag.ShardId = new SelectList(this.db.Shards, "Id", "Name", character.ShardId);
			ViewBag.FactionId = new SelectList(this.db.Factions, "Id", "Name", character.FactionId);
			ViewBag.RaceId = new SelectList(this.db.Races, "Id", "Name", character.RaceId);
			ViewBag.PlayerClassId = new SelectList(this.db.PlayerClasses, "Id", "Name", character.PlayerClassId);

			return character.Id;
		}

		// GET: /Character/Edit/5 
		[Authorize]
		public ActionResult Edit(Guid id)
		{
			if(this.character.IsCharacterOwner(this.account.GetLoggedInUserId(), id))
			{
				Character character = this.db.Characters.Find(id);
				ViewBag.ShardId = new SelectList(this.db.Shards, "Id", "Name", character.ShardId);
				ViewBag.FactionId = new SelectList(this.db.Factions, "Id", "Name", character.FactionId);
				ViewBag.RaceId = new SelectList(this.db.Races, "Id", "Name", character.RaceId);
				ViewBag.PlayerClassId = new SelectList(this.db.PlayerClasses, "Id", "Name", character.PlayerClassId);

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
			if(this.character.IsCharacterOwner(this.account.GetLoggedInUserId(), character.Id))
			{
				if(ModelState.IsValid)
				{
					character.UserId = this.account.GetLoggedInUserId();
					this.db.Entry(character).State = EntityState.Modified;
					this.db.SaveChanges();
					return RedirectToAction("MyAccount", "Account");
				}

				ViewBag.ShardId = new SelectList(this.db.Shards, "Id", "Name", character.ShardId);
				ViewBag.FactionId = new SelectList(this.db.Factions, "Id", "Name", character.FactionId);
				ViewBag.RaceId = new SelectList(this.db.Races, "Id", "Name", character.RaceId);
				ViewBag.PlayerClassId = new SelectList(this.db.PlayerClasses, "Id", "Name", character.PlayerClassId);

				return View(character);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
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