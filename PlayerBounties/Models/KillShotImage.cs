using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
namespace PlayerBounties.Models
{
	public class KillShotImage
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

		public string FilePath
		{
			get;
			set;
		}

		public string FileName
		{
			get;
			set;
		}

		#endregion
	}
}