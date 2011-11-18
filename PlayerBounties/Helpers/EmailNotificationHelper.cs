using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using PlayerBounties.Models;
using PlayerBounties.ViewModels;
using Postal;

namespace PlayerBounties.Helpers
{
	public class EmailNotificationHelper
	{
		#region Fields

		private Character character = new Character();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		public void SendPasswordResetNotification(string password, ForgotPasswordModel forgotPassword, string emailView)
		{
			dynamic email = new Email(emailView);

			email.UserEmailAddress = forgotPassword.EmailAddress;
			email.Password = password;

			try
			{
				email.Send();
			}
			catch
			{
				// Need to log when it fails, the email type and information
			}
		}   

        public void SendHelpAndSupportEmail(HelpViewModel helpViewModel)
        {
            dynamic email = new Email("HelpAndSupport");

            email.EmailAddress = helpViewModel.EmailAddress;
            email.Subject = helpViewModel.SubjectLine;
			email.Message = helpViewModel.Message;

            try
            {
                email.Send();
            }
            catch
            {
                // Need to log when it fails, the email type and information
            }
        }

		public void SendNotificationEmail(Bounty bounty, string emailView, Guid accountId)
		{
			if(accountId != Guid.Empty)
			{
				if(this.db.Accounts.Find(accountId).EmailNotification == true)
				{
					dynamic email = new Email(emailView);

					string placedOn = bounty.CharacterName(bounty.PlacedOnId);
					string placedBy = bounty.CharacterName(bounty.PlacedById);
					string killedBy = string.Empty;
					string shard = bounty.CharacterShard(bounty.PlacedOnId);

					email.UserEmailAddress = this.db.Accounts.Find(accountId).EmailAddress;
					email.Amount = bounty.Amount;
					email.Reason = bounty.Reason;
					email.Message = bounty.Message;
					email.ClientName = placedBy;
					email.TargetName = placedOn;
					email.Shard = shard;
					email.BountyId = bounty.Id;

					if(bounty.KilledById != null)
					{
						killedBy = bounty.CharacterName(bounty.KilledById.Value);
					}

					try
					{
						email.Send();
					}
					catch
					{
						// Need to log when it fails, the email type and information
					}
				}
			}
		}

