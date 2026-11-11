using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using MusicStore.Models;

namespace MusicStore.Controllers
{
    public class ProductsController : Controller
    {
        private MusicStoreDBModel db = new MusicStoreDBModel();

        [ChildActionOnly]
        public PartialViewResult CategoryMenu()
        {
            var list = db.Categories.ToList();
            return PartialView(list);
        }

        public ActionResult ProductsByCategory(int id)
        {
            var list = db.Products.Where(x => x.CategoryId == id).ToList();
            ViewBag.CategoryName = db.Categories
                             .Where(c => c.CategoryId == id)
                             .Select(c => c.CategoryName)
                             .FirstOrDefault();
            return View(list);
        }

        // GET: Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
