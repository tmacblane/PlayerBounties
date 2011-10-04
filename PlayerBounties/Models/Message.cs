using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlayerBounties.Models
{
	public class Message
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

		public Guid UserId
		{
			get;
			set;
		}

		public DateTime DateCreated
		{
			get;
			set;
		}

		public string Subject
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public bool IsRead
		{
			get;
			set;
		}

		public bool IsAdminMessage
		{
			get;
			set;
		}

		#endregion

		#region Type specific methods

		public void AddNotificationMessage(Guid userId, string subject, string message)
		{
			this.Id = Guid.NewGuid();
			this.UserId = userId;
			this.DateCreated = DateTime.Now;
			this.Subject = subject;
			this.Description = message;
			this.IsRead = false;
			this.IsAdminMessage = false;

			db.Messages.Add(this);
			db.SaveChanges();
		}

		public void AddBountyNotificationMessage(Bounty bounty, string messageType)
		{
			Account account = new Account();
			Character character = new Character();
			WatchedBounty watchedBounty = new WatchedBounty();			
			IQueryable<WatchedBounty> watchedBounties;

			Message adminMessage;
			Message clientMessage;
			Message hunterMessage;
			Message targetMessage;
			Message watcherMessage;

			List<Guid> adminIds = new List<Guid>();
			IQueryable<Account> admins = account.GetAdminUserIds();

			foreach(Account admin in admins)
			{
				adminIds.Add(admin.Id);
			}

			DateTime dateCreated = DateTime.Now;

			string placedOn = bounty.CharacterName(bounty.PlacedOnId);
			string placedBy = bounty.CharacterName(bounty.PlacedById);
			string killedBy = string.Empty;

			if(bounty.KilledById != null)
			{
				killedBy = bounty.CharacterName(bounty.KilledById.Value);
			}

			switch(messageType)
			{
				case "Pending Placement":					
					// Admin Notification
					foreach(Guid adminId in adminIds)
					{
						adminMessage = new Message
						{
							Id = Guid.NewGuid(),
							UserId = adminId,
							DateCreated = dateCreated,
							Subject = string.Format("Placement submitted for approval on {0}", placedOn),
							Description = string.Format("A bounty has been placed by {0} on {1} in the amount of {2} that is pending your approval", placedBy, placedOn, bounty.Amount),
							IsRead = false,
							IsAdminMessage = true
						};

						db.Messages.Add(adminMessage);
					}

					// Client Notification
					clientMessage = new Message
					{
						Id = Guid.NewGuid(),
						UserId = character.GetCharacterUserId(bounty.PlacedById),
						DateCreated = dateCreated,
						Subject = string.Format("Placement submitted for approval on {0}", placedOn),
						Description = string.Format("The bounty placed on {0} in the amount of {1} is pending approval.", placedOn, bounty.Amount),
						IsRead = false,
						IsAdminMessage = false
					};

					db.Messages.Add(clientMessage);

					db.SaveChanges();

					break;

				case "Placement Approved":
					// Client Notification
					clientMessage = new Message
					{
						Id = Guid.NewGuid(),
						UserId = character.GetCharacterUserId(bounty.PlacedById),
						DateCreated = dateCreated,
						Subject = string.Format("Bounty submitted on {0} approved", placedOn),
						Description = string.Format("The bounty placed on {0} in the amount of {1} has been approved.", placedOn, bounty.Amount),
						IsRead = false,
						IsAdminMessage = false
					};

					db.Messages.Add(clientMessage);

					// Target Notification
					targetMessage = new Message
					{
						Id = Guid.NewGuid(),
						UserId = character.GetCharacterUserId(bounty.PlacedOnId),
						DateCreated = dateCreated,
						Subject = string.Format("Bounty has been placed on {0}", placedOn),
						Description = string.Format("A bounty has been placed on your head by {0} in the amount of {1} for '{2}'.", placedBy, bounty.Amount, bounty.Reason),
						IsRead = false,
						IsAdminMessage = false
					};

					db.Messages.Add(targetMessage);

					db.SaveChanges();

					break;

				case "Pending Completion":
					// Admin Notification
					foreach(Guid adminId in adminIds)
					{
						adminMessage = new Message
						{
							Id = Guid.NewGuid(),
							UserId = adminId,
							DateCreated = dateCreated,
							Subject = string.Format("Completion submitted for approval on {0}", placedOn),
							Description = string.Format("The bounty that has been placed on {0} has been completed by {1} is pending your approval", placedOn, killedBy),
							IsRead = false,
							IsAdminMessage = true
						};

						db.Messages.Add(adminMessage);
					}

					// Hunter Notification
					hunterMessage = new Message
					{
						Id = Guid.NewGuid(),
						UserId = character.GetCharacterUserId(bounty.KilledById.Value),
						DateCreated = dateCreated,
						Subject = string.Format("Completion submitted for approval on {0}", placedOn),
						Description = string.Format("The bounty placed on {0} in the amount of {1} has been submitted for approval.", placedOn, bounty.Amount),
						IsRead = false,
						IsAdminMessage = false
					};
					
					db.Messages.Add(hunterMessage);

					// Watcher Notifications
					watchedBounties = watchedBounty.GetWatchedBounties(bounty.Id);

					foreach(WatchedBounty watchedBountyItem in watchedBounties)
					{
						watcherMessage = new Message
						{
							Id = Guid.NewGuid(),
							UserId = watchedBountyItem.AccountId,
							DateCreated = dateCreated,
							Subject = string.Format("The completion of the bounty on {0} is pending approval", placedOn),
							Description = string.Format("The bounty on {0} has been submitted for approval.  Please refrain from hunting the target", placedOn, placedOn),
							IsRead = false,
							IsAdminMessage = false
						};
					
						db.Messages.Add(watcherMessage);
					}

					db.SaveChanges();

					break;

				case "Completion Approved":
					// Client Notification
					clientMessage = new Message
					{
						Id = Guid.NewGuid(),
						UserId = character.GetCharacterUserId(bounty.PlacedById),
						DateCreated = dateCreated,
						Subject = string.Format("Bounty completion on {0} approved", placedOn),
						Description = string.Format("The bounty completed on {0} in the amount of {1} has been approved.", placedOn, bounty.Amount),
						IsRead = false,
						IsAdminMessage = false
					};

					db.Messages.Add(clientMessage);

					// Hunter Notification
					hunterMessage = new Message
					{
						Id = Guid.NewGuid(),
						UserId = character.GetCharacterUserId(bounty.KilledById.Value),
						DateCreated = dateCreated,
						Subject = string.Format("Bounty completion on {0} approved", placedOn),
						Description = string.Format("The bounty placed on {0} in the amount of {1} has been approved.", placedOn, bounty.Amount),
						IsRead = false,
						IsAdminMessage = false
					};

					db.Messages.Add(hunterMessage);
					
					// Target Notification
					targetMessage = new Message
					{
						Id = Guid.NewGuid(),
						UserId = character.GetCharacterUserId(bounty.PlacedOnId),
						DateCreated = dateCreated,
						Subject = string.Format("Bounty placed on {0} has been completed", placedOn),
						Description = string.Format("The bounty placed on your head has been completed."),
						IsRead = false,
						IsAdminMessage = false
					};

					db.Messages.Add(targetMessage);

					// Watcher Notifications
					watchedBounties = watchedBounty.GetWatchedBounties(bounty.Id);

					foreach(WatchedBounty watchedBountyItem in watchedBounties)
					{
						watcherMessage = new Message
						{
							Id = Guid.NewGuid(),
							UserId = watchedBountyItem.AccountId,
							DateCreated = dateCreated,
							Subject = string.Format("The bounty on {0} has been completed", placedOn),
							Description = string.Format("The bounty on {0} has been completed. {1} was successfully killed by {2}. This bounty has been removed from your watch list", placedOn, placedOn, killedBy),
							IsRead = false,
							IsAdminMessage = false
						};
					
						db.Messages.Add(watcherMessage);
					}

					db.SaveChanges();

					break;

					// To Do - Completion Denied
					// To Do - Placement Denied
					// To Do - Bounty Cancelled
			}
		}

		public IQueryable<Message> GetAllMessages(Guid userId)
		{
			return this.db.Messages.Where(m => m.UserId == userId);
		}

		public IQueryable<Message> GetUnreadMessages(Guid userId)
		{
			return this.db.Messages.Where(m => m.UserId == userId).Where(m => m.IsRead == false);
		}

		public IQueryable<Message> GetReadMessages(Guid userId)
		{
			return this.db.Messages.Where(m => m.UserId == userId).Where(m => m.IsRead == true);
		}

		public IQueryable<Message> GetUnreadAdminMessages()
		{
			return this.db.Messages.Where(m => m.IsAdminMessage == true);
		}

		#endregion
	}
}