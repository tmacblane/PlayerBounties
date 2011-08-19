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

		public DbSet<Bounty> Bounties
		{
			get;
			set;
		}

		public DbSet<Character> Characters
		{
			get;
			set;
		}

		public DbSet<PlayerClass> PlayerClasses
		{
			get;
			set;
		}

		public DbSet<Faction> Factions
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

		#endregion
	}
}