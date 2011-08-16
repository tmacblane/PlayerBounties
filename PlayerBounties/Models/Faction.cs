using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlayerBounties.Models
{
	public class Faction
	{
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
	}
}