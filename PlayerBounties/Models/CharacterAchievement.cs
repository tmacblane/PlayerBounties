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
	public class CharacterAchievement
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

		public Guid CharacterId
		{
			get;
			set;
		}


		public Guid AchievementId
		{
			get;
			set;
		}

		#endregion
	}
}