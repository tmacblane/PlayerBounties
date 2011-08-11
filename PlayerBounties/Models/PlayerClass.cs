using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlayerBounties.Models
{
	public class PlayerClass
	{
		[Key]
		public Guid Id
		{
			get;
			set;
		}

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
	}
}