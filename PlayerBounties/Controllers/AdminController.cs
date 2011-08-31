using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;

namespace PlayerBounties.Controllers
{
	public class AdminController : Controller
	{
		#region Fields

		private Account account = new Account();
		private Bounty bounty = new Bounty();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		// GET: /Admin/
		[Authorize]
		public ActionResult Index()
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(bounty);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		#endregion
	}
}