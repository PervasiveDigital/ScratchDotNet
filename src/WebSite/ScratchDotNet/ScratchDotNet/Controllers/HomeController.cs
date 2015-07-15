using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScratchDotNet.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page";

            return View();
        }

        public ActionResult Details()
        {
            ViewBag.Message = "Technical Overview page";

            return View();
        }

        public ActionResult Hardware()
        {
            ViewBag.Message = "Compatible Hardware";

            return View();
        }
        public ActionResult GettingStarted()
        {
            ViewBag.Message = "Getting Started";

            return View();
        }

        public ActionResult Documentation()
        {
            ViewBag.Message = "Documentation Home Page";

            return View();
        }
    }
}