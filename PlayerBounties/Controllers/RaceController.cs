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
	[Authorize]
	public class RaceController : Controller
	{
		#region Fields

		private Account account = new Account();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		// GET: /Race/
		public ActionResult Index()
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(this.db.Races.ToList());
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Race/Details/5
		public ActionResult Details(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Race race = this.db.Races.Find(id);
				return View(race);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Race/Create
		public ActionResult Create()
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View();
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Race/Create
		[HttpPost]
		public ActionResult Create(Race race)
		{
			if(ModelState.IsValid)
			{
				race.Id = Guid.NewGuid();
				this.db.Races.Add(race);
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(race);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Race/Edit/5
		public ActionResult Edit(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Race race = this.db.Races.Find(id);
				return View(race);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Race/Edit/5
		[HttpPost]
		public ActionResult Edit(Race race)
		{
			if(ModelState.IsValid)
			{
				this.db.Entry(race).State = EntityState.Modified;
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(race);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Race/Delete/5
		public ActionResult Delete(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Race race = this.db.Races.Find(id);
				return View(race);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Race/Delete/5
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Race race = this.db.Races.Find(id);
				this.db.Races.Remove(race);
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		#endregion

		#region Base class overrides

		protected override void Dispose(bool disposing)
		{
			this.db.Dispose();
			base.Dispose(disposing);
		}

		#endregion
	}
}