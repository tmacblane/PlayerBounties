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

        public ActionResult Index()
        {
            return View(bounty);
        }

        public ActionResult BountiesSummary()
        {
            return View(bounty);
        }

        public ActionResult TopHunters()
        {
            return View(bounty);
        }
    }
}
