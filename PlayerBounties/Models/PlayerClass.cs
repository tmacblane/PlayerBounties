using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlayerBounties.Models
{
	public class PlayerClass
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

		#endregion
	}
}