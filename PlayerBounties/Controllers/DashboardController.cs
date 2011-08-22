﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;

namespace PlayerBounties.Controllers
{
	[Authorize]
	public class DashboardController : Controller
	{
		#region Fields

		private Account account = new Account();
		private Character character = new Character();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		// GET: /Dashboard/
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Characters()
		{
			var characters = this.character.GetAllCharactersForAnAccount(this.account.GetLoggedInUserId());
			return PartialView("_Characters", characters.ToList());
		}

		#endregion
	}
}