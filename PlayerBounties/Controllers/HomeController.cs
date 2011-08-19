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
	public class HomeController : Controller
	{
		#region Fields

		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		public ActionResult Index()
		{
			ViewBag.Message = "Coming Soon!";

			return View();
		}

		public ActionResult About()
		{
			return View();
		}

		public ActionResult Rules()
		{
			return View();
		}

		public ActionResult Statistics()
		{
			return View();
		}

		[Authorize]
		public ActionResult Dashboard()
		{
			return View();
		}

		#endregion
	}
}