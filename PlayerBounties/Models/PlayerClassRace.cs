using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlayerBounties.Models
{
	public class PlayerClassRace
	{
		#region Fields

		private List<SelectListItem> _playerClassRaces = new List<SelectListItem>();

		#endregion

		#region Type specific properties

		[Key]
		public Guid Id
		{
			get;
			set;
		}

		public Guid PlayerClassId
		{
			get;
			set;
		}

		public Guid RaceId
		{
			get;
			set;
		}

		#endregion
	}
}