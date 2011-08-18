using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using PlayerBounties.Models;

namespace PlayerBounties.Controllers
{
	public class FileUploadController : Controller
	{
		#region Type specific methods

		// GET: /FileUpload/
		[Authorize]
		public ActionResult Index()
		{
			return View();
		}

		[Authorize]
		[HttpPost]
		public ActionResult Index(Bounty bounty, HttpPostedFile file)
		{
			// Verify that a file has been selected
			if(file != null && file.ContentLength > 0)
			{
				// extract the file name
				var fileName = Path.GetFileName(file.FileName);

				//store the file inside images folder
				var path = Path.Combine(Server.MapPath("../App_Data/images"), fileName);
				file.SaveAs(path);

				// set is bounty pending to true
				bounty.SetPendingCompletionToFalse(bounty);
			}

			return RedirectToAction("Dashboard", "Home");
		}

		#endregion
	}
}