		public void SendBountyNotificationEmail(Bounty bounty, string emailType)
		{
			Account account = new Account();
			Favorite favorite = new Favorite();
			IQueryable<Favorite> favoritedCharacters;
			WatchedBounty watchedBounty = new WatchedBounty();
			IQueryable<WatchedBounty> watchedBounties;

			List<Guid> adminIds = new List<Guid>();
			IQueryable<Account> admins = account.GetAdminUserIds();

			foreach(Account admin in admins)
			{
				adminIds.Add(admin.Id);
			}			

			switch(emailType)
			{
				case "Pending Placement":
					// Admin Notification
					foreach(Guid adminId in adminIds)
					{
						this.SendNotificationEmail(bounty, "PendingBountyPlaced-AdminAlert", adminId);
					}

					// Client Notification
					this.SendNotificationEmail(bounty, "PendingBountyPlaced-ClientAlert", this.character.GetCharacterUserId(bounty.PlacedById));

					break;

				case "Placement Approved":
					// Client Notification
					this.SendNotificationEmail(bounty, "BountyPlacedApproved-ClientAlert", this.character.GetCharacterUserId(bounty.PlacedById));

					// Target Notification
					this.SendNotificationEmail(bounty, "BountyPlacedApproved-TargetAlert", this.character.GetCharacterUserId(bounty.PlacedOnId));

					// Favorited Notifications
					favoritedCharacters = favorite.GetFavoritedCharacters(bounty.PlacedOnId);

					foreach(Favorite favoritedCharacterItem in favoritedCharacters)
					{
						this.SendNotificationEmail(bounty, "BountyPlacedApproved-FavoritedAlert", favoritedCharacterItem.AccountId);
					}

					break;

				case "Pending Completion":
					// Admin Notification
					foreach(Guid adminId in adminIds)
					{
						this.SendNotificationEmail(bounty, "PendingBountyCompletion-AdminAlert", adminId);
					}

					// Hunter Notification
					this.SendNotificationEmail(bounty, "PendingBountyCompletion-HunterAlert", this.character.GetCharacterUserId(bounty.KilledById.Value));

					// Watcher Notifications
					watchedBounties = watchedBounty.GetWatchedBounties(bounty.Id);

					foreach(WatchedBounty watchedBountyItem in watchedBounties)
					{
						this.SendNotificationEmail(bounty, "PendingBountyCompletion-WatchedAccountAlert", watchedBountyItem.AccountId);
					}
					
					break;

				case "Completion Approved":
					// Client Notification
					this.SendNotificationEmail(bounty, "BountyCompletionApproved-ClientAlert", this.character.GetCharacterUserId(bounty.PlacedById));

					// Hunter Notification
					this.SendNotificationEmail(bounty, "BountyCompletionApproved-HunterAlert", this.character.GetCharacterUserId(bounty.KilledById.Value));

					// Target Notification
					this.SendNotificationEmail(bounty, "BountyCompletionApproved-TargetAlert", this.character.GetCharacterUserId(bounty.PlacedOnId));

					// Watcher Notifications
					watchedBounties = watchedBounty.GetWatchedBounties(bounty.Id);

					foreach(WatchedBounty watchedBountyItem in watchedBounties)
					{
						this.SendNotificationEmail(bounty, "BountyCompletionApproved-WatchedAccountAlert", watchedBountyItem.AccountId);
					}

					// Favorited Notifications
					favoritedCharacters = favorite.GetFavoritedCharacters(bounty.PlacedOnId);

					foreach(Favorite favoritedCharacterItem in favoritedCharacters)
					{
						this.SendNotificationEmail(bounty, "BountyCompletionApproved-FavoritedAlert", favoritedCharacterItem.AccountId);
					}

					break;

				case "Placement Denied":
					// Client Notification
					this.SendNotificationEmail(bounty, "BountyPlacementDenied-ClientAlert", this.character.GetCharacterUserId(bounty.PlacedById));

					break;


				case "Completion Denied":
					// Hunter Notification
					this.SendNotificationEmail(bounty, "BountyCompletionDenied-HunterAlert", this.character.GetCharacterUserId(bounty.KilledById.Value));

					// Watcher Notifications
					watchedBounties = watchedBounty.GetWatchedBounties(bounty.Id);

					foreach(WatchedBounty watchedBountyItem in watchedBounties)
					{
						this.SendNotificationEmail(bounty, "BountyCompletionDenied-WatchedAccountAlert", watchedBountyItem.AccountId);
					}

					break;


				case "Bounty Cancelled":
					// Admin Notification
					foreach(Guid adminId in adminIds)
					{
						this.SendNotificationEmail(bounty, "BountyCancelled-AdminAlert", adminId);
					}

					// Target Notification
					this.SendNotificationEmail(bounty, "BountyCancelled-TargetAlert", this.character.GetCharacterUserId(bounty.PlacedOnId));

					// Watcher Notifications
					watchedBounties = watchedBounty.GetWatchedBounties(bounty.Id);

					foreach(WatchedBounty watchedBountyItem in watchedBounties)
					{
						this.SendNotificationEmail(bounty, "BountyCancelled-WatchedAccountAlert", watchedBountyItem.AccountId);
					}

					// Favorited Notifications
					favoritedCharacters = favorite.GetFavoritedCharacters(bounty.PlacedOnId);

					foreach(Favorite favoritedCharacterItem in favoritedCharacters)
					{
						this.SendNotificationEmail(bounty, "BountyCancelled-FavoritedAlert", favoritedCharacterItem.AccountId);
					}

					break;
			}
		}
	}
}