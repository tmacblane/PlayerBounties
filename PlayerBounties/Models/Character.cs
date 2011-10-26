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
	public class Character
	{
		#region Fields

		private List<SelectListItem> _characters = new List<SelectListItem>();
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

		[Required(ErrorMessage = "Character name is required.")]
		[StringLength(100, ErrorMessage = "The {0} must be less than {1} characters.")]
		public string Name
		{
			get;
			set;
		}

		[Required(ErrorMessage = "Shard is required.")]
		public Guid ShardId
		{
			get;
			set;
		}

		[Required(ErrorMessage = "Faction is required.")]
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

		[Required(ErrorMessage = "Class is required.")]
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

		[DataType(DataType.MultilineText)]
		[StringLength(4000, ErrorMessage = "The {0} must be less than {1} characters.")]
		public string Motto
		{
			get;
			set;
		}

		[DataType(DataType.MultilineText)]
		[StringLength(4000, ErrorMessage = "The {0} must be less than {1} characters.")]
		public string Bio
		{
			get;
			set;
		}

		[Display(Name = "Default")]
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

		public List<SelectListItem> GetCharacterListForAnAccount(Guid accountId)
		{
			foreach(Character item in this.GetAllCharactersForAnAccount(accountId))
			{
				_characters.Add(new SelectListItem()
				{
					Text = item.Name,
					Value = item.Id.ToString()
				});
			}

			return _characters;
		}

		public IQueryable<Character> GetCharacterById(Guid characterId)
		{
			return this.db.Characters.Where(c => c.Id == characterId).Include(c => c.Shard).Include(c => c.Faction).Include(c => c.Race).Include(c => c.PlayerClass);
		}

		public IQueryable<Character> GetCharacterByName(string name, Guid shard, Guid faction)
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

			try
			{
				if(userId == this.db.Characters.Find(characterId).UserId)
				{
					isCharacterOwner = true;
				}
			}
			catch(Exception)
			{
				isCharacterOwner = false;
			}

			return isCharacterOwner;
		}

		public bool IsCharacterFavorited(Guid characterId, Guid accountId)
		{
			bool characterFavorited = false;

			Favorite favorite = new Favorite();

			if(favorite.IsCharacterFavorited(characterId, accountId) == true)
			{
				characterFavorited = true;
			}

			return characterFavorited;
		}

		public string CharacterName(Guid characterId)
		{
			return this.db.Characters.Find(characterId).Name;
		}

		public string CharacterShard(Guid characterId)
		{
			return this.db.Characters.Where(c => c.Id == characterId).Include(c => c.Shard).Single().Shard.Name;
		}

		public string CharacterAllegience(Guid characterId)
		{
			return this.db.Characters.Where(c => c.Id == characterId).Include(c => c.Faction).Single().Faction.Name;
		}

		public string CharacterClass(Guid characterId)
		{
			return this.db.Characters.Where(c => c.Id == characterId).Include(c => c.PlayerClass).Single().PlayerClass.Name;
		}

		public string CharacterRace(Guid characterId)
		{
			return this.db.Characters.Where(c => c.Id == characterId).Include(c => c.Race).Single().Race.Name;
		}

		public List<Bounty> GetBountiesKilled(Guid characterId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetBountiesCompleted(characterId);
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

		public List<Bounty> GetAccountPendingBountiesPlaced(Guid accountId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAccountPendingBountiesPlaced(accountId);
		}

		public List<Bounty> GetAccountActiveBounties(Guid accountId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAccountActiveBounties(accountId);
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

		public List<Bounty> GetAccountBountiesSignedUpFor(Guid accountId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAccountBountiesSignedUpFor(accountId);
		}

		public int GetFavoriteCharactersCount(Guid accountId)
		{
			Favorite favorite = new Favorite();

			return this.GetFavoriteCharacters(accountId).Count();
		}

		public IQueryable<Favorite> GetFavoriteCharacters(Guid accountId)
		{
			Favorite favorite = new Favorite();

			return favorite.GetFavoriteCharacters(accountId);
		}

		public double GetAmountEarned(Guid characterId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAmountEarned(characterId);
		}

		public double GetAmountSpent(Guid characterId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAmountSpent(characterId);
		}

		public double GetAccountAmountSpent(Guid accountId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAccountAmountSpent(accountId);
		}

		public double GetAccountAmountEarned(Guid accountId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAccountAmountEarned(accountId);
		}

		public double GetAmountWorth(Guid characterId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetAmountWorth(characterId);
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
					return "republicTxt";

				case "Sith Empire":
					return "empireTxt";
			}

			return string.Empty;
		}

		public List<Guid> GetAllKillShotImageIdsForACharacter(Guid characterId, string imageType)
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
					IQueryable<Bounty> completedBountiesPlacedOn = this.db.Bounties.Where(b => b.PlacedById == characterId).Where(b => b.IsCompletionPending == false);

					foreach(Bounty completedBounty in completedBountiesPlacedOn)
					{
						killShotImages.Add(completedBounty.KillShotImageId.Value);
					}

					break;
			}

			return killShotImages;
		}

		public string GetBountyStatus(Guid bountyId)
		{
			Bounty bounty = new Bounty();

			return bounty.GetStatus(bountyId);
		}

		public bool IsBountyWatched(Guid bountyId, Guid accountId)
		{
			Bounty bounty = new Bounty();

			return bounty.IsBountyWatched(bountyId, accountId);
		}

		public Guid GetCharacterUserId(Guid characterId)
		{
			return this.db.Characters.Find(characterId).UserId;
		}

		#endregion
	}
}