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

		[Required]
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

		[Display(Name = "File Name")]
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
			Character character = new Character();

			bool isBountyOwner = false;

			var bounty = this.db.Bounties.Find(bountyId);

			Guid characterUserId = character.GetCharacterById(bounty.PlacedById).Single().UserId;

			if(userId == characterUserId)
			{
				isBountyOwner = true;
			}

			return isBountyOwner;
		}

		public IQueryable<Bounty> GetPendingPlacementBounties()
		{
			return this.db.Bounties.Where(b => b.IsPlacementPending == true);
		}

		public IQueryable<Bounty> GetPendingCompletionBounties()
		{
			return this.db.Bounties.Where(b => b.IsCompletionPending == true);
		}

		public IEnumerable<Bounty> GetCompletedBounties()
		{
			return this.db.Bounties.Where(b => b.IsCompletionPending == false);
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

		public List<Guid> GetTopHuntersList()
		{
			List<Guid> characterIds = new List<Guid>();
			List<Guid> topHunters = new List<Guid>();

			IEnumerable<Bounty> completedBounties = this.GetCompletedBounties();

			foreach(Bounty bounty in completedBounties)
			{
				characterIds.Add(bounty.KilledById.Value);
			}

			var hunters = from x in characterIds
						  group x by x into g
						  let count = g.Count()
						  orderby count descending
						  select new
						  {
							  Id = g.Key
						  };

			foreach(var hunter in hunters)
			{
				topHunters.Add(hunter.Id);
			}

			return topHunters;
		}

		public List<Guid> GetTopMarksList()
		{
			List<Guid> characterIds = new List<Guid>();
			List<Guid> topMarks = new List<Guid>();

			IEnumerable<Bounty> completedBounties = this.GetCompletedBounties();

			foreach(Bounty bounty in completedBounties)
			{
				characterIds.Add(bounty.PlacedOnId);
			}

			var marks = from x in characterIds
						group x by x into g
						let count = g.Count()
						orderby count descending
						select new
						{
							Id = g.Key
						};

			foreach(var mark in marks)
			{
				topMarks.Add(mark.Id);
			}

			return topMarks;
		}

		public List<Guid> GetTopClientsList()
		{
			List<Guid> characterIds = new List<Guid>();
			List<Guid> topClients = new List<Guid>();

			IEnumerable<Bounty> completedBounties = this.GetCompletedBounties();

			foreach(Bounty bounty in completedBounties)
			{
				characterIds.Add(bounty.PlacedById);
			}

			var clients = from x in characterIds
						  group x by x into g
						  let count = g.Count()
						  orderby count descending
						  select new
						  {
							  Id = g.Key
						  };

			foreach(var client in clients)
			{
				topClients.Add(client.Id);
			}

			return topClients;
		}

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
			List<Bounty> characterCompletedBounties = new List<Bounty>();

			IEnumerable<Bounty> completedBounties = this.db.Bounties.Where(b => b.KilledById == characterId);

			if(completedBounties.Count() != 0)
			{
				foreach(Bounty completedBounty in completedBounties)
				{
					if(this.GetStatus(completedBounty.Id) == "Completed")
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

		public double GetAmountEarned(Guid characterId)
		{
			IEnumerable<Bounty> bounties = this.GetBountiesCompleted(characterId);

			double amountEarned = 0;

			foreach(var bounty in bounties)
			{
				amountEarned = amountEarned + bounty.Amount;
			}

			amountEarned = amountEarned - (amountEarned * 0.05);

			// amountEarned + Great Hunt winnings

			return amountEarned;
		}

		public double GetAmountSpent(Guid characterId)
		{
			IEnumerable<Bounty> bounties = this.GetBountiesPlaced(characterId);

			double amountSpent = 0;

			foreach(var bounty in bounties)
			{
				amountSpent = amountSpent + bounty.Amount;
			}

			return amountSpent;
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

		public List<Guid> GetMostRecentlyCompletedBounties(int count)
		{
			List<Guid> killShotImages = new List<Guid>();

			IQueryable<Bounty> completedBountiesKilledBy = this.db.Bounties.Where(b => b.IsCompletionPending == false).OrderByDescending(b => b.DateCompleted);

			foreach(Bounty completedBounty in completedBountiesKilledBy.Take(count))
			{
				killShotImages.Add(completedBounty.KillShotImageId.Value);
			}

			return killShotImages;
		}

		public string GetKillShotImage(Guid killShotImageId)
		{
			return this.db.KillShotImages.Find(killShotImageId).FileName;
		}

		public List<Guid> GetAllKillShotImageIds(string imageType)
		{
			List<Guid> killShotImages = new List<Guid>();

			switch(imageType)
			{
				case "bountiesPlaced":
					IQueryable<Bounty> completedBountiesPlacedBy = this.db.Bounties.Where(b => b.IsCompletionPending == false);

					foreach(Bounty completedBounty in completedBountiesPlacedBy)
					{
						killShotImages.Add(completedBounty.KillShotImageId.Value);
					}

					break;

				case "targetsKilled":
					IQueryable<Bounty> completedBountiesKilledBy = this.db.Bounties.Where(b => b.IsCompletionPending == false);

					foreach(Bounty completedBounty in completedBountiesKilledBy)
					{
						killShotImages.Add(completedBounty.KillShotImageId.Value);
					}

					break;

				case "bountiesPlacedOn":
					IQueryable<Bounty> completedBountiesPlacedOn = this.db.Bounties.Where(b => b.IsCompletionPending == false);

					foreach(Bounty completedBounty in completedBountiesPlacedOn)
					{
						killShotImages.Add(completedBounty.KillShotImageId.Value);
					}

					break;
			}

			return killShotImages;
		}

		public List<Guid> GetAllKillShotImageIdsByAccount(Guid accountId, string imageType)
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
			List<Guid> killShotImages = new List<Guid>();

			switch(imageType)
			{
				case "bountiesPlaced":
					IQueryable<Bounty> completedBountiesPlacedBy = this.db.Bounties.Where(b => b.PlacedById == characterId).Where(b => b.IsCompletionPending == false);

					foreach(Bounty completedBounty in completedBountiesPlacedBy)
					{
						killShotImages.Add(completedBounty.KillShotImageId.Value);
					}

					break;

				case "targetsKilled":
					IQueryable<Bounty> completedBountiesKilledBy = this.db.Bounties.Where(b => b.KilledById == characterId).Where(b => b.IsCompletionPending == false);

					foreach(Bounty completedBounty in completedBountiesKilledBy)
					{
						killShotImages.Add(completedBounty.KillShotImageId.Value);
					}

					break;

				case "bountiesPlacedOn":
					IQueryable<Bounty> completedBountiesPlacedOn = this.db.Bounties.Where(b => b.PlacedOnId == characterId).Where(b => b.IsCompletionPending == false);

					foreach(Bounty completedBounty in completedBountiesPlacedOn)
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