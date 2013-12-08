using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cost.Models;
using WebMatrix.WebData;

namespace Cost.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            Cost3Entities db = new Cost3Entities();
            ViewBag.mrpcn = "yes";
            return View(db.Factory.ToList());
            
        }

    }
}
