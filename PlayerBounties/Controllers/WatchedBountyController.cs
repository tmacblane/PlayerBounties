using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;

namespace PlayerBounties.Controllers
{
	public class WatchedBountyController : Controller
	{
		#region Fields

		private PlayerBountyContext db = new PlayerBountyContext();
		private WatchedBounty watchedBounty = new WatchedBounty();

		#endregion

		#region Type specific methods

		[Authorize]
		public ActionResult Watch(Guid bountyId, Guid accountId, string view = "bountyDetails")
		{
			Bounty bounty = this.db.Bounties.Find(bountyId);

			// if already watching throw error
			//if(this.bounty.IsActiveBountyOnCharacter(character.Id) == true)
			//{
			//    ModelState.AddModelError(string.Empty, "You are already watching this bounty");
			//}

			if(ModelState.IsValid)
			{
				this.watchedBounty.Id = Guid.NewGuid();
				this.watchedBounty.BountyId = bountyId;
				this.watchedBounty.AccountId = accountId;

				this.db.WatchedBounties.Add(this.watchedBounty);
				this.db.SaveChanges();

				if(view == "bountyDetails")
				{
					return RedirectToAction("Details", "Bounty", new { id = bounty.Id });
				}
				else if(view == "bounties")
				{
					return RedirectToAction("Bounties", "Bounty");
				}
				else if(view == "searchResults")
				{
					return View("Search");
				}
			}
			else
			{
				if(view == "bountyDetails")
				{
					return RedirectToAction("Details", "Bounty", new { id = bounty.Id });
				}
				else if(view == "bounties")
				{
					return RedirectToAction("Bounties", "Bounty");
				}
				else if(view == "searchResults")
				{
					return View("Search");
				}
			}

			return RedirectToAction("Details", "Bounty", new { id = bounty.Id });
		}

		[Authorize]
		public ActionResult UnWatch(Guid bountyId, Guid accountId, string view = "bountyDetails")
		{
			Bounty bounty = this.db.Bounties.Find(bountyId);

			IQueryable<WatchedBounty> watchedBounty = this.watchedBounty.GetWatchedBounty(bountyId, accountId);

			if(watchedBounty.Count() != 0)
			{
				WatchedBounty watchedBountyToRemove = this.db.WatchedBounties.Where(b => b.BountyId == bountyId).Where(b => b.AccountId == accountId).Single();

				this.db.WatchedBounties.Remove(watchedBountyToRemove);
				this.db.SaveChanges();
			}
			else
			{
				if(view == "bountyDetails")
				{
					return RedirectToAction("Details", "Bounty", new { id = bounty.Id });
				}
				else if(view == "bounties")
				{
					return RedirectToAction("Bounties", "Bounty");
				}
				else if(view == "cancelled")
				{
				}
			}

			if(view == "bounties")
			{
				return RedirectToAction("Bounties", "Bounty");
			}
			else
			{
				return RedirectToAction("Details", "Bounty", new { id = bounty.Id });
			}
		}

		#endregion
	}
}