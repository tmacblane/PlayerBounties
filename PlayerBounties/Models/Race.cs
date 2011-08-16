using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlayerBounties.Models
{
	public class Race
	{
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
	}
}