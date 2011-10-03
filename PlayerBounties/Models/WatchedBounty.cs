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
	public class WatchedBounty
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

		public Guid BountyId
		{
			get;
			set;
		}

		public Guid AccountId
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		public IQueryable<WatchedBounty> GetWatchedBounty(Guid bountyId, Guid accountId)
		{
			return this.db.WatchedBounties.Where(b => b.BountyId == bountyId).Where(b => b.AccountId == accountId);
		}

		public bool IsBountyWatched(Guid bountyId, Guid accountId)
		{
			bool bountyWatched = false;

			if(this.GetWatchedBounty(bountyId, accountId).Count() != 0)
			{
				bountyWatched = true;
			}

			return bountyWatched;
		}

		#endregion

		public int GetBountyWatchedCount(Guid bountyId)
		{
			return this.db.WatchedBounties.Where(b => b.BountyId == bountyId).Count();
		}
	}
}