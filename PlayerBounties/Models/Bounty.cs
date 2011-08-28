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

		public Guid? KillShotImageId
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

			var bounty = this.db.Bounties.Find(bountyId);

			Character character = new Character();
			Guid characterUserId = character.GetCharacterById(bounty.PlacedById).Single().UserId;

			if(userId == characterUserId)
			{
				isBountyOwner = true;
			}

			return isBountyOwner;
		}

		public IEnumerable<Bounty> GetCompletedBounties()
		{
			return this.db.Bounties.Where(b => b.KilledById != null);
		}

		public IEnumerable<Bounty> GetActiveBounties()
		{
			return this.db.Bounties.Where(b => b.KilledById == Guid.Empty);
		}

		public IEnumerable<Bounty> GetLargestBountyPlaced()
		{
			IEnumerable<Bounty> completedBounties = this.GetCompletedBounties();

			IEnumerable<Bounty> largestBounty = from bounty in completedBounties
												orderby bounty.Amount descending
												select bounty;

			return largestBounty;
		}

		// public IEnumerable<Bounty> GetTopHuntersList()
		// {
		//     IEnumerable<Bounty> completedBounties = this.GetCompletedBounties();
		//
		//     foreach(Bounty bounty in completedBounties)
		//     {
		//         // get a count of distinct killed by id's
		//         // http://stackoverflow.com/questions/454601/how-to-count-duplicates-in-list-with-linq
		//     }
		// }

		public List<Bounty> GetAccountBountiesCompleted(Guid accountId)
		{
			Character character = new Character();

			List<Bounty> accountBounties = new List<Bounty>();

			IEnumerable<Character> accountCharacters = character.GetAllCharactersForAnAccount(accountId);

			foreach(Character accountCharacter in accountCharacters)
			{
				List<Bounty> completedBounties = this.GetBountiesCompleted(accountCharacter.Id);

				if(completedBounties.Count() != 0)
				{
					foreach(Bounty completedBounty in completedBounties)
					{
						accountBounties.Add(completedBounty);
					}
				}
			}

			return accountBounties;
		}

		public List<Bounty> GetAccountBountiesPlaced(Guid accountId)
		{
			Character character = new Character();

			List<Bounty> accountBounties = new List<Bounty>();

			IEnumerable<Character> accountCharacters = character.GetAllCharactersForAnAccount(accountId);

			foreach(Character accountCharacter in accountCharacters)
			{
				IQueryable<Bounty> completedBounties = this.GetBountiesPlaced(accountCharacter.Id);

				if(completedBounties.Count() != 0)
				{
					foreach(Bounty completedBounty in completedBounties)
					{
						accountBounties.Add(completedBounty);
					}
				}
			}

			return accountBounties;
		}

		public List<Bounty> GetAccountBountiesPlacedOn(Guid accountId)
		{
			Character character = new Character();

			List<Bounty> accountBounties = new List<Bounty>();

			IEnumerable<Character> accountCharacters = character.GetAllCharactersForAnAccount(accountId);

			foreach(Character accountCharacter in accountCharacters)
			{
				IQueryable<Bounty> completedBounties = this.GetBountiesPlacedOn(accountCharacter.Id);

				if(completedBounties.Count() != 0)
				{
					foreach(Bounty completedBounty in completedBounties)
					{
						accountBounties.Add(completedBounty);
					}
				}
			}

			return accountBounties;
		}

		public List<Bounty> GetBountiesCompleted(Guid characterId)
		{
            Bounty bounty = new Bounty();

            List<Bounty> characterCompletedBounties = new List<Bounty>();

            IEnumerable<Bounty> completedBounties = this.db.Bounties.Where(b => b.KilledById == characterId);

            if (completedBounties.Count() != 0)
            {
                foreach (Bounty completedBounty in completedBounties)
                {
                    if (bounty.GetStatus(completedBounty.Id) == "Completed")
                    {
                        characterCompletedBounties.Add(completedBounty);
                    }
                }
            }

            return characterCompletedBounties;
		}

		public int GetBountiesCompletedCount(Guid characterId)
		{
            return this.GetBountiesCompleted(characterId).Count();
		}

		public IQueryable<Bounty> GetBountiesPlaced(Guid characterId)
		{
			return this.db.Bounties.Where(b => b.PlacedById == characterId);
		}

		public int GetBountiesPlacedCount(Guid characterId)
		{
			return this.db.Bounties.Where(b => b.PlacedById == characterId).Count();
		}

		public IQueryable<Bounty> GetBountiesPlacedOn(Guid characterId)
		{
			return this.db.Bounties.Where(b => b.PlacedOnId == characterId).Where(b => b.IsPlacementPending == false);
		}

		public int GetBountiesPlacedOnCount(Guid characterId)
		{
			return this.db.Bounties.Where(b => b.PlacedOnId == characterId).Where(b => b.IsPlacementPending == false).Count();
		}

		public bool IsActiveBountyOnCharacter(Guid characterId)
		{
			bool isActiveBounty = false;

			if(this.db.Bounties.Where(b => b.PlacedOnId == characterId).Count() != 0)
			{
				if(this.db.Bounties.Where(b => b.PlacedOnId == characterId).Where(b => b.KilledById.Value == null).Count() != 0)
				{
					isActiveBounty = true;
				}
			}

			return isActiveBounty;
		}

		public string GetStatus(Guid bountyId)
		{
			Bounty bounty = this.db.Bounties.Find(bountyId);

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
			return this.db.Bounties.Where(b => b.PlacedOnId.Equals(characterId)).Where(b => b.IsCompletionPending.Equals(null)).Single().Id;
		}

		public string GetClassStyle(string className)
		{
			Character character = new Character();

			return character.GetClassStyle(className);
		}

		public string GetFactionStyle(string factionName)
		{
			Character character = new Character();

			return character.GetFactionStyle(factionName);
		}

		public List<Guid> GetAllKillShotImageIds(Guid accountId, string imageType)
		{
			Character character = new Character();
			var accountCharacters = character.GetAllCharactersForAnAccount(accountId);

			List<Guid> killShotImages = new List<Guid>();

			switch(imageType)
			{
				case "bountiesPlaced":
					foreach(Character accountCharacter in accountCharacters)
					{
						IQueryable<Bounty> completedBountiesPlacedBy = this.db.Bounties.Where(b => b.PlacedById == accountCharacter.Id).Where(b => b.IsCompletionPending == false);
						
						foreach(Bounty completedBounty in completedBountiesPlacedBy)
						{
							killShotImages.Add(completedBounty.KillShotImageId.Value);
						}
					}

					break;

				case "targetsKilled":
					foreach(Character accountCharacter in accountCharacters)
					{
						IQueryable<Bounty> completedBountiesKilledBy = this.db.Bounties.Where(b => b.KilledById == accountCharacter.Id).Where(b => b.IsCompletionPending == false);

						foreach(Bounty completedBounty in completedBountiesKilledBy)
						{
							killShotImages.Add(completedBounty.KillShotImageId.Value);
						}
					}

					break;

				case "bountiesPlacedOn":
					foreach(Character accountCharacter in accountCharacters)
					{
						IQueryable<Bounty> completedBountiesPlacedOn = this.db.Bounties.Where(b => b.PlacedOnId == accountCharacter.Id).Where(b => b.IsCompletionPending == false);

						foreach(Bounty completedBounty in completedBountiesPlacedOn)
						{
							killShotImages.Add(completedBounty.KillShotImageId.Value);
						}
					}

					break;
			}

			return killShotImages;
		}

        public List<Guid> GetAllKillShotImageIdsByCharacter(Guid characterId, string imageType)
        {
            Character character = new Character();

            List<Guid> killShotImages = new List<Guid>();

            switch (imageType)
            {
                case "bountiesPlaced":
                    IQueryable<Bounty> completedBountiesPlacedBy = this.db.Bounties.Where(b => b.PlacedById == characterId).Where(b => b.IsCompletionPending == false);

                    foreach (Bounty completedBounty in completedBountiesPlacedBy)
                    {
                        killShotImages.Add(completedBounty.KillShotImageId.Value);
                    }

                    break;

                case "targetsKilled":
                    IQueryable<Bounty> completedBountiesKilledBy = this.db.Bounties.Where(b => b.KilledById == characterId).Where(b => b.IsCompletionPending == false);

                    foreach (Bounty completedBounty in completedBountiesKilledBy)
                    {
                        killShotImages.Add(completedBounty.KillShotImageId.Value);
                    }

                    break;

                case "bountiesPlacedOn":
                    IQueryable<Bounty> completedBountiesPlacedOn = this.db.Bounties.Where(b => b.PlacedOnId == characterId).Where(b => b.IsCompletionPending == false);

                    foreach (Bounty completedBounty in completedBountiesPlacedOn)
                    {
                        killShotImages.Add(completedBounty.KillShotImageId.Value);
                    }

                    break;
            }

            return killShotImages;
        }

		public Guid GetBountyIdFromKillShotImageId(Guid killShotImageId)
		{
			return this.db.Bounties.Where(b => b.KillShotImageId == killShotImageId).First().Id;
		}

		#endregion
	}
}