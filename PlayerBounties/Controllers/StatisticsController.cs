using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;
using System.Threading.Tasks;

namespace PlayerBounties.Controllers
{
	public class StatisticsController : AsyncController
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

		public void TopHuntersAsync()
		{
			AsyncManager.OutstandingOperations.Increment();
			Task.Factory.StartNew(() => this.TopHuntersHelper());
		}

		public void TopHuntersHelper()
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

			AsyncManager.Parameters["topHunters"] = topHunters;
			AsyncManager.OutstandingOperations.Decrement();
		}

		public ActionResult TopHuntersCompleted(List<Character> topHunters)
		{
			return PartialView("_TopHunters", topHunters);
		}

		public void TopMarksAsync()
		{
			AsyncManager.OutstandingOperations.Increment();
			Task.Factory.StartNew(() => this.TopMarksHelper());
		}

		public void TopMarksHelper()
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

			AsyncManager.Parameters["topMarks"] = topMarks;
			AsyncManager.OutstandingOperations.Decrement();
		}

		public ActionResult TopMarksCompleted(List<Character> topMarks)
		{
			return PartialView("_TopMarks", topMarks);
		}

		public void TopClientsAsync()
		{
			AsyncManager.OutstandingOperations.Increment();
			Task.Factory.StartNew(() => this.TopClientsHelper());
		}

		public void TopClientsHelper()
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

			AsyncManager.Parameters["topClients"] = topClients;
			AsyncManager.OutstandingOperations.Decrement();
		}

		public ActionResult TopClientsCompleted(List<Character> topClients)
		{

			return PartialView("_TopClients", topClients);
		}

		#endregion
	}
}