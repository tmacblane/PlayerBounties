using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlayerBounties.Models
{
	public class Faction
	{
		#region Fields

		private List<SelectListItem> _factions = new List<SelectListItem>();
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
		[Display(Name = "Faction")]
		public string Name
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		public IEnumerable<Faction> GetFactionsList()
		{
			return this.db.Factions.OrderBy(f => f.Name).ToList();
		}

		public List<SelectListItem> GetFactionList()
		{
			foreach(Faction item in this.GetFactionsList())
			{
				_factions.Add(new SelectListItem()
				{
					Text = item.Name,
					Value = item.Id.ToString()
				});
			}

			return _factions;
		}

		#endregion
	}
}