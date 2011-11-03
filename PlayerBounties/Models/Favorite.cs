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

		public IQueryable<Favorite> GetFavoriteCharacters(Guid accountId)
		{
			return this.db.Favorites.Where(f => f.AccountId == accountId);
		}

		public string CharacterName(Guid characterId)
		{
			Character character = new Character();

			return character.CharacterName(characterId);
		}

		public string CharacterShard(Guid characterId)
		{
			Character character = new Character();

			return character.CharacterShard(characterId);
		}

		public string CharacterAllegience(Guid characterId)
		{
			Character character = new Character();

			return character.CharacterAllegience(characterId);
		}

		public string CharacterClass(Guid characterId)
		{
			Character character = new Character();

			return character.CharacterClass(characterId);
		}

		public string GetClassStyle(string className)
		{
			Character character = new Character();

			return character.GetClassStyle(className);
		}

		public Guid GetLoggedInUserId()
		{
			return this.db.Accounts.Where(row => row.EmailAddress == System.Web.HttpContext.Current.User.Identity.Name).Single().Id;
		}

		public bool IsActiveBountyOnCharacter(Guid characterId)
		{
			Bounty bounty = new Bounty();

			return bounty.IsActiveBountyOnCharacter(characterId);
		}

		public Guid GetActiveBountyId(Guid characterId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetActiveBountyId(characterId);
		}

		#endregion
	}
}