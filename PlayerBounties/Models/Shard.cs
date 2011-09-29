using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlayerBounties.Models
{
	public class Shard
	{
		#region Fields

		private List<SelectListItem> _shards = new List<SelectListItem>();
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

		public List<SelectListItem> GetShardList()
		{
			foreach(Shard item in this.GetShardsList())
			{
				_shards.Add(new SelectListItem()
				{
					Text = item.Name,
					Value = item.Id.ToString()
				});
			}

			return _shards;
		}

		#endregion
	}
}