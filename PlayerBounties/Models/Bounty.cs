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
	public class Bounty
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

		public int Amount
		{
			get;
			set;
		}

		public string Reason
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		[Required]
		public Guid PlacedById
		{
			get;
			set;
		}

		[Required]
		public Guid PlacedOnId
		{
			get;
			set;
		}

		public DateTime? DatePlaced
		{
			get;
			set;
		}

		public bool IsPlacementPending
		{
			get;
			set;
		}

		public DateTime? DateCompleted
		{
			get;
			set;
		}

		public bool IsCompletionPending
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		public string CharacterName(Guid characterId)
		{
			Character character = new Character();
			return character.GetCharacterById(characterId).Single().Name;
		}

		public string CharacterShard(Guid characterId)
		{
			Character character = new Character();
			return character.GetCharacterById(characterId).Single().Shard.Name;
		}

		public string CharacterAllegience(Guid characterId)
		{
			Character character = new Character();
			return character.GetCharacterById(characterId).Single().Faction.Name;
		}

		public string CharacterClass(Guid characterId)
		{
			Character character = new Character();
			return character.GetCharacterById(characterId).Single().PlayerClass.Name;
		}

		public void SetPendingPlacementToFalse(Bounty bounty)
		{
			bounty.IsPlacementPending = false;
			this.db.Entry(bounty).State = EntityState.Modified;
			this.db.SaveChanges();
		}

		#endregion
	}
}