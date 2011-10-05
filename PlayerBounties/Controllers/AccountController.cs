using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

using PlayerBounties.Models;
using Postal;

namespace PlayerBounties.Controllers
{
	public class AccountController : Controller
	{
		#region Fields

		private Account account = new Account();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion		

		#region Type specific methods

		// GET: /Account/_Login
		public ActionResult Login()
		{
			return View();
		}

		// POST: /Account/_Login
		[HttpPost]
		public ActionResult Login(LoginModel loginModel, string returnUrl)
		{
			if(ModelState.IsValid)
			{
				if(this.account.ValidateUser(loginModel.Email, loginModel.Password))
				{
					FormsAuthentication.SetAuthCookie(loginModel.Email, true);
					if(Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
						&& !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
					{
						return Redirect(returnUrl);
					}
					else
					{
						return RedirectToAction("Dashboard", "Home");
					}
				}
				else
				{
					ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
				}
			}

			// If we got this far, something failed, redisplay form
			return View(loginModel);
		}

		// GET: /Account/LogOff
		public ActionResult LogOff()
		{
			FormsAuthentication.SignOut();

			return RedirectToAction("Index", "Home");
		}

		// GET: /Account/SignUp
		public ActionResult SignUp()
		{
			return View();
		}

		// POST: /Account/SignUp
		[HttpPost]
		public ActionResult SignUp(SignUpModel signUpModel)
		{
			try
			{
				if(ModelState.IsValid)
				{
					if(!this.account.IsUserLoginIDExist(signUpModel.Email))
					{
						this.account.AddUserToDatabase(signUpModel);
						FormsAuthentication.SetAuthCookie(signUpModel.Email, false);

						var accountId = this.account.GetUserId(signUpModel.Email);

						// Email notification
						dynamic email = new Email("Welcome");
						email.UserEmailAddress = signUpModel.Email;
						email.Send();

						return RedirectToAction("Create", "Character");
					}
					else
					{
						ModelState.AddModelError(string.Empty, "A user with this email address already exists er some junk");
					}
				}
			}
			catch
			{
				return View(signUpModel);
			}

			return View(signUpModel);
		}

		[Authorize]
		public ActionResult MyAccount()
		{
			Account userAccount = this.db.Accounts.Find(this.account.GetLoggedInUserId());
			return View(userAccount);
		}

		// GET: /Account/ChangePassword
		[Authorize]
		public ActionResult ChangePassword()
		{
			return View();
		}

		// POST: /Account/ChangePassword
		[Authorize]
		[HttpPost]
		public ActionResult ChangePassword(ChangePasswordModel account)
		{
			if(ModelState.IsValid)
			{
				// ChangePassword will throw an exception rather
				// than return false in certain failure scenarios.
				bool changePasswordSucceeded;

				try
				{
					MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
					changePasswordSucceeded = currentUser.ChangePassword(account.OldPassword, account.NewPassword);
				}
				catch(Exception)
				{
					changePasswordSucceeded = false;
				}

				if(changePasswordSucceeded)
				{
					return RedirectToAction("ChangePasswordSuccess");
				}
				else
				{
					ModelState.AddModelError(string.Empty, "The current password is incorrect or the new password is invalid.");
				}
			}

			// If we got this far, something failed, redisplay form
			return View(account);
		}

		// GET: /Account/ChangePasswordSuccess
		public ActionResult ChangePasswordSuccess()
		{
			return View();
		}

		public ActionResult _Preferences()
		{
			Account myAccount = this.db.Accounts.Find(this.account.GetLoggedInUserId());
			return PartialView(myAccount);
		}

		[HttpPost]
		public ActionResult _Preferences(Account account)
		{
			this.account = this.db.Accounts.Find(account.GetLoggedInUserId());
			this.account.EmailNotification = account.EmailNotification;			
			
			this.db.Entry(this.account).State = EntityState.Modified;
			this.db.SaveChanges();
			return RedirectToAction("MyAccount");
		}

		#endregion

		#region Helper methods

		private static string ErrorCodeToString(MembershipCreateStatus createStatus)
		{
			// See http://go.microsoft.com/fwlink/?LinkID=177550 for
			// a full list of status codes.
			switch(createStatus)
			{
				case MembershipCreateStatus.DuplicateUserName:
					return "User name already exists. Please enter a different user name.";

				case MembershipCreateStatus.DuplicateEmail:
					return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

				case MembershipCreateStatus.InvalidPassword:
					return "The password provided is invalid. Please enter a valid password value.";

				case MembershipCreateStatus.InvalidEmail:
					return "The e-mail address provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidAnswer:
					return "The password retrieval answer provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidQuestion:
					return "The password retrieval question provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidUserName:
					return "The user name provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.ProviderError:
					return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				case MembershipCreateStatus.UserRejected:
					return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				default:
					return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
			}
		}

		#endregion
	}
}