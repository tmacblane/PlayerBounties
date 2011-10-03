using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;

namespace PlayerBounties.ViewModels
{
	public class BountiesViewModel
	{
		#region Fields

		private Shard shard = new Shard();

		#endregion

		#region Type specific properties

		public Bounty Bounty
		{
			get;
			set;
		}

		public Guid SelectedShard
		{
			get;
			set;
		}

		public List<SelectListItem> Shards
		{
			get
			{
				return shard.GetShardList();
			}
		}

		#endregion
	}
}