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
	public class PlayerClassController : Controller
	{
		#region Fields

		private Account account = new Account();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		// GET: /PlayerClass/
		public ActionResult Index()
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(this.db.PlayerClasses.ToList());
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}			
		}

		// GET: /PlayerClass/Details/5
		public ActionResult Details(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				PlayerClass playerclass = this.db.PlayerClasses.Find(id);
				return View(playerclass);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}	
		}

		// GET: /PlayerClass/Create
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

		// POST: /PlayerClass/Create
		[HttpPost]
		public ActionResult Create(PlayerClass playerclass)
		{
			if(ModelState.IsValid)
			{
				playerclass.Id = Guid.NewGuid();
				this.db.PlayerClasses.Add(playerclass);
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(playerclass);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}	
		}

		// GET: /PlayerClass/Edit/5
		public ActionResult Edit(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				PlayerClass playerclass = this.db.PlayerClasses.Find(id);
				return View(playerclass);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}	
		}

		// POST: /PlayerClass/Edit/5
		[HttpPost]
		public ActionResult Edit(PlayerClass playerclass)
		{
			if(ModelState.IsValid)
			{
				this.db.Entry(playerclass).State = EntityState.Modified;
				this.db.SaveChanges();
				return RedirectToAction("Index");
			}

			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				return View(playerclass);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}	
		}

		// GET: /PlayerClass/Delete/5
		public ActionResult Delete(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				PlayerClass playerclass = this.db.PlayerClasses.Find(id);
				return View(playerclass);
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}	
		}

		// POST: /PlayerClass/Delete/5
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(Guid id)
		{
			if(this.account.IsUserAdmin(this.account.GetLoggedInUserId()))
			{
				PlayerClass playerclass = this.db.PlayerClasses.Find(id);
				this.db.PlayerClasses.Remove(playerclass);
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