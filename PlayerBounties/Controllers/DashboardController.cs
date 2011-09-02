using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;

namespace PlayerBounties.Controllers
{
	[Authorize]
	public class DashboardController : Controller
	{
		#region Fields

		private Account account = new Account();
		private Bounty bounty = new Bounty();
		private KillShotImage killShotImage = new KillShotImage();
		private Character character = new Character();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		// GET: /Dashboard/
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Characters()
		{
			var characters = this.character.GetAllCharactersForAnAccount(this.account.GetLoggedInUserId());
			return PartialView("_Characters", characters.ToList());
		}

		public ActionResult KillShotImages(string imageType)
		{
			List<KillShotImage> killShotImages = new List<KillShotImage>();
			List<Guid> killImageIds = new List<Guid>();

			switch(imageType)
			{
                case "bountiesPlaced":
					killImageIds = this.bounty.GetAllKillShotImageIdsByAccount(this.account.GetLoggedInUserId(), imageType);
					break;

				case "targetsKilled":
					killImageIds = this.bounty.GetAllKillShotImageIdsByAccount(this.account.GetLoggedInUserId(), imageType);
					break;

                case "bountiesPlacedOn":
					killImageIds = this.bounty.GetAllKillShotImageIdsByAccount(this.account.GetLoggedInUserId(), imageType);
					break;
			}

			foreach(Guid killImageId in killImageIds)
			{
				killShotImages.Add(new KillShotImage
				{
					Id = this.db.KillShotImages.Find(killImageId).Id,
					FileName = this.db.KillShotImages.Find(killImageId).FileName,
					FilePath = this.db.KillShotImages.Find(killImageId).FilePath,
					ThumbnailFileName = this.db.KillShotImages.Find(killImageId).ThumbnailFileName,
					ThumbnailFilePath = this.db.KillShotImages.Find(killImageId).ThumbnailFilePath
				});
			}

			return PartialView("_KillShotImageSlider", killShotImages);
		}

		#endregion
	}
}