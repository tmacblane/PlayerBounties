using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlayerBounties.Models
{
	public class Shard
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

		[Display(Name = "Shard")]
		public string Name
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		public IEnumerable<Shard> GetShardsList()
		{
			return this.db.Shards.OrderBy(s => s.Name).ToList();
		}

		#endregion
	}
}