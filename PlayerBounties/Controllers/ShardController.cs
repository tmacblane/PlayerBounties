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
	public class ShardController : Controller
	{
		#region Fields

		private Account account = new Account();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		// GET: /Shard/
		public ActionResult Index()
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(this.db.Shards.ToList());
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Shard/Details/5
		public ActionResult Details(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Shard shard = this.db.Shards.Find(id);
				return View(shard);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Shard/Create
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

		// POST: /Shard/Create
		[HttpPost]
		public ActionResult Create(Shard shard)
		{
			if(ModelState.IsValid)
			{
				shard.Id = Guid.NewGuid();
				this.db.Shards.Add(shard);
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(shard);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Shard/Edit/5
		public ActionResult Edit(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Shard shard = this.db.Shards.Find(id);
				return View(shard);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Shard/Edit/5
		[HttpPost]
		public ActionResult Edit(Shard shard)
		{
			if(ModelState.IsValid)
			{
				this.db.Entry(shard).State = EntityState.Modified;
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(shard);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// GET: /Shard/Delete/5
		public ActionResult Delete(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Shard shard = this.db.Shards.Find(id);
				return View(shard);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
		}

		// POST: /Shard/Delete/5
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				Shard shard = this.db.Shards.Find(id);
				this.db.Shards.Remove(shard);
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