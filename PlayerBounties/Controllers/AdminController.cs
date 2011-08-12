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
    public class AdminController : Controller
	{
		#region Fields

		private Account account = new Account();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

        //
        // GET: /Admin/
		[Authorize]
        public ActionResult Index()
        {
			if(account.IsUserAdmin(account.GetLoggedInUserId()))
			{
				return View();
			}
			else
			{
				return RedirectToAction("Dashboard", "Home");
			}
        }
    }
}