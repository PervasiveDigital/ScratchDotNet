﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScratchDotNet.Controllers
{
    public class DocumentationController : Controller
    {
        // GET: Documentation
        public ActionResult Index()
        {
            return View();
        }
    }
}