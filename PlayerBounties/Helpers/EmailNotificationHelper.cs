using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using PlayerBounties.Models;
using Postal;

namespace PlayerBounties.Helpers
{
	public class EmailNotificationHelper
	{
		#region Fields

		private Character character = new Character();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		public void SendBountyNotificationEmail(string emailType, Bounty bounty, Guid accountId)
		{
			Bounty bountyModel = bounty;
			dynamic email = new Email(emailType);

			email.Amount = bountyModel.Amount;
			email.Reason = bountyModel.Reason;
			email.Message = bountyModel.Message;

			if(accountId != Guid.Empty)
			{
				email.UserEmailAddress = this.db.Accounts.Find(accountId).EmailAddress;
			}

			email.ClientName = this.character.CharacterName(bountyModel.PlacedById);
			email.TargetName = this.character.CharacterName(bountyModel.PlacedOnId);

			if(bountyModel.KilledById != null)
			{
				email.HunterName = this.character.CharacterName(bountyModel.KilledById.Value);
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