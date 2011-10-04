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
	public class MessageController : Controller
	{
		#region Fields

		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		// GET: /Message/
		public ViewResult Index()
		{
			return View(db.Messages.ToList());
		}

		// GET: /Message/Details/5
		public ViewResult Details(Guid id)
		{
			Message message = db.Messages.Find(id);
			return View(message);
		}

		// GET: /Message/Create
		public ActionResult Create()
		{
			return View();
		}

		// POST: /Message/Create
		[HttpPost]
		public ActionResult Create(Message message)
		{
			if(ModelState.IsValid)
			{
				message.Id = Guid.NewGuid();
				db.Messages.Add(message);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(message);
		}

		// GET: /Message/Edit/5
		public ActionResult Edit(Guid id)
		{
			Message message = db.Messages.Find(id);
			return View(message);
		}

		// POST: /Message/Edit/5
		[HttpPost]
		public ActionResult Edit(Message message)
		{
			if(ModelState.IsValid)
			{
				db.Entry(message).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(message);
		}

		// GET: /Message/Delete/5
		public ActionResult Delete(Guid id)
		{
			Message message = db.Messages.Find(id);
			return View(message);
		}

		// POST: /Message/Delete/5
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(Guid id)
		{
			Message message = db.Messages.Find(id);
			db.Messages.Remove(message);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

		#endregion

		#region Base class overrides

		protected override void Dispose(bool disposing)
		{
			db.Dispose();
			base.Dispose(disposing);
		}

		#endregion
	}
}