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
        #region Fields

        private Account account = new Account();

        #endregion

        [Authorize]
        public ActionResult Dashboard()
        {
            var accountId = account.GetLoggedInUserId();

            return RedirectToAction("Dashboard", "Home", new
            {
                accountId = accountId
            });
        }
    }
}
