using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlayerBounties.Models
{
	public class Bounty
	{
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

		#endregion
	}
}