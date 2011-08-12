using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using PlayerBounties.Models;

namespace PlayerBounties.Controllers
{
    public class HeaderController : Controller
    {
		public ActionResult _Header()
		{			
			Account account = new Account();

			return View(account);
		}
    }
}
