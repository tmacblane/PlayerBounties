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
	public class FavoriteController : Controller
	{
		#region Fields

		private Favorite favorite = new Favorite();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		[Authorize]
		public ActionResult AddToFavorites(Guid characterId, Guid accountId)
		{
			if(ModelState.IsValid)
			{
				this.favorite.Id = Guid.NewGuid();
				this.favorite.AccountId = accountId;
				this.favorite.CharacterId = characterId;

				this.db.Favorites.Add(this.favorite);
				this.db.SaveChanges();

				return RedirectToAction("Details", "Character", new { id = characterId });
			}
			else
			{
				return RedirectToAction("Details", "Character", new { id = characterId });
			}
		}

		[Authorize]
		public ActionResult RemoveFromFavorites(Guid characterId, Guid accountId)
		{
			IQueryable<Favorite> favoriteCharacter = this.favorite.GetFavoritedCharacter(characterId, accountId);

			if(favoriteCharacter.Count() != 0)
			{
				Favorite favoriteCharacterToRemove = this.db.Favorites.Where(f => f.CharacterId == characterId).Where(f => f.AccountId == accountId).Single();

				this.db.Favorites.Remove(favoriteCharacterToRemove);
				this.db.SaveChanges();
				
				return RedirectToAction("Details", "Character", new { id = characterId });
			}
			else
			{
				return RedirectToAction("Details", "Character", new { id = characterId });
			}
		}

		#endregion
	}
}