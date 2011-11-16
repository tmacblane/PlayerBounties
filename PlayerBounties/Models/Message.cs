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
			Favorite favorite = new Favorite();
			IQueryable<Favorite> favoritedCharacters;
			WatchedBounty watchedBounty = new WatchedBounty();			
			IQueryable<WatchedBounty> watchedBounties;

			Message adminMessage;
			Message clientMessage;
			Message hunterMessage;
			Message targetMessage;
			Message favoriteMessage;
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
			string shard = bounty.CharacterShard(bounty.PlacedOnId);

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
							Subject = string.Format("Placement submitted for approval on {0} - {1}", placedOn, shard),
							Description = string.Format("A bounty has been placed by {0} - {1} on {2} - {3} in the amount of {4} that is pending your approval", placedBy, shard, placedOn, shard, bounty.Amount),
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
						Subject = string.Format("Bounty placement against {0} - {1} is Pending Approval", placedOn, shard),
						Description = string.Format("We have recieved the bounty request against {0} - {1}. In order to finalize the bounty, you must deliver the bounty amount of {2} to one of our operatives. Once recieved, your bounty will be posted for every hunter in the galaxy to see in order to apprehend the target and bring them to whatever justice you so desired.", placedOn, shard, bounty.Amount),
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
						Subject = string.Format("Bounty placement against {0} - {1} has been Approved", placedOn, shard),
						Description = string.Format("The bounty against {0} - {1} has been approved.  Every hunter in the galaxy will now be on the hunt. Once the target has been killed you will be alerted with photographic proof of the targets demise.", placedOn, bounty.Amount),
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
						Subject = string.Format("Bounty has been placed on {0} - {1}", placedOn, shard),
						Description = string.Format("A bounty has been placed on your head by {0} - {1} in the amount of {2} for '{3}'.", placedBy, shard, bounty.Amount, bounty.Reason),
						IsRead = false,
						IsAdminMessage = false
					};

					db.Messages.Add(targetMessage);

					// Favorited Notifications
					favoritedCharacters = favorite.GetFavoritedCharacters(bounty.PlacedOnId);

					foreach(Favorite favoritedCharacterItem in favoritedCharacters)
					{
						favoriteMessage = new Message
						{
							Id = Guid.NewGuid(),
							UserId = favoritedCharacterItem.AccountId,
							DateCreated = dateCreated,
							Subject = string.Format("New bounty has been placed on {0} - {1}", placedOn, shard),
							Description = string.Format("One of your favorited characters, {0} - {1}, has commited a crime against {2} - {3} and a bounty has been placed on their head.", placedOn, shard, placedBy, shard),
							IsRead = false,
							IsAdminMessage = false
						};

						db.Messages.Add(favoriteMessage);
					}

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
							Subject = string.Format("A bounty completion has been posted on {0} - {1} by {2} - {3}", placedOn, shard, killedBy, shard),
							Description = string.Format("The bounty against {0} - {1} has been submitted for completion by {2} - {3}", placedOn, shard, killedBy, shard),
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
						Subject = string.Format("The bounty being collected on {0} - {1} is pending approval for completion", placedOn, shard),
						Description = string.Format("The bounty on {0} - {1} in the amount of @Model.Amount has been submitted for approval.  Once approved, the funds, minus any processing fees will be delivered to account.", placedOn, shard, bounty.Amount),
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
							Subject = string.Format("The bounty being collected on {0} - {1} is pending approval for completion", placedOn, shard),
							Description = string.Format("The bounty on {0} - {1} has been submitted for approval by another brave hunter. For the time being, please refrain from hunting the target. In the event that the collection request is denied, you will be alerted and thehunt can continue.", placedOn, shard),
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
						Subject = string.Format("Bounty has been completed on {0} - {1}", placedOn, shard),
						Description = string.Format("The bounty placed on {0} - {1} has been completed. {2} - {3} has been killed by {4} - {5} and has successfully delivered your message, visual proof of the targets demise can be seen on the bounty details page.", placedOn, shard, placedOn, shard, killedBy, shard),
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
						Subject = string.Format("Bounty has been completed on {0} - {1}", placedOn, shard),
						Description = string.Format("The completion for the bounty on {0} - {1} in the amount of {2} has been approved. One of our representatives will be delivering the bounty amount minus any processing fees to you.  We congratulate you on your success andon an excellent hunt!", placedOn, shard, bounty.Amount),
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
						Subject = string.Format("Bounty has been completed on {0} - {1}", placedOn, shard),
						Description = string.Format("The bounty placed on your head has been completed by {0} - {1}.", killedBy, shard),
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
							Subject = string.Format("Bounty has been completed on {0} - {1}", placedOn, shard),
							Description = string.Format("The bounty placed on {0} - {1} has been collected by {0} - {1}.  We ask that at this time you no longer continue the hunt. This bounty has been removed from all watch lists.", placedOn, shard, placedOn, shard, killedBy),
							IsRead = false,
							IsAdminMessage = false
						};
					
						db.Messages.Add(watcherMessage);
					}

					// Favorited Notifications
					favoritedCharacters = favorite.GetFavoritedCharacters(bounty.PlacedOnId);

					foreach(Favorite favoritedCharacterItem in favoritedCharacters)
					{
						favoriteMessage = new Message
						{
							Id = Guid.NewGuid(),
							UserId = favoritedCharacterItem.AccountId,
							DateCreated = dateCreated,
							Subject = string.Format("Bounty has been completed on {0} - {1}", placedOn, shard),
							Description = string.Format("The bounty on one of your favorited characters, {0} - {1} has been completed. {2} - {3} was the one who tracked and killed {4} - {5}.", placedOn, shard, killedBy, shard, placedOn, shard),
							IsRead = false,
							IsAdminMessage = false
						};

						db.Messages.Add(favoriteMessage);
					}

					db.SaveChanges();

					break;

                case "Placement Denied":

                    // Client Notification
                    clientMessage = new Message
                    {
                        Id = Guid.NewGuid(),
                        UserId = character.GetCharacterUserId(bounty.PlacedById),
                        DateCreated = dateCreated,
						Subject = string.Format("Bounty placed on {0} - {1} hase been denied", placedOn, shard),
						Description = string.Format("The bounty placed on {0} - {1} in the amount of {2} has been denied.", placedOn, shard, bounty.Amount),
                        IsRead = false,
                        IsAdminMessage = false
                    };

                    db.Messages.Add(clientMessage);

                    db.SaveChanges();
                    break;

                case "Completion Denied":
                    // Hunter Notification
                    hunterMessage = new Message
                    {
                        Id = Guid.NewGuid(),
                        UserId = character.GetCharacterUserId(bounty.KilledById.Value),
                        DateCreated = dateCreated,
                        Subject = string.Format("The bounty on {0} - {1} that was submitted for completion has been denied.", placedOn, shard),
						Description = string.Format("The bounty on {0} - {1} that was submitted for completion has been denied.", placedOn, shard),
                        IsRead = false,
                        IsAdminMessage = false
                    };

                    db.Messages.Add(hunterMessage);

                    // Watcher Notifications
                    watchedBounties = watchedBounty.GetWatchedBounties(bounty.Id);

                    foreach (WatchedBounty watchedBountyItem in watchedBounties)
                    {
                        watcherMessage = new Message
                        {
                            Id = Guid.NewGuid(),
                            UserId = watchedBountyItem.AccountId,
                            DateCreated = dateCreated,
							Subject = string.Format("The bounty on {0} - {1} that was submitted for completion has been denied.", placedOn, shard),
							Description = string.Format("The bounty on {0} - {1} that was submitted for completion has been denied. The target is still available to be hunted!", placedOn, shard),
                            IsRead = false,
                            IsAdminMessage = false
                        };

                        db.Messages.Add(watcherMessage);
                    }

                    db.SaveChanges();

                    break;
                    
                case "Bounty Cancelled":
                    // Admin Notification
                    foreach (Guid adminId in adminIds)
                    {
                        adminMessage = new Message
                        {
                            Id = Guid.NewGuid(),
                            UserId = adminId,
                            DateCreated = dateCreated,
							Subject = string.Format("The bounty placed on {0} - {1} has been cancelled by {1} - {1}", placedOn, shard, placedBy, shard),
							Description = string.Format("The bounty placed on {0} - {1} has been cancelled by {1} - {1}", placedOn, shard, placedBy, shard),
                            IsRead = false,
                            IsAdminMessage = true
                        };

                        db.Messages.Add(adminMessage);
                    }

                    // Watcher Notifications
                    watchedBounties = watchedBounty.GetWatchedBounties(bounty.Id);

                    foreach (WatchedBounty watchedBountyItem in watchedBounties)
                    {
                        watcherMessage = new Message
                        {
                            Id = Guid.NewGuid(),
                            UserId = watchedBountyItem.AccountId,
                            DateCreated = dateCreated,
							Subject = string.Format("The bounty on {0} - {1} has been cancelled by the client", placedOn, shard),
							Description = string.Format("The bounty on {0} - {1} has been cancelled by the client. This bounty has been removed from your watch list", placedOn, shard),
                            IsRead = false,
                            IsAdminMessage = false
                        };

                        db.Messages.Add(watcherMessage);
                    }

                    // Favorited Notifications
                    favoritedCharacters = favorite.GetFavoritedCharacters(bounty.PlacedOnId);

                    foreach (Favorite favoritedCharacterItem in favoritedCharacters)
                    {
                        favoriteMessage = new Message
                        {
                            Id = Guid.NewGuid(),
                            UserId = favoritedCharacterItem.AccountId,
                            DateCreated = dateCreated,
							Subject = string.Format("New bounty on {0} - {1} has been cancelled by the client", placedOn, shard),
							Description = string.Format("The bounty on {0} - {1} has been cancelled by the client", placedOn, shard),
                            IsRead = false,
                            IsAdminMessage = false
                        };

                        db.Messages.Add(favoriteMessage);
                    }

                    // Target Notification
                    targetMessage = new Message
                    {
                        Id = Guid.NewGuid(),
                        UserId = character.GetCharacterUserId(bounty.PlacedOnId),
                        DateCreated = dateCreated,
                        Subject = string.Format("Bounty on {0} - {1} has been cancelled by the client", placedOn, shard),
						Description = string.Format("The bounty placed on {0} - {1} has been cancelled by the client.", placedOn, shard),
                        IsRead = false,
                        IsAdminMessage = false
                    };

                    db.Messages.Add(targetMessage);

                    db.SaveChanges();

                    break;
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