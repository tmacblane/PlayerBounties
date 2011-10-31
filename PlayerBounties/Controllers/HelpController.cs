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

using PlayerBounties.Helpers;
using PlayerBounties.Models;
using PlayerBounties.ViewModels;
using Postal;

namespace PlayerBounties.Controllers
{
    public class HelpController : Controller
    {
        #region Fields

        private EmailNotificationHelper emailNotificationHelper = new EmailNotificationHelper();
        private PlayerBountyContext db = new PlayerBountyContext();

        #endregion

        #region Type specific methods

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HelpViewModel helpViewModel)
        {
            Account account = new Account();

            if (Request.IsAuthenticated)
            {
                var loggedInUserId = account.GetLoggedInUserId();
                helpViewModel.EmailAddress = this.db.Accounts.Find(loggedInUserId).EmailAddress;
            }

            this.emailNotificationHelper.SendHelpAndSupportEmail(helpViewModel);

            return RedirectToAction("Index", "Help", null);
        }

        #endregion
    }
}
