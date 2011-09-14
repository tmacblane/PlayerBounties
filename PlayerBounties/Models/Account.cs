using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace PlayerBounties.Models
{
	public class Account
	{
		#region Fields

		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific properties

		[Key]
		public Guid Id
		{
			get;
			set;
		}

		public string EmailAddress
		{
			get;
			set;
		}

		public string Password
		{
			get;
			set;
		}

		public bool IsAdmin
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		/// <summary>
		/// Adds a user entry to the database based on the information passed from the SignUpModel
		/// </summary>
		/// <param name="signUpModel">Contains all of the account information</param>
		public void AddUserToDatabase(SignUpModel signUpModel)
		{
			this.Id = Guid.NewGuid();
			this.EmailAddress = signUpModel.Email;
			this.Password = signUpModel.Password;

			this.db.Accounts.Add(this);
			this.db.SaveChanges();
		}

		/// <summary>
		/// Checks if the specified email address exists in the database
		/// </summary>
		/// <param name="emailAddress">account email address</param>
		/// <returns>login exists bool</returns>
		public bool IsUserLoginIDExist(string emailAddress)
		{
			bool loginIdExists = false;

			if(this.db.Accounts.Count() != 0)
			{
				if(this.db.Accounts.Where(row => row.EmailAddress == emailAddress).Count() != 0)
				{
					loginIdExists = true;
				}
			}

			return loginIdExists;
		}

		public Guid GetUserId(string emailAddress)
		{
			return this.db.Accounts.Where(row => row.EmailAddress == emailAddress).Single().Id;
		}

		/// <summary>
		/// Gets the password for the specified user email address
		/// </summary>
		/// <param name="emailAddress">account email address</param>
		/// <returns>account password</returns>
		public string GetUserPasswordByEmailAddress(string emailAddress)
		{
			if(this.IsUserLoginIDExist(emailAddress).Equals(true))
			{
				return this.db.Accounts.Where(row => row.EmailAddress == emailAddress).Single().Password;
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Checks the login credentials being submitted
		/// </summary>
		/// <param name="emailAddress">account email address</param>
		/// <param name="password">account password</param>
		/// <returns>user valid bool</returns>
		public bool ValidateUser(string emailAddress, string password)
		{
			bool isUserValid = false;

			if(this.db.Accounts.Count() != 0)
			{
				var result = this.GetUserPasswordByEmailAddress(emailAddress);

				if(result == password)
				{
					return true;
				}
			}

			return isUserValid;
		}

		public Guid GetLoggedInUserId()
		{
			return this.db.Accounts.Where(row => row.EmailAddress == System.Web.HttpContext.Current.User.Identity.Name).Single().Id;
		}

		public bool IsUserAdmin(Guid userId)
		{
			bool isAdmin = false;

			if(this.db.Accounts.Find(userId).IsAdmin.Equals(true))
			{
				isAdmin = true;
			}

			return isAdmin;
		}

		#endregion
	}	

	public class ChangePasswordModel
	{
		#region Type specific properties

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current password")]
		public string OldPassword
		{
			get;
			set;
		}

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword
		{
			get;
			set;
		}

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword
		{
			get;
			set;
		}

		#endregion
	}

	public class LoginModel
	{
		#region Type specific properties

		[Required(ErrorMessage = "Email Address is required.")]
		[Display(Name = "Email Address")]
		public string Email
		{
			get;
			set;
		}

		[Required(ErrorMessage = "Password is required.")]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password
		{
			get;
			set;
		}

		[Display(Name = "Remember me?")]
		public bool RememberMe
		{
			get;
			set;
		}

		#endregion		
	}

	public class SignUpModel
	{
		#region Type specific properties

		[Required]
		[DataType(DataType.EmailAddress)]
		[Display(Name = "Email address")]
		public string Email
		{
			get;
			set;
		}

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password
		{
			get;
			set;
		}

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword
		{
			get;
			set;
		}

		#endregion
	}
}
