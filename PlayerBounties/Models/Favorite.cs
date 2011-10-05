using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlayerBounties.Models
{
	public class Favorite
	{
		#region Fields

		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific properties

		[Key]
		public Guid Id
		{
			get;
			set;
		}

		public Guid CharacterId
		{
			get;
			set;
		}

		public Guid AccountId
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		public IQueryable<Favorite> GetFavoritedCharacter(Guid characterId, Guid accountId)
		{
			return this.db.Favorites.Where(f => f.CharacterId == characterId).Where(f => f.AccountId == accountId);
		}

		public bool IsCharacterFavorited(Guid characterId, Guid accountId)
		{
			bool characterFavorited = false;

			if(this.GetFavoritedCharacter(characterId, accountId).Count() != 0)
			{
				characterFavorited = true;
			}

			return characterFavorited;
		}

		public IQueryable<Favorite> GetFavoritedCharacters(Guid characterId)
		{
			return this.db.Favorites.Where(f => f.CharacterId == characterId);
		}

		#endregion
	}
}