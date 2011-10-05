using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace PlayerBounties.Models
{
	public class PlayerBountyContext : DbContext
	{
		#region Type specific properties

		public DbSet<Account> Accounts
		{
			get;
			set;
		}

		public DbSet<Achievement> Achievements
		{
			get;
			set;
		}

		public DbSet<Avatar> Avatars
		{
			get;
			set;
		}

		public DbSet<Bounty> Bounties
		{
			get;
			set;
		}

		public DbSet<CharacterAchievement> CharacterAchievements
		{
			get;
			set;
		}

		public DbSet<Character> Characters
		{
			get;
			set;
		}

		public DbSet<Faction> Factions
		{
			get;
			set;
		}

		public DbSet<Favorite> Favorites
		{
			get;
			set;
		}

		public DbSet<KillShotImage> KillShotImages
		{
			get;
			set;
		}

		public DbSet<Message> Messages
		{
			get;
			set;
		}

		public DbSet<PlayerClass> PlayerClasses
		{
			get;
			set;
		}

		public DbSet<PlayerClassRace> PlayerClassRaces
		{
			get;
			set;
		}

		public DbSet<Race> Races
		{
			get;
			set;
		}

		public DbSet<Shard> Shards
		{
			get;
			set;
		}

		public DbSet<WatchedBounty> WatchedBounties
		{
			get;
			set;
		}

		#endregion
	}
}