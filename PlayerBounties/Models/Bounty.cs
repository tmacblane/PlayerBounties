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

		[Display(Name = "Amount")]
		public int Amount
		{
			get;
			set;
		}

		[Display(Name = "Reason")]
		public string Reason
		{
			get;
			set;
		}

		[Display(Name = "Message")]
		public string Message
		{
			get;
			set;
		}

		[Required]
		[Display(Name = "Placed By")]
		public Guid PlacedById
		{
			get;
			set;
		}

		[Required]
		[Display(Name = "Placed On")]
		public Guid PlacedOnId
		{
			get;
			set;
		}

		[Display(Name = "Date Placed")]
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

		[Display(Name = "Killed By")]
		public Guid? KilledById
		{
			get;
			set;
		}

		[Display(Name = "Date Completed")]
		public DateTime? DateCompleted
		{
			get;
			set;
		}

		public bool? IsCompletionPending
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

		public void SetPendingCompletionToFalse(Bounty bounty)
		{
			bounty.IsCompletionPending = false;
			this.db.Entry(bounty).State = EntityState.Modified;
			this.db.SaveChanges();
		}

		public Guid GetLoggedInUserId()
		{
			return this.db.Accounts.Where(row => row.EmailAddress == System.Web.HttpContext.Current.User.Identity.Name).Single().Id;
		}

		public bool IsBountyOwner(Guid userId, Guid bountyId)
		{
			bool isBountyOwner = false;

			var bounty = db.Bounties.Find(bountyId);

			Character character = new Character();
			Guid characterUserId = character.GetCharacterById(bounty.PlacedById).Single().UserId;

			if(userId == characterUserId)
			{
				isBountyOwner = true;
			}

			return isBountyOwner;
		}

		public int GetBountiesCompletedCount(Guid characterId)
		{
			return db.Bounties.Where(b => b.KilledById == characterId).Count();
		}

		public int GetBountiesPlacedCount(Guid characterId)
		{
			return db.Bounties.Where(b => b.PlacedById == characterId).Count();
		}

		public int GetBountiesPlacedOnCount(Guid characterId)
		{
			return db.Bounties.Where(b => b.PlacedOnId == characterId).Where(b => b.IsPlacementPending != true).Count();
		}

		public bool IsActiveBountyOnCharacter(Guid characterId)
		{
			bool isActiveBounty = false;

			if(db.Bounties.Where(b => b.PlacedOnId == characterId).Count() != 0)
			{
				if(db.Bounties.Where(b => b.PlacedOnId == characterId).Where(b => b.KilledById.Value == null).Count() != 0)
				{
					isActiveBounty = true;
				}
			}

			return isActiveBounty;
		}

		public string GetStatus(Guid bountyId)
		{
			Bounty bounty = db.Bounties.Find(bountyId);

			string bountyStatus = string.Empty;
			
			if(bounty.IsPlacementPending.Equals(true))
			{
				return bountyStatus = "Placement Pending";
			}
			else if(bounty.IsPlacementPending.Equals(false) && bounty.IsCompletionPending.Equals(null))
			{
				return bountyStatus = "Active";
			}
			else if(bounty.IsCompletionPending.Equals(true))
			{
				return bountyStatus = "Completion Pending";
			}
			else if(bounty.IsCompletionPending.Equals(false))
			{
				return bountyStatus = "Completed";
			}

			return bountyStatus;
		}

		public Guid GetActiveBountyId(Guid characterId)
		{
			return db.Bounties.Where(b => b.PlacedOnId.Equals(characterId)).Where(b => b.IsCompletionPending.Equals(null)).Single().Id;
		}

		#endregion
	}
}