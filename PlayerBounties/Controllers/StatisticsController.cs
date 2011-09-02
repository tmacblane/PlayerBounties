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
			List<Guid> characterIds = this.bounty.GetTopHuntersList();
			List<Character> topHunters = new List<Character>();

			foreach(Guid characterId in characterIds.Take(5))
			{
				topHunters.Add(new Character
				{
					Id = this.db.Characters.Find(characterId).Id,
					Name = this.db.Characters.Find(characterId).Name
				});
			}

			return PartialView("_TopHunters", topHunters);
		}

		public ActionResult TopMarks()
		{
			List<Guid> characterIds = this.bounty.GetTopMarksList();
			List<Character> topMarks = new List<Character>();

			foreach(Guid characterId in characterIds.Take(5))
			{
				topMarks.Add(new Character
				{
					Id = this.db.Characters.Find(characterId).Id,
					Name = this.db.Characters.Find(characterId).Name
				});
			}

			return PartialView("_TopMarks", topMarks);
		}

		public ActionResult TopClients()
		{
			List<Guid> characterIds = this.bounty.GetTopClientsList();
			List<Character> topClients = new List<Character>();

			foreach(Guid characterId in characterIds.Take(5))
			{
				topClients.Add(new Character
				{
					Id = this.db.Characters.Find(characterId).Id,
					Name = this.db.Characters.Find(characterId).Name
				});
			}

			return PartialView("_TopClients", topClients);
		}

		#endregion
	}
}
