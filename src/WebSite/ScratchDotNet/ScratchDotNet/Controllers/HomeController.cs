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
            ViewBag.Message = "Scratch for .Net Micro Framework";

            return View();
        }

        public ActionResult Forum()
        {
            ViewBag.Message = "Scratch for .Net Forums";

            return View();
        }

        public ActionResult Details()
        {
            ViewBag.Message = "Technical Overview page";

            return View();
        }
    }
}