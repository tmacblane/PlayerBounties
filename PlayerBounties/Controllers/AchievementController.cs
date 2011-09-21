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
	public class AchievementController : Controller
	{
		#region Fields

		private Account account = new Account();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		// GET: /Achievement/
		public ActionResult Index()
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(this.db.Achievements.ToList());
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Achievement/Details/5
		public ActionResult Details(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Achievement achievement = this.db.Achievements.Find(id);
				return View(achievement);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Achievement/Create
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

		// POST: /Achievement/Create
		[HttpPost]
		public ActionResult Create(Achievement achievement)
		{
			if(ModelState.IsValid)
			{
				achievement.Id = Guid.NewGuid();
				this.db.Achievements.Add(achievement);
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(achievement);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Achievement/Edit/5
		public ActionResult Edit(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Achievement achievement = this.db.Achievements.Find(id);
				return View(achievement);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Achievement/Edit/5
		[HttpPost]
		public ActionResult Edit(Achievement achievement)
		{
			if(ModelState.IsValid)
			{
				this.db.Entry(achievement).State = EntityState.Modified;
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(achievement);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Achievement/Delete/5
		public ActionResult Delete(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Achievement achievement = this.db.Achievements.Find(id);
				return View(achievement);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Achievement/Delete/5
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Achievement achievement = this.db.Achievements.Find(id);
				this.db.Achievements.Remove(achievement);
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