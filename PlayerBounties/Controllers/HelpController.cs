using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PlayerBounties.Controllers
{
    public class HelpController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
