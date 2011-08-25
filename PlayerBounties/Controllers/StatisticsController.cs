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
	public class StatisticsController : Controller
	{
		#region Fields

		private Account account = new Account();
		private Bounty bounty = new Bounty();
		private Character character = new Character();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		public ActionResult Index()
		{
			return View(this.bounty);
		}

		public ActionResult BountiesSummary()
		{
			return View(this.bounty);
		}

		public ActionResult TopHunters()
		{
			return View(this.bounty);
		}

		#endregion
	}
}
