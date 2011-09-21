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
	public class HomeController : Controller
	{
		#region Fields

		private Bounty bounty = new Bounty();
		private Character character = new Character();
		private KillShotImage killShotImage = new KillShotImage();
		private PlayerBountyContext db = new PlayerBountyContext();

		#endregion

		#region Type specific methods

		public ActionResult Index()
		{
			if(Request.IsAuthenticated)
			{
				return View("Dashboard", null, this.character);
			}
			else
			{
				return View();
			}
		}

		public ActionResult About()
		{
			return View();
		}

		public ActionResult Rules()
		{
			return View();
		}

		public ActionResult Statistics()
		{
			return View();
		}

		[Authorize]
		[OutputCache(Duration = 300)]
		public ActionResult Dashboard()
		{
			return View(this.character);
		}

		public ActionResult KillShotImages(string imageType)
		{
			List<KillShotImage> killShotImages = new List<KillShotImage>();
			List<Guid> killImageIds = new List<Guid>();

			killImageIds = this.bounty.GetMostRecentlyCompletedBounties(25);

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