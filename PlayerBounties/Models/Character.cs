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

		public Guid? AvatarId
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
		[Display(Name = "Class")]
		public PlayerClass PlayerClass
		{
			get;
			set;
		}

		[ForeignKey("AvatarId")]
		[Display(Name = "Avatar")]
		public Avatar Avatar
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		public IQueryable<Character> GetAllCharactersForAnAccount(Guid accountId)
		{
			return this.db.Characters.Where(c => c.UserId == accountId).Include(c => c.Shard).Include(c => c.Faction).Include(c => c.Race).Include(c => c.PlayerClass);
		}

		public IQueryable<Character> GetCharacterById(Guid characterId)
		{
			return this.db.Characters.Where(c => c.Id == characterId).Include(c => c.Shard).Include(c => c.Faction).Include(c => c.Race).Include(c => c.PlayerClass);
		}

		public IQueryable<Character> GetCharacter(string name, Guid shard, Guid faction)
		{
			return this.db.Characters.Where(c => c.Name == name).Where(c => c.Shard.Id == shard).Where(c => c.Faction.Id == faction);
		}

		public IQueryable<Character> GetDefaultCharacterForAnAccount(Guid accountId)
		{
			return this.GetAllCharactersForAnAccount(accountId).Where(c => c.IsPrimary == true);
		}

		public IQueryable<Character> GetAllCharactersOnAShardForAnAccount(Guid accountId, string shard)
		{
			return this.db.Characters.Where(c => c.UserId == accountId).Where(c => c.Shard.Name == shard).Include(c => c.Shard).Include(c => c.Faction).Include(c => c.Race).Include(c => c.PlayerClass);
		}

		public void SetDefaultCharacterToFalse(Guid characterId)
		{
			var character = this.db.Characters.Find(characterId);
			character.IsPrimary = false;
			this.db.Entry(character).State = EntityState.Modified;
			this.db.SaveChanges();
		}

		public Guid GetLoggedInUserId()
		{
			try
			{
				return this.db.Accounts.Where(row => row.EmailAddress == System.Web.HttpContext.Current.User.Identity.Name).Single().Id;
			}
			catch(Exception)
			{
				return Guid.Empty;
			}
		}

		public bool IsCharacterOwner(Guid userId, Guid characterId)
		{
			bool isCharacterOwner = false;

			var character = this.db.Characters.Find(characterId);

			try
			{
				if(userId == character.UserId)
				{
					isCharacterOwner = true;
				}
			}
			catch(Exception)
			{
			}

			return isCharacterOwner;
		}

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

		public string CharacterRace(Guid characterId)
		{
			Character character = new Character();
			return character.GetCharacterById(characterId).Single().Race.Name;
		}

		public int GetBountiesKilledCount(Guid characterId)
		{
			Bounty bounty = new Bounty();
			
			return bounty.GetBountiesCompletedCount(characterId);
		}
		
		public List<Bounty> GetAccountBountiesCompleted(Guid accountId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAccountBountiesCompleted(accountId);
		}

		public List<Bounty> GetAccountBountiesPlaced(Guid accountId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAccountBountiesPlaced(accountId);
		}

		public List<Bounty> GetAccountBountiesPlacedOn(Guid accountId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAccountBountiesPlacedOn(accountId);
		}

		public int GetBountiesPlacedCount(Guid characterId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetBountiesPlacedCount(characterId);
		}

		public int GetBountiesPlacedOnCount(Guid characterId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetBountiesPlacedOnCount(characterId);
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

		public Avatar GetClassAvatar(Guid avatarId)
		{
			return this.db.Avatars.Find(avatarId);
		}

		public string GetClassAvatarPath(Guid avatarId)
		{
			Avatar avatar = this.GetClassAvatar(avatarId);

			string FilePath = avatar.FilePath;
			string FileName = avatar.FileName;

			return FilePath + FileName;
		}

		public string GetClassStyle(string className)
		{
			switch(className)
			{
				case "Smuggler":
					return "smugglerTxt";

				case "Republic Trooper":
					return "trooperTxt";

				case "Jedi Knight":
					return "jediKnightTxt";

				case "Jedi Consular":
					return "jediConsularTxt";

				case "Bounty Hunter":
					return "bountyHunterTxt";

				case "Imperial Agent":
					return "imperialAgentTxt";

				case "Sith Warrior":
					return "sithWarriorTxt";

				case "Sith Inquisitor":
					return "sithInquisitorTxt";
			}

			return string.Empty;
		}

		public string GetFactionStyle(string factionName)
		{
			switch(factionName)
			{
				case "Galactic Republic":
					return "republic";

				case "Sith Empire":
					return "empire";
			}
			return string.Empty;
		}

		public List<Guid> GetAllKillShotImageIdsForACharacter(Guid characterId)
		{
			List<Guid> killShotImages = new List<Guid>();

			// find all bounties that have been placed by and placed on with a status of "complete"
			IQueryable<Bounty> completedBountiesPlacedOn = this.db.Bounties.Where(b => b.PlacedById == characterId).Where(b => b.IsCompletionPending == false);
			IQueryable<Bounty> completedBountiesPlacedBy = this.db.Bounties.Where(b => b.PlacedById == characterId).Where(b => b.IsCompletionPending == false);
			IQueryable<Bounty> completedBountiesKilledBy = this.db.Bounties.Where(b => b.KilledById == characterId).Where(b => b.IsCompletionPending == false);

			foreach(Bounty completedBounty in completedBountiesPlacedOn)
			{
				killShotImages.Add(completedBounty.KillShotImageId.Value);
			}

			foreach(Bounty completedBounty in completedBountiesPlacedBy)
			{
				killShotImages.Add(completedBounty.KillShotImageId.Value);
			}

			foreach(Bounty completedBounty in completedBountiesKilledBy)
			{
				killShotImages.Add(completedBounty.KillShotImageId.Value);
			}

			return killShotImages;
		}
		


		#endregion
	}
}