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
    public class PlayerClassRaceController : Controller
    {
        #region Fields

        private PlayerBountyContext db = new PlayerBountyContext();

        #endregion

        #region Type specific properties

        public ActionResult Index()
        {
            PlayerClassRace playerClassRace = new PlayerClassRace();

            return View(playerClassRace);
        }

        //public ActionResult Index(PlayerClassRace playerClassRace)
        //{

        //}

        #endregion
    }
}