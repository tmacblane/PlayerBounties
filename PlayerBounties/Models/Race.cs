using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlayerBounties.Models
{
	public class Race
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

		[Display(Name = "Race")]
		public string Name
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		public IEnumerable<Race> GetRacesList()
		{
			return this.db.Races.OrderBy(r => r.Name).ToList();
		}

		#endregion
	}
}