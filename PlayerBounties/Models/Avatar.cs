using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlayerBounties.Models
{
	public class Avatar
	{
		#region Fields

		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific properties

		[Key]
		public Guid id
		{
			get;
			set;
		}

		[Required]
		public string FilePath
		{
			get;
			set;
		}

		[Required]
		public string FileName
		{
			get;
			set;
		}

		public Guid ClassId
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		public IQueryable<Avatar> GetAvatarBasedOnClass(Guid classId)
		{
			return this.db.Avatars.Where(a => a.ClassId == classId);
		}

		#endregion
	}
}