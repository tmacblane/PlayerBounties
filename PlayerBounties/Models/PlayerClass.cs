using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlayerBounties.Models
{
	public class PlayerClass
	{
		#region Fields

		private List<SelectListItem> _playerClasses = new List<SelectListItem>();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific properties

		[Key]
		public Guid Id
		{
			get;
			set;
		}

		[Display(Name = "Class")]
		public string Name
		{
			get;
			set;
		}

		public Guid FactionId
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

		#endregion

		#region Type specific methods

		public IEnumerable<PlayerClass> GetPlayerClassesList()
		{
			return this.db.PlayerClasses.OrderBy(p => p.Name).ToList();
		}

		public List<SelectListItem> GetPlayerClassList()
		{
			foreach(PlayerClass item in this.GetPlayerClassesList())
			{
				_playerClasses.Add(new SelectListItem()
				{
					Text = item.Name,
					Value = item.Id.ToString()
				});
			}

			return _playerClasses;
		}

		#endregion
	}
}