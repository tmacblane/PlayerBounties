using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

using PlayerBounties.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace PlayerBounties.Models
{
	public class Account
	{
		#region Fields

		private Message message = new Message();
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

		[Display(Name = "Email Notifications")]
		public bool EmailNotification
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
			this.Password = HashPassword(signUpModel.Password);
			this.IsAdmin = false;
			this.EmailNotification = false;

			this.db.Accounts.Add(this);
			this.db.SaveChanges();
		}

		public string GetPasswordHash(string password)
		{
			return HashPassword(password);
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

				if(ValidatePassword(password, result))
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

		public IQueryable<Message> GetUnreadMessages(Guid userId)
		{
			return this.message.GetUnreadMessages(userId);
		}

		public IQueryable<Message> GetUnreadAdminMessages()
		{
			return this.message.GetUnreadAdminMessages();
		}

		public IQueryable<Account> GetAdminUserIds()
		{
			return this.db.Accounts.Where(a => a.IsAdmin == true);
		}
		
		/// <summary>
		/// Hashes a password
		/// </summary>
		/// <param name="password">The password to hash</param>
		/// <returns>The hashed password as a 128 character hex string</returns>
		public static string HashPassword(string password)
		{
			string salt = GetRandomSalt();
			string hash = Sha256Hex(salt + password);
			return salt + hash;
		}

		/// <summary>
		/// Validates a password
		/// </summary>
		/// <param name="password">The password to test</param>
		/// <param name="correctHash">The hash of the correct password</param>
		/// <returns>True if password is the correct password, false otherwise</returns>
		public static bool ValidatePassword(string password, string correctHash)
		{
			if(correctHash.Length < 128)
				throw new ArgumentException("correctHash must be 128 hex characters!");
			string salt = correctHash.Substring(0, 64);
			string validHash = correctHash.Substring(64, 64);
			string passHash = Sha256Hex(salt + password);
			return string.Compare(validHash, passHash) == 0;
		}

		//returns the SHA256 hash of a string, formatted in hex
		private static string Sha256Hex(string toHash)
		{
			SHA256Managed hash = new SHA256Managed();
			byte[] utf8 = UTF8Encoding.UTF8.GetBytes(toHash);
			return BytesToHex(hash.ComputeHash(utf8));
		}

		//Returns a random 64 character hex string (256 bits)
		private static string GetRandomSalt()
		{
			RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
			byte[] salt = new byte[32]; //256 bits
			random.GetBytes(salt);
			return BytesToHex(salt);
		}

		//Converts a byte array to a hex string
		private static string BytesToHex(byte[] toConvert)
		{
			StringBuilder s = new StringBuilder(toConvert.Length * 2);
			foreach(byte b in toConvert)
			{
				s.Append(b.ToString("x2"));
			}
			return s.ToString();
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

	public class ForgotPasswordModel
	{
		#region Type specific properties

		[Required]
		[Display(Name = "Email Address")]
		public string EmailAddress
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
		[StringLength(100)]
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
		[StringLength(100)]
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
