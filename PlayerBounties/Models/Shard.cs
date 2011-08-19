using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlayerBounties.Models
{
	public class Shard
	{
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
	}
}