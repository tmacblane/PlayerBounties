using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

using PlayerBounties.Helpers;
using PlayerBounties.Models;
using Postal;

namespace PlayerBounties.Controllers
{
	public class AccountController : Controller
	{
		#region Fields

		private Account account = new Account();
		private EmailNotificationHelper emailNotificationHelper = new EmailNotificationHelper();
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
						ModelState.AddModelError(string.Empty, "A user with this email address already exists");
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
		public ActionResult _ChangePassword()
		{
			return View();
		}

		// POST: /Account/ChangePassword
		[Authorize]
		[HttpPost]
		public ActionResult _ChangePassword(ChangePasswordModel passwordModel)
		{
			Account account = new Account();
			var loggedInUserId = this.account.GetLoggedInUserId();
			account = this.db.Accounts.Find(loggedInUserId);

			if(this.account.ValidateUser(account.EmailAddress, passwordModel.OldPassword) == false)
			{
				ModelState.AddModelError(string.Empty, "Please enter the correct current password.");
			}

			if(ModelState.IsValid)
			{
				if(passwordModel.NewPassword == passwordModel.ConfirmPassword)
				{
					this.account = this.db.Accounts.Find(account.GetLoggedInUserId());
					this.account.Password = account.GetPasswordHash(passwordModel.NewPassword);

					this.db.Entry(this.account).State = EntityState.Modified;
					this.db.SaveChanges();
				}

				return RedirectToAction("ChangePasswordSuccess");
			}
			else
			{

				var viewModel = new ChangePasswordModel
				{
					OldPassword = string.Empty,
					NewPassword = string.Empty,
					ConfirmPassword = string.Empty
				};

				return View(viewModel);

				// return RedirectToAction("MyAccount");
			}
		}

		// GET: /Account/ChangePasswordSuccess
		[Authorize]
		public ActionResult ChangePasswordSuccess()
		{
			return View();
		}

		public ActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		public ActionResult ForgotPassword(ForgotPasswordModel forgotPassword)
		{
			var userAccount = this.db.Accounts.Where(a => a.EmailAddress == forgotPassword.EmailAddress);

			if(userAccount.Count() == 0)
			{
				ModelState.AddModelError(string.Empty, "Email address not found");
			}

			if(ModelState.IsValid)
			{
				string newPassword = GetRandomHexPassword();

				this.account = userAccount.Single();
				this.account.Password = account.GetPasswordHash(newPassword);

				this.db.Entry(this.account).State = EntityState.Modified;
				this.db.SaveChanges();

				// Send email notification
				this.emailNotificationHelper.SendPasswordResetNotification(newPassword, forgotPassword, "ResetPassword");

				return RedirectToAction("ResetPasswordSuccess");
			}
			else
			{
				return View();
			}
		}

		public ActionResult ResetPasswordSuccess()
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

		private static string GetRandomHexPassword()
		{
			Random random = new Random();

			byte[] buffer = new byte[4];
			random.NextBytes(buffer);

			string result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());

			return result + random.Next(16).ToString("X");
		}

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