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
	public class FactionController : Controller
	{
		#region Fields

		private Account account = new Account();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		// GET: /Faction/
		public ActionResult Index()
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(this.db.Factions.ToList());
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Faction/Details/5
		public ActionResult Details(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Faction faction = this.db.Factions.Find(id);
				return View(faction);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Faction/Create
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

		// POST: /Faction/Create
		[HttpPost]
		public ActionResult Create(Faction faction)
		{
			if(ModelState.IsValid)
			{
				faction.Id = Guid.NewGuid();
				this.db.Factions.Add(faction);
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(faction);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Faction/Edit/5 
		public ActionResult Edit(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Faction faction = this.db.Factions.Find(id);
				return View(faction);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Faction/Edit/5
		[HttpPost]
		public ActionResult Edit(Faction faction)
		{
			if(ModelState.IsValid)
			{
				this.db.Entry(faction).State = EntityState.Modified;
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(faction);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Faction/Delete/5 
		public ActionResult Delete(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Faction faction = this.db.Factions.Find(id);
				return View(faction);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Faction/Delete/5
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Faction faction = this.db.Factions.Find(id);
				this.db.Factions.Remove(faction);
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