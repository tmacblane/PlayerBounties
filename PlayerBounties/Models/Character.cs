using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace PlayerBounties.Models
{
	public class Character
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

		public Guid UserId
		{
			get;
			set;
		}

		[Required]
		public string Name
		{
			get;
			set;
		}

		[Required]
		public Guid ShardId
		{
			get;
			set;
		}

		[Required]
		public Guid FactionId
		{
			get;
			set;
		}

		public Guid? RaceId
		{
			get;
			set;
		}

		[Required]
		public Guid PlayerClassId
		{
			get;
			set;
		}

		public string Motto
		{
			get;
			set;
		}

		public string Bio
		{
			get;
			set;
		}

		public bool IsPrimary
		{
			get;
			set;
		}

		public bool IsBountyTarget
		{
			get;
			set;
		}

		[ForeignKey("ShardId")]
		public Shard Shard
		{
			get;
			set;
		}

		[ForeignKey("FactionId")]
		public Faction Faction
		{
			get;
			set;
		}

		[ForeignKey("RaceId")]
		public Race Race
		{
			get;
			set;
		}

		[ForeignKey("PlayerClassId")]
		public PlayerClass PlayerClass
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		public IQueryable<Character> GetAllCharactersForAnAccount(Guid accountId)
		{
			return db.Characters.Where(c => c.UserId == accountId).Include(c => c.Shard).Include(c => c.Faction).Include(c => c.Race).Include(c => c.PlayerClass);
		}

		public IQueryable<Character> GetCharacterById(Guid characterId)
		{
			return db.Characters.Where(c => c.Id == characterId).Include(c => c.Shard).Include(c => c.Faction).Include(c => c.Race).Include(c => c.PlayerClass);
		}

		public IQueryable<Character> GetCharacter(string name, Guid shard, Guid faction)
		{
			return db.Characters.Where(c => c.Name == name).Where(c => c.Shard.Id == shard).Where(c => c.Faction.Id == faction);
		}

		public IQueryable<Character> GetDefaultCharacterForAnAccount(Guid accountId)
		{
			return this.GetAllCharactersForAnAccount(accountId).Where(c => c.IsPrimary == true);
		}

		public void SetDefaultCharacterToFalse(Guid characterId)
		{
			var character = db.Characters.Find(characterId);
			character.IsPrimary = false;
			db.Entry(character).State = EntityState.Modified;
			db.SaveChanges();
		}

		#endregion
	}
}