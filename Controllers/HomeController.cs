using MusicStore.Models;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MusicStore.Controllers
{
    public class HomeController : Controller
    {
        private MusicStoreDBModel db = new MusicStoreDBModel();
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);

            int userId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;

            var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserId == userId);
            if (cart != null)
            {
                Session["CartCount"] = db.CartItems.Where(c => c.CartId == cart.CartId).Sum(c => (int?)c.Quantity) ?? 0;
            }
            else
            {
                Session["CartCount"] = 0;
            }

            return View(products.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}