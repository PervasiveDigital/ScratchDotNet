using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.WindowsAzure.Storage.Table;

using ScratchDotNet.Models;

namespace ScratchDotNet.Controllers
{
    public class HardwareController : Controller
    {
        private readonly AzureStorageContext _storage;

        public HardwareController(AzureStorageContext storage)
        {
            _storage = storage;
        }

        // GET: Hardware
        public ActionResult Index()
        {
            var manufacturers = _storage.ManufacturersTable;
            TableQuery<ManufacturerEntity> query = new TableQuery<ManufacturerEntity>();
            var manufList = manufacturers.ExecuteQuery(query);

            return View(manufList);
        }

        // GET: Hardware/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Hardware/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Hardware/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Hardware/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Hardware/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Hardware/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Hardware/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